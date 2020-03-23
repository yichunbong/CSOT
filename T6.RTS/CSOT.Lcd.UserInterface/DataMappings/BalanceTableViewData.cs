using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class BalanceTableViewData
    {
        #region Input Table Name

        public const string DEMAND_DATA_TABLE = "InOutPlan";
        public const string INNER_BOM_DATA_TABLE = "InnerBom";
        public const string STEP_TARGET_DATA_TABLE = "StepTarget";
        public const string EQP_PLAN_DATA_TABLE = "EqpPlan";
        public const string WIP_DATA_TABLE = "Wip";

        #endregion

        #region Conditions

        //public const bool USE_STEP_GROUP = false;
        //public const bool USE_EQP_GROUP = false;

        //public const bool USE_HOUR_QUERY = false;

        #endregion


        #region Input Data Transform

        public class Demand
        {
            public string ShopID;
            public string ProductID;
            
            public Demand(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
            }
        }

        public class InnerBom
        {
            public string ShopID;
            public string ToProductID;
            public string FromProductID;
            public string StepID;

            public InnerBom(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ToProductID = string.Empty;
                this.FromProductID = string.Empty;
                this.StepID = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ToProductID = row.GetString(Schema.TO_PRODUCT_ID);
                this.FromProductID = row.GetString(Schema.FROM_PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string TO_PRODUCT_ID = "TO_PRODUCT_ID";
                public static string FROM_PRODUCT_ID = "FROM_PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
            }
        }

        public class StepTarget
        {
            public string LineID;
            public string ProductID;
            public string StepID;
            public string TargetDate;
            public float InTargetQty;
            public float OutTargetQty;
            public string StepType;

            public StepTarget(DataRow row)
            {
                this.LineID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.TargetDate = string.Empty;
                this.InTargetQty = 0.0f;
                this.OutTargetQty = 0.0f;
                this.StepType = string.Empty;

                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            {
                this.LineID = row.GetString(Schema.LINE_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.TargetDate = row.GetDateTime(Schema.TARGET_DATE).ToString("yyyy-MM-dd HH");
                this.InTargetQty = row.GetFloat(Schema.IN_TARGET_QTY);
                this.OutTargetQty = row.GetFloat(Schema.OUT_TARGET_QTY);
                this.StepType = row.GetString(Schema.STEP_TYPE);
            }


            internal class Schema
            {
                public static string LINE_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string TARGET_DATE = "TARGET_SHIFT";
                public static string OUT_TARGET_QTY = "OUT_QTY";
                public static string IN_TARGET_QTY = "IN_QTY";
                public static string STEP_TYPE = "STEP_TYPE";
            }


        }

        public class EqpPlan
        {
            public string LineID;
            public string ProductID;
            public string StepID;
            public DateTime StartTime;
            public DateTime EndTime;
            public float OutTargetQty;

            public EqpPlan(DataRow row)
            {
                this.LineID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.StartTime = DateTime.MinValue;
                this.EndTime = DateTime.MinValue;
                this.OutTargetQty = 0.0f;

                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            {
                this.LineID = row.GetString(Schema.LINE_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.StartTime = row.GetDateTime(Schema.START_TIME);
                this.EndTime = row.GetDateTime(Schema.END_TIME);
                this.OutTargetQty = row.GetFloat(Schema.OUT_TARGET_QTY);
            }


            internal class Schema
            {
                public static string LINE_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string START_TIME = "START_TIME"; //"TARGET_DATE";
                public static string END_TIME = "END_TIME";
                public static string OUT_TARGET_QTY = "UNIT_QTY";
            }


        }


        public class Wip
        {
            public string ShopID;
            public string ProductID;
            public string StepID;
            public float Qty;

            public Wip(DataRow row)
            {
                ShopID = string.Empty;
                ProductID = string.Empty;
                StepID = string.Empty;
                Qty = 0.0f;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                ShopID = row.GetString(Schema.SHOP_ID);
                ProductID = row.GetString(Schema.PRODUCT_ID);
                StepID = row.GetString(Schema.STEP_ID);
                Qty = row.GetFloat(Schema.QTY);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string QTY = "GLASS_QTY";
            }
        }

        #endregion

        #region STATIC VARIABLE

        public static string BANK_STEP = "C00000";
        public static string NEXT_STEP = "C01000";
        public static string CELL_STEP = "C06000";
        public static string INITIAL_SHIFT = "BOH";

        #endregion 

        #region UI 화면 Caption

        // 메인 UI
        public const string CELL_OUT_PRODUCT = "CELL_OUT_PRODUCT";
        public const string CELL_PRODUCT = "CELL_PRODUCT";
        public const string FROM_PRODUCT_ID = "FROM_PRODUCT_ID";
        public const string CATEGORY = "CATEGORY";
        public const string TOTAL = "TOTAL";
        public const string TARGET_DATE = "TARGET_DATE";
        public const string QTY = "QTY";

        #endregion
    }
}
