using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Mozart.Studio.TaskModel.UserLibrary;

using CSOT.Lcd.UserInterface.Common;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class SimResultData
    {
        public class OutputName
        {
            public const string EqpPlan = "EqpPlan";
            public const string StepMove = "StepMove";
            public const string StepWip = "StepWip";
            public const string EqpDispatchLog = "EqpDispatchLog";
            public const string LoadHistory = "LoadHistory";
            public const string LoadStat = "LoadStat";
            public const string MaskHistory = "MaskHistory";
            public const string SetupHistory = "SetupHistory";
            public const string WeightPresetLog = "WeightPresetLog";
        }

        public class EqpPlanSchema
        {
            public const string VERSION_NO = "VERSION_NO";
            public const string FACTORY_ID = "FACTORY_ID";
            public const string SHOP_ID = "SHOP_ID";
            public const string BATCH_ID = "BATCH_ID";
            public const string EQP_ID = "EQP_ID";
            public const string EQP_GROUP_ID = "EQP_GROUP_ID";
            public const string LOT_ID = "LOT_ID";
            public const string STEP_ID = "STEP_ID";
            public const string STEP_DESC = "STEP_GROUP";
            public const string LAYER_ID = "LAYER_ID";
            public const string START_TIME = "START_TIME";
            public const string END_TIME = "END_TIME";
            public const string TOOL_ID = "TOOL_ID";
            public const string PROCESS_ID = "PROCESS_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
        }

        public class StepMoveSchema
        {
            public const string VERSION_NO = "VERSION_NO";
            public const string FACTORY_ID = "FACTORY_ID";
            public const string SHOP_ID = "SHOP_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string PRODUCT_VERSION = "PRODUCT_VERSION";
            public const string OWNER_TYPE = "OWNER_TYPE";
            //public const string PROCESS_ID = "PROCESS_ID";
            public const string STEP_ID = "STEP_ID";
            public const string TARGET_DATE = "PLAN_DATE";
            public const string EQP_ID = "EQP_ID";
            public const string IN_QTY = "IN_QTY";
            public const string OUT_QTY = "OUT_QTY";
        }

        public class StepWipSchema
        {
            public const string FACTORY_ID = "FACTORY_ID";
            public const string SHOP_ID = "SHOP_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string PROCESS_ID = "PROCESS_ID";
            public const string STEP_ID = "STEP_ID";
            public const string TARGET_DATE = "PLAN_DATE";
            public const string WAIT_QTY = "WAIT_QTY";
            public const string RUN_QTY = "RUN_QTY";

        }

        public class StepMoveInfo
        {
            public string ShopID;
            public string ProductID;
            public string ProductVersion;
            public string OwnerType;
            public string StepID;
            public string EqpID;
            public DateTime TargetDate;
            public float InQty;
            public float OutQty;

            public StepMoveInfo(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.ProductVersion = "-";
                this.OwnerType = string.Empty;
                this.StepID = string.Empty;
                this.EqpID = string.Empty;
                this.TargetDate = DateTime.MinValue;
                this.InQty = 0.0f;
                this.OutQty = 0.0f;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                ShopID = row.GetString(StepMoveSchema.SHOP_ID);
                ProductID = row.GetString(StepMoveSchema.PRODUCT_ID);
                OwnerType = row.GetString(StepMoveSchema.OWNER_TYPE);
                StepID = row.GetString(StepMoveSchema.STEP_ID);
                EqpID = row.GetString(StepMoveSchema.EQP_ID);
                TargetDate = row.GetDateTime(StepMoveSchema.TARGET_DATE);
                InQty = row.GetFloat(StepMoveSchema.IN_QTY);
                OutQty = row.GetFloat(StepMoveSchema.OUT_QTY);
            }
        }

        public class StepWip
        {
            public string ShopID;
            public string ProductID;
            public string ProductVersion;
            public string ProcessID;
            public string StepGroup;
            public string EqpGroup;
            public string StepID;
            public DateTime TargetDate;
            public float WaitQty;
            public float RunQty;

            public StepWip(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.ProductVersion = "-";
                this.ProcessID = string.Empty;
                this.StepID = string.Empty;
                this.StepGroup = string.Empty;
                this.EqpGroup = string.Empty;
                this.TargetDate = DateTime.MinValue;
                this.WaitQty = 0.0f;
                this.RunQty = 0.0f;

                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            { 
                this.ShopID = row.GetString("SHOP_ID");

                this.ProductID = row.GetString("PRODUCT_ID");

                this.ProcessID = row.GetString("PROCESS_ID");

                this.StepID = row.GetString("STEP_ID");

                this.TargetDate = row.GetDateTime("PLAN_DATE");

                this.WaitQty = row.GetFloat("WAIT_QTY");

                this.RunQty = row.GetFloat("RUN_QTY");
            }
        }

        public class EqpPlan
        {
            public string ShopID;
            public string BatchID;
            public string EqpID;
            public string EqpStatus;
            public string EqpGroupID;
            public string LotID;
            public string StepID;
            public string LayerID;
            public DateTime StartTime;
            public DateTime EndTime;
            public DateTime TrackInTime;
            public DateTime TrackOutTime;
            public string ToolID;
            public string ProductID;
            public string ProcessID;
            public int Qty;

            public EqpPlan(DataRow row)
            {
                this.ShopID = string.Empty;
                this.BatchID = string.Empty;
                this.EqpID = string.Empty;
                this.EqpStatus = string.Empty;
                this.EqpGroupID = string.Empty;
                this.LotID = string.Empty;
                this.StepID = string.Empty;
                this.LayerID = string.Empty;
                this.StartTime = DateTime.MaxValue;
                this.EndTime = DateTime.MaxValue;
                this.TrackInTime = DateTime.MaxValue;
                this.TrackOutTime = DateTime.MaxValue;
                this.ToolID = string.Empty;
                this.ProductID = string.Empty;
                this.ProcessID = string.Empty;
                this.Qty = 0;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.BatchID = row.GetString(Schema.BATCH_ID);
                this.EqpID = row.GetString(Schema.EQP_ID);
                this.EqpStatus = row.GetString(Schema.EQP_STATUS);
                this.EqpGroupID = row.GetString(Schema.EQP_GROUP_ID);
                this.LotID = row.GetString(Schema.LOT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.LayerID = row.GetString(Schema.LAYER_ID);
                this.StartTime = row.GetDateTime(Schema.START_TIME);
                this.EndTime = row.GetDateTime(Schema.END_TIME);
                this.TrackInTime = row.GetDateTime(Schema.TRACK_IN_TIME);
                this.TrackOutTime = row.GetDateTime(Schema.TRACK_OUT_TIME);
                this.ToolID = row.GetString(Schema.TOOL_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.ProcessID = row.GetString(Schema.PROCESS_ID);
                this.Qty = row.GetInt32(Schema.UNIT_QTY);
            }

            public class Schema
            {
                public const string VERSION_NO = "VERSION_NO";
                public const string FACTORY_ID = "FACTORY_ID";
                public const string SHOP_ID = "SHOP_ID";
                public const string BATCH_ID = "BATCH_ID";
                public const string EQP_ID = "EQP_ID";
                public const string EQP_STATUS = "EQP_STATUS";
                public const string EQP_GROUP_ID = "EQP_GROUP_ID";
                public const string PHASE_NO = "PHASE_NO";
                public const string LOT_ID = "LOT_ID";
                public const string STEP_ID = "STEP_ID";
                public const string LAYER_ID = "LAYER_ID";
                public const string START_TIME = "START_TIME";
                public const string END_TIME = "END_TIME";
                public const string TRACK_IN_TIME = "TRACK_IN_TIME";
                public const string TRACK_OUT_TIME = "TRACK_OUT_TIME";
                public const string TOOL_ID = "TOOL_ID";
                public const string PROCESS_ID = "PROCESS_ID";
                public const string PRODUCT_ID = "PRODUCT_ID";
                public const string UNIT_QTY = "UNIT_QTY";
            }
        }

        public class EqpDispatchLog
        {
            public string ShopID;
            public string EqpGroup;
            public string EqpID;
            public string SubEqpID;
            public DateTime DispatchingTime;
            public int InitWipCount;
            public int FilteredWipCount;
            public int SelectedWipCount;
            public string SelectedWip;
            public string FilteredWipLog;
            public string DispatchWipLog;
            public string PresetID;

            public EqpDispatchLog(DataRow dispatchingInfo)
            {
                this.ShopID = string.Empty;
                this.EqpGroup = string.Empty;
                this.EqpID = string.Empty;
                this.SubEqpID = string.Empty;
                this.DispatchingTime = DateTime.MinValue;
                this.InitWipCount = 0;
                this.FilteredWipCount = 0;
                this.SelectedWipCount = 0;
                this.SelectedWip = string.Empty;
                this.FilteredWipLog = string.Empty;
                this.DispatchWipLog = string.Empty;
                this.PresetID = string.Empty;

                ParseRow(dispatchingInfo);
            }

            private void ParseRow(DataRow row)
            {
                this.ShopID = row.GetString(Schema.SHOP_ID);

                this.EqpGroup = row.GetString(Schema.EQP_GROUP);
                this.EqpID = row.GetString(Schema.EQP_ID);
                this.SubEqpID = row.GetString(Schema.SUB_EQP_ID);

                this.DispatchingTime = row.GetString(Schema.DISPATCH_TIME).DbToDateTime();
                this.InitWipCount = row.GetInt32(Schema.INIT_WIP_CNT);
                this.FilteredWipCount = row.GetInt32(Schema.FILTERED_WIP_CNT);
                this.SelectedWipCount = row.GetInt32(Schema.SELECTED_WIP_CNT);
                this.SelectedWip = row.GetString(Schema.SELECTED_WIP);
                this.FilteredWipLog = row.GetString(Schema.FILTERED_WIP_LOG);
                this.DispatchWipLog = row.GetString(Schema.DISPATCH_WIP_LOG);
                this.PresetID = row.GetString(Schema.PRESET_ID);
            }

            private void ParseRow(CSOT.Lcd.Scheduling.Outputs.EqpDispatchLog dispatchingInfo)
            {
                this.ShopID = dispatchingInfo.SHOP_ID;

                this.EqpGroup = dispatchingInfo.EQP_GROUP;
                this.EqpID = dispatchingInfo.EQP_ID;
                this.SubEqpID = dispatchingInfo.SUB_EQP_ID;                

                this.DispatchingTime = dispatchingInfo.DISPATCHING_TIME.DbToDateTime();
                this.InitWipCount = dispatchingInfo.INIT_WIP_CNT;
                this.FilteredWipCount = dispatchingInfo.FILTERED_WIP_CNT;
                this.SelectedWipCount = dispatchingInfo.SELECTED_WIP_CNT;
                this.SelectedWip = dispatchingInfo.SELECTED_WIP;
                this.FilteredWipLog = dispatchingInfo.FILTERED_WIP_LOG;
                this.DispatchWipLog = dispatchingInfo.DISPATCH_WIP_LOG;
                this.PresetID = dispatchingInfo.PRESET_ID;
            }

            public class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string EQP_GROUP = "EQP_GROUP";
                public static string EQP_ID = "EQP_ID";
                public static string SUB_EQP_ID = "SUB_EQP_ID";
                public static string DISPATCH_TIME = "DISPATCHING_TIME";
                public static string INIT_WIP_CNT = "INIT_WIP_CNT";
                public static string FILTERED_WIP_CNT = "FILTERED_WIP_CNT";
                public static string SELECTED_WIP_CNT = "SELECTED_WIP_CNT";
                public static string SELECTED_WIP = "SELECTED_WIP";
                public static string FILTERED_WIP_LOG = "FILTERED_WIP_LOG";
                public static string DISPATCH_WIP_LOG = "DISPATCH_WIP_LOG";
                public static string PRESET_ID = "PRESET_ID";
            }
        }

        public class ToolMoveHistory
        {
            public string ShopID;
            public string ToolID;
            public string FromLocation;
            public string ToLocation;
            public DateTime LastReleaseTime;
            public DateTime MovedTime;

            public ToolMoveHistory(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ToolID = string.Empty;
                this.FromLocation = string.Empty;
                this.ToLocation = string.Empty;
                this.LastReleaseTime = DateTime.MaxValue;
                this.MovedTime = DateTime.MaxValue;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ToolID = row.GetString(Schema.TOOL_ID);
                this.FromLocation = row.GetString(Schema.FROM_LOCATION);
                this.ToLocation = row.GetString(Schema.TO_LOCATION);
                this.LastReleaseTime = row.GetDateTime(Schema.LAST_RELEASE_TIME);
                this.MovedTime = row.GetDateTime(Schema.MOVED_TIME);
            }

            public class Schema
            {
                public const string VERSION_NO = "VERSION_NO";
                public const string SHOP_ID = "SHOP_ID";
                public const string TOOL_ID = "TOOL_ID";
                public const string FROM_LOCATION = "FROM_LOCATION";
                public const string TO_LOCATION = "TO_LOCATION";
                public const string LAST_RELEASE_TIME = "LAST_RELEASE_TIME";
                public const string MOVED_TIME = "MOVED_TIME";
            }
        }

        public class SetupHistory
        {
            public string EqpID;
            public string ProductID;
            public string StepID;
            public string SetupType;
            public DateTime StartTime;
            public DateTime EndTime;

            public SetupHistory(DataRow row)
            {
                this.EqpID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.SetupType = string.Empty;
                this.StartTime = DateTime.MaxValue;
                this.EndTime = DateTime.MaxValue;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                this.EqpID = row.GetString(Schema.EQP_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.SetupType = row.GetString(Schema.SETUP_TYPE);
                this.StartTime = row.GetDateTime(Schema.START_TIME);
                this.EndTime = row.GetDateTime(Schema.END_TIME);
            }

            public class Schema
            {
                public const string EQP_ID = "EQP_ID";
                public const string PRODUCT_ID = "PRODUCT_ID";
                public const string STEP_ID = "STEP_ID";
                public const string SETUP_TYPE = "SETUP_TYPE";
                public const string START_TIME = "START_TIME";
                public const string END_TIME = "END_TIME";
            }
        }
    }
}
