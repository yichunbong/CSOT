using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class EqpGanttChartData
    {        
        #region Input Table Name

        public const string PRODUCT_TABLE_NAME = "Product";                
        public const string EQP_TABLE_NAME = "Eqp";
        public const string LOAD_HIST_TABLE_NAME = "LoadHistory";
        public const string LOAD_STAT_TABLE_NAME = "LoadStat";
        public const string EQP_DISPATCH_LOG_TABLE_NAME = "EqpDispatchLog";

        public const string WEIGHT_PRESET_LOG_TABLE_NAME = "WeightPresetLog";
        //public const string PRESET_INFO_TABLE_NAME = "WeightPresets";

        public const string STD_STEP_TABLE_NAME = "StdStep";
        public const string WIP_TABLE_NAME = "Wip";
        public const string CONST_TABLE_NAME = "Const";

        #endregion

        #region Input Data Transform

        #region Const
        public class Const
        {
            public string Category;
            public string Code;
            public string Description;

            public Const(DataRow row)
            {
                Category = string.Empty;
                Code = string.Empty;
                Description = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                Category = row.GetString(Schema.CATEGORY);
                Code = row.GetString(Schema.CODE);
                Description = row.GetString(Schema.DESCRIPTION);
            }

            internal class Schema
            {
                public static string CATEGORY = "CATEGORY";
                public static string CODE = "CODE";
                public static string DESCRIPTION = "DESCRIPTION";
            }

        }
        #endregion

        #region Product
        public class Product
        {
            public string ShopID;
            public string ProductID;
            public string ProductType;
            public string ProductKind;

            public Product(DataRow row)
            {
                ShopID = string.Empty;
                ProductID = string.Empty;
                ProductType = string.Empty;
                ProductKind = string.Empty;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                ShopID = row.GetString(Schema.SHOP_ID);
                ProductID = row.GetString(Schema.PRODUCT_ID);
                ProductType = row.GetString(Schema.PRODUCT_TYPE);
                ProductKind = row.GetString(Schema.PRODUCT_KIND);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string PRODUCT_TYPE = "PRODUCT_TYPE";
                public static string PRODUCT_KIND = "PRODUCT_KIND";
            }

        }
        #endregion

        #region Eqp
        public class Eqp
        {
            public string ShopID;
            public string EqpID;
            public string EqpGroup;
            public string DspEqpGroup;
            public string SimType;
            public int MaxBatchSize;
            public int MinBatchSize;
            public int ViewSeq;
            
            public Eqp(DataRow row)
            {
                ShopID = string.Empty;
                EqpID = string.Empty;
                EqpGroup = string.Empty;
                DspEqpGroup = string.Empty;
                SimType = string.Empty;
                MaxBatchSize = 0;
                MinBatchSize = 0;
                ViewSeq = 999;
             
                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                ShopID = row.GetString(Schema.SHOP_ID);
                EqpID = row.GetString(Schema.EQP_ID);
                EqpGroup = row.GetString(Schema.EQP_GROUP_ID);
                DspEqpGroup = row.GetString(Schema.EQP_GROUP_ID);
                SimType = row.GetString(Schema.SIM_TYPE);
                MaxBatchSize = row.GetInt32(Schema.MAX_BATCH_SIZE);
                MinBatchSize = row.GetInt32(Schema.MIN_BATCH_SIZE);
                ViewSeq = row.GetInt32(Schema.VIEW_SEQ);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string EQP_ID = "EQP_ID";
                public static string EQP_GROUP_ID = "EQP_GROUP_ID";
                public static string DSP_EQP_GROUP_ID = "DSP_EQP_GROUP_ID";
                public static string SIM_TYPE = "SIM_TYPE";
                public static string MAX_BATCH_SIZE = "MAX_BATCH_SIZE";
                public static string MIN_BATCH_SIZE = "MIN_BATCH_SIZE";
                public static string VIEW_SEQ = "VIEW_SEQ";
            }
        }
        #endregion 

        #region LoadingHistory
        public class LoadingHistory
        {
            public string ShopID;
            public string EqpID;
            public DateTime TargetDate;
            public string InfoGzip;

            public LoadingHistory(DataRow row)
            {
                ShopID = string.Empty;
                EqpID = string.Empty;
                TargetDate = DateTime.MinValue;
                InfoGzip = "";
         
                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                // EqpID
                ShopID = row.GetString(Schema.SHOP_ID);

                EqpID = row.GetString(Schema.EQP_ID);

                TargetDate = row.GetString(Schema.TARGET_DATE).DbToDateTime();

                InfoGzip = row.GetString(Schema.INFO_GZIP);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string EQP_ID = "EQP_ID";
                public static string EQP_GROUP_ID = "EQP_GROUP_ID";
                public static string TARGET_DATE = "TARGET_DATE";
                public static string INFO_GZIP = "INFO_GZIP";
            }
        }
        #endregion

        #region LoadStat
        public class LoadStat
        {
            public string EqpID;
            public DateTime TargetDate;
            public float Setup;
            public float Busy;
            public float IdleRun;
            
            public LoadStat(DataRow row)
            {
                EqpID = string.Empty;
                TargetDate = DateTime.MinValue;
                Setup = 0.0f;
                Busy = 0.0f;
                IdleRun = 0.0f;
            
                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                // EqpID
                EqpID = row.GetString(Schema.EQP_ID);

                // TargetDate - Default :: yyyyMMdd 형태의 string 
                TargetDate = row.GetString(Schema.TARGET_DATE).DbToDateTime();

                // Setup
                Setup = row.GetFloat(Schema.SETUP);

                // Busy
                Busy = row.GetFloat(Schema.BUSY);

                // IdleRun
                IdleRun = row.GetFloat(Schema.IDLERUN);
            }

            internal class Schema
            {
                public static string EQP_ID = "EQP_ID";
                public static string TARGET_DATE = "TARGET_DATE";
                public static string SETUP = "SETUP";
                public static string BUSY = "BUSY";
                public static string IDLERUN = "IDLERUN";
            }
        }
        #endregion

        #region EqpDispatchLog
        public class EqpDispatchLog
        {
            public string EqpID;
            public string SubEqpID;
            public DateTime DispatchingTime;
            public int InitWipCount;
            public int FilteredWipCount;
            public int SelectedWipCount;
            public string LastWip;
            public string SelectedWip;
            public string FilteredWipLog;
            public string DispatchWipLog;
            public string PresetID;

            public EqpDispatchLog(DataRow dispatchingInfo)
            {
                EqpID = string.Empty;
                SubEqpID = string.Empty;
                DispatchingTime = DateTime.MinValue;
                InitWipCount = 0;
                FilteredWipCount = 0;
                SelectedWipCount = 0;
                LastWip = string.Empty;
                SelectedWip = string.Empty;
                FilteredWipLog = string.Empty;
                DispatchWipLog = string.Empty;
                PresetID = string.Empty;

                ParseRow(dispatchingInfo);
            }

            private void ParseRow(DataRow row)
            {
                this.EqpID = row.GetString(Schema.EQP_ID);
                this.SubEqpID = row.GetString(Schema.SUB_EQP_ID);
                this.DispatchingTime = row.GetString(Schema.DISPATCH_TIME).DbToDateTime();
                this.InitWipCount = row.GetInt32(Schema.INIT_WIP_CNT);
                this.FilteredWipCount = row.GetInt32(Schema.FILTERED_WIP_CNT);
                this.SelectedWipCount = row.GetInt32(Schema.SELECTED_WIP_CNT);
                this.LastWip = row.GetString(Schema.LAST_WIP);
                this.SelectedWip = row.GetString(Schema.SELECTED_WIP);
                this.FilteredWipLog = row.GetString(Schema.FILTERED_WIP_LOG);
                this.DispatchWipLog = row.GetString(Schema.DISPATCH_WIP_LOG);
                this.PresetID = row.GetString(Schema.PRESET_ID);
            }

            //private void ParseRow(CSOT.Lcd.Scheduling.Outputs.EqpDispatchLog info)
            //{
            //    this.EqpID = info.EQP_ID;
            //    this.SubEqpID = info.SUB_EQP_ID;
            //    this.DispatchingTime = info.DISPATCHING_TIME.DbToDateTime();
            //    this.InitWipCount = info.INIT_WIP_CNT;
            //    this.FilteredWipCount = info.FILTERED_WIP_CNT;
            //    this.SelectedWipCount = info.SELECTED_WIP_CNT;
            //    this.LastWip = info.LAST_WIP;
            //    this.SelectedWip = info.SELECTED_WIP;
            //    this.FilteredWipLog = info.FILTERED_WIP_LOG;
            //    this.DispatchWipLog = info.DISPATCH_WIP_LOG;
            //    this.PresetID = info.PRESET_ID;
            //}

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string EQP_ID = "EQP_ID";
                public static string SUB_EQP_ID = "SUB_EQP_ID";
                public static string DISPATCH_TIME = "DISPATCHING_TIME";
                public static string INIT_WIP_CNT = "INIT_WIP_CNT";
                public static string FILTERED_WIP_CNT = "FILTERED_WIP_CNT";
                public static string SELECTED_WIP_CNT = "SELECTED_WIP_CNT";
                public static string LAST_WIP = "LAST_WIP";
                public static string SELECTED_WIP = "SELECTED_WIP";
                public static string FILTERED_WIP_LOG = "FILTERED_WIP_LOG";
                public static string DISPATCH_WIP_LOG = "DISPATCH_WIP_LOG";
                public static string PRESET_ID = "PRESET_ID";
            }
        }
        #endregion

        #region PresetInfo

        public class PresetInfo
        {
            public string PresetID;
            public string FactorID;
            public string FactorType;
            public string OrderType;
            public float FactorWeight;
            public float Sequence;
            public string FactorName;
            public string Criteria;

            public PresetInfo(DataRow row)
            {
                PresetID = string.Empty;
                FactorID = string.Empty;
                FactorType = string.Empty;
                OrderType = string.Empty;
                FactorWeight = 0.0f;
                Sequence = 0.0f;
                FactorName = string.Empty;
                Criteria = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // PresetID
                PresetID = row.GetString(Schema.PRESET_ID);

                // FactorID
                FactorID = row.GetString(Schema.FACTOR_ID);

                FactorType = row.GetString(Schema.FACTOR_TYPE);

                OrderType = row.GetString(Schema.ORDER_TYPE);

                FactorWeight = row.GetFloat(Schema.FACTOR_WEIGHT);

                Sequence = row.GetFloat(Schema.SEQUENCE);

                FactorName = row.GetString(Schema.FACTOR_NAME);

                Criteria = row.GetString(Schema.CRITERIA);
            }

            internal class Schema
            {
                public static string PRESET_ID = "PRESET_ID";
                public static string FACTOR_ID = "FACTOR_ID";
                public static string FACTOR_TYPE = "FACTOR_TYPE";
                public static string ORDER_TYPE = "ORDER_TYPE";
                public static string FACTOR_WEIGHT = "FACTOR_WEIGHT";
                public static string SEQUENCE = "SEQUENCE";
                public static string FACTOR_NAME = "FACTOR_NAME";
                public static string CRITERIA = "CRITERIA";
            }

            public class Comparer : IComparer<PresetInfo>
            {
                public int Compare(PresetInfo x, PresetInfo y)
                {
                    if (object.ReferenceEquals(x, y))
                        return 0;

                    int cmp = x.Sequence.CompareTo(y.Sequence);

                    if (cmp == 0)
                    {                        
                        cmp = string.Compare(x.FactorID, y.FactorID);
                    }

                    return cmp;
                }

                public static Comparer Default = new Comparer();
            }
        }
               
        #endregion

        #region StdStep

        public class StdStep
        {
            public string ShopID { get; set; }
            public string StdStepID { get; set; }
            public string Layer { get; set; }
            public int StepSeq { get; set; }

            public StdStep(string shopID, string stepID, string layer, int stepSeq)
            {
                this.ShopID = shopID;
                this.Layer = layer;
                this.StdStepID = stepID;
                this.StepSeq = stepSeq;
            } 

            public StdStep(DataRow row)
            {
                if (row == null)
                    return;

                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.Layer = row.GetString(Schema.LAYER_ID);
                this.StdStepID = row.GetString(Schema.STEP_ID);
                this.StepSeq = row.GetInt32(Schema.STEP_SEQ);
            }

            public string GetKey()
            {
                return StringHelper.ConcatKey(this.ShopID, this.StdStepID);
            }

            public static string CreateKey(string shopID, string stepID)
            {
                return StringHelper.ConcatKey(shopID, stepID);
            }

            internal class Schema
            {
                public static string AREA_ID = "AREA_ID";
                public static string SHOP_ID = "SHOP_ID";
                public static string LAYER_ID = "LAYER_ID";
                public static string STEP_ID = "STEP_ID";
                //public static string STEP_TYPE = "STEP_TYPE";
                public static string STEP_SEQ = "STEP_SEQ";
            }
        }

        #endregion


        #endregion

        #region UI 화면 Caption

        // 메인 UI

        public const string LINE_ID = "Shop ID";
        public const string PRODUCT_ID = "Product ID";
        public const string KOP = "KOP";
        public const string LOT_ID = "Lot ID";
        public const string QUERY_RANGE = "Date Range";

        // SUB UI - Data Grid
        public const string SUB_TITLE = "Process Lot List";
        public const string SUB_EQP_ID = "EQP";
        public const string SUB_STATUS = "STATUS";
        public const string SUB_LOT_ID = "LOT_ID";
        public const string SUB_STEP_ID = "STEP_ID";
        public const string SUB_PRODUCT_ID = "PRODUCT_ID";
        public const string SUB_START_TIME = "START_TIME";
        public const string SUB_END_TIME = "END_TIME";
        public const string SUB_PROCESS_TIME = "PROCESS_TIME";
        public const string SUB_TRACK_IN = "T/I";
        public const string SUB_TRACK_OUT = "T/O";


        // 팝업 UI
        public const string POPUP_TITLE = "Bar Information";
        
        #endregion
    }
}
