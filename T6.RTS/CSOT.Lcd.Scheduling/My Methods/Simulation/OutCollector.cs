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
using Mozart.SeePlan;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class OutCollector
    {
        static List<DateTime> _collectsStepWips;
        static List<LotHistory> _lotHist;
        static List<FabLot> _lotList;
        static List<FabLot> _inputLotList;

        private static bool IsOnStart { get; set; }
        private static string TimeKey { get; set; }
        private static DateTime TimeKeyDate { get; set; }


        private static string EVENTNAME = "INSERT";
        private static string EVENTUSER = "RTS";

        private static DateTime PlanStartTime = ModelContext.Current.StartTime;

        internal static void OnStart()
        {
            _collectsStepWips = new List<DateTime>();
            _lotHist = new List<LotHistory>();
            _lotList = new List<FabLot>();
            _inputLotList = new List<FabLot>();

            if (IsOnStart == false)
            {
                OutCollector.TimeKeyDate = DateTime.Now;
                OutCollector.TimeKey = TimeKeyDate.ToString("yyyyMMddHHmmssffffff");

                IsOnStart = true;
            }
        }

        internal static void OnDone(AoFactory aoFactory)
        {
            //RTD_IF : Load
            WriteEqpPlan(aoFactory);

            //RTD_IF : LotRecord/Unload
            WriteLotHistory();

            WriteRtsPcPlanDay();

            //RTD_IF : MaskTransfer
            WriteMaskHistory();

            WriteStepMove();

            WriteEqpArrangeHistory();

            WriteRelasePlan();

            WriteOutProfileHistory();
        }

        internal static void AddLotforInit(FabLot lot)
        {
            _lotList.Add(lot);
        }

        private static void WriteEqpArrangeHistory()
        {
            bool isCellRunning = SimHelper.IsCellRunning;

            var infos = EqpArrangeMaster.Infos.Values.ToList();
            var list = infos.FindAll(t => t.StdStep.IsCellShop == isCellRunning);

            foreach (var info in list)
            {
                string key = info.Key;

                //Default Group
                WriteEqpArrangeHistory(key, info, info.DefaultGroups, "DEFAULT");

                //Etc Group
                WriteEqpArrangeHistory(key, info, info.EtcGroups, "ETC");
            }
        }

        private static void WriteEqpArrangeHistory(string key, EqpArrangeSet setInfo,
            Dictionary<string, List<EqpArrangeInfo>> groups, string infoType)
        {
            if (groups == null || groups.Count == 0)
                return;

            foreach (var list in groups.Values)
            {
                int seq = 0;
                foreach (var item in list)
                {
                    WriteEqpArrangeHistory(key, setInfo, item, infoType, seq++);
                }
            }
        }

        private static void WriteEqpArrangeHistory(string key, EqpArrangeSet item,
            EqpArrangeInfo info, string infoType, int seq)
        {
            Outputs.EqpArrangeHistory row = new EqpArrangeHistory();

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = item.StdStep.FactoryID;
            row.SHOP_ID = item.StdStep.ShopID;

            row.KEY_INFO = key;
            row.STEP_ID = item.StepID;
            row.EQP_ID = item.EqpID;
            row.EQP_GROUP_ID = item.TargetEqp.EqpGroup;
            row.PRODUCT_ID = item.ProductID;
            row.PRODUCT_VERSION = item.ProductVer;
            row.DEFAULT_LIMIT_TYPE = item.DefaultArrange;
            row.IS_FIXED_PRODUT_VERSION = item.IsFixedProductVer.ToStringYN();
            row.IS_SUB_EQP = item.IsSubEqp.ToStringYN();

            row.INFO_TYPE = infoType;

            row.ARR_SEQ = seq;
            row.ARR_LIMIT_TYPE = info.LimitType;
            row.ARR_ACTIVE_TYPE = info.ActivateType.ToString();
            row.ARR_STEP_ID = info.StepID;
            row.ARR_PRODUCT_ID = info.ProductID;
            row.ARR_PRODUCT_VERSION = info.ProductVer;
            row.ARR_MASK_ID = info.MaskID;
            row.ARR_LIMIT_QTY = info.LimitQty;
            row.ARR_ACT_QTY = info.InitActQty;
            row.ARR_DUE_DATE = info.DueDate;
            row.ARR_DAILY_MODE = info.IsDailyMode.ToStringYN();

            OutputMart.Instance.EqpArrangeHistory.Add(row);
        }

        private static void WriteEqpPlan(AoFactory aoFactory)
        {
            DateTime nowDT = aoFactory.NowDT;

            foreach (FabAoEquipment eqp in aoFactory.Equipments.Values)
            {
                //Plan End처리
                PostProcessing(eqp, nowDT);

                //OutPut 기록
                OutCollector.WriteEqpPlan(eqp as FabAoEquipment);
            }
        }

        private static void PostProcessing(FabAoEquipment eqp, DateTime nowDT)
        {
            //Eqp.LoadPlan의 마지막 IDLE 추가
            if (eqp.LoadingState != LoadingStates.IDLE)
            {
                eqp.WriteHistory(LoadingStates.IDLE, null);
                eqp.OnStateChanged(LoadingStates.IDLE, null, true);
            }
        }

		private static void WriteLotHistory()
		{
			Dictionary<string, List<FabPlanInfo>> dic = new Dictionary<string, List<FabPlanInfo>>();

			foreach (FabLot lot in _lotList)
			{
				WriteLotHistory(lot);
				Write_Rtd_LotRecord(lot);

				ClassyfyLotPlanByEqpID(dic, lot);
			}

			foreach (KeyValuePair<string, List<FabPlanInfo>> item in dic)
			{
				item.Value.Sort(new CompareHelper.RtsUnloadLotComparer());

				int groupNo = 0;
				int seqNo = 1;

				FabPlanInfo prev = null;
				foreach (var plan in item.Value)
				{
					CheckCount(prev, plan, ref groupNo, ref seqNo);

					Write_RTD_RtsPlanningUnload(item.Key, plan, groupNo, seqNo);

					prev = plan;

				}
			}
		}

		private static void CheckCount(FabPlanInfo prev, FabPlanInfo current, ref int groupNo, ref int seqNo)
		{
			bool isChange = IsChange(prev, current);

			if(isChange)
			{
				groupNo++;
				seqNo = 1;
			}
			else
			{
				seqNo++;
			}
		}

		private static void Write_RTD_RtsPlanningUnload(string eqpID, FabPlanInfo plan, int groupNo, int seqNo)
		{
			Outputs.RTD_RtsPlanningUnload row = new RTD_RtsPlanningUnload();

			FabEqp targetEqp = plan.LoadedResource as FabEqp;
			FabLoadInfo item = plan.EqpLoadInfo;

			row.VERSION = ModelContext.Current.VersionNo;
			row.TIMEKEY = TimeKey;

			row.SITE = targetEqp.FactoryID;
			row.FACTORYNAME = targetEqp.ShopID;

			row.SOURCEMACHINENAME = eqpID;
			row.DESTINATIONMACHINENAME = Constants.NULL_ID;

			row.MACHINEGROUPNAME = targetEqp.EqpGroup;

			if (item != null)
			{
				row.MACHINESTATUS = item.State.ToString();

				row.STARTTIME = item.StartTime.DbNullDateTime();
				row.ENDTIME = item.EndTime.DbNullDateTime();
			}
			else
			{
				row.MACHINESTATUS = "BUSY";

				row.STARTTIME = plan.StartTime.DbNullDateTime();
				row.ENDTIME = plan.EndTime.DbNullDateTime();
			}

			row.EVENTTIME = TimeKeyDate;
			row.EVENTNAME = EVENTNAME;
			row.EVENTUSER = EVENTUSER;

			row.INPUTPRIORITY = groupNo;
			row.INPUTORDER = seqNo;

			row.UNITQTY = plan.UnitQty;


			row.LOTNAME = plan.LotID;
			row.PROCESSFLOWNAME = plan.ProcessID;
			row.PROCESSOPERATIONNAME = plan.StepID;
			row.PRODUCTSPECNAME = plan.ProductID;
			row.PRODUCTSPECVERSION = plan.ProductVersion;

			row.LOTPRIORITY = plan.WipInfo.Priority.ToString();

			row.LAYERNAME = plan.FabStep.LayerID;
			row.CARRIERNAME = plan.WipInfo.BatchID;

			row.TRACKINTIME = plan.TrackInTime.DbNullDateTime();
			row.TRACKOUTTIME = plan.TrackOutTime.DbNullDateTime();

			row.OWNERTYPE = plan.WipInfo.OwnerType;
			row.OWNERID = plan.WipInfo.OwnerID;

			int index = plan.Lot.Plans.IndexOf(plan);

			if (plan.Lot.Plans.Count > index + 1)
			{
				FabPlanInfo nextPlan = plan.Lot.Plans[index + 1] as FabPlanInfo;
				if (nextPlan.IsLoaded)
					row.DESTINATIONMACHINENAME = nextPlan.LoadedResource.ResID;
			}

			//DESTINATIONMACHINENAME(NextEqpID)이 존재하는 경우만 기록 (2019.10.11 RTD 담당자)
			if (LcdHelper.IsEmptyID(row.DESTINATIONMACHINENAME))
				return;

			OutputMart.Instance.RTD_RtsPlanningUnload.Add(row);
		}

		private static void ClassyfyLotPlanByEqpID(Dictionary<string, List<FabPlanInfo>> dic, FabLot lot)
		{
			foreach (FabPlanInfo plan in lot.Plans)
			{
				if (plan.IsLoaded == false)
					continue;

				List<FabPlanInfo> list;
				if (dic.TryGetValue(plan.LoadedResource.ResID, out list) == false)
				{
					list = new List<FabPlanInfo>();
					dic.Add(plan.LoadedResource.ResID, list);
				}

				list.Add(plan);
			}
		}





		#region StepWip

        public static void WriteStepWip()
        {
            DateTime now = AoFactory.Current.NowDT;

            DateTime targetTime = now;
            if (_collectsStepWips.Contains(targetTime))
                return;

            _collectsStepWips.Add(targetTime);

            //StepWip
            WriteStepWipByInflow(targetTime);

            //StepWip2
            WriteStepWipByWipManager(targetTime);
        }

        private static void WriteStepWipByWipManager(DateTime targetTime)
        {
            var mgr = AoFactory.Current.WipManager;
            var group = mgr.GetGroup("StepWips");
            if (group == null)
                return;

            Dictionary<string, StepWip2> dic = new Dictionary<string, StepWip2>();

            foreach (FabLot sample in group.UniqueValues())
            {
                foreach (FabLot lot in group.Find(sample))
                {
                    string origProductVersion = lot.OrigProductVersion;
                    if (string.IsNullOrEmpty(origProductVersion))
                        origProductVersion = lot.CurrentProductVersion;

                    string key = LcdHelper.CreateKey(lot.CurrentShopID, lot.CurrentStepID,
                        lot.CurrentProductID, lot.CurrentProductVersion, origProductVersion, lot.OwnerType);

                    StepWip2 row;
                    if (dic.TryGetValue(key, out row) == false)
                    {
                        row = new StepWip2();

                        row.VERSION_NO = ModelContext.Current.VersionNo;
                        row.FACTORY_ID = lot.CurrentFactoryID;
                        row.AREA_ID = lot.CurrentFabStep.AreaID;
                        row.SHOP_ID = lot.CurrentShopID;

                        row.STEP_ID = lot.CurrentStep.StepID;
                        row.STD_STEP_ID = lot.CurrentFabStep.StdStepID;
                        row.STD_STEP_SEQ = lot.CurrentFabStep.StdStep.StepSeq;

                        row.PRODUCT_ID = lot.CurrentProductID;
                        row.PRODUCT_VERSION = lot.CurrentProductVersion;
                        row.ORIG_PRODUCT_VERSION = origProductVersion;
                        row.OWNER_TYPE = lot.OwnerType;
                        row.PROCESS_ID = lot.CurrentProcessID;

                        row.PLAN_DATE = targetTime;

                        OutputMart.Instance.StepWip2.Add(row);

                        dic.Add(key, row);
                    }

                    if (lot.CurrentState == EntityState.RUN)
                        row.RUN_QTY += lot.UnitQty;
                    else
                        row.WAIT_QTY += lot.UnitQty;
                }
            }
        }

        private static void WriteStepWipByInflow(DateTime targetTime)
        {
            Dictionary<string, StepWip> dic = new Dictionary<string, StepWip>();

            foreach (var jobState in InFlowMaster.JobStateDic.Values)
            {
                foreach (var stepWips in jobState.StepWips.Values)
                {
                    //if(jobState.ProductID == "TH645A1AB100" && stepWips.Step.StdStepID == "2401")
                    //    Console.WriteLine("B");

                    foreach (var it in stepWips.WipList)
                    {
                        foreach (FabLot lot in it.Value)
                        {
                            string origProductVersion = lot.OrigProductVersion;
                            if (string.IsNullOrEmpty(origProductVersion))
                                origProductVersion = lot.CurrentProductVersion;

                            string key = LcdHelper.CreateKey(lot.CurrentShopID, lot.CurrentStepID,
                                lot.CurrentProductID, lot.CurrentProductVersion, origProductVersion, lot.OwnerType);

                            StepWip row;
                            if (dic.TryGetValue(key, out row) == false)
                            {
                                row = new StepWip();

                                row.VERSION_NO = ModelContext.Current.VersionNo;
                                row.FACTORY_ID = lot.CurrentFactoryID;
                                row.AREA_ID = lot.CurrentFabStep.AreaID;
                                row.SHOP_ID = lot.CurrentShopID;

                                row.STEP_ID = lot.CurrentStep.StepID;
                                row.STD_STEP_ID = lot.CurrentFabStep.StdStepID;
                                row.STD_STEP_SEQ = lot.CurrentFabStep.StdStep.StepSeq;

                                row.PRODUCT_ID = lot.CurrentProductID;
                                row.PRODUCT_VERSION = lot.CurrentProductVersion;
                                row.ORIG_PRODUCT_VERSION = origProductVersion;
                                row.OWNER_TYPE = lot.OwnerType;
                                row.PROCESS_ID = lot.CurrentProcessID;

                                row.PLAN_DATE = targetTime;

                                OutputMart.Instance.StepWip.Add(row);

                                dic.Add(key, row);
                            }

                            if (it.Key == WipType.Wait)
                                row.WAIT_QTY += lot.UnitQty;
                            else if (it.Key == WipType.Run)
                                row.RUN_QTY += lot.UnitQty;
                        }
                    }
                }
            }
        }

        //public static void ResetCollectTime()
        //{
        //    _collectsStepWips.Clear();
        //}

        #endregion

        #region StepMove

        private static void WriteStepMove()
        {
            //DateTime planStartTime = ModelContext.Current.StartTime;
            //DateTime planEndTime = ModelContext.Current.EndTime;

            foreach (LotHistory item in _lotHist)
            {
                //초기 RunWip IN QTY 기록 제외
                if (LcdHelper.ToBoolYN(item.WIP_INIT_RUN) == false)
                {
                    DateTime tkIn = item.START_TIME.GetValueOrDefault(DateTime.MinValue);
                    WriteStepMove(item, tkIn, (int)item.IN_QTY, false);
                }

                DateTime tkOut = item.END_TIME.GetValueOrDefault(DateTime.MaxValue);
                WriteStepMove(item, tkOut, (int)item.OUT_QTY, true);
            }
        }



        private static void WriteStepMove(LotHistory item, DateTime eventTime, int qty, bool isOut)
        {
            if (qty <= 0)
                return;

            DateTime planStartTime = ModelContext.Current.StartTime;
            DateTime planEndTime = ModelContext.Current.EndTime;

            if (eventTime < planStartTime || eventTime >= planEndTime)
                return;

            DateTime targetDate = TimeHelper.GetRptDate_1Hour(eventTime);

            StepMove row = GetRow(item, targetDate);

            if (isOut)
                row.OUT_QTY += qty;
            else
                row.IN_QTY += qty;
        }

        internal static StepMove GetRow(LotHistory item, DateTime targetDate)
        {
            string verNo = item.VERSION_NO;
            string factoryID = item.FACTORY_ID;
            string areaID = item.AREA_ID;
            string shopID = item.SHOP_ID;
            string stepID = item.STEP_ID;

            string stdStepID = item.STD_STEP_ID;
            string productID = item.PRODUCT_ID;
            string productVer = item.PROUDCT_VERSION;
            string processID = item.PROCESS_ID;
            string ownerType = item.OWNER_TYPE;

            string eqpID = LcdHelper.ToSafeString(item.EQP_ID);
            string eqpGroupID = item.EQP_GROUP_ID;

            FabStdStep stdStep = BopHelper.FindStdStep(shopID, stdStepID);
            int stdStepSeq = stdStep != null ? stdStep.StepSeq : -1;

            StepMove sp = OutputMart.Instance.StepMove.Find(verNo,
                factoryID, shopID, stepID, productID, productVer, ownerType, processID, targetDate, eqpID);

            if (sp == null)
            {
                sp = new StepMove();

                sp.VERSION_NO = verNo;
                sp.FACTORY_ID = factoryID;
                sp.AREA_ID = areaID;
                sp.SHOP_ID = shopID;
                sp.STEP_ID = stepID;
                sp.STD_STEP_ID = stdStepID;
                sp.STD_STEP_SEQ = stdStepSeq;
                sp.PRODUCT_ID = productID;
                sp.PRODUCT_VERSION = productVer;
                sp.OWNER_TYPE = ownerType;
                sp.PROCESS_ID = processID;
                sp.PLAN_DATE = targetDate;
                sp.EQP_ID = eqpID;
                sp.EQP_GROUP_ID = eqpGroupID;

                OutputMart.Instance.StepMove.Add(sp);
            }

            return sp;
        }

        #endregion

        #region LotHistory
        private static void WriteLotHistory(FabLot lot)
        {
            int idx = 0;
            FabPlanInfo prevInfo = null;
            foreach (FabPlanInfo item in lot.Plans)
            {
                OutCollector.WriteLotHistory(item, prevInfo, idx);
                prevInfo = item;
                idx++;
            }

            QTimeMaster.WriteStayHourHist(lot);
        }


        private static void WriteLotHistory(FabPlanInfo plan, FabPlanInfo prevPlan, int seq)
        {

#if DEBUG
            if (plan.LotID == "TH981024N00")
                Console.WriteLine();
#endif

            //if (plan.StartTime == plan.EndTime
            //    && plan.WipInfo.IsInitWip
            //    && plan.WipInfo.WipStepID == plan.StepID
            //    && plan.WipInfo.CurrentState == EntityState.RUN
            //    )
            //    return;

            Outputs.LotHistory row = new LotHistory();

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = plan.FabStep.FactoryID;
            row.AREA_ID = plan.FabStep.AreaID;
            row.SHOP_ID = plan.FabStep.ShopID;
            row.SEQ = seq;

            row.LOT_ID = plan.LotID;
            row.PARENT_ID = Constants.NULL_ID;

            row.PROCESS_ID = plan.ProcessID;
            row.PRODUCT_ID = plan.ProductID;
            row.PROUDCT_VERSION = plan.ProductVersion;

            row.STD_STEP_ID = plan.FabStep.StdStepID;
            row.STEP_ID = plan.FabStep.StepID;

            row.RUN_RATE = 1;

            if (prevPlan != null)
            {
                if (plan.LotID != prevPlan.LotID)
                    row.PARENT_ID = prevPlan.LotID;

                if (plan.ProcessID != prevPlan.ProcessID)
                    row.FROM_PROCESS_ID = prevPlan.ProcessID;

                if (plan.ProductID != prevPlan.ProductID)
                    row.FROM_PRODUCT_ID = prevPlan.ProductID;

                if (plan.FabStep.ShopID != prevPlan.FabStep.ShopID)
                    row.FROM_SHOP_ID = prevPlan.FabStep.ShopID;
            }

            row.TRANSFER_START_TIME = plan.TransferStartTime.DbNullDateTime();
            row.TRANSFER_END_TIME = plan.TransferEndTime.DbNullDateTime();

            row.START_TIME = plan.TrackInTime.DbNullDateTime();
            row.END_TIME = plan.TrackOutTime.DbNullDateTime();

            row.EQP_IN_START_TIME = plan.EqpInStartTime.DbNullDateTime();
            row.EQP_IN_END_TIME = plan.EqpInEndTime.DbNullDateTime();

            if (plan.IsLoaded)
            {
                FabEqp eqp = plan.LoadedResource as FabEqp;

                if (eqp != null)
                {
                    row.EQP_ID = eqp.EqpID;
                    row.EQP_GROUP_ID = eqp.EqpGroup;
                }

                row.MASK_ID = plan.MaskID; //plan.ToolID
                row.RUN_RATE = plan.LoadedResource.Utilization;
                row.SIM_TYPE = eqp.SimType.ToString();

                DateTime eqpInEndTime = plan.EqpInEndTime != DateTime.MinValue ? plan.EqpInEndTime : ModelContext.Current.EndTime;
                DateTime eqpInStartTime = plan.EqpInStartTime != DateTime.MinValue ? plan.EqpInStartTime : ModelContext.Current.EndTime;

                double tatcTime = (eqpInEndTime - eqpInStartTime).TotalSeconds;
                row.TACT_TIME = tatcTime.ToRound();
                row.N_TACT_TIME = (tatcTime / plan.InQty).ToRound();

                DateTime trackInTime = plan.TrackInTime != DateTime.MinValue ? plan.TrackInTime : ModelContext.Current.EndTime;
                DateTime trackOutTime = plan.TrackOutTime != DateTime.MinValue ? plan.TrackOutTime : ModelContext.Current.EndTime;

                double flowTime = (trackOutTime - trackInTime).TotalSeconds;
                row.FLOW_TIME = flowTime.ToRound();
                row.N_FLOW_TIME = (flowTime - (row.N_TACT_TIME * (plan.InQty - 1))).ToRound();
            }

            row.IN_QTY = (float)plan.InQty;
            row.OUT_QTY = (float)plan.OutQty;

            var wipInfo = plan.WipInfo;
            row.IS_WIP = wipInfo.IsInitWip.ToStringYN();
            row.PRE_PROCESS_ID = wipInfo.MainProcessID;
            row.PRE_STEP_ID = wipInfo.MainStepID;
            row.OWNER_TYPE = wipInfo.OwnerType;
            row.OWNER_ID = wipInfo.OwnerID;
            row.IS_INPORT = wipInfo.IsInPortWip.ToStringYN();

            row.WIP_INIT_RUN = plan.IsInitRunWip.ToStringYN();
            row.IS_EQP_RECIPE = plan.IsEqpRecipe.ToStringYN();

            if (plan.UsedEqpArrangeInfo != null)
                row.USED_EQP_ARRANGE = plan.UsedEqpArrangeInfo.ToString();

            row.REMAIN_QTIME = GetRemainQtime(plan);

            OutputMart.Instance.LotHistory.Add(row);

            _lotHist.Add(row);
        }

        private static double GetRemainQtime(FabPlanInfo plan)
        {
            if (plan.QTimeInfo == null)
                return 0;

            List<StayHour> list;
            plan.QTimeInfo.TryGetValue("MAX", out list);

            if (list == null || list.Count == 0)
                return 0;

            DateTime planEndTime = ModelContext.Current.EndTime;

            Double min = Double.MaxValue;
            foreach (var item in list)
            {
                if (item.FromStepOutTime == DateTime.MinValue)
                    continue;

                if (item.ToStep != plan.FabStep)
                    continue;

                DateTime lastStateTime = item.ToStepInTime.IsMinValue() ? planEndTime : item.ToStepInTime;
                var stayTime = (lastStateTime - item.FromStepOutTime).TotalMinutes;

                var remain = item.QTime.TotalMinutes - stayTime;

                min = Math.Min(min, remain);

            }

            if (min == Double.MaxValue)
                return 0;

            return min.ToRound(3);
        }
        #endregion

        #region Write EqpPlan
        internal static void WriteEqpPlan(FabAoEquipment eqp)
        {
            if (eqp.IsParallelChamber)
            {
                WriteEqpPlan_ParallelChamber(eqp);
                return;
            }

            FabEqp targetEqp = eqp.ToFabEqp();
            List<FabLoadInfo> loadInfos = eqp.LoadInfos;

            WriteEqpPlan(targetEqp, loadInfos);
            WriteInterface_RTD(targetEqp, loadInfos);
        }

        private static void WriteEqpPlan_ParallelChamber(FabAoEquipment eqp)
        {
            if (eqp.IsParallelChamber == false)
                return;

            List<FabLoadInfo> loadInfos = new List<FabLoadInfo>();

            var targetEqp = eqp.ToFabEqp();

            //SubEqp별로 Loading된 Glass 수량으로 분할 기록
            foreach (var subEqp in targetEqp.SubEqps.Values)
            {
                foreach (var item in subEqp.LoadInfos)
                {
                    loadInfos.AddSort(item, CompareHelper.FabLoadInfoComparer.Default);
                }
            }

            WriteEqpPlan(targetEqp, loadInfos);

            //parent eqp 기준으로 merge
            var mergelist = MergeLoadInfo_ParallelChamber(loadInfos);
            WriteInterface_RTD(targetEqp, mergelist);
        }

        private static List<FabLoadInfo> MergeLoadInfo_ParallelChamber(List<FabLoadInfo> loadInfos)
        {
            if (loadInfos == null || loadInfos.Count == 0)
                return loadInfos;

            //이미 sort된 상태로 넘어옴.
            //if (loadInfos.Count > 1)
            //    loadInfos.Sort(CompareHelper.FabLoadInfoComparer.Default);

            List<FabLoadInfo> list = new List<FabLoadInfo>();

            FabLoadInfo prev = null;
            foreach (var curr in loadInfos)
            {
                var currPlan = curr.Target as FabPlanInfo;

                bool isMerge = IsMergeLoadInfo(prev, curr);
                if (isMerge)
                {
                    prev.EndTime = LcdHelper.Max(prev.EndTime, curr.EndTime);
                    prev.UnitQty += curr.UnitQty;

                    continue;
                }

                //clone
                FabLoadInfo item = curr.ShallowCopy();

                item.SubEqpID = null; //subEqpID 무시

                if (prev != null)
                {
                    if (item.State != LoadingStates.SETUP && item.State != LoadingStates.BUSY)
                    {
                        item.StartTime = LcdHelper.Max(item.StartTime, prev.EndTime);
                        if (item.StartTime >= item.EndTime)
                            continue;
                    }
                }

                list.Add(item);

                prev = item;
            }

            return list;
        }

        private static bool IsMergeLoadInfo(FabLoadInfo prev, FabLoadInfo curr)
        {
            if (prev == null || curr == null)
                return false;

            var prevPlan = prev.Target as FabPlanInfo;
            var currPlan = curr.Target as FabPlanInfo;

            if (prevPlan != null && currPlan != null)
            {
                if (prevPlan.LotID == currPlan.LotID && prev.Step == curr.Step)
                {
                    //첫 SETUP은 남기고 나머지 SETUP은 Merge 처리
                    if (prev.State == LoadingStates.SETUP && prev.State != curr.State)
                        return false;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (prev.State == curr.State)
            {
                if (prev.EndTime >= curr.StartTime)
                    return true;
            }

            return false;
        }

        private static void WriteEqpPlan(FabEqp targetEqp, List<FabLoadInfo> loadInfos)
        {
            foreach (var item in loadInfos)
            {
                if (item.EndTime <= ModelContext.Current.StartTime)
                    continue;

                WriteEqpPlan(targetEqp, item);
            }
        }

        private static void WriteInterface_RTD(FabEqp targetEqp, List<FabLoadInfo> loadInfos)
        {
            int groupNo = 0;
            int seqNo = 1;

            FabLoadInfo prev = null;
            foreach (var item in loadInfos)
            {
                if (item.EndTime <= ModelContext.Current.StartTime)
                    continue;

                //BUSY만 기록 (2019.09.25 RTD 담당자)
                if (item.State != LoadingStates.BUSY)
                    continue;

                var plan = item.Target as FabPlanInfo;

				//초기 RUN 재공은 I/ F 제외
				if (plan != null && plan.IsInitRunWip)
					continue;

                CheckCount(prev, item, ref groupNo, ref seqNo);

				Write_RTD_RtsPlanningLot(targetEqp, item, plan, groupNo, seqNo);
				prev = item;
			}
		}

        private static void CheckCount(FabLoadInfo prev, FabLoadInfo curr, ref int groupNo, ref int seqNo)
        {
            if (prev != null)
            {
                if (IsChange(prev, curr))
                {
                    groupNo++;
                    seqNo = 1;

                    return;
                }

                if (IsFirstBusyAfterSetup(prev, curr))
                    return;

                seqNo++;
            }
            else
            {
                groupNo++;
                seqNo = 1;
            }
        }

        private static bool IsChange(FabLoadInfo prev, FabLoadInfo curr)
        {
            if (prev.State != curr.State)
            {
                if (IsFirstBusyAfterSetup(prev, curr))
                    return false;

                return true;
            }

            var prevPlan = prev.Target as FabPlanInfo;
            var currPlan = curr.Target as FabPlanInfo;

			return IsChange(prevPlan, currPlan);
		}

		private static bool IsChange(FabPlanInfo prevPlan, FabPlanInfo currPlan)
		{
			bool prevNull = prevPlan == null;
			bool currNull = currPlan == null;

            if (prevNull != currNull)
                return true;

            //all null
            if (prevNull && currNull)
                return false;

            if (prevPlan.StepID != currPlan.StepID)
                return true;

            if (prevPlan.ProductID != currPlan.ProductID)
                return true;

            if (prevPlan.ProductVersion != currPlan.ProductVersion)
                return true;

            if (prevPlan.OwnerType != currPlan.OwnerType)
                return true;

            if (prevPlan.OwnerID != currPlan.OwnerID)
                return true;

            return false;
        }

        private static bool IsFirstBusyAfterSetup(FabLoadInfo prev, FabLoadInfo info)
        {
            if (prev == null)
                return false;

            if (prev.State == LoadingStates.SETUP && info.State == LoadingStates.BUSY)
                return true;

            return false;
        }

        private static void Write_RTD_RtsPlanningLot(FabEqp targetEqp, FabLoadInfo item, FabPlanInfo plan, int groupNo, int seqNo)
        {
            //if (item.EqpStatus != LoadingStates.BUSY)
            //    return;

            Outputs.RTD_RtsPlanningLot row = new RTD_RtsPlanningLot();

            row.VERSION = ModelContext.Current.VersionNo;
            row.TIMEKEY = TimeKey;

            row.SITE = targetEqp.FactoryID;
            row.FACTORYNAME = targetEqp.ShopID;

            row.MACHINENAME = targetEqp.EqpID;
            row.MACHINEGROUPNAME = targetEqp.EqpGroup;
            row.MACHINESTATUS = item.State.ToString();

            row.STARTTIME = item.StartTime.DbNullDateTime();
            row.ENDTIME = item.EndTime.DbNullDateTime();

            row.EVENTTIME = TimeKeyDate;
            row.EVENTNAME = EVENTNAME;
            row.EVENTUSER = EVENTUSER;
            row.ISINPUTLOT = "N";

            row.INPUTPRIORITY = groupNo;
            row.INPUTORDER = seqNo;

            row.UNITQTY = item.UnitQty;

            if (plan != null)
            {
                row.LOTNAME = plan.LotID;
                row.PROCESSFLOWNAME = plan.ProcessID;
                row.PROCESSOPERATIONNAME = plan.StepID;
                row.PRODUCTSPECNAME = plan.ProductID;
                row.PRODUCTSPECVERSION = plan.ProductVersion;

                row.LOTPRIORITY = plan.WipInfo.Priority.ToString();
                row.ISINPUTLOT = plan.WipInfo.IsInputLot.ToStringYN();

                row.LAYERNAME = plan.FabStep.LayerID;
                row.CARRIERNAME = plan.WipInfo.BatchID;


                row.TRACKINTIME = plan.TrackInTime.DbNullDateTime();
                row.TRACKOUTTIME = plan.TrackOutTime.DbNullDateTime();

                row.OWNERTYPE = plan.WipInfo.OwnerType;
                row.OWNERID = plan.WipInfo.OwnerID;

            }

            OutputMart.Instance.RTD_RtsPlanningLot.Add(row);
        }

        private static void Write_RTD_RtsPlanningUnload(FabEqp targetEqp, FabLoadInfo item, FabPlanInfo plan, int groupNo, int seqNo)
        {
            Outputs.RTD_RtsPlanningUnload row = new RTD_RtsPlanningUnload();

            row.VERSION = ModelContext.Current.VersionNo;
            row.TIMEKEY = TimeKey;

            row.SITE = targetEqp.FactoryID;
            row.FACTORYNAME = targetEqp.ShopID;

            row.SOURCEMACHINENAME = targetEqp.EqpID;
            row.DESTINATIONMACHINENAME = Constants.NULL_ID;

            row.MACHINEGROUPNAME = targetEqp.EqpGroup;
            row.MACHINESTATUS = item.State.ToString();

            row.STARTTIME = item.StartTime.DbNullDateTime();
            row.ENDTIME = item.EndTime.DbNullDateTime();

            row.EVENTTIME = TimeKeyDate;
            row.EVENTNAME = EVENTNAME;
            row.EVENTUSER = EVENTUSER;

            row.INPUTPRIORITY = groupNo;
            row.INPUTORDER = seqNo;

            row.UNITQTY = item.UnitQty;

            if (plan != null)
            {
                row.LOTNAME = plan.LotID;
                row.PROCESSFLOWNAME = plan.ProcessID;
                row.PROCESSOPERATIONNAME = plan.StepID;
                row.PRODUCTSPECNAME = plan.ProductID;
                row.PRODUCTSPECVERSION = plan.ProductVersion;

                row.LOTPRIORITY = plan.WipInfo.Priority.ToString();

                row.LAYERNAME = plan.FabStep.LayerID;
                row.CARRIERNAME = plan.WipInfo.BatchID;

                row.TRACKINTIME = plan.TrackInTime.DbNullDateTime();
                row.TRACKOUTTIME = plan.TrackOutTime.DbNullDateTime();

                row.OWNERTYPE = plan.WipInfo.OwnerType;
                row.OWNERID = plan.WipInfo.OwnerID;

                int index = plan.Lot.Plans.IndexOf(plan);

                if (plan.Lot.Plans.Count > index + 1)
                {
                    FabPlanInfo nextPlan = plan.Lot.Plans[index + 1] as FabPlanInfo;
                    if (nextPlan.IsLoaded)
                        row.DESTINATIONMACHINENAME = nextPlan.LoadedResource.ResID;
                }
            }

            //DESTINATIONMACHINENAME(NextEqpID)이 존재하는 경우만 기록 (2019.10.11 RTD 담당자)
            if (LcdHelper.IsEmptyID(row.DESTINATIONMACHINENAME))
                return;

            OutputMart.Instance.RTD_RtsPlanningUnload.Add(row);
        }

        private static void WriteEqpPlan(FabEqp eqp, FabLoadInfo item)
        {
            Outputs.EqpPlan row = new EqpPlan();

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = eqp.FactoryID;
            row.AREA_ID = GetAreaID(item.AreaID, eqp.ShopID);
            row.SHOP_ID = eqp.ShopID;
            row.EQP_ID = eqp.EqpID;
            row.SUB_EQP_ID = item.SubEqpID;
            row.EQP_GROUP_ID = eqp.EqpGroup;
            row.EQP_STATUS = item.State.ToString();
            row.EQP_STATUS_INFO = item.StateInfo;

            row.START_TIME = item.StartTime.DbNullDateTime();
            row.END_TIME = item.EndTime.DbNullDateTime();

            row.LOT_ID = item.LotID;

            var plan = item.Target as FabPlanInfo;
            if (plan != null)
            {
                row.LOT_ID = plan.LotID;
                row.STEP_ID = plan.StepID;
                row.LAYER_ID = plan.FabStep.LayerID;
                row.STEP_TYPE = plan.FabStep.StepType;

                if (plan.IsInitRunWip)
                    row.START_TIME = plan.StartTime.DbNullDateTime();

                row.TRACK_IN_TIME = plan.TrackInTime.DbNullDateTime();
                row.TRACK_OUT_TIME = plan.TrackOutTime.DbNullDateTime();

                row.PRODUCT_ID = plan.ProductID;
                row.PRODUCT_VERSION = plan.ProductVersion;
                row.PROCESS_ID = plan.ProcessID;

                row.LOT_PRIORITY = plan.Lot.Priority;
                row.BATCH_ID = Constants.NULL_ID;

                row.UNIT_QTY = (decimal)item.UnitQty;
                //row.UNIT_QTY = item.State == LoadingStates.BUSY ? (decimal)item.UnitQty : 0;

                var wipInfo = plan.WipInfo;

                row.WIP_STEP_ID = wipInfo.WipStepID;
                row.WIP_PROCESS_ID = wipInfo.WipProcessID;
                row.WIP_PRODUCT_ID = wipInfo.WipProductID;
                row.OWNER_TYPE = wipInfo.OwnerType;
                row.OWNER_ID = wipInfo.OwnerID;
                row.WIP_IS_INPORT = wipInfo.IsInPortWip.ToStringYN();

                row.TOOL_ID = plan.MaskID;

                row.WIP_INIT_RUN = plan.IsInitRunWip.ToStringYN();
                row.IS_EQP_RECIPE = plan.IsEqpRecipe.ToStringYN();

                if (plan.IsPegged)
                {
                    FabStepTarget st = plan.PegInfoList[0].StepTarget as FabStepTarget;

                    row.TARGET_PRODUCT_ID = st.Mo.ProductID;
                    row.TARGET_DATE = st.DueDate;
                    row.DEMAND_ID = st.Mo.DemandID;
                    row.DEMAND_PLAN_DATE = st.Mo.DueDate;
                }
            }

            if (string.IsNullOrEmpty(item.SubEqpID) == false)
            {
                var subEqp = eqp.GetSubEqp(item.SubEqpID) as FabSubEqp;
                if (subEqp != null && subEqp.SubEqpGroup != null)
                    row.SUB_EQP_COUNT = subEqp.SubEqpGroup.SubEqps.Count;
            }

            OutputMart.Instance.EqpPlan.Add(row);
        }

        private static string GetAreaID(string areaID, string shopID)
        {
            if (LcdHelper.IsEmptyID(areaID))
            {
                switch (shopID)
                {
                    case Constants.ArrayShop:
                        return Constants.TFT;

                    case Constants.CfShop:
                        return Constants.CF;

                    case Constants.CellShop:
                        return Constants.CellShop;
                }
            }

            return areaID;
        }

        #endregion

        internal static void WriteInputLotLog(FabLot lot, DateTime now)
        {
            Outputs.InputLotLog row = new InputLotLog();

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = lot.Wip.FactoryID;
            row.SHOP_ID = lot.Wip.ShopID;

            row.LOT_ID = lot.LotID;
            row.PRODUCT_ID = lot.Wip.WipProductID;
            row.PROCESS_ID = lot.Wip.WipProcessID;
            row.STEP_ID = lot.Wip.WipStepID;

            row.INPUT_QTY = lot.UnitQty;
            row.IN_TIME = now;
            row.RELEASE_TIME = lot.ReleaseTime;

            if (lot.FrontInTarget != null)
            {
                row.TARGET_DATE = lot.FrontInTarget.TargetDate;
                row.TARGET_DUE_DATE = lot.FrontInTarget.DueDate;
                row.TARGET_KEY = lot.FrontInTarget.TargetKey;
                row.DEMAND_ID = lot.FrontInTarget.DemandID;
            }

            OutputMart.Instance.InputLotLog.Add(row);
        }

        internal static void CollectInputLot(FabLot lot)
        {
            string shopID = lot.CurrentShopID;

            if (BopHelper.IsCellShop(shopID))
                return;

            _inputLotList.Add(lot);
        }

        internal static void WriteLotVerChangeInfo(FabLot lot, FabEqp eqp, string fromProdVer, string toProdVer)
        {
            Outputs.LotVerChangeInfo row = new LotVerChangeInfo();

            FabStep step = lot.CurrentFabStep;

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = step.FactoryID;
            row.SHOP_ID = step.ShopID;
            row.AREA_ID = step.AreaID;
            row.STEP_ID = step.StepID;
            row.LAYER_ID = step.LayerID;

            row.EQP_ID = eqp.EqpID;
            row.EQP_GROUP_ID = eqp.EqpGroup;

            row.LOT_ID = lot.LotID;
            row.CARRIERNAME = lot.Wip.BatchID;

            row.PRODUCT_ID = lot.CurrentProductID;

            row.FROM_VERSION = fromProdVer;
            row.TO_VERSION = toProdVer;

            row.CHANGE_TYPE = lot.Wip.IsInitWip ? "CHANGE" : "NEW";

            row.UPDATE_TIME = AoFactory.Current.NowDT;

            OutputMart.Instance.LotVerChangeInfo.Add(row);

            Write_RTD_RtsPlanningChangeSpec(lot, fromProdVer, toProdVer);
        }


        private static void Write_RTD_RtsPlanningChangeSpec(FabLot lot, string fromProdVer, string toProdVer)
        {
            if (lot.Wip.IsInitWip == false)
                return;

            Outputs.RTD_RtsPlanningChangeSpec row = new RTD_RtsPlanningChangeSpec();

            row.TIMEKEY = TimeKey;
            row.VERSION = ModelContext.Current.VersionNo;
            row.SITE = lot.CurrentFactoryID;
            row.LOTNAME = lot.LotID;
            row.FACTORYNAME = lot.CurrentShopID;
            row.PRODUCTSPECNAME = lot.CurrentProductID;
            row.PRODUCTSPECVERSION = fromProdVer;
            row.PROCESSFLOWNAME = lot.CurrentProcessID;
            row.PROCESSOPERATIONNAME = lot.CurrentStepID;
            row.OWNERID = lot.OwnerID;
            row.DESTPRODUCTSPECVERSION = toProdVer;
            row.PRIORITY = 1;
            row.CHANGEDFLAG = "";
            row.EVENTTIME = TimeKeyDate;
            row.EVENTNAME = EVENTNAME;
            row.EVENTUSER = EVENTUSER;

            OutputMart.Instance.RTD_RtsPlanningChangeSpec.Add(row);
        }

        internal static void WriteLimitMLog(EqpArrangeInfo item, FabAoEquipment eqp, FabLot lot, DateTime nowDt, string gubun = Constants.NULL_ID)
        {
            Outputs.LimitMLog row = new LimitMLog();

            row.VERSION_NO = ModelContext.Current.VersionNo;
            row.KEY = item.Key;

            if (eqp != null)
            {
                var targetEqp = eqp.Target as FabEqp;
                row.EQP_ID = item.EqpID;
                row.EQP_GROUP_ID = targetEqp.EqpGroup;
            }
            else
            {
                row.EQP_ID = item.EqpID;
                row.EQP_GROUP_ID = Constants.NULL_ID;
            }

            row.FACTORY_ID = item.FactoryID;
            row.SHOP_ID = item.ShopID;

            row.LIMIT_TYPE = item.LimitType;

            row.PRODUCT_ID = item.ProductID;
            row.PRODUCT_VERSION = item.ProductVer;
            row.STEP_ID = item.StepID;

            row.LIMIT_QTY = item.LimitQty;
            row.ACTUAL_CAPA = item.MoveQty;
            row.REMAIN_CAPA = item.RemainQty;

            if (lot != null)
            {
                row.LOT_ID = lot.LotID;
                row.LOT_QTY = lot.UnitQty;
                row.LOT_PRODUCT_ID = lot.CurrentProductID;
                row.LOT_PRODUCT_VERSION = lot.CurrentProductVersion;
            }
            else
            {
                row.LOT_ID = gubun;
                row.LOT_QTY = 0;
                row.LOT_PRODUCT_ID = Constants.NULL_ID;
                row.LOT_PRODUCT_VERSION = Constants.NULL_ID;
            }

            row.STATE_TIME = nowDt;

            row.DAILY_MODE = item.IsDailyMode.ToStringYN();
            row.ARR_ACTIVE_TYPE = item.ActivateType.ToString();
            row.DUE_DATE = item.DueDate.DbNullDateTime();

            OutputMart.Instance.LimitMLog.Add(row);
        }

        internal static void WriteMaskHistory()
        {
            if (SimHelper.IsCellRunning)
            {
                WriteMaskHistory_CELL();
                return;
            }

            foreach (FabMask mask in InputMart.Instance.FabMask.Values)
            {
                int priority = 1;

                MaskLoadInfo prev = null;
                foreach (var item in mask.LoadInfos)
                {
                    AddMaskHistory(mask, item);

                    if (LcdHelper.Equals(item.LotID, "MOVE"))
                        continue;

                    if (prev != null && prev.EqpID != item.EqpID)
                        Write_RTD_RtsPlanningMaskTransfer(mask, prev, item, ref priority);

                    prev = item;
                }
            }
        }

        internal static void WriteMaskHistory_CELL()
        {
            foreach (FabMask mask in JigMaster.Jigs.Values)
            {
                foreach (var item in mask.LoadInfos)
                {
                    AddMaskHistory(mask, item);
                }
            }
        }

        private static void AddMaskHistory(FabMask mask, MaskLoadInfo item)
        {
            Outputs.MaskHistory row = new MaskHistory();

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = mask.FactoryID;
            row.SHOP_ID = mask.ShopID;
            row.TOOL_ID = mask.ToolID;

            row.EQP_ID = item.EqpID;
            row.LOT_ID = item.LotID;
            row.STEP_ID = item.StepID;

            row.START_TIME = item.StartTime;
            row.END_TIME = item.EndTime;

            row.INIT_EQP_ID = mask.InitEqpID;
            row.INIT_LOCATION = mask.InitEqpID;
            row.INIT_STATE = mask.InitStateCode;

            row.PRODUCT_ID = item.ProductID;
            row.PRODUCT_VERSION = item.ProductVersion;

            OutputMart.Instance.MaskHistory.Add(row);
        }

        private static void Write_RTD_RtsPlanningMaskTransfer(FabMask mask, MaskLoadInfo prev, MaskLoadInfo curr, ref int priority)
        {
            if (prev == null || curr == null)
				return;
			
			string fromEqpID = prev.EqpID;
			string toEqpID = curr.EqpID;

			if (fromEqpID == toEqpID)
				return;

			Outputs.RTD_RtsPlanningMaskTransfer row = new RTD_RtsPlanningMaskTransfer();

			row.TIMEKEY = TimeKey;
			row.VERSION = ModelContext.Current.VersionNo;
			row.SITE = mask.FactoryID;
			row.FACTORYNAME = mask.ShopID;
			row.MASKNAME = mask.ToolID;
			row.PRIORITY = priority;
			row.SOURCEMACHINENAME = fromEqpID;
			row.DESTMACHINENAME = toEqpID;
			row.DESTSTARTTIME = curr.StartTime;
			row.EVENTTIME = TimeKeyDate;
			row.EVENTNAME = EVENTNAME;
			row.EVENTUSER = EVENTUSER;
			row.TRANSPORTJOBNAME = "";

			OutputMart.Instance.RTD_RtsPlanningMaskTransfer.Add(row);

            priority++;
        }

		internal static void WriteRelasePlan()
		{
			ReleasePlanMaster.WriteRelasePlan();
		}

		internal static void WriteRelasePlan_Cell(List<FabLot> list)
		{
			if (list == null || list.Count == 0)
				return;

			foreach (var lot in list)
			{
				Outputs.ReleasePlan row = new ReleasePlan();

				row.VERSION_NO = ModelContext.Current.VersionNo;

				row.FACTORY_ID = lot.Wip.FactoryID;
				row.SHOP_ID = lot.Wip.ShopID;

				row.EQP_GROUP_ID = Constants.NULL_ID;
				row.EQP_ID = Constants.NULL_ID;

				row.STEP_ID = ReleasePlanMaster.CELL_STEP;

				row.PRODUCT_ID = lot.Wip.WipProductID;

				DateTime releaseTime = lot.ReleaseTime;

				DateTime planDate = ShopCalendar.SplitDate(releaseTime);
				row.PLAN_DATE = LcdHelper.DbToString(planDate, false);

				row.START_TIME = releaseTime;
				row.END_TIME = releaseTime;

				row.UNIT_QTY = lot.UnitQty;

				var target = lot.FrontInTarget;
				if (target != null)
				{
					row.TARGET_DATE = target.TargetDate;
					row.TARGET_QTY = (int)target.TargetQty;

					var mo = target.Mo[0];
					if (mo != null)
					{
						row.DEMAND_ID = mo.DemandID;
						row.DEMAND_PLAN_DATE = mo.DueDate;
					}
				}

				row.ALLOC_EQP_SEQ = 0;
				row.ALLOC_SEQ = 0;
				row.ALLOC_TIME = LcdHelper.DbToString(AoFactory.Current.NowDT);

				OutputMart.Instance.ReleasePlan.Add(row);
			}
		}

		private static void WriteOutProfileHistory()
		{
			if (SimHelper.IsTftRunning == false)
				return;

			foreach (var item in InOutProfileMaster.OutProfiles.Values)
			{
				foreach (OutInfo info in item.Infos)
				{
					Outputs.OutProfileHistory row = new OutProfileHistory();

					row.VERSION_NO = ModelContext.Current.VersionNo;

					row.FACTORY_ID = info.FactoryID;
					row.SHOP_ID = info.ShopID;
					row.PRODUCT_ID = info.ProductID;
					row.PRODUCT_VERSION = info.ProductVersion;
					row.OWNER_TYPE = info.OwnerType;
					row.OUT_TIME = info.ReleaseTime;
					row.OUT_QTY = info.Qty;
					row.IS_WIP = LcdHelper.ToStringYN(info.IsWip);

					var cellCodeList = CellCodeMaster.GetCellCodeList(info.ProductID);
					row.CELL_CODE = LcdHelper.ToString(cellCodeList) ?? Constants.NULL_ID;

					OutputMart.Instance.OutProfileHistory.Add(row);
				}
			}
		}

		private static void Write_Rtd_LotRecord(FabLot lot)
		{
            var wip = lot.Wip;
            if (wip.IsInitWip == false)
                return;

            Outputs.RTD_RtsPlanngLotRecord row = new RTD_RtsPlanngLotRecord();

			row.VERSION = ModelContext.Current.VersionNo;
            row.SITE = wip.FactoryID;
            row.FACTORYNAME = wip.ShopID;
			row.LOTNAME = lot.LotID;
            row.PRODUCTSPECNAME = wip.WipProductID;
			row.PRODUCTSPECVERSION = wip.ProductVersion;
            row.PROCESSFLOWNAME = wip.WipProcessID;
            row.PROCESSOPERATIONNAME = wip.WipStepID;
            row.OWNERID = wip.OwnerID;
            row.OWNERTYPE = wip.OwnerType;
			row.EVENTTIME = TimeKeyDate;
			row.EVENTNAME = EVENTNAME;
			row.EVENTUSER = EVENTUSER;

			OutputMart.Instance.RTD_RtsPlanngLotRecord.Add(row);
		}

		internal static void Write_Rtd_LotUpkTracking(FabLot lot)
		{
			if (lot == null || lot.IsDummy)
				return;

            if (lot.Wip.IsInputLot)
                return;

            //ARRAY = 1100, CF = 0100
            if (LcdHelper.IsUnpackWipStep(lot.Wip.ShopID, lot.Wip.WipStepID) == false)
                return;

            //ARRAY = 1200, CF = 1300
            var currStep = lot.CurrentFabStep;
            if (currStep == null)
                return;

            var stdStep = currStep.StdStep;
			if (LcdHelper.IsUnpackTargetStep(stdStep.ShopID, stdStep.StepID) == false)
				return;

            float waitTat = 0;
            var tat = currStep.GetTat(lot.CurrentProductID, true);
            if (tat != null)
                waitTat = tat.WaitTat;              
            
            DateTime checkTime = PlanStartTime.AddMinutes(waitTat);
            DateTime trackInTime = lot.CurrentFabPlan.TrackInTime;

            if (trackInTime > checkTime)
				return;

			Outputs.RTD_RtsAutoUpkTraking row = new RTD_RtsAutoUpkTraking();

			row.TIMEKEY = TimeKey;
			row.VERSION = ModelContext.Current.VersionNo;
			row.LOTNAME = lot.LotID;
			row.PRODUCTSPECNAME = lot.CurrentProductID;
			row.PRODUCTSPECVERSION = lot.CurrentProductVersion;
			row.PRODUCTIONTYPE = currStep.AreaID;
			row.CARRIERNAME = lot.Wip.BatchID;
			row.OWNER = lot.OwnerID;
			row.EVENTNAME = EVENTNAME;
			row.EVENTUSER = EVENTUSER;
			row.EVENTTIME = TimeKeyDate;
            row.TRACKINTIME = trackInTime;

            OutputMart.Instance.RTD_RtsAutoUpkTraking.Add(row);
		}

		private static void WriteRtsPcPlanDay()
		{
            string defaultOwnerID = SiteConfigHelper.GetDefaultOwnerID();
            string defaultProductVersion = SiteConfigHelper.GetDefaultProductVersion();

            foreach (var lot in _inputLotList)
			{
				var dcnPlan = lot.ReleasePlan;
				if (dcnPlan == null)
					continue;

				Outputs.RTD_RtsPcPlanDay row = new RTD_RtsPcPlanDay();
								
				row.TIMEKEY = TimeKey;
				row.VERSION = ModelContext.Current.VersionNo;
				row.EVENTTIME = TimeKeyDate;
				row.EVENTNAME = EVENTNAME;				

				var fixPlan = dcnPlan.FixPlan;
				row.ISMESRESERVED = LcdHelper.ToStringYN(fixPlan != null);
                row.OWNER = defaultOwnerID;
                row.MASKVERSION = defaultProductVersion;

                if (fixPlan != null)
				{
					row.RESERVEDORDER = fixPlan.PLAN_SEQ;
					row.PRODUCTSPECNAME = fixPlan.PRODUCT_ID;
					row.RESERVEDMACHINENAME = fixPlan.EQP_ID;
					row.PLANSTATE = fixPlan.PLAN_STATE;
					row.MASKVERSION = fixPlan.PRODUCT_VERSION;
					row.MONTH = fixPlan.PLAN_DATE.ToString("yyyyMM");
					row.DAY = fixPlan.PLAN_DATE.Day.ToString();
					row.PRODUCTTYPE = fixPlan.AREA_ID;
					row.PLANQUANTITY = fixPlan.PLAN_QTY;
					row.FACTORYNAME = fixPlan.SHOP_ID;
					row.EVENTUSER = fixPlan.EVENTUSER;
					row.CREATIONTYPE = fixPlan.CREATIONTYPE;
                    row.OWNER = fixPlan.OWNER_ID;
				}
				else
				{
					row.RESERVEDORDER = dcnPlan.AllocSeq;
					row.PRODUCTSPECNAME = dcnPlan.ProductID;
					row.RESERVEDMACHINENAME = dcnPlan.EqpID;

                    row.PLANSTATE = DcnPlanState.Wait.ToString();

                    //1300 step product version
					if (string.IsNullOrEmpty(lot.OrigProductVersion) == false && LcdHelper.IsEmptyID(lot.OrigProductVersion) == false)
						row.MASKVERSION = lot.OrigProductVersion;

                    row.MONTH = TimeKeyDate.ToString("yyyyMM");
					row.DAY = TimeKeyDate.Day.ToString();
					row.PRODUCTTYPE = GetAreaID(null, dcnPlan.ShopID);
					row.PLANQUANTITY = dcnPlan.AllocQty;
					row.FACTORYNAME = dcnPlan.ShopID;
					row.EVENTUSER = EVENTUSER;
					row.CREATIONTYPE = EVENTUSER;                    
				}

				OutputMart.Instance.RTD_RtsPcPlanDay.Add(row);
			}
		}
	}
}
