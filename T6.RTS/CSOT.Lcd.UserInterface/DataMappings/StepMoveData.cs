using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    class StepMoveData
    {
        //#region Input Table Name

        //public const string DATA_TABLE_1 = "StepMove";

        //#endregion



        //#region Input Data Transform

        //public class StepMove
        //{
        //    public string ShopID;
        //    public string ProductID;
        //    public string ProcessID;
        //    public string StepID;
        //    public string StdStepID;
        //    public int StdStepSeq;
        //    public string EqpID;
        //    public DateTime TargetDate;
        //    public float InQty;
        //    public float OutQty;

        //    public StepMove(DataRow row)
        //    {
        //        this.ShopID = string.Empty;
        //        this.ProductID = string.Empty;
        //        this.ProcessID = string.Empty;
        //        this.StepID = string.Empty;
        //        this.StdStepID = string.Empty;
        //        this.StdStepSeq = 0;
        //        this.EqpID = string.Empty;
        //        this.TargetDate = DateTime.MinValue;
        //        this.InQty = 0.0f;
        //        this.OutQty = 0.0f;

        //        ParseRow(row);
        //    }

        //    ///
        //    /// 개발 포인트 
        //    ///
        //    private void ParseRow(DataRow row)
        //    {
        //        // LINE_ID
        //        ShopID = row.GetString("SHOP_ID");

        //        // PRODUCT_ID
        //        ProductID = row.GetString("PRODUCT_ID");

        //        // PROCESS_ID
        //        ProcessID = row.GetString("PROCESS_ID");

        //        // STEP_ID
        //        StepID = row.GetString("STEP_ID");

        //        // STD_STEP_ID
        //        StdStepID = row.GetString("STD_STEP_ID");

        //        // STD_STEP_SEQ
        //        StdStepSeq = row.GetInt32("STD_STEP_SEQ");

        //        // EQP_ID
        //        EqpID = row.GetString("EQP_ID");

        //        // TARGET_DATE
        //        TargetDate = row.GetDateTime("PLAN_DATE");

        //        // WAIT_QTY
        //        InQty = row.GetFloat("IN_QTY");

        //        // RUN_QTY
        //        OutQty = row.GetFloat("OUT_QTY");
        //    }
        //}


        //#endregion



        #region UI 화면 Caption

        public const string TITLE = "StepMoveView";
        public const string CHART_TITLE = "";
        public const string TFT = "TFT";

        // 메인 UI
        public const string SHOP_ID = "SHOP_ID";
        public const string PRODUCT_ID = "PRODUCT_ID";
        public const string OWNER_TYPE = "OWNER_TYPE";
        public const string PRODUCT_VERSION = "PRODUCT_VERSION";
        //public const string PROCESS_ID = "PROCESS_ID";
        public const string STD_STEP = "STEP";
        public const string EQP_ID = "EQP_ID";
        public const string EQP_GROUP_ID = "EQP_GROUP_ID";
        public const string TARGET_DATE = "TARGET_DATE";
        public const string IN_QTY = "IN_QTY";
        public const string OUT_QTY = "OUT_QTY";

        #endregion
    }
}
