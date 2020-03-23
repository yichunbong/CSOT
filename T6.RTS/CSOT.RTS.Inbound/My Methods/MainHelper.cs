using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.RTS.Inbound.DataModel;
using CSOT.RTS.Inbound.Inputs;
using CSOT.RTS.Inbound.Outputs;
using CSOT.RTS.Inbound.Persists;
using Mozart.DataActions;
using Mozart.SeePlan;

namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class MainHelper
    {
        /// <summary>
        /// Input Table的数据DataAtion激活
        /// Input Table의 DataAtion 활성화
        /// 默认在所有Table有做Deactivation (item.ActiveAction = null)
        /// Default 로 모든 테이블이 Deactivation 되어 있음. (item.ActiveAction = null)
        /// 只有设置item.ActiveAction才能执行query
        /// item.ActiveAction을 설정해줘야 쿼리가 실행됨.
        /// </summary>
        public static void SetActiveAction(InboudRunType runType)
        {
            SetActiveAction(ModelContext.Current.Inputs, runType);
            SetActiveAction(ModelContext.Current.Outputs, runType);
        }

        private static void SetActiveAction(IDataLayer dataLayer, InboudRunType runType)
        {
            if (dataLayer == null)
                return;

            string[] names = dataLayer.GetItemList();
            if (names == null)
                return;

            foreach (var name in names)
            {
                var item = dataLayer.GetItem(name);
                if (item == null)
                    continue;

                //clear ActiveAction
                item.ActiveAction = null;

                if (IsNeedActive(name, runType))
                {
                    foreach (var dataAction in item.Actions)
                        item.SetActiveAction(dataAction.Name);
                }
                else
                {
                    item.ActiveAction = null;
                }
            }
        }

        private static bool IsNeedActive(string name, InboudRunType runType)
        {
            if (name == null)
                return false;

            //共同使用的List
            //공통 사용 List
            List<string> commonList = new List<string>(new string[] { "ShiftTime",
                                                                      "IF_LOTHISTORY",
                                                                      "IF_MACHINE",
                                                                      "IF_OWNER_MAP",
                                                                      "IF_CT_FACILITYRESULTHISTORY",
                                                                      "RTS_EQP",
                                                                      "RTS_EQP_IN_OUT_ACT_IF",
                                                                      "RSL_RTS_EQP_IN_OUT_ACT_IF",
                                                                      "RTS_EQP_LAST_IN_ACT_IF",
                                                                      "RSL_RTS_EQP_LAST_IN_ACT_IF",
                                                                      "RTS_EQP_STATUS_IF",
                                                                      "RSL_RTS_EQP_STATUS_IF",
                                                                      "RTS_LOTHISTORY_IF",
                                                                      "RSL_RTS_LOTHISTORY_IF"
                                                                    });

            if (commonList.Contains(name))
                return true;

            //INBOUND_IF 전용 List
            List<string> inboundOnlyList = new List<string>(new string[] { "" });
            if (inboundOnlyList.Contains(name))
            {
                return runType == InboudRunType.INBOUND_IF;
            }
            else
            {
                return runType != InboudRunType.INBOUND_IF;
            }
        }

        public static bool CheckActiveAction(string name)
        {            
            var item = ModelContext.Current.Inputs.GetItem(name);
            if (item == null)
                item = ModelContext.Current.Outputs.GetItem(name);

            if (item == null)
                return false;

            if (item.ActiveAction != null)
                return true;

            return false;
        }              

        public static DateTime GetRptDate_1Hour(DateTime t)
        {
            //DayStartTime为基准
            //DayStartTime 기준
            int baseMinute = ShopCalendar.StartTimeOfDayT(ModelContext.Current.StartTime).Minute;

            return LcdHelper.GetRptDate_1Hour(t, baseMinute);
        }

        public static string GetOwnerType(string ownerID)
        {
            if (string.IsNullOrEmpty(ownerID))
                return null;

            var table = InputMart.Instance.IF_OWNER_MAP;
            if (table == null || table.Rows.Count == 0)
                return null;

            var find = table.Rows.Find(ownerID);
            if (find != null)
                return find.OWNER_TYPE;

            return null;
        }
                
        public static bool IsChamberEqp(string factoryID, string shopID, string eqpID)
        {
            var chamberType = GetChamberType(factoryID, shopID, eqpID);
                        
            return LcdHelper.IsChamberType(chamberType);
        }

        public static ChamberType GetChamberType(string factoryID, string shopID, string eqpID)
        {
            if (string.IsNullOrEmpty(factoryID)
                || string.IsNullOrEmpty(shopID)
                || string.IsNullOrEmpty(eqpID))
            {
                return ChamberType.NONE;
            }

            var eqps = InputMart.Instance.RTS_EQP;
            if (eqps == null || eqps.Rows.Count == 0)
                return ChamberType.NONE;

            var find = eqps.Rows.Find(factoryID, shopID, eqpID);
            
            if (find != null)
            {
                return LcdHelper.GetChamberType(find.SIM_TYPE);
            }

            return ChamberType.NONE;
        }

        public static int GetChamberCount(string factoryID, string shopID, string eqpID)
        {
            int defaultCount = 1;

            if (string.IsNullOrEmpty(factoryID)
                || string.IsNullOrEmpty(shopID)
                || string.IsNullOrEmpty(eqpID))
            {
                return defaultCount;
            }

            var eqps = InputMart.Instance.RTS_EQP;
            if (eqps == null || eqps.Rows.Count == 0)
                return 1;

            var find = eqps.Rows.Find(factoryID, shopID, eqpID);
            if (find != null)
            {
                if (LcdHelper.IsChamberType(find.SIM_TYPE))
                    return (int)Math.Max(find.CHAMBER_COUNT, 1);
            }

            return defaultCount;
        }

        public static void RUN()
        {
            var dashboard = InputMart.Instance.Dashboard;

            Load_IF_MACHINE();
            
            Update_RSL_RTS_EQP_IN_OUT_ACT_IF();
                        
            if (dashboard.RunType == InboudRunType.ENG_RUN)
            {
                //用最新的RSL_RTS_EQP_STATUS_IF结果生成EQP_STATUS
                //최종 Update된 RSL_RTS_EQP_STATUS_IF 결과를 이용하여 EQP_STATUS 생성
                Add_ENG_EQP_STATUS();

                //用RSL_RTS_EQP_IN_OUT_ACT结果生成ENG_EQP_IN_OUT_ACT 
                //RSL_RTS_EQP_IN_OUT_ACT 결과를 이용하여 ENG_EQP_IN_OUT_ACT 생성
                Add_ENG_EQP_IN_OUT_ACT();

                //用ENG_EQP_IN_OUT_ACT结果生成ENG_FAB_IN_OUT_ACT
                //ENG_EQP_IN_OUT_ACT 결과를 이용하여 ENG_FAB_IN_OUT_ACT 생성
                Add_ENG_FAB_IN_OUT_ACT();                
            }
        }

        private static void Load_IF_MACHINE()
        {
            var table = InputMart.Instance.IF_MACHINE;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();
            //DateTime versionDate = LcdHelper.GetVersionDate();

            var result = OutputMart.Instance.RSL_RTS_EQP_STATUS_IF;

            foreach (var entity in table.DefaultView)
            {
                string shopID = entity.FACTORYNAME;
                string eqpID = entity.MACHINENAME;

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

                find.STATUS = entity.MACHINESTATENAME;
                find.REASON_CODE = entity.LASTEVENTNAME;

                DateTime eventStartTime = LcdHelper.DbToDateTime(LcdHelper.SafeSubstring(entity.LASTEVENTTIMEKEY, 0, 14));
                find.EVENT_START_TIME = LcdHelper.DbDateTime(eventStartTime);
                                
                DateTime dueDate = LcdHelper.DbToDateTime(LcdHelper.SafeSubstring(entity.DUEDATE, 0, 14));
                find.EVENT_END_TIME = LcdHelper.DbDateTime(dueDate);

                find.UPDATE_DTTM = now;
            }

            //更新最新的 BATCH信息(by LotHistory)
            //LAST진행 BATCH 정보 업데이트(by LotHistory)
            Update_RSL_RTS_EQP_STATUS_IF();

            //LAST ACID浓度信息
            //LAST ACID 농도 정보
            Load_IF_CT_FACILITYRESULTHISTORY();

            //更新CHAMBER EQP STATUS信息
            //CHAMBER EQP STATUS 정보 업데이트
            Load_IF_MACHINESPEC_CHAMBER();

            var dashboard = InputMart.Instance.Dashboard;

            //设定时间 (lastLotEventTime) 
            //확정된 시간 (lastLotEventTime) 설정
            DateTime lastLotEventTime = dashboard.LastLotEventTime;
            foreach (var entity in result.Table.Rows)
            {
                entity.IF_TIME = lastLotEventTime;
            }
        }

        private static void Update_RSL_RTS_EQP_STATUS_IF()
        {
            var table = InputMart.Instance.IF_LOTHISTORY;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            if (table == null || table.Rows.Count == 0)
                return;

            var dashboard = InputMart.Instance.Dashboard;
            DateTime lastInterfaceTime = dashboard.LastInterfaceTime;

            //仅统计“设定时间(lastInterfaceTime)” 后的信息
            //확정된 시간 (lastInterfaceTime) 이후의 정보만으로 정보 집계
            var list = table.Rows.ToList().FindAll(t => t.EVENTNAME == "TrackIn" && t.EVENTTIME > lastInterfaceTime);
            if (list == null || list.Count == 0)
                return;

            var result = OutputMart.Instance.RSL_RTS_EQP_STATUS_IF;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();

            var group = list.GroupBy(t => t.MACHINENAME);
            foreach (var it in group)
            {
                string eqpID = it.Key;
                var infos = it.OrderBy(t => t.EVENTTIME);
                if (infos == null)
                    continue;

                var finds = InputMart.Instance.RTS_EQPbyEqpID.FindRows(eqpID);
                if (finds == null)
                    continue;

                var eqp = finds.FirstOrDefault();
                if (eqp == null)
                    continue;

                string shopID = eqp.SHOP_ID;

                var find = result.Find(factoryID, shopID, eqpID);
                if (find == null)
                {
                    find = new RSL_RTS_EQP_STATUS_IF();
                    find.FACTORY_ID = factoryID;
                    find.SHOP_ID = shopID;
                    find.EQP_ID = eqpID;
                    find.LAST_CONTINUOUS_QTY = 0;

                    result.Add(find);
                }

                foreach (var entity in infos)
                {
                    DateTime evnetTime = entity.EVENTTIME;
                    int qty = entity.PRODUCTQUANTITY;

                    bool isLastPlan = IsLastPlan(find, entity);
                    if (isLastPlan)
                    {
                        find.LAST_CONTINUOUS_QTY += qty;
                    }
                    else
                    {
                        find.LAST_SHOP_ID = entity.FACTORYNAME;
                        find.LAST_STEP_ID = entity.PROCESSOPERATIONNAME;
                        find.LAST_PRODUCT_ID = entity.PRODUCTSPECNAME;
                        find.LAST_PRODUCT_VERSION = entity.PRODUCTSPECVERSION;
                        find.LAST_OWNER_TYPE = GetOwnerType(entity.OWNERID);

                        find.LAST_CONTINUOUS_QTY = qty;
                    }

                    find.UPDATE_DTTM = now;
                }
            }
        }

        private static void Load_IF_MACHINESPEC_CHAMBER()
        {
            var table = InputMart.Instance.IF_MACHINESPEC_CHAMBER;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;
            
            var result = OutputMart.Instance.RSL_RTS_EQP_STATUS_IF;

            string factoryID = LcdHelper.GetTargetFactoryID();
            DateTime versionDate = LcdHelper.GetVersionDate();
            
            var eqpGroup = table.Rows.GroupBy(t => new Tuple<string, string>(t.FACTORYNAME, t.SUPERMACHINENAME));
            foreach (var it in eqpGroup)
            {
                var key = it.Key;
                var chamberList = it.ToList();

                string shopID = key.Item1;
                string eqpID = key.Item2;                

                if (chamberList == null || chamberList.Count == 0)
                    continue;

                //parent EQP基准的EqpStatus最新信息
                //parent EQP 기준의 EqpStatus 최신 정보
                var parent = result.Find(factoryID, shopID, eqpID);
                if (parent == null)
                    continue;
                                
                foreach (var chamber in chamberList)
                {
                    string chamberID = chamber.MACHINENAME;
                    string lastStepID = ConfigHelper.GetCodeMap_LineOperMode(chamber.LINEOPERMODE);

                    var find = result.Find(factoryID, shopID, chamberID);
                    if (find == null)
                    {
                        find = new RSL_RTS_EQP_STATUS_IF();                        
                        find.FACTORY_ID = factoryID;
                        find.SHOP_ID = shopID;
                        find.EQP_ID = chamberID;

                        result.Add(find);
                    }

                    if (parent.LAST_STEP_ID == lastStepID)
                        Set_RSL_RTS_EQP_STATUS_IF(parent, find);
                    else
                        Set_RSL_RTS_EQP_STATUS_IF(eqpID, lastStepID, parent, find);
                }
            }
        }

        private static void Set_RSL_RTS_EQP_STATUS_IF(RSL_RTS_EQP_STATUS_IF parent, RSL_RTS_EQP_STATUS_IF child)
        {
            if (parent == null || child == null)
                return;

            child.STATUS = parent.STATUS;
            child.REASON_CODE = parent.REASON_CODE;
            child.EVENT_START_TIME = parent.EVENT_START_TIME;
            child.EVENT_END_TIME = parent.EVENT_END_TIME;
            //child.VALID_CHAMBER = parent.VALID_CHAMBER;

            child.LAST_SHOP_ID = parent.LAST_SHOP_ID;
            child.LAST_STEP_ID = parent.LAST_STEP_ID;            
            child.LAST_PRODUCT_ID = parent.LAST_PRODUCT_ID;
            child.LAST_PRODUCT_VERSION = parent.LAST_PRODUCT_VERSION;
            child.LAST_OWNER_TYPE = parent.LAST_OWNER_TYPE;            
            child.LAST_TRACK_IN_TIME = parent.LAST_TRACK_IN_TIME;
            child.LAST_CONTINUOUS_QTY = parent.LAST_CONTINUOUS_QTY;

            child.UPDATE_DTTM = parent.UPDATE_DTTM;            
        }

        private static void Set_RSL_RTS_EQP_STATUS_IF(string eqpID, string lastStepID, RSL_RTS_EQP_STATUS_IF parent, RSL_RTS_EQP_STATUS_IF child)
        {
            var table = InputMart.Instance.IF_LOTHISTORY;

            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            child.STATUS = parent.STATUS;
            child.REASON_CODE = parent.REASON_CODE;
            child.EVENT_START_TIME = parent.EVENT_START_TIME;
            child.EVENT_END_TIME = parent.EVENT_END_TIME;
            //child.VALID_CHAMBER = parent.VALID_CHAMBER;
            child.UPDATE_DTTM = parent.UPDATE_DTTM;

            //LOTHISTORY : 해당 Step를 진행한 마지막 정보 기록
            var list = table.Rows.ToList().FindAll(t => t.EVENTNAME == "TrackIn" && t.MACHINENAME == eqpID && t.PROCESSOPERATIONNAME == lastStepID);
            if (list == null || list.Count == 0)
                return;

            var last = list.OrderBy(t => t.EVENTTIME).LastOrDefault();
            if (last != null)
            {
                child.LAST_SHOP_ID = last.FACTORYNAME;
                child.LAST_STEP_ID = last.PROCESSOPERATIONNAME;
                child.LAST_PRODUCT_ID = last.PRODUCTSPECNAME;
                child.LAST_PRODUCT_VERSION = last.PRODUCTSPECVERSION;
                child.LAST_OWNER_TYPE = GetOwnerType(last.OWNERID);                
                child.LAST_TRACK_IN_TIME = LcdHelper.DbDateTime(last.EVENTTIME);

                //不可By Chamber统计(2019.09.21)
                //Chamber별 정보 집계 불가한 상태(2019.09.21)
                child.LAST_CONTINUOUS_QTY = 0;
            }
        }
        
        private static bool IsLastPlan(RSL_RTS_EQP_STATUS_IF last, IF_LOTHISTORY entity)
        {
            if (last == null || entity == null)
                return false;

            if (last.LAST_STEP_ID != entity.PROCESSOPERATIONNAME)
                return false;

            if (last.LAST_PRODUCT_ID != entity.PRODUCTSPECNAME)
                return false;

            if (last.LAST_PRODUCT_VERSION != entity.PRODUCTSPECVERSION)
                return false;

            if (last.LAST_OWNER_TYPE != GetOwnerType(entity.OWNERID))
                return false;

            return true;
        }

        private static void Load_IF_CT_FACILITYRESULTHISTORY()
        {
            var table = InputMart.Instance.IF_CT_FACILITYRESULTHISTORY;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            if (table == null || table.Rows.Count == 0)
                return;

            var result = OutputMart.Instance.RSL_RTS_EQP_STATUS_IF;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();

            var group = table.Rows.GroupBy(t => t.MACHINENAME);
            foreach (var it in group)
            {
                string eqpID = it.Key;

                //按时间最近进行排序
                //최근 순으로 정렬
                var infos = it.OrderByDescending(t => t.EVENTTIME);
                if (infos == null)
                    continue;

                var finds = InputMart.Instance.RTS_EQPbyEqpID.FindRows(eqpID);
                if (finds == null)
                    continue;

                var eqp = finds.FirstOrDefault();
                if (eqp == null)
                    continue;

                var entity = infos.FirstOrDefault();
                if (entity == null)
                    continue;

                string shopID = eqp.SHOP_ID;

                float density = LcdHelper.ToFloat(entity.PARAVALUE);
                if (density <= 0)
                    continue;

                var find = result.Find(factoryID, shopID, eqpID);
                if (find == null)
                {
                    find = new RSL_RTS_EQP_STATUS_IF();
                    find.FACTORY_ID = factoryID;
                    find.SHOP_ID = shopID;
                    find.EQP_ID = eqpID;
                    find.LAST_ACID_DENSITY = 0;

                    result.Add(find);
                }
                
                find.LAST_ACID_DENSITY = density;
                find.UPDATE_DTTM = now;
            }
        }

        private static void Add_ENG_EQP_STATUS()
        {
            var table = OutputMart.Instance.RSL_RTS_EQP_STATUS_IF.Table;
            if (table == null || table.Rows.Count == 0)
                return;

            if (MainHelper.CheckActiveAction(table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;

            DateTime versionDate = LcdHelper.GetVersionDate();

            foreach (var entity in table.Rows)
            {
                var newEntity = new ENG_EQP_STATUS();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = modelContext.VersionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.STATUS = entity.STATUS;
                newEntity.REASON_CODE = entity.REASON_CODE;
                newEntity.EVENT_START_TIME = entity.EVENT_START_TIME;
                newEntity.EVENT_END_TIME = entity.EVENT_END_TIME;
                newEntity.VALID_CHAMBER = entity.VALID_CHAMBER;
                newEntity.LAST_SHOP_ID = entity.LAST_SHOP_ID;
                newEntity.LAST_STEP_ID = entity.LAST_STEP_ID;
                newEntity.LAST_PRODUCT_ID = entity.LAST_PRODUCT_ID;
                newEntity.LAST_PRODUCT_VERSION = entity.LAST_PRODUCT_VERSION;
                newEntity.LAST_OWNER_TYPE = entity.LAST_OWNER_TYPE;
                newEntity.LAST_TRACK_IN_TIME = entity.LAST_TRACK_IN_TIME;
                newEntity.LAST_CONTINUOUS_QTY = entity.LAST_CONTINUOUS_QTY;
                newEntity.LAST_ACID_DENSITY = entity.LAST_ACID_DENSITY;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                OutputMart.Instance.ENG_EQP_STATUS.Add(newEntity);
            }
        }

        private static void Update_RSL_RTS_EQP_IN_OUT_ACT_IF()
        {
            var table = InputMart.Instance.IF_LOTHISTORY;
            
            if (table == null || table.Rows.Count == 0)
                return;

            var result = OutputMart.Instance.RSL_RTS_EQP_IN_OUT_ACT_IF;

            if (MainHelper.CheckActiveAction(result.Table.TableName) == false)
                return;

            var dashboard = InputMart.Instance.Dashboard;
            var runType = dashboard.RunType;

            var modelContext = ModelContext.Current;
            DateTime now = modelContext.StartTime;

            string factoryID = LcdHelper.GetTargetFactoryID();
            string isFixed = runType == InboudRunType.INBOUND_IF ? "Y" : "N";

            DateTime currRptDate = GetRptDate_1Hour(now);
            
            foreach (var entity in table.DefaultView)
            {
                int qty = entity.PRODUCTQUANTITY;
                if (qty <= 0)
                    continue;

                string eventName = entity.EVENTNAME;
                string shopID = entity.FACTORYNAME;                
                string eqpID = entity.MACHINENAME;

                //IN : TrackIn, OUT : TrackOut / Ship(9900, 9990)
                bool baseTrackIn = LcdHelper.Equals(eventName, "TrackIn");                

                DateTime rptDate = GetRptDate_1Hour(entity.EVENTTIME);

                //已保存到RTS_EQP_IN_OUT_ACT_IF table的时间排除在Update对象里 
                //이미 RTS_EQP_IN_OUT_ACT_IF table에 확정 저장된 시간은 Update 제외
                if (rptDate <= dashboard.ActFixedDate)
                    continue;

                //TrackOut의 경우 OLD값 기준으로 기록
                string stepID = baseTrackIn ? entity.PROCESSOPERATIONNAME : entity.OLDPROCESSOPERATIONNAME;
                string processID = baseTrackIn ? entity.PROCESSFLOWNAME : entity.OLDPROCESSFLOWNAME;
                string productID = baseTrackIn ? entity.PRODUCTSPECNAME : entity.OLDPRODUCTSPECNAME;
                string productVersion = baseTrackIn ? entity.PRODUCTSPECVERSION : entity.OLDPRODUCTSPECVERSION;
                                
                string ownerID = entity.OWNERID;
                string ownerType = GetOwnerType(ownerID);

                Add_RSL_RTS_EQP_IN_OUT_ACT_IF(result, factoryID, shopID, rptDate,
                                              eqpID, stepID, processID, productID, productVersion, 
                                              ownerType, ownerID, qty,
                                              isFixed, baseTrackIn, currRptDate, now);

                //新增ARRAY/CF 9900/9990 TrackIn Event (按9900/9990前面站点的TrackInOut为基准)
                //ARRAY/CF 9900/9990 TrackIn 이벤트 추가 생성 (9900/9990 이전 공정의 TrackInOut을 기준으로 생성)
                if (LcdHelper.Equals(eventName, "TrackInOut") == false && LcdHelper.IsCellShop(shopID) == false)
                {                    
                    string chkStepID = entity.PROCESSOPERATIONNAME;
                    var ioType = LcdHelper.GetInOutType(shopID, chkStepID);
                    if (ioType == InOutType.OUT)
                    {
                        string dummyEqpID = LcdHelper.IdentityNull();

                        //9900/9990 TrackIn
                        Add_RSL_RTS_EQP_IN_OUT_ACT_IF(result, factoryID, shopID, rptDate,
                                                      dummyEqpID, chkStepID,
                                                      entity.PROCESSFLOWNAME,
                                                      entity.PRODUCTSPECNAME,
                                                      entity.PRODUCTSPECVERSION,
                                                      ownerType, ownerID, qty,
                                                      isFixed, true, currRptDate, now);

                        //Ship(9900, 9990) Event记录，做例外处理 (2019.09.26 - by.刘健)
                        //Ship(9900, 9990) 이벤트에서 기록되어 제외 처리 (2019.09.26 - by.liujian(유건))
                        //以9900/9990 Track Out --> 9900/9990 Ship基准进行记录 
                        //9900/9990 Track Out --> 9900/9990 Ship 기준으로 기록
                        //Add_RSL_RTS_EQP_IN_OUT_ACT_IF(result, factoryID, shopID, rptDate, 
                        //                              dummyEqpID, chkStepID,
                        //                              entity.PROCESSFLOWNAME,
                        //                              entity.PRODUCTSPECNAME,
                        //                              entity.PRODUCTSPECVERSION,
                        //                              ownerType, ownerID, qty,
                        //                              isFixed, false, currRptDate, now);
                    }
                }
            }
        }

        private static void Add_RSL_RTS_EQP_IN_OUT_ACT_IF(IEntityWriter<RSL_RTS_EQP_IN_OUT_ACT_IF> result, 
            string factoryID, string shopID, DateTime rptDate, string eqpID, string stepID, string processID, 
            string productID, string productVersion, string ownerType, string ownerID, 
            int qty, string isFixed, bool baseTrackIn, DateTime currRptDate, DateTime now)
        {
            var find = result.Find(factoryID,
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
                find = new RSL_RTS_EQP_IN_OUT_ACT_IF();

                find.FACTORY_ID = factoryID;
                find.SHOP_ID = shopID;
                find.RPT_DATE = rptDate;
                find.EQP_ID = eqpID;
                find.STEP_ID = stepID;
                find.PROC_ID = processID;
                find.PROD_ID = productID;
                find.PROD_VER = productVersion;
                find.OWNER_TYPE = ownerType;
                find.OWNER_ID = ownerID;
                find.IN_QTY = 0;
                find.OUT_QTY = 0;
                find.IS_FIXED = isFixed;

                result.Add(find);
            }

            if (baseTrackIn)
                find.IN_QTY += qty;
            else
                find.OUT_QTY += qty;

            //当前进行的时间段时，做“未确定”处理
            //현재 진행중인 시간대인 경우 미확정 처리
            if (rptDate >= currRptDate)
                find.IS_FIXED = "N";

            find.UPDATE_DTTM = now;
        }       

        private static void Add_ENG_EQP_IN_OUT_ACT()
        {
            var table = OutputMart.Instance.RSL_RTS_EQP_IN_OUT_ACT_IF.Table;
            if (table == null || table.Rows.Count == 0)
                return;

            var result = OutputMart.Instance.ENG_EQP_IN_OUT_ACT;

            if (MainHelper.CheckActiveAction(result.Table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;

            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.Rows)
            {
                var newEntity = new ENG_EQP_IN_OUT_ACT();

                newEntity.VERSION_DATE = versionDate;
                newEntity.VERSION_NO = versionNo;

                newEntity.FACTORY_ID = entity.FACTORY_ID;
                newEntity.SHOP_ID = entity.SHOP_ID;
                newEntity.RPT_DATE = entity.RPT_DATE;
                newEntity.EQP_ID = entity.EQP_ID;
                newEntity.STEP_ID = entity.STEP_ID;
                newEntity.PROCESS_ID = entity.PROC_ID;
                newEntity.PRODUCT_ID = entity.PROD_ID;
                newEntity.PRODUCT_VERSION = entity.PROD_VER;
                newEntity.OWNER_TYPE = entity.OWNER_TYPE;
                newEntity.OWNER_ID = entity.OWNER_ID;
                newEntity.IN_QTY = entity.IN_QTY;
                newEntity.OUT_QTY = entity.OUT_QTY;

                newEntity.UPDATE_TIME = entity.UPDATE_DTTM;

                result.Add(newEntity);
            }
        }

        private static void Add_ENG_FAB_IN_OUT_ACT()
        {
            var table = OutputMart.Instance.ENG_EQP_IN_OUT_ACT.Table;
            if (table == null || table.Rows.Count == 0)
                return;

            var result = OutputMart.Instance.ENG_FAB_IN_OUT_ACT;
            if (MainHelper.CheckActiveAction(result.Table.TableName) == false)
                return;

            var modelContext = ModelContext.Current;            

            DateTime versionDate = LcdHelper.GetVersionDate();
            string versionNo = modelContext.VersionNo;

            foreach (var entity in table.Rows)
            {
                //Shop In数量 = ARRAY(1100)/CF(0100)的Out数量 (2019.09.26 - by.刘健)
                //Shop In 수량 = ARRAY(1100)/CF(0100)의 Out 수량 (2019.09.26 - by.liujian(유건))
                //int inQty = entity.IN_QTY;
                int inQty = entity.OUT_QTY;
                int outQty = entity.OUT_QTY;

                if (inQty <= 0 && outQty <= 0)
                    continue;
                               
                string factoryID = entity.FACTORY_ID;
                string shopID = entity.SHOP_ID;

                string stepID = entity.STEP_ID;

                var ioType = LcdHelper.GetInOutType(shopID, stepID);
                if (ioType == InOutType.NONE)
                    continue;

                if (ioType == InOutType.IN && inQty <= 0)
                    continue;

                if (ioType == InOutType.OUT && outQty <= 0)
                    continue;
                                                
                DateTime rptDate = entity.RPT_DATE;

                string processID = entity.PROCESS_ID;
                string productID = entity.PRODUCT_ID;
                string productVersion = entity.PRODUCT_VERSION;

                string ownerID = entity.OWNER_ID;
                string ownerType = entity.OWNER_TYPE;

                var find = result.Find(versionDate,
                                       versionNo,
                                       factoryID,
                                       shopID,
                                       rptDate,
                                       processID,
                                       productID,
                                       productVersion,
                                       ownerType,
                                       ownerID);

                if (find == null)
                {
                    find = new ENG_FAB_IN_OUT_ACT();

                    find.VERSION_DATE = versionDate;
                    find.VERSION_NO = versionNo;

                    find.FACTORY_ID = factoryID;
                    find.SHOP_ID = shopID;
                    find.RPT_DATE = rptDate;
                    find.PROCESS_ID = processID;
                    find.PRODUCT_ID = productID;
                    find.PRODUCT_VERSION = productVersion;
                    find.OWNER_TYPE = ownerType;
                    find.OWNER_ID = ownerID;
                    find.IN_QTY = 0;
                    find.OUT_QTY = 0;

                    result.Add(find);
                }

                if(ioType == InOutType.IN)
                    find.IN_QTY += entity.IN_QTY;

                if (ioType == InOutType.OUT)
                    find.OUT_QTY += entity.OUT_QTY;

                find.UPDATE_TIME = entity.UPDATE_TIME;
            }
        }        
    }
}
