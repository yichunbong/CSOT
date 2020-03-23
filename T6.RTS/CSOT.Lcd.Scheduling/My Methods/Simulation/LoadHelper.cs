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
	public static partial class LoadHelper
	{
		public static void OnStartTask(FabLot lot)
		{
			//if (lot != null && lot.LotID == "TH980305N00")
			//    Console.WriteLine("B");

			FabPlanInfo plan = lot.CurrentFabPlan;
			DateTime nowDT = AoFactory.Current.NowDT;

			//TODO : ParallelChamber는 Busy 시점에 TrackInTime 기록처리
			//plan.TrackInTime = nowDT;

			plan.UnitQty = lot.UnitQty;

			var targetEqp = plan.LoadedResource as FabEqp;
			if (targetEqp == null || targetEqp.IsParallelChamber == false)
			{
				plan.TrackInTime = nowDT;
				plan.InQty = lot.UnitQty;
			}

			if (lot.IsRunWipFirstPlan())
			{
				plan.TrackInTime = lot.Wip.LastTrackInTime;
				plan.StartTime = lot.Wip.LastTrackInTime;
				plan.InQty = lot.UnitQty;

				plan.IsInitRunWip = true;
			}

			//사용한 EqpArrangeInfo 정보 기록
			var currEA = lot.CurrentEqpArrange;
			if (currEA != null)
				plan.UsedEqpArrangeInfo = currEA.UsedEqpArrange;

			//초기화
			lot.CurrentEqpArrange = null;

			//LoadGraphMgr.AddWip(lot, nowDT);
		}

		public static void OnEndTask(FabLot lot)
		{
			FabPlanInfo plan = lot.CurrentFabPlan;
			DateTime nowDT = AoFactory.Current.NowDT;

			plan.OutQty = lot.UnitQty;
			plan.TrackOutTime = nowDT;
		}

		#region FabAoEquipment Func

		public static void OnStateChanged(this FabAoEquipment eqp, LoadingStates state,
			FabLot lot = null, bool isDone = false)
		{
			//if (eqp.EqpID == "FHUPH100" && lot != null && lot.LotID == "TH011010N0F")
			//	Console.WriteLine("B");

			DateTime now = eqp.NowDT;

			bool isAheadSetup = false;
			if (IsAhead(eqp, state, now))
			{
				if (state == LoadingStates.SETUP)
					now = eqp.AvailableSetupTime;

				if (state == LoadingStates.PM)
					now = eqp.AvailablePMTime;

				isAheadSetup = true;
			}

			eqp.UpdateLastLoadInfo(now, state);
			eqp.SetLastFabLoadInfo(state, now, lot, isDone, isAheadSetup);

			UpdateAheadSetupInfo(eqp, state, now);

			if (eqp.IsParallelChamber)
			{
				if (state == LoadingStates.BUSY && lot != null)
				{
					FabPlanInfo plan = lot.CurrentFabPlan;
					if (plan.IsInitRunWip == false)
					{
						plan.TrackInTime = now;
						plan.InQty = lot.UnitQty;
					}
				}
			}
			else
			{
				if (state == LoadingStates.BUSY && lot != null)
					lot.CurrentFabPlan.TrackInTime = now;
			}

			OnChamberStateChanged(eqp, lot, state, now, isDone);

			if (state == LoadingStates.IDLERUN || state == LoadingStates.IDLE)
			{
				if (state == LoadingStates.IDLERUN)
				{
					if (eqp.LastIdleRunStartTime == DateTime.MinValue)
						eqp.LastIdleRunStartTime = now;
				}

				if (eqp.LastIdleStartTime == DateTime.MinValue)
					eqp.LastIdleStartTime = now;
			}
			else
			{
				//reset (not idle state)
				eqp.LastIdleRunStartTime = DateTime.MinValue;
				eqp.LastIdleStartTime = DateTime.MinValue;
			}
		}

		//private static void OnChamberStateChangedTemp(FabAoEquipment eqp, FabLot lot, LoadingStates state, DateTime now, bool isDone)
		//{
		//	if (eqp.IsParallelChamber == false)
		//		return;

		//	int count = eqp.SubEqps.Length;
		//	for (int i = 0; i < count; i++)
		//	{
		//		var subEqp = eqp.SubEqps[i];

		//		if (isDone)
		//		{
		//			subEqp.OnStateChanged(eqp, state, lot, isDone);
		//			continue;
		//		}

		//		ChamberInfo chamberInfo;
		//		if (eqp.CheckParallelChamberState(i, state, lot, out chamberInfo))
		//		{
		//			subEqp.OnStateChanged(eqp, state, lot, isDone);

		//			//TODO : 임시처리로직 ParallelChamber 설비의 BUSY 이벤트 누락 발생으로 임시 처리함. (이슈 확인 후 삭제 필요함.)
		//			if (state == LoadingStates.SETUP)
		//			{
		//				Time delay = Time.Max((subEqp.ChamberInfo.SetupEndTime - now), Time.Zero);
		//				if (delay > Time.Zero)
		//				{
		//					object[] args = new object[3] { subEqp, LoadingStates.BUSY.ToString(), lot };
		//					eqp.AddTimeout(delay, SimHelper.OnEqpLoadingStateChanged, args);
		//				}
		//			}
		//		}
		//		else
		//		{
		//			if (state == LoadingStates.BUSY && subEqp.ChamberInfo.Next != null)
		//			{
		//				FabLot itsMe = subEqp.ChamberInfo.Next as FabLot;
		//				if (lot.CurrentPlan == itsMe.CurrentPlan)
		//				{
		//					if (subEqp.ChamberInfo.OutTime > subEqp.SetupEndTime)
		//					{
		//						Time delay = Time.Max((subEqp.ChamberInfo.OutTime - now), Time.Zero);
		//						if (delay > Time.Zero)
		//						{
		//							object[] args = new object[3] { subEqp, LoadingStates.BUSY.ToString(), lot };
		//							eqp.AddTimeout(delay, SimHelper.OnEqpLoadingStateChanged, args);
		//						}
		//					}
		//				}
		//			}
		//			else if (state == LoadingStates.IDLERUN)
		//			{
		//				if (subEqp.ChamberInfo.Current != null)
		//				{
		//					Time delay = Time.Max((subEqp.ChamberInfo.OutTime - now), Time.Zero);
		//					if (delay > Time.Zero)
		//					{
		//						object[] args = new object[3] { subEqp, LoadingStates.IDLERUN.ToString(), lot };
		//						eqp.AddTimeout(delay, SimHelper.OnEqpLoadingStateChanged, args);
		//					}
		//				}
		//			}
		//			else if (state == LoadingStates.SETUP)
		//			{

		//				if (subEqp.ChamberInfo.Current != null)
		//				{
		//					var workInfo = subEqp.ChamberInfo.List.Find(p => (p.Entity as FabLot).CurrentPlan == lot.CurrentPlan);
		//					if (workInfo != null)
		//					{
		//						Time delay = Time.Max((workInfo.SetupStartTime - now), Time.Zero);
		//						if (delay > Time.Zero)
		//						{
		//							object[] args = new object[3] { subEqp, LoadingStates.SETUP.ToString(), lot };
		//							eqp.AddTimeout(delay, SimHelper.OnEqpLoadingStateChanged, args);

		//							subEqp.SetupStartTime = workInfo.SetupStartTime;
		//						}

		//						delay = Time.Max((workInfo.SetupEndTime - now), Time.Zero);
		//						if (delay > Time.Zero)
		//						{
		//							object[] args = new object[3] { subEqp, LoadingStates.BUSY.ToString(), lot };
		//							eqp.AddTimeout(delay, SimHelper.OnEqpLoadingStateChanged, args);

		//							subEqp.SetupEndTime = workInfo.SetupEndTime;
		//						}
		//					}
		//				}
		//			}					
		//		}

		//		//if (eqp.EqpID == "THCVD500")
		//		//{
		//		//    string lotID = "-";
		//		//    if (subEqp.ChamberInfo.Current != null)
		//		//        lotID = (subEqp.ChamberInfo.Current as FabLot).LotID;

		//		//    Logger.MonitorInfo("{0};{1};{2};{3};{4};{5};{6};{7};{8}\t", eqp.NowDT.ToString("HH:mm:ss"), state, lot == null ? "-" : lot.LotID, subEqp.SubEqpID, lotID, subEqp.ChamberInfo.OutTime == Time.Zero ? "-" : subEqp.ChamberInfo.OutTime.ToString(), subEqp.ChamberInfo.Next == null ? "-" : (subEqp.ChamberInfo.Next as FabLot).LotID, subEqp.ChamberInfo.Next == null ? "-" : subEqp.ChamberInfo.NextOutTime.ToString(), subEqp.ChamberInfo.LastUnits);
		//		//}
		//	}
		//}

		private static void OnChamberStateChanged(FabAoEquipment eqp, FabLot lot, LoadingStates state, DateTime now, bool isDone)
		{
			var findSubEqps = isDone ? eqp.GetSubEqpList() : eqp.FindSubEqpsByState(state, lot);

			if (findSubEqps != null && findSubEqps.Count > 0)
			{
				foreach (var subEqp in findSubEqps)
				{
					subEqp.OnStateChanged(eqp, state, lot, isDone);

					//TODO : 임시처리로직 ParallelChamber 설비의 BUSY 이벤트 누락 발생으로 임시 처리함. (이슈 확인 후 삭제 필요함.)
					if (state == LoadingStates.SETUP)
					{
						Time delay = Time.Max((subEqp.ChamberInfo.SetupEndTime - now), Time.Zero);
						if (delay > Time.Zero)
						{
							object[] args = new object[3] { subEqp, LoadingStates.BUSY.ToString(), lot };
							eqp.AddTimeout(delay, SimHelper.OnEqpLoadingStateChanged, args);
						}
					}
				}
			}
		}

		public static void OnChamberStateChanged(this FabSubEqp subEqp, FabAoEquipment eqp, LoadingStates state,
			FabLot lot = null, bool isDone = false)
		{
			if (subEqp == null)
				return;

			subEqp.OnStateChanged(eqp, state, lot, isDone);
		}

		private static void SetLastFabLoadInfo(this FabAoEquipment eqp, LoadingStates state,
			DateTime now, FabLot lot = null, bool isDone = false, bool isAheadSetup = false)
		{
			var infos = eqp.LoadInfos;

			var newInfo = SetLastFabLoadInfo(infos, state, now, lot, isDone, isAheadSetup);

			newInfo.RunMode = eqp.GetCurrentRunMode();
		}

		private static void UpdateLastLoadInfo(this FabAoEquipment eqp, DateTime now, LoadingStates state)
		{
			var lastInfo = eqp.LastLoadInfo;
			if (lastInfo == null)
				return;

			lastInfo.EndTime = now;

			var lastPlan = lastInfo.Target as FabPlanInfo;
			if (lastPlan != null)
			{
				lastPlan.EqpInEndTime = now;
			}

			var infos = eqp.LoadInfos;
			if (lastInfo.IsDummyState())
			{
				infos.Remove(lastInfo);
			}
		}

		private static List<FabSubEqp> FindSubEqpsByState(this FabAoEquipment eqp, LoadingStates state, FabLot lot)
		{
			List<FabSubEqp> list = new List<FabSubEqp>();

			var subEqpList = eqp.SubEqps;
			if (subEqpList == null || subEqpList.Length == 0)
				return list;

			int count = eqp.SubEqps.Length;
			for (int i = 0; i < count; i++)
			{
				var subEqp = eqp.SubEqps[i];

				ChamberInfo chamberInfo;
				if (eqp.CheckParallelChamberState(i, state, lot, out chamberInfo))
					list.Add(subEqp);
			}

			return list;
		}

		public static void OnStateChanged(this FabSubEqp subEqp, FabAoEquipment parent,
			LoadingStates state, FabLot lot = null, bool isDone = false)
		{
			var eqp = parent;
			DateTime now = eqp.NowDT;

			//PM 발생시 IDLE 무시
			bool isIngore = subEqp.UpdateLastLoadInfo(now, state, eqp, isDone);

			if (isIngore && isDone == false)
				return;

			subEqp.SetLastFabLoadInfo(state, now, lot, isDone);

			UpdateAheadSetupInfo(eqp, state, now);

			if (state == LoadingStates.BUSY)
			{
				subEqp.LastPlan = lot.CurrentFabPlan;

				subEqp.LastIdleRunStartTime = DateTime.MinValue;
				subEqp.LastIdleStartTime = DateTime.MinValue;
			}

			if (state == LoadingStates.IDLE)
				subEqp.LastIdleStartTime = now;

			if (state == LoadingStates.IDLERUN)
				subEqp.LastIdleRunStartTime = now;


			subEqp.ChangeCurrentRunMode(lot);

		}

		private static void SetLastFabLoadInfo(this FabSubEqp subEqp, LoadingStates state,
			DateTime now, FabLot lot = null, bool isDone = false)
		{
			var infos = subEqp.LoadInfos;

			var newInfo = SetLastFabLoadInfo(infos, state, now, lot, isDone);

			newInfo.SubEqpID = subEqp.SubEqpID;

			if (state == LoadingStates.BUSY)
			{
				newInfo.UnitQty = subEqp.ChamberInfo.CurrentUnits;
				newInfo.RunMode = subEqp.SubEqpGroup.CurrentRunMode;
			}
		}

		private static bool UpdateLastLoadInfo(this FabSubEqp subEqp, DateTime now, LoadingStates state, FabAoEquipment eqp, bool isDone)
		{
			var lastInfo = subEqp.LastLoadInfo;
			if (lastInfo == null)
				return false;

			if (SimHelper.IgnoreStateChange(eqp, state, isDone))
				return true;

			lastInfo.EndTime = now;

			var lastPlan = lastInfo.Target as FabPlanInfo;
			if (lastPlan != null)
			{
				lastPlan.EqpInEndTime = now;
			}

			var infos = subEqp.LoadInfos;
			if (lastInfo.IsDummyState())
				infos.Remove(lastInfo);

			//TODO : 임시처리 BUSY 중복 제외
			if (lastInfo.IsDummyState_Busy())
				infos.Remove(lastInfo);

			return false;
		}

		private static bool IsDummyState_Busy(this FabLoadInfo info)
		{
			if (info.State != LoadingStates.BUSY)
				return false;

			return info.StartTime == info.EndTime;
		}

		private static FabLoadInfo SetLastFabLoadInfo(List<FabLoadInfo> infos, LoadingStates state,
			DateTime now, FabLot lot = null, bool isDone = false, bool isAheadSetup = false)
		{
			FabLoadInfo newInfo = new FabLoadInfo();
			newInfo.State = state;
			newInfo.StartTime = now;

			if (lot != null && state == LoadingStates.BUSY)
				lot.CurrentFabPlan.EqpLoadInfo = newInfo;

			//PlanEndTime 마감처리
			if (isDone)
				newInfo.EndTime = now;

			FabPlanInfo target = null;
			if (lot != null && IsRunState(state)) //true = BUSY or SETUP
			{
				target = lot.CurrentFabPlan;
				newInfo.Target = target;

				if (state == LoadingStates.BUSY)
					newInfo.UnitQty = target.UnitQty;

				if (state == LoadingStates.SETUP)
				{
					if (isAheadSetup)
						newInfo.StateInfo = "AHEAD";

					if (target.IsIdleSetup)
						newInfo.StateInfo = "IDLE_SETUP";

					if (isAheadSetup && target.IsIdleSetup)
						newInfo.StateInfo = "AHEAD_IDLE_SETUP";

					var eqp = ResHelper.GetFabAoEquipment(lot.CurrentPlan.LoadedResource.ResID);
					if (eqp != null && eqp.IsAcidConst && eqp.AcidDensity.IsSetupMark)
					{
						if (isAheadSetup)
							newInfo.StateInfo = "AHEAD_ACID";
						else
							newInfo.StateInfo = "ACID";

						//용액교체 마크 해제
						AcidMaster.SetSetupMark(eqp, false);
					}
				}
			}

			if (state == LoadingStates.PM)
			{
				if (isAheadSetup)
				{
					newInfo.StateInfo = "AHEAD";
				}
			}

			infos.Add(newInfo);

			return newInfo;
		}

		private static bool IsDummyState(this FabLoadInfo info)
		{
			if (IsIgnorableState(info.State) == false)
				return false;

			return info.StartTime == info.EndTime;
		}

		public static bool IsIgnorableState(LoadingStates state)
		{
			return state == LoadingStates.IDLE || state == LoadingStates.IDLERUN;
		}

		private static bool IsRunState(LoadingStates state)
		{
			return state == LoadingStates.BUSY || state == LoadingStates.SETUP;
		}

		private static bool IsAhead(FabAoEquipment eqp, LoadingStates state, DateTime now)
		{
			if (state == LoadingStates.SETUP)
			{
				if (eqp.AvailableSetupTime < now)
					return true;
			}

			if (state == LoadingStates.PM)
			{
				if (eqp.AvailablePMTime < now)
					return true;
			}

			return false;
		}

		private static void UpdateAheadSetupInfo(FabAoEquipment eqp, LoadingStates state, DateTime now)
		{
			eqp.AvailableSetupTime = DateTime.MaxValue;
			eqp.AvailablePMTime = DateTime.MaxValue;

			//TODO : ParallelChamber는 제외 처리 함
			if (eqp.IsParallelChamber)
				return;

			//AheadSetup - Setup가능시간 설정 (Idle시작시간 기록)
			if (state == LoadingStates.IDLE || state == LoadingStates.IDLERUN)
				eqp.AvailableSetupTime = now;
		}

		#endregion FabAoEquipment Func
	}
}
