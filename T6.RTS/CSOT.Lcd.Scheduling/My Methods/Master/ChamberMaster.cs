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
	public static partial class ChamberMaster
	{
		#region Equpment Initialize

		//Chamber생성
		internal static void InitializeParallelChamber(AoEquipment aeqp)
		{
			var eqp = aeqp.ToFabAoEquipment();

			if (eqp.IsParallelChamber == false)
				return;

			FabEqp taretEqp = aeqp.Target as FabEqp;

			AoChamberProc2 proc = eqp.FirstProcess<AoChamberProc2>();
			var chambers = proc.Chambers;
			if (chambers != null)
			{
				int count = chambers.Length;
				eqp.SubEqps = new FabSubEqp[count];

				for (int i = 0; i < count; i++)
				{
					var chamberInfo = chambers[i];
					string subEqpID = chamberInfo.Label;

					var subEqp = taretEqp.GetSubEqp(subEqpID) as FabSubEqp;
					subEqp.ChamberInfo = chamberInfo;

					eqp.SubEqps[i] = subEqp;

					subEqp.LastPlan = SimHelper.CreateInitLastPlan(subEqp.StatusInfo);
					subEqp.LastIdleRunStartTime = aeqp.NowDT;
					subEqp.LastIdleStartTime = aeqp.NowDT;
				}
			}
		}

		#endregion

		#region Equipment Module

		//internal static List<FabSubEqp> GetLoadableSubEqps(this FabAoEquipment eqp, FabLot lot, bool needLoadableUnits = true)
		//{
		//	DateTime now = eqp.NowDT;

		//	List<FabSubEqp> loadableList = new List<FabSubEqp>();
		//	List<SubEqpGroup> groupList = new List<SubEqpGroup>();


		//	var subEqpList = eqp.SubEqps;
		//	foreach (var subEqp in subEqpList)
		//	{
		//		//check eqpArrange (subEqp)
		//		if (EqpArrangeMaster.IsLoadable_ParallelChamber(subEqp, lot) == false)
		//			continue;

		//		//check state
		//		if (subEqp.IsLoadable(lot.CurrentFabStep, now) == false)
		//			continue;

		//		if (groupList.Contains(subEqp.SubEqpGroup) == false)
		//			groupList.Add(subEqp.SubEqpGroup);

		//		if (loadableList.Contains(subEqp) == false)
		//			loadableList.Add(subEqp);
		//	}

		//	//실제 로딩가능한 개별챔버Unit만 (needLoadableUnits)
		//	if (needLoadableUnits && groupList.Count <= 1)
		//		return loadableList;

		//	if (groupList.Count == 0)
		//		return new List<FabSubEqp>();

		//	//로딩가능한 챔버 그룹의 전체Unit 반환
		//	SubEqpGroup selectGroup = null;
		//	if (groupList.Count >= 2)
		//	{
		//		loadableList.Sort(new CompareHelper.SubEqpLoadableUnitComparer(lot));
		//		selectGroup = loadableList[0].SubEqpGroup;
		//	}
		//	else
		//	{
		//		selectGroup = groupList[0];
		//	}

		//	var list = selectGroup.SubEqps.Values.ToList();

		//	return list;
		//}

		//internal static string[] GetLoadableChambers(AoEquipment aeqp, FabLot lot)
		//{
		//	HashSet<string> list = new HashSet<string>();

		//	var eqp = aeqp.ToFabAoEquipment();
		//	var loadableList = GetLoadableSubEqps(eqp, lot);

		//	if (loadableList != null)
		//	{
		//		foreach (var subEqp in loadableList)
		//			list.Add(subEqp.SubEqpID);
		//	}

		//	return list.ToArray();
		//}

		internal static List<FabSubEqp> GetLoadableSubEqps(this FabAoEquipment eqp, FabLot lot)
		{
			var now = eqp.NowDT;

			var info = eqp.TriggerInfo;

			//init run wip
			if (info == null && lot.IsRunWipFirstPlan())
				return eqp.GetLoadableSubEqps_RunWip(lot);

			if (info == null)							
				return new List<FabSubEqp>();

			return info.GetLoadableList(lot, now);
		}

		private static List<FabSubEqp> GetLoadableSubEqps_RunWip(this FabAoEquipment eqp, FabLot lot)
		{
			List<FabSubEqp> loadableList = new List<FabSubEqp>();

			var availablelist = eqp.GetAvailableSubEqp();
			FabSubEqp triggerSubEqp = SelectBestTriggerSubEqp(availablelist, lot);

			if(triggerSubEqp != null)
				return triggerSubEqp.GetLoadableSubEqpsByTriggerSubEqp(lot);
				
			return loadableList;			
		}

		private static FabSubEqp SelectBestTriggerSubEqp(List<FabSubEqp> list, FabLot lot)
		{
			if (lot == null)
				return null;

			if (list == null || list.Count == 0)
				return null;

			//last plan
			var find = list.Find(t => t.IsLastPlan(lot));
			if (find != null)
				return find;

			//last step(run mode)
			find = list.Find(t => t.LastPlan != null && t.LastPlan.StepID == lot.CurrentStepID);
			if (find != null)
				return find;

			//last null
			find = list.Find(t => t.LastPlan == null);
			if (find != null)
				return find;

			//first
			return list.FirstOrDefault();
		}

		internal static bool IsLoadable_ParallelChamber(AoEquipment aeqp, FabLot lot)
		{
			var eqp = aeqp.ToFabAoEquipment();
			if (eqp.IsParallelChamber == false)
				return true;

			var subEqpList = GetLoadableSubEqps(eqp, lot);
			if (subEqpList != null && subEqpList.Count > 0)
				return true;

			return false;
		}

		internal static List<FabSubEqp> GetAvailableSubEqp(this FabAoEquipment eqp)
		{
			var now = eqp.NowDT;

			List<FabSubEqp> list = new List<FabSubEqp>();
			if (eqp.IsParallelChamber == false)
				return list;

			var subEqpList = eqp.SubEqps;
			if (subEqpList == null)
				return list;

			foreach (var subEqp in subEqpList)
			{
				if (subEqp.IsAvailable(now))
					list.Add(subEqp);
			}

			return list;
		}

		internal static void StartDispatch_ParallelChamber(this AoEquipment aeqp)
		{
			var eqp = aeqp.ToFabAoEquipment();

			if (eqp.IsParallelChamber == false)
				return;

			eqp.SetTriggerSubEqpInfo();
		}

		internal static void EndDispatch_ParallelChamber(this AoEquipment aeqp)
		{
			var eqp = aeqp.ToFabAoEquipment();

			if (eqp.IsParallelChamber == false)
				return;

			eqp.TriggerInfo = null;
		}

		internal static void SetTriggerSubEqpInfo(this FabAoEquipment eqp)
		{
			if (eqp.IsParallelChamber == false)
				return;

			//if (eqp.EqpID == "THCVD400" && eqp.NowDT >= LcdHelper.StringToDateTime("20200305 141500 123540"))
			//	Console.WriteLine("B");

			var info = eqp.TriggerInfo;
			if (info == null)
			{
				info = new TriggerSubEqpInfo() { Target = eqp };
				eqp.TriggerInfo = info;

				info.InitTriggerSubEqpInfo();
			}

			info.SetTriggerSubEqp();
		}

		internal static void CheckAvailableSubEqps(this FabAoEquipment eqp)
		{
			if (eqp.IsParallelChamber == false)
				return;
							
			var info = eqp.TriggerInfo;
			if (info == null)
				return;

			var currSubEqp = info.TriggerSubEqp;
			info.RemoveAvailableList(currSubEqp);

			if(info.AvailableList == null || info.AvailableList.Count == 0)
			{
				eqp.TriggerInfo = null;
				return;
			}

			info.SetTriggerSubEqp();
			if (info.TriggerSubEqp != null)
			{
				//recall dispatch
				eqp.SetModified();
			}
		}

		internal static List<FabSubEqp> GetSubEqpList(this FabAoEquipment eqp)
		{
			var subEqpList = eqp.SubEqps;
			if (subEqpList == null || subEqpList.Length == 0)
				return new List<FabSubEqp>();

			return subEqpList.ToList();
		}

		internal static FabSubEqp FindSubEqp(this FabAoEquipment eqp, ChamberInfo chamber)
		{
			var targetEqp = eqp.TargetEqp;

			string subEqpID = chamber.Label;
			if (string.IsNullOrEmpty(subEqpID))
				return null;

			FabSubEqp subEqp;
			if (targetEqp.SubEqps.TryGetValue(subEqpID, out subEqp))
				return subEqp;

			return subEqp;
		}

		#endregion

		#region FabEqp Function

		public static List<FabSubEqp> GetSubEqps(this FabEqp eqp, ChamberInfo[] chmabers)
		{
			List<FabSubEqp> list = new List<FabSubEqp>();
			foreach (var item in chmabers)
				list.Add(eqp.GetSubEqp(item.Label) as FabSubEqp);

			return list;
		}

		public static List<FabSubEqp> GetSubEqpList(this FabEqp eqp, List<string> subEqps)
		{
			List<FabSubEqp> list = new List<FabSubEqp>();

			foreach (var item in subEqps)
			{
				FabSubEqp sub = GetSubEqp(eqp, item);

				if (sub != null)
					list.Add(sub);

			}

			return list;

		}


		public static FabSubEqp GetSubEqp(this FabEqp eqp, string subEqpID)
		{
			if (string.IsNullOrEmpty(subEqpID))
				return null;

			FabSubEqp subEqp;
			if (eqp.SubEqps.TryGetValue(subEqpID, out subEqp))
				return subEqp;

			return null;
		}

		public static List<string> GetSubEqpIDs(this FabEqp eqp)
		{
			return eqp.SubEqps.Keys.ToList();
		}

		public static bool IsMixRunning(this FabEqp eqp)
		{
			if (eqp.IsParallelChamber == false)
				return false;

			if (eqp.SubEqpGroups.Count < 2)
				return false;

			List<string> currentSteps = new List<string>();
			foreach (var item in eqp.SubEqps.Values)
			{
				if (item.CurrentStep != null)
				{
					if (item.CurrentStep.StdStep.IsMixRunStep == false)
						continue;

					if (currentSteps.Contains(item.CurrentStepID) == false)
						currentSteps.Add(item.CurrentStepID);
				}
			}

			return currentSteps.Count > 1;
		}

		#endregion

		#region FabSubEqp Funtion

		internal static List<FabSubEqp> GetLoadableSubEqpsByTriggerSubEqp(this FabSubEqp triggerSubEqp, FabLot lot)
		{
			List<FabSubEqp> loadableList = new List<FabSubEqp>();

			//group별 운영
			var group = triggerSubEqp.SubEqpGroup;
			if (group != null && group.SubEqps != null)
			{
				var list = group.SubEqps.Values.ToList();
				foreach (var subEqp in list)
				{
					if (loadableList.Contains(subEqp))
						continue;

					//check eqpArrange (subEqp)
					if (EqpArrangeMaster.IsLoadable_ParallelChamber(subEqp, lot) == false)
						continue;

					loadableList.Add(subEqp);
				}
			}

			return loadableList;
		}

		public static bool IsAvailable(this FabSubEqp subEqp, DateTime now)
		{
			if (subEqp.ChamberInfo.Active == false)
				return false;

			if (subEqp.ChamberInfo.Current != null)
				return false;

			if (subEqp.ChamberInfo.GetAvailableTime() <= now)
				return true;

			return false;
		}

		public static bool IsLoadable(this FabSubEqp subEqp, FabStep step, DateTime now)
		{
			if (subEqp.IsAvailable(now) == false)
				return false;

			var targetEqp = subEqp.Parent as FabEqp;

			int groupCount = targetEqp.SubEqpGroups.Count;
			bool isMulti = groupCount > 1;

			if (isMulti == false)
				return true;

			//진행 중인 Step과 동일한 Step 가용
			if (subEqp.ChamberInfo.Current != null && subEqp.CurrentStepID == step.StepID)
				return true;

			bool isIdle, isAvailableMixRun;
			targetEqp.CheckOtherSubEqpGroup(subEqp, step.StdStep, out isIdle, out isAvailableMixRun);

			//타 Group의 Chamber가 모두 IDLE 상태
			if (isIdle)
				return true;

			//타 Group의 Chamber가 MixRun가능 Step을 진행 중인 경우 MixRun 가능 Step은 동시 가용
			if (isAvailableMixRun && step.StdStep.IsMixRunStep)
				return true;

			return false;
		}

		private static void CheckOtherSubEqpGroup(this FabEqp targetEqp, FabSubEqp subEqp, FabStdStep targetStdStep, out bool isIdle, out bool isAvailableMixRun)
		{
			isIdle = true;
			isAvailableMixRun = true;

			var groupList = targetEqp.SubEqpGroups;
			foreach (var group in groupList)
			{
				//동일 Group 체크 X
				if (group == subEqp.SubEqpGroup)
					continue;

				var sampleSubEqp = group.SubEqps.Values.FirstOrDefault();
				if (sampleSubEqp == null)
					continue;

				if (sampleSubEqp.Current != null)
					isIdle = false;

				if (sampleSubEqp.IsMixRunStep == false)
					isAvailableMixRun = false;
				else
				{
					if (sampleSubEqp.CurrentStepID == targetStdStep.StepID)
						continue;

					FabStdStep stdStep = sampleSubEqp.CurrentStep.StdStep;
					if (stdStep.MixRunPairSteps.Contains(targetStdStep) == false)
						isAvailableMixRun = false;
				}
			}

			//모두 IDLE인 경우는 isAvailableMixRun = false 상태이므로
			if (isIdle && isAvailableMixRun)
				isAvailableMixRun = false;
		}

		#endregion

		#region TriggerSubEqpInfo Funtion

		private static void InitTriggerSubEqpInfo(this TriggerSubEqpInfo info)
		{
			var eqp = info.Target;

			info.AvailableList = eqp.GetAvailableSubEqp();			
		}

		private static void SetTriggerSubEqp(this TriggerSubEqpInfo info)
		{
			info.TriggerSubEqp = null;
			
			var list = info.AvailableList;
			if(list != null)
				info.TriggerSubEqp = list.FirstOrDefault();	
		}

		private static bool RemoveAvailableList(this TriggerSubEqpInfo info, FabSubEqp subEqp)
		{
			if (subEqp == null)
				return false;

			var list = info.AvailableList;
			if (list == null || list.Count == 0)
				return false;

			return list.Remove(subEqp);			
		}

		private static List<FabSubEqp> GetLoadableList(this TriggerSubEqpInfo info, FabLot lot, DateTime now)
		{
			List<FabSubEqp> loadableList = new List<FabSubEqp>();
			if (info == null)
				return loadableList;
							
			var triggerSubEqp = info.TriggerSubEqp;
			if (triggerSubEqp == null)
				return loadableList;
				
			loadableList.Add(triggerSubEqp);

			//개별 운영
			//var list = info.AvailableList;					
			//if (list == null || list.Count == 0)
			//	return loadableList;

			//foreach (var subEqp in list)
			//{
			//	if (loadableList.Contains(subEqp))
			//		continue;

			//	if (subEqp.GroupID != triggerSubEqp.GroupID)
			//		continue;

			//	//check eqpArrange (subEqp)
			//	if (EqpArrangeMaster.IsLoadable_ParallelChamber(subEqp, lot) == false)
			//		continue;

			//	//check state
			//	if (subEqp.IsLoadable(lot.CurrentFabStep, now) == false)
			//		continue;

			//	loadableList.Add(subEqp);
			//}

			//group별 운영
			var group = triggerSubEqp.SubEqpGroup;
			if (group != null && group.SubEqps != null)
			{
				var list = group.SubEqps.Values.ToList();
				foreach (var subEqp in list)
				{
					if (loadableList.Contains(subEqp))
						continue;

					//check eqpArrange (subEqp)
					if (EqpArrangeMaster.IsLoadable_ParallelChamber(subEqp, lot) == false)
						continue;

					loadableList.Add(subEqp);
				}
			}

			return loadableList;
		}

		#endregion

		internal static ISet<string> GetNeedSetupChamberIDs(List<FabSubEqp> chambers, FabLot lot)
		{
			ISet<string> result = new HashSet<string>();

			var list = GetNeedSetupChamberList(chambers, lot);

			foreach (var unit in list)
				result.Add(unit.SubEqpID);

			return result;
		}

		internal static List<FabSubEqp> GetNeedSetupChamberList(List<FabSubEqp> chambers, FabLot lot)
		{
			List<FabSubEqp> list = GetNeedSetupChamberList(chambers,
														   lot.CurrentShopID,
														   lot.CurrentStepID,
														   lot.CurrentProductID,
														   lot.CurrentProductVersion,
														   lot.OwnerType,
														   lot.OwnerID);

			return list;
		}

		internal static List<FabSubEqp> GetNeedSetupChamberList(
			List<FabSubEqp> chambers,
			string shopID,
			string stepID,
			string productID,
			string prodVer,
			string ownerType,
			string ownerID)
		{
			List<FabSubEqp> list = new List<FabSubEqp>();

			foreach (var subEqp in chambers)
			{
				if (subEqp.IsNeedSetup(shopID, stepID, productID, prodVer, ownerType, ownerID))
					list.Add(subEqp);
			}

			return list;
		}

		internal static bool IsNeedSetup(this FabSubEqp subEqp, string shopID, string stepID,
			string productID, string prodVer, string ownerType, string ownerID)
		{
			var setupTime = subEqp.GetSetupTime(shopID,
												stepID,
												productID,
												prodVer,
												ownerType,
												ownerID);

			if (setupTime > 0)
				return true;

			return false;
		}

		//internal static bool IsNeedSetup(this FabSubEqp subEqp, string stepID, string productID, string prodVer, string ownerType)
		//{
		//	if (ResHelper.IsIgnoreSetup(subEqp.LoadInfos))
		//		return false;

		//	FabPlanInfo last = subEqp.LastPlan;

		//	if (subEqp.Current != null)
		//	{
		//		FabLot current = subEqp.Current as FabLot;
		//		last = current.CurrentFabPlan;
		//	}

		//	if (last == null)
		//		return true;

		//	if (last.Step == null || last.Product == null)
		//		return true;

		//	var eqp = ResHelper.GetFabAoEquipment(subEqp.Parent.EqpID);
		//	if (eqp == null)
		//		return false;

		//	if (last.StepID != stepID)
		//		return true;

		//	if (last.ProductID != productID)
		//		return true;

		//	if (last.ProductVersion != prodVer)
		//		return true;

		//	if (last.OwnerType != ownerType)
		//		return true;

		//	return false;
		//}

		internal static bool IsLastPlan(this FabSubEqp subEqp, FabLot lot)
		{
			if (lot == null)
				return false;

			return subEqp.IsLastPlan(lot.CurrentShopID,
									 lot.CurrentStepID,
									 lot.CurrentProductID,
									 lot.CurrentProductVersion,
									 lot.OwnerType,
									 lot.OwnerID);
		}

		internal static float GetSetupTime(this FabSubEqp subEqp, FabLot lot)
		{
			string shopID = lot.CurrentShopID;
			string stepID = lot.CurrentStepID;
			string productID = lot.CurrentProductID;
			string prodVer = lot.CurrentProductVersion;
			string ownerType = lot.OwnerType;
			string ownerID = lot.OwnerID;

			var setupTime = subEqp.GetSetupTime(shopID,
												stepID,
												productID,
												prodVer,
												ownerType,
												ownerID);

			return setupTime;
		}

		internal static float GetSetupTime(this FabSubEqp subEqp, string shopID, string stepID,
			string productID, string prodVer, string ownerType, string ownerID)
		{
			var setupTime = SetupMaster.GetSetupTime(subEqp,
													 shopID,
													 stepID,
													 productID,
													 prodVer,
													 ownerType,
													 ownerID);
			return setupTime;
		}

		internal static void ChangeCurrentRunMode(this FabSubEqp subEqp, FabLot lot)
		{
			if (subEqp == null)
				return;

			if (lot == null)
				return;

			var step = lot.CurrentFabStep;
			if (step == null || step.StdStep == null)
				return;

			var targetEqp = subEqp.Parent as FabEqp;
			string eqpGroup = targetEqp.EqpGroup;
			string runMode = subEqp.SubEqpGroup.CurrentRunMode;
			string productID = lot.CurrentProductID;
			string ownerType = lot.OwnerType;

			var branchStep = BranchStepMaster.GetBranchStep(eqpGroup, runMode);
			if (branchStep != null)
			{
				//현재 runMode에서 Loadable 한 경우 RunMode 유지
				if (branchStep.IsLoadable(productID, ownerType))
					return;
			}

			//change runMode (DefaultRunMode)
			subEqp.SubEqpGroup.CurrentRunMode = step.StdStep.DefaultRunMode;
		}
	}
}
