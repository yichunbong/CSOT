using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class TargetPlanCompareData
    {
        #region Input Table Name

        public const string DATA_TABLE_0 = "Product";
        public const string DATA_TABLE_1 = "ProcStep";
        public const string DATA_TABLE_2 = "StepTarget";
        public const string DATA_TABLE_3 = "EqpPlan";
        public const string DATA_TABLE_4 = "StepMove";

        #endregion

        #region Conditions

        //public const bool USE_STEP_GROUP = false;
        //public const bool USE_EQP_GROUP = false;

        //public const bool USE_HOUR_QUERY = false;

        #endregion


        #region Input Data Transform

        public class Product
        {
            public string ShopID;
            public string ProductID;
            public string ProductGroup;
            public string ProductKind;

            public Product(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.ProductGroup = string.Empty;
                this.ProductKind = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                ShopID = row.GetString(Schema.SHOP_ID);
                ProductID = row.GetString(Schema.PRODUCT_ID);
                ProductGroup = row.GetString(Schema.PRODUCT_GROUP);
                ProductKind = row.GetString(Schema.PRODUCT_KIND);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string PRODUCT_GROUP = "PRODUCT_GROUP";
                public static string PRODUCT_KIND = "PRODUCT_KIND";
            }
        }

        public class ProcStep
        {
            public string ShopID;
            public string ProductID;
            public string StepID;
            public string StepType;

            public ProcStep(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.StepType = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                // LINE_ID
                ShopID = row.GetString(Schema.SHOP_ID);
                ProductID = row.GetString(Schema.PRODUCT_ID);
                StepID = row.GetString(Schema.STEP_ID);
                StepType = row.GetString(Schema.STEP_TYPE);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string STEP_TYPE = "STEP_TYPE";
            }
        }


        public class StepTarget
        {
            public string AreaID;
            public string ShopID;
            public string ProductID;
            public string StepID;
            public string TargetDate;
            public DateTime CampareTargetDate;
            public float OutTargetQty;
            public string StepType;

            public StepTarget(DataRow row)
            {
                this.AreaID = string.Empty;
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.TargetDate = string.Empty;
                this.CampareTargetDate = DateTime.MinValue;
                this.OutTargetQty = 0.0f;
                this.StepType = string.Empty;
                
                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            {
                this.AreaID = row.GetString(Schema.AREA_ID);
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);

                this.TargetDate = ShopCalendar.SplitDate(row.GetDateTime(Schema.TARGET_DATE)).ToString("yyyyMMdd");
                this.CampareTargetDate = row.GetDateTime(Schema.TARGET_DATE);

                this.OutTargetQty = row.GetFloat(Schema.OUT_TARGET_QTY);
                this.StepType = row.GetString(Schema.STEP_TYPE);
            }


            internal class Schema
            {
                public static string AREA_ID = "AREA_ID";
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string TARGET_DATE = "TARGET_DATE";
                public static string OUT_TARGET_QTY = "TARGET_OUT_QTY";
                public static string STEP_TYPE = "STEP_TYPE";
            }


        }

        public class EqpPlan
        {
            public string ShopID;
            public string ProductID;
            public string StepID;
            public string StepType;
            public string OwnerType;
            public string TargetDate;
            public DateTime TrackInTime;
            public DateTime TrackOutTime;
            public float OutTargetQty;
           
            public EqpPlan(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.StepType = string.Empty;
                this.OwnerType = string.Empty;
                this.TargetDate = string.Empty;
                this.TrackInTime = DateTime.MinValue;
                this.TrackOutTime = DateTime.MinValue;
                this.OutTargetQty = 0.0f;

                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            {
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.StepType = row.GetString(Schema.STEP_TYPE);
                this.OwnerType = row.GetString(Schema.OWNER_TYPE);

                this.TargetDate = row.GetDateTime(Schema.TARGET_DATE).ToString("yyyyMMdd");
                //this.TargetDate = ShopCalendar.SplitDate(row.GetDateTime(Schema.TARGET_DATE)).ToString("yyyyMMdd");
                this.TrackInTime = row.GetDateTime(Schema.TRACK_IN_TIME);
                this.TrackOutTime = row.GetDateTime(Schema.TRACK_OUT_TIME);

                this.OutTargetQty = row.GetFloat(Schema.OUT_TARGET_QTY);
            }


            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string STEP_TYPE = "STEP_TYPE";
                public static string OWNER_TYPE = "OWNER_TYPE";
                public static string TARGET_DATE = "START_TIME"; //"TARGET_DATE";
                public static string TRACK_IN_TIME = "TRACK_IN_TIME";
                public static string TRACK_OUT_TIME = "TRACK_OUT_TIME";
                public static string OUT_TARGET_QTY = "UNIT_QTY";
            }


        }

        public class StepMove
        {
            public string AreaID;
            public string ShopID;
            public string ProductID;
            public string StepID;
            public int StdStepSeq;
            public string OwnerType;
            public string TargetDate;
            public DateTime CompareTargetDate;
            public float InQty;
            public float OutQty;

            public StepMove(DataRow row)
            {
                this.AreaID = string.Empty;
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.StdStepSeq = 0;
                this.OwnerType = string.Empty;
                this.TargetDate = string.Empty;
                this.CompareTargetDate = DateTime.MinValue;
                this.InQty = 0.0f;
                this.OutQty = 0.0f;

                ParseRow(row);
            }

            ///
            /// 개발 포인트 
            ///
            private void ParseRow(DataRow row)
            {
                this.AreaID = row.GetString(Schema.AREA_ID);
                this.ShopID = row.GetString(Schema.SHOP_ID);
                this.ProductID = row.GetString(Schema.PRODUCT_ID);
                this.StepID = row.GetString(Schema.STEP_ID);
                this.StdStepSeq = row.GetInt32(Schema.STD_STEP_SEQ);
                this.OwnerType = row.GetString(Schema.OWNER_TYPE);

                //this.TargetDate = row.GetDateTime(Schema.TARGET_DATE).ToString("yyyyMMdd");
                this.TargetDate = ShopCalendar.SplitDate(row.GetDateTime(Schema.TARGET_DATE)).ToString("yyyyMMdd");
                this.CompareTargetDate = row.GetDateTime(Schema.TARGET_DATE);
                this.InQty = row.GetFloat(Schema.IN_QTY);
                this.OutQty = row.GetFloat(Schema.OUT_QTY);
            }


            internal class Schema
            {
                public static string AREA_ID = "AREA_ID";
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string STEP_ID = "STEP_ID";
                public static string STD_STEP_SEQ = "STD_STEP_SEQ";
                public static string OWNER_TYPE = "OWNER_TYPE";
                public static string TARGET_DATE = "PLAN_DATE";
                public static string IN_QTY = "IN_QTY";
                public static string OUT_QTY = "OUT_QTY";
            }


        }


        #endregion



        #region UI 화면 Caption

        // 메인 UI
        public const string SHOP_ID = "SHOP_ID";
        public const string PRODUCT_ID = "PRODUCT_ID";
        public const string STEP_ID = "STEP_ID";
        public const string OWNER_TYPE = "OWNER_TYPE";
        public const string CATEGORY = "CATEGORY";
        public const string TOTAL = "TOTAL";
        public const string TARGET_DATE = "TARGET_DATE";
        public const string QTY = "QTY";

        #endregion
    }
}
