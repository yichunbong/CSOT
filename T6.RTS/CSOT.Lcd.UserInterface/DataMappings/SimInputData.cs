using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class SimInputData
    {
        public class InputName
        {
            public const string StepMoveAct = "StepMoveAct";
            public const string StdStep = "StdStep";
            public const string Eqp = "Eqp";
            public const string ProcStep = "ProcStep";
            public const string Const = "Const";
            public const string Product = "Product";
        }

        public class ConstSchema
        {
            public const string CATEGORY = "CATEGORY";
            public const string CODE = "CODE";
            public const string DESCRIPTION = "DESCRIPTION";
        }

        public class StdStepSchema
        {
            public const string SHOP_ID = "SHOP_ID";
        }

        public class EqpSchema
        {
            public const string SHOP_ID = "SHOP_ID";
        }

        public class StepMoveActSchema
        {
            public const string VERSION_NO = "VERSION_NO";
            public const string FACTORY_ID = "FACTORY_ID";
            public const string SHOP_ID = "SHOP_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string OWNER_TYPE = "OWNER_TYPE";
            //public const string PROCESS_ID = "PROCESS_ID";
            public const string STEP_ID = "STEP_ID";
            public const string TARGET_DATE = "TARGET_DATE";
            public const string EQP_ID = "EQP_ID";
            public const string IN_QTY = "IN_QTY";
            public const string OUT_QTY = "OUT_QTY";
        }

        public class StepMoveAct
        {
            public string ShopID;
            public string ProductID;
            public string OwnerType;
            public string StepID;
            public string EqpID;
            public DateTime TargetDate;
            public float InQty;
            public float OutQty;

            public StepMoveAct(DataRow row)
            {
                this.ShopID = string.Empty;
                this.ProductID = string.Empty;
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
                ShopID = row.GetString(StepMoveActSchema.SHOP_ID);
                ProductID = row.GetString(StepMoveActSchema.PRODUCT_ID);
                OwnerType = row.GetString(StepMoveActSchema.OWNER_TYPE);
                StepID = row.GetString(StepMoveActSchema.STEP_ID);
                EqpID = row.GetString(StepMoveActSchema.EQP_ID);
                TargetDate = row.GetDateTime(StepMoveActSchema.TARGET_DATE);
                InQty = row.GetFloat(StepMoveActSchema.IN_QTY);
                OutQty = row.GetFloat(StepMoveActSchema.OUT_QTY);
            }
        }

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

        public class Eqp
        {
            public string FactoryID;
            public string ShopID;
            public string EqpGroupID;
            public string EqpID;            

            public Eqp(DataRow row)
            {
                FactoryID = string.Empty;
                ShopID = string.Empty;
                EqpID = string.Empty;
                EqpGroupID = string.Empty;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                FactoryID = row.GetString(Schema.FACTORY_ID);
                ShopID = row.GetString(Schema.SHOP_ID);
                EqpGroupID = row.GetString(Schema.EQP_GROUP_ID);
                EqpID = row.GetString(Schema.EQP_ID);                
            }

            public class Schema
            {
                public static string FACTORY_ID = "FACTORY_ID";
                public static string SHOP_ID = "SHOP_ID";
                public static string EQP_GROUP_ID = "EQP_GROUP_ID";
                public static string EQP_ID = "EQP_ID";

                public static string SIM_TYPE = "SIM_TYPE";
                public static string MAX_BATCH_SIZE = "MAX_BATCH_SIZE";
                public static string MIN_BATCH_SIZE = "MIN_BATCH_SIZE";
                public static string STATUS = "STATUS";
                public static string STATUS_CHANGE_TIME = "STATUS_CHANGE_TIME";
            }
        }

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

            public class Schema
            {
                public static string SHOP_ID = "SHOP_ID";
                public static string PRODUCT_ID = "PRODUCT_ID";
                public static string PRODUCT_TYPE = "PRODUCT_TYPE";
                public static string PRODUCT_KIND = "PRODUCT_KIND";
            }

        }
        #endregion
    }
}
