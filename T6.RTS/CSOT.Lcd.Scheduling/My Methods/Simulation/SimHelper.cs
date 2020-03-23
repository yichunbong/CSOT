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
namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class SimHelper
    {
        public static bool firstFireAtOnHour = true;

        public static bool IsTftRunning { get { return InputMart.Instance.SimulationRunType == SimRunType.TFT_CF; } }

        public static bool IsCellRunning { get { return InputMart.Instance.SimulationRunType == SimRunType.CELL; } }


        public static Dictionary<string, List<FabStdStep>> _dspEqpGroup = new Dictionary<string, List<FabStdStep>>();


        internal static FabLot ToFabLot(this IHandlingBatch hb)
        {
            if (hb == null)
                return null;

            return hb.Sample as FabLot;
        }

        internal static bool IsFirstPlan(this FabLot lot)
        {
            if (lot == null)
                return false;

            return lot.PreviousPlan == null;
        }

        internal static bool IsRunWipFirstPlan(this FabLot lot)
        {
            if (lot == null)
                return false;

            return lot.Wip.IsRun && lot.IsFirstPlan();
        }

        //internal static bool IsWip(this FabLot lot)
        //{
        //    //return lot.Wip.IsInputLot == false;
        //    return lot.IsWipHandle;
        //}

        internal static FabAoEquipment ToFabAoEquipment(this AoEquipment aeqp)
        {
            return aeqp as FabAoEquipment;

        }

        internal static FabEqp ToFabEqp(this FabAoEquipment aeqp)
        {
            return aeqp.Target as FabEqp;

        }
        
        internal static MesEqpStatus GetEqpStatus(string status)
        {
            MesEqpStatus st = LcdHelper.ToEnum(status, MesEqpStatus.NONE);

            if (st == MesEqpStatus.NONE)
                return MesEqpStatus.DOWN;

            switch (status.ToUpper())
            {
                case "E-RUN":
                    return MesEqpStatus.E_RUN;

                case "W-CST":
                    return MesEqpStatus.W_CST;
            }

            return st;
        }

        internal static SimEqpType GetSimType(string simType)
        {
            var type = LcdHelper.ToEnum(simType, SimEqpType.None);

            if (type == SimEqpType.None)
            {
                string upper = simType.ToUpper();

                if (upper == SimEqpType.Inline.ToString().ToUpper())
                    return SimEqpType.Inline;

                if (upper == SimEqpType.Table.ToString().ToUpper())
                    return SimEqpType.Table;

                if (upper == SimEqpType.Chamber.ToString().ToUpper())
                    return SimEqpType.Chamber;

                if (upper == SimEqpType.ParallelChamber.ToString().ToUpper())
                    return SimEqpType.ParallelChamber;

                if (upper == SimEqpType.LotBatch.ToString().ToUpper())
                    return SimEqpType.LotBatch;

                if (upper == SimEqpType.BatchInline.ToString().ToUpper())
                    return SimEqpType.BatchInline;

                if (upper == SimEqpType.Bucket.ToString().ToUpper())
                    return SimEqpType.Bucket;

                if (upper == SimEqpType.UnitBatch.ToString().ToUpper())
                    return SimEqpType.UnitBatch;
            }

            return type;

        }


        //internal static LoadingStates GetLoadingState(LoadStates loadState)
        //{
        //    LoadingStates states = LoadingStates.IDLE;

        //    switch (loadState)
        //    {
        //        case LoadStates.SETUP:
        //            states = LoadingStates.SETUP;
        //            break;

        //        case LoadStates.BUSY:
        //            states = LoadingStates.BUSY;
        //            break;

        //        case LoadStates.IDLERUN:
        //            states = LoadingStates.IDLERUN;
        //            break;

        //        case LoadStates.IDLE:
        //            states = LoadingStates.IDLE;
        //            break;

        //        case LoadStates.PM:
        //            states = LoadingStates.PM;
        //            break;

        //        case LoadStates.DOWN:
        //            states = LoadingStates.DOWN;
        //            break;

        //        case LoadStates.WAIT_SETUP:
        //            states = LoadingStates.WAIT_SETUP;
        //            break;

        //        case LoadStates.RENT:
        //            states = LoadingStates.PM;
        //            break;
        //    }

        //    return states;
        //}

        //internal static LoadStates GetLoadState(LoadingStates states)
        //{
        //    LoadStates loadState = LoadStates.IDLE;

        //    switch (states)
        //    {
        //        case LoadingStates.BUSY:
        //            loadState = LoadStates.BUSY;
        //            break;
        //        case LoadingStates.DOWN:
        //            loadState = LoadStates.DOWN;
        //            break;

        //        case LoadingStates.IDLE:
        //            loadState = LoadStates.IDLE;
        //            break;

        //        case LoadingStates.IDLERUN:
        //            loadState = LoadStates.IDLERUN;
        //            break;

        //        case LoadingStates.PM:
        //            loadState = LoadStates.PM;
        //            break;

        //        case LoadingStates.SETUP:
        //            loadState = LoadStates.SETUP;
        //            break;

        //        case LoadingStates.WAIT_SETUP:
        //            loadState = LoadStates.WAIT_SETUP;
        //            break;

        //    }

        //    return loadState;
        //}

        public static List<string> GetNowAvailableChambers(AoEquipment aeqp, FabLot lot, DateTime now)
        {
            if (aeqp.IsParallelChamber == false)
                return null;

            //AoChamberProc2 가 Parallel Chamber임
            AoChamberProc2 proc = aeqp.FirstProcess<AoChamberProc2>();

            List<string> result = new List<string>();

            foreach (ChamberInfo chamber in proc.Chambers)
            {
                //chamber 진행여부(chamber.Current 는 현재 진행인 상태(currently in process)를 나타냄)
                if (chamber.Current != null)
                    continue;

                //chamber.GetrAvailableTime 이 미래이면 현재 가동중, 현재(now) 이면 idle인 상태 
                if (chamber.GetAvailableTime() <= now)
                {
                    result.Add(chamber.Label);
                }
            }

            return result;
        }

        public static void OnEqpOutBuffer(object sender, object args)
        {
            object[] arr = args as object[];
            if (arr == null || arr.Length != 2)
                return;

            AoEquipment aeqp = arr[0] as AoEquipment;
            if (aeqp == null)
                return;

            IHandlingBatch hb = arr[1] as IHandlingBatch;
            if (hb == null)
                return;

            aeqp.AddOutBuffer(hb);
        }

        public static void OnEqpLoadingStateChanged(object sender, object args)
        {
            object[] arr = args as object[];
            if (arr == null || arr.Length != 3)
                return;

            FabSubEqp subEqp = arr[0] as FabSubEqp;
            if (subEqp == null)
                return;

            string stateName = arr[1] as string;

            LoadingStates state;
            if (Enum.TryParse(stateName, out state) == false)
                return;

            FabLot lot = arr[2] as FabLot;

			if (subEqp.SubEqpID.StartsWith("THCVD9") && lot.LotID == "TH9A0759N01")
				Console.WriteLine();

            var eqp = AoFactory.Current.GetEquipment(subEqp.Parent.EqpID) as FabAoEquipment;
            subEqp.OnStateChanged(eqp, state, lot);
        }

        internal static FabPlanInfo CreateInitLastPlan(EqpStatusInfo info)
        {
            string shopID = info.LastShopID;
            FabProduct prod = BopHelper.FindProduct(shopID, info.LastProduct);

            if (prod == null || prod.Process == null)
                return null;

            FabStep step = prod.Process.FindStep(info.LastStep) as FabStep;
            if (step == null)
                return null;

            FabPlanInfo plan = new FabPlanInfo(step);

			plan.ShopID = shopID;
            plan.Product = prod;
            plan.ProductID = prod.ProductID;
            plan.ProductVersion = info.LastProductVer;
            plan.OwnerType = info.LastOwnerType;         

            //TODO : last OwnerID
            //plan.OwnerID = null;

            return plan;
        }

        internal static bool IsMaskConst(FabLot lot)
        {
            if (lot.CurrentFabStep == null)
                return false;

            return IsMaskConst(lot.CurrentFabStep.StdStep);
        }

        internal static bool IsMaskConst(FabStdStep step)
        {
            if (InputMart.Instance.GlobalParameters.ApplySecondResource && step.IsUseMask)
                return true;

            return false;
        }


        /// <summary>
        /// 같은 설비 그룹군 내에 동일 제품을 생산하는 설비가 있는지?
        /// </summary>
        /// <param name="withMe">자신의 설비도 포할할지 여부</param>
        public static bool IsAnyWorking(FabAoEquipment baseEqp, FabLot lot, bool withMe = false)
        {
            int cnt = HowManyWorking(baseEqp, lot);

            return cnt > 0;
        }

        /// <summary>
        /// 같은 설비 그룹군 내에서 동일한 제품을 생산하는 설비 댓수
        /// </summary>
        /// <param name="withMe">자신의 설비도 포함하여 카운트할지 여부</param>
        public static int HowManyWorking(FabAoEquipment baseEqp, FabLot lot, bool withMe = false)
        {
            List<FabAoEquipment> list = ResHelper.GetEqpsByDspEqpGroup(baseEqp.DspEqpGroupID);

            int cnt = 0;
            foreach (var eqp in list)
            {
                if (withMe == false && eqp.EqpID == baseEqp.EqpID)
                    continue;

                if (eqp.IsLastPlan(lot))
                    cnt++;
            }

            return cnt;
        }



        internal static void AddDspEqpGroup(FabStdStep stdStep)
        {
            if (string.IsNullOrEmpty(stdStep.DspEqpGroup))
                return;

            List<FabStdStep> list;
            if(_dspEqpGroup.TryGetValue(stdStep.DspEqpGroup, out list) == false)
                _dspEqpGroup.Add(stdStep.DspEqpGroup, list = new List<FabStdStep>());

            list.Add(stdStep);

        }

        internal static List<FabStdStep> GetDspEqpSteps(string dspEqpGroup)
        {
            if (string.IsNullOrEmpty(dspEqpGroup))
                return new List<FabStdStep>();

            List<FabStdStep> list;
            _dspEqpGroup.TryGetValue(dspEqpGroup, out list);

            if (list == null)
                return new List<FabStdStep>();

            return list;
        }

		/// <summary>
		/// PM/Down 발생시 동일시간에 이벤트가 PM -> IDLE or IDLERun 순서로 발생하여
		/// Eqp Load 이력이 제대로 기록되지 않음.
		/// PM 발생 직후 IDLE/IDELRUN을 무시하는 코드임.
		/// PM이 종료 될 경우 Loader는 블럭이 해제되며 동일 시간에 PM-> IDLE 순서로 이벤트 발생함.
		/// 이때는 IDLE을 Eqp Load 이력을 기록하므로서 PM이 종료이력이 기록됨.
		/// </summary>
		internal static bool IgnoreStateChange(AoEquipment aeqp, LoadingStates state, bool isDone = false)
        {
			if (isDone)
				return false;

			//PM/Down 발생시 Loader가 블럭됨.
			if (aeqp.Loader.IsBlocked())
            {
                if (IsIgnoreStateForPM(state))
                    return true;
            }

            return false;
        }

        internal static bool IgnoreStateChange(FabLoadInfo lastInfo, LoadingStates state)
        {
            if (lastInfo.State != LoadingStates.PM)
                return false;

            if (IsIgnoreStateForPM(state))
                return true;

            return false;
        }

        private static bool IsIgnoreStateForPM(LoadingStates state)
        {
            if (state == LoadingStates.IDLE || state == LoadingStates.IDLERUN)
                return true;

            return false;

        }
    }
}
