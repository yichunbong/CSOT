using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class RtcvData
    {
        public class StepTImeInfo
        {
            public double TactTime { get; private set; }
            public double ProcTime { get; private set; }

            public StepTImeInfo(double tactTime, double procTime)
            {
                this.TactTime = tactTime;
                this.ProcTime = procTime;
            }
        }

        public class ResultInfo
        {
            public string FactoryID { get; private set; }
            public string ShopID { get; private set; }
            public string EqpID { get; private set; }
            public string LotID { get; private set; }
            public string StepID { get; private set; }
            public string LayerID { get; private set; }
            public DateTime StartTIme { get; private set; }
            public DateTime EndTime { get; private set; }
            public string ProcessID { get; private set; }
            public string ProductID { get; private set; }
            public double RunQty { get; private set; }
            public double ActRunTime { get; private set; }
            public double SimRunTime { get; private set; }
            public double Difference { get { return this.ActRunTime - this.SimRunTime; } }

            public ResultInfo(string facID, string shopID, string eqpID, string lotID, string stepID, string layerID, DateTime startTime, DateTime endTime,
                string processID, string productID, int runQty)
            {
                this.FactoryID = facID;
                this.EqpID = eqpID;
                this.ShopID = shopID;
                this.LotID = lotID;
                this.StepID = stepID;
                this.LayerID = layerID;
                this.StartTIme = startTime;
                this.EndTime = endTime;
                this.ProcessID = processID;
                this.ProductID = productID;
                this.RunQty = runQty;
                this.ActRunTime = this.EndTime <= this.StartTIme ? 0 : (this.EndTime - this.StartTIme).TotalSeconds;
                this.SimRunTime = 0;
            }

            public void SetSimRunTime(double simRunTime)
            {
                this.SimRunTime = simRunTime;
            }
        }

        public class Schema
        {
            public const string FACTORY_ID = "FACTORY_ID";
            public const string SHOP_ID = "SHOP_ID";
            public const string EQP_ID = "EQP_ID";
            public const string LOT_ID = "LOT_ID";
            public const string STEP_ID = "STEP_ID";
            public const string LAYER_ID = "LAYER_ID";
            public const string START_TIME = "START_TIME";
            public const string END_TIME = "END_TIME";
            public const string PROCESS_ID = "PROCESS_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string RUN_QTY = "RUN_QTY";
            public const string ACT_RUN_TIME = "ACT_RUN_TIME";
            public const string SIM_RUN_TIME = "SIM_RUN_TIME";
            public const string DIFFERENCE = "DIFFERENCE";
        }
    }
}
