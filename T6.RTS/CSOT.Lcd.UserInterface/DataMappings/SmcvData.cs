using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class SmcvData
    {
        public class Schema
        {
            public const string SHOP_ID = "SHOP_ID";
            public const string TARGET_DATE = "TARGET_DATE";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string PRODUCT_VERSION = "PRODUCT_VERSION";
            public const string PROCESS_ID = "PROCESS_ID";
            public const string EQP_ID = "EQP_ID";
            public const string STEP_ID = "STEP_ID";
            public const string STEP_DESC = "STEP_DESC";
            public const string STEP_TYPE = "STEP_TYPE";
            public const string STEP_SEQ = "STEP_SEQ";
            public const string LAYER = "LAYER";
            public const string SIM_MOVE_QTY = "SIM_MOVE_QTY";
            public const string ACT_MOVE_QTY = "ACT_MOVE_QTY";
            public const string WIP_CUR = "WIP_CUR";
            public const string WIP_MAIN = "WIP_MAIN";
            public const string WIP_QTY = "WIP_QTY";
            public const string WIP_BOH = "WIP_BOH";
            public const string WIP_EOH = "WIP_EOH";
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
            public string StepDesc { get; private set; }
            public int StdStepSeq { get; private set; }
            public string NextMainStepID { get; private set; }
            public string NextMainStepDesc { get; private set; }
            public int NextMainStdStepSeq { get; private set; }

            public ProcStepInfo(string shopID, string procID, string stepID, string stepDesc, int stepSeq)
            {
                this.ShopID = shopID;
                this.ProcID = procID;
                this.StepID = stepID;
                this.StepDesc = stepDesc;
                this.StdStepSeq = stepSeq;
                this.NextMainStepID = Consts.NULL_ID;
                this.NextMainStepDesc = Consts.NULL_ID;
                this.NextMainStdStepSeq = int.MaxValue;
            }

            public void SetNextMainStepInf(string nextMainstepID, string nextMainStepDesc, int nextMainStepSeq)
            {
                this.NextMainStepID = nextMainstepID;
                this.NextMainStepDesc = nextMainStepDesc;
                this.NextMainStdStepSeq = nextMainStepSeq;
            }
        }


        public class MoveInfo
        {
            //string FactoryID { public get; private set; }
            public string ShopID { get; private set; }
            public DateTime TargetDate { get; private set; }
            public string ProductID { get; private set; }
            public string ProductVersion { get; private set; }
            public string StepID { get; private set; }
            public string StepDesc { get; private set; }
            public string StepType { get; private set; }
            public int StepSeq {  get; private set; }
            public string Layer { get; private set; }
            public int SimMoveQty { get; private set; }
            public int ActMoveQty { get; private set; }
            
            public MoveInfo(string shopID, DateTime targetDate, string productID, string productVersion, SmcvData.StepInfo stepInfo)
            {
                this.ShopID = shopID;
                this.TargetDate = targetDate;
                this.ProductID = productID;
                this.ProductVersion = "-";
                this.StepID = stepInfo.StepID;
                this.StepDesc = stepInfo.StepDesc;
                this.StepType = stepInfo.StepType;
                this.StepSeq = stepInfo.StepSeq;
                this.Layer = stepInfo.Layer;
                this.SimMoveQty = 0;
                this.ActMoveQty = 0;
            }

            public void AddSimMoveQty(int simMoveQty)
            {
                this.SimMoveQty += simMoveQty;
            }

            public void AddActMoveQty(int actMoveQty)
            {
                this.ActMoveQty += actMoveQty;
            }
        }


        public class Wip
        {
            public string ShopID { get; private set; }
            public string ProductID { get; private set; }
            public string StepID { get; private set; }
            public string StepDesc { get; private set; }
            public int GlassQty { get; private set; }
            public Dictionary<string, int> GlassQtyByVersion { get; private set; }
            
            public Wip(string shopID, string prodID, string stepID)
            {
                this.ShopID = shopID;
                this.ProductID = prodID;
                this.StepID = stepID;
                this.StepDesc = Consts.NULL_ID;
                this.GlassQty = 0;
                this.GlassQtyByVersion = new Dictionary<string, int>();
            }

            public void SetStepDesc(string stepDesc)
            {
                this.StepDesc = stepDesc;
            }

            public void AddGlassQty(string productVersion, int glassQty)
            {
                this.GlassQty += glassQty;

                int preQty = 0;
                if (this.GlassQtyByVersion.TryGetValue(productVersion, out preQty) == false)
                    this.GlassQtyByVersion.Add(productVersion, glassQty);
                else
                    this.GlassQtyByVersion[productVersion] += glassQty;
            }
        }

        public class WipDetail
        {
            public string ShopID { get; private set; }
            public string ProductID { get; private set; }
            public string ProductVersion { get; private set; }
            public string OwnerType { get; private set; }
            public string StepID { get; private set; }
            public string StepDesc { get; private set; }
            public int StepSeq { get; private set; }
            public string Layer { get; private set; }
            public int GlassQty { get; private set; }

            public WipDetail(string shopID, string prodID, string prodVer, string ownerType, string stepID, int stepSeq, string stepDesc, string layer)
            {
                this.ShopID = shopID;
                this.ProductID = prodID;
                this.ProductVersion = prodVer;
                this.OwnerType = ownerType;
                this.StepID = stepID;
                this.Layer = layer;
                this.StepSeq = stepSeq;
                this.StepDesc = stepDesc;
                this.GlassQty = 0;
            }
            
            public void AddGlassQty(int glassQty)
            {
                this.GlassQty += glassQty;
            }
        }


        #region Internal Class : ResultItem
        internal class ResultItem
        {
            public string ShopID { get; private set; }
            public string ProductID { get; private set; }
            public string ProductVersion { get; private set; }
            public string OwnerType { get; private set; }
            public string StepID { get; private set; }
            public int StepSeq { get; private set; }
            public string StepDesc { get; private set; }
            public string Layer { get; private set; }
            public string EqpID { get; private set; }

            public DateTime TargetDate { get; private set; }

            public int SimInQty { get; private set; }
            public int OutQty { get; private set; }
            public int ActInQty { get; private set; }
            public int ActOutQty { get; private set; }

            public int WipCurStepQty { get; private set; }
            public int WipMainStepQty { get; private set; }

            public ResultItem(string shopID, string productID, string productVersion, string ownerType, string stepID, string eqpID, DateTime targetDate)
            {
                this.ShopID = shopID;
                this.ProductID = productID;
                this.ProductVersion = productVersion;
                this.OwnerType = ownerType;
                this.StepID = stepID;
                this.StepSeq = int.MaxValue;
                this.StepDesc = Consts.NULL_ID;
                this.Layer = Consts.NULL_ID;
                
                this.EqpID = eqpID;

                this.TargetDate = targetDate;

                this.SimInQty = 0;
                this.OutQty = 0;
                this.ActInQty = 0;
                this.ActOutQty = 0;

                this.WipCurStepQty = 0;
                this.WipMainStepQty = 0;
            }

            public void SetStepInfo(StepInfo stepInfo)
            {
                if (stepInfo == null)
                    return;

                this.StepSeq =  stepInfo.StepSeq;
                this.StepDesc = stepInfo.StepDesc;
                this.Layer = stepInfo.Layer;
            }

            public void UpdateWipQty(int curWip, int mainWip)
            {
                this.WipCurStepQty += curWip;
                this.WipMainStepQty += mainWip;
            }

            public void UpdateSimQty(int inQty, int outQty)
            {
                this.SimInQty += inQty;
                this.OutQty += outQty;
            }

            public void UpdateActQty(int inQty, int outQty)
            {
                this.ActInQty += inQty;
                this.ActOutQty += outQty;
            }
        }
        #endregion
    }
}
