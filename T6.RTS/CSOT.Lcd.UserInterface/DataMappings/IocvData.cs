using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class IocvData
    {
        public class Consts
        {
            public const string InFlag = "IN";
            public const string OutFlag = "OUT";
            public const string ACT = "ACT";
            public const string PLAN = "PLAN";
            public const string SIM = "SIM";
        }

        public class Schema
        {
            public const string SHOP_ID = "SHOP_ID";
            public const string IN_OUT_FLAG = "IN_OUT_FLAG";
            public const string TARGET_DATE = "TARGET_DATE";
            public const string PROD_ID = "PROD_ID";
            public const string PROD_VER = "PROD_VER";
            public const string DEPT = "DEPT";
            public const string QTY = "QTY";
        }

        public class ResultPivot
        {
            public string SHOP_ID {get; private set; }
            public string IN_OUT_FLAG { get; private set; }
            public string TARGET_DATE { get; private set; }
            public DateTime TARGET_DATE_ORG {get; private set; }
            public string PROD_ID {get; private set; }
            public string PROD_VER { get; private set; }
            public string DEPT {get; private set; }
            public double QTY { get; private set; }

            public ResultPivot(string shopID, string inOutFlag, string targetDate, DateTime targetDateOrg, string prodID, string prodVer, string dept)
            {
                this.SHOP_ID = shopID;
                this.IN_OUT_FLAG = inOutFlag;
                this.TARGET_DATE = targetDate;
                this.TARGET_DATE_ORG = targetDateOrg;
                this.PROD_ID = prodID;
                this.PROD_VER = prodVer;
                this.DEPT = dept;
                this.QTY = 0;
            }

            public void AddQty(double qty)
            {
                this.QTY += qty;
            }
        }

        public class ResultChart
        {
            public string SHOP_ID { get; private set; }
            public string TARGET_DATE { get; private set; }
            public DateTime TARGET_DATE_ORG { get; private set; }
            public string IN_OUT_FLAG { get; private set; }
            public double ACT_QTY { get; private set; }
            public double PLAN_QTY { get; private set; }
            public double SIM_QTY { get; private set; }

            public string KEY_CHART { get { return this.TARGET_DATE + "_" + this.IN_OUT_FLAG; } }
            
            public ResultChart(string shopID, string targetDate, DateTime targetDateOrg, string inOutFlag)
            {
                this.SHOP_ID = shopID;
                this.TARGET_DATE = targetDate;
                this.TARGET_DATE_ORG = targetDateOrg;
                this.IN_OUT_FLAG = inOutFlag;
                this.ACT_QTY = 0;
                this.PLAN_QTY = 0;
                this.SIM_QTY = 0;
            }

            public void AddQty(double qty, string kindOfQty)
            {
                if (kindOfQty == IocvData.Consts.ACT)
                    this.ACT_QTY += qty;
                else if (kindOfQty == IocvData.Consts.PLAN)
                    this.PLAN_QTY += qty;
                else if (kindOfQty == IocvData.Consts.SIM)
                    this.SIM_QTY += qty;
            }
        }
    }
}
