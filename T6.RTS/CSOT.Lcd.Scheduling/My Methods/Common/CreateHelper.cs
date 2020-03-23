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
using CSOT.Lcd.Scheduling.Persists;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;
using Mozart.Simulation.Engine;
using System.Text;
namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class CreateHelper
    {
        public static Layer CreateLayer(string shopID, string layerID)
        {
            Layer layer = new Layer();

            layer.ShopID = shopID;
            layer.LayerID = layerID;
            layer.Steps = new List<FabStdStep>();

            return layer;
        }
        
        internal static FabProcess GetSafeProcess(string factoryID, string shopID, string processID)
        {
            FabProcess proc = BopHelper.FindProcess(shopID, processID);

            if (proc == null)
            {
                proc = CreateProcess(factoryID, shopID, processID);

                InputMart.Instance.FabProcess.Add(proc.Key, proc);
            }

            return proc;
        }


        internal static FabProcess CreateProcess(string factoryID, string shopID, string processID)
        {
            FabProcess proc = new FabProcess();

            proc.Key = BopHelper.GetProcessKey(shopID, processID);
            proc.FactoryID = factoryID;
            proc.ShopID = shopID;
            proc.ProcessID = processID;

            proc.Mappings = new Dictionary<string, Mozart.SeePlan.Lcd.DataModel.LcdStep>();
			proc.NonPathSteps = new Dictionary<string, Mozart.SeePlan.Lcd.DataModel.LcdStep>();

            return proc;
        }


        internal static ConfigInfo CreateConfigInfo(Config item)
        {
            ConfigInfo info = new ConfigInfo();

            info.CodeGroup = item.CODE_GROUP;
            info.CodeName = item.CODE_NAME;
            info.CodeValue = item.CODE_VALUE;

            return info;

        }

        internal static FabStdStep CreateStdStep(StdStep entity)
        {
            FabStdStep stdStep = entity.ToFabStdStep();

            stdStep.StepType = entity.STEP_TYPE;

            stdStep.IsMandatory = entity.IS_MANDATORY.ToBoolYN();
            stdStep.IsUseMask = entity.IS_USE_MASK.ToBoolYN();
			stdStep.IsUseJig = entity.IS_USE_JIG.ToBoolYN();

            stdStep.BalaceGap = entity.BALANCE_GAP == 0 ? 0 : entity.BALANCE_GAP;
            stdStep.BalaceWipQty = entity.BALANCE_WIP_QTY;
            stdStep.BalanceSteps = new List<FabStdStep>();

            stdStep.ToolArrange = new DoubleDictionary<string, string, List<MaskArrange>>();
			stdStep.JigArrange = new DoubleDictionary<string, string, List<MaskArrange>>();
			stdStep.MixRunPairSteps = new List<FabStdStep>();

            stdStep.IsInputStep = false;

            return stdStep;
        }

        internal static FabPegTarget CreateFabPegTarget(FabPegPart pp, FabMoPlan mo)
        {
            FabPegTarget pt = new FabPegTarget(pp, mo);
            pt.TargetKey = mo.TargetKey;

            return pt;
        }

        internal static FabStep CreateStep(ProcStep item, FabStdStep stdStep)
        {
            FabStep step = new FabStep(item.STEP_ID);

            step.FactoryID = item.FACTORY_ID;
            step.ShopID = item.SHOP_ID;

            step.StdStep = stdStep;
            step.StdStepID = stdStep.StepID;
            step.Description = item.STEP_DESC;
            step.NEXT_STEP_ID = item.NEXT_STEP_ID;

            step.TransferTime = item.TRANSFER_TIME;
            step.StepType = stdStep.StepType;
                        
            step.StepSeq = item.STEP_SEQ;
            step.Sequence = item.STEP_SEQ; //BopBuilder.InitializeRoute 에서 다시 설정됨

            step.Yields = new Dictionary<string, float>();
            step.StepTats = new Dictionary<string, StepTat>();
            step.StepTimes = new Dictionary<string, List<StepTime>>();

			step.DefaultTAT = step.GetDefaultTAT();

            //Array 1300 Step 이전만 false, 나머지 true
            step.NeedVerCheck = true;

            return step;
        }

        #region Product
        internal static FabProduct CreateProduct(Product item, FabProcess proc)
        {
            FabProduct prod = new FabProduct(item.PRODUCT_ID, proc);

            prod.FactoryID = item.FACTORY_ID;
            prod.ShopID = item.SHOP_ID;

            prod.ProductGroup = item.PRODUCT_GROUP;
            prod.CstSize = item.CST_SIZE;
            prod.PPG = item.PPG;
            prod.BatchSize = item.BATCH_SIZE;
            prod.ProductUnit = LcdHelper.ToEnum<ProductUnit>(item.PRODUCT_UNIT);
            prod.ProductType = LcdHelper.ToEnum<ProductType>(item.PRODUCT_TYPE);

            return prod;
        }
        #endregion

        internal static FabMoPlan CreateMoPlan(PreMoPlan item, FabMoMaster mm)
        {
            FabMoPlan mp = new FabMoPlan(mm, (float)item.Qty, item.DueDate);

            mp.FactoryID = item.FactoryID;
            mp.ShopID = item.ShopID;

            mp.DemandID = item.DemandID;
            mp.Priority = item.Priority;
            mp.Qty = item.Qty;

            mp.LineType = item.LineType;
                    
            mp.PreMoPlan = item;

            return mp;
        }

        internal static PreMoPlan CreatePreMoPlan(InOutPlan item)
        {
            PreMoPlan mo = new PreMoPlan();

            mo.FactoryID = item.FACTORY_ID;
            mo.ShopID = item.SHOP_ID;
            mo.DueDate =  Mozart.SeePlan.ShopCalendar.StartTimeOfNextDay(item.PLAN_DATE).AddSeconds(-1);
            mo.Qty = item.PLAN_QTY;
            mo.Priority = item.PRIORITY;

            mo.InOut = item.IN_OUT;

            mo.LineType = LcdHelper.ToEnum<LineType>(item.LINE_TYPE);

            return mo;
        }            

        internal static FabEqp CreateEqp(Eqp item, SimEqpType type)
        {
            FabEqp eqp = item.ToFabEqp();

            eqp.ResID = item.EQP_ID;
            eqp.Key = item.EQP_ID;

            eqp.SimType = type;

            eqp.FactoryID = item.FACTORY_ID;
            eqp.ShopID = item.SHOP_ID;
            eqp.PresetID = item.PRESET_ID;

            eqp.InStocker = item.IN_STOCKER;
            eqp.OutStocker = item.OUT_STOCKER;

			eqp.UseJigCount = item.USE_JIG_COUNT;

            if (InputMart.Instance.GlobalParameters.ApplyEqpOperationRatio)
                eqp.Utilization = item.OPERATING_RATIO;
            else
                eqp.Utilization = 1f;
                            
            eqp.ResGroup = LcdHelper.ToSafeString(item.DSP_EQP_GROUP_ID);

            eqp.EqpGroup = item.EQP_GROUP_ID;

            eqp.EqpType = item.EQP_TYPE;

            if (eqp.SimType == SimEqpType.Chamber || eqp.SimType == SimEqpType.ParallelChamber)
                eqp.ChildCount = (int)item.CHAMBER_COUNT;

            eqp.SetupTimes = new MultiDictionary<string, SetupTime>();
            eqp.CacheSetupTimes = new DoubleDictionary<string, string, SetupTime>();

            eqp.SubEqps = new Dictionary<string, FabSubEqp>();

            eqp.TransferTimeFrom = new Dictionary<string, int>();
            eqp.TransferTimeTo = new Dictionary<string, int>();

            eqp.State = ResourceState.Up;
            eqp.StatusInfo = CreateHelper.CreateEqpStatus(eqp);

            eqp.MapType = InlineMapType.NONE;
            
            eqp.FromMapEqps = new List<FabEqp>();
            eqp.ToMapEqps = new List<FabEqp>();

            #region Preset Setting
            
            string presetID = item.PRESET_ID;

            FabWeightPreset preset = WeightHelper.GetWeightPreset(presetID);

            var debugPreset = DebugHelper.GetDebugPreset(presetID);
            if (debugPreset != null)
                preset = debugPreset;

            if (preset == null)
                Logger.Warn("[WARNING] invaild presetID : {0}", presetID);
            else
                preset.MapPresetID = presetID;

            eqp.Preset = preset;

            eqp.DispatcherType = LcdHelper.ToEnum<DispatcherType>(item.DISPATCH_RULE);
            eqp.DispatchingRule = item.DISPATCH_RULE;

            if (eqp.Preset == null || eqp.Preset.FactorList.Count == 0)
            {
                if (eqp.DispatcherType != DispatcherType.Fifo)
                {
                    eqp.DispatcherType = DispatcherType.Fifo;
                    eqp.DispatchingRule = eqp.DispatcherType.ToString();
                }
            }

            #endregion

            #region Set Main Run Steps

            eqp.MainRunSteps = new List<FabStdStep>();
            
            if (LcdHelper.IsEmptyID(item.MAIN_RUN_STEP) == false)
            {
                string[] list = item.MAIN_RUN_STEP.Split(',');

                StringBuilder sb = new StringBuilder(); 
                bool isFirst = true;
                foreach (var str in list)
                {
                    FabStdStep stdStep = BopHelper.FindStdStep(item.SHOP_ID, str);
                    #region Write ErrorHistory
                    if (stdStep == null)
                    {
                        ErrHist.WriteIf(string.Format("CreateEqp{0}/step{1}", item.EQP_ID, str),
                            ErrCategory.PERSIST,
                            ErrLevel.INFO,
                            item.FACTORY_ID,
                            item.SHOP_ID,
                            Constants.NULL_ID,
                            Constants.NULL_ID,
                            Constants.NULL_ID,
                            Constants.NULL_ID,
                            item.EQP_ID,
                            str,
                            "NOT FOUND STEP",
                            string.Format("Table:Eqp → Create EQP Object"));

                        continue;
                    }
                    #endregion

                    eqp.MainRunSteps.Add(stdStep);

                    if(isFirst)
                        sb.Append(str);
                    else
                        sb.AppendFormat(",{0}", str);

                    isFirst = false;
                }

                eqp.EqpMainRunsSteps = sb.ToString();
            } 

            #endregion

            return eqp;
        }

        internal static FabWeightFactor CreateWeightFactor(WeightPresets item)
        {
            var factorType = LcdHelper.ToEnum<FactorType>(item.FACTOR_TYPE, FactorType.LOTTYPE);
            var order = LcdHelper.ToEnum<OrderType>(item.ORDER_TYPE, OrderType.ASC);

            bool isAllow = LcdHelper.ToBoolYN(item.ALLOW_FILTER);

            var wfactor = new FabWeightFactor(item.FACTOR_ID,
                                              item.FACTOR_WEIGHT,
                                              item.SEQUENCE,
                                              factorType,
                                              order,
                                              item.CRITERIA,
                                              isAllow);

            return wfactor;
        }

        internal static StepTat CreateStepTat(Tat item, float run, float wait, bool isMainLine)
        {
            StepTat tat = new StepTat();
            tat.ProductID = item.PRODUCT_ID;

            tat.RunTat = run;
            tat.WaitTat = wait;
            tat.TAT = run + wait;

            tat.IsMain = isMainLine;

            return tat;
        }

        /// <summary>
        /// Input Persist용
        /// </summary>
        /// <returns></returns>
        internal static FabWipInfo CreateWip(Wip item, FabStep step, FabProduct prod, double unitQty, bool isSubStepWip)
        {
            FabWipInfo wip = CreateWipInfo(
                item.FACTORY_ID,
                item.SHOP_ID,
                item.LOT_ID,
                item.BATCH_ID,
                prod,
                step,
                item.LOT_STATUS,
                item.GLASS_QTY,
                item.EQP_ID,
                item.STEP_ID,
                item.PROCESS_ID,
                item.PRODUCT_ID,
                item.PRODUCT_VERSION,
                item.STATE_TIME,
                item.TKIN_TIME,
                item.TKOUT_TIME,
                item.PRIORITY,
                step.ProcessID, //MainProcessID
                item.MAIN_STEP_ID,
                item.OWNER_ID,
                item.OWNER_TYPE,
                item.HOLD_CODE,
                item.HOLD_TIME,
                item.SHEET_ID,
                item.SHEET_ID_TIME,
                item.INSP_SHEET_ID,
                item.INSP_SHEET_TIME,
                isSubStepWip,
                true,
                false
                );

            //Is_INPORT = Y && WAIT
            wip.IsInPortWip = item.IS_INPORT.ToBoolYN() && wip.CurrentState == EntityState.WAIT;

            return wip;
        }

        /// <summary>
        /// InputBatch용
        /// </summary>
        /// <param name="lotID"></param>
        /// <param name="prod"></param>
        /// <param name="step"></param>
        /// <param name="unitQty"></param>
        /// <returns></returns>
        internal static FabWipInfo CreateWipInfo(string lotID, FabProduct prod, FabStep step, double unitQty)
        {
            string defaultOwnerType = SiteConfigHelper.GetDefaultOwnerType();
            string defaultOwnerID = SiteConfigHelper.GetDefaultOwnerID();
            
            FabWipInfo wip = CreateWipInfo(
                step.FactoryID,
                step.ShopID,
                lotID,
                Constants.NULL_ID,
                prod,
                step,
                EntityState.WAIT.ToString(),
                unitQty,
                Constants.NULL_ID,
                step.StepID,
                prod.ProcessID,
                prod.ProductID,
                Constants.NULL_ID,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
				SiteConfigHelper.GetDefaultLotPriority(),
                Constants.NULL_ID,
                Constants.NULL_ID,
                defaultOwnerID,
                defaultOwnerType,
                Constants.NULL_ID,
                DateTime.MinValue,
                Constants.NULL_ID,
                DateTime.MinValue,
                Constants.NULL_ID,
                DateTime.MinValue,
                false,
                false,
                true
                );

            return wip;

        }

        /// <summary>
        /// BankWip
        /// </summary>
        public static FabWipInfo CreateWipInfo(BankWip item, FabProduct prod, FabStep step, BankWipStatus wipStatus)
        {
            FabWipInfo wip = CreateWipInfo(item.FACTORY_ID,
            item.SHOP_ID,
            item.LOT_ID,
            item.BATCH_ID,
            prod,
            step,
            item.LOT_STATUS,
            item.GLASS_QTY,
            Constants.NULL_ID,
            item.STEP_ID,
            item.PROCESS_ID,
            item.PRODUCT_ID,
            item.PRODUCT_VERSION,
            item.STATE_TIME,
            item.TKIN_TIME,
            item.TKOUT_TIME,
            item.PRIORITY,
            Constants.NULL_ID,
            Constants.NULL_ID,
            item.OWNER_ID,
            item.OWNER_TYPE,
            Constants.NULL_ID,
            DateTime.MinValue,
            Constants.NULL_ID,
            DateTime.MinValue,
            Constants.NULL_ID,
            DateTime.MinValue,
            false,
            true,
            false) ;

            wip.BankWipStatus = wipStatus;

            return wip;

        }

        private static FabWipInfo CreateWipInfo(
            string factoryID,
            string shopID,
            string lotID,
            string batchID,
            FabProduct prod,
            FabStep step,
            string status,
            double unitQty,
            string wipEqpID,
            string wipStepID,
            string wipProcessID,
            string wipProductID,
            string productVer,
            DateTime stateTime,
            DateTime trackInTime,
            DateTime trackOutTime,
            int priority,
            string preProcess,
            string preStep,
            string ownerID,
            string ownerType,
            string holdCode,
            DateTime holdTime,
            string sheetID,
            DateTime sheetIDTime,
            string inspSheetID,
            DateTime inspSheetIDTime,
            bool isSubStepWip,
            bool isInitWip,
            bool isInputLot
            )
        {
            FabWipInfo wip = new FabWipInfo();


            wip.FactoryID = factoryID;
            wip.ShopID = shopID;

            wip.LotID = lotID;
            wip.BatchID = batchID;

            wip.Product = prod;

            wip.InitialStep = step;
            wip.Process = prod.Process;

            wip.UnitQty = unitQty;

            wip.WipState = status;
            wip.CurrentState = EntityHelper.GetEntityState(status);

            wip.WipEqpID = wipEqpID;
            wip.WipStepID = wipStepID;
            wip.WipProcessID = wipProcessID;
            wip.WipProductID = wipProductID;
            wip.ProductVersion = productVer;

            wip.WipStateTime = stateTime;

            wip.LastTrackInTime = trackInTime;
            wip.LastTrackOutTime = trackOutTime;

            wip.Priority = priority;

            wip.MainProcessID = preProcess;
            wip.MainStepID = preStep;

            wip.OwnerID = ownerID;
            wip.OwnerType = ownerType;

            wip.HoldCode = holdCode;
            wip.HoldTime = holdTime;
            wip.SheetID = sheetID;
            wip.SheetIDTime = sheetIDTime;
            wip.InspSheepID = inspSheetID;
            wip.InspSheetIDTime = inspSheetIDTime;

            wip.IsSubStepWip = isSubStepWip;
            wip.IsInitWip = isInitWip;
            wip.IsInputLot = isInputLot;

            return wip;
        }

        /// <summary>
        /// DummyLot
        /// </summary>
        public static FabWipInfo CreateWipInfoDummy(
            string lotID,
            BatchInfo batchInfo,
            FabProduct product,
            FabProcess process,
            FabStep step,
            string productVer,
            string ownerType,
            string ownerID,
            int priority,
            int unitQty,
            EntityState state,
            FabEqp tkInEqp,
            string eqpID,
            DateTime wipStateTime,
            DateTime lastTrackInTime
            )
        {
            FabWipInfo info = new FabWipInfo();

            info.LotID = lotID;
            info.Batch = batchInfo;
            info.Product = product;
            info.Process = process;
            //info.ProdType = prodType;
            info.ProductVersion = productVer;
            info.Priority = priority;
            info.InitialStep = step;
            info.UnitQty = unitQty;
            info.CurrentState = state;
            info.InitialEqp = tkInEqp;
            info.WipEqpID = eqpID;
            info.WipStateTime = wipStateTime;
            info.LastTrackInTime = lastTrackInTime;

            info.OwnerType = ownerType;
            info.OwnerID = ownerID;

            if (batchInfo != null)
            {
                EntityHelper.AddBatchInfo(batchInfo, step, unitQty);
            }

            return info;
        }

        internal static FabLot CreateLot(FabWipInfo wip, LotState state, bool isDummy = false)
        {
            FabLot lot = new FabLot(wip);

            //lot.WipInfo = wip;
            lot.LotID = wip.LotID;
            lot.Product = wip.Product;
            lot.Priority = wip.Priority;
            lot.UnitQty = (int)wip.UnitQty;
            lot.UnitQtyDouble = wip.UnitQty;
                        
            lot.OrigProductVersion = wip.ProductVersion;
            lot.CurrentProductVersion = lot.OrigProductVersion;

            lot.CurrentState = wip.CurrentState;
            lot.LotState = state;

			lot.PlanSteps = new List<string>();

            //?
            //lot.ReleaseTime = info.CurrentState == EntityState.RUN ? info.LastTrackInTime : info.WipStateTime;

            QTimeMaster.SetQTimeInfo(lot, lot.Wip.InitialStep as FabStep);

            if(isDummy == false)
                OutCollector.AddLotforInit(lot);

            return lot;
        }

        internal static StepTime CreateStepTime(EqpStepTime item, FabStep step)
        {
            StepTime st = new StepTime();

            st.EqpID = item.EQP_ID;
            st.Step = step;
            st.ProductID = item.PRODUCT_ID;
            st.ProcessID = item.PROCESS_ID;
            st.TactTime = item.TACT_TIME;
            st.ProcTime = item.PROC_TIME;

            return st;
        }

        internal static StepTime CreateStepTime(FabEqp eqp, FabStep step, string productID, float tactTime, float procTime)
        {
            StepTime st = new StepTime();

            st.EqpID = eqp.EqpID;
            st.Step = step;
            st.TactTime = tactTime;
            st.ProcTime = procTime;
            st.ProductID = productID;

            return st;
        }

        internal static InFlowSteps CreateInFlowSteps(string eqpID, string productID)
        {
            InFlowSteps steps = new InFlowSteps();

            steps.EqpID = eqpID;
            steps.ProductID = productID;
            steps.Steps = new List<FabStep>();

            return steps;
        }

        public static JobFilterInfo CreateDispatchFilterInfo(FabStep step, string productID, string prodVer, string ownerType, string ownerID)
        {
            JobFilterInfo info = new JobFilterInfo();
            info.ProductID = productID;
            info.ProductVersion = prodVer;
            info.Step = step;
            info.LotList = new List<FabLot>();
            info.OwnerType = ownerType;
            info.OwnerID = ownerID;

            return info;
        }

        public static BatchInfo CreateBatchInfo(string batchID)
        {
            BatchInfo batch = new BatchInfo();

            batch.BatchID = batchID;
            batch.BatchQty = 0;

            return batch;
        }

        public static FabLot CreateDispatchDummyLot(FabStep step, FabPlanInfo lastPlan)
        {
            FabWipInfo info = CreateWipInfoDummy(
                "DUMMY",
                null,
                lastPlan.Product,
                step.Process as FabProcess,
                step,
                lastPlan.ProductVersion,
                lastPlan.OwnerType,
                lastPlan.OwnerID,
				SiteConfigHelper.GetDefaultLotPriority(),
                0,
                EntityState.RUN,
                null,
                string.Empty,
                DateTime.MinValue,
                DateTime.MinValue);

            FabLot lot = CreateLot(info, LotState.WIP, true);
            lot.MoveFirst(AoFactory.Current.NowDT);
            lot.IsDummy = true;

            return lot;
        }

        internal static FabPlanWip CreatePlanWip(FabWipInfo wip)
        {
            FabPlanWip planWip = new FabPlanWip(wip);

            planWip.Step = wip.InitialStep as FabStep;
            planWip.MapStep = wip.InitialStep;

            planWip.AvailableTime = wip.AvailableTime;

            return planWip;
        }

        internal static FabInterBom CreateFabInterBom(FabProduct prod, FabStep step, FabProduct changeProd, FabStep changeStep, bool isFromTo)
        {
            FabInterBom bom = new FabInterBom();

            bom.Product = prod;
            bom.CurrentStep = step;

            bom.ChangeProduct = changeProd;
            bom.ChangeStep = changeStep;

            bom.IsFromToRoute = isFromTo;


            return bom;
        }        

        internal static SetupTime CreateSetupTime(SetupTimes item)
        {
            SetupTime st = new SetupTime();

            st.Time = item.SETUP_TIME;
            st.EqpID = item.EQP_ID;
            st.EqpGroup = item.EQP_GROUP;

            st.FromStepID = item.FROM_STEP_ID;
            st.FromProductID = item.FROM_PRODUCT_ID;
            st.FromProductVersion = item.FROM_PRODUCT_VERSION;
            st.FromEtc = item.FROM_ETC;

            st.ToStepID = item.TO_STEP_ID;
            st.ToProductID = item.TO_PRODUCT_ID;
            st.ToProductVersion = item.TO_PRODUCT_VERSION;
            st.ToEtc = item.TO_ETC;

            st.ChangeType = item.CHANGE_TYPE;
            st.ChangeTypeList = new List<ChangeType>();
            st.Priority = item.PRIORITY;

            if (st.FromStepID.IsSameID())
                st.FromStepID = st.ToStepID;

            if (st.FromProductID.IsSameID())
                st.FromProductID = st.ToProductID;

            if (st.FromProductVersion.IsSameID())
                st.FromProductVersion = st.ToProductID;

            if (st.FromEtc.IsSameID())
                st.FromEtc = st.ToEtc;

            if (st.ToStepID.IsSameID())
                st.ToStepID = st.FromStepID;

            if (st.ToProductID.IsSameID())
                st.ToProductID = st.FromProductID;

            if (st.ToProductVersion.IsSameID())
                st.ToProductVersion = st.FromProductVersion;

            if (st.ToEtc.IsSameID())
                st.ToEtc = st.FromEtc;

            var arr = LcdHelper.ToListString(st.ChangeType);
            foreach (string str in arr)
            {
                ChangeType changeType = LcdHelper.ToEnum(str, ChangeType.NONE);
                if (changeType == ChangeType.NONE)
                    continue;

                st.ChangeTypeList.Add(changeType);
            }

            st.IsEqpAll = st.EqpID.IsAllID();

            return st;
        }

		internal static SetupInfo CreateSetupInfo(AoEquipment aeqp, string shopID, string stepID, 
            string productID, string prodVer, string ownerType, string ownerID)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

            var targetEqp = eqp.TargetEqp;
            var last = eqp.GetLastPlan();

            SetupInfo info = CreateSetupInfo(targetEqp, 
                                             last, 
                                             shopID, 
                                             stepID,
                                             productID, 
                                             prodVer, 
                                             ownerType, 
                                             ownerID);

            return info;
		}

        internal static SetupInfo CreateSetupInfo(FabSubEqp subEqp, string shopID, string stepID,
            string productID, string prodVer, string ownerType, string ownerID)
        {
            var targetEqp = subEqp.Parent as FabEqp;
            var curr = subEqp.CurrentLot;
            if (curr != null)
            {
                SetupInfo info = CreateSetupInfo(targetEqp,
                                 curr,
                                 shopID,
                                 stepID,
                                 productID,
                                 prodVer,
                                 ownerType,
                                 ownerID);

                return info;
            }
            else
            {
                var last = subEqp.LastPlan as FabPlanInfo;
                SetupInfo info = CreateSetupInfo(targetEqp,
                                                 last,
                                                 shopID,
                                                 stepID,
                                                 productID,
                                                 prodVer,
                                                 ownerType,
                                                 ownerID);

                return info;
            }
        }

        private static SetupInfo CreateSetupInfo(FabEqp targetEqp, FabPlanInfo last, string shopID, string stepID,
            string productID, string prodVer, string ownerType, string ownerID)
        {
            SetupInfo info = new SetupInfo()
            {
                Eqp = targetEqp
            };

            if (last != null)
            {
                info.FromShopID = last.ShopID;
                info.FromStepID = last.StepID;
                info.FromProductID = last.ProductID;
                info.FromProductVersion = last.ProductVersion;
                info.FromOwnerType = last.OwnerType;
                info.FromOwnerID = last.OwnerID;
            }

            info.ToShopID = shopID;
            info.ToStepID = stepID;
            info.ToProductID = productID;
            info.ToProductVersion = prodVer;
            info.ToOwnerType = ownerType;
            info.ToOwnerID = ownerID;

            return info;
        }

        private static SetupInfo CreateSetupInfo(FabEqp targetEqp, FabLot lot, string shopID, string stepID, 
            string productID, string prodVer, string ownerType, string ownerID)
        {
            SetupInfo info = new SetupInfo()
            {
                Eqp = targetEqp
            };

            if (lot != null)
            {
                info.FromShopID = lot.CurrentShopID;
                info.FromStepID = lot.CurrentStepID;
                info.FromProductID = lot.CurrentProductID;
                info.FromProductVersion = lot.CurrentProductVersion;
                info.FromOwnerType = lot.OwnerType;
                info.FromOwnerID = lot.OwnerID;
            }

            info.ToShopID = shopID;
            info.ToStepID = stepID;
            info.ToProductID = productID;
            info.ToProductVersion = prodVer;
            info.ToOwnerType = ownerType;
            info.ToOwnerID = ownerID;

            return info;
        }

        internal static HoldInfo CreateHoldInfo(HoldTime item)
        {
            HoldInfo info = new HoldInfo();

            info.FactoryID = item.FACTORY_ID;
            info.ShopID = item.SHOP_ID;
            info.HoldCode = item.HOLD_CODE;
            info.HoldTime = item.HOLD_TIME;

            return info;

        }

        //internal static FabLoadInfo CreateLoadInfo(LoadStates state)
        //{
        //    return CreateLoadInfo(null, DateTime.MinValue, state, Constants.NULL_ID, Constants.NULL_ID);
        //}

        //internal static FabLoadInfo CreateLoadInfo(FabLot lot, DateTime startTime, LoadStates state, string maskID, string subEqp)
        //{
        //    FabLoadInfo info = new FabLoadInfo();

        //    info.SubEqpID = subEqp;
        //    info.StartTime = startTime;
        //    info.State = state;
        //    info.ToolID = maskID;

        //    if (lot != null)
        //    {
        //        info.Step = lot.CurrentFabStep;
        //    }

        //    info.PlanInfos = new List<FabPlanInfo>();

        //    return info;
        //}

        internal static EqpStatusInfo CreateEqpStatus(FabEqp eqp)
        {
            EqpStatusInfo info = new EqpStatusInfo();

            info.FactoryID = eqp.FactoryID;
            info.ShopID = eqp.ShopID;
            info.EqpID = eqp.EqpID;

            info.MesStatus = MesEqpStatus.IDLE;
            info.Status = ResourceState.Up;

            info.StartTime = ModelContext.Current.StartTime;

            return info;
        }

        internal static EqpStatusInfo CreateEqpStatus(EqpStatus item, DateTime planStartTime, Time statusCheckTime, Time defaultDownTime)
        {
            EqpStatusInfo info = new EqpStatusInfo();

            info.FactoryID = item.FACTORY_ID;
            info.ShopID = item.SHOP_ID;

            info.EqpID = item.EQP_ID;
            info.StartTime = item.EVENT_START_TIME;
            info.EndTime = item.EVENT_END_TIME;

            info.LastShopID = item.LAST_SHOP_ID;
            info.LastProduct = item.LAST_PRODUCT_ID;
            info.LastProductVer = item.LAST_PRODUCT_VERSION;            
            info.LastStep = item.LAST_STEP_ID;
            info.LastOwnerType = item.LAST_OWNER_TYPE;
            info.LastAcidDensity = item.LAST_ACID_DENSITY;

            info.ReasonCode = item.REASON_CODE;
            info.MesStatus = SimHelper.GetEqpStatus(item.STATUS);
            info.OrigineStatus = info.MesStatus;

			info.LastContinuousQty = item.LAST_CONTINUOUS_QTY;

            if (info.MesStatus == MesEqpStatus.RUN || info.MesStatus == MesEqpStatus.E_RUN
                || info.MesStatus == MesEqpStatus.IDLE || info.MesStatus == MesEqpStatus.W_CST)
            {
                info.Status = ResourceState.Up;
            }
            else
            {
                info.Status = ResourceState.Down;
            }

            //EndTime(MES:DUEDATE) < planStartTime (과거)인 경우 EndTime 무시
            if (info.EndTime < planStartTime)
                info.EndTime = DateTime.MinValue;

            //UP 상태이고, EndTime(MES:DUEDATE)가 존재하고,
            //StartTime(MES:EVENTSTARTTIME)의 기간이 statusCheckTime 이내인 경우 
            //Status 상태 무시하고 EndTime까지 DOWN 처리.
            if (info.Status == ResourceState.Up && info.EndTime > DateTime.MinValue)
            {
                var diff = planStartTime - info.StartTime;
                if (diff < statusCheckTime)
                {
                    info.Status = ResourceState.Down;
                    info.MesStatus = MesEqpStatus.DOWN;
                }
            }

            //EndTime(MES:DUEDATE)이 없는 DOWN 상태에 대한 EndTime 설정
            if (info.Status == ResourceState.Down && info.EndTime == DateTime.MinValue)
            {
                if (info.MesStatus == MesEqpStatus.OFF)
                {
                    info.EndTime = DateTime.MaxValue;
                }
                else
                {
                    //DOWN(OFF제외) 설비는 현시각부터 REASON_CODE별 HoldTime 반영(Default=30분)
                    double downTime = defaultDownTime.TotalMinutes;

                    HoldInfo holdInfo = HoldMaster.GetHoldInfo(item.SHOP_ID, item.REASON_CODE);
                    if (holdInfo != null)
                        downTime = holdInfo.HoldTime;

                    info.EndTime = planStartTime.AddMinutes(downTime);
                }
            }

            return info;
        }

        internal static InOutAct CreateInOutActInfo(FabInOutAct item)
        {
            InOutAct act = new InOutAct();

            act.FactoryID = item.FACTORY_ID;
            act.ShopID = item.SHOP_ID;
            act.ProductID = item.PRODUCT_ID;
            act.ProcessID = item.PROCESS_ID;
            act.ProductVersion = item.PRODUCT_VERSION;
            act.OwnerType = item.OWNER_TYPE;
            act.OwnerID = item.OWNER_ID;
            act.InQty = item.IN_QTY;
            act.OutQty = item.OUT_QTY;

            act.AvailableTime = item.RPT_DATE;

            return act;
        }

        internal static EqpPairInfo CreateEqpPairInfo(FabEqp fromEqp, FabEqp toEqp, FabStdStep fromStep, FabStdStep toStep)
        {
            EqpPairInfo info = new EqpPairInfo();

            info.FromEqp = fromEqp;
            info.ToEqp = toEqp;
            info.FromStep = fromStep;
            info.ToStep = toStep;

            return info;

        }

        //internal static JobState CreateJobState(FabLot lot)
        //{
        //    JobState state = new JobState();

        //    state.Product = lot.Prod;
        //    state.OwnerType = lot.OwnerType;

        //    state.WipProfile = new Dictionary<string, WipProfile>();
        //    state.StdStepWips = new Dictionary<string, WipVar>();
        //    state.StepRouteInfoDic = new Dictionary<string, StepRouteInfo>();

        //    foreach (FabStep item in state.Process.Steps)
        //    {
        //        StepRouteInfo info = CreateHelper.CreateStepRoute(item);
        //        state.StepRouteInfoDic[item.StdStepID] = info;
        //    }

        //    return state;
        //}

        //private static StepRouteInfo CreateStepRoute(FabStep step)
        //{
        //    StepRouteInfo info = new StepRouteInfo();

        //    info.LoadableEqps = new List<string>();
        //    info.LoadedEqps = new List<string>();

        //    info.LoadableEqps = step.StdStep.GetLoadableEqpList();

        //    //Todo

        //    info.TactSec = 0;// step.GetAvgTactTime();
        //    info.RunTAT = 0;// step.GEtAvgFlowTime();

        //    if (info.WaitTAT < 0)
        //        info.WaitTAT = 600;

        //    return info;
        //}

        //internal static LotLocation CreateLotLocation(FabLot lot, EventType eventType)
        //{
        //    LotLocation loc = new LotLocation();

        //    loc.Lot = lot;
        //    loc.Location = CreateHelper.CreateLocationInfo(lot, eventType);

        //    return loc;
        //}

        //private static LocationInfo CreateLocationInfo(FabLot lot, EventType eventType)
        //{
        //    LocationInfo loc = new LocationInfo();

        //    loc.EventType = eventType;
        //    loc.StdStepSeq = lot.CurrentFabStep.StdStepID;
        //    loc.Eqp = lot.CurrentFabPlan.IsLoaded ? lot.CurrentFabPlan.LoadedResource as FabEqp : lot.LastLoadedEqp;

        //    return loc;


        //}

        internal static ProfileGraph CreateProfileGraph(string key)
        {
            ProfileGraph graph = new ProfileGraph();

            graph.Key = key;
            graph.Tol = 1E-10 / 3600;//0.0000000277778;  // 1/10000초

            graph._in = new List<Pt2D>();
            graph._cumIn = new List<Pt2D>();
            graph._cumOut = new List<Pt2D>();

            graph._in.Add(new Pt2D(0, 0));
            graph._cumIn.Add(new Pt2D(0, 0));
            graph._cumOut.Add(new Pt2D(0, 0));


            return graph;
        }

        internal static EqpArrangeInfo CreateEqpArrangeInfo(EqpArrange entity)
        {
            EqpArrangeInfo info = new EqpArrangeInfo();

            info.Key = LcdHelper.CreateKey(entity.EQP_ID, entity.PRODUCT_ID, entity.PRODUCT_VERSION, entity.STEP_ID, entity.MASK_ID);

            info.FactoryID = entity.FACTORY_ID;
            info.ShopID = entity.SHOP_ID;

            info.EqpID = entity.EQP_ID;
            info.StepID = entity.STEP_ID;
            info.ProductID = entity.PRODUCT_ID;
            info.ProductVer = entity.PRODUCT_VERSION;

            info.LimitType = LcdHelper.ToUpper(entity.LIMIT_TYPE);
            info.LimitTypeList = LcdHelper.ParseLimitType(info.LimitType);

            info.ActivateType = LcdHelper.ToEnum(entity.ACTIVATE_TYPE, ActivateType.NONE);

            info.LimitQty = entity.LIMIT_QTY;
            info.InitActQty = entity.ACTUAL_QTY;
            info.MoveQty = info.InitActQty;

            info.IsDailyMode = LcdHelper.ToBoolYN(entity.DAILY_MODE);
            info.DueDate = entity.DUE_DATE;

            info.MaskID = entity.MASK_ID;

            return info;
        }

        internal static FabMask CreateFabMask(Tool item, string jigID = Constants.NULL_ID)
        {
            FabMask mask = new FabMask();

            mask.FactoryID = item.FACTORY_ID;
            mask.ShopID = item.SHOP_ID;
            mask.ToolID = item.TOOL_ID;
			mask.JigID = jigID;
			mask.ToolType = LcdHelper.ToEnum(item.TOOL_TYPE, ToolType.None);

            string eqpID = Constants.NULL_ID;
            string location = Constants.NULL_ID;  //LcdHelper.ToSafeString(item.LOCATION);

            FabEqp eqp = ResHelper.FindEqp(item.EQP_ID);
            if (eqp != null)
            {
                eqpID = eqp.EqpID;
                location = eqpID;
            }

            mask.InitEqpID = eqpID;
            mask.InitLoacation = location;
            mask.InitStateCode = item.STATE_CODE;

            mask.EqpID = eqpID;
            mask.Location = location;
            mask.StateCode = LcdHelper.ToEnum(item.STATE_CODE, ToolStatus.WAIT);

            mask.AvailableTime = ModelContext.Current.StartTime;
            mask.StateChangeTime = item.STATE_CHANGE_TIME;

            mask.LoadInfos = new List<MaskLoadInfo>();
			mask.LoadInfosView = new Dictionary<string, MaskLoadInfo>();

            mask.Qty = item.QTY;

            mask.AllEqps = new List<string>();
            mask.AllProduct = new Dictionary<string, List<string>>();
            mask.AllSteps = new List<string>();

            return mask;

        }
        
        internal static MaskArrange CreateMaskArrange(ToolArrange item, FabMask mask)
        {
            MaskArrange arr = new MaskArrange();

            arr.FactoryID = item.FACTORY_ID;
            arr.ShopID = item.SHOP_ID;

            arr.EqpID = item.EQP_ID;
            arr.StepID = item.STEP_ID;

            arr.Mask = mask;

            arr.ProductID = item.PRODUCT_ID;
            arr.ProductVersion = item.PRODUCT_VERSION;
            arr.Priority = item.PRIORITY;

            return arr;
        }
               
        internal static StayHour CreateStayHour(StayHours item, FabProduct prod, FabStep step, FabStep toStep, Time qtime, QTimeType qtype)
        {
            StayHour sh = new StayHour();

            sh.FatoryID = item.FACTORY_ID;
            sh.ShopID = item.SHOP_ID;

            sh.Product = prod;

            sh.FromStep = step;
            sh.ToStep = toStep;

            sh.QTime = qtime;
            sh.QType = qtype;

            sh.FromStepOutTime = DateTime.MinValue;
            sh.CurrStepInTime = DateTime.MinValue;

            sh.StepList = new List<FabStep>();

            FabStep currStep = step;
            FabProduct currProd = prod;
                        
            int safeCnt = 1;
            while (currStep != null)
            {
                if (safeCnt > 999)
                    break;

                FabStep nextStep = currStep.GetNextStep(currProd, ref currProd);
                if (nextStep == null)
                    break;

                if (sh.StepList.Contains(currStep) == false)
                    sh.StepList.Add(currStep);

                if (currStep.StepID == toStep.StepID)
                    break;

                currStep = nextStep;
                safeCnt++;
            }

            return sh;
        }

        internal static FabPMSchedule CreateFabPMSchedule(DateTime evnetTime, int duration, ScheduleType type, float allowAheadTime, float allowDelayTime)
        {
            FabPMSchedule schedule = new FabPMSchedule(evnetTime, duration, type, allowAheadTime, allowDelayTime);


            return schedule;
        }

        internal static QTimeInfo CreateQTimeInfo(FabLot lot)
        {
            QTimeInfo info = new QTimeInfo();

            info.List = new List<StayHour>();
            info.MinList = new List<StayHour>();
            info.MaxList = new List<StayHour>();

            info.Lot = lot;

            return info;
        }

        internal static FabSubEqp CreateFabSubEqp(EqpChamber item, FabEqp eqp)
        {
            FabSubEqp sub = new FabSubEqp();

            sub.FactoryID = item.FACTORY_ID;
            sub.ShopID = item.SHOP_ID;

            sub.SubEqpID = item.CHAMBER_ID;
            sub.Parent = eqp;

            sub.ArrangeStep = BopHelper.FindStdStep(item.SHOP_ID, item.ARRANGE_STEP);
            sub.State = ResourceState.Up;

            sub.LoadInfos = new List<FabLoadInfo>();

            return sub;

        }

      

        internal static CellBom CreateCellBom(CellCodeMap item, CellActionType type)
        {
            CellBom bom = new CellBom();

            bom.FactoryID = item.FACTORY_ID;
            bom.ActionType = type;
            bom.CellCode = LcdHelper.ToSafeString(item.CELL_CODE);

            bom.FromShopID = item.FROM_SHOP_ID;
            bom.FromProductID = item.FROM_PRODUCT_ID;
            bom.FromProductVer = item.FROM_PRODUCT_VERSION;

            bom.ToShopID = item.TO_SHOP_ID;
            bom.ToProductID = item.TO_PRODUCT_ID;
            bom.ToProductVer = item.TO_PRODUCT_VERSION;

            return bom;

        }

        internal static EqpRecipeInfo CreateEqpRecipeInfo(FabEqp eqp, EqpRecipeTime item)
        {
            EqpRecipeInfo info = new EqpRecipeInfo();

            info.Eqp = eqp;

            info.FactoryID = item.FACTORY_ID;
            info.ShopID = item.SHOP_ID;
            info.ProductID = item.PRODUCT_ID;
            info.ProductVersion = item.PRODUCT_VERSION;
            info.StepID = item.STEP_ID;
            info.DueDate = item.DUE_DATE;
            info.PlanDueDate = item.DUE_DATE;
            info.CheckFlag = item.CHECK_FLAG;
            info.MaxCount = item.MAX_COUNT;
            info.TrackInCount = item.TRACK_IN_COUNT;
            info.RunMode = item.RUN_MODE;
            info.ToolID = item.TOOL_ID;

            info.Flag = LcdHelper.ToEnum<RecipeFlag>(item.CHECK_FLAG, RecipeFlag.None);

            info.ActiveStartTime = Time.MinValue;

            //TODO : 시간입력받으면 수정필요 (Default 6시간)
            info.Duration = Time.FromMinutes(6 * 60);

            return info;
        }

        internal static OutInfo CreateOutInfo(FabWipInfo wip, DateTime outTime)
        {
            return CreateOutInfo
                (
                wip.FactoryID,
                wip.ShopID,
                wip.WipProductID,
                wip.ProductVersion,
                wip.OwnerType,
                (int)wip.UnitQty,
                outTime,
                true,
                wip.WipStepID
                );
        }


        internal static OutInfo CreateOutInfo(FabLot lot, DateTime outTime)
        {
            return CreateOutInfo
                (
                lot.CurrentFactoryID,
                lot.CurrentShopID,
                lot.CurrentProductID,
                lot.CurrentProductVersion,
                lot.OwnerType,
                lot.UnitQty,
                outTime,
                false,
                lot.CurrentStepID
                ) ;
        }

        internal static OutInfo CreateOutInfo(string factorID, string shopID, string productID, string prodVer, string ownerType, int qty, DateTime outTime, bool isWip, string stepID)
        {
            OutInfo info = new OutInfo();

            info.FactoryID = factorID;
            info.ShopID = shopID;
            info.ProductID = productID;
            info.ProductVersion = prodVer;
            info.OwnerType = ownerType;
            info.Qty = qty;
            info.ReleaseTime = outTime;
            info.IsWip = isWip;
            info.StepID = stepID;

            return info;
        }


        internal static ShopInTarget CreateShopInTarget(FabPegTarget pt, FabProduct prod, FabStep step)
        {
            ShopInTarget target = new ShopInTarget();

            target.Product = prod;
            target.TargetStep = step;
            target.TargetDate = pt.CalcDate;

            target.Targets = new List<FabPegTarget>();
            target.Mo = new List<FabMoPlan>();
            target.StepID = step.StepID;
            return target;
        }

        internal static CellInProfile CreateCellInProfile(FabProduct prod, string ownerType)
        {
            CellInProfile profile = new CellInProfile();

            profile.Product = prod;
            profile.OwnerType = ownerType;
            
            return profile;
        }


        internal static FabOutProfile CreateFabOutProfile(OutInfo info)
        {
            FabOutProfile profile = new FabOutProfile();

            profile.FactoryID = info.FactoryID;
            profile.ShopID = info.ShopID;
            profile.ProductID = info.ProductID;

            profile.Infos = new List<ProfileItem>();

            return profile;
        }


		internal static InInfo CreateInInfo(CellInProfile profile, DateTime inTime, int qty, ShopInTarget inTarget)
        {
            InInfo info = new InInfo();
            
            info.FactoryID = profile.FactoryID;
            info.ShopID = profile.ShopID;
            info.ProductID = profile.ProductID;

            //info.ProductVersion = profile.ProductVersion;
            info.OwnerType = profile.OwnerType;

            info.ReleaseTime = inTime;
            info.Qty = qty;
            info.InTarget = inTarget;

            return info;
        }
        
        internal static AcidDensity CreateAcidDensity(FabAoEquipment eqp)
        {
            AcidDensity acid = new AcidDensity();
            acid.Eqp = eqp;

            return acid;
        }

		internal static OwnerLimitInfo CreateOwnerLimitInfo(OwnerLimit item)
		{
			OwnerLimitInfo info = new OwnerLimitInfo();
			info.FactoryID = item.FACTORY_ID;
			info.StepID = item.STEP_ID;
			info.OwnerID = item.OWNER_ID;

            info.YList = new List<string>();
            info.NList = new List<string>();

            return info;
		}

        internal static BranchStepInfo CreateBranchStepInfo(BranchStep item)
        {
            BranchStepInfo info = new BranchStepInfo();

            info.EqpGroup = item.EQP_GROUP_ID;
            info.StepID = item.STEP_ID;
            info.NextStepID = item.NEXT_STEP_ID;
            info.Priority = item.PRIORITY;

            string productID = item.PRODUCT_ID;

            if (string.IsNullOrEmpty(productID)
                || LcdHelper.IsEmptyID(productID)
                || LcdHelper.Equals(productID, "ALL"))
            {
                info.IsAllProduct = true;
            }
            else
            {
                info.IsAllProduct = false;
                info.ProductList = LcdHelper.ToListString(item.PRODUCT_ID, ",");
            }
            
            return info;
        }
    }
}
