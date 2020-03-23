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
using Mozart.SeePlan.Lcd.DataModel;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class ResHelper
	{
		static Dictionary<string, List<FabAoEquipment>> _eqpsByDspEqpGroup = new Dictionary<string, List<FabAoEquipment>>();
		
		internal static FabEqp FindEqp(string eqpID)
		{
			if (string.IsNullOrEmpty(eqpID))
				return null;

			FabEqp eqp;
			InputMart.Instance.FabEqp.TryGetValue(eqpID, out eqp);

			return eqp;
		}

		internal static FabSubEqp FindSubEqp(string subEqpID)
		{
			if (string.IsNullOrEmpty(subEqpID))
				return null;

			FabSubEqp subEqp;
			InputMart.Instance.FabSubEqp.TryGetValue(subEqpID, out subEqp);

			return subEqp;
		}

		internal static FabAoEquipment GetFabAoEquipment(string eqpID)
		{
			var aeqp = AoFactory.Current.GetEquipment(eqpID);

			if (aeqp == null)
				return null;

			return aeqp.ToFabAoEquipment();
		}
		
		internal static void AddEqpByGroup(FabAoEquipment eqp)
		{
			AddEqpByDspEqpGroup(eqp);
		}

		internal static void AddEqpByDspEqpGroup(FabAoEquipment eqp)
		{
			string key = eqp.DspEqpGroupID;
			List<FabAoEquipment> list;
			if (_eqpsByDspEqpGroup.TryGetValue(key, out list) == false)
				_eqpsByDspEqpGroup.Add(key, list = new List<FabAoEquipment>());

			list.Add(eqp);
		}

		internal static List<FabAoEquipment> GetEqpsByDspEqpGroup(string dspEqpGroup)
		{
			if (dspEqpGroup == null)
				return null;

			List<FabAoEquipment> list;
			_eqpsByDspEqpGroup.TryGetValue(dspEqpGroup, out list);

			return list;
		}

		internal static List<string> GetAllDspEqpGroup()
		{
			return _eqpsByDspEqpGroup.Keys.ToList();
		}

		internal static Dictionary<string, List<FabAoEquipment>> GetWorkingEqpInfos(AoEquipment aeqp, bool with)
		{
			var eqp = aeqp.ToFabAoEquipment();

			Dictionary<string, List<FabAoEquipment>> result = new Dictionary<string, List<FabAoEquipment>>();

			string dspEqpGroupID = eqp.DspEqpGroupID;

			List<FabAoEquipment> eqpGroups = GetEqpsByDspEqpGroup(dspEqpGroupID);

			if (eqpGroups != null)
			{
				foreach (FabAoEquipment it in eqpGroups)
				{
					if ((aeqp.Target as FabEqp).ShopID != (it.Target as FabEqp).ShopID)
						continue;

					if (with == false && it == aeqp)
						continue;

					var last = it.LastPlan as FabPlanInfo;
					if (last == null || it.Loader.IsBlocked())
						continue;

					string shopID = last.ShopID;
					string stepID = last.StepID;
					string productID = last.ProductID;
					string prodVer = last.ProductVersion;
					string ownerType = last.OwnerType;

					string key = CreateKeyForJobFilter(shopID, stepID, productID, prodVer, ownerType);

					List<FabAoEquipment> list;
					if (result.TryGetValue(key, out list) == false)
						result.Add(key, list = new List<FabAoEquipment>());

					list.Add(it);
				}
			}

			return result;
		}

		internal static FabPlanInfo GetLastPlan(this FabAoEquipment eqp)
		{
			var last = eqp.LastPlan as FabPlanInfo;					

			if (eqp.IsParallelChamber == false)
				return last;

			var info = eqp.TriggerInfo;
			if (info == null)
				return last;

			if (info.TriggerSubEqp != null)
				return info.TriggerSubEqp.LastPlan;
			
			var list = eqp.GetAvailableSubEqp();

			var first = list.FirstOrDefault();
			if (first != null)
				last = first.LastPlan;

			return last;
		}

		internal static LineType GetLineType(this FabAoEquipment eqp)
		{
			if (eqp == null)
				return LineType.NONE;

			string eqpID = LcdHelper.ToUpper(eqp.EqpID);
			if (eqp != null && eqpID.Contains("UPH"))
				return LineType.SUB;

			var preset = eqp.Preset;
			if (preset != null)
			{
				string presetID = LcdHelper.ToUpper(preset.Name);
				if (presetID != null & presetID.Contains("ULINE"))
					return LineType.SUB;
			}

			return LineType.MAIN;
		}

		internal static bool IsLastPlan(this FabAoEquipment eqp, FabLot lot, bool checkProductVersion = true)
		{
			if (lot == null)
				return false;

			string shopID = lot.CurrentShopID;
			string stepID = lot.CurrentStepID;
			string productID = lot.CurrentProductID;

			string productVersion = lot.CurrentProductVersion;
			string ownerType = lot.OwnerType;
			string ownerID = lot.OwnerID;

			if (eqp.IsLastPlan(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion))
				return true;

			return false;
		}

		internal static bool IsLastPlan(this FabAoEquipment eqp, string shopID, string stepID, 
			string productID, string productVersion, string ownerType, string ownerID, 
			bool checkProductVersion = true)
		{
			var last = eqp.GetLastPlan();
			if (last == null)
				return false;

			if (last.IsMatched(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion))
				return true;

			return false;
		}
		                
        internal static bool IsLastPlan(this FabSubEqp subEqp, string shopID, string stepID,
			string productID, string productVersion, string ownerType, string ownerID, 
			bool checkProductVersion = true)
        {
            var last = subEqp.LastPlan as FabPlanInfo;

			if (last == null)
				return false;
							
			return last.IsMatched(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion);
		}

		internal static bool IsNeedSetup(FabAoEquipment eqp, FabLot lot)
		{
			if (lot == null)
				return false;
							
			if (eqp.IsParallelChamber)
            {
                return ResHelper.IsNeedChamberSetup(eqp, lot);
            }

			if (ResHelper.IsIgnoreSetup(eqp))
				return false;

			return IsNeedSetup(eqp,
                               lot.CurrentShopID,
                               lot.CurrentStepID,
                               lot.CurrentProductID,
                               lot.CurrentProductVersion,
                               lot.OwnerType,
                               lot.OwnerID);
        }

        private static bool IsNeedSetup(this FabAoEquipment eqp, string shopID, string stepID,
            string productID, string prodVer, string ownerType, string ownerID)
        {
            var setupTime = SetupMaster.GetSetupTime(eqp, 
                                                     shopID,
                                                     stepID,
                                                     productID,
                                                     prodVer,
                                                     ownerType, 
                                                     ownerID);

			if (setupTime > 0)
				return true;

            return false;
        }

		public static bool IsIgnoreSetup(AoEquipment aeqp)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			return IsIgnoreSetup(eqp.LoadInfos);
		}

		public static bool IsIgnoreSetup(List<FabLoadInfo> infos)
        {
            if (infos == null || infos.Count == 0)
                return false;

			var last = infos.FindLast(p => LoadHelper.IsIgnorableState(p.State) == false);

			//if (last != null && (last.State == LoadingStates.DOWN || last.State == LoadingStates.PM))
			//	return true;

			//down 이후 stepup 필요 (2020.01.14 - by.liujian(유건))
			if (last != null && (last.State == LoadingStates.PM))
				return true;

			return false;
		}

        private static bool IsNeedChamberSetup(FabAoEquipment eqp, FabLot lot)
        {
            //Equipment → ProcessControl → GetLoadableChambers : 투입가능한 챔버리스트 반환
            List<FabSubEqp> list = ChamberMaster.GetLoadableSubEqps(eqp, lot);

			if (list.Count == 0)
				return false;

            //Equipment → ProcessControl → GetNeedSetupChamber 
            var needSetupChamberList = ChamberMaster.GetNeedSetupChamberList(list, lot);

            if (needSetupChamberList != null && needSetupChamberList.Count > 0)
                return true;

            return false;
        }
        
        public static WaitingWipInfo GetTargetStepWaitingWip(string stepID)
        {
            var eqps = EqpArrangeMaster.GetStepAssignedEqps(stepID);
            var waitingInfo = new WaitingWipInfo();

			waitingInfo.TargetStepID = stepID;

			HashSet<FabLot> results = new HashSet<FabLot>();
			foreach (var it in eqps)
			{
				//var targetEqp = AoFactory.Current.GetEquipment(it);

				var lots = AoFactory.Current.WipManager.InEqps(it, WipTags.Agent.Waiting);
				foreach (FabLot lot in lots)
				{
					if (waitingInfo.TargetStepID != lot.CurrentStepID)
						continue;

					if (results.Contains(lot))
						continue;

					results.Add(lot);

					string key = lot.CurrentProductID;
					if (!waitingInfo.QtyByProductID.ContainsKey(key))
						waitingInfo.QtyByProductID.Add(key, 0);

					waitingInfo.QtyByProductID[key] += lot.UnitQty;
					waitingInfo.TotQty += lot.UnitQty;
				}
			}

			return waitingInfo;
		}

		////TODO : Temp-POC 모델용
		//public static bool EqualsStep(this FabStep current, FabStep next)
		//{
		//	bool rvalue = true;
		//	if (current.StepID != next.StepID)
		//	{
		//		rvalue = false;
		//		if (current.StepID.EndsWith("R") || next.StepID.EndsWith("R"))
		//		{
		//			var orignStepID = "-"; //current.GetStepPatternKey();// current.StepID.Split('-')[0];
		//			var newStepID = "-"; //next.GetStepPatternKey();// next.StepID.Split('-')[0];

		//			if (orignStepID.Equals(newStepID))
		//				rvalue = true;
		//		}
		//	}

		//	return rvalue;
		//}

		public static void CheckContinueousQty(AoEquipment aeqp, IHandlingBatch hb)
		{
			var eqp = aeqp.ToFabAoEquipment();
			FabLot lot = hb.ToFabLot();

			bool isLast = ResHelper.IsLastPlan(eqp, lot, true);
			if (isLast == false)
				ResetContinueousQty(aeqp);
		
			UpdateContinueousQty(aeqp, lot.UnitQty);
		}

		private static void ResetContinueousQty(AoEquipment aeqp)
		{
			FabAoEquipment feqp = aeqp as FabAoEquipment;

			feqp.ContinuousQty = 0;
		}

		private static void UpdateContinueousQty(AoEquipment aeqp, int unitQty)
		{
			FabAoEquipment eqp = aeqp as FabAoEquipment;
			
			if (unitQty < 0 && eqp.ContinuousQty <= 0)
				return;

			eqp.ContinuousQty += unitQty;
		}

		internal static void SetLastLoadingInfo(AoEquipment aeqp, IHandlingBatch hb)
		{
			EqpSetupControl.Instance.SetLastLoadingInfo(aeqp, hb);
		}

		internal static string CreateKeyForJobFilter(string shop, string stepID, string productID, string prodVer, string ownerType)
		{
			return LcdHelper.CreateKey(shop, stepID, productID, prodVer, ownerType);
		}
	}
}
