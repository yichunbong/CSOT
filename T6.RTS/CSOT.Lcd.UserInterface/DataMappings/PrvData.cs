using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class PrvData
    {
        public class Schema
        {
            public const string PROG_RATE = "PROG_RATE";
            public const string GLAGG_QTY = "GLAGG_QTY";

            public const string SHOP_ID = "SHOP_ID";
            public const string TARGET_DATE = "TARGET_DATE";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string PROCESS_ID = "PROCESS_ID";
            public const string STEP_ID = "STEP_ID";
            public const string STD_STEP_SEQ = "STD_STEP_SEQ";
            public const string STEP_DESC = "STEP_DESC";
            public const string LAYER = "LAYER";
            public const string IS_RUN = "IS_RUN";
            public const string QTY = "QTY";
            //public const string PROG_RATE = "PROG_RATE";
            public const string RANGE = "RANGE";
            public const string WAIT_TAT = "WAIT_TAT";
            public const string RUN_TAT = "RUN_TAT";
            public const string TAT = "TAT";
            public const string CUM_TAT = "CUM_TAT";
            public const string TOTAL_TAT = "TOTAL_TAT";
        }

        public class TatInfo
        {
            public double WaitTat { get; private set; }
            public double RunTat { get; private set; }
            public double Tat { get; private set; }
            //public double CumTat { get; private set; }
            //public double BcumTat { get; private set; }

            public TatInfo(double waitTat, double runTat)
            {
                this.WaitTat = waitTat;
                this.RunTat = runTat;
                this.Tat = waitTat + runTat;
                //this.CumTat = 0;
                //this.BcumTat = 0;
            }
        }

        public class ProcStepTatInfo
        {
            public string ShopID { get; private set; }
            public string ProdID { get; private set; }
            public string ProcID { get; private set; }
            public string StepID { get; private set; }
            public int StepSeq { get; private set; }            
            public double WaitTat { get; private set; }
            public double RunTat { get; private set; }
            public double Tat { get; private set; }
            public double CumTat { get; private set; }
            public double BcumTat { get; private set; }
            public double TotalTat { get; private set; }

            public ProcStepTatInfo(string shopID, string prodID, string procID, string stepID, int stepSeq, double waitTat, double runTat, double cumTat)
            {
                this.ShopID = shopID;
                this.ProdID = prodID;
                this.ProcID = procID;
                this.StepID = stepID;
                this.StepSeq = stepSeq;                
                this.WaitTat = waitTat;
                this.RunTat = runTat;
                this.Tat = waitTat + runTat;
                this.CumTat = cumTat;
                this.BcumTat = 0;
                this.TotalTat = 0;
            }

            public void SetTotalTat(double totalTat)
            {
                this.TotalTat = totalTat;
                this.BcumTat = totalTat - this.CumTat;
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

        public class ProcStepInfo
        {
            //string FactoryID { public get; private set; }
            public string ShopID { get; private set; }
            public string ProcID { get; private set; }
            public string StepID { get; private set; }
            public int StepSeq { get; private set; }
            //public string StepDesc { get; private set; }
            //public string StepType { get; private set; }
            //public string Layer { get; private set; }
            public bool IsMandatory { get; private set; }

            public ProcStepInfo(string shopID, string procID, string stepID, int stepSeq, char isMandatory)
            {
                this.ShopID = shopID;
                this.ProcID = procID;
                this.StepID = stepID;
                this.StepSeq = stepSeq;
                this.IsMandatory = isMandatory == 'Y' ? true : false;
            }
        }

        public class ResultData
        {
            //string FactoryID { public get; private set; }
            public string ShopID { get; private set; }
            public string ProdID { get; private set; }
            public string ProcID { get; private set; }
            public string StepID { get; private set; }
            //public int StepSeq { get; private set; }
            public int StdStepSeq { get; private set; }
            public string StepDesc { get; private set; }
            //public bool IsMandatory { get; private set; }
            //public string StepType { get; private set; }
            public string Layer { get; private set; }
            public DateTime TargetDate { get; private set; }
            public int RunQty { get; private set; }
            public int WaitQty { get; private set; }
            public double ProgressRateRun { get; private set; }
            public int MaxProgRateRunInRange { get; private set; }
            public string RangeOfRun { get; private set; }
            public double ProgressRateWait { get; private set; }
            public int MaxProgRateWaitInRange { get; private set; }
            public string RangeOfWait { get; private set; }
            //public bool IsRun { get; private set; }
            public double WaitTat { get; private set; }
            public double RunTat { get; private set; }
            public double Tat { get; private set; }
            public double CumTat { get; private set; }
            public double BcumTat { get; private set; }
            public double TotalTat { get; private set; }
            
            public ResultData(string shopID, string prodID, string procID, string stepID,StepInfo stepInfo,// bool isMandatory
                DateTime targetDate)//, bool isRun)
            {
                this.ShopID = shopID;
                this.ProdID = prodID;
                this.ProcID = procID;
                this.StepID = stepID;
                //this.StepSeq = stepSeq;
                this.StdStepSeq = stepInfo == null ? int.MaxValue : stepInfo.StepSeq;
                this.StepDesc = stepInfo == null ? Consts.NULL_ID : stepInfo.StepDesc;
                //this.IsMandatory = isMandatory;
                this.Layer = stepInfo == null ? Consts.NULL_ID : stepInfo.Layer;
                this.TargetDate = targetDate;
                this.WaitQty = 0;
                this.RunQty = 0;
                this.ProgressRateRun = 0;
                this.MaxProgRateRunInRange = 0;
                this.RangeOfRun = Consts.NULL_ID;
                this.ProgressRateWait = 0;
                this.MaxProgRateWaitInRange = 0;
                this.RangeOfWait = Consts.NULL_ID;
                //this.IsRun = isRun;

                this.WaitTat = 0;
                this.RunTat = 0;
                this.Tat = 0;
                this.CumTat = 0;
                this.BcumTat = 0;
                this.TotalTat = 0;
            }
            
            public void AddQty(int waitQty, int runQty)
            {
                this.WaitQty += waitQty;
                this.RunQty += runQty;
            }

            public void SetTatAndProgRate(ProcStepTatInfo tatInfo, int selectedSectorSize)
            {
                this.WaitTat = tatInfo == null ? 0 : tatInfo.WaitTat;
                this.RunTat = tatInfo == null ? 0 : tatInfo.RunTat;
                this.Tat = tatInfo == null ? 0 : tatInfo.WaitTat + tatInfo.RunTat;
                this.CumTat = tatInfo == null ? 0 : tatInfo.CumTat;
                this.BcumTat = tatInfo == null ? 0 : tatInfo.BcumTat;
                this.TotalTat = tatInfo == null ? 0 : tatInfo.TotalTat;

                if (tatInfo != null && tatInfo.TotalTat > 0)
                {
                    this.ProgressRateRun = tatInfo.CumTat / tatInfo.TotalTat * 100.0;
                    this.ProgressRateWait = (tatInfo.CumTat - tatInfo.RunTat) / tatInfo.TotalTat * 100.0;

                    int maxProgRateRunInRange = (int)Math.Ceiling(this.ProgressRateRun / selectedSectorSize) * selectedSectorSize;
                    int maxProgRateWaitInRange = (int)Math.Ceiling(this.ProgressRateWait / selectedSectorSize) * selectedSectorSize;

                    this.MaxProgRateRunInRange = maxProgRateRunInRange;
                    this.MaxProgRateWaitInRange = maxProgRateWaitInRange;

                    this.RangeOfRun = (maxProgRateRunInRange - selectedSectorSize).ToString() + " ~ " + maxProgRateRunInRange.ToString();
                    this.RangeOfWait = (maxProgRateWaitInRange - selectedSectorSize).ToString() + " ~ " + maxProgRateWaitInRange.ToString();
                }
            }

            //public void SetProgressRate(double progressRate)
            //{
            //    this.ProgressRate = progressRate;
            //}
        }

        public class ResultChartData
        {
            public string Range { get; private set; }
            public string TargetDate { get; private set; }
            public int RunQty { get; private set; }
            public int WaitQty { get; private set; }
            public int Qty { get; private set; }

            public ResultChartData(string range, string targetDate)
            {
                this.Range = range;
                this.TargetDate = targetDate;
                this.WaitQty = 0;//+= waitQty;
                this.RunQty = 0;//+= runQty;
                this.Qty = 0;//+= waitQty + runQty;
            }

            public void AddQty(bool isRun, int qty)
            {
                if (isRun)
                    this.RunQty += qty;
                else
                    this.WaitQty += qty;

                this.Qty += qty;

            }
        }
    }
}
