using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Collections;
using Mozart.Studio.TaskModel.Projects;
using System.Data;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.Lcd.Scheduling;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Gantts
{
    public class EqpMaster
    {
        public DataTable Table { get; private set; }

        //Dictionary<EqpID, Eqp>
        public Dictionary<string, Eqp> EqpAll { get; set; }

        //DoubleDictionary<lineID, EqpID, Eqp>      
        private DoubleDictionary<string, string, Eqp> Eqps { get; set; }

        //Dictionary<AreaID+EqpGroup, int>
        private Dictionary<string, int> DspGroupSeqs { get; set; }  
        
        public EqpMaster()
        {
            this.Eqps = new DoubleDictionary<string, string, Eqp>();
            this.EqpAll = new Dictionary<string, Eqp>();
            this.DspGroupSeqs = new Dictionary<string, int>();
        }

        internal void LoadEqp(IExperimentResultItem result)
        {
            this.Table = result.LoadInput(EqpGanttChartData.EQP_TABLE_NAME);

            var dt = this.Table;
            if (dt != null)
            {
                foreach (DataRow srow in dt.Rows)
                {
                    Eqp eqp = new Eqp(srow);                                        
                    this.AddEqps(eqp);
                }
            }

            SetDspEqpGroupSeq(result);
        }

        internal void SetDspEqpGroupSeq(IExperimentResultItem result)
        {
            var modelContext = result.GetCtx<ModelDataContext>();
            var table = modelContext.StdStep;            
            if (table == null || table.Count() == 0)
                return;

            foreach (var eqp in this.EqpAll.Values)
            {
                string dspEqpGroup = eqp.DspEqpGroup;
                if (string.IsNullOrEmpty(dspEqpGroup))
                    continue;
                
                string areaID = GetAreaID(eqp.ShopID);

                var finds = table.Where(t => t.AREA_ID == areaID && t.DSP_EQP_GROUP_ID == dspEqpGroup).OrderBy(t => t.STEP_SEQ);
                var find = finds.FirstOrDefault();

                if (find == null)
                   continue;

                eqp.DspEqpGroupSeq = find.STEP_SEQ;

                string key = CommonHelper.CreateKey(areaID, eqp.EqpGroup);
                this.DspGroupSeqs[key] = find.STEP_SEQ;
            }
        }

        internal void AddEqps(Eqp eqp)
        {
            this.Eqps.TryAdd(eqp.ShopID, eqp.EqpID, eqp);

            if (this.EqpAll.ContainsKey(eqp.EqpID) == false)
                this.EqpAll.Add(eqp.EqpID, eqp);
        }

        internal Eqp FindEqp(string eqpId)
        {
            Eqp eqp;
            if (this.EqpAll.TryGetValue(eqpId, out eqp))
                return eqp;

            return null;
        }

        internal Eqp FindEqp(string lineid, string eqpId)
        {
            Eqp eqp;
            if (this.Eqps.TryGetValue(lineid, eqpId, out eqp) == false)
                return null;

            return eqp;
        }

        internal Dictionary<string, Eqp> GetEqpsByLine(string selectedShopID = null)
        {
            Dictionary<string, Eqp> dic;
            if (string.IsNullOrEmpty(selectedShopID) || selectedShopID == "ALL")
                return this.EqpAll;

            if (this.Eqps.TryGetValue(selectedShopID, out dic))
                return dic;

            return null;
        }

        public int GetDspGroupSeq(string shopID, string eqpGroupID)
        {
            string areaID = GetAreaID(shopID);

            string key = CommonHelper.CreateKey(areaID, eqpGroupID);
            if (string.IsNullOrEmpty(key) == false)
            {
                int seq;
                if (this.DspGroupSeqs.TryGetValue(key, out seq))
                    return seq;
            }

            return 999;
        }

        public int GetEqpSeq(string eqpID)
        {
            var eqp = FindEqp(eqpID);
            if (eqp != null)
                return eqp.GetEqpSeq();

            return 999;
        }

        private string GetAreaID(string shopID)
        {
            string areaID = shopID;
            if (CommonHelper.Equals(shopID, "ARRAY"))
                areaID = "TFT";

            return areaID;
        }

        public class Eqp
        {
            public string ShopID { get; set; }
            public string EqpGroup { get; set; }
            public string DspEqpGroup { get; set; }
            public string EqpID { get; set; }
            public string EqpType { get; set; }
            public string MaxBatchSize { get; set; }
            public string MinBatchSize { get; set; }
            public int ViewSeq { get; set; }

            public int DspEqpGroupSeq { get; set; }            

            public bool IsLotBatch
            {
                get
                {
                    if (string.Equals(this.EqpType, "Lot_Batch", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(this.EqpType, "Batch_Inline", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(this.EqpType, "SCRIBE", StringComparison.CurrentCultureIgnoreCase))
                        return true;

                    return false;
                }
            }

            public Eqp(string shopID, string eqpID)
            {
                this.ShopID = shopID;
                this.EqpID = eqpID;
                this.EqpGroup = string.Empty;
                
                this.DspEqpGroupSeq = 999;
                this.ViewSeq = 999;
            }

            public Eqp(DataRow srow)
            {
                EqpGanttChartData.Eqp eqp = new EqpGanttChartData.Eqp(srow);

                this.ShopID = eqp.ShopID;
                this.EqpID = eqp.EqpID;
                this.EqpGroup = eqp.EqpGroup;
                this.DspEqpGroup = eqp.DspEqpGroup;
                this.EqpType = eqp.SimType;
                this.MaxBatchSize = eqp.MaxBatchSize.ToString();
                this.MinBatchSize = eqp.MinBatchSize.ToString();

                this.DspEqpGroupSeq = 999;
                this.ViewSeq = eqp.ViewSeq;
            }

            public int GetEqpSeq()
            {                
                int groupSeq = this.DspEqpGroupSeq * 1000;
                int viewSeq = this.ViewSeq;

                return groupSeq + viewSeq;
            }

            public static Eqp CreateDummy(string shopID, string eqpID)
            {
                return new Eqp(shopID, eqpID);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();                
                sb.AppendFormat("{0} : {1}\r\n", EqpGanttChartData.Eqp.Schema.EQP_ID, this.EqpID);
                sb.AppendFormat("{0} : {1}\r\n", EqpGanttChartData.Eqp.Schema.EQP_GROUP_ID, this.EqpGroup);
                sb.AppendFormat("{0} : {1}\r\n", EqpGanttChartData.Eqp.Schema.SIM_TYPE, this.EqpType);
                //sb.AppendFormat("{0} : {1}\r\n", EqpGanttChartData.Eqp.Schema.MIN_BATCH_SIZE, string.IsNullOrEmpty(this.MinBatchSize) ? "-" : this.MinBatchSize);
                //sb.AppendFormat("{0} : {1}\r\n", EqpGanttChartData.Eqp.Schema.MAX_BATCH_SIZE, string.IsNullOrEmpty(this.MaxBatchSize) ? "-" : this.MaxBatchSize);

                return sb.ToString();
            }
        }
    }
}
