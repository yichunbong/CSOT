using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class StepWipData
    {
        #region Input Table Name

        public const string DATA_TABLE_1 = "StepWip";
        public const string DATA_TABLE_2 = "ProcStep";
        public const string DATA_TABLE_3 = "StdStep";
        public const string TFT = "TFT";

        #endregion

        #region Input Data Transform


        public class ProcStep
        {
            public string LineID;
            public string StepID;

            public ProcStep(DataRow row)
            {
                this.LineID = string.Empty;
                this.StepID = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                LineID = row.GetString("SHOP_ID");

                // STEP_ID
                StepID = row.GetString("STEP_ID");
            }

        }


        public class StepWip
        {
            public string AreaID;
            public string ShopID;
            public string ProductID;
            public string ProductVersion;
            public string OrigProductVersion;
            public string OwnerType;
            public string StepGroup;
            public string EqpGroup;
            public string StepID;
            public string StdStepID;
            public int StdStepSeq;
            public DateTime TargetDate;
            public float WaitQty;
            public float RunQty;

            public StepWip(DataRow row)
            {
                this.AreaID = string.Empty;
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.ProductVersion = string.Empty;
                this.OrigProductVersion = string.Empty;
                this.OwnerType = string.Empty;
                this.StepID = string.Empty;
                this.StdStepID = string.Empty;
                this.StdStepSeq = 0;
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
                // AREA_ID
                AreaID = row.GetString("AREA_ID");
                
                // SHOP_ID
                ShopID = row.GetString("SHOP_ID");

                // PRODUCT_ID
                ProductID = row.GetString("PRODUCT_ID");

                // PRODUCT_VERSION
                ProductVersion = row.GetString("PRODUCT_VERSION");

                // PRODUCT_VERSION
                OrigProductVersion = row.GetString("ORIG_PRODUCT_VERSION");

                // OWNER_TYPE
                OwnerType = row.GetString("OWNER_TYPE");

                // STEP_ID
                StepID = row.GetString("STEP_ID");

                // STD_STEP_ID
                StdStepID = row.GetString("STD_STEP_ID");

                // STD_STEP_SEQ
                StdStepSeq = row.GetInt32("STD_STEP_SEQ");

                // TARGET_DATE
                this.TargetDate = row.GetDateTime("PLAN_DATE");

                // WAIT_QTY
                WaitQty = row.GetFloat("WAIT_QTY");

                // RUN_QTY
                RunQty = row.GetFloat("RUN_QTY");
            }
        }


        #endregion

        #region UI 화면 Caption

        // 메인 UI
        public const string SHOP_ID = "SHOP_ID";
        public const string STD_STEP = "STEP";
        public const string PRODUCT_ID = "PRODUCT_ID";
        public const string PRODUCT_VERSION = "PRODUCT_VERSION";
        public const string OWNER_TYPE = "OWNER_TYPE";
        //public const string PROCESS_ID = "PROCESS_ID";        
        
        public const string TARGET_DATE = "TARGET_DATE";
        public const string WAIT_QTY = "WAIT_QTY";
        public const string RUN_QTY = "RUN_QTY";
        public const string TOTAL_QTY = "TOTAL_QTY";

        public const string BALANCE_STEP = "BALANCE_STEP";
        public const string BALANCE_WIP_QTY = "BALANCE_WIP_QTY";
        public const string BALANCE_DIFF = "BALANCE_DIFF";

        #endregion
    }
}
