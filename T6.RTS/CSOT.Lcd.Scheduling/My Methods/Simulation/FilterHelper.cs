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
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class FilterHelper
	{
		public static void BuildJobFilterInfo(AoEquipment eqp, IList<IHandlingBatch> wips, IDispatchContext ctx)
		{
			if (eqp.Dispatcher is FifoDispatcher)
				return;

			List<JobFilterInfo> joblist = FilterHelper.CreateJobList(eqp, wips);
			ctx.Set(Constants.JobGroup, joblist);
		}

		public static List<JobFilterInfo> CreateJobList(AoEquipment aeqp, IList<IHandlingBatch> list)
		{
			Dictionary<string, JobFilterInfo> joblist
			= new Dictionary<string, JobFilterInfo>();

			FabAoEquipment eqp = aeqp as FabAoEquipment;
			var last = eqp.GetLastPlan();
			if (last != null)
			{
				FabPlanInfo plan = last;

				string shopID = plan.Step.StepID;
				string stepID = plan.StepID;
				string productID = plan.ProductID;
				string prodVer = plan.ProductVersion;
				string ownerType = plan.OwnerType;
				string ownerID = plan.OwnerID;

				string key = GetJobFilterKey(shopID, stepID, productID, prodVer, ownerType);

				JobFilterInfo info = CreateHelper.CreateDispatchFilterInfo(plan.FabStep, productID, prodVer, ownerType, ownerID);
				info.IsEmpty = true;

				joblist.Add(key, info);
				eqp.LastPlanFilterInfo = info;
			}

			foreach (IHandlingBatch hb in list)
			{
				FabLot lot = hb.ToFabLot();

				lot.DispatchFilterInfo = null;

				string productID = lot.CurrentProductID;
				string prodVer = lot.CurrentProductVersion;
				string ownerType = lot.CurrentFabPlan.OwnerType;
				string ownerID = lot.OwnerID;

				FabStep step = lot.CurrentFabStep;

				string key = GetJobFilterKey(step.ShopID, step.StepID, productID, prodVer, ownerType);

				JobFilterInfo info;
				if (joblist.TryGetValue(key, out info) == false)
					joblist.Add(key, info = CreateHelper.CreateDispatchFilterInfo(step, productID, prodVer, ownerType, ownerID));			

				if (hb.HasContents)
				{
					foreach (FabLot item in hb.Contents)
					{
						info.LotList.Add(item);
						info.WaitSum += item.UnitQty;

						item.DispatchFilterInfo = info;
						item.CurrentFabPlan.LotFilterInfo.Clear();
					}
				}
				else
				{
					info.LotList.Add(lot);
					info.WaitSum += lot.UnitQty;

					lot.DispatchFilterInfo = info;
					lot.CurrentFabPlan.LotFilterInfo.Clear();
				}				
							   
				info.IsEmpty = false;
			}

			foreach (var info in joblist.Values)
			{
				info.WorkingEqpCnt = info.Step.StdStep.GetWorkingEqpCount(info, true, false);
				
				info.ExistInflowWip = info.WaitSum > 0;
				if (info.ExistInflowWip == false)
				{
					if (ExistInflowWip(eqp, info))
						info.ExistInflowWip = true;
				}
			}

			return joblist.Values.ToList();
		}
		
		private static bool ExistInflowWip(FabAoEquipment eqp, JobFilterInfo info)
		{
			var wf = WeightHelper.GetWeightFactor(eqp.Target.Preset, Constants.WF_ALLOW_RUN_DOWN_TIME);

			if (wf == null || wf.Factor == 0)
				return false;

			decimal inflowHour = (decimal)wf.Criteria[0];
			if (inflowHour <= 0)
				return false;
							
			var idleTime = eqp.GetIdleRunTime();
			decimal adjustHour = inflowHour - Convert.ToDecimal(idleTime.TotalHours);

			if (adjustHour <= 0)
				return false;

			var inflowQty = InFlowMaster.GetAllowRunDownWip(eqp, info.ProductID, info.ProductVersion, info.OwnerType, info.Step, adjustHour);
			return inflowQty > 0;
		}

		public static string GetJobFilterKey(JobFilterInfo info)
		{
			return ResHelper.CreateKeyForJobFilter(info.Step.ShopID, info.Step.StepID, info.ProductID, info.ProductVersion, info.OwnerType);
		}

		public static string GetJobFilterKey(string shop, string stepID, string productID, string prodVer, string ownerType)
		{
			return ResHelper.CreateKeyForJobFilter(shop, stepID, productID, prodVer, ownerType);
		}

		public static bool CheckIsRunning(this FabAoEquipment eqp, JobFilterInfo info)
		{
			if (eqp.Loader.IsBlocked())
				return false;

			string shopID = eqp.TargetEqp.ShopID;
			string stepID = info.Step.StepID;
			string productID = info.ProductID;
			string productVer = info.ProductVersion;
			string ownerType = info.OwnerType;
			string ownerID = info.OwnerID;

			if (eqp.IsLastPlan(shopID, stepID, productID, productVer, ownerType, ownerID))
				return true;

			return false;
		}

		public static List<FabAoEquipment> InitJobFilterInfo(this JobFilterInfo info, FabAoEquipment eqp, Dictionary<string, List<FabAoEquipment>> workingEqps)
		{
			info.IsRunning = eqp.CheckIsRunning(info);

			var key = FilterHelper.GetJobFilterKey(info);

			List<FabAoEquipment> eqpList;
			workingEqps.TryGetValue(key, out eqpList);

			if (eqpList == null || eqpList.Count == 0)
				info.IsNoAssign = true;

			info.SetHarmonicTactInfo(eqp, eqpList);
			info.SetSetupTime(eqp);

			return eqpList;
		}
		
		public static void SetHarmonicTactInfo(this JobFilterInfo info, AoEquipment aeqp, List<FabAoEquipment> eqplist)
		{
			info.HarmonicTact = GetHarmonicTact(aeqp, eqplist, info);
			info.HarmonicAvgTact = info.HarmonicTact * (eqplist == null ? 1 : eqplist.Count);
		}

		public static void SetSetupTime(this JobFilterInfo info, AoEquipment aeqp)
		{
			FabLot lot = info.Sample;
			if (lot == null)
				return;

			info.SetupTime = SetupMaster.GetSetupTime(aeqp, lot);

			var eqp = aeqp.ToFabAoEquipment();
		}
		
		private static decimal GetHarmonicTact(AoEquipment aeqp, List<FabAoEquipment> eqplist, JobFilterInfo info)
		{
			List<FabAoEquipment> assigned = new List<FabAoEquipment>();

			if (eqplist != null)
				assigned.AddRange(eqplist);

			//신규 추가인 경우 해당 설비를 포함하여 TactTime 계산
			if (info.IsRunning == false)
				assigned.Add(aeqp as FabAoEquipment);

			decimal harmonicTact = TimeHelper.GetHarmonicTactTime(info.Step, assigned, info.ProductID);

			return harmonicTact;
		}

		//private static void RevaluateJobInfo(AoEquipment aeqp, List<JobFilterInfo> joblist)
		//{

		//    foreach (JobFilterInfo info in joblist)
		//    {
		//        bool isRunning = CheckIsRunning(aeqp, info);
		//        info.IsRunning = isRunning;

		//    }
		//}
	}
}
