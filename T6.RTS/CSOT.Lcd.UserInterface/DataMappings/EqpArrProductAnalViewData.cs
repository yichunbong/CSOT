using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class EqpArrProductAnalViewData
    {
        #region Input Table Name

        public const string EQP_ARRANGE_DT = "EqpArrange";
        public const string STD_STEP_DT = "StdStep";

        #endregion

        public class Consts
        {
            public const string Production = "Production";            
        }

        #region Pivot Column Name

        public const string SHOP_ID = "SHOP_ID";
        public const string EQP_ID = "EQP_ID";
        public const string PROD_QTY = "PROD_QTY";
        public const string STEP_ID = "STEP_ID";
        public const string STEP_DESC = "STEP_DESC";

        public const string PRODUCT_ID = "PRODUCT_ID";
        public const string PRODUCT_VERSION = "PRODUCT_VERSION";

        #endregion
        
        #region Input Data Transform

        public class EqpArrange
        {
            public string FactoryID;
            public string ShopID;
            public string EqpID;
            public string EqpType;
            public string ProcessID;
            public string ProductID;
            public string StepID;
            public string ArrType;
            public string Priority;
            public DateTime StartTime;
            public DateTime EndTime;
            public DateTime UpdateTime;

            public EqpArrange(DataRow row)
            {
                this.FactoryID = string.Empty;
                this.ShopID = string.Empty;
                this.EqpID = string.Empty;
                this.EqpType = string.Empty;
                this.ProcessID = string.Empty;
                this.ProductID = string.Empty;
                this.StepID = string.Empty;
                this.ArrType = string.Empty;
                this.Priority = string.Empty;
                this.StartTime = DateTime.MinValue;
                this.EndTime = DateTime.MinValue;
                this.UpdateTime = DateTime.MinValue;

                ParseRow(row);
            }

            private void ParseRow(DataRow row)
            {
                FactoryID = row.GetString("FACTORY_ID");
                ShopID = row.GetString("SHOP_ID");
                EqpID = row.GetString("EQP_ID");
                EqpType = row.GetString("EQP_TYPE");
                ProcessID = row.GetString("PROCESS_ID");
                ProductID = row.GetString("PRODUCT_ID");
                StepID = row.GetString("STEP_ID");
                ArrType = row.GetString("ARR_TYPE");
                Priority = row.GetString("PRIORITY");
                StartTime = row.GetDateTime("START_TIME");
                EndTime = row.GetDateTime("END_TIME");
                UpdateTime = row.GetDateTime("UPDATE_TIME");
            }
        }


        #endregion

        public class EqpArrAnalResult
        {
            public string SHOP_ID;
            public string EQP_ID;
            public string STEP_ID;
            public string STEP_DESC;
            public Dictionary<string, List<string>> PROD_DIC;
            
            public int PROD_QTY
            {
                get
                {
                    return this.PROD_DIC == null ? 0 : this.PROD_DIC.Count; 
                }
            }

            public EqpArrAnalResult(string shopID, string eqpID, string stepID, string stepDesc)
            {
                this.SHOP_ID = shopID;
                this.EQP_ID = eqpID;
                this.STEP_ID = stepID;
                this.STEP_DESC = stepDesc;

                this.PROD_DIC = new Dictionary<string, List<string>>();
            }

            public void AddProduct(string productID, string productVersion)
            {
                List<string> verList;
                if (this.PROD_DIC.TryGetValue(productID, out verList) == false)
                    this.PROD_DIC.Add(productID, verList = new List<string>());

                if (verList.Contains(productVersion) == false)
                    verList.Add(productVersion);
            }
        }
    }
}
