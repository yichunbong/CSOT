using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.Lcd.Scheduling.DataModel;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Persists;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;
using System.Text;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
	[FeatureBind()]
	public partial class DispatcherControl
	{
		/// <summary>
		/// </summary>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public Type GET_LOT_BATCH_TYPE0(ref bool handled, Type prevReturnValue)
		{
			return typeof(FabLotBatch);
		}

		/// <summary>
		/// </summary>
		/// <param name="dc"/>
		/// <param name="aeqp"/>
		/// <param name="wips"/>
		/// <param name="handled"/>
		public void UPDATE_CONTEXT0(IDispatchContext dc, AoEquipment aeqp, IList<IHandlingBatch> wips, ref bool handled)
		{
			//////20161122075408
			//if (aeqp.EqpID == "8APPH02" && aeqp.NowDT == new DateTime(2016, 12, 12, 8, 42, 14))
			//    Console.WriteLine("T");

			List<JobFilterInfo> joblist = dc.Get<List<JobFilterInfo>>(Constants.JobGroup, null);

			if (aeqp.Preset == null)
				return;

			double maxRequireEqp;
			if (WeightHelper.TryGetMaxRequiredEqp(aeqp, joblist, out maxRequireEqp))
				dc.Set(Constants.WF_MAX_REQUIRED_EQP_COUNT, maxRequireEqp);

			LayerStats sts = WeightHelper.CalcLayerBalance(aeqp);
			dc.Set(Constants.WF_LAYER_BALANCE_PRIORITY, sts);

			//투입을 위한 waiting wip infomation(unpack 설비 )
			Dictionary<string, WaitingWipInfo> waitingInfos = WeightHelper.SetWatingWipInfo(aeqp, joblist);
			dc.Set(Constants.WF_WAITING_WIP_INFO, waitingInfos);

			dc.Set(Constants.WF_MINMAX_VALUE_INFOS, WeightHelper.SetMinMaxVaule_WF(aeqp, joblist));

			if (wips.Count > 0)
				dc.Set(Constants.WF_LOT_PRIORITY, wips.Max(x => x.ToFabLot().Priority));

			DispatchLogHelper.InitLotLogDetail(wips);
		}

		/// <summary>
		/// </summary>
		/// <param name="db"/>
		/// <param name="aeqp"/>
		/// <param name="wips"/>
		/// <param name="ctx"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public IHandlingBatch[] DO_SELECT0(Mozart.SeePlan.Simulation.DispatcherBase db, Mozart.SeePlan.Simulation.AoEquipment aeqp, IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IDispatchContext ctx, ref bool handled, Mozart.SeePlan.Simulation.IHandlingBatch[] prevReturnValue)
		{
			WeightPreset preset = aeqp.Target.Preset;
			FabAoEquipment eqp = aeqp as FabAoEquipment;

			//InPort Wip 처리
			if (eqp.HasInPortWip)
			{
				//여러 Lot을 넘길 경우 첫번째 투입, 나머지는 설비의 Buffer에 넣음.
				IHandlingBatch[] list = eqp.InitInPortWips.ToArray();

				eqp.InitInPortWips.Clear();

				return list;
			}

			IHandlingBatch[] selected = null;

			if (wips.Count > 0)
			{
				List<IHandlingBatch> newlist = new List<IHandlingBatch>(wips);
				var control = DispatchControl.Instance;

				//if (eqp.EqpID == "FHRPH100" && eqp.NowDT >= LcdHelper.StringToDateTime("20200113 073000"))
				//	Console.WriteLine("B");

				var dummy = WeightHelper.NeedAllowRunDown_Dummy(eqp, wips);
				if (dummy != null)
				{
					newlist.Add(dummy);
					dummy.DispatchFilterInfo = eqp.LastPlanFilterInfo;
				}

				var dummyList = FilterMaster.WaitForPrevStepWip_Dummy(ctx, eqp);
				if (dummyList != null && dummyList.Count > 0)
				{
					newlist.AddRange(dummyList);
				}

				var lotList = control.Evaluate(db, newlist, ctx);
				selected = control.Select(db, eqp, lotList);

				if (control.IsWriteDispatchLog(eqp))
				{
					DispatchLogHelper.AddDispatchInfo(eqp, lotList, selected, preset);
					//eqp.EqpDispatchInfo.AddDispatchInfo(lotList, selected, preset);
				}
			}

			if (selected == null)
			{
				eqp.CheckAvailableSubEqps();
			}

			return selected;
		}

		/// <summary>
		/// </summary>
		/// <param name="db"/>
		/// <param name="wips"/>
		/// <param name="ctx"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public IList<IHandlingBatch> EVALUATE0(Mozart.SeePlan.Simulation.DispatcherBase db, IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IDispatchContext ctx, ref bool handled, IList<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
		{
			if (db is FifoDispatcher)
				return wips;

			if (db.Comparer == null)
				return wips;

			var stepDic = new Dictionary<string, WeightInfo>();

			var list = new List<IHandlingBatch>(wips.Count);
			
			var eqp = db.Eqp.ToFabAoEquipment();
			if (eqp.Dispatcher is WeightSumDispatcher)
				db.Comparer = new CompareHelper.WeightSumComparer(eqp);

			foreach (IHandlingBatch hb in wips)
			{
				if (hb.Sample == null)
					continue;

				var lot = hb.Sample;

				lot.WeightInfo = new WeightInfo();
				WeightInfo lotInfo = lot.WeightInfo;
				WeightInfo stepInfo;
				if (!stepDic.TryGetValue(lot.CurrentStep.StepID, out stepInfo))
					stepDic.Add(lot.CurrentStep.StepID, stepInfo = new WeightInfo());


				bool hasMinusValue = false;

				if (db.FactorList != null)
				{
					//Logger.Info(string.Format("<< {0} EVALUATE {1} >>");
					foreach (var info in db.FactorList)
					{
						WeightValue wval = null;
						FabWeightFactor factor = info.Factor as FabWeightFactor;

						if (factor.Type == FactorType.FIXED)
						{
							wval = lotInfo.GetValueData(factor);
							if (wval.IsMinValue)
								wval = db.WeightEval.GetWeight(factor, info.Method, lot, ctx);
						}
						else if (factor.Type == FactorType.STEPTYPE)
						{
							wval = stepInfo.GetValueData(factor);
							if (wval.IsMinValue)
								wval = db.WeightEval.GetWeight(factor, info.Method, lot, ctx);

							stepInfo.SetValueData(factor, wval);
						}
						else //LOTTYPE
						{
							wval = db.WeightEval.GetWeight(factor, info.Method, lot, ctx);

							//Factor가 하나라도 음수를 가질 경우 해당 재공은 제외 (필터를 허용할 경우)
							if (wval.Value < 0)
							{
								wval = new WeightValue(factor, 0, wval.Description);

								if (factor.IsAllowFilter)
								{
									lotInfo.SetValueData(factor, wval);
									hasMinusValue = true;

									db.Eqp.EqpDispatchInfo.AddFilteredWipInfo(hb, string.Format("Minus_Value：{0}, Desc：{1}", factor.Name, wval.Description));
								}

							}
						}

						lotInfo.SetValueData(factor, wval);
					}
				}

				if (hasMinusValue == false)
					list.AddSort(hb, db.Comparer);
			}

			return list;
		}


		/// <summary>
		/// </summary>
		/// <param name="db"/>
		/// <param name="aeqp"/>
		/// <param name="wips"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public IHandlingBatch[] SELECT0(Mozart.SeePlan.Simulation.DispatcherBase db, Mozart.SeePlan.Simulation.AoEquipment aeqp, IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, ref bool handled, Mozart.SeePlan.Simulation.IHandlingBatch[] prevReturnValue)
		{
			var eqp = aeqp.ToFabAoEquipment();

			if (wips == null || wips.Count == 0)
				return null;

			var lot = wips[0].ToFabLot();

			//if (eqp.EqpID == "THCVD300" && eqp.NowDT >= LcdHelper.StringToDateTime("20191107 075929"))
			//	Console.WriteLine("B");

			if (lot.IsDummy)
			{
				eqp.IsDummyWait = true;
				return null;
			}

			if (eqp.IsParallelChamber)
			{
				if (ChamberMaster.IsLoadable_ParallelChamber(eqp, lot) == false)
					return null;
			}

			eqp.IsDummyWait = false;

			return new IHandlingBatch[] { lot };
		}

		public IHandlingBatch[] SELECT1(DispatcherBase db, AoEquipment aeqp, IList<IHandlingBatch> wips, ref bool handled, IHandlingBatch[] prevReturnValue)
		{
			if (InputMart.Instance.GlobalParameters.ApplyFlexialePMSchedule == false)
				return prevReturnValue;

			if (prevReturnValue == null)
				return prevReturnValue;

			var list = aeqp.DownManager.GetStartScheduleItems(Time.MaxValue);
			if (list == null || list.Count == 0)
				return prevReturnValue;

			FabLot lot = prevReturnValue[0] as FabLot;

			var eqp = aeqp.ToFabAoEquipment();
			var proc = eqp.Processes[0];
			var outTime = proc.GetUnloadingTime(lot);

			bool isLast = eqp.IsLastPlan(lot);
			bool isNeedSetup = ResHelper.IsNeedSetup(eqp, lot);

			foreach (var item in list)
			{
				var schedule = item.Tag as PeriodSection;
				var isPM = schedule is PMSchedule;

				if (isPM == false)
					continue;

				var pm = schedule as FabPMSchedule;
				var reaminNextPMStart = pm.StartTime - eqp.NowDT;

				//하루 이내만 조정
				if (reaminNextPMStart.TotalDays > 1)
					continue;

				bool isAdjust = false;
				FabPMSchedule adjust = pm.Clone() as FabPMSchedule;

				if (isLast)
				{
					if ((DateTime)outTime < pm.LimitDelayTime && outTime > pm.StartTime)
					{
						adjust.StartTime = (DateTime)outTime;
						adjust.EndTime = adjust.StartTime.AddMinutes(pm.InputDuration.TotalMinutes);
						adjust.Description = "DELAY";
						adjust.AllowAheadTime = (float)(outTime - eqp.Now).TotalMinutes;
						adjust.ScheduleType = DownScheduleType.ShiftBackward;

						aeqp.DownManager.AdjustEvent(item, adjust);
						isAdjust = true;
					}
				}
				else
				{
					if (reaminNextPMStart.TotalMinutes < pm.AllowAheadTime)
					{
						prevReturnValue = null;
						adjust.StartTime = aeqp.NowDT;
						adjust.EndTime = adjust.StartTime.AddMinutes(pm.InputDuration.TotalMinutes);

						if (adjust.Description != "DELAY")
							adjust.Description = "AHEAD";

						adjust.IsNeedAdjust = false;

						aeqp.DownManager.AdjustEvent(item, adjust);
						isAdjust = true;
					}
				}

				if (isAdjust == false && pm.StartTime < outTime)
				{
					float setupTime = 0f;
					if (isNeedSetup)
						setupTime = SetupMaster.GetSetupTime(aeqp, lot);

					adjust.StartTime = (DateTime)(outTime + Time.FromMinutes(setupTime));
					adjust.EndTime = adjust.StartTime.AddMinutes(pm.InputDuration.TotalMinutes);

					aeqp.DownManager.AdjustEvent(item, adjust);
				}
			}

			//var data = aeqp.DownManager.GetStartScheduleItems(Time.MaxValue);
			return prevReturnValue;
		}

		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public bool IS_WRITE_DISPATCH_LOG0(Mozart.SeePlan.Simulation.AoEquipment aeqp, ref bool handled, bool prevReturnValue)
		{
			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="eqp"/>
		/// <param name="info"/>
		/// <param name="lot"/>
		/// <param name="wp"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public string ADD_DISPATCH_WIP_LOG0(Mozart.SeePlan.DataModel.Resource eqp, Mozart.SeePlan.Simulation.EntityDispatchInfo info, Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.WeightPreset wp, ref bool handled, string prevReturnValue)
		{
			string log = string.Empty;

			var flot = lot as FabLot;
			var targetEqp = eqp as FabEqp;

			log = DispatchLogHelper.GetDispatchWipLog(targetEqp, info, flot, wp);


			return log;
		}

		/// <summary>
		/// </summary>
		/// <param name="eqp"/>
		/// <param name="sels"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public string GET_SELECTED_WIP_LOG0(Mozart.SeePlan.DataModel.Resource eqp, Mozart.SeePlan.Simulation.IHandlingBatch[] sels, ref bool handled, string prevReturnValue)
		{
			string log = DispatchLogHelper.GetSelectedWipLog(eqp as FabEqp, sels);

			return log;
		}

		/// <summary>
		/// </summary>
		/// <param name="da"/>
		/// <param name="info"/>
		/// <param name="handled"/>
		public void WRITE_DISPATCH_LOG0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.EqpDispatchInfo info, ref bool handled)
		{
			var targetEqp = info.TargetEqp as FabEqp;
			string eqpID = targetEqp.EqpID;

			if (eqpID == "THCVD200")
				Console.WriteLine();

			var eqp = da.GetEquipment(eqpID) as FabAoEquipment;

			//ParallelChamber는 SubEqp별로 별도로 기록 (더미Lot선택은 기록)
			if (eqp.IsParallelChamber && info.DispatchWipLog.StartsWith("DUMMY"))
			{
				foreach (var subEqp in targetEqp.SubEqps.Values)
				{
					if (subEqp.Current != null)
						continue;

					DispatchLogHelper.WriteDispatchLog(eqp, info, subEqp);
				}
			}

			//ParallelChamber도 ParentEqpID 기준으로 추가 기록함(2019.11.08)
			////ParallelChamber는 SubEqp별로 별도로 기록
			//if (eqp.IsParallelChamber)
			//    return;

			DispatchLogHelper.WriteDispatchLog(eqp, info);
		}

		/// <summary>
		/// </summary>
		/// <param name="da"/>
		/// <param name="aeqp"/>
		/// <param name="wips"/>
		/// <param name="handled"/>
		public void ON_DISPATCHED0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch[] wips, ref bool handled)
		{
			var eqp = aeqp.ToFabAoEquipment();

			foreach (var item in wips)
			{
				var lot = item.ToFabLot();

				SetProductVersion(eqp, lot);
				EqpArrangeMaster.OnDispatched(eqp, lot);

				FilterMaster.StartEqpRecipeTime(lot, eqp);
			}
		}

		private void SetProductVersion(FabAoEquipment eqp, FabLot lot)
		{
			//if (eqp.EqpID == "THPHL100" && lot.LotID == "LARRAYI0021")
			//    Console.WriteLine("B");

			var stdStep = lot.CurrentFabStep.StdStep;
			string fromProductVer = lot.CurrentProductVersion;

			if (EqpArrangeMaster.AvailableChangeProductVer(stdStep, fromProductVer) == false)
				return;

			var currEqpArrange = lot.CurrentEqpArrange;
			if (currEqpArrange == null)
				return;

			string toProductVer = currEqpArrange.UsedMaskProductVersion;
			if (string.IsNullOrEmpty(toProductVer))
				return;

			if (fromProductVer == toProductVer)
				return;

			lot.OrigProductVersion = toProductVer;
			lot.CurrentProductVersion = lot.OrigProductVersion;

			lot.CurrentFabPlan.ProductVersion = toProductVer;

			OutCollector.WriteLotVerChangeInfo(lot, eqp.Target as FabEqp, fromProductVer, toProductVer);
		}

		public IList<IHandlingBatch> RETRY_LOT_GROUP0(AoEquipment aeqp, IList<IHandlingBatch> wips, ref bool handled, IList<IHandlingBatch> prevReturnValue)
		{

			List<IHandlingBatch> list = new List<IHandlingBatch>();
			if (aeqp.Dispatcher is FifoDispatcher)
			{
				foreach (IHandlingBatch lotGroup in wips)
				{
					foreach (var lot in lotGroup)
					{
						list.Add(lot);
					}
				}

				list.Sort(new CompareHelper.FifoDispatcherSort());

				return list;
			}

			return wips;
		}

		public IList<IHandlingBatch> SORT_LOT_GROUP_CONTENTS1(DispatcherBase db, IList<IHandlingBatch> list, IDispatchContext ctx, ref bool handled, IList<IHandlingBatch> prevReturnValue)
		{
			list.QuickSort(LotGroupSorter);

			return list;
		}

		private int LotGroupSorter(IHandlingBatch x, IHandlingBatch y)
		{
			DateTime now = AoFactory.Current.NowDT;
			FabLot a = x.ToFabLot();
			FabLot b = y.ToFabLot();

			int cmp = 0;

			//LotPriority
			if (cmp == 0)
				cmp = a.Priority.CompareTo(b.Priority);

			//MaxQtime
			if (cmp == 0)
				cmp = a.GetMinimumRemainTime(now).CompareTo(b.GetMinimumRemainTime(now));

			//Fifo
			if (cmp == 0)
				cmp = a.DispatchInTime.CompareTo(b.DispatchInTime);

			return cmp;
		}
	}
}
