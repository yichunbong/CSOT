using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class TcData
    {
        public class Schema
        {
            public const string SHOP_ID = "SHOP_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string MAIN_PROC_ID = "MAIN_PROC_ID";
            public const string STEP_ID = "STEP_ID";
            public const string STEP_DESC = "STEP_DESC";
            public const string STD_STEP_SEQ = "STD_STEP_SEQ";
            public const string LAYER_ID = "LAYER_ID";
            public const string ACT_RUN_TAT = "ACT_RUN_TAT";
            public const string ACT_WAIT_TAT = "ACT_WAIT_TAT";
            public const string ACT_TAT = "ACT_TAT";
            public const string SIM_RUN_TAT = "SIM_RUN_TAT";
            public const string SIM_WAIT_TAT = "SIM_WAIT_TAT";
            public const string SIM_TAT = "SIM_TAT";
            public const string TAT_DIFF = "TAT_DIFF";

            public const string DIFF = "DIFF";
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
        
        public class SimTatInfo
        {
            public string ShopID { get; private set; }
            public DateTime TargetShift { get; private set; }
            public string ProductID { get; private set; }
            public string MainProcessID { get; private set; }
            public string StepID { get; private set; }
            public List<double> RunTatList { get; private set; }
            public List<double> WaitTatList { get; private set; }
            
            public double RunTatAvg
            {
                get
                {
                    double avg = 0;

                    if (this.RunTatList.Count <= 0)
                        return avg;

                    avg = this.RunTatList.Average();

                    return avg;
                }
            }

            public double WaitTatAvg
            {
                get
                {
                    double avg = 0;

                    if (this.WaitTatList.Count <= 0)
                        return avg;

                    avg = this.WaitTatList.Average();

                    return avg;
                }
            }

            public double TatAvg
            {
                get
                {
                    return this.RunTatAvg + this.WaitTatAvg;
                }
            }

            public SimTatInfo(string shopID, DateTime targetShift, string prodID, string mainProcID, string stepID)
            {
                this.ShopID = shopID;
                this.TargetShift = targetShift;
                this.ProductID = prodID;
                this.MainProcessID = mainProcID;
                this.StepID = stepID;

                this.RunTatList = new List<double>();
                this.WaitTatList = new List<double>();
            }

            public void AddRunTatToList(double runTat)
            {
                this.RunTatList.Add(runTat);
            }

            public void AddRunTatToList(List<double> runTatList)
            {
                this.RunTatList.AddRange(runTatList);
            }

            public void AddWaitTatToList(double waitTat)
            {
                this.WaitTatList.Add(waitTat);
            }

            public void AddWaitTatToList(List<double> waitTatList)
            {
                this.WaitTatList.AddRange(waitTatList);
            }
        }


        public class StepTatRsltInf
        {
            //string FactoryID { public get; private set; }
            public string ShopID { get; private set; }
            public string ProdID { get; private set; }
            public string MainProcID { get; private set; }
            public string StepID { get; private set; }
            //public int StepSeq { get; private set; }
            public int StdStepSeq { get; private set; }
            public string StepDesc { get; private set; }
            //public bool IsMandatory { get; private set; }
            //public string StepType { get; private set; }
            public string Layer { get; private set; }

            public double WaitTatAct { get; private set; }
            public double RunTatAct { get; private set; }
            public double TatAct { get; private set; }

            public double WaitTatSim { get; private set; }
            public double RunTatSim { get; private set; }
            public double TatSim { get; private set; }

            public StepTatRsltInf(string shopID, string prodID, string mainProcID, string stepID, StepInfo stepInfo)
            {
                this.ShopID = shopID;
                this.ProdID = prodID;
                this.MainProcID = mainProcID;
                this.StepID = stepID;
                //this.StepSeq = stepSeq;
                this.StdStepSeq = stepInfo == null ? int.MaxValue : stepInfo.StepSeq;
                this.StepDesc = stepInfo == null ? Consts.NULL_ID : stepInfo.StepDesc;
                //this.IsMandatory = isMandatory;
                this.Layer = stepInfo == null ? Consts.NULL_ID : stepInfo.Layer;

                this.WaitTatAct = 0;
                this.RunTatAct = 0;
                this.TatAct = 0;

                this.WaitTatSim = 0;
                this.RunTatSim = 0;
                this.TatSim = 0;
            }

            public void SetActualTat(double waitTAT, double runTAT)
            {
                this.WaitTatAct = waitTAT;
                this.RunTatAct = runTAT;
                this.TatAct = waitTAT + runTAT;
            }

            public void SetSimulationTat(double waitTAT, double runTAT)
            {
                this.WaitTatSim = waitTAT;
                this.RunTatSim = runTAT;
                this.TatSim = waitTAT + runTAT;
            }
        }

        public class TotalTatRsltInf
        {
            //string FactoryID { public get; private set; }
            public string ShopID { get; private set; }
            public string ProdID { get; private set; }
            public string MainProcID { get; private set; }

            //public double WaitTatAct { get; private set; }
            //public double RunTatAct { get; private set; }
            public double TotalTatAct { get; private set; }
            public double TotalTatSim { get; private set; }

            public TotalTatRsltInf(string shopID, string prodID, string mainProcID)
            {
                this.ShopID = shopID;
                this.ProdID = prodID;
                this.MainProcID = mainProcID;

                this.TotalTatAct = 0;
                this.TotalTatSim = 0;
            }

            public void SetActTat(double totalTAT)
            {
                this.TotalTatAct = totalTAT;
            }

            public void AddSimTat(double simStepTAT)
            {
                this.TotalTatSim += simStepTAT;
            }
        }
    }
}
