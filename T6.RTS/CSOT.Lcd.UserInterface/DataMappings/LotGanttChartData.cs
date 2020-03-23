using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class LotGanttChartData
    {
        public const string LOT_TABLE_NAME = "Wip";

        public const string EQP_PLAN_TABLE_NAME = "EqpPlan";

        #region Product
        public class Product
        {
            public string LineID;
            public string ProductID;
            public string ProductGroup;
            public string ProductKind;

            public Product(DataRow row)
            {
                LineID = string.Empty;
                ProductID = string.Empty;
                ProductGroup = string.Empty;
                ProductKind = string.Empty;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                LineID = row.GetString(Schema.LINE_ID);
                ProductID = row.GetString(Schema.PRODUCT_ID);
                ProductGroup = row.GetString(Schema.PRODUCT_GROUP);
                ProductKind = row.GetString(Schema.PRODUCT_KIND);
            }

            internal class Schema
            {
                public static string LINE_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string PRODUCT_GROUP = "PRODUCT_TYPE";
                public static string PRODUCT_KIND = "PRODUCT_KIND";
            }

        }
        #endregion

        #region Lot
        public class Lot
        {
            public string LineID;
            public string LotID;

            public Lot(DataRow row)
            {
                LineID = string.Empty;
                LotID = string.Empty;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                LineID = row.GetString(Schema.SHOP_ID);
                LotID = row.GetString(Schema.LOT_ID);
            }

            internal class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string LOT_ID = "LOT_ID";
            }
        }
        #endregion 

        #region EqpPlan
        public class EqpPlan
        {
            public string LineID;
            public string LotID;
            public DateTime TargetDate;
            
            public EqpPlan(DataRow row)
            {
                LineID = string.Empty;
                LotID = string.Empty;
                TargetDate = DateTime.MinValue;
                
                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                LineID = row.GetString(Schema.LINE_ID);

                LotID = row.GetString(Schema.LOT_ID);

                TargetDate = row.GetString(Schema.TARGET_DATE).DbToDateTime();
            }

            internal class Schema
            {
                public static string LINE_ID = "SHOP_ID";
                public static string LOT_ID = "LOT_ID";
                public static string STEP_SEQ = "STEP_ID";
                public static string START_TIME = "START_TIME";
                public static string END_TIME = "END_TIME";
                public static string UNIT_QTY = "UNIT_QTY";
                public static string TARGET_DATE = "TARGET_DAY";
            }
        }
        #endregion EqpPlan

        // 메인 UI

        public const string LINE_ID = "Shop ID";
        public const string PRODUCT_ID = "Product ID";
        public const string KOP = "KOP";
        public const string LOT_ID = "Lot ID";
        public const string QUERY_RANGE = "Date Range";
    }
}
