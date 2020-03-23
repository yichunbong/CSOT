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
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class WeightHelper
	{
		public static void WriteWeightPresetLog()
		{
			HashSet<FabWeightPreset> list = new HashSet<FabWeightPreset>();

			var eqps = AoFactory.Current.Equipments.Values;
			foreach (var eqp in eqps)
			{
				var preset = eqp.Preset as FabWeightPreset;
				if (preset == null)
					continue;

				list.Add(preset);
			}

			foreach (var preset in list)
			{
				WriteWeightPresetLog(preset);
			}
		}

		private static void WriteWeightPresetLog(FabWeightPreset preset)
		{
			if (preset == null)
				return;

			string presetID = preset.Name;
			string mapPresetID = preset.MapPresetID;

			foreach (FabWeightFactor factor in preset.FactorList)
			{
				Outputs.WeightPresetLog row = new WeightPresetLog();

				row.VERSION_NO = ModelContext.Current.VersionNo;
				row.PRESET_ID = presetID;
				row.MAP_PRESET_ID = mapPresetID;
				row.FACTOR_ID = factor.Name;
				row.FACTOR_TYPE = factor.Type.ToString();
				row.FACTOR_WEIGHT = factor.Factor;
				row.FACTOR_NAME = Constants.NULL_ID;
				row.SEQUENCE = (int)factor.Sequence;
				row.ORDER_TYPE = factor.OrderType.ToString();
				row.CRITERIA = factor.OrigCriteria;
				row.ALLOW_FILTER = LcdHelper.ToStringYN(factor.IsAllowFilter);

				OutputMart.Instance.WeightPresetLog.Add(row);
			}
		}

		public static FabWeightPreset GetWeightPreset(string presetID)
		{
			if (LcdHelper.IsEmptyID(presetID))
				return null;

			FabWeightPreset preset;
			InputMart.Instance.FabWeightPreset.TryGetValue(presetID, out preset);

			return preset;
		}

		public static FabWeightPreset GetSafeWeightPreset(string presetID)
		{
			if (LcdHelper.IsEmptyID(presetID))
				return null;

			FabWeightPreset preset;
			if (InputMart.Instance.FabWeightPreset.TryGetValue(presetID, out preset) == false)
			{
				preset = new FabWeightPreset(presetID);
				InputMart.Instance.FabWeightPreset.Add(presetID, preset);
			}

			return preset;
		}

		public static T GetCriteria<T>(WeightFactor factor, int index, T defaultValue)
		{
			try
			{
				if (factor == null)
					return defaultValue;

				var criteria = factor.Criteria;
				if (criteria == null || criteria.Length <= index)
					return defaultValue;

				var ovalue = criteria[index];
				if (ovalue is T)
					return (T)ovalue;

				return defaultValue;
			}
			catch
			{
				return defaultValue;
			}
		}

		public static string[] ParseCriteria(string criteria, int number)
		{
			string[] arr = new string[number];
			//arr.ForEach(t => t = string.Empty);

			if (criteria == null)
				return arr;

			string[] splits = criteria.Split(';');

			int count = splits.Length;
			for (int i = 0; i < count; i++)
			{
				if (i >= number)
					break;

				arr[i] = splits[i];
			}

			return arr;
		}

		public static WeightFactor GetWeightFactor(WeightPreset preset, string name)
		{
			if (preset == null || preset.FactorList == null)
				return null;

			foreach (var wf in preset.FactorList)
			{
				if (string.Equals(wf.Name, name, StringComparison.CurrentCultureIgnoreCase))
					return wf;
			}

			return null;
		}

		public static bool TryGetEqpWeightFactor(AoEquipment aeqp, string factorName, out WeightFactor wf)
		{
			wf = WeightHelper.GetWeightFactor(aeqp.Target.Preset, factorName);

			if (wf == null || wf.Factor == 0)
				return false;

			return true;
		}

		public static FabLot NeedAllowRunDown_Dummy(AoEquipment aeqp, IList<IHandlingBatch> wips)
		{
			//TODO : Current Wait + PreviousStep Run + Inflow Arrivedin
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			//if (eqp.EqpID == "THCVD400" && eqp.NowDT >= LcdHelper.StringToDateTime("20200211 091146"))
			//	Console.WriteLine("B");

			decimal inflowHours;
			if (CanMakeDummy(eqp, wips, out inflowHours) == false)
				return null;

			var last = eqp.GetLastPlan(); //eqp.LastPlan as FabPlanInfo;
			FabStep lastStep = last.Step as FabStep;
			FabLot dummy = CreateHelper.CreateDispatchDummyLot(lastStep, last);

			if (IsLoadableDummy(eqp, dummy, inflowHours) == false)
				return null;

			var idleTime = eqp.GetIdleRunTime();
			decimal adjustHour = inflowHours - Convert.ToDecimal(idleTime.TotalHours);

			//직전 Step에 Run Wip이 존재하는 경우는 대기함
			bool isExistPrevStepRunWip = InFlowMaster.ExistPrevStepRunWip(eqp, last.ProductID, last.ProductVersion, last.OwnerType, lastStep, adjustHour);
			if (isExistPrevStepRunWip == false)
			{
				//AllowRunDown Factor            
				decimal inflowQty = InFlowMaster.GetAllowRunDownWip(eqp, last.ProductID, last.ProductVersion, last.OwnerType, lastStep, adjustHour);

				if (inflowQty <= 0)
					inflowQty += eqp.GetFilteredWipQty(last);

				if (inflowQty <= 0)
					return null;
			}

			dummy.DispatchInTime = eqp.NowDT;

			return dummy;
		}

		private static bool IsLoadableDummy(FabAoEquipment eqp, FabLot dummy, decimal inflowHours)
		{
			var now = eqp.NowDT;

			var last = eqp.GetLastPlan(); //eqp.LastPlan as FabPlanInfo;
			if (last == null)
				return false;

			//M제약에 의해 진행 중인 last의 Arrange가 없어진 경우
			if (EqpArrangeMaster.IsLoadable(eqp, dummy) == false)
				return false;

			var currEA = dummy.CurrentEqpArrange;
			if (currEA == null || currEA.IsLoableByProductVersion(last.ProductVersion) == false)
				return false;

			string reason = string.Empty;
			if (FilterMaster.IsEqpRecipeTime(eqp, dummy, now.AddHours((double)inflowHours), ref reason) == false)
				return false;

			//Dummy Mask 확인
			if (FilterMaster.IsLoadableToolArrange(eqp, dummy, false) == false)
				return false;

			//MixRun(ParallelChamber)
			if (eqp.IsParallelChamber)
			{
				var subEqp = eqp.TriggerSubEqp;
				if (subEqp != null)
				{
					if (subEqp.IsLoadable(last.FabStep, now) == false)
						return false;
				}
			}

			return true;
		}

		private static bool CanMakeDummy(FabAoEquipment eqp, IList<IHandlingBatch> wips, out decimal inflowHours)
		{
			inflowHours = 0;

			var last = eqp.GetLastPlan(); // eqp.LastPlan as FabPlanInfo;
			if (last == null)
				return false;

			if (IsSomeOneDummyWait(eqp, last))
				return false;

			WeightFactor wf;
			if (TryGetEqpWeightFactor(eqp, Constants.WF_ALLOW_RUN_DOWN_TIME, out wf) == false)
				return false;

			inflowHours = (decimal)wf.Criteria[0];
			if (inflowHours <= 0)
				return false;

			var idleTime = eqp.GetIdleRunTime();

			//AllowRunDown 지나면 기다릴 필요 없음.
			//(IdleRun이 발생시작부터 (jung-임시), Idle 시작부터면 설정된 Inflow 시간 + ProcessTime이됨.
			if (idleTime.TotalHours > Convert.ToDouble(inflowHours))
				return false;

			//EqpRecipe X Type인 경우는 DummyLot을 만들지 않는다.
			if (eqp.IsEqpRecipeRun)
				return false;

			//var stdStep = last.FabStep.StdStep;
			//int count = stdStep.GetWorkingEqpCount(last);
			//if (count > 1)
			//    return null;

			//동일한 Spec Lot이 존재하는 경우 대기 X
			if (wips != null && wips.Count > 0)
			{
				var find = wips.FirstOrDefault(t => eqp.IsLastPlan(t.Sample as FabLot));
				if (find != null)
					return false;
			}

			//Setup이 필요없는 제품 존재하는 경우 대기 X(2019.12.16 - by.liujian(유건))
			if (wips != null && wips.Count > 0)
			{
				var find = wips.FirstOrDefault(t => ResHelper.IsNeedSetup(eqp, t.Sample as FabLot) == false);
				if (find != null)
					return false;
			}

			//CF PHOTO (MAIN/SUB) - remain target						

			return true;
		}

		private static bool IsSomeOneDummyWait(FabAoEquipment eqp, FabPlanInfo last)
		{
			//var list = last.FabStep.StdStep.GetWorkingEqpList(last.ProductID, last.ProductVersion, last.OwnerType, last.OwnerID);
			var list = ResHelper.GetEqpsByDspEqpGroup(eqp.DspEqpGroupID);
			if (list == null)
				return false;

			//var waitEqps = list.FindAll(x => x.IsDummyWait && x.EqpID != eqp.EqpID);

			var waitEqps = new List<FabAoEquipment>();
			foreach (FabAoEquipment item in list)
			{
				if (item.EqpID == eqp.EqpID)
					continue;

				if (item.IsDummyWait == false)
					continue;

				float setupTime = SetupMaster.GetSetupTime(item, last.ShopID, last.StepID, last.ProductID, last.ProductVersion, last.OwnerType, last.OwnerID);
				if (setupTime > 0)
					continue;

				waitEqps.Add(item);
			}

			if (waitEqps.Count > 0)
				return true;

			return false;
		}

		public static bool TryGetMaxRequiredEqp(AoEquipment aeqp, List<JobFilterInfo> joblist, out double maxRequireEqp)
		{
			maxRequireEqp = 0;

			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			//if (eqp.EqpID == "THATS400")// && eqp.NowDT >= LcdHelper.StringToDateTime("20190923181719"))
			//	Console.WriteLine("B");						

			WeightFactor wf;
			if (TryGetEqpWeightFactor(eqp, Constants.WF_REQUIRED_EQP_PRIORITY, out wf) == false)
				return false;

			double inflowHour = (double)wf.Criteria[0];

			foreach (JobFilterInfo info in joblist)
			{
				double workCnt = info.WorkingEqpCnt;
				double needCnt = info.GetNewAssignNeedCnt(eqp, inflowHour, workCnt);

				maxRequireEqp = Math.Max(maxRequireEqp, needCnt);

				info.NewAssignNeedCount = needCnt;
			}

			return true;
		}

		private static double GetRequiredEqpCnt(this JobFilterInfo info, FabAoEquipment eqp, decimal inflowHour)
		{
			decimal inflowQty = InFlowAgent.GetInflowQty(info, eqp, inflowHour, 0);

			decimal avtTact = info.HarmonicAvgTact;

			if (eqp.IsParallelChamber)
				avtTact = info.HarmonicAvgTact / eqp.GetChamberCapacity();

			decimal inflowSec = inflowHour * 3600;
			double requiredEqp = Convert.ToSingle(inflowQty * avtTact / Math.Max(inflowSec, 1));

			if (requiredEqp <= 0)
				requiredEqp = 0;

			return requiredEqp;
		}

		private static double GetNewAssignNeedCnt(double reqCnt, double workCnt)
		{
			//=(REQ - WORK) - MIN(보정계수(=0.1) * WORK), 0.5)
			double a = 0.1d;
			double b = 0.5d;

			double needCnt = (reqCnt - workCnt) - Math.Min((a * workCnt), b);

			//=MAX(needCnt, 0)
			return Math.Max(needCnt, 0);
		}

		internal static double GetNewAssignNeedCnt(this JobFilterInfo info, FabAoEquipment eqp, double inflowHour, double workCnt)
		{
			double reqCnt = info.GetRequiredEqpCnt(eqp, (decimal)inflowHour);
			double needCnt = GetNewAssignNeedCnt(reqCnt, workCnt);

			return needCnt;
		}

		////TODO : 함수 이름 변경 필요
		//internal static LayerStats GetLayerCumWipQtyRatio(AoEquipment aeqp)
		//{
		//	var eqp = aeqp.ToFabAoEquipment();

		//	LayerStats sts = new LayerStats(eqp);

		//	return sts;
		//}

		internal static Dictionary<string, object> SetMinMaxVaule_WF(AoEquipment eqp, List<JobFilterInfo> joblist)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();

			dict[Constants.WF_SETUP_TIME_PRIORITY] = GetMinMax_SetupTime(eqp, joblist);
			dict[Constants.WF_MAX_QTIME_PRIORITY] = GetMinMax_RemainTime(eqp, joblist);

			return dict;
		}

		internal static T GetMinVaule_WF<T>(IDispatchContext ctx, string key, T defaultValue)
		{
			if (key == null)
				return defaultValue;

			var infos = ctx.Get<Dictionary<string, Tuple<object, object>>>(Constants.WF_MINMAX_VALUE_INFOS, null);
			if (infos == null)
				return defaultValue;

			try
			{
				Tuple<object, object> info;
				if (infos.TryGetValue(key, out info))
					return (T)info.Item1;
			}
			catch { }

			return defaultValue;
		}

		internal static T GetMaxVaule_WF<T>(IDispatchContext ctx, string key, T defaultValue)
		{
			if (key == null)
				return defaultValue;

			var infos = ctx.Get<Dictionary<string, Tuple<object, object>>>(Constants.WF_MINMAX_VALUE_INFOS, null);
			if (infos == null)
				return defaultValue;

			try
			{
				Tuple<object, object> info;
				if (infos.TryGetValue(key, out info))
					return (T)info.Item2;
			}
			catch { }

			return defaultValue;
		}

		private static Tuple<object, object> GetMinMax_SetupTime(AoEquipment eqp, List<JobFilterInfo> joblist)
		{
			float minVaule = float.MaxValue;
			float maxVaule = float.MinValue;

			foreach (JobFilterInfo it in joblist)
			{
				var sample = it.Sample;
				if (sample == null)
					continue;

				float setupTime = SetupMaster.GetSetupTime(eqp, sample);

				minVaule = Math.Min(setupTime, minVaule);
				maxVaule = Math.Max(setupTime, maxVaule);
			}
			
			return new Tuple<object, object>(minVaule, maxVaule);
		}

		private static Tuple<object, object> GetMinMax_RemainTime(AoEquipment eqp, List<JobFilterInfo> joblist)
		{
			Time minVaule = Time.MaxValue;
			Time maxVaule = Time.MinValue;

			foreach (JobFilterInfo info in joblist)
			{
				var sample = info.Sample;
				if (sample == null)
					continue;

				if (sample.QtimeInfo == null)
					continue;

				var find = sample.QtimeInfo.FindMinimumRemainTime(eqp.NowDT);
				if (find == null)
					continue;

				var remainTime = find.RemainTime(sample, sample.CurrentFabStep, eqp.NowDT);
				if (remainTime == Time.MaxValue)
					continue;

				minVaule = Time.Min(remainTime, minVaule);
				maxVaule = Time.Max(remainTime, maxVaule);
			}

			return new Tuple<object, object>(minVaule, maxVaule);
		}

		internal static Dictionary<string, WaitingWipInfo> SetWatingWipInfo(AoEquipment eqp, List<JobFilterInfo> joblist)
		{
			WeightFactor wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_ONGOING_PRODUCT_WIP_PRIORITY);

			if (wf == null)
				return null;

			Dictionary<string, WaitingWipInfo> waiting = new Dictionary<string, WaitingWipInfo>();
			foreach (var job in joblist)
			{
				var fps = (job.Step.Process as FabProcess).FirstPhotoStep;
				if (fps == null)
					continue;

				string stepKey = fps.StepKey;

				if (waiting.ContainsKey(stepKey))
					continue;

				WaitingWipInfo info = ResHelper.GetTargetStepWaitingWip(stepKey);
				waiting.Add(stepKey, info);
			}

			return waiting;
		}

		public static void ClearDispatchingInfo(this StepLayerInfo layerStep)
		{
			layerStep.LoadedEqpCnt = 0;
			layerStep.RequiredEqpCnt = 0;
			layerStep.InFlowQty = 0;
		}

		//private static void CalcStepLayerInfo(FabAoEquipment eqp, decimal inflowHour, DateTime now)
		//{
		//	var inflowSteps = InFlowMaster.GetInFlowStepsValues(eqp.EqpID);
		//	var eqps = ResHelper.GetEqpsByDspEqpGroup(eqp.DspEqpGroupID);

		//	foreach (var fsi in inflowSteps)
		//	{
		//		foreach (var step in fsi.Steps)
		//		{
		//			string key = step.GetLayerStepKey();

		//			StepLayerInfo info;
		//			if (!InputMart.Instance.StepLayerGroups.TryGetValue(key, out info))
		//				continue;

		//			if (info.DispatchTime != now)
		//			{
		//				info.DispatchTime = now;
		//				info.ClearDispatchingInfo();
		//			}

		//			//CHECK : jung
		//			decimal qty = InFlowAgent.GetLayerInflowQty(fsi.ProductID, step, eqp, inflowHour);
		//			if (qty > 0)
		//				info.InFlowQty += qty;

		//			int loadedCnt = eqps.Count(x => x.LastStep != null && x.LastStep == step);
		//			if (loadedCnt > 0)
		//				info.LoadedEqpCnt += loadedCnt;
		//		}
		//	}
		//}

		static Dictionary<string, LayerStats> _layerBalanceState = new Dictionary<string, LayerStats>();

		internal static void SetLayerBalance(AoEquipment aeqp, LayerStats sts)
		{
			var eqp = aeqp.ToFabAoEquipment();

			string dspEqpGroupID = eqp.DspEqpGroupID;

			_layerBalanceState[dspEqpGroupID] = sts;
		}

		internal static LayerStats GetLayerBalacne(string eqpGroup)
		{
			LayerStats sts;
			_layerBalanceState.TryGetValue(eqpGroup, out sts);

			return sts;
		}

		internal static LayerStats CalcLayerBalance(AoEquipment aeqp)
		{
			var eqp = aeqp.ToFabAoEquipment();

			LayerStats sts = new LayerStats(eqp);

			SetLayerBalance(aeqp, sts);

			return sts;
		}
	}

	[FeatureBind()]
	public partial class LayerStats
	{
		/// <summary>
		/// Key:StdStepID
		/// </summary>
		Dictionary<string, StepWipStat> dic = new Dictionary<string, StepWipStat>();

		public decimal MaxQty { get; private set; }

		public decimal MinQty { get; private set; }


		public LayerStats(FabAoEquipment eqp)
		{
			var factor = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_LAYER_BALANCE_PRIORITY);
			if (factor == null)
				return;

			string dspEqpGroupID = eqp.DspEqpGroupID;
			List<FabStdStep> steps = SimHelper.GetDspEqpSteps(dspEqpGroupID);

			for (int i = 0; i < steps.Count; i++)
			{
				FabStdStep step = steps[i];

				int qty = InFlowMaster.GetStepWipQtyforLayerBalance(step.BalanceSteps);

				StepWipStat stat;
				if (dic.TryGetValue(step.StepID, out stat) == false)
				{
					stat = new StepWipStat(this, step);
					dic.Add(step.StepID, stat);
				}

				stat.AddQty(qty);
			}


			this.MinQty = decimal.MaxValue;
			this.MaxQty = decimal.MinValue;

			foreach (StepWipStat stat in dic.Values)
			{
				this.MinQty = Math.Min(stat.GapQty, this.MinQty);
				this.MaxQty = Math.Max(stat.GapQty, this.MaxQty);
			}
		}

		/// <param name="step">StdStepID</param>
		public StepWipStat GetWipStat(string step)
		{
			StepWipStat sts;
			dic.TryGetValue(step, out sts);

			return sts;
		}

		internal float GetLayerBalanceScore(LayerStats.StepWipStat wipStat, out string desc)
		{
			float score = 0f;
			desc = string.Empty;

			if (wipStat != null)
			{
				score = 1 - ((float)(wipStat.GapQty - this.MinQty) / (float)(this.MaxQty - this.MinQty));

				if (float.IsInfinity(score) || float.IsNaN(score))
					score = 0f;

				int adv = 0;
				if (wipStat.IsExcess)
				{
					adv = -1;
					score = -1f;
				}
				else
					if (wipStat.IsUnder)
				{
					adv = 2;
					score = score * 2;
				}

				//if (score > 0)
				//	Console.WriteLine();

				desc = string.Format("[{0}, Gap:{1}=Qty:{2}-Base:{3} Min:{4},Max:{5} Adv:{6}]"
					, score.ToRound(2), wipStat.GapQty, wipStat.Qty, wipStat.BaseQty, this.MinQty, this.MaxQty, adv);
			}

			return score;
		}

		public override string ToString()
		{
			return string.Format("Min:{0}/Max:{1} Count:{2}", this.MinQty, this.MaxQty, this.dic.Count);
		}


		public class StepWipStat
		{
			LayerStats parent;

			FabStdStep StdStep;

			public decimal Qty = 0;

			public string ShopID { get { return this.StdStep.ShopID; } }
			public string StepID { get { return this.StdStep.StepID; } }

			public int BaseQty { get { return this.StdStep.BalaceWipQty; } }

			private decimal AllowGapQty { get { return (this.BaseQty * (decimal)StdStep.BalaceGap); } }

			public decimal AllowMaxQty { get { return this.BaseQty + AllowGapQty; } }
			public decimal AllowMinQty { get { return this.BaseQty - AllowGapQty; } }

			public decimal GapQty { get { return (decimal)Qty - this.BaseQty; } }


			public bool IsExcess { get { return this.BaseQty > 0 && this.Qty > AllowMaxQty; } }
			public bool IsUnder { get { return this.BaseQty > 0 && this.Qty < AllowMinQty; } }

			//public Dictionary<string, decimal> ProductInfo = new Dictionary<string, decimal>();

			public StepWipStat(LayerStats parent, FabStdStep stdStep)
			{
				this.parent = parent;
				this.StdStep = stdStep;
			}


			public override string ToString()
			{
				string gubun = "Normal";

				if (GapQty < 0 && IsUnder)
					gubun = "IsUnder";
				else if (GapQty > 0 && IsExcess)
					gubun = "IsExcess";

				return string.Format("{0}_Gap:{1}=BalaceWipQty:{2}-Qty:{3}/{4}", this.StdStep.StepID, this.GapQty, this.BaseQty, this.Qty, gubun);
			}


			//private void AddProdInfo(string p, decimal nextQty)
			//{
			//    if (this.ProductInfo.ContainsKey(p) == false)
			//        this.ProductInfo.Add(p, 0);

			//    ProductInfo[p] += nextQty;
			//}

			internal void AddQty(int qty)
			{
				this.Qty += qty;
			}
		}
	}
}
