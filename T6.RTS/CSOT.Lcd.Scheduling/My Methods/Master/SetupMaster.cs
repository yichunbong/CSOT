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
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class SetupMaster
	{
		public static float GetSetupTimeWithAhead(FabAoEquipment eqp, FabLot lot)
		{
			float setupTime = GetSetupTime(eqp, lot, true);
			setupTime = eqp.CheckAheadSetupTime(setupTime, lot);

			return setupTime;
		}

		//단위 : 분
		public static float GetSetupTime(AoEquipment aeqp, FabLot lot, bool markingAcid = false)
		{
			string shopID = lot.CurrentShopID;
			string stepID = lot.CurrentStepID;
			string productID = lot.CurrentProductID;
			string prodVer = lot.CurrentProductVersion;
			string ownerType = lot.OwnerType;
			string ownerID = lot.OwnerID;

			return GetSetupTime(aeqp, shopID, stepID, productID, prodVer, ownerType, ownerID, markingAcid);
		}
		
		public static float GetSetupTime(AoEquipment aeqp, string shopID, string stepID, 
			string productID, string prodVer, string ownerType, string ownerID, bool markingAcid = false)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			//if(eqp.EqpID == "THATS300")
			//	Console.WriteLine("B");

			//if (CheckLastPlan(eqp, shopID, stepID, productID, prodVer, ownerType, ownerID))
			//	return 0;

			SetupInfo info = CreateHelper.CreateSetupInfo(eqp, shopID, stepID, productID, prodVer, ownerType, ownerID);

			string eqpGroup = eqp.TargetEqp.EqpGroup;
			float setupTime = GetSetupTime(eqpGroup, info);
			float acidChangeTime = AcidMaster.GetAcidChangeTime(eqp, stepID, productID);
			float totalSetupTime = setupTime + acidChangeTime;

			//용액교체 Setup 발생시 EqpPlan 기록을 위해 표시
			if (markingAcid && acidChangeTime > 0)
				AcidMaster.SetSetupMark(eqp, true);

			return totalSetupTime;
		}

		public static float GetSetupTime(FabSubEqp subEqp, string shopID, string stepID,
			string productID, string prodVer, string ownerType, string ownerID, bool markingAcid = false)
		{			
			SetupInfo info = CreateHelper.CreateSetupInfo(subEqp, shopID, stepID, productID, prodVer, ownerType, ownerID);

			string eqpGroup = (subEqp.Parent as FabEqp).EqpGroup;
			float setupTime = GetSetupTime(eqpGroup, info);

			return setupTime;
		}

		private static float GetSetupTime(string eqpGroup, SetupInfo info)
		{
			List<SetupTime> list = GetMatchSetupTime(eqpGroup);
			List<SetupTime> matchList = GetMatchSetupTime(info, list);

			if (matchList.Count > 0)
				return matchList[0].Time;

			return 0;
		}

		private static List<SetupTime> GetMatchSetupTime(string eqpGroup)
		{
			ICollection<SetupTime> list;
			InputMart.Instance.SetupTime.TryGetValue(eqpGroup, out list);

			if (list == null)
				return new List<SetupTime>();

			return list.ToList();
		}

		private static List<SetupTime> GetMatchSetupTime(SetupInfo info, List<SetupTime> setupTimes)
		{
			List<SetupTime> list = new List<SetupTime>();

			foreach (var item in setupTimes)
			{
				if (item.IsMatch(info))
					list.Add(item);
			}

			list.Sort(CompareHelper.SetupTimeComparer.Default);

			return list;
		}

		//private static bool CheckLastPlan(FabAoEquipment eqp, string shopID, string stepID, string productID, string prodVer, string ownerType, string ownerID)
		//{
		//	if (AcidMaster.IsNeedChangeAcid(eqp, stepID, productID))
		//		return false;

		//	if (eqp.IsParallelChamber)
		//	{
		//		//Equipment → ProcessControl → GetNeedSetupChamber 
		//		List<FabSubEqp> needSetupChambers = ChamberMaster.GetNeedSetupChamberList(eqp.SubEqps.ToList(), shopID, stepID, productID, prodVer, ownerType, ownerID);
		//		if (needSetupChambers.Count == 0)
		//			return true;
		//	}

		//	if (eqp.IsLastPlan(shopID, stepID, productID, prodVer, ownerType, ownerID))
		//		return true;

		//	return false;
		//}
		
		private static bool IsMatch(this SetupTime item, SetupInfo info)		
		{
			//if(info.EqpID == "THPHL100" && info.ToProductID == "TH645A3AB100")
			//	Console.WriteLine("B");

			if (item.IsMatch_Base(info) == false)
				return false;

			if (item.IsEqpAll == false)
			{
				if (LcdHelper.Equals(item.EqpID, info.EqpID) == false)
					return false;
			}
		
			if (item.HasChangeType(ChangeType.P))
			{
				if (IsMatch(item.FromProductID, item.ToProductID, info.FromProductID, info.ToProductID) == false)
					return false;
			}

			if (item.HasChangeType(ChangeType.O))
			{
				if (IsMatch(item.FromStepID, item.ToStepID, info.FromStepID, info.ToStepID) == false)
					return false;
			}

			if (item.HasChangeType(ChangeType.B))
			{
				if (IsMatch(item.FromProductVersion, item.ToProductVersion, info.FromProductVersion, info.ToProductVersion) == false)
					return false;
			}

			if (item.HasChangeType(ChangeType.PRODUCT_VERSION))
			{
				if (IsMatch(item.FromEtc, item.ToEtc, info.FromProductVersion, info.ToProductVersion) == false)
					return false;
			}

			if (item.HasChangeType(ChangeType.OWNER_TYPE))
			{
				if (IsMatch(item.FromEtc, item.ToEtc, info.FromOwnerType, info.ToOwnerType) == false)
					return false;
			}

			if (item.HasChangeType(ChangeType.OWNER_ID))
			{
				if (IsMatch(item.FromEtc, item.ToEtc, info.FromOwnerID, info.ToOwnerID) == false)
					return false;
			}

			return true;
		}

		private static bool IsMatch_Base(this SetupTime item, SetupInfo info)
		{			
			if (item.HasChangeType(ChangeType.P))
			{
				if (info.FromProductID != info.ToProductID)
					return true;
			}

			if (item.HasChangeType(ChangeType.O))
			{
				if (info.FromStepID != info.ToStepID)
					return true;
			}

			if (item.HasChangeType(ChangeType.B))
			{
				if (info.FromProductVersion != info.ToProductVersion)
					return true;
			}

			if (item.HasChangeType(ChangeType.PRODUCT_VERSION))
			{
				if (info.FromProductVersion != info.ToProductVersion)
					return true;
			}

			if (item.HasChangeType(ChangeType.OWNER_TYPE))
			{
				if (info.FromOwnerType != info.ToOwnerType)
					return true;
			}

			if (item.HasChangeType(ChangeType.OWNER_ID))
			{
				if (info.FromOwnerID != info.ToOwnerID)
					return true;
			}

			return false;
		}

		private static bool IsMatch(string fromItem, string toItem, string from, string to)
		{
            //from
            if (IsMatch(from, fromItem, toItem) == false)
                return false;

            //to
            if (IsMatch(to, toItem, fromItem) == false)
                return false;

			return true;
		}

        private static bool IsMatch(string info, string myitem, string otheritem)
        {
			if (string.IsNullOrEmpty(info))
				return false;

			if (string.IsNullOrEmpty(myitem) || string.IsNullOrEmpty(otheritem))
				return false;

            //ALL
            if (myitem.IsAllID())
                return true;

			//OTHER
			if (myitem.IsOtherID())
			{
				if (otheritem.IsAllID() || otheritem.Contains(info))
					return false;
			}
			else
			{
				//Match
				if (myitem.Contains(info) == false)
					return false;
			}

            return true;
        }

        public static List<ChangeType> GetSetupType(FabProduct fromProduct, FabProduct toProduct, FabStep fromStep, FabStep toStep, string fromProductVer, string toProductVer)
		{
			List<ChangeType> result = new List<ChangeType>();

			if (IsProductChange(fromProduct, toProduct))
				result.Add(ChangeType.P);

			if (IsProcessChange(fromStep, toStep))
				result.Add(ChangeType.O);

			if (IsVersionChange(fromProductVer, toProductVer))
				result.Add(ChangeType.B);

			return result;
		}

		private static bool IsProductChange(FabProduct fromProduct, FabProduct toProduct)
		{
			if (fromProduct == null)
				return true;

			return fromProduct.ProductID != toProduct.ProductID;
		}

		private static bool IsProcessChange(FabStep fromStep, FabStep toStep)
		{
			if (fromStep == null)
				return true;

			return fromStep.StepID != toStep.StepID;
		}

		private static bool IsVersionChange(string fromVersion, string toVersion)
		{
			if (LcdHelper.IsEmptyID(fromVersion))
				return true;

			return fromVersion != toVersion;
		}        

		private static float GetAdditionalSetupTime(string eqpGroupID, string stepID, string productID, float idleTime)
		{
			float addSetupTime = 0f;

			var finds = InputMart.Instance.SetupTimeIdlebyStep.FindRows(eqpGroupID, stepID).ToList();
			if (finds == null || finds.Count() == 0)
				return addSetupTime;

            //product matched
            var matchList = finds.FindAll(t => t.PRODUCT_ID == productID);

            //all
            if (matchList == null || matchList.Count == 0)
                matchList = finds.FindAll(t => LcdHelper.Equals(t.PRODUCT_ID, "ALL"));

            var list = matchList.FindAll(t => t.IDLE_TIME <= idleTime);
            if (list != null && list.Count > 0)
            {
                //range matched
                list.Sort(new CompareHelper.SetupTimeIdleComparer());
                addSetupTime = list[0].SETUP_TIME;
            }

            return addSetupTime;
		}

		internal static float GetAdditionalSetupTime(FabAoEquipment eqp, string stepID, string productID, DateTime setupStartTime)
		{
			if (eqp.LastIdleStartTime == DateTime.MinValue)
				return 0;
                			
            string eqpGroupID = eqp.TargetEqp.EqpGroup;
            TimeSpan idleTime = setupStartTime - eqp.LastIdleStartTime;

            float setupTime = GetAdditionalSetupTime(eqpGroupID, stepID, productID, (float)idleTime.TotalMinutes);

			return setupTime;
		}

		#region SetupTime Func

		public static bool HasChangeType(this SetupTime item, ChangeType ctype)
		{
			if (item == null)
				return false;

			var list = item.ChangeTypeList;
			if (list == null || list.Count == 0)
				return false;

			if (list.Contains(ctype))
				return true;

			return false;
		}

		#endregion SetupTime Func
	}
}
