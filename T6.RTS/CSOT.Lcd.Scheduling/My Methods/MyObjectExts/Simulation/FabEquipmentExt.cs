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
	public static partial class FabEquipmentExt
	{
		#region FabEqpFunc

		public static void SetInitEqpStatus(this FabEqp targetEqp, FabAoEquipment eqp)
		{
			EqpStatusInfo info = targetEqp.StatusInfo;
			if (info == null)
				return;

			eqp.ContinuousQty = info.LastContinuousQty;

			//down 이후 setup 필요 (2020.01.14 - by.liujian(유건))
			//if (targetEqp.State == ResourceState.Down && info.OrigineStatus != MesEqpStatus.IDLE)
			//    return;

			var status = info.MesStatus;

			if (status == MesEqpStatus.RUN || status == MesEqpStatus.Set_Up
				|| status == MesEqpStatus.E_RUN || info.OrigineStatus == MesEqpStatus.IDLE
				|| info.OrigineStatus == MesEqpStatus.DOWN)
			{
				eqp.LastPlan = SimHelper.CreateInitLastPlan(info);
			}
		}

		public static bool HasMainRunStep(this FabEqp eqp)
		{
			var list = eqp.MainRunSteps;
			if (list != null && list.Count > 0)
				return true;

			return false;
		}

		public static bool IsMainRunStep(this FabEqp eqp, string stepID)
		{
			if (eqp.HasMainRunStep() == false)
				return false;

			var list = eqp.MainRunSteps;
			var find = list.Find(t => t.StepID == stepID);
			if (find != null)
				return true;

			return false;
		}

		#endregion FabEqpFunc

		//internal static void UpdateLastLoading(this FabAoEquipment eqp, LoadStates state)
		//{
		//    var last = eqp.LastPlan;
		//    if (last != null && (state == LoadStates.IDLERUN || state == LoadStates.IDLE))
		//    {
		//        var planInfo = last as FabPlanInfo;
		//        if (planInfo.EqpInEndTime == DateTime.MinValue)
		//            planInfo.EqpInEndTime = eqp.NowDT;
		//    }
		//}

		internal static bool UseSameMaskTool(this FabAoEquipment aeqp, FabLot lot)
		{
			return false;
		}

		internal static Time GetIdleTime(this FabAoEquipment eqp)
		{
			if (eqp.IsParallelChamber)
				return GetIdleRunTime(eqp);

			if (eqp.LastIdleStartTime == DateTime.MinValue)
				return Time.Zero;

			Time idleTIme = eqp.NowDT - eqp.LastIdleStartTime;

			return idleTIme;
		}

		internal static Time GetIdleRunTime(this FabAoEquipment eqp)
		{
			if (eqp.IsParallelChamber)
				return GetChamberIdleRunTime(eqp);

			if (eqp.LastIdleRunStartTime == DateTime.MinValue)
				return Time.Zero;

			Time idleRunTIme = eqp.NowDT - eqp.LastIdleRunStartTime;

			return idleRunTIme;
		}

		private static Time GetChamberIdleRunTime(FabAoEquipment eqp)
		{
			if (eqp.TriggerSubEqp == null)
				return Time.Zero;

			DateTime lastIdleStartTime = eqp.TriggerInfo.TriggerSubEqp.LastIdleRunStartTime;

			if (lastIdleStartTime == DateTime.MinValue)
				return Time.Zero;

			Time idleRunTime = eqp.NowDT - lastIdleStartTime;

			return idleRunTime;
		}

		private static Time GetChamberIdleTime(FabAoEquipment eqp)
		{
			if (eqp.TriggerSubEqp == null)
				return Time.Zero;

			DateTime lastIdleStartTime = eqp.TriggerInfo.TriggerSubEqp.LastIdleStartTime;

			if (lastIdleStartTime == DateTime.MinValue)
				return Time.Zero;

			Time idleRunTime = eqp.NowDT - lastIdleStartTime;

			return idleRunTime;
		}

		//#region Load History 관련
		//internal static bool RequireMerge(this FabAoEquipment eqp)
		//{
		//    if (eqp.CurrentLoadInfo != null && eqp.CurrentLoadInfo.IsEmpty == false)
		//        return eqp.CurrentLoadInfo.StartTime == eqp.NowDT && eqp.CurrentLoadInfo.IsIgnoreableState;

		//    return false;
		//}


		///// <summary>
		///// 임의로 LoadPlan을 삽입을 위한 함수입니다.
		///// </summary>
		//internal static void AddLoadPlan(this FabAoEquipment eqp, LoadStates state, DateTime date)
		//{
		//    FabLoadInfo info = CreateHelper.CreateLoadInfo(null, date, state, eqp.MaskID, Constants.NULL_ID);
		//    FabLoadInfo last;

		//    if (eqp.LastLoadInfo == null)
		//    {
		//        eqp.LoadInfos = new List<FabLoadInfo>();
		//        eqp.AddLoadPlan(info);
		//    }
		//    else
		//    {
		//        last = eqp.LastLoadInfo;
		//        last.EndTime = date;
		//        eqp.AddLoadPlan(info);
		//    }

		//}


		///// <summary>
		///// 설비에 Plan을 입력합니다.
		///// </summary>
		///// <param name="eqp"></param>
		///// <param name="info"></param>
		//internal static void AddLoadPlan(this FabAoEquipment eqp, FabLoadInfo info)
		//{
		//    if (eqp.LoadInfos == null)
		//        eqp.LoadInfos = new List<FabLoadInfo>();

		//    eqp.LoadInfos.Add(info);
		//}


		///// <summary>
		///// 설비의 StateChange 최초 호출시 사용됩니다.
		///// </summary>
		//public static void AddNewLoadPlan(this FabAoEquipment eqp, FabLoadInfo info, FabLot lot)
		//{
		//    if (lot != null)
		//        info.AddPlanInfo(lot.CurrentFabPlan);

		//    eqp.AddLoadPlan(info);
		//    eqp.CurrentLoadInfo = info;
		//}


		///// <summary>
		///// 이전 Plan을 종료하고 현재Plan을 삽입합니다.
		///// </summary>
		///// <param name="eqp"></param>
		///// <param name="last"></param>
		///// <param name="info"></param>
		///// <param name="lot"></param>
		//public static void ChangeLoadPlan(this FabAoEquipment eqp, FabLoadInfo last, FabLoadInfo info, FabLot lot)
		//{
		//    if (lot != null)
		//        info.AddPlanInfo(lot.CurrentFabPlan);

		//    last.EndTime = eqp.NowDT;
		//    eqp.AddLoadPlan(info);
		//} 
		//#endregion



		#region PM 관련
		public static bool IsPmPeriod(this FabEqp eqp, DateTime date)
		{
			List<PMSchedule> list = eqp.GetPMList();

			if (list == null || list.Count == 0)
				return false;

			foreach (PMSchedule item in list)
			{
				if (item.IsPmPeriodSection(date))
					return true;
			}

			return false;

		}


		private static bool IsPmPeriodSection(this PMSchedule pm, DateTime date)
		{
			return pm.StartTime <= date && date <= pm.EndTime;
		}


		public static List<PMSchedule> GetPMList(this FabEqp eqp)
		{
			if (eqp.PMList == null)
				eqp.PMList = new List<PMSchedule>();

			return eqp.PMList;
		}


		public static void InitPM(this FabEqp eqp)
		{
			if (eqp.PMList == null)
				eqp.PMList = new List<PMSchedule>();

			List<PMSchedule> pmlist = DownMaster.GetPmList(eqp);

			if (eqp.State == ResourceState.Down && eqp.StatusInfo.MesStatus == MesEqpStatus.PM)
			{
				//EqpStatus PM 상태 반영
				FabPMSchedule initPM = CreateHelper.CreateFabPMSchedule(eqp.StatusInfo.StartTime, (int)eqp.StatusInfo.Duration, ScheduleType.PM, 0, 0);
				eqp.PMList.Add(initPM);

				//설비상태가 PM일경우 24시간 이내 PM은 무시
				DateTime pmFenceDate = ModelContext.Current.StartTime.AddDays(1);
				foreach (PMSchedule item in pmlist)
				{
					if (item.StartTime < pmFenceDate)
						continue;

					eqp.PMList.Add(item);
				}
			}
			else
			{
				eqp.PMList.AddRange(pmlist);
			}
		}

		public static bool OnParallelChamberPM(this FabAoEquipment eqp, PMSchedule pm, DownEventType det)
		{
			if (pm == null || eqp.IsParallelChamber || string.IsNullOrEmpty(pm.ComponentID))
				return false;

			var chamberID = pm.ComponentID;
			if (string.IsNullOrEmpty(chamberID))
				chamberID = Constants.NULL_ID;

			var cproc = eqp.FirstProcess<AoChamberProc2>();
			if (det == DownEventType.End)
			{
				cproc.Live(chamberID);
				eqp.WriteHistoryAfterBreak();
				eqp.SetModified();
			}
			else
			{
				cproc.Die(chamberID, pm.EndTime);
				eqp.WriteHistory(LoadingStates.PM);
			}
			return true;
		}


		#endregion


		public static void ClearFromToEqp(this FabEqp eqp)
		{
			eqp.FromMapEqps.Clear();
			eqp.ToMapEqps.Clear();
		}

		public static void AddToEqp(this FabEqp eqp, FabEqp target)
		{
			if (eqp.ToMapEqps == null)
				eqp.ToMapEqps = new List<FabEqp>();

			eqp.ToMapEqps.Add(target);
		}

		public static void AddFromEqp(this FabEqp eqp, FabEqp target)
		{
			if (eqp.FromMapEqps == null)
				eqp.FromMapEqps = new List<FabEqp>();

			eqp.FromMapEqps.Add(target);
		}

		public static float CheckAheadSetupTime(this FabAoEquipment eqp, float setupTime, FabLot lot)
		{
			DateTime now = eqp.NowDT;
			float addIdleSetupTime = 0f;

			string stepID = lot.CurrentStepID;
			string productID = lot.CurrentProductID;

			if (setupTime > 0 && eqp.AvailableSetupTime < now)
			{
				DateTime availableTime = GetAvailableSetupTime(eqp, now);
				DateTime aheadSetupStart = LcdHelper.Max(availableTime, (now.AddMinutes(-setupTime)));

				//IDLE에 따른 추가 Setup시간 반영 (AheadSetup에 따라 Idle 시간이 판단됨, 추가 IDLE_SETUP TIME은 Ahead미반영)
				addIdleSetupTime = SetupMaster.GetAdditionalSetupTime(eqp, stepID, productID, aheadSetupStart);
				if (addIdleSetupTime > 0)
					lot.CurrentFabPlan.IsIdleSetup = true;

				if (aheadSetupStart < now)
				{
					setupTime = setupTime - (float)(now - aheadSetupStart).TotalMinutes;
					eqp.AvailableSetupTime = aheadSetupStart;

					if (setupTime == 0 && addIdleSetupTime == 0)
						eqp.OnStateChanged(LoadingStates.SETUP, lot);
				}
				else
					eqp.AvailableSetupTime = DateTime.MaxValue;

				return setupTime + addIdleSetupTime;
			}

			//AheadSetup이 아닐 경우(또는 Setup시간이 0 이지만 추가 Setup이 있을 경우 Setup을 함)
			//기존: Setup이 필요하지만 Setup시간이 0이면 Setup을 하지 않음
			//변경 : Setup이 필요하지만 Setup시간이 0 이어도 추가 Setup 정보가 있을 경우 Setup을 함.
			addIdleSetupTime = SetupMaster.GetAdditionalSetupTime(eqp, stepID, productID, now);
			if (addIdleSetupTime > 0)
				lot.CurrentFabPlan.IsIdleSetup = true;

			setupTime += addIdleSetupTime;

			return setupTime;
		}

		private static DateTime GetAvailableSetupTime(FabAoEquipment eqp, DateTime now)
		{
			DateTime availableTime = eqp.AvailableSetupTime;

			//마스크 사용시 
			if (eqp.InUseMask != null)
			{
				//실제Setup시 Seize된 툴이 들어옴. 현재 이전의 LoadInfo을 참조해야함.
				if (eqp.InUseMask.LoadInfos.Count > 1)
				{
					MaskLoadInfo loadInfo = eqp.InUseMask.LoadInfos[eqp.InUseMask.LoadInfos.Count - 2];
					if (loadInfo.EqpID != eqp.EqpID)
					{
						availableTime = LcdHelper.Max(eqp.AvailableSetupTime, (DateTime)loadInfo.AvailableTime);

						if (availableTime > now)
							availableTime = now;
					}
				}
			}

			return availableTime;
		}


		//CHECK : jung  : IsLastPlan 통합필요
		public static int GetFilteredWipQty(this FabAoEquipment eqp, FabPlanInfo plan)
		{
			if (plan == null)
				return 0;

			int qty = 0;

			foreach (var item in eqp.EqpDispatchInfo.FilterInfos.Values)
			{
				foreach (FabLot lot in item.FilterWips)
				{
					//TODO : OwnerType 관련 리팩토링 필요
					if (plan.FabStep != lot.CurrentFabStep)
						continue;

					if (plan.ProductID != lot.CurrentProductID)
						continue;

					if (plan.ProductVersion != lot.CurrentProductID)
						continue;

					if (plan.OwnerType != lot.OwnerType)
						continue;

					qty += lot.UnitQty;
				}
			}

			return qty;
		}

		public static string GetCurrentRunMode(this FabAoEquipment eqp)
		{
			if (eqp == null)
				return null;

			if (eqp.IsParallelChamber)
			{
				var subEqp = eqp.TriggerSubEqp;
				if (subEqp != null)
					return subEqp.SubEqpGroup.CurrentRunMode;
			}

			return null;
		}
	}
}
