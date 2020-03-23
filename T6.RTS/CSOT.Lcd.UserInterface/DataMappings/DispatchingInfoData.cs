using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class DispatchingInfoData
    {
        #region Input Table Name

        public const string DATA_TABLE_1 = "Eqp";

        public const string DATA_TABLE_2 = "EqpDispatchLog";

        public const string DATA_TABLE_3 = "PresetInfo";

        #endregion



        #region Input Data Transform


        public class Eqp
        {
            public string EqpID;
            public string StepGroup;

            public Eqp(DataRow row)
            {
                EqpID = string.Empty;
                StepGroup = string.Empty;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */

            public static string ColEqpID = "EQP_ID";
            public static string ColStepGroup = "STEP_GROUP";

            private void ParseRow(DataRow row)
            {
                // EqpID
                EqpID = row.GetString(Eqp.ColEqpID);

                // StepGroup
                StepGroup = row.GetString(Eqp.ColStepGroup);
            }
        }


        public class EqpDispatchLog
        {
            public string EqpID;
            public string DispatchingTime;

            public int InitCnt;
            public int FilteredCnt;
            public int SelectedCnt;

            public string SelectedWip;
            public string FilteredWipLog;
            public string DispatchWipLog;

            public string PresetID;

            public EqpDispatchLog(DataRow row)
            {
                EqpID = string.Empty;
                DispatchingTime = string.Empty;

                InitCnt = 0;
                FilteredCnt = 0;
                SelectedCnt = 0;

                SelectedWip = string.Empty;
                FilteredWipLog = string.Empty;
                DispatchWipLog = string.Empty;
                PresetID = string.Empty;
                
                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */

            public static string ColEqpID = "EQP_ID";
            public static string ColDispatchingTime = "DISPATCHING_TIME";
            public static string ColInitWipCnt = "INIT_WIP_CNT";
            public static string ColFilteredWipCnt = "FILTERED_WIP_CNT";
            public static string ColSelectedWipCnt = "SELECTED_WIP_CNT";
            public static string ColSelectedWip = "SELECTED_WIP";
            public static string ColFilteredWipLog = "FILTERED_WIP_LOG";
            public static string ColDispatchWipLog = "DISPATCH_WIP_LOG";
            public static string ColPresetID = "PRESET_ID";

            private void ParseRow(DataRow row)
            {
                // EqpID
                EqpID = row.GetString(ColEqpID);


                DispatchingTime = row.GetString(ColDispatchingTime);


                InitCnt = row.GetInt32(ColInitWipCnt);

                FilteredCnt = row.GetInt32(ColFilteredWipCnt);

                SelectedCnt = row.GetInt32(ColSelectedWipCnt);


                SelectedWip = row.GetString(ColSelectedWip);

                FilteredWipLog = row.GetString(ColFilteredWipLog);

                DispatchWipLog = row.GetString(ColDispatchWipLog);

                PresetID = row.GetString(ColPresetID);
            }
        }


        #endregion



        #region Output Caption

        // 메인 UI
        public const string TITLE = "Dispatching Info";
        public const string EQP_GRP = "EQP_GROUP";
        public const string EQP = "EQP_ID";
        public const string DATE = "조회기간";

        public const string EQP_ID = "EQP_ID";
        public const string DISPATCHING_TIME = "DISPATCHING_TIME";
        public const string INIT_CNT = "INIT_CNT";
        public const string FILTERED_CNT = "FILTERED_CNT";
        public const string DISPATCHING_CNT = "DISPATCHING_CNT";
        public const string SELECTED_CNT = "SELECTED_CNT";
        public const string INIT_WIP = "INIT_WIP";
        public const string DISPATCHING_WIP = "DISPATCHING_WIP";
        public const string FILTERED_WIP = "FILTERED_WIP";
        public const string SELECTED_WIP = "SELECTED_WIP";
        public const string FACTOR_LIST = "FACTOR_LIST";

        // 팝업 UI
        public const string EQP_ID_POPUP = "EQP_ID";
        public const string TOTAL_POPUP = "합계";

        #endregion
    }
}
