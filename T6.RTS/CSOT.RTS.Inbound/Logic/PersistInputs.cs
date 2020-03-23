using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.RTS.Inbound.Inputs;
using CSOT.RTS.Inbound.Outputs;
using CSOT.RTS.Inbound.Persists;
using Mozart.Task.Execution.Persists;
using CSOT.RTS.Inbound.DataModel;
using Mozart.SeePlan;
using Mozart.Data.Entity;

namespace CSOT.RTS.Inbound.Logic
{
    [FeatureBind()]
    public partial class PersistInputs
    {
        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_JobRunMonitor(IPersistContext context)
        {
            if (RunStateMasterFunc.IsAutoRun() == false)
                return;

            //only ENG_RUN Mode
            if (LcdHelper.GetRunType() != InboudRunType.ENG_RUN)
                return;

            Inputs.JobRunMonitor runState = InputMart.Instance.RunStateMst.SafeGet();                                   
            
            InputMart.Instance.RunStateMst.OnStart(runState);
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_STD_STEP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_STD_STEP;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_STD_STEP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.AREA_ID = entity.AREA_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.LAYER_ID = entity.LAYER_ID;
                newEntity.DSP_EQP_GROUP_ID = entity.DSP_EQP_GROUP_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.STEP_DESC = entity.DESCRIPT;
                newEntity.STEP_TYPE = entity.STEP_TYPE;

                newEntity.IS_MANDATORY = entity.IS_MANDATORY;                
                newEntity.STEP_SEQ = entity.STEP_SEQ;
                newEntity.DEFAULT_ARRANGE = entity.DEFAULT_ARRANGE;
                newEntity.RECIPE_LPOBM = entity.RECIPE_LPOBM;

                newEntity.BALANCE_TO_STEP = entity.BALANCE_TO_STEP;
                newEntity.BALANCE_WIP_QTY = entity.BALANCE_WIP_QTY;
                newEntity.BALANCE_GAP = entity.BALANCE_GAP;

                newEntity.IS_USE_MASK = entity.IS_USE_MASK;
                newEntity.IS_USE_JIG = entity.IS_USE_JIG;
                newEntity.DEFAULT_RUN_MODE = entity.DEFAULT_RUN_MODE;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;
                
                OutputMart.Instance.ENG_STD_STEP.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_PRODUCT(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_PRODUCT;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_PRODUCT();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.PRODUCT_ID = entity.PROD_ID;
                newEntity.PROCESS_ID = entity.PROC_ID;                
                newEntity.PRODUCT_TYPE = entity.PROD_TYPE;
                newEntity.INCH = (float)Math.Round(entity.CELL_SIZE, 3);
                newEntity.PRODUCT_KIND = entity.PROD_KIND;
                newEntity.VIEW_COLOR = entity.DISP_COLOR;  

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                //newEntity.PRODUCT_GROUP = null;
                //newEntity.PRODUCT_UNIT = null;
                //newEntity.CST_SIZE = 0;
                //newEntity.BATCH_SIZE = 0;
                //newEntity.PPG = 1;

                OutputMart.Instance.ENG_PRODUCT.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_EQP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_EQP;
            
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                string simType = entity.SIM_TYPE;

                var newEntity = new ENG_EQP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.EQP_TYPE = entity.EQP_TYPE;
                newEntity.EQP_GROUP_ID = entity.EQP_GROUP_ID;
                newEntity.DSP_EQP_GROUP_ID = entity.DSP_EQP_GROUP_ID;
                newEntity.LOCATION = entity.LOCATION;
                newEntity.PHASE_NO = entity.PHASE_NO;
                newEntity.SIM_TYPE = simType;
                newEntity.DISPATCH_RULE = entity.DISPATCH_RULE;
                newEntity.PRESET_ID = entity.PRESET_ID;
                newEntity.OPERATING_RATIO = entity.OPERATING_RATIO;
                newEntity.MAX_BATCH_SIZE = entity.MAX_BATCH_SIZE;
                newEntity.MIN_BATCH_SIZE = entity.MIN_BATCH_SIZE;
                newEntity.BATCH_WAIT_TIME = entity.BATCH_WAIT_TIME;

                if (LcdHelper.IsChamberType(simType))
                    newEntity.CHAMBER_COUNT = Math.Max(entity.CHAMBER_COUNT, 1);

                newEntity.START_TIME = DateTime.MinValue;
                newEntity.END_TIME = DateTime.MaxValue;
                
                newEntity.IS_ACTIVE = entity.IS_ACTIVE;
                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;
                newEntity.IN_STOCKER = entity.IN_STOCKER;
                newEntity.OUT_STOCKER = entity.OUT_STOCKER;

                newEntity.VIEW_SEQ = entity.VIEW_SEQ;
                newEntity.MAIN_RUN_STEP = entity.MAIN_RUN_STEP;
                newEntity.USE_JIG_COUNT = entity.USE_JIG_COUNT;

                OutputMart.Instance.ENG_EQP.Add(newEntity);
            }
        }
                
        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_TAT(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_TAT;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_TAT();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.PROCESS_ID = entity.PROC_ID;
                newEntity.PRODUCT_ID = entity.PROD_ID;
                newEntity.STEP_ID = entity.STEP_ID;

                //KEY NOT NULL
                if (string.IsNullOrEmpty(entity.PROD_ID)
                    || string.IsNullOrEmpty(entity.PROC_ID)
                    || string.IsNullOrEmpty(entity.STEP_ID))
                {
                    continue;
                }

                string timeUnit = entity.TIME_UNIT;
                var wt = LcdHelper.ParseTimeSpan(Convert.ToSingle(entity.ENG_WAIT_TAT), timeUnit);
                var rt = LcdHelper.ParseTimeSpan(Convert.ToSingle(entity.ENG_RUN_TAT), timeUnit);

                var u_wt = LcdHelper.ParseTimeSpan(Convert.ToSingle(entity.WAIT_TAT), timeUnit);
                var u_rt = LcdHelper.ParseTimeSpan(Convert.ToSingle(entity.RUN_TAT), timeUnit);

                //engine minute base
                newEntity.WAIT_TAT = (float)Math.Round(wt.TotalMinutes, 3);
                newEntity.RUN_TAT = (float)Math.Round(rt.TotalMinutes, 3);

                newEntity.U_WAIT_TAT = (float)Math.Round(u_wt.TotalMinutes, 3);
                newEntity.U_RUN_TAT = (float)Math.Round(u_rt.TotalMinutes, 3);

                newEntity.U_STEP_TAT = newEntity.U_WAIT_TAT + newEntity.U_RUN_TAT;
                newEntity.STEP_TAT = newEntity.WAIT_TAT + newEntity.RUN_TAT;


                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;                                

                OutputMart.Instance.ENG_TAT.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_STEP_TIME(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_STEP_TIME;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;

            DateTime versionDate = LcdHelper.GetVersionDate();

            string eqpStepTimeType = GlobalParameters.Instance.EqpStepTimeType;
            var estType = LcdHelper.ToEnum(eqpStepTimeType, EqpStepTimeType.DEFAULT);
            
            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                string factoryID = entity.FACTORY_ID;
                string shopID = entity.SHOP_ID;
                string eqpID = entity.EQP_ID;

                var newEntity = new ENG_EQP_STEP_TIME();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = factoryID;
                newEntity.SHOP_ID = shopID;
                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.PRODUCT_ID = entity.PROD_ID;
                newEntity.PROCESS_ID = entity.PROC_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                                
                float tactTime = Convert.ToSingle(entity.TACT_TIME);
                float procTime = Convert.ToSingle(entity.PROC_TIME);

                if(estType == EqpStepTimeType.LONG)
                {
                    tactTime = Convert.ToSingle(entity.LONG_TACT_TIME);
                    procTime = Convert.ToSingle(entity.LONG_PROC_TIME);
                }

                //在CELL PI同时投入产出TFT/CF
                //CELL PI에서 TFT/CF 동시에 투입                
                if (entity.STEP_ID == "2100")
                    tactTime = tactTime * 2;

                //CHAMBER : 以User维护的所有Chamber为基准注册到RTS Table（By Chamber转换 TactTime）
                //CHAMBER : RTS Table에 User가 등록시 전체 Chamber 기준으로 등록함(Chamber별 TactTime으로 변환)
                bool isChamber = MainHelper.IsChamberEqp(factoryID, shopID, eqpID);
                if(isChamber)
                {
                    int chamberCount = MainHelper.GetChamberCount(factoryID, shopID, eqpID);
                    tactTime = tactTime * chamberCount;
                }

                newEntity.TACT_TIME = tactTime;
                newEntity.PROC_TIME = procTime;
                
                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;
                
                OutputMart.Instance.ENG_EQP_STEP_TIME.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_PROCSTEP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_PROCSTEP;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;

            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_PROCSTEP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.PROCESS_ID = entity.PROC_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.STEP_DESC = entity.DESCRIPT;
                newEntity.NEXT_STEP_ID = entity.NEXT_STEP_ID;
                newEntity.STEP_SEQ = entity.STEP_SEQ;                
                newEntity.TRANSFER_TIME = entity.TRANSFER_TIME;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_PROCSTEP.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_LOT(IPersistContext context)
        {
            var table = InputMart.Instance.IF_LOT;

            if (table == null || table.Rows.Count == 0)
                return;
                        
            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;
                        
            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string identityNull = LcdHelper.IdentityNull();

            foreach (var entity in table.DefaultView)
            {                                
                var newEntity = new ENG_WIP();

                newEntity.VERSION_DATE = LcdHelper.GetVersionDate();
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.ORIENTEDSITE;
                newEntity.SHOP_ID = entity.FACTORYNAME;
                newEntity.LOT_ID = entity.LOTNAME;

                //TODO : BATCH_ID (CST_ID)
                newEntity.BATCH_ID = LcdHelper.ToSafeString(entity.CARRIERNAME, identityNull);
               
                string shopID = entity.FACTORYNAME;
                string stepID = entity.PROCESSOPERATIONNAME;
                string prevStepID = entity.PRE_STEP_ID;

                bool existRtsStep = DataVaildHelper.IsVaildStdStep(shopID, stepID);

                //仅对RTS对象的WIP做有效处理
                //RTS 대상 WIP만 유효 처리
                if (existRtsStep == false 
                    && DataVaildHelper.IsVaildStdStep(prevStepID) == false)
                {
                    continue;
                }
                
                newEntity.PROCESS_ID = entity.PROCESSFLOWNAME;
                newEntity.PRODUCT_ID = entity.PRODUCTSPECNAME;
                newEntity.PRODUCT_VERSION = entity.PRODUCTSPECVERSION;
                newEntity.PRODUCT_TYPE = entity.PRODUCTIONTYPE;                
                
                newEntity.PRIORITY = entity.PRIORITY;
                newEntity.STEP_ID = stepID;
                newEntity.LOT_TYPE = entity.PRODUCTTYPE;

                string lotStatus = entity.LOTPROCESSSTATE;
                newEntity.LOT_STATUS = lotStatus;
                
                newEntity.GLASS_QTY = entity.PRODUCTQUANTITY;
                newEntity.PANEL_QTY = entity.PRODUCTQUANTITY;

                string isInport = "N";

                //仅对MandatoryStep的wait WIP进行处理
                //MandatoryStep에 위치한 대기(WAIT) 재공의 경우만 해당
                bool isMandatoryStep = IsMandatoryStep(shopID, stepID);
                if (isMandatoryStep && LcdHelper.Equals(lotStatus, "WAIT"))
                {
                    string onMachine = entity.ONMACHINE;
                    if (LcdHelper.Equals(onMachine, "Y"))
                        isInport = "Y";
                }

                newEntity.IS_INPORT = isInport;

                //RUN ?  LOT MACHINENAME : DURABLE MACHINENAME(2020.03.11 - by.liujian(유건))
                string eqpID = entity.MACHINENAME;
                if (LcdHelper.Equals(lotStatus, "RUN") == false
                    && LcdHelper.Equals(isInport, "Y") == false)
                {
                    eqpID = entity.MACHINENAME_DURABLE;
                }

                newEntity.EQP_ID = eqpID;
                                
                newEntity.SHOP_IN_TIME = LcdHelper.DbDateTime(entity.RELEASETIME);

                //newEntity.PREV_STEP_END_TIME = LcdHelper.DbDateTime(entity.LASTLOGGEDOUTTIME);

                newEntity.TKIN_TIME = LcdHelper.DbDateTime(entity.LASTLOGGEDINTIME);
                newEntity.TKOUT_TIME = LcdHelper.DbDateTime(entity.LASTLOGGEDOUTTIME);

                newEntity.STATE_TIME = LcdHelper.DbDateTime(entity.LASTEVENTTIME);

                newEntity.OWNER_TYPE = entity.OWNER_TYPE;
                newEntity.OWNER_ID = entity.OWNER_ID;


                if (stepID != prevStepID)
                {                    
                    newEntity.MAIN_STEP_ID = entity.PRE_STEP_ID;
                }
                
                newEntity.HOLD_TIME = LcdHelper.DbDateTime(entity.HOLD_TIME);

                newEntity.SHEET_ID = entity.SHEETNAME;                
                newEntity.SHEET_ID_TIME = LcdHelper.DbDateTime(entity.SHEETNAME_TIME);

                newEntity.INSP_SHEET_ID = entity.INSP_SHEET_ID;
                newEntity.INSP_SHEET_TIME = LcdHelper.DbDateTime(entity.INSP_SHEET_TIME);

                string holdCode = null;

                //1.OnHold < 2.AbnSheet < 3.InspSheet < 4.PRIORITY_4
                //1.OnHold
                if (LcdHelper.Equals(entity.LOTHOLDSTATE, "OnHold"))
                {
                    newEntity.LOT_STATUS = "HOLD";
                    holdCode = "OnHold";
                }

                //2.AbnSheet
                if (string.IsNullOrEmpty(newEntity.SHEET_ID) == false)
                {
                    newEntity.LOT_STATUS = "HOLD";
                    holdCode = "AbnSheet";
                }

                //3.InspSheet
                if (string.IsNullOrEmpty(newEntity.INSP_SHEET_ID) == false)
                {
                    newEntity.LOT_STATUS = "HOLD";

                    if (existRtsStep == false)
                        holdCode = "InspBank";                        
                    else
                        holdCode = "InspSheet";
                }

                //4.SepBank (HoldStep)
                string findHoldCode;
                if(ConfigHelper.CheckHoldStep(shopID, stepID, out findHoldCode))
                {
                    newEntity.LOT_STATUS = "HOLD";
                    holdCode = findHoldCode;
                }

                //5.PRIORITY_4 (PRIORITY = 4(WAIT)时，做HOLD(2019.09.21 By 严嘉明)) 
                          //5.PRIORITY_4 (PRIORITY = 4(WAIT)인 경우 HOLD 처리(2019.09.21 옌쟈밍))
                if (newEntity.PRIORITY == 4 && LcdHelper.Equals(lotStatus, "WAIT"))
                {
                    newEntity.LOT_STATUS = "HOLD";
                    holdCode = "PRIORITY_4";
                }

                //Apply HoldCode Map
                if(holdCode != null)
                    newEntity.HOLD_CODE = ConfigHelper.GetCodeMap_HoldCode(holdCode);
                                
                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_WIP.Add(newEntity);
            }
        }

        //private IF_NODE FindPreMainNode(string shopID, string stepID, string nodeID)
        //{
        //    if (string.IsNullOrEmpty(nodeID))
        //        return null;

        //    //check main
        //    if (IsMainStep(shopID, stepID))
        //        return null;

        //    var table = InputMart.Instance.IF_NODE_DICT;

        //    HashSet<string> temps = new HashSet<string>();

        //    IF_NODE find = null;
        //    string currNodeID = nodeID;

        //    do
        //    {                
        //        temps.Add(currNodeID);
                
        //        find = table.FindRows(currNodeID).FirstOrDefault();

        //        if (find != null)
        //        {
        //            string toShopID = find.FACTORYNAME;
        //            string toStepID = find.STEP_ID;

        //            if (IsMainStep(toShopID, toStepID))
        //                return find;
        //        }

        //        string toNodeID = find == null ? null : find.TONODEID;

        //        //防止无限loop
        //        //무한 loop 방지
        //        if (temps.Contains(toNodeID))
        //            break;
                
        //        currNodeID = toNodeID;         
        //    }
        //    while (find != null);

        //    //check main
        //    if (find != null && IsMainStep(find.FACTORYNAME, find.STEP_ID))
        //        return find;

        //    return null;
        //}

        private bool IsMandatoryStep(string shopID, string stepID)
        {
            if (string.IsNullOrEmpty(shopID) || string.IsNullOrEmpty(stepID))
                return false;

            var table = InputMart.Instance.RTS_STD_STEP_DICT;
            var find = table.FindRows(shopID, stepID).FirstOrDefault();

            if (find != null && LcdHelper.Equals(find.IS_MANDATORY, "Y"))
                return true;
            
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_LOT_BANK(IPersistContext context)
        {
            var table = InputMart.Instance.IF_LOT_BANK;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string identityNull = LcdHelper.IdentityNull();

            foreach (var entity in table.DefaultView)
            {
                var newEntity = new ENG_BANK_WIP();

                newEntity.VERSION_DATE = LcdHelper.GetVersionDate();
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.ORIENTEDSITE;

                string shopID = entity.FACTORYNAME;               
                string stepID = entity.PROCESSOPERATIONNAME;
                string productID = entity.PRODUCTSPECNAME;

                //按ProductID基准变更ShopID
                //ProductID 기준의 ShopID로 변경 처리
                if (LcdHelper.IsSTB1(stepID) && LcdHelper.IsCellShop(shopID))
                    shopID = LcdHelper.GetShopIDByProductID(productID, shopID);

                newEntity.SHOP_ID = shopID;
                newEntity.LOT_ID = entity.LOTNAME;

                //TODO : BATCH_ID (CST_ID)
                newEntity.BATCH_ID = LcdHelper.ToSafeString(entity.CARRIERNAME, identityNull);

                newEntity.PROCESS_ID = entity.PROCESSFLOWNAME;
                newEntity.PRODUCT_ID = productID;
                newEntity.PRODUCT_VERSION = entity.PRODUCTSPECVERSION;
                newEntity.PRODUCT_TYPE = entity.PRODUCTIONTYPE;

                newEntity.PRIORITY = entity.PRIORITY;
                newEntity.STEP_ID = stepID;
                newEntity.LOT_TYPE = entity.PRODUCTTYPE;

                string lotStatus = entity.LOTSTATE; //entity.LOTPROCESSSTATE;
                newEntity.LOT_STATUS = lotStatus;

                newEntity.GLASS_QTY = entity.PRODUCTQUANTITY;
                newEntity.PANEL_QTY = entity.PRODUCTQUANTITY;
                newEntity.EQP_ID = entity.MACHINENAME;

                newEntity.SHOP_IN_TIME = LcdHelper.DbDateTime(entity.RELEASETIME);

                //newEntity.PREV_STEP_END_TIME = LcdHelper.DbDateTime(entity.LASTLOGGEDOUTTIME);

                newEntity.TKIN_TIME = LcdHelper.DbDateTime(entity.LASTLOGGEDINTIME);
                newEntity.TKOUT_TIME = LcdHelper.DbDateTime(entity.LASTLOGGEDOUTTIME);

                newEntity.STATE_TIME = LcdHelper.DbDateTime(entity.LASTEVENTTIME);

                newEntity.OWNER_TYPE = entity.OWNER_TYPE;
                newEntity.OWNER_ID = entity.OWNER_ID;                              

                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_BANK_WIP.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_INNER_BOM(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_INNER_BOM;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_INNER_BOM();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.FROM_PROCESS_ID = entity.FROM_PROCESS_ID;
                newEntity.FROM_PRODUCT_ID = entity.FROM_PRODUCT_ID;
                newEntity.TO_PROCESS_ID = entity.TO_PROCESS_ID;
                newEntity.TO_PRODUCT_ID = entity.TO_PRODUCT_ID;
                newEntity.COMP_QTY = entity.COMP_QTY;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_INNER_BOM.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_INTER_SHOP_BOM(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_INTER_SHOP_BOM;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_INTER_SHOP_BOM();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.FROM_SHOP_ID = entity.FROM_SHOP_ID;
                newEntity.FROM_STEP_ID = entity.FROM_STEP_ID;
                newEntity.FROM_PROCESS_ID = entity.FROM_PROCESS_ID;
                newEntity.FROM_PRODUCT_ID = entity.FROM_PRODUCT_ID;
                newEntity.TO_SHOP_ID = entity.TO_SHOP_ID;
                newEntity.TO_STEP_ID = entity.TO_STEP_ID;
                newEntity.TO_PROCESS_ID = entity.TO_PROCESS_ID;
                newEntity.TO_PRODUCT_ID = entity.TO_PRODUCT_ID;
                newEntity.TRANSFER_TIME = entity.TRANSFER_TIME;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_INTER_SHOP_BOM.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_PRESET_INFO(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_PRESET_INFO;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_PRESET_INFO();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.PRESET_ID = entity.PRESET_ID;
                newEntity.PRESET_DESC = entity.PRESET_DESC;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_PRESET_INFO.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_WEIGHT_FACTOR(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_WEIGHT_FACTORS;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_WEIGHT_FACTORS();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTOR_ID = entity.FACTOR_ID;
                newEntity.FACTOR_DESC = entity.FACTOR_DESC;
                newEntity.FACTOR_KIND = entity.FACTOR_KIND;
                newEntity.IS_ACTIVE = entity.IS_ACTIVE.ToString();

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_WEIGHT_FACTORS.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_WEIGHT_PRESETS(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_WEIGHT_PRESETS;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_WEIGHT_PRESETS();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.PRESET_ID = entity.PRESET_ID;
                newEntity.FACTOR_ID = entity.FACTOR_ID;
                newEntity.FACTOR_TYPE = entity.FACTOR_TYPE;
                newEntity.FACTOR_WEIGHT = entity.FACTOR_WEIGHT;
                newEntity.FACTOR_NAME = entity.FACTOR_NAME;
                newEntity.SEQUENCE = entity.SEQUENCE;
                newEntity.ORDER_TYPE = entity.ORDER_TYPE;
                newEntity.CRITERIA = entity.CRITERIA;
                newEntity.ALLOW_FILTER = entity.ALLOW_FILTER;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_WEIGHT_PRESETS.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_SETUP_TIMES(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_SETUP_TIMES;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                //CHANGE_TYPE is NOT NULL
                if (string.IsNullOrEmpty(entity.CHANGE_TYPE))
                    continue;

                var newEntity = new ENG_SETUP_TIMES();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.EQP_GROUP = entity.EQP_GROUP;
                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.CHANGE_TYPE = entity.CHANGE_TYPE;
                newEntity.SETUP_TIME = entity.SETUP_TIME;
                
                newEntity.FROM_STEP_ID = entity.FROM_STEP_ID;
                newEntity.FROM_PRODUCT_ID = entity.FROM_PRODUCT_ID;
                newEntity.FROM_PRODUCT_VERSION = entity.FROM_PRODUCT_VERSION;
                newEntity.FROM_ETC = entity.FROM_ETC;

                newEntity.TO_STEP_ID = entity.TO_STEP_ID;
                newEntity.TO_PRODUCT_ID = entity.TO_PRODUCT_ID;
                newEntity.TO_PRODUCT_VERSION = entity.TO_PRODUCT_VERSION;
                newEntity.TO_ETC = entity.TO_ETC;

                newEntity.PRIORITY = entity.PRIORITY;
                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_SETUP_TIMES.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_TOOL(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_TOOL;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var maskStates = InputMart.Instance.IF_DURABLEbyMaskID;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                string maskID = entity.TOOL_ID;

                //maskID is NOT NULL
                if (string.IsNullOrEmpty(maskID))
                    continue;

                var newEntity = new ENG_TOOL();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.TOOL_ID = maskID;
                newEntity.TOOL_TYPE = entity.TOOL_TYPE;
                newEntity.QTY = entity.QTY;
                newEntity.DESCRIPTION_ID = entity.DESCRIPTION_ID;
                
                var find = maskStates.FindRows(maskID).FirstOrDefault();
                if(find != null)
                {
                    newEntity.LOCATION = find.LOCATION;
                    newEntity.EQP_ID = find.MACHINENAME;
                    newEntity.STATE_CODE = find.MASKSTATE;
                    newEntity.STATE_CHANGE_TIME = find.LASTEVENTTIME;
                }

                newEntity.UPDATE_TIME = entity.UPDATE_TIME;

                OutputMart.Instance.ENG_TOOL.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_EQP_MOVE_TIME(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_EQP_MOVE_TIME;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                //FROM_EQP, TO_EQP is NOT NULL
                if (string.IsNullOrEmpty(entity.FROM_EQP) || string.IsNullOrEmpty(entity.TO_EQP))
                    continue;

                if (entity.TRANSFERTIME <= 0)
                    continue;
                                
                var newEntity = new ENG_EQP_MOVE_TIME();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.FROM_EQP_ID = entity.FROM_EQP;
                newEntity.TO_EQP_ID = entity.TO_EQP;

                string timeUnit = entity.TIME_UNIT;
                var t = LcdHelper.ParseTimeSpan(Convert.ToSingle(entity.TRANSFERTIME), timeUnit);

                newEntity.TRANSFER_TIME = (float)Math.Round(t.TotalMinutes, 3);
                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_EQP_MOVE_TIME.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_EQP_INLINE_MAP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_EQP_INLINE_MAP;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                //FROM_EQP, TO_EQP is NOT NULL
                if (string.IsNullOrEmpty(entity.FROM_EQP_ID) || string.IsNullOrEmpty(entity.TO_EQP_ID))
                    continue;
                                
                var newEntity = new ENG_EQP_INLINE_MAP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.MAP_TYPE = entity.MAP_TYPE;
                newEntity.FROM_STEP = entity.FROM_STEP;
                newEntity.FROM_EQP_ID = entity.FROM_EQP_ID;
                newEntity.TO_STEP = entity.TO_STEP;
                newEntity.TO_EQP_ID = entity.TO_EQP_ID;
                newEntity.BASE_POINT = entity.BASE_POINT;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_EQP_INLINE_MAP.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_PROCESSLIMITBYLINE(IPersistContext context)
        {
            var table = InputMart.Instance.IF_PROCESSLIMITBYLINE;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;

            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {                
                string shopID = entity.FACTORYNAME;
                string eqpID = ConfigHelper.GetCodeMap_ChamberID(entity.LINENAME);

                string limitType = entity.PROCESSLIMITTYPE;
                string stepID = entity.PROCESSOPERATIONNAME;
                string activateType = entity.PROCESSENABLE;

                //TODO : [bong]임시처리(기준정립 후 수정필요)(2019.09.26 - by.liujian(유건))
                if (LcdHelper.Equals(entity.DEPARTMENTID, "MFG"))
                {
                    //DEPARTMENTID = MFG, PROCESSLIMITTYPE = LO, PROCESSENABLE = M
                    if (LcdHelper.Equals(limitType, "LO") && LcdHelper.Equals(activateType, "M"))
                        continue;
                }

                bool isMType = LcdHelper.Equals(limitType, "M");
                                
                //EQPID, STEPID为必须(PROCESSLIMITTYPE=M做例外处理，ALL可以）
                //EQPID, STEPID는 필수(PROCESSLIMITTYPE=M은 예외, ALL 가능)
                if (isMType == false)
                {
                    if (DataVaildHelper.IsVaildEqp(eqpID) == false)
                        continue;
                        
                    if (DataVaildHelper.IsVaildStdStep(shopID, stepID) == false)
                        continue;
                }

                string productID = entity.PRODUCTSPECNAME;

                //TODO : CELL PI/ODF时，以Array(T)基准进行处理（后续进一步讨论后，再进行变更） 
                //TODO : CELL PI/ODF 경우는 Array(T) 기준으로 정보 사용(추후 협의에 따라 변경 필요)   
                //CELL ProductID以 F开头的信息除外
                //CELL ProductID 중 F로 시작되는 정보 제외                                
                if (DataVaildHelper.ExcludeData(shopID, productID))
                    continue;

                //转换为CellCode
                //CellCode로 변환
                productID = DataVaildHelper.ToVaildProductID(shopID, productID);
                
                var newEntity = new ENG_EQP_ARRANGE();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;
                newEntity.SHOP_ID = shopID;
                newEntity.LIMIT_TYPE = limitType;
                newEntity.EQP_ID = eqpID;
                newEntity.PRODUCT_ID = productID;
                newEntity.PRODUCT_VERSION = entity.PRODUCTSPECVERSION;
                newEntity.STEP_ID = stepID;
                newEntity.MASK_ID = entity.MASKNAME;
                newEntity.ACTIVATE_TYPE = activateType;
                newEntity.LIMIT_QTY = entity.PROCESSLIMITQTY;
                newEntity.ACTUAL_QTY = entity.ACTUALQTY;
                newEntity.DAILY_MODE = entity.DAILYMODE;

                DateTime dueDate = LcdHelper.DbToDateTime(entity.DUEDATE);
                newEntity.DUE_DATE = LcdHelper.DbDateTime(dueDate);
                                
                newEntity.UPDATE_TIME = entity.CREATETIME;

                newEntity.DEPARTMENT_ID = entity.DEPARTMENTID;

                OutputMart.Instance.ENG_EQP_ARRANGE.Add(newEntity);
            }
        }

        /// <summary>
        /// RTS_EQP_INOUT_ACT_IF --> ENG_EQP_IN_OUT_ACT (IS_FIXED = 'Y')
        /// 在还未结束(IS_FIXED = 'Y')的时间段的实时Actual统计使用LOTHISTORY(MES)
        /// 마감(IS_FIXED = 'Y')되지 않은 시간대의 실시간 실적 집계는 LOTHISTORY(MES)를 사용.
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_EQP_INOUT_ACT_IF(IPersistContext context)
        {
            //记录FixedDate（用在LOTHISTORY loading时）
            //FixedDate 기록 (LOTHISTORY loading시 필요)
            CheckActFixRptDate();

            var table = InputMart.Instance.RTS_EQP_IN_OUT_ACT_IF;
            if (table == null || table.Rows.Count == 0)
                return;

            //以ENG_EQP_IN_OUT_ACT基准Check 
            //ENG_EQP_IN_OUT_ACT 기준으로 체크
            var result = OutputMart.Instance.ENG_EQP_IN_OUT_ACT;            
            if (MainHelper.CheckActiveAction(result.Table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
          
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.Rows)
            {
                int inQty = entity.IN_QTY;
                int outQty = entity.OUT_QTY;

                if (inQty <= 0 && outQty <= 0)
                    continue;

                //仅IS_FIXED = 'Y'有效。为N的RPT_DATE使用LotHistory(MES)의的实时数据
                //IS_FIXED = 'Y'만 유효, N인 RPT_DATE는 LotHistory(MES)의 실시간 DATA를 사용.
                if (LcdHelper.Equals(entity.IS_FIXED, "Y") == false)
                    continue;

                string factoryID = entity.FACTORY_ID;
                string shopID = entity.SHOP_ID;
                string stepID = entity.STEP_ID;

                DateTime rptDate = entity.RPT_DATE;
                string eqpID = entity.EQP_ID;
                string processID = entity.PROC_ID;
                string productID = entity.PROD_ID;
                string productVersion = entity.PROD_VER;

                string ownerID = entity.OWNER_ID;
                string ownerType = entity.OWNER_TYPE;

                var find = result.Find(versionDate,
                                       versionNo,
                                       factoryID,
                                       shopID,
                                       rptDate,
                                       eqpID,
                                       stepID,
                                       processID,
                                       productID,
                                       productVersion,
                                       ownerType,
                                       ownerID);

                if (find == null)
                {
                    find = new ENG_EQP_IN_OUT_ACT();

                    find.VERSION_DATE = versionDate;
                    find.VERSION_NO = versionNo;

                    find.FACTORY_ID = factoryID;
                    find.SHOP_ID = shopID;
                    find.RPT_DATE = rptDate;
                    find.EQP_ID = eqpID;
                    find.STEP_ID = stepID;
                    find.PROCESS_ID = processID;
                    find.PRODUCT_ID = productID;
                    find.PRODUCT_VERSION = productVersion;
                    find.OWNER_TYPE = ownerType;
                    find.OWNER_ID = ownerID;
                    find.IN_QTY = 0;
                    find.OUT_QTY = 0;

                    result.Add(find);
                }

                if (inQty > 0)
                    find.IN_QTY += inQty;

                if (outQty > 0)
                    find.OUT_QTY += outQty;

                find.UPDATE_TIME = entity.UPDATE_DTTM;
            }
        }

        private void CheckActFixRptDate()
        {
            var context = ModelContext.Current;

            //记录FixedDate (用于LOTHISTORY loading时)
            //FixedDate 기록 (LOTHISTORY loading시 필요)
            var dashboard = InputMart.Instance.Dashboard;
            dashboard.ActFixedDate = LcdHelper.GetActFixedDate_Default();
            
            var table = InputMart.Instance.RTS_EQP_IN_OUT_ACT_IF;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;
                       
            var list = table.Rows.ToList();
            list.Sort((x, y) => (x.RPT_DATE.CompareTo(y.RPT_DATE) * -1));

            var last = list.FirstOrDefault(t => LcdHelper.Equals(t.IS_FIXED, "Y"));
            if (last != null)
            {
                dashboard.ActFixedDate = last.RPT_DATE;

                //change output args : ARG_ACT_FIXED_DATE
                var args = context.QueryArgs;
                args["ARG_ACT_FIXED_DATE"] = LcdHelper.DbToString(dashboard.ActFixedDate);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_EQP_STATUS_IF(IPersistContext context)
        {
            //记录LastInterfaceTime (用于LOTHISTORY loading时)
            //LastInterfaceTime 기록 (LOTHISTORY loading시 필요)
            CheckLastInterfaceTime();

            var table = InputMart.Instance.RTS_EQP_STATUS_IF;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var result = OutputMart.Instance.RSL_RTS_EQP_STATUS_IF;
            if (MainHelper.CheckActiveAction(result.Table.TableName) == false)
                return;

            foreach (var entity in table.DefaultView)
            {
                string factoryID = entity.FACTORY_ID;
                string shopID = entity.SHOP_ID;
                string eqpID = entity.EQP_ID;

                if (DataVaildHelper.IsVaildEqp(eqpID) == false)
                    continue;

                var find = result.Find(factoryID, shopID, eqpID);
                if (find == null)
                {
                    find = new RSL_RTS_EQP_STATUS_IF();                    
                    find.FACTORY_ID = factoryID;
                    find.SHOP_ID = shopID;
                    find.EQP_ID = eqpID;

                    result.Add(find);
                }

                find.STATUS = entity.STATUS;
                find.REASON_CODE = entity.REASON_CODE;
                find.EVENT_START_TIME = LcdHelper.DbDateTime(entity.EVENT_START_TIME);
                find.EVENT_END_TIME = LcdHelper.DbDateTime(entity.EVENT_END_TIME);
                find.VALID_CHAMBER = entity.VALID_CHAMBER;
                find.LAST_SHOP_ID = entity.LAST_SHOP_ID;
                find.LAST_STEP_ID = entity.LAST_STEP_ID;
                find.LAST_PRODUCT_ID = entity.LAST_PRODUCT_ID;
                find.LAST_PRODUCT_VERSION = entity.LAST_PRODUCT_VERSION;
                find.LAST_OWNER_TYPE = entity.LAST_OWNER_TYPE;
                find.LAST_TRACK_IN_TIME = LcdHelper.DbDateTime(entity.LAST_TRACK_IN_TIME);
                find.LAST_CONTINUOUS_QTY = entity.LAST_CONTINUOUS_QTY;
                find.LAST_ACID_DENSITY = entity.LAST_ACID_DENSITY;

                find.UPDATE_DTTM = entity.UPDATE_DTTM;
            }
        }

        private void CheckLastInterfaceTime()
        {

            //记录LastInterfaceTime (用于LOTHISTORY loading时)
            //LastInterfaceTime 기록 (LOTHISTORY loading시 필요)          
            var table = InputMart.Instance.RTS_EQP_STATUS_IF;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;
            
            var last = table.Rows.FirstOrDefault();
            if (last != null)
            {
                var dashboard = InputMart.Instance.Dashboard;
                dashboard.LastInterfaceTime = last.IF_TIME;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_CT_PRODUCTQUEUETIME(IPersistContext context)
        {
            var table = InputMart.Instance.IF_CT_PRODUCTQUEUETIME;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;
                        
            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                string shopID = entity.FACTORYNAME;
                string stepID = entity.PROCESSOPERATIONNAME;

                if (DataVaildHelper.IsVaildStdStep(shopID, stepID) == false)
                    continue;

                var newEntity = new ENG_WIP_STAY_HOURS();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;
                newEntity.LOT_ID = entity.LOTNAME;

                newEntity.SHOP_ID = shopID;
                newEntity.STEP_ID = stepID;

                string productID = entity.PRODUCTSPECNAME;


                //WIP不变
                //WIP은 변환 하지 않으므로 (변환 X)
                ////转换为CellCode
                ////CellCode로 변환
                //productID = DataVaildHelper.ToVaildProductID(shopID, productID);

                newEntity.PRODUCT_ID = productID;
                newEntity.PROCESS_ID = entity.PROCESSFLOWNAME;

                newEntity.TO_SHOP_ID = entity.TOFACTORYNAME;
                newEntity.TO_STEP_ID = entity.TOPROCESSOPERATIONNAME;
                newEntity.TO_PROCESS_ID = entity.TOPROCESSFLOWNAME;

                newEntity.MAX_Q_TIME = entity.MAXQUEUETIME;
                newEntity.MIN_HOLD_TIME = entity.MINQUEUETIME;
                newEntity.FROM_STEP_OUT_TIME = entity.FROMTRACKOUTTIME;
                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_WIP_STAY_HOURS.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_POSQUEUETIME(IPersistContext context)
        {
            var table = InputMart.Instance.IF_POSQUEUETIME;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                string shopID = entity.FACTORYNAME;
                string stepID = entity.PROCESSOPERATIONNAME;

                if (DataVaildHelper.IsVaildStdStep(shopID, stepID) == false)
                    continue;

                string toShopID = entity.TOFACTORYNAME;
                string toStepID = entity.TOPROCESSOPERATIONNAME;
                                
                if (DataVaildHelper.IsVaildStdStep(toShopID, toStepID) == false)
                    continue;

                var newEntity = new ENG_STAY_HOURS();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;

                newEntity.SHOP_ID = shopID;
                newEntity.STEP_ID = stepID;

                string productID = entity.PRODUCTSPECNAME;

                //转换为CellCode
                //CellCode로 변환
                productID = DataVaildHelper.ToVaildProductID(shopID, productID);

                newEntity.PRODUCT_ID = productID;
                newEntity.PROCESS_ID = entity.PROCESSFLOWNAME;

                newEntity.TO_SHOP_ID = entity.TOFACTORYNAME;
                newEntity.TO_STEP_ID = entity.TOPROCESSOPERATIONNAME;
                newEntity.TO_PROCESS_ID = entity.TOPROCESSFLOWNAME;

                newEntity.MAX_Q_TIME = entity.MAXQUEUETIME;
                newEntity.MIN_HOLD_TIME = entity.MINQUEUETIME;

                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_STAY_HOURS.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_UNPREDICT_WIP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_UNPREDICT_WIP;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_UNPREDICT_WIP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.CATEGORY = entity.CATEGORY;
                newEntity.PATTERN = entity.PATTERN;
                newEntity.DESCRIPTION = entity.DESCRIPTION;
                

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_UNPREDICT_WIP.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_MTPROJECT(IPersistContext context)
        {
            var table = InputMart.Instance.IF_MTPROJECT;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            
            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;
            DateTime now = modelContext.StartTime;

            int fixDays = 2;
            DateTime fixStartTime = now;
            DateTime fixEndTime = now.AddDays(fixDays);

            string pmType = PMType.Full.ToString();

            var fixedPmDelayTime = ConfigHelper.GetDefaultFixedPmDelayTime(); //1hours

            foreach (var entity in table.DefaultView)
            {
                var newEntity = new ENG_PM_SCHEDULE();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;

                string eqpID = entity.EQID;

                if (DataVaildHelper.IsVaildEqp(eqpID) == false)
                    continue;

                var duration = TimeSpan.FromHours(entity.PMHOUR);
                if (duration <= TimeSpan.Zero)
                    continue;

                string shopID = null;
                var eqp = DataVaildHelper.GetEqp(eqpID);
                if (eqp != null)
                {
                    shopID = eqp.SHOP_ID;
                }
                else
                {
                    var dcnEqp = DataVaildHelper.GetDcnEqp(eqpID);
                    if (dcnEqp != null)
                        shopID = dcnEqp.SHOP_ID;
                }

                if (string.IsNullOrEmpty(shopID))
                    continue;

                newEntity.SHOP_ID = shopID;
                newEntity.EQP_ID = eqpID;
                newEntity.PM_TYPE = pmType;

                DateTime startTime = entity.PMSTARTTIME;
                var delayTime = TimeSpan.FromDays(entity.DELAYDAYS);

                DateTime expectStartTime = entity.EXPECTSTARTTIME;
                //FIXED_PM : EXPECTSTARTTIME小于2天时，进行fixedPmDelayTime(1hours)内的PM（2019.12.23 - by.刘健）
                //FIXED_PM : EXPECTSTARTTIME이 2일 이내인 경우 fixedPmDelayTime(1hours) 이내 PM 진행 (2019.12.23 - by.liujian(유건))

                bool isFixedPM = expectStartTime >= fixStartTime && expectStartTime < fixEndTime;
                if (isFixedPM)
                {
                    startTime = expectStartTime;
                    delayTime = fixedPmDelayTime;
                }

                newEntity.START_TIME = startTime;
                newEntity.DURATION = (float)Math.Round(duration.TotalMinutes, 3);

                newEntity.ALLOW_AHEAD_TIME = (float)Math.Round(delayTime.TotalMinutes, 3);
                newEntity.ALLOW_DELAY_TIME = (float)Math.Round(delayTime.TotalMinutes, 3);

                //down chamber count
                newEntity.UNIT_QTY = eqp == null ? 0 : eqp.CHAMBER_COUNT; 

                newEntity.UPDATE_TIME = entity.UPDATETIME;

                OutputMart.Instance.ENG_PM_SCHEDULE.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_TPFOMPOLICY(IPersistContext context)
        {
             var table = InputMart.Instance.IF_TPFOMPOLICY;
            
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            string toolType = ToolType.MASK.ToString();

            foreach (var entity in table.DefaultView)
            {
                string shopID = entity.FACTORYNAME;

                //2019.08.09 CF ToolArrange除外（用RTS_TOOL_ARRANGE代替，MES不做管理 ）--刘健
                //2019.08.09 CF ToolArrange 제외(RTS_TOOL_ARRANGE로 대체, MES 관리 X) - liuJian
                if (LcdHelper.IsCfShop(shopID))
                    continue;

                string toolID = entity.MASKNAME;
                if (DataVaildHelper.IsVaildTool(toolID) == false)
                    continue;

                string eqpID = entity.MACHINENAME;
                if (DataVaildHelper.IsVaildEqp(eqpID) == false)
                    continue;

                string stepID = entity.PROCESSOPERATIONNAME;                
                string productID = entity.PRODUCTSPECNAME;
                string productVersion = entity.PRODUCTSPECVERSION;

                if (LcdHelper.IsNullOrEmpty_AnyOne(stepID, productID, productVersion))
                    continue;

                if (DataVaildHelper.IsVaildStdStep(stepID) == false)
                    continue;

                if (DataVaildHelper.IsVaildProductID(productID) == false)
                    continue;               
                               
                var newEntity = new ENG_TOOL_ARRANGE();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;
                newEntity.SHOP_ID = shopID;
                newEntity.TOOL_ID = entity.MASKNAME;
                newEntity.TOOL_TYPE = toolType;
                newEntity.EQP_ID = eqpID;
                newEntity.STEP_ID = stepID;
                newEntity.PRODUCT_ID = productID;
                newEntity.PRODUCT_VERSION = productVersion;
                newEntity.ACTIVATE_TYPE = entity.PROCESSENABLE;
                newEntity.PRIORITY = entity.PRIORITY;
                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_TOOL_ARRANGE.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_MACHINESPEC_CHAMBER(IPersistContext context)
        {
            var table = InputMart.Instance.IF_MACHINESPEC_CHAMBER;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                string chamberID = entity.MACHINENAME;
                if (string.IsNullOrEmpty(chamberID))
                    continue;

                string shopID = entity.FACTORYNAME;
                string eqpID = entity.SUPERMACHINENAME;

                if (DataVaildHelper.IsVaildEqp(eqpID) == false)
                    continue;

                string lineOperMode = entity.LINEOPERMODE;
                string stepID = ConfigHelper.GetCodeMap_LineOperMode(lineOperMode);

                var newEntity = new ENG_EQP_CHAMBER();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;
                newEntity.SHOP_ID = shopID;
                newEntity.EQP_ID = eqpID;
                newEntity.CHAMBER_ID = chamberID;
                newEntity.EQP_ID = eqpID;
                newEntity.RUN_MODE = lineOperMode;
                newEntity.ARRANGE_STEP = stepID;
                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_EQP_CHAMBER.Add(newEntity);
            }
        }

        /// <summary></summary>
        /// <param name="context" />
        public void OnAction_RTS_EQP_RENT_SCHEDULE(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_EQP_RENT_SCHEDULE;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_EQP_RENT_SCHEDULE();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.START_TIME = entity.START_TIME;
                newEntity.DURATION = entity.DURATION;
                newEntity.ALLOW_AHEAD_TIME = entity.ALLOW_AHEAD_TIME;
                newEntity.ALLOW_DELAY_TIME = entity.ALLOW_DELAY_TIME;
                newEntity.DESCRIPTION = entity.DESCRIPTION;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_EQP_RENT_SCHEDULE.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_CONFIG(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_CONFIG;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_CONFIG();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.CODE_GROUP = entity.CODE_GROUP;
                newEntity.CODE_NAME = entity.CODE_NAME;
                newEntity.CODE_VALUE = entity.CODE_VALUE;
                newEntity.DESCRIPTION = entity.DESCRIPTION;
                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_CONFIG.Add(newEntity);
            }
        }

        public void OnAction_RTS_HOLD_TIME(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_HOLD_TIME;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_HOLD_TIME();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.HOLD_CODE = entity.HOLD_CODE;
                newEntity.HOLD_TIME = entity.HOLD_TIME;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_HOLD_TIME.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_IN_OUT_PLAN(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_IN_OUT_PLAN;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                int qty = entity.PLAN_QTY;
                if (qty <= 0)
                    continue;

                var newEntity = new ENG_IN_OUT_PLAN();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.IN_OUT = entity.IN_OUT;
                newEntity.PRODUCT_ID = entity.PRODUCT_ID;
                newEntity.PRIORITY = entity.PRIORITY;
                newEntity.PLAN_QTY = qty;
                newEntity.PLAN_DATE = entity.PLAN_DATE;
                newEntity.LINE_TYPE = entity.LINE_TYPE;

                newEntity.UPDATE_TIME = entity.UPDATE_TIME;

                OutputMart.Instance.ENG_IN_OUT_PLAN.Add(newEntity);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_POSPAIRPRODUCT(IPersistContext context)
        {
            var table = InputMart.Instance.IF_POSPAIRPRODUCT;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var dashboard = InputMart.Instance.Dashboard;

            var dict = SetCodeMapSet(table);            

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;
            string factoryID = LcdHelper.GetTargetFactoryID();

            foreach (var entity in table.DefaultView)
            {
                var newEntity = new ENG_CELL_CODE_MAP();

                string actionType = LcdHelper.ToUpper(entity.ACTIONTYPE);
                var codeMapType = LcdHelper.ToEnum(actionType, CodeMapType.NONE);
                if (codeMapType == CodeMapType.NONE)
                    continue;

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;
                newEntity.FACTORY_ID = factoryID;

                newEntity.ACTIONTYPE = entity.ACTIONTYPE;

                newEntity.FROM_SHOP_ID = entity.FACTORYNAME;
                newEntity.FROM_PRODUCT_ID = entity.PRODUCTSPECNAME;
                newEntity.FROM_PRODUCT_VERSION = entity.PRODUCTSPECVERSION;
                                
                newEntity.TO_SHOP_ID = entity.DESTFACTORYNAME;
                newEntity.TO_PRODUCT_ID = entity.DESTPRODUCTSPECNAME;
                newEntity.TO_PRODUCT_VERSION = entity.DESTPRODUCTSPECVERSION;
                                
                newEntity.UPDATE_TIME = entity.LASTUPDATETIME;

                //if(newEntity.FROM_PRODUCT_ID == "TH645A1AB100")
                //    Console.WriteLine("B");

                newEntity.CELL_CODE = GetCellCodeMap(dict, newEntity);

                OutputMart.Instance.ENG_CELL_CODE_MAP.Add(newEntity);

                dashboard.AddCellCodeMaps(newEntity.FROM_PRODUCT_ID, newEntity.CELL_CODE);
            }
        }

        private Dictionary<CodeMapType, CodeMapSet> SetCodeMapSet(EntityTable<IF_POSPAIRPRODUCT> table)
        {
            var dict = InputMart.Instance.CodeMapSet;

            foreach (var entity in table.DefaultView)
            {                
                string actionType = LcdHelper.ToUpper(entity.ACTIONTYPE);
                var codeMapType = LcdHelper.ToEnum(actionType, CodeMapType.NONE);
                if (codeMapType == CodeMapType.NONE)
                    continue;

                CodeMapSet codeMapSet;
                if (dict.TryGetValue(codeMapType, out codeMapSet) == false)
                {
                    dict.Add(codeMapType, codeMapSet = new CodeMapSet());

                    codeMapSet.MapType = codeMapType;
                }

                var newEntity = new ENG_CELL_CODE_MAP();

                newEntity.ACTIONTYPE = entity.ACTIONTYPE;

                newEntity.FROM_SHOP_ID = entity.FACTORYNAME;
                newEntity.FROM_PRODUCT_ID = entity.PRODUCTSPECNAME;
                newEntity.FROM_PRODUCT_VERSION = entity.PRODUCTSPECVERSION;

                newEntity.TO_SHOP_ID = entity.DESTFACTORYNAME;
                newEntity.TO_PRODUCT_ID = entity.DESTPRODUCTSPECNAME;
                newEntity.TO_PRODUCT_VERSION = entity.DESTPRODUCTSPECVERSION;
                newEntity.UPDATE_TIME = entity.LASTUPDATETIME;

                string key = CodeMapSetFunc.CreateKey(newEntity.FROM_SHOP_ID, newEntity.FROM_PRODUCT_ID, newEntity.FROM_PRODUCT_VERSION);

                List<ENG_CELL_CODE_MAP> list;
                if (codeMapSet.Infos.TryGetValue(key, out list) == false)
                    codeMapSet.Infos.Add(key, list = new List<ENG_CELL_CODE_MAP>());

                list.Add(newEntity);
            }

            return dict;
        }

        private string GetCellCodeMap(Dictionary<CodeMapType, CodeMapSet> dict, ENG_CELL_CODE_MAP entity)
        {
            string actionType = LcdHelper.ToUpper(entity.ACTIONTYPE);
            var codeMapType = LcdHelper.ToEnum(actionType, CodeMapType.NONE);

            if (codeMapType <= CodeMapType.ASSY)
                return GetCellCodeMap_Assy(dict, entity);
            else
                return GetCellCodeMap_StbCutBank(entity);
        }

        private string GetCellCodeMap_Assy(Dictionary<CodeMapType, CodeMapSet> dict, ENG_CELL_CODE_MAP entity)
        {
            string actionType = LcdHelper.ToUpper(entity.ACTIONTYPE);
            var codeMapType = LcdHelper.ToEnum(actionType, CodeMapType.NONE);

            if (codeMapType == CodeMapType.ASSY)
                return entity.TO_PRODUCT_ID;

            if(dict == null)
                return null;

            var list = dict.Values.ToList();

            //CodeMapType排序（正序）
            //CodeMapType 순서 정렬(정순)
            list.Sort(CodeMapSetFunc.Compare);

            //CodeMapType排序后，依次转换Assy Map信息（Dest:找到Assy信息）
            //CodeMapType 순서 정렬 후 Assy Map 정보까지 순차 변환 적용(Dest:Assy 정보 찾기)
            ENG_CELL_CODE_MAP dest = null;

            var it = entity;
            foreach (var codeMapSet in list)
            {
                //Assy까지만 비교
                if (codeMapSet.MapType > CodeMapType.ASSY)
                    break;

                if (it == null)
                    break;

                var infos = codeMapSet.Infos;
                if (infos == null || infos.Count == 0)
                    continue;

                string key = CodeMapSetFunc.CreateKey(it.TO_SHOP_ID, it.TO_PRODUCT_ID, it.TO_PRODUCT_VERSION);

                List<ENG_CELL_CODE_MAP> finds;
                if(infos.TryGetValue(key, out finds) == false)
                {
                    key = CodeMapSetFunc.CreateKey(it.TO_SHOP_ID, it.TO_PRODUCT_ID, "ALL");
                    infos.TryGetValue(key, out finds);
                }

                ENG_CELL_CODE_MAP find = null;
                if (finds != null && finds.Count > 0)
                    find = finds.OrderByDescending(t => t.UPDATE_TIME).FirstOrDefault();

                if (codeMapSet.MapType == CodeMapType.ASSY)
                {
                    dest = find;
                    break;
                }

                if (find != null)
                    it = find;
            }

            if (dest != null)
                return dest.TO_PRODUCT_ID;

            return null;
        }

        private string GetCellCodeMap_StbCutBank(ENG_CELL_CODE_MAP entity)
        {
            string actionType = LcdHelper.ToUpper(entity.ACTIONTYPE);
            var codeMapType = LcdHelper.ToEnum(actionType, CodeMapType.NONE);
            if (codeMapType == CodeMapType.STBCUTBANK)
                return entity.FROM_PRODUCT_ID;

            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_IF_LOTHISTORY(IPersistContext context)
        {
            var table = InputMart.Instance.IF_LOTHISTORY;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            foreach (var entity in table.DefaultView)
            {
                var eventTime = entity.EVENTTIME;

                var newEntity = new RSL_RTS_LOTHISTORY_IF();

                newEntity.CURRENTSITE = entity.CURRENTSITE;
                newEntity.FACTORYNAME = entity.FACTORYNAME;
                newEntity.CURRENTFACTORYNAME = entity.CURRENTFACTORYNAME;
                newEntity.EVENTNAME = entity.EVENTNAME;
                newEntity.EVENTTIME = eventTime;
                newEntity.LOTNAME = entity.LOTNAME;
                newEntity.ORIGINALLOTNAME = entity.ORIGINALLOTNAME;
                newEntity.ROOTLOTNAME = entity.ROOTLOTNAME;
                newEntity.PARENTLOTNAME = entity.PARENTLOTNAME;
                newEntity.MACHINENAME = entity.MACHINENAME;
                newEntity.AREANAME = entity.AREANAME;
                newEntity.PRODUCTSPECNAME = entity.PRODUCTSPECNAME;
                newEntity.OLDPRODUCTSPECNAME = entity.OLDPRODUCTSPECNAME;
                newEntity.PRODUCTSPECVERSION = entity.PRODUCTSPECVERSION;
                newEntity.OLDPRODUCTSPECVERSION = entity.OLDPRODUCTSPECVERSION;
                newEntity.PROCESSFLOWNAME = entity.PROCESSFLOWNAME;
                newEntity.OLDPROCESSFLOWNAME = entity.OLDPROCESSFLOWNAME;
                newEntity.PROCESSFLOWVERSION = entity.PROCESSFLOWVERSION;
                newEntity.OLDPROCESSFLOWVERSION = entity.OLDPROCESSFLOWVERSION;
                newEntity.PROCESSOPERATIONNAME = entity.PROCESSOPERATIONNAME;
                newEntity.OLDPROCESSOPERATIONNAME = entity.OLDPROCESSOPERATIONNAME;
                newEntity.PROCESSOPERATIONVERSION = entity.PROCESSOPERATIONVERSION;
                newEntity.OLDPROCESSOPERATIONVERSION = entity.OLDPROCESSOPERATIONVERSION;
                newEntity.PROCESSGROUPNAME = entity.PROCESSGROUPNAME;
                newEntity.CARRIERNAME = entity.CARRIERNAME;
                newEntity.PRODUCTTYPE = entity.PRODUCTTYPE;
                newEntity.PRODUCTQUANTITY = entity.PRODUCTQUANTITY;
                newEntity.OLDPRODUCTQUANTITY = entity.OLDPRODUCTQUANTITY;
                newEntity.SUBPRODUCTQUANTITY = entity.SUBPRODUCTQUANTITY;
                newEntity.OLDSUBPRODUCTQUANTITY = entity.OLDSUBPRODUCTQUANTITY;
                newEntity.PRODUCTIONTYPE = entity.PRODUCTIONTYPE;
                newEntity.LOTGRADE = entity.LOTGRADE;
                newEntity.DUEDATE = entity.DUEDATE;
                newEntity.PRIORITY = entity.PRIORITY;
                newEntity.LASTLOGGEDINTIME = entity.LASTLOGGEDINTIME;
                newEntity.LASTLOGGEDOUTTIME = entity.LASTLOGGEDOUTTIME;
                newEntity.LOTSTATE = entity.LOTSTATE;
                newEntity.LOTHOLDSTATE = entity.LOTHOLDSTATE;
                newEntity.REASONCODE = entity.REASONCODE;
                newEntity.OWNERID = entity.OWNERID;
                newEntity.SHEETNAME = entity.SHEETNAME;
                newEntity.QTIMEOVERFLAG = entity.QTIMEOVERFLAG;
                newEntity.NODESTACK = entity.NODESTACK;
                newEntity.REWORKSTATE = entity.REWORKSTATE;
                newEntity.REWORKCOUNT = entity.REWORKCOUNT;
                newEntity.REWORKNODEID = entity.REWORKNODEID;
                newEntity.PORTNAME = entity.PORTNAME;
                newEntity.SHIPTIMEKEY = entity.SHIPTIMEKEY;
                newEntity.SHIPTYPE = entity.SHIPTYPE;
                newEntity.PPID = entity.PPID;
                newEntity.SYSTEMTIME = entity.SYSTEMTIME;

                OutputMart.Instance.RSL_RTS_LOTHISTORY_IF.Add(newEntity);

                CheckLastLotEventTime(eventTime);
            }
        }

        private void CheckLastLotEventTime(DateTime eventTime)
        {            
            var dashboard = InputMart.Instance.Dashboard;
            dashboard.LastLotEventTime = LcdHelper.Max(eventTime, dashboard.LastLotEventTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_RTS_TOOL_ARRANGE(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_TOOL_ARRANGE;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                string shopID = entity.SHOP_ID;

                //2019.08.09 Array는 MES IF - liuJian
                if (LcdHelper.IsArrayShop(shopID))
                    continue;

                string toolID = entity.TOOL_ID;
                if (DataVaildHelper.IsVaildTool(toolID) == false)
                    continue;

                string stepID = entity.STEP_ID;
                if (DataVaildHelper.IsVaildStdStep(stepID) == false)
                    continue;

                var eqpList = DataVaildHelper.GetEqpListByStepID(shopID, stepID);
                if (eqpList == null && eqpList.Count == 0)
                    continue;

                string productID = entity.PRODUCT_ID;
                if (DataVaildHelper.IsVaildProductID(productID) == false)
                    continue;

                string productVersion = LcdHelper.GetDefaultProductVersion(shopID);

                foreach (var eqpID in eqpList)
                {
                    var newEntity = new ENG_TOOL_ARRANGE();

                    newEntity.VERSION_DATE = versionDate;
                    newEntity.VERSION_NO = versionNo;

                    newEntity.EQP_ID = eqpID;

                    newEntity.FACTORY_ID = entity.FACTORY_ID;
                    newEntity.SHOP_ID = shopID;
                    newEntity.TOOL_ID = toolID;
                    newEntity.TOOL_TYPE = entity.TOOL_TYPE;
                    newEntity.STEP_ID = stepID;

                    newEntity.PRODUCT_ID = productID;
                    newEntity.PRODUCT_VERSION = productVersion;

                    newEntity.ACTIVATE_TYPE = entity.ACTIVITY_TYPE;
                    newEntity.UPDATE_TIME = entity.UPDATE_TIME;

                    OutputMart.Instance.ENG_TOOL_ARRANGE.Add(newEntity);
                }
            }
        }

        public void OnAction_RTS_EQP_DCN(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_EQP_DCN;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_EQP_DCN();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.EQP_GROUP_ID = entity.EQP_GROUP_ID;
                newEntity.DAILY_CAPA = entity.DAILY_CAPA;
                newEntity.MAIN_RUN_SHOP = entity.MAIN_RUN_SHOP;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_EQP_DCN.Add(newEntity);
            }
        }

        public void OnAction_IF_CT_PHOTORECIPETIME(IPersistContext context)
        {
            var table = InputMart.Instance.IF_CT_PHOTORECIPETIME;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            string factoryID = LcdHelper.GetTargetFactoryID();

            foreach (var entity in table.DefaultView)
            {
                var newEntity = new ENG_EQP_RECIPE_TIME();

                string eqpID = entity.MACHINENAME;
                if (DataVaildHelper.IsVaildEqp(eqpID) == false)
                    continue;

                var finds = InputMart.Instance.RTS_EQPbyEqpID.FindRows(eqpID);
                if (finds == null)
                    continue;

                var eqp = finds.FirstOrDefault();
                if (eqp == null)
                    continue;

                string stepID = entity.PROCESSOPERATIONNAME;
                if (DataVaildHelper.IsVaildStdStep(stepID) == false)
                    continue;

                string productID = entity.PRODUCTSPECNAME;
                if (DataVaildHelper.IsVaildProductID(productID) == false)
                    continue;

                string shopID = entity.FACTORYNAME;

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = factoryID;
                newEntity.SHOP_ID = shopID; // eqp.SHOP_ID;
                newEntity.EQP_ID = eqpID;
                newEntity.PRODUCT_ID = productID;
                newEntity.PRODUCT_VERSION = entity.PRODUCTSPECVERSION;
                newEntity.STEP_ID = stepID;

                newEntity.DUE_DATE = entity.DUEDATETIME;

                newEntity.CHECK_FLAG = entity.CHECKFLAG;
                newEntity.MAX_COUNT = entity.MAXCOUNT;
                newEntity.TRACK_IN_COUNT = entity.TKINCOUNT;
                newEntity.RUN_MODE = entity.RUNMODE;
                newEntity.TOOL_ID = entity.MASKNAME;

                newEntity.UPDATE_TIME = entity.UPDATEDATETIME;

                OutputMart.Instance.ENG_EQP_RECIPE_TIME.Add(newEntity);
            }
        }

        public void OnAction_RTS_ACID_ALTER(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_ACID_ALTER;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_ACID_ALTER();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.EQP_GROUP = entity.EQP_GROUP;
                newEntity.PRODUCT_ID = entity.PRODUCT_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.DENSITY_ALTER = entity.DENSITY_ALTER;
                newEntity.ALTER_TYPE = entity.ALTER_TYPE;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_ACID_ALTER.Add(newEntity);
            }
        }

        public void OnAction_RTS_ACID_CHG(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_ACID_CHG;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_ACID_CHG();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.EQP_GROUP = entity.EQP_GROUP;
                newEntity.DENSITY = entity.DENSITY;
                newEntity.CHG_TIME = entity.CHG_TIME;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_ACID_CHG.Add(newEntity);
            }
        }

        public void OnAction_RTS_ACID_LIMIT(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_ACID_LIMIT;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_ACID_LIMIT();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.EQP_GROUP = entity.EQP_GROUP;
                newEntity.PRODUCT_ID = entity.PRODUCT_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.DENSITY_LIMIT = entity.DENSITY_LIMIT;
                newEntity.DENSITY_JC = entity.DENSITY_JC;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_ACID_LIMIT.Add(newEntity);
            }
        }

        public void OnAction_RTS_SETUP_TIMES_IDLE(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_SETUP_TIMES_IDLE;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_SETUP_TIMES_IDLE();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.EQP_GROUP = entity.EQP_GROUP;
                newEntity.IDLE_TIME = entity.IDLE_TIME;
                newEntity.SETUP_TIME = entity.SETUP_TIME;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.PRODUCT_ID = entity.PRODUCT_ID;

                OutputMart.Instance.ENG_SETUP_TIMES_IDLE.Add(newEntity);
            }
        }

        public void OnAction_RTS_PRODUCT_MAP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_PRODUCT_MAP;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_PRODUCT_MAP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;

                newEntity.PRODUCT_ID = entity.PROD_ID;
                newEntity.MAIN_PRODUCT_ID = entity.MAIN_PROD_ID;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_PRODUCT_MAP.Add(newEntity);
            }
        }

        public void OnAction_RTS_OWNER_LIMIT(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_OWNER_LIMIT;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var eqpList = LcdHelper.ToListString(entity.EQP_ID);
                if (eqpList == null)
                    continue;

                //eqp list --> eqpid rows
                foreach (var eqpID in eqpList)
                    Add_RTS_OWNER_LIMIT(entity, eqpID);
            }
        }

        private void Add_RTS_OWNER_LIMIT(RTS_OWNER_LIMIT entity, string eqpID)
        {
            if (entity == null)
                return;

            if (string.IsNullOrEmpty(eqpID))
                return;
                
            //var finds = InputMart.Instance.RTS_EQPbyEqpID.FindRows(eqpID);
            //if (finds == null)
            //    return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            var newEntity = new ENG_OWNER_LIMIT();

            newEntity.VERSION_DATE = versionDate;
            newEntity.VERSION_NO = versionNo;

            newEntity.FACTORY_ID = entity.FACTORY_ID;
            newEntity.SHOP_ID = entity.SHOP_ID;

            newEntity.OWNER_ID = entity.OWNER_ID;
            newEntity.EQP_ID = eqpID;
            newEntity.STEP_ID = entity.STEP_ID;
            newEntity.LIMIT_TYPE = entity.LIMIT_TYPE;

            newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

            OutputMart.Instance.ENG_OWNER_LIMIT.Add(newEntity);
        }

        public void OnAction_IF_CT_PCPLANDAY(IPersistContext context)
        {
            var table = InputMart.Instance.IF_CT_PCPLANDAY;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;
            string factoryID = LcdHelper.GetTargetFactoryID();

            DateTime now = modelContext.StartTime;
            DateTime baseDate = now.AddDays(-1).Date;

            foreach (var entity in table.DefaultView)
            {
                string month = entity.MONTH;
                string day = entity.DAY;

                if (string.IsNullOrEmpty(month) || string.IsNullOrEmpty(day))
                    continue;

                if (day.Length == 1)
                    day = "0" + day;

                //from : -1 days
                DateTime planDate = LcdHelper.StringToDate(month + day);
                if (planDate.Date < baseDate.Date)
                    continue;

                var newEntity = new ENG_FIX_PLAN_DCN();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;
                newEntity.FACTORY_ID = factoryID;

                newEntity.SHOP_ID = entity.FACTORYNAME;
                newEntity.EQP_ID = entity.RESERVEDMACHINENAME;                                

                newEntity.PLAN_DATE = planDate;

                newEntity.PRODUCT_ID = entity.PRODUCTSPECNAME;
                newEntity.PRODUCT_VERSION = entity.MASKVERSION;
                newEntity.BATCH_ID = entity.CARRIERNAME;
                newEntity.AREA_ID = entity.PRODUCTTYPE;
                newEntity.OWNER_ID = entity.OWNER;
                newEntity.PLAN_STATE = entity.PLANSTATE;
                newEntity.PLAN_QTY = entity.PLANQUANTITY;
                newEntity.RELEASED_QTY = entity.RELEASEDQUANTITY;
                newEntity.INPUT_QTY = entity.INPUTQUANTITY;
                newEntity.PLAN_SEQ = entity.RESERVEDORDER;

                newEntity.EVENTUSER = entity.EVENTUSER;
                newEntity.CREATIONTYPE = entity.CREATIONTYPE;

                newEntity.UPDATE_TIME = now;

                OutputMart.Instance.ENG_FIX_PLAN_DCN.Add(newEntity);
            }
        }

        public void OnAction_RTS_BRANCH_STEP(IPersistContext context)
        {
            var table = InputMart.Instance.RTS_BRANCH_STEP;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.DefaultView)
            {
                if (LcdHelper.Equals(entity.DATA_FLAG, "N"))
                    continue;

                var newEntity = new ENG_BRANCH_STEP();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.PRODUCT_ID = entity.PRODUCT_ID;
                newEntity.EQP_GROUP_ID = entity.EQP_GROUP_ID;
                newEntity.RUN_MODE = entity.RUN_MODE;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.NEXT_STEP_ID = entity.NEXT_STEP_ID;
                newEntity.PRIORITY = entity.PRIORITY;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_BRANCH_STEP.Add(newEntity);
            }

        }
    }
}
