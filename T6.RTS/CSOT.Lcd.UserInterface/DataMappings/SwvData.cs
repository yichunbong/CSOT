using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class SwvData
    {
        public class StepWipChartInf
        {
            public DateTime TargetDate;
            public string ShopID;
            public string StepID;
            public string StepDesc;
            public int StdStepSeq;
            public string Layer;
            public int WaitQty;
            public int RunQty;
            public int WipQty;

            public StepWipChartInf(DateTime targetDate, string shopID, string stepID,
                string stepDesc, int stdStepSeq, string layer)
            {
                this.TargetDate = targetDate;
                this.ShopID = shopID;
                this.StepID = stepID;
                this.StepDesc = stepDesc;
                this.StdStepSeq = stdStepSeq;
                this.Layer = layer;
                this.WaitQty = 0;
                this.RunQty = 0;
                this.WipQty = 0;
            }

            public void AddWip(int waitQty, int runQty)
            {
                this.WaitQty += waitQty;
                this.RunQty += runQty;
                this.WipQty += waitQty + runQty;
            }
        }

        public class StepWipInf
        {
            public DateTime TargetDate;
            public string ShopID;
            public string ProductID;
            public string ProcessID;
            public string StepID;
            public string StepDesc;
            public int StdStepSeq;
            public string Layer;
            public int WaitQty;
            public int RunQty;
            public int WipQty;

            public StepWipInf(DateTime targetDate, string shopID, string prodID, string procID, string stepID, 
                string stepDesc, int stdStepSeq, string layer)
            {
                this.TargetDate = targetDate;
                this.ShopID = shopID;
                this.ProductID = prodID;
                this.ProcessID = procID;
                this.StepID = stepID;
                this.StepDesc = stepDesc;
                this.StdStepSeq = stdStepSeq;
                this.Layer = layer;
                this.WaitQty = 0;
                this.RunQty = 0;
                this.WipQty = 0;
            }

            public void AddWip(int waitQty, int runQty)
            {
                this.WaitQty += waitQty;
                this.RunQty += runQty;
                this.WipQty += waitQty + runQty;
            }
        }

        public class StepInfo
        {
            //string FactoryID { public get; private set; }
            public string ShopID { get; private set; }
            public string StepID { get; private set; }
            public int StepSeq { get; private set; }
            public string StepDesc { get; private set; }
            public string StepType { get; private set; }
            public string Layer { get; private set; }

            public StepInfo(string shopID, string stepID, string stepDesc, string stepType, int stepSeq, string layer)
            {
                this.ShopID = shopID;
                this.StepID = stepID;
                this.StepDesc = stepDesc;
                this.StepType = stepType;
                this.StepSeq = stepSeq;
                this.Layer = layer;
            }
        }

    }
}
