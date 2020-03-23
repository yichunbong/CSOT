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

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class AcidMaster
	{
		internal static void InitAcidDensity(this FabAoEquipment eqp)
		{
			AcidDensity acid = CreateHelper.CreateAcidDensity(eqp);

			acid.InitAcid = eqp.TargetEqp.StatusInfo == null ? 0 : eqp.TargetEqp.StatusInfo.LastAcidDensity;
			acid.CurrentAcid = acid.InitAcid;

			var chgInfos = InputMart.Instance.AcidChgInfoView.FindRows(eqp.ShopID, eqp.TargetEqp.EqpGroup);
			if (chgInfos == null || chgInfos.Count() == 0)
			{
				//등록된 설비 그룹만 농도체크
				eqp.AcidDensity = null;
				return;
			}
			else
			{
				AcidChgInfo info = chgInfos.First();
				acid.ChangeDensity = info.DENSITY;
				acid.ChangeTime = info.CHG_TIME;

				eqp.AcidDensity = acid;
			}
		}

		internal static AcidAlter GetAcidAlter(string shopID, string eqpGroupID, string productID, string stepID)
		{
			var rows = InputMart.Instance.AcidAlterView.FindRows(shopID, eqpGroupID, productID, stepID);

			if (rows == null || rows.Count() == 0)
				return null;

			return rows.First();
		}

		internal static AcidLimit GetAcidLimit(string shopID, string eqpGroupID, string productID, string stepID)
		{
			var rows = InputMart.Instance.AcidLimitVIew.FindRows(shopID, eqpGroupID, productID, stepID);

			if (rows == null || rows.Count() == 0)
				return null;

			return rows.First();
		}
		
		internal static bool IsNeedChangeAcid(FabAoEquipment eqp, string stepID, string productID)
		{
			if (eqp == null)
				return false;

			if (eqp.IsAcidConst == false)
				return false;

			AcidDensity acid = eqp.AcidDensity;

			//설비의 최대 농도 초과시 Setup필요
			if (acid.CurrentAcid > acid.ChangeDensity)
				return true;

			AcidLimit limit = AcidMaster.GetAcidLimit(eqp.ShopID, eqp.TargetEqp.EqpGroup, productID, stepID);
			if (limit == null)
				return false;

			bool isLast = IsLastPlan(eqp, productID, stepID);
			float limitDensity = isLast ? limit.DENSITY_LIMIT : limit.DENSITY_JC;

			//제품별 농도 초과시 Setup필요
			if (acid.CurrentAcid > limitDensity)
				return true;

			return false;
		}

		private static bool IsLastPlan(FabAoEquipment eqp, string productID, string stepID)
		{
			var last = eqp.GetLastPlan(); //eqp.LastPlan as FabPlanInfo;
			if (last == null)
				return false;

			if (last.ProductID != productID)
				return false;

			if (last.StepID != stepID)
				return false;

			return true;
		}

		internal static float GetAcidChangeTime(AoEquipment aeqp, FabLot lot)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			string productID = lot.CurrentProductID;
			string stepID = lot.CurrentStepID;			

			return GetAcidChangeTime(eqp, productID, stepID);
		}

		internal static float GetAcidChangeTime(FabAoEquipment eqp, string stepID, string productID)
		{
			if (eqp == null)
				return 0f;

			if (eqp.IsAcidConst == false)
				return 0f;

			if (IsNeedChangeAcid(eqp, stepID, productID) == false)
				return 0f;

			return eqp.AcidDensity.ChangeTime;
		}

		internal static void SetSetupMark(FabAoEquipment eqp, bool mark)
		{
			if (eqp == null)
				return;

			if (eqp.IsAcidConst == false)
				return;

			eqp.AcidDensity.IsSetupMark = mark;
		}

		internal static void OnBegingProcessing(AoEquipment aeqp, FabLot lot)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			AcidAlter alter = GetAcidAlter(eqp.ShopID, eqp.TargetEqp.EqpGroup, lot.CurrentProductID, lot.CurrentStepID);

			if (alter == null)
				return;

			float alterQty = alter.DENSITY_ALTER * lot.UnitQty;

			if (alter.ALTER_TYPE != "UP")
			{
				eqp.AcidDensity.CurrentAcid -= alterQty;
			}
			else
			{
				eqp.AcidDensity.CurrentAcid += alterQty;
				WriteAcidDensityLog(eqp, lot, alter, alterQty, aeqp.NowDT, "BUSY");
			}
		}

		internal static void ResetAcidDensity(FabAoEquipment eqp, FabLot lot, DateTime now)
		{
			eqp.AcidDensity.CurrentAcid = 0;
			WriteAcidDensityLog(eqp, lot, null, 0, now, "RESET");
		}

		internal static void WriteAcidDensityLog(FabAoEquipment eqp, FabLot lot, AcidAlter alter, float alterQty, DateTime inTime, string status)
		{
			Outputs.AcidDensityLog row = new AcidDensityLog();

			row.VERSION_ID = ModelContext.Current.VersionNo;

			row.FACTORY_ID = eqp.FactoryID;
			row.SHOP_ID = eqp.ShopID;
			row.EQP_ID = eqp.EqpID;
			row.EQP_GROUP = eqp.TargetEqp.EqpGroup;
			row.IN_TIME = inTime;
			row.STATUS = status;

			row.LOT_ID = lot.LotID;
			row.PRODUCT_ID = lot.CurrentProductID;
			row.STEP_ID = lot.CurrentStepID;
			row.UNIT_QTY = lot.UnitQty;

			row.CURRENT_DENSITY = eqp.AcidDensity.CurrentAcid;
			row.USED_DENSITY = alterQty;
			row.ALTER_DENCITY = alter != null ? alter.DENSITY_ALTER : 0;
			row.INIT_DENSITY = eqp.AcidDensity.InitAcid;
			row.MAX_DENSITY = eqp.AcidDensity.ChangeDensity;

			OutputMart.Instance.AcidDensityLog.Add(row);
		}


		//Filter에서 쓰던 코드 참고용
		//private static bool IsAcidDensityLimit(AoEquipment aeqp, FabLot lot, ref string reason)
		//{
		//	FabAoEquipment eqp = aeqp.ToFabAoEquipment();

		//	if (eqp.IsAcidConst == false)
		//		return true;

		//	if (eqp.EqpID == "THITO100")
		//		Console.WriteLine();

		//	#region 설비 최대 Acid농도 제약
		//	AcidAlter alter = AcidMaster.GetAcidAlter(eqp.ShopID, eqp.TargetEqp.EqpGroup, lot.CurrentProductID, lot.CurrentStepID);

		//	float currentAcid = eqp.AcidDensity.CurrentAcid;
		//	float alterValue = alter == null ? 0 : alter.DENSITY_ALTER;
		//	float alterQty = lot.UnitQty * alterValue;

		//	if (currentAcid > eqp.AcidDensity.ChangeDensity)
		//	{
		//		reason = string.Format("Max Density：{0} < Current：{1} + {2}", eqp.AcidDensity.ChangeDensity, currentAcid, alterQty);
		//		return false;
		//	}
		//	#endregion


		//	#region Acid Limit 제약
		//	AcidLimit limit = AcidMaster.GetAcidLimit(eqp.ShopID, eqp.TargetEqp.EqpGroup, lot.CurrentProductID, lot.CurrentStepID);
		//	if (limit == null)
		//		return true;

		//	bool isLast = eqp.IsLastPlan(lot);

		//	if (isLast)
		//	{
		//		if (currentAcid > limit.DENSITY_LIMIT)
		//		{
		//			reason = string.Format("Limit：{0} < Current：{1} + {2}", limit.DENSITY_LIMIT, currentAcid, alterQty);
		//			return false;
		//		}
		//	}
		//	else
		//	{
		//		if (currentAcid > limit.DENSITY_JC)
		//		{
		//			reason = string.Format("Density_JC：{0} < Current：{1} + {2}", limit.DENSITY_JC, currentAcid, alterQty);
		//			return false;
		//		}
		//	}
		//	#endregion



		//	return true;
		//}
	}
}