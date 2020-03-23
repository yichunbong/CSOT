using CSOT.Lcd.Scheduling.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;
using Mozart.Simulation.Engine;
using Mozart.SeePlan;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class FilterMaster
	{
		private static Dictionary<string, List<EqpRecipeInfo>> _cacheRecipeInfo = new Dictionary<string, List<EqpRecipeInfo>>();
		
		public static void DoFilter(AoEquipment eqp, IList<IHandlingBatch> wips, IDispatchContext ctx)
		{
			//if (eqp.EqpID == "THPHL700" && eqp.NowDT >= LcdHelper.StringToDateTime("20200115 073000"))
			//	Console.WriteLine("B");

			for (int i = wips.Count - 1; i >= 0; i--)
			{
				var hb = wips[i];

				if (hb.HasContents)
				{
					var hbs = hb.Contents;

					for (int j = hbs.Count - 1; j >= 0; j--)
					{
						var lot = hbs[j] as FabLot;
						if (IsLoadable(eqp, lot) == false)
						{
							hbs.RemoveAt(j);
							continue;
						}
					}

					if (hb.Contents.Count == 0)
					{
						wips.RemoveAt(i);
						continue;
					}
				}
				else
				{
					if (IsLoadable(eqp, hb.ToFabLot()) == false)
					{
						wips.RemoveAt(i);
						continue;
					}
				}
			}
		}

		private static bool IsLoadable(AoEquipment eqp, FabLot lot)
		{
			string reason = string.Empty;

			
			//EqpArrange
			if (IsLoadableEqpArrange(eqp, lot) == false)
			{
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, "EqpArrnage LimitQty");
				return false;
			}

			//Mask
			if (IsLoadableToolArrange(eqp, lot) == false)
			{
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, "Mask");
				return false;
			}

			//Jig
			if (IsLoadableJigArrange(eqp, lot) == false)
			{
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, "Jig");
				return false;
			}

			//ParallelChamber 확인
			if (ChamberMaster.IsLoadable_ParallelChamber(eqp, lot) == false)
			{				
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, "Not Available ChamberUnit");
				return false;
			}

			//설비간 이동시간 반영
			if (IsArrive(eqp, lot, ref reason) == false)
			{
				reason = string.Format("TransferTime／{0} ", reason);

				lot.CurrentFabPlan.AddLotFilteredInfo(reason, DispatchFilter.TransferTime);
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, reason);

				return false;
			}

			//MinQtime 제약
			if (IsStayDuringMinQTime(eqp, lot, ref reason) == false)
			{
				reason = string.Format("Stay MinTime／{0}", reason);

				lot.CurrentFabPlan.AddLotFilteredInfo(reason, DispatchFilter.MinQtime);
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, reason);

				return false;
			}

			//EqpRecipe 제약
			if (IsEqpRecipeTime(eqp, lot, eqp.Now, ref reason) == false)
			{
				reason = string.Format("RecipeTime／{0}", reason);

				lot.CurrentFabPlan.AddLotFilteredInfo(reason, DispatchFilter.RecipeTime);
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, reason);

				return false;
			}

			//OwnerID Limit
			if (IsNotOwnerLimit(eqp, lot, ref reason) == false)
			{
				reason = string.Format("OwenrLimit／{0}", reason);

				lot.CurrentFabPlan.AddLotFilteredInfo(reason, DispatchFilter.OwnerLimit);
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, reason);

				return false;
			}

			//PREVENT_LAYER_CHANGE_FILTER
			if (IsFilterPreventLayerChange(eqp, lot))
			{
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, "PreventLayerChangeFilter");
				return false;
			}

			//CF_PHOTO_TARGET_LINE_FILTER
			if (IsCfPhotoTargetLine(eqp, lot) == false)
			{
				eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, "CfPhotoTargetLineFilter");
				return false;
			}

			return true;
		}

		public static void DoGroupFilter(AoEquipment aeqp, IList<IHandlingBatch> wips, IDispatchContext ctx, List<IHandlingBatch> revaluation)
		{
			var eqp = aeqp.ToFabAoEquipment();

			FilterHelper.BuildJobFilterInfo(eqp, wips, ctx);

			List<JobFilterInfo> list = ctx.Get<List<JobFilterInfo>>(Constants.JobGroup, null);

			if (list == null)
				return;

			EvaluateJobFilter(eqp, list);

			for (int i = wips.Count - 1; i >= 0; i--)
			{
				var hb = wips[i];

				bool isRemove = false;
				if (hb.HasContents)
				{
					for (int j = hb.Contents.Count - 1; j >= 0; j--)
					{
						var lot = hb.Contents[j] as FabLot;
						if(DoGroupFilterMore(aeqp, lot, revaluation))
						{
							hb.Contents.RemoveAt(j);
							continue;
						}
					}

					isRemove = hb.Contents.Count == 0;
				}
				else
				{
					isRemove = DoGroupFilterMore(eqp, hb.ToFabLot(), revaluation);
				}

				if (isRemove)
				{
					wips.RemoveAt(i);
					continue;
				}
			}
		}

		private static bool DoGroupFilterMore(AoEquipment aeqp, FabLot lot, List<IHandlingBatch> revaluation)
		{
			var eqp = aeqp.ToFabAoEquipment();

			if (HasFilterInfo(aeqp, lot))
			{
				//재평가 필요한 경우(구현필요)
				if (HasWeakFilterInfo(aeqp, lot))
					revaluation.Add(lot);
				else
					WriteFilterInfo(lot, eqp);

				return true;
			}

			return false;
		}

		private static void EvaluateJobFilter(AoEquipment aeqp, List<JobFilterInfo> list)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();
			string runMode = eqp.GetCurrentRunMode();

			Dictionary<string, List<FabAoEquipment>> workingEqps = ResHelper.GetWorkingEqpInfos(aeqp, true);

			foreach (JobFilterInfo info in list)
			{
				List<FabAoEquipment> eqpList = info.InitJobFilterInfo(eqp, workingEqps);

				//RunMode
				if(info.IsFilterRunMode(eqp, list))
				{
					info.FilterType = DispatchFilter.RunMode;
					info.FilterReason = runMode;
					continue;
				}

				//Dummy Wait 필터
				if (info.IsFilterDummyWaitEqp(eqp, eqpList))
				{
					info.FilterType = DispatchFilter.DummyWaitEqp;
					continue;
				}

				//Setup 필터
				if (info.IsFilterSetup(eqp))
				{
					info.FilterType = DispatchFilter.Setup;
					continue;
				}

				//New Eqp Assign 필터
				if (info.IsFilterNewEqpAssign(eqp, eqpList))
				{
					info.FilterType = DispatchFilter.NewEqpAssign;
					continue;
				}

				

				//TODO : 로직 검증 필요 (bong - 2019.11.11)
				////M Type Arrnage 잔여반영 필터
				//if (info.IsFilterInflowMoreThenRemainArrMtype(eqp))
				//{
				//	info.FilterType = DispatchFilter.InflowArrTypeM;
				//	continue;
				//}
			}
		}

		private static bool HasFilterInfo(AoEquipment eqp, FabLot lot)
		{
			if (lot.DispatchFilterInfo == null)
				return false;

			if (lot.DispatchFilterInfo.FilterType == DispatchFilter.None)
				return false;

			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_PREVENT_SMALL_LOT_FILTER);
			if (wf == null || wf.Factor == 0)
			{
				//자신의 초기 Step일 경우에 Lot Priority 가 1,2,3 일 경우 필터하지 않음
				if (lot.CurrentStep == lot.Wip.InitialStep)
				{
					if (lot.Wip.Priority == 1 || lot.Wip.Priority == 2 || lot.Wip.Priority == 3)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static bool HasWeakFilterInfo(AoEquipment eqp, FabLot lot)
		{
			if (lot.DispatchFilterInfo == null)
				return false;

			if (lot.DispatchFilterInfo.FilterType == DispatchFilter.Setup)
				return true;

			if (lot.DispatchFilterInfo.FilterType == DispatchFilter.NewEqpAssign)
				return true;

			if (lot.DispatchFilterInfo.FilterType == DispatchFilter.InflowArrTypeM)
				return true;

			return false;
		}

		internal static IList<IHandlingBatch> Revaluate(AoEquipment eqp, List<IHandlingBatch> wips)
		{
			if (eqp.Dispatcher is FifoDispatcher)
				return wips;

			for (int i = wips.Count - 1; i >= 0; i--)
			{
				var hb = wips[i];
				var lot = hb.ToFabLot();

				bool isRevaluate = false;

				//ALLOW_SMALL_LOT_FILTER 기준으로 소량인 경우만 Revaluate 시킴
				if (lot.DispatchFilterInfo.FilterType == DispatchFilter.Setup)
				{
					if (FilterMaster.IsStepSmallLot(eqp, lot))
						isRevaluate = true;
				}

				if(lot.DispatchFilterInfo.FilterType == DispatchFilter.NewEqpAssign)
				{
					if (lot.DispatchFilterInfo.NewAssignNeedCount > 0)
						isRevaluate = true;
				}


				if (isRevaluate)
				{
					lot.CurrentFabPlan.AddLotFilteredInfo("Revaluate", DispatchFilter.Revaluate);
				}
				else
				{
					wips.RemoveAt(i);
					WriteFilterInfo(hb, eqp);
				}
			}

			return wips;
		}

		private static bool IsLoadableEqpArrange(AoEquipment aeqp, FabLot lot)
		{
			var eqp = aeqp.ToFabAoEquipment();

			bool checkQty = true;
			if (InputMart.Instance.GlobalParameters.ApplyArrangeMType == false)
				checkQty = false;

			bool isLoadable = EqpArrangeMaster.IsLoadable(eqp, lot, checkQty);
			if (isLoadable == false)
				return false;

			//1차 선택 (1차 : Mask 미고려 선택, 2차 : Mask 고려 후 선택)
			if (lot.CurrentEqpArrange != null)
			{
				string productVersion = lot.CurrentProductVersion;
				lot.CurrentEqpArrange.SetUsedArrange(productVersion);
			}

			return true;
		}

		internal static bool IsLoadableToolArrange(AoEquipment aeqp, FabLot lot, bool isReal = true)
		{
			var eqp = aeqp.ToFabAoEquipment();

			if (MaskMaster.IsUseTool(lot) == false)
				return true;

			var mask = MaskMaster.SelectBestMask(aeqp, lot);
			if (mask == null)
				return false;

			if (isReal)
				lot.CurrentMask = mask;

			return true;
		}

		private static bool IsLoadableJigArrange(AoEquipment aeqp, IHandlingBatch hb, bool isReal = true)
		{
			var lot = hb.ToFabLot();
			var eqp = aeqp.ToFabAoEquipment();

			if (JigMaster.IsUseJig(lot) == false)
				return true;

			List<FabMask> list = JigMaster.SelectJigMask(aeqp, lot);

			if (list == null || list.Count < eqp.UseJigCount)
				return false;

			if (isReal)
				lot.CurrentJig = list;

			return true;
		}

		#region JobFilterInfo - IsFilters

		private static bool IsFilterNewEqpAssign(this JobFilterInfo info, FabAoEquipment eqp, List<FabAoEquipment> workingEqpList)
		{
			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_NEW_EQP_ASSIGN_FILTERING);
			if (wf == null || wf.Factor == 0)
				return false;

			//연속진행인 경우
			if (info.IsRunning)
				return false;

			//Setup이 필요없는 제품의 경우 Filter 제외(2019.12.16 - by.liujian(유건))
			if (eqp.IsParallelChamber == false)
			{
				var setupTime = info.SetupTime;
				if (setupTime <= 0)
					return false;
			}

			////NoAssign 제품의 경우는 IsFilterSetup로 체크(bong - J/C 증가로 제외)
			//if (info.IsNoAssign)
			//    return false;

			//if (eqp.EqpID == "FHMPH100" && eqp.NowDT >= LcdHelper.StringToDateTime("20200113 203008"))
			//	Console.WriteLine("B");

			double param1 = (double)wf.Criteria[0];
			double param2 = (double)wf.Criteria[1];

			//if (info.IsNoAssign)
			//{
			//	WeightFactor awf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_ALLOW_RUN_DOWN_TIME);
			//	if (awf != null && awf.Factor != 0)
			//		param1 = param1 + Convert.ToDouble(awf.Criteria[0]);
			//}

			int workCnt = workingEqpList != null ? workingEqpList.Count : 0;

			//param1
			string reason;
			bool isFilter = IsFilterNewEqpAssign(info, eqp, param1, workCnt, out reason);

			//param2
			if (isFilter == false && workCnt > 0)
			{
				if (param2 > param1)
					isFilter = IsFilterNewEqpAssign(info, eqp, param2, workCnt, out reason);
			}

			if (isFilter)
				info.FilterReason = reason;

			return isFilter;
		}

		private static bool IsFilterNewEqpAssign(JobFilterInfo info, FabAoEquipment eqp, double inflowHour, int workCnt, out string reason)
		{
			reason = string.Empty;
						
			double needCnt = Math.Round(info.GetNewAssignNeedCnt(eqp, inflowHour, workCnt), 2);

			bool isFilter = needCnt < 1;

			if (isFilter)
			{
				reason = string.Format("HOUR({0})：REQ({1}), WORK({2})：{3} < 1",
									   Math.Round(inflowHour, 2),
									   Math.Round(needCnt, 2),
									   workCnt,
									   needCnt);
			}

			info.NewAssignNeedCount = needCnt;

			return needCnt < 1;
		}

		private static bool IsFilterSetup(this JobFilterInfo info, AoEquipment aeqp)
		{
			WeightFactor wf = WeightHelper.GetWeightFactor(aeqp.Target.Preset, Constants.WF_SETUP_FILTERING);
			if (wf == null || wf.Factor == 0)
				return false;

			FabLot lot = info.Sample;
			if (lot == null)
				return false;

			FabAoEquipment eqp = aeqp.ToFabAoEquipment();
			var step = info.Step;

			//if (eqp.EqpID == "CHPIL300" && eqp.NowDT >= LcdHelper.StringToDateTime("20200119 105032")
			//	&& lot.CurrentProductID == "CW42512AB000")
			//	Console.WriteLine("B");

			if (eqp.IsLastPlan(lot))
				return false;

			double setupTime = info.SetupTime;
			if (setupTime <= 0)
				return false;

			double ratio = Convert.ToDouble(wf.Criteria[0]);

			double continuousQty = info.WaitSum;

			if(continuousQty > 0)
				continuousQty += InFlowMaster.GetContinuousQty(lot, step);
			
			if (eqp.IsParallelChamber)
				continuousQty = continuousQty / eqp.GetChamberCapacity();

			var st = step.GetStepTime(aeqp.EqpID, info.ProductID);
			if (st == null)
				return false;

			double tactTime = st.TactTime;
			double workSec = Math.Round(continuousQty * tactTime, 2);
			double setupSec = Math.Round(setupTime * 60 * ratio, 2);
						
			bool isFilter = workSec < setupSec;
			if (isFilter == false)
				return false;

			//단순 Setup > Tact Time 일 경우 Inflow를 고려
			//다른 곳에서 진행중인가? Yes : 필터, No: 소량검사
			if (SimHelper.IsAnyWorking(eqp, lot) == false)
			{
				//기다려도 오지 않는 작은 Lot인가? Yes : 필터하지 않음. No : 필터
				if (IsWaitSmallSizeLot(aeqp, info, lot, continuousQty, setupTime, ratio, st))
					isFilter = false;
			}

			if (isFilter)
				info.FilterReason = string.Format("SetupTime：{0} > {1}(Qty：{2} * Tact：{3})", setupSec, workSec, continuousQty, st.TactTime);

			return isFilter;
		}

		private static bool IsWaitSmallSizeLot(AoEquipment aeqp, JobFilterInfo info, FabLot lot, double waitQty, double setupTime, double ratio, StepTime st)
		{
			var step = lot.CurrentFabStep;
			if (step == null)
				return false;

			var stdStep = step.StdStep;
			if (stdStep == null || stdStep.IsInputStep)
				return false;
				
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			TimeSpan firstInflowTime = TimeSpan.FromMinutes(setupTime);
			decimal allowTime = 3m;

			WeightFactor wf;
			WeightHelper.TryGetEqpWeightFactor(eqp, Constants.WF_ALLOW_RUN_DOWN_TIME, out wf);		
			if (wf != null)
				allowTime = (decimal)wf.Criteria[0];

			double inflowQty1 = Convert.ToDouble(InFlowAgent.GetInflowQty(info, aeqp, (decimal)firstInflowTime.TotalHours, 0));
			double inflowQty2 = Convert.ToDouble(InFlowAgent.GetInflowQty(info, aeqp, allowTime, 0));

			double waitQty1 = waitQty + inflowQty1;
			double waitQty2 = waitQty + inflowQty2;

			//Setup 시간 이내에 유입이 있나?
			if (LcdHelper.IsIncludeInRange(waitQty, waitQty1 * 0.95d, waitQty1 * 1.05d))
			{
				//지정된 시간내에 유입재공이 있나?
				if (LcdHelper.IsIncludeInRange(waitQty, waitQty2 * 0.95d, waitQty2 * 1.05d))
				{
					double requiredSec = st.TactTime * waitQty2;

					bool isSmall = requiredSec < setupTime * 60 * ratio;

					return isSmall;
				}
			}

			return false;
		}

		private static bool IsFilterDummyWaitEqp(this JobFilterInfo info, FabAoEquipment eqp, List<FabAoEquipment> eqpList)
		{
			if (eqpList == null)
				return false;

			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_DUMMY_WAIT_EQP_FILTER);
			if (wf == null || wf.Factor == 0)
				return false;

			//연속진행인 경우
			if (info.IsRunning)
				return false;

			var waitEqps = eqpList.FindAll(x => x.IsDummyWait);

			bool isFilter = waitEqps.Count > 0;

			return isFilter;
		}

		private static bool IsFilterRunMode(this JobFilterInfo info, FabAoEquipment eqp, List<JobFilterInfo> jobList)
		{
			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_RUN_MODE_FILTER);
			if (wf == null || wf.Factor == 0)
				return false;

			//연속진행인 경우
			if (info.IsRunning)
				return false;

			string eqpGroup = eqp.TargetEqp.EqpGroup;
			string runMode = eqp.GetCurrentRunMode();
			
			var branchStep = BranchStepMaster.GetBranchStep(eqpGroup, runMode);
			if (branchStep == null)
				return false;
							
			if (branchStep.IsAllProduct)
				return false;

			var productList = branchStep.ProductList;
			if (productList == null || productList.Count == 0)
				return false;

			string productID = info.ProductID;
			string ownerType = info.OwnerType;			

			bool isFilter = true;
			if(branchStep.IsLoadable(productID, ownerType))
				isFilter = false;
				
			if(isFilter)
			{
				string defaultOwnerType = SiteConfigHelper.GetDefaultOwnerType();
				if (ExistRemainWip(jobList, productList, defaultOwnerType) == false)
					isFilter = false;
			}

			return isFilter;
		}

		private static bool ExistRemainWip(List<JobFilterInfo> jobList, List<string> productList, string defaultOwnerType)
		{
			if (productList == null || productList.Count == 0)
				return false;

			foreach (var info in jobList)
			{
				string productID = info.ProductID;
				string ownerType = info.OwnerType;

				if (productList.Contains(productID) == false || ownerType != defaultOwnerType)
					continue;

				if (info.ExistInflowWip)
					return true;
			}

			return true;
		}

		private static bool IsFilterInflowMoreThenRemainArrMtype(this JobFilterInfo info, FabAoEquipment eqp)
		{
			if (InputMart.Instance.GlobalParameters.ApplyArrangeMType == false)
				return false;

			WeightFactor wf;
			WeightHelper.TryGetEqpWeightFactor(eqp, Constants.WF_MIN_MOVEQTY_PRIORITY, out wf);

			if (wf == null || wf.Factor == 0)
				return false;

			FabLot lot = info.Sample;
			if (info.IsRunning)
				return false;

			if (lot == null)
				return false;

			var list = lot.CurrentEqpArrange.EqpArrrangeSet.Items.FindAll(x => x.ActivateType == ActivateType.M);
			if (list == null || list.Count == 0)
				return false;

			float minMoveQty = (int)wf.Criteria[0] / 2;

			float tactTime = (float)SiteConfigHelper.GetDefaultTactTime().TotalSeconds;

			StepTime st = info.Step.GetStepTime(eqp.EqpID, info.ProductID);
			if (st != null)
				tactTime = st.TactTime;

			Time inflowTime = Time.FromSeconds(minMoveQty * tactTime);

			decimal inflowQty = InFlowAgent.GetInflowQty(lot, eqp, (decimal)inflowTime.TotalHours, 0);

			Time endTime = eqp.Now + inflowTime;
			bool isContinueNextDay = ShopCalendar.StartTimeOfNextDay(eqp.NowDT) <= endTime;

			foreach (var item in list)
			{
				int remainQty = item.RemainQty;
				if (isContinueNextDay && item.IsDailyMode)
					remainQty += item.LimitQty;

				//limit(M) 잔여 수량이 MIN_MOVEQTY의 1 / 2 이상인 경우 체크 제외.
				if (remainQty >= minMoveQty)
					continue;

				if (remainQty < inflowQty)
				{
					info.FilterReason = string.Format("Remain：{0} < Inflow：{1}", remainQty, inflowQty);
					return true;
				}
			}

			return false;
		}
	
		#endregion

		#region Lot별 필터확인

		private static bool IsNotOwnerLimit(AoEquipment aeqp, FabLot lot, ref string reason)
		{
			string key = LcdHelper.CreateKey(lot.CurrentShopID, lot.CurrentStepID, lot.OwnerID);
			OwnerLimitInfo info;
			InputMart.Instance.OwnerLimitInfo.TryGetValue(key, out info);

			if (info == null)
				return true;

			string eqpID = aeqp.EqpID;

			//N >> Y 순
			if (info.HasN)
			{
				if (info.NList.Contains(eqpID))
				{
					reason = string.Format("Type N");
					return false;
				}
			}

			if (info.HasY)
			{
				if (info.YList.Contains(eqpID))
				{
					return true;
				}
				else
				{
					//Y인 정보가 한개라도 등록된 경우 (등록된 Eqp만 가능)
					reason = string.Format("No Register Type Y");
					return false;
				}
			}

			return true;
		}

		public static bool IsEqpRecipeTime(AoEquipment aeqp, FabLot lot, Time inTime, ref string reason)
		{
			if (InputMart.Instance.GlobalParameters.ApplyRecipeTime == false)
				return true;

			var eqp = aeqp.ToFabAoEquipment();
			List<EqpRecipeInfo> list = FilterMaster.GetRecipeMatchList(eqp, lot);

			if (list == null || list.Count == 0)
				return true;

			EqpRecipeInfo info = list[0];

			//M 무조건 투입방지 (Matching에서 활용가능한 M만 가져왔음)
			if (info.Flag == RecipeFlag.M)
			{
				reason = string.Format("M：{0}≤{1}", info.MaxCount, info.TrackInCount);
				return false;
			}

			//TestLot 하나 흘림. 그동안 해당 제품은 흘릴 수 없음.
			else if (info.Flag == RecipeFlag.X)
			{
				if (info.ActiveEndTime == Time.MaxValue)
				{
					FilterMaster.AddCurrentEqpRecipeInfo(lot, info);
					return true;
				}

				if (info.ActiveStartTime != DateTime.MinValue)
				{
					if (inTime < info.ActiveEndTime)
					{
						Time remain = info.ActiveEndTime - inTime;
						reason = string.Format("X：Remain {0}Hour", remain.TotalHours.ToRound(1));
						return false;
					}
				}
			}
			//DueDate까지만 흘릴 수 있음. (DueDate 이내에 Lot이 들어가지 않을 경우 X처럼동작
			else if (info.Flag == RecipeFlag.Y)
			{
				if (info.ChangeYtoX)
				{
					if (info.ActiveStartTime != DateTime.MinValue)
					{
						if (inTime < info.ActiveEndTime)
						{
							Time remain = info.ActiveEndTime - inTime;
							reason = string.Format("Y：Remain {0}Hour", remain.TotalHours.ToRound(1));
							return false;
						}
					}
				}
				else
				{
					FilterMaster.AddCurrentEqpRecipeInfo(lot, info);
				}
			}

			return true;
		}

		private static bool IsStayDuringMinQTime(AoEquipment eqp, FabLot lot, ref string reason)
		{
			if (SimHelper.IsCellRunning)
				return true;

			DateTime now = AoFactory.Current.NowDT;

			if (lot.QtimeInfo == null)
				return true;

			var find = lot.QtimeInfo.FindMaximumMinHoldTime(now);
			if (find == null)
				return true;

			Time remainTime = find.RemainMinHoldTime(lot, lot.CurrentFabStep, now);

			/* 2020-01-03 Wait 까지만 (아래코드는 Run Out 까지)
			AoProcess proc = eqp.Processes[0];
			Time outTime = proc.GetUnloadingTime(lot);
			Time flowTime = outTime - eqp.Now;
			Time setupTime = Time.FromMinutes(SetupMaster.GetSetupTime(eqp, lot));

			remainTime -= (setupTime + flowTime);
			*/

			if (remainTime > 0)
			{
				reason = string.Format("Remain {0}Min", remainTime.TotalMinutes.ToRound(2));
				lot.CurrentFabPlan.LotFilterInfo.RemainMinStayTime = remainTime;

				return false;
			}

			return true;
		}

		private static bool IsArrive(AoEquipment aeqp, FabLot lot, ref string reason)
		{
			var eqp = aeqp.ToFabAoEquipment();
			DateTime now = aeqp.NowDT;

			//초기 Wait재공
			if (lot.PreviousPlan == null && lot.Wip.InitialStep.StepID == lot.CurrentFabStep.StepID)
				return true;

			Time tranferTime = TransferMaster.GetTransferTime(lot, eqp);

			if (now < lot.DispatchInTime + tranferTime)
			{
				Time remain = lot.DispatchInTime.AddMinutes(tranferTime.TotalMinutes) - now;
				reason = string.Format("Remain {0}Min", remain.TotalMinutes.ToRound(2));
				return false;
			}

			return true;
		}

		private static bool IsStepSmallLot(AoEquipment aeqp, FabLot lot)
		{
			WeightFactor wf;
			WeightHelper.TryGetEqpWeightFactor(aeqp, Constants.WF_ALLOW_SMALL_LOT_FILTER, out wf);

			if (wf == null || wf.Factor == 0)
				return false;

			decimal inflowHour = (decimal)wf.Criteria[0];
			decimal inflowQty = InFlowAgent.GetInflowQty(lot, aeqp, inflowHour, 1);

			if (inflowQty == 0)
				return true;

			return false;
		}

		private static bool IsCfPhotoTargetLine(AoEquipment aeqp, FabLot lot)
		{
			var eqp = aeqp.ToFabAoEquipment();

			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_CF_PHOTO_TARGET_LINE_FILTER);
			if (wf == null || wf.Factor == 0)
				return true;

			//if(eqp.EqpID == "FHRPH100" && eqp.NowDT >= LcdHelper.StringToDateTime("20200115 073000") 
			//	&& lot.CurrentProductID == "TH645A1AB100")
			//	Console.WriteLine("B");

			var lineType = eqp.GetLineType();
			if (lineType == LineType.NONE)
				return true;

			bool isPegged = lot.CurrentFabPlan.IsPegged;
			if (isPegged == false)
			{
				if (eqp.IsLastPlan(lot))
				{
					return true;
				}
				else
				{
					if (lineType == LineType.SUB)
						return false;
					else
						return true;
				}
			}

			var stepTarget = lot.CurrentFabPlan.MainTarget;
			if (stepTarget == null || stepTarget.Mo == null)
				return true;

			var targetLine = stepTarget.Mo.LineType;
			if (lineType != targetLine)
			{
				if(ExistRemainStepPlan(lot, lineType) == false)
					return false;
			}

			return true;
		}

		private static bool ExistRemainStepPlan(FabLot lot, LineType lineType)
		{
			if (EixstRemainStepPlan_Wait(lot, lineType))
				return true;

			var step = lot.CurrentStep;
			var prod = lot.FabProduct;
			string key = prod.IsTestProduct ? prod.MainProductID : prod.ProductID;

			var mgr = StepPlanManager.Current;
			var plan = mgr.GetStepPlan(step, key, false);

			if (plan == null)
				return false;

			var list = plan.StepTargetList;
			if (list == null || list.Count == 0)
				return false;

			foreach (FabStepTarget stepTarget in list)
			{
				if (stepTarget.Mo.LineType == lineType)
					return true;
			}

			return false;
		}

		private static bool EixstRemainStepPlan_Wait(FabLot lot, LineType lineType)
		{
			var step = lot.CurrentFabStep;
			
			var job = InFlowMaster.GetJobState(lot);
			if (job == null)
				return false;

			var list = job.GetStepWipList(step, WipType.Wait);
			if (list == null || list.Count == 0)
				return false;

			foreach (var it in list)
			{
				if (it.CurrentFabPlan == null)
					continue;

				var stepTarget = it.CurrentFabPlan.MainTarget;
				if (stepTarget == null)
					continue;

				if (stepTarget.Mo.LineType == lineType)
					return true;
			}

			return false;
		}

		public static bool IsFilterPreventLayerChange(AoEquipment aeqp, FabLot lot)
		{
			var eqp = aeqp.ToFabAoEquipment();

			//if (eqp.EqpID == "THCVDC00" && eqp.NowDT >= LcdHelper.StringToDateTime("20200108 090229"))
			//	Console.WriteLine("B");

			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_PREVENT_LAYER_CHANGE_FILTER);
			if (wf == null || wf.Factor == 0)
				return false;

			var step = lot.CurrentFabStep;
			if (IsLastRunStep(eqp, step))
				return false;

			var stepList = SimHelper.GetDspEqpSteps(eqp.DspEqpGroupID);
			if (stepList == null || stepList.Count <= 1)
				return false;

			var last = eqp.GetLastPlan();
			if (last == null)
				return false;

			bool isFilter = ExistRunEqpByDspEqpGroup(eqp, last.FabStep) == false;

			//if (isFilter)
			//	info.FilterReason = string.Format("");

			return isFilter;
		}

		private static bool ExistRunEqpByDspEqpGroup(FabAoEquipment baseEqp, FabStep step)
		{
			if (baseEqp == null)
				return false;

			string dspEqpGroup = baseEqp.DspEqpGroupID;
			var eqpList = ResHelper.GetEqpsByDspEqpGroup(dspEqpGroup);

			if (eqpList == null || eqpList.Count == 0)
				return false;

			foreach (var eqp in eqpList)
			{
				if (eqp == baseEqp)
					continue;

				if (IsLastRunStep(eqp, step))
					return true;
			}

			return false;
		}

		private static bool IsLastRunStep(FabAoEquipment eqp, FabStep step)
		{
			if (eqp == null || step == null)
				return false;

			if (eqp.IsParallelChamber)
			{
				foreach (var subEqp in eqp.SubEqps)
				{
					var last = subEqp.LastPlan;
					if (last != null && last.StepID == step.StepID)
						return true;
				}
			}
			else
			{
				var last = eqp.GetLastPlan();
				if (last != null && last.StepID == step.StepID)
					return true;
			}

			return false;
		}

		#endregion

		#region RecipeTime 관련
		internal static List<EqpRecipeInfo> GetRecipeMatchList(FabAoEquipment eqp, FabLot lot)
		{
			FabStep step = lot.CurrentFabStep;
			FabMask mask = lot.CurrentMask;
			string productID = lot.CurrentProductID;

			string stepID = step.StdStep.RecipeLPOBM.Contains("O") ? lot.CurrentFabStep.StepID : Constants.NULL_ID;
			string prodVer = step.StdStep.RecipeLPOBM.Contains("B") ? lot.CurrentProductVersion : Constants.NULL_ID; ;
			string toolID = step.StdStep.RecipeLPOBM.Contains("M") ? mask != null ? mask.ToolID : Constants.NULL_ID : Constants.NULL_ID;

			string key = LcdHelper.CreateKey(eqp.EqpID, step.StepID, productID, prodVer, toolID);

			List<EqpRecipeInfo> list;
			if (_cacheRecipeInfo.TryGetValue(key, out list))
				return list;

			FabEqp targetEqp = eqp.TargetEqp;

			if (targetEqp.RecipeTime.TryGetValue(productID, out list) == false)
				return new List<EqpRecipeInfo>();

			List<EqpRecipeInfo> result = new List<EqpRecipeInfo>();
			foreach (var item in list)
			{
				if (item.Flag == RecipeFlag.N || item.Flag == RecipeFlag.None)
					continue;

				if (item.Flag == RecipeFlag.M && item.IsVaildMFlag == false)
					continue;

				string runMode = eqp.GetCurrentRunMode();

				//RunMode
				if (LcdHelper.IsEmptyID(runMode) == false)
				{
					if (runMode != item.RunMode)
						continue;
				}

				//Shop
				if (step.ShopID != item.ShopID)
					continue;

				//O - Step
				if (LcdHelper.IsEmptyID(stepID) == false && stepID != item.StepID)
					continue;

				//B - Version
				if (LcdHelper.IsEmptyID(prodVer) == false)
				{
					if (item.ProductVersion != prodVer)
						continue;
				}

				//M - Mask
				if (LcdHelper.IsEmptyID(toolID) == false)
				{
					if (toolID != item.ToolID)
						continue;
				}

				result.Add(item);
			}

			result.Sort(new CompareHelper.EqpRecipeTimeComparer());

			_cacheRecipeInfo.Add(key, result);

			return result;
		}

		internal static bool StartEqpRecipeTime(FabLot lot, FabAoEquipment eqp)
		{
			if (InputMart.Instance.GlobalParameters.ApplyRecipeTime == false)
				return false;

			if (lot.CurrentPlan == null)
				return false;

			if (lot.CurrentFabPlan.LotFilterInfo.RecipeTimes == null)
				return false;

			EqpRecipeInfo info;
			if (lot.CurrentFabPlan.LotFilterInfo.RecipeTimes.TryGetValue(eqp.EqpID, out info) == false)
				return false;

			if (info.ActiveEndTime != Time.MaxValue)
				Logger.MonitorInfo("!!!! Check EqpRecipeTime Logic : EqpID:{0}, LotID{1}, CHECK_FLAG:{2}  ", info.EqpID, lot.LotID, info.CheckFlag);

			if (info.Flag == RecipeFlag.N)
			{
				info.ActiveStartTime = eqp.Now;

				lot.CurrentFabPlan.CurrentRecipeTime = info;
				lot.CurrentFabPlan.IsEqpRecipe = true;
			}
			else if (info.Flag == RecipeFlag.Y)
			{
				//DueDate 이내에는 Lot을 넣을 수 있음. Select될 경우 DueDate 갱신(+7일)
				if (eqp.NowDT < info.PlanDueDate)
				{
					info.PlanDueDate = eqp.NowDT.AddDays(7);
				}
				else
				{ //DueDate 이내에 갱신되지 않았을 경우 X Flag 처럼 동작
					if (info.PlanDueDate == info.DueDate)
					{
						info.ChangeYtoX = true;

						info.ActiveStartTime = eqp.Now;

						lot.CurrentFabPlan.CurrentRecipeTime = info;
						lot.CurrentFabPlan.IsEqpRecipe = true;
					}
				}
			}
			return true;
		}

		internal static void AddCurrentEqpRecipeInfo(FabLot lot, EqpRecipeInfo info)
		{

			if (lot.CurrentFabPlan.LotFilterInfo.RecipeTimes == null)
				lot.CurrentFabPlan.LotFilterInfo.RecipeTimes = new Dictionary<string, EqpRecipeInfo>();

			lot.CurrentFabPlan.LotFilterInfo.RecipeTimes[info.EqpID] = info;

		}
		#endregion

		internal static void WriteFilterInfo(IHandlingBatch hb, AoEquipment eqp)
		{
			FabLot lot = hb.ToFabLot();
			string reason = string.Format("{0}／{1}", lot.DispatchFilterInfo.FilterType, lot.DispatchFilterInfo.FilterReason);
			eqp.EqpDispatchInfo.AddFilteredWipInfo(lot, reason);
		}

		internal static void Clear(this LotFilterInfo info)
		{
			info.FilterType = DispatchFilter.None;
			info.Reason = Constants.NULL_ID;
			info.RemainMinStayTime = Time.MinValue;
			info.RecipeTimes.Clear();
		}

		internal static List<FabLot> WaitForPrevStepWip_Dummy(IDispatchContext ctx, FabAoEquipment eqp)
		{
			List<JobFilterInfo> jobList = ctx.Get<List<JobFilterInfo>>(Constants.JobGroup, null);
			if (jobList == null)
				return null;

			FabPlanInfo last = eqp.GetLastPlan(); //eqp.LastPlan as FabPlanInfo;
			if (last == null)
				return null;

			//if (eqp.EqpID == "THWEM200" && LcdHelper.StringToDateTime("20191021235617") <= eqp.NowDT)
			//	Console.WriteLine();

			JobState state = InFlowMaster.GetJobState(last.ProductID, last.OwnerType);
			if (state == null)
				return null;

			var holdWips = state.GetHoldWipList(last.FabStep, last.ProductVersion);
			var prvRunWips = state.GetPrevStepRunWipList(last.FabStep, last.ProductVersion);

			JobFilterInfo minSetupJobFilter = null;
			List<JobFilterInfo> filteredList = new List<JobFilterInfo>();
			Dictionary<string, JobFilterInfo> current = new Dictionary<string, JobFilterInfo>();
			
			foreach (var info in jobList)
			{
				if (info.IsEmpty)
					continue;

				string key = FilterHelper.GetJobFilterKey(info);
				current.Add(key, info);

				if (FilterHelper.CheckIsRunning(eqp, info))
				{
					filteredList.Add(info);
					continue;
				}

				if (info.FilterType != DispatchFilter.None)
				{
					filteredList.Add(info);
					continue;
				}

				if (info.SetupTime == 0)
					continue;

				if (minSetupJobFilter == null)
					minSetupJobFilter = info;

				if (minSetupJobFilter.SetupTime > info.SetupTime)
					minSetupJobFilter = info;
			}

			if (minSetupJobFilter == null)
				return null;

			Dictionary<string, FabLot> avableLots = new Dictionary<string, FabLot>();

			foreach (var lot in holdWips)
			{
				if (eqp.IsLastPlan(lot.CurrentShopID, last.StepID, lot.CurrentProductID, lot.CurrentProductVersion, lot.OwnerType, lot.OwnerID))
					continue;

				string key = FilterHelper.GetJobFilterKey(lot.CurrentShopID, last.StepID, lot.CurrentProductID, lot.CurrentProductVersion, lot.OwnerType);
				if (current.ContainsKey(key))
					continue;

				Time remainHold = lot.HoldTime - (eqp.NowDT - lot.HoldStartTime);
				float setupTime = SetupMaster.GetSetupTime(eqp, lot);

				if (remainHold.TotalMinutes + setupTime < minSetupJobFilter.SetupTime)
				{
					if (avableLots.ContainsKey(key) == false)
						avableLots.Add(key, lot);
				}
			}

			foreach (var lot in prvRunWips)
			{
				string lastShopID = last.ShopID;
				string lastStepID = last.StepID;
				string currProductID = lot.CurrentProductID;
				string origProductVersion = lot.OrigProductVersion;
				string ownerType = lot.OwnerType;
				string ownerID = lot.OwnerID;

				//TODO : bong - product version ??
				if (eqp.IsLastPlan(lastShopID, lastStepID, currProductID, origProductVersion, ownerType, ownerID))
					continue;

				string key = FilterHelper.GetJobFilterKey(lastShopID, lastStepID, currProductID, origProductVersion, ownerType);
				if (current.ContainsKey(key))
					continue;

				Time tranferTime = TransferMaster.GetTransferTime(lot, eqp);						
				Time setupTime = SetupMaster.GetSetupTime(eqp, lastShopID, lastStepID, currProductID, origProductVersion, ownerType, ownerID);

				if (tranferTime + setupTime < minSetupJobFilter.SetupTime)
				{
					if (avableLots.ContainsKey(key) == false)
						avableLots.Add(key, lot);
				}
			}

			Dictionary<string, List<FabAoEquipment>> workingEqps = ResHelper.GetWorkingEqpInfos(eqp, true);

			List<FabLot> list = new List<FabLot>();
			foreach (var lot in avableLots.Values)
			{
				FabPlanInfo plan = EntityControl.Instance.CreateLoadInfo(lot, last.Step) as FabPlanInfo;
				FabLot dummy = CreateHelper.CreateDispatchDummyLot(last.FabStep, plan);
				dummy.LotID = "DUMMY_PREVSTEP";

				JobFilterInfo jobfilter = CreateHelper.CreateDispatchFilterInfo(last.Step as FabStep, lot.CurrentProductID, lot.OrigProductVersion, lot.OwnerType, lot.OwnerID);
				jobfilter.InitJobFilterInfo(eqp, workingEqps);
				jobfilter.LotList.Add(dummy);
				dummy.DispatchFilterInfo = jobfilter;

				list.Add(dummy);
			}

			return list;
		}
	}
}