using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class JobChangePlanData
    {
        #region Input Table Name

        public const string PRODUCT_DATA_TABLE = "Product";
        public const string STD_STEP_DATA_TABLE = "StdStep";
        public const string EQP_PLAN_DATA_TABLE = "EqpPlan";
        public const string INNER_BOM_DATA_TABLE = "InnerBom";

        #endregion

        #region Conditions

        //public const bool USE_STEP_GROUP = false;
        //public const bool USE_EQP_GROUP = false;

        //public const bool USE_HOUR_QUERY = false;

        #endregion


        #region Input Data Transform

        #region Product
        public class Product
        {
            public string LineID;
            public string ProductID;
            public string ProductGroup;
            public string ProductKind;

            public Product(DataRow row)
            {
                this.LineID = string.Empty;
                this.ProductID = string.Empty;
                this.ProductGroup = string.Empty;
                this.ProductKind = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                LineID = row.GetString(Schema.LINE_ID);
                ProductID = row.GetString(Schema.PRODUCT_ID);
                ProductGroup = row.GetString(Schema.PRODUCT_GROUP);
                ProductKind = row.GetString(Schema.PRODUCT_KIND);
            }

            internal class Schema
            {
                public static string LINE_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string PRODUCT_GROUP = "PRODUCT_GROUP";
                public static string PRODUCT_KIND = "PRODUCT_KIND";
            }
        }
        #endregion

        #region StdStep

        public class StdStep
        {
            public string ShopID;
            public string StepID;
            public string Layer;
            public string StepType;
            public string StepGroup;
            public string StepDesc;

            public StdStep(DataRow row)
            {
                ShopID = string.Empty;
                StepID = string.Empty;
                Layer = string.Empty;
                StepType = string.Empty;
                StepGroup = string.Empty;
                StepDesc = string.Empty;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                ShopID = row.GetString(Schema.SHOP_ID);
                StepID = row.GetString(Schema.STEP_ID);
                Layer = row.GetString(Schema.LAYER);
                StepType = row.GetString(Schema.STEP_TYPE);
                StepGroup = row.GetString(Schema.STEP_GROUP);
                StepDesc = row.GetString(Schema.STEP_DESC);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string STEP_ID = "STEP_ID";
                public static string LAYER = "LAYER";
                public static string STEP_TYPE = "STEP_TYPE";
                public static string STEP_GROUP = "STEP_GROUP";
                public static string STEP_DESC = "STEP_DESC";
            }
        }

        #endregion

        #region EqpPlan
        public class EqpPlan
        {
            public string ShopID;
            public string ProductID;
            public string BatchID;
            public string StepID;
            public string TargetDate;
            public DateTime StartTime;
            public DateTime EndTime;
            public float OutTargetQty;
            public string LotID;

            public EqpPlan(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.BatchID = string.Empty;
                this.StepID = string.Empty;
                this.TargetDate = string.Empty;
                this.OutTargetQty = 0.0f;
                this.StartTime = DateTime.MinValue;
                this.EndTime = DateTime.MaxValue;
                this.LotID = string.Empty;

                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            {
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.BatchID = row.GetString(Schema.BATCH_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.LotID = row.GetString(Schema.LOT_ID);

                this.TargetDate = ShopCalendar.SplitDate(row.GetDateTime(Schema.TARGET_DATE)).ToString("yyyyMMdd");

                this.OutTargetQty = row.GetFloat(Schema.OUT_TARGET_QTY);
                this.StartTime = row.GetDateTime(Schema.START_TIME);
                this.EndTime = row.GetDateTime(Schema.END_TIME);
            }


            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string BATCH_ID = "BATCH_ID";
                public static string STEP_ID = "STEP_ID";
                public static string TARGET_DATE = "START_TIME"; //"TARGET_DATE";
                public static string OUT_TARGET_QTY = "UNIT_QTY";
                public static string START_TIME = "START_TIME";
                public static string END_TIME = "END_TIME";
                public static string LOT_ID = "LOT_ID";
            }
        }
        #endregion

        #region InnerBom

        public class InnerBom
        {
            public string ShopID;
            public string ToProductID;
            public string FromProductID;

            public InnerBom(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ToProductID = string.Empty;
                this.FromProductID = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ToProductID = row.GetString(Schema.TO_PRODUCT_ID);
                this.FromProductID = row.GetString(Schema.FROM_PRODUCT_ID);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string TO_PRODUCT_ID = "TO_PRODUCT_ID";
                public static string FROM_PRODUCT_ID = "FROM_PRODUCT_ID";
            }
        }

        #endregion 

        #endregion


        #region UI 화면 Caption

        // 메인 UI : Grid
        public const string PRODUCT_ID = "PRODUCT_ID";
        public const string BATCH_ID = "BATCH_ID";
        public const string SHOP_ID = "SHOP_ID";
        public const string IN_QTY = "IN_QTY";
        public const string OUT_QTY = "OUT_QTY";
        public const string AVG_TAT = "AVG. TAT(DAYS)";
        public const string IN_TIME = "IN_TIME";
        public const string OUT_TIME = "OUT_TIME";
        public const string IN_DURATION = "IN_DURATION(HR)";
        public const string OUT_DURATION = "OUT_DURATION(HR)";

        // 서브 UI : PivotGrid
        public const string LAYER = "LAYER";
        public const string STEP_ID = "STEP_ID";
        public const string STEP_DESC = "DESC";
        public const string SPAN = "SPAN(HR)";
        public const string TOTAL = "TOTAL";
        public const string SHIFT = "SHIFT";
        public const string QTY = "QTY";

        #endregion
    }
}
