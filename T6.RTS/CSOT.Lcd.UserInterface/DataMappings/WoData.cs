using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class WoData
    {
        public class Schema
        {
            public const string SHOP_ID = "SHOP_ID";
            public const string EQP_ID = "EQP_ID";
            public const string EQP_GROUP_ID = "EQP_GROUP_ID";

            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string DAY = "DAY";
            public const string NIGHT = "NIGHT";
            public const string SHIFT = "SHIFT";
            public const string WORK_ORDER = "WORK_ORDER";
            public const string START_TIME = "START_TIME";
            public const string LAYER = "LAYER";
            public const string STEP_ID = "STEP_ID";
            public const string QTY = "QTY";

            public const string START_TIME2 = "START_TIME2";
            public const string LAYER2 = "LAYER2";
            public const string STEP_ID2 = "STEP_ID2";
            public const string QTY2 = "QTY2";

            public const string VALUE = "VALUE";
        }

        public class Const
        {
            public const string DAY = "DAY";
            public const string NIGHT = "NIGHT";
        }

        public class WorkOrder
        {
            public string ShopID { get; private set; }
            public string EqpID { get; private set; }
            public string EqpGroupID { get; private set; }
            public string EqpGroupIdInEqp { get; private set; }
            public string ProductID { get; private set; }
            public string Shift { get; private set; }

            public DateTime StartTime { get; private set; }
            public string StartTimeString { get; private set; }
            public string Layer { get; private set; }
            public string StepID { get; private set; }
            public int Qty { get; private set; }

            public WorkOrder()
            {
                this.ShopID = string.Empty;
                this.EqpID = string.Empty;
                this.EqpGroupID = string.Empty;
                this.ProductID = string.Empty;
                this.Shift = string.Empty;
                this.StartTime = DateTime.MaxValue;
                this.StartTimeString = string.Empty;
                this.Layer = string.Empty; ;
                this.StepID = string.Empty; ;
                this.Qty = 0;
            }

            public void SetBaseInfo(string shopID, string eqpID, string eqpGroupID, string eqpGroupIdInEqp, string productID, string shift,
                DateTime startTime, string layer, string stepID)
            {
                this.ShopID = shopID;
                this.EqpID = eqpID;
                this.EqpGroupID = eqpGroupID;
                this.EqpGroupIdInEqp = eqpGroupIdInEqp;
                this.ProductID = productID;
                this.Shift = shift;
                this.StartTime = startTime;
                this.StartTimeString = startTime.ToString("yyyy-MM-dd HH:mm:ss");
                this.Layer = layer;
                this.StepID = stepID;
                this.Qty = 0;
            }

            public void AddQty(int qty)
            {
                this.Qty += qty;
            }
        }

        //public class WorkOrderRslt
        //{
        //    public string ShopID { get; private set; }
        //    public string EqpGroupID { get; private set; }
        //    public string EqpID { get; private set; }
        //    public string ProductID { get; private set; }
        //    public string Shift { get; private set; }
        //    public string Category { get; private set; }
        //    public string Value { get; private set; }

        //    public WorkOrderRslt(string shopID, string eqpGrpID, string eqpID, string prodID, string shift, string category, string value)
        //    {
        //        this.ShopID = shopID;
        //        this.EqpGroupID = eqpGrpID;
        //        this.EqpID = eqpID;
        //        this.ProductID = prodID;
        //        this.Shift = shift;
        //        this.Category = category;
        //        this.Value = value;
        //    }
        //}
    }
}
