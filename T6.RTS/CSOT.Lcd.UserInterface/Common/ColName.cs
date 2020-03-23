using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSOT.Lcd.UserInterface.Common
{
    class ColName
    {
        #region BOP
        public const string LotID = "LOT_ID";
        public const string LineID = "LINE_ID";
        public const string ProductID = "PRODUCT_ID";
        public const string ProductVer = "PRODUCT_VERSION";
        public const string StepSeq = "STEP_SEQ";
        public const string StepID = "STEP_ID";
        public const string StepGroup = "STEP_GROUP";
        public const string StdStepID = "STD_STEP_ID";
        public const string StdStepSeq = "STD_STEP_SEQ";
        public const string ProcessID = "PROCESS_ID";
        public const string ProcessGrp = "PROCESS_GROUP";
        #endregion

        #region Eqpument
        public const string EqpID = "EQP_ID";
        public const string EqpGroup = "EQP_GROUP_ID";
        public const string Location = "LOCATION";
        public const string IsActive = "IS_ACTIVE";
        public const string SimType = "SIM_TYPE";
        public const string Preset_ID = "PRESET_ID";
        public const string DispatcherType = "DISPATCHER_TYPE";
        public const string OperRateRatio = "OPERRATING_RATIO";
        public const string BatchSize = "BATCH_SIZE";
        public const string ChamberCnt = "CHAMBER_COUNT";
        public const string ChamberID = "CHAMBER_ID";
        public const string State = "STATE";
        public const string MTTR = "MTTR_MIN";
        public const string StateTime = "STATUS_CHANGE_TIME";
        public const string Status = "STATUS";
        #endregion

        #region Date
        public const string TargetDate = "TARGET_DATE";
        public const string TargetDay = "TARGET_DAY";
        public const string StartTime = "START_TIME";
        public const string EndTime = "END_TIME";
        #endregion;

        #region Dispatching
        public const string DisptchingTime = "DISPATCHING_TIME";
        public const string SelectInfo = "SELECT_INFO";
        public const string FilterInfo = "FILTER_INFO";
        public const string DispatchInfo = "DISPATCH_INFO";
        public const string Factor_List = "FACTOR_LIST";
        #endregion

        #region Utilization
        public const string Setup = "SETUP";
        public const string Busy = "BUSY";
        public const string IdleRun = "IDLERUN";
        public const string Idle = "IDLE";
        public const string Pm = "PM";
        public const string Down = "DOWN";
        public const string Run = "RUN";
        #endregion

        public const string SLOPE = "SLOPE";
        public const string WEEK1 = "1_WEEK";
        public const string WEEK2 = "2_WEEK";
        public const string WEEK3 = "3_WEEK";
        public const string WEEK4 = "4_WEEK";


        #region Qty 수량
        public const string InUnitQty = "IN_UNIT_QTY";
        public const string InLotQty = "IN_LOT_QTY";
        public const string OutUnitQty = "OUT_UNIT_QTY";
        public const string OutLotQty = "OUT_LOT_QTY";

        public const string RunUnitQty = "RUN_UNIT_QTY";
        public const string WaitUnitQty = "WAIT_UNIT_QTY";
        public const string RunLotQty = "RUN_LOT_QTY";
        public const string WaitLotQty = "WAIT_LOT_QTY";

        public const string MainUnitQty = "MAIN_UNIT_QTY";
        public const string MainLotQty = "MAIN_LOT_QTY";

        public const string MinUnitQty = "MIN_UNIT_QTY";
        public const string MaxUnitQty = "MAX_UNIT_QTY";
        public const string AvgUnitQty = "AVG_UNIT_QTY";

        public const string FilterQty = "FILTER_QTY";
        public const string DispatchQty = "DISPATH_QTY";

        #endregion

        public const string OperName = "OperationName";

        public const string TIQty = "T/I";
        public const string TOQty = "T/O";

        public const string TYPE = "TYPE";
        public const string TOTAL = "TOTAL";
        public const string QTY_TYPE = "QTY_TYPE";
        public const string DELAY = "DELAY";
        public const string ACHIEVE = "ACHIEVE";
        public const string QTY = "QTY";
    }
}
