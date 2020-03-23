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
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
	[FeatureBind()]
	public partial class Weights
	{
		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue ASSIGN_STEP_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabEqp targetEqp = eqp.TargetEqp;
			FabLot lot = entity as FabLot;

			float score = 0f;

			bool isLastStep = false;
			var last = eqp.GetLastPlan();
			if (last != null)
			{
				if (last.StepID == lot.CurrentStepID)
					isLastStep = true;
			}

			if (isLastStep || targetEqp.MainRunSteps.Contains(lot.CurrentFabStep.StdStep))
				score = 1f;

			string lastStepID = last == null ? "-" : last.StepID;
			string desc = string.Format("[Last : {0}, MainSteps：{1}]",
										lastStepID, targetEqp.EqpMainRunsSteps);

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue LAYER_BALANCE_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			LayerStats sts = ctx.Get(Constants.WF_LAYER_BALANCE_PRIORITY, default(LayerStats));

			LayerStats.StepWipStat wipStat = sts.GetWipStat(lot.CurrentStep.StdStepID);

			string desc = string.Empty;
			float score = 0f;

			if (wipStat != null)
				score = sts.GetLayerBalanceScore(wipStat, out desc);

			return new WeightValue(score * factor.Factor, desc);
		}


		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue LAYER_BALANCE_FOR_PHOTO(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;

			int checkStepCount = (int)factor.Criteria[0];
			if (checkStepCount == 0)
				checkStepCount = 10;

			var currentStep = lot.CurrentFabStep;
			var currentProd = lot.FabProduct;

			FabStep nextPhoto = currentStep.GetNextPhotoNearByMe(currentProd, checkStepCount, out _);

			LayerStats sts = WeightHelper.GetLayerBalacne(nextPhoto.StdStep.DspEqpGroup);

			if (sts == null)
			{
				AoEquipment aeqp = null;
				foreach (var eqp in nextPhoto.StdStep.AllEqps)
				{
					aeqp = AoFactory.Current.GetEquipment(eqp.EqpID);
					if (aeqp != null)
						break;
				}

				if (aeqp == null)
					return new WeightValue(0);

				sts = WeightHelper.CalcLayerBalance(aeqp);
			}


			LayerStats.StepWipStat wipStat = sts.GetWipStat(nextPhoto.StdStepID);

			string desc = string.Empty;
			float score = 0f;

			if (wipStat != null)
				score = sts.GetLayerBalanceScore(wipStat, out desc);


			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue LOT_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			if (lot.IsDummy)
				return new WeightValue(0);

			//if (eqp.EqpID == "THOVN300" && eqp.NowDT >= LcdHelper.StringToDateTime("20200113 090018"))
			//	Console.WriteLine("B");

			bool isLastPlan = eqp.IsLastPlan(lot);

			float maxPriority = ctx.Get<int>(Constants.WF_LOT_PRIORITY, SiteConfigHelper.GetDefaultLotPriority());
			int lotPriority = lot.Priority;

			float score = 0f;

			int workingCnt = lot.DispatchFilterInfo.WorkingEqpCnt;
			if (isLastPlan == false && workingCnt > 0)
			{
				score = 0f;
			}
			else
			{
				if (maxPriority > 0)
					score = 1 - (lotPriority / maxPriority);

				//working : score * 2
				if (isLastPlan)
					score = score * 2;
			}

			string desc = string.Format("[{0}／Max{1}]", lotPriority, maxPriority);

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue MASK_MOVE_PREVENT_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;
			var eqp = target as FabAoEquipment;

			if (SimHelper.IsMaskConst(lot) == false)
				return new WeightValue(0);

			var currentMask = eqp.InUseMask;

			if (currentMask == null)
				return new WeightValue(0);

			float score = 0f;

			if (currentMask == lot.CurrentMask)
				score = 1f;

			return new WeightValue(score * factor.Factor);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue MAX_MOVE_LIMIT_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			var eqp = target as FabAoEquipment;

			var last = eqp.GetLastPlan(); //eqp.LastPlan;
			if (last == null)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;

			decimal limitQty = (decimal)factor.Criteria[0];

			if (limitQty == decimal.MaxValue)
				return new WeightValue(0);

			bool isNeedSetup = ResHelper.IsNeedSetup(eqp, lot);
			if (eqp.ContinuousQty > limitQty)
			{
				if (isNeedSetup)
					return new WeightValue(factor.Factor);
			}

			string desc = string.Format("[LIMIT：{0}, Cnt：{1}, SETUP：{2}]", limitQty, eqp.ContinuousQty, isNeedSetup);

			return new WeightValue(0, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue MIN_MOVEQTY_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			var last = eqp.GetLastPlan(); //eqp.LastPlan;

			float score = 0f;

			if (last == null)
				return new WeightValue(0, string.Format("[LastPlan NULL, CONT:{0}]", eqp.ContinuousQty));

			int minMoveQty = (int)factor.Criteria[0];
			if (minMoveQty == 0)
				return new WeightValue(0, string.Format("[MinMove Not Setting, CONT:{0}]", eqp.ContinuousQty));

			FabLot lot = entity as FabLot;

			bool isLastPlan = eqp.IsLastPlan(lot);

			if (isLastPlan && eqp.ContinuousQty < minMoveQty)
				score = 1f;

			string desc = string.Format("[MIN:{0}, CONT:{1}, IsLast:{2}]", minMoveQty, eqp.ContinuousQty, isLastPlan);

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue PREVENT_LAYER_CHANGE_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			float score = 0f;

			string currentStepID = lot.CurrentStepID;

			//연속진행
			bool isLastPlan = eqp.IsLastPlan(lot);
			if (isLastPlan)
			{
				score = 1;
			}
			else
			{
				var targetEqp = eqp.TargetEqp;
				if (targetEqp.IsMainRunStep(currentStepID))
				{
					score = 1;
				}
				else
				{
					//타설비에서 진행 중이면 Step이면 0, 아니면 1
					bool isExistRunStep;
					if (ExistRunEqpByDspEqpGroup(eqp, lot, out isExistRunStep))
						score = -1;
					else if (isExistRunStep)
						score = 0;
					else
						score = 1;
				}
			}

			string desc = null;
			return new WeightValue(score * factor.Factor, desc);
		}

		private bool ExistRunEqpByDspEqpGroup(FabAoEquipment baseEqp, FabLot lot, out bool isExistRunStep)
		{
			isExistRunStep = false;

			if (lot == null)
				return false;

			string dspEqpGroupID = baseEqp.DspEqpGroupID;

			var eqpList = ResHelper.GetEqpsByDspEqpGroup(dspEqpGroupID);
			if (eqpList == null || eqpList.Count == 0)
				return false;

			string currentStepID = lot.CurrentStepID;
			foreach (var eqp in eqpList)
			{
				if (eqp == baseEqp)
					continue;

				var last = eqp.GetLastPlan();
				if (last == null)
					continue;

				if (eqp.IsLastPlan(lot))
				{
					isExistRunStep = true;
					return true;
				}
				else if (last.StepID == currentStepID)
				{
					isExistRunStep = true;
				}
			}

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue REQUIRED_EQP_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;
			FabAoEquipment eqp = target as FabAoEquipment;

			//if (eqp.EqpID == "THATS400" && (lot.LotID == "TH9C1582N07" || lot.LotID == "TH9C1642N0F"))
			//	Console.WriteLine("B");

			double maxEqpCnt = ctx.Get(Constants.WF_MAX_REQUIRED_EQP_COUNT, 0d);
			double needEqpCnt = lot.DispatchFilterInfo.NewAssignNeedCount;
			int workingCnt = lot.DispatchFilterInfo.WorkingEqpCnt;

			float score = 0f;

			if (maxEqpCnt > 0)
				score = (float)(needEqpCnt / maxEqpCnt);

			bool isContinue = eqp.IsLastPlan(lot);

			if (isContinue && needEqpCnt > 0)
				score = 1f;
			else if (needEqpCnt <= 0)
				score = -1f;

			string desc = string.Format("[{0}=REQ:{1}／MAX:{2},WORK:{3}]",
										Math.Round(score, 2),
										Math.Round(needEqpCnt, 2),
										Math.Round(maxEqpCnt, 2),
										workingCnt);

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue SETUP_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			float score = 1f;

			FabAoEquipment aeqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			float setupTime = SetupMaster.GetSetupTime(aeqp, lot);
			if (setupTime > 0)
				score = 0;

			return new WeightValue(score * factor.Factor);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue SETUP_TIME_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			float score = 1;
			int criteria0 = 1;

			if (factor.Criteria != null && factor.Criteria.Length > 0)
				criteria0 = Math.Max((int)factor.Criteria[0], 1);

			int cutMinutes = (int)Time.FromMinutes(criteria0).TotalMinutes;

			FabAoEquipment aeqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			//if (aeqp.EqpID == "THCVD300" && aeqp.NowDT >= LcdHelper.StringToDateTime("20200108 123433"))
			//	Console.WriteLine("B");

			float setupTime = SetupMaster.GetSetupTime(aeqp, lot);
			float maxSetupTime = WeightHelper.GetMaxVaule_WF(ctx, Constants.WF_SETUP_TIME_PRIORITY, 0f);

			if (setupTime > 0 && maxSetupTime > 0)
			{
				float s = (int)(setupTime / cutMinutes);
				float m = (int)(maxSetupTime / cutMinutes);
				float r = m == 0 ? 0 : (float)Math.Round(s / m, 3);

				score = 1 - r;
			}

			string desc = string.Format("[{0} = SETUP：{1}m／MAX：{2}m]", score, setupTime, maxSetupTime);

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue SMALL_BATCH_MERGE_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			int baseQty = (int)factor.Criteria[0];
			int checkStepCount = (int)factor.Criteria[1];

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			//if (eqp.IsLastPlan(lot.CurrentShopID, lot.CurrentStepID, lot.CurrentProductID, Constants.NULL_ID, lot.OwnerType, lot.OwnerID))
			//	return new WeightValue(1 * factor.Factor, string.Format("[IsLast：Y]"));

			FabStep currentStep = lot.CurrentFabStep;
			var currentProd = lot.FabProduct;

			FabStep prevStep = currentStep.GetPrevMainStep(lot.FabProduct, true);

			StepTat tat = null;
			if (prevStep != null)
				tat = prevStep.GetTat(lot.CurrentProductID, true);

			if (tat == null)
				tat = currentStep.GetDefaultTAT();


			var job = InFlowMaster.GetJobState(lot);

			if (job == null)
				return new WeightValue(0);


			int waitQty = job.GetCurrenStepWaitWipQty(eqp, currentStep, Constants.NULL_ID, (decimal)TimeSpan.FromMinutes(tat.RunTat).TotalHours);
			if (waitQty > baseQty)
				return new WeightValue(0, string.Format("[Wait：{0}＞{1}：Base]", waitQty, baseQty));

			int prevWipQty = job.GetPrevStepRunWipQty(eqp, currentStep, Constants.NULL_ID, now.AddMinutes(tat.TAT));

			if (prevWipQty > 0)
				return new WeightValue(0, string.Format("[PrevRunWip：{0}＞0", prevWipQty));

			int nextWipQty = 0;

			var nextStepList = currentStep.GetNextStepList(currentProd, checkStepCount);
			foreach (FabStep next in nextStepList)
			{
				nextWipQty += job.GetStepWips(next, WipType.Total);
			}

			int runQty = job.GetStepWips(currentStep, WipType.Run);
			if (runQty + nextWipQty == 0)
				return new WeightValue(0, string.Format("[Run：{0} + NextQty：{1} = 0]", runQty, nextWipQty));

			return new WeightValue(1 * factor.Factor, string.Format("[Run：{0} + NextQty：{1}＞0]", runQty, nextWipQty));
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue STEP_TARGET_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;

			if (lot.LotID == "TH011797N01")
				Console.WriteLine();

			if (lot.CurrentFabPlan.IsPegged == false)
				return new WeightValue(0);

			int precedeDay = (int)factor.Criteria[0];
			int delayDay = (int)factor.Criteria[1];

			double duration = precedeDay + delayDay;
			if (duration <= 0)
				return new WeightValue(0);

			DateTime targetdate = lot.CurrentFabPlan.MainTarget.DueDate;
			DateTime targetday = ShopCalendar.SplitDate(targetdate);
			DateTime nowday = ShopCalendar.SplitDate(now);

			TimeSpan gap = nowday - targetday;

			//shift zero point (zero point로 변경하여 로직 처리)
			double gapDays = gap.TotalDays + precedeDay;

			float score = 0f;

			if (gapDays >= 0)
			{
				double ratio = gapDays / duration;
				score = (float)Math.Max(ratio, 1f);
			}

			string desc = string.Format("[GAP：{0}]", Math.Round(gap.TotalDays, 2));

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue LAST_RUN(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			FabAoEquipment eqp = target as FabAoEquipment;

			var last = eqp.GetLastPlan(); //eqp.LastPlan;
			if (last == null)
				return new WeightValue(0);

			FabPlanInfo plan = last as FabPlanInfo;

			FabLot lot = entity as FabLot;

			bool isNeedSetup = eqp.IsNeedSetup(lot);

			float markValue = 0;
			if (isNeedSetup == false)
			{
				markValue = 1;

				if (lot.CurrentFabPlan.OwnerType != plan.OwnerType)
					markValue = 0.5f;
			}

			return new WeightValue(markValue * factor.Factor);
		}

		public WeightValue ALLOW_RUN_DOWN_TIME(ISimEntity entity, DateTime now, ActiveObject target, WeightFactor factor, IDispatchContext ctx)
		{
			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			var wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_ALLOW_RUN_DOWN_TIME);

			if (wf == null || wf.Factor == 0)
				return new WeightValue(0);

			decimal inflowHour = (decimal)wf.Criteria[0];

			var idleTime = eqp.GetIdleRunTime();
			decimal adjustHour = inflowHour - Convert.ToDecimal(idleTime.TotalHours);

			if (adjustHour < 0)
				return new WeightValue(0);

			var inflowQty = InFlowMaster.GetAllowRunDownWip(eqp, lot.CurrentProductID, lot.OrigProductVersion, lot.OwnerType, lot.CurrentStep as FabStep, adjustHour);

			float score = 0f;

			if (inflowQty > 0)
				score = 1f;

			string desc = string.Format("[inflow：{0}]", inflowQty);

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue MAX_QTIME_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;
			var currStep = lot.CurrentFabStep;

			//FabAoEquipment eqp = target as FabAoEquipment;
			//if (lot.LotID == "TH9C1396N0X" && eqp.EqpID == "THOVN200") //&& eqp.NowDT >= LcdHelper.StringToDateTime("20191128 133538"))
			//	Console.WriteLine("B");

			//if (lot.LotID == "TH9C1472N0L" && eqp.EqpID == "THOVN200") //&& eqp.NowDT >= LcdHelper.StringToDateTime("20191128 133538"))
			//	Console.WriteLine("B");

			var info = lot.QtimeInfo;
			if (info == null || info.HasMaxQTime() == false)
				return new WeightValue(0);

			var find = info.FindMinimumRemainTime(now);
			if (find == null)
				return new WeightValue(0);

			int criteria0 = Math.Max(WeightHelper.GetCriteria(factor, 0, 1), 1);
			int criteria1 = Math.Max(WeightHelper.GetCriteria(factor, 1, 0), 0);
			int criteria2 = Math.Max(WeightHelper.GetCriteria(factor, 2, 0), 0);

			int cutMinutes = (int)Time.FromMinutes(criteria0).TotalMinutes;

			Time baseSafe = Time.FromMinutes(criteria1);
			Time stepSafe = Time.FromMinutes(criteria2);

			int remainStepCount = find.RemainStepCount(currStep);
			Time safeTime = baseSafe + (stepSafe * remainStepCount);

			Time remainTime = find.RemainTime(lot, currStep, now);
			Time maxRemainTime = WeightHelper.GetMaxVaule_WF(ctx, Constants.WF_MAX_QTIME_PRIORITY, Time.Zero);

			float score = 0;

			if (remainTime < safeTime)
			{
				if (remainTime > 0 && maxRemainTime > 0)
				{
					float s = (int)(remainTime.TotalMinutes / cutMinutes);
					float m = (int)(maxRemainTime.TotalMinutes / cutMinutes);
					float r = m == 0 ? 0 : (float)Math.Round(s / m, 3);

					score = 1 - r;
				}
			}

			string desc = string.Format("[{0} = SAFE：{1}min, REMAIN：{2}min, MAX：{3}min])",
							Math.Round(score, 2),
							Math.Round(safeTime.TotalMinutes, 2),
							Math.Round(remainTime.TotalMinutes, 2),
							Math.Round(maxRemainTime.TotalMinutes, 2));

			return new WeightValue(score * factor.Factor, desc);
		}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <param name="now"/>
		/// <param name="target"/>
		/// <param name="factor"/>
		/// <param name="ctx"/>
		/// <returns/>
		public WeightValue NEXT_STEP_CONTINUOUS_PRODUCTION_PRIORITY(Mozart.Simulation.Engine.ISimEntity entity, DateTime now, Mozart.Simulation.Engine.ActiveObject target, Mozart.SeePlan.DataModel.WeightFactor factor, Mozart.SeePlan.Simulation.IDispatchContext ctx)
		{
			FabAoEquipment eqp = target as FabAoEquipment;

			WeightFactor wf;
			WeightHelper.TryGetEqpWeightFactor(eqp, Constants.WF_NEXT_STEP_CONTINUOUS_PRODUCTION_PRIORITY, out wf);

			FabLot lot = entity as FabLot;

			string desc = string.Empty;
			float score = 0;
			int checkStepCount = (int)wf.Criteria[0];
			int minLimitQty = (int)wf.Criteria[1];
			int maxLimitQty = (int)wf.Criteria[2];

			var currentStep = lot.CurrentFabStep;
			var currentProd = lot.FabProduct;

			FabStep nextPhotoStep = currentStep.GetNextPhotoNearByMe(currentProd, checkStepCount, out int idx);

			if (nextPhotoStep == null)
			{
				score = 0f;
			}
			else
			{
				var job = InFlowMaster.GetJobState(lot);

				if (job == null)
					return new WeightValue(0);

				int nextPhlWipQty = job.GetStepWips(nextPhotoStep, WipType.Total);

				if (nextPhlWipQty <= maxLimitQty && nextPhlWipQty > minLimitQty)
				{
					bool checkProductVersion = false;

					var workingEqps = currentStep.StdStep.GetWorkingEqpList(lot, checkProductVersion);
					int workingCnt = workingEqps == null ? 0 : workingEqps.Count;
					Decimal curStepTactTime = TimeHelper.GetAvgTactTimeForEqps(currentStep, currentProd, workingEqps);

					var targetWorkingEqps = nextPhotoStep.StdStep.GetWorkingEqpList(lot, checkProductVersion);
					int targetWorkingCnt = targetWorkingEqps == null ? 0 : targetWorkingEqps.Count;
					Decimal nextPhotoStepTactTime = TimeHelper.GetAvgTactTimeForEqps(nextPhotoStep, currentProd, targetWorkingEqps);

					if ((workingCnt / curStepTactTime) < (targetWorkingCnt / nextPhotoStepTactTime))
						score = 1f;

					desc = string.Format("[Working：{0}, Next_Photo：{1}]", workingCnt, targetWorkingCnt);

				}
				else if (nextPhlWipQty <= minLimitQty)
				{
					float s = idx;
					float m = checkStepCount;
					float r = m == 0 ? 0 : (float)Math.Round(s / m, 3);

					score = 1 - r;
					int adv = 2;
					score *= adv;

					desc = string.Format("[NextPhotoStepWipQty：{0}, Adv：{1}]", nextPhlWipQty, adv);
				}
			}

			return new WeightValue(score * wf.Factor, desc);
		}


		public WeightValue OWNER_TYPE_PRIORITY(ISimEntity entity, DateTime now, ActiveObject target, WeightFactor factor, IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabLot lot = entity as FabLot;

			float score = 0f;
			string desc = string.Empty;
			string ownerType = lot.OwnerType;


			if (factor.Criteria != null && factor.Criteria.Length > 0)
			{
				string[] types = (string[])factor.Criteria;

				if (types.Length > 0)
				{
					if (ownerType == types[0])
						score = 1f;
				}

				if (types.Length > 1)
				{
					if (ownerType == types[1])
						score = 0.5f;
				}

				if (types.Length > 2)
				{
					if (ownerType == types[2])
						score = 0f;
				}
			}

			return new WeightValue(score * factor.Factor, desc);
		}

		public WeightValue NEXT_STEP_RUN_PRIORITY(ISimEntity entity, DateTime now, ActiveObject target, WeightFactor factor, IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			float criteria0 = WeightHelper.GetCriteria(factor, 0, 0.5f);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			FabStep nextStep = BopHelper.GetNextMandatoryStep(lot);

			float score = 0f;
			int workingCnt = 0;
			int adv = 0;

			if (nextStep != null && nextStep.IsMandatoryStep)
			{
				bool checkProductVersion = true;
				var workingEqps = nextStep.StdStep.GetWorkingEqpList(lot, checkProductVersion);

				//checkProductVersion = false
				if (workingEqps == null || workingEqps.Count == 0)
				{
					checkProductVersion = false;
					workingEqps = nextStep.StdStep.GetWorkingEqpList(lot, checkProductVersion);
				}

				workingCnt = workingEqps == null ? 0 : workingEqps.Count;

				if (workingCnt > 0)
					score = checkProductVersion ? 1f : criteria0;

				var hasDummy = workingEqps.Find(t => t.IsDummyWait) != null;
				if (hasDummy)
				{
					adv = 2;
					score *= adv;
				}
			}

			string nextStepID = nextStep != null ? nextStep.StepID : Constants.NULL_ID;
			string desc = string.Format("[Next：{0}, Working：{1}, Adv：{2}]", nextStepID, workingCnt, adv);

			return new WeightValue(score * factor.Factor, desc);
		}

		public WeightValue CU_DENSITY_3400(ISimEntity entity, DateTime now, ActiveObject target, WeightFactor factor, IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;
			FabStep step = lot.CurrentFabStep;

			string targetStep = "3400";

			int limitDensity = (int)factor.Criteria[0];
			if (limitDensity == 0)
				return new WeightValue(0);

			float currDensity = eqp.AcidDensity == null ? 0 : eqp.AcidDensity.CurrentAcid;
			string desc = string.Format("[Density：{0}, Limit：{1}]", currDensity, limitDensity);

			float score = 0f;
			if (step.StepID == targetStep && currDensity > limitDensity)
				score = 1f;

			return new WeightValue(score * factor.Factor, desc);
		}

		public WeightValue CU_DENSITY_3402(ISimEntity entity, DateTime now, ActiveObject target, WeightFactor factor, IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;
			FabStep step = lot.CurrentFabStep;

			string targetStep = "3402";

			int limitDensity = (int)factor.Criteria[0];
			if (limitDensity == 0)
				return new WeightValue(0);

			float currDensity = eqp.AcidDensity == null ? 0 : eqp.AcidDensity.CurrentAcid;
			string desc = string.Format("[Density：{0}, Limit：{1}]", currDensity, limitDensity);

			float score = 0f;
			if (step.StepID == targetStep && currDensity < limitDensity)
				score = 1f;

			return new WeightValue(score * factor.Factor, desc);
		}

		public WeightValue SMALL_LOT(ISimEntity entity, DateTime now, ActiveObject target, WeightFactor factor, IDispatchContext ctx)
		{
			if (factor.Factor == 0)
				return new WeightValue(0);

			FabAoEquipment eqp = target as FabAoEquipment;
			FabLot lot = entity as FabLot;

			int smallSize = (int)factor.Criteria[0];
			if (smallSize == 0)
				return new WeightValue(0);

			int stepCount = (int)factor.Criteria[1];
			if (stepCount == 0)
				return new WeightValue(0);

			var job = InFlowMaster.GetJobState(lot);
			if (job == null)
				return new WeightValue(0);

			int currentUnitQty = lot.UnitQty;
			string shopID = lot.CurrentShopID;
			string productID = lot.CurrentProductID;
			string productVer = lot.CurrentProductVersion;

			bool isLastPlan = ResHelper.IsLastPlan(eqp, lot);

			float score = 0f;
			if (isLastPlan)
				score = 1f;

			FabStep step = lot.CurrentFabStep;
			string stepType = step.StepType;

			int cnt = 0;
			int runQty = 0;
			int waitQty = 0;
			int total = 0;
			while (cnt < stepCount)
			{
				List<FabStep> preSteps = step.GetPrevSteps(productID);

				List<FabLot> runWips = job.GetPrevStepWipList(step, WipType.Run, productVer);
				List<FabLot> waitWips = job.GetPrevStepWipList(step, WipType.Wait, productVer);

				if (runWips.Count <= 0 && waitWips.Count <= 0)
				{
					cnt++;
					continue;
				}

				int prevRunQty = runWips.Sum(x => x.UnitQty);
				int preWaitQty = waitWips.Sum(x => x.UnitQty);

				runQty += prevRunQty;
				waitQty += preWaitQty;
				total += runQty + waitQty;

				foreach (FabStep prevStep in preSteps)
				{

					if (prevStep.StepType == "MAIN")
						step = prevStep;

					if (step == null)
						continue;
				}

				cnt++;
			}

			int compareQty = currentUnitQty + total;

			string desc = string.Format("[SmallSize：{0}, CompareQty：{1}, IsLast：{2}]", smallSize, compareQty, isLastPlan);

			if (compareQty > smallSize)
				score = 1f;

			return new WeightValue(score * factor.Factor, desc);
		}
	}
}
