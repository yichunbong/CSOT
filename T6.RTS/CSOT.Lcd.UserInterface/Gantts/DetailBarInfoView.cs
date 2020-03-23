using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSOT.Lcd.UserInterface.DataMappings;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.UserInterface.Common;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.Application;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Analysis;

namespace CSOT.Lcd.UserInterface.Gantts
{
    public partial class DetailBarInfoView : XtraUserControlView
    {
        private const string _pageID = "DetailBarInfoView";

        private IExperimentResultItem _result;
        private GanttBar _bar;
        private List<EqpGanttChartData.PresetInfo> _presetList;               

        public DetailBarInfoView()
        {
            InitializeComponent();
        }

        public DetailBarInfoView(IServiceProvider serviceProvider, List<EqpGanttChartData.PresetInfo> presetList, IExperimentResultItem result)
            : base(serviceProvider)
        {
            InitializeComponent();

            _presetList = presetList;

            _result = result;
        }
       
        protected override void LoadDocument()
        {
            if (this.Document == null)
                return;
        }

        public void SetBarInfo(GanttBar bar)
        {
            this._bar = bar;            

            StringBuilder sb = new StringBuilder();

            if (bar != null)
            {
                //sb.AppendLine("+ -----------------------------------------");     
                string stateInfo = StringHelper.IsEmptyID(bar.StateInfo) ? "" : string.Format(" [{0}]", bar.StateInfo);

                sb.AppendLine(string.Format("State : {0}{1}", bar.State.ToString(), stateInfo));
                sb.AppendLine(string.Format("EqpID : {0}", bar.EqpID));
                sb.AppendLine(string.Format("SubEqpID : {0}", bar.SubEqpID));
                sb.AppendLine(string.Format("Layer(Step) : {0}({1})", bar.Layer, bar.StepID));
                
                sb.AppendLine(string.Format("InQty : {0}", bar.TIQty));
                sb.AppendLine(string.Format("Start : {0}", bar.StartTime.ToString("yyyy-MM-dd HH:mm:ss")));
                sb.AppendLine(string.Format("End : {0}", bar.EndTime.ToString("yyyy-MM-dd HH:mm:ss")));
                sb.AppendLine(string.Format("Gap : {0}", bar.EndTime - bar.StartTime));
                                
                sb.AppendLine(string.Format("LotID : {0}", bar.OrigLotID));
                sb.AppendLine(string.Format("ProductID : {0}", bar.ProductID));
                sb.AppendLine(string.Format("ProductVersion : {0}", bar.ProductVersion));
                sb.AppendLine(string.Format("OwnerType : {0}", bar.OwnerType));

                sb.AppendLine(string.Format("ProcessID : {0}", bar.ProcessID));                
                sb.AppendLine(string.Format("ToolID : {0}", bar.ToolID));
                sb.AppendLine(string.Format("LotPriority : {0}", bar.LotPriority));
                sb.AppendLine(string.Format("LongTimeNoRun : {0}", bar.EqpRecipe ?? "N"));
            }

            memoEdit1.Text = sb.ToString();

            btnDispBar.Enabled = bar != null && bar.DispatchingInfo != null;
            btnDispEqp.Enabled = bar != null;
        }
        
        //private bool TryGetWipLot( DataTable dtWipLot , string lotId , 
        //    out string partNo, out string mcpSeq, out string stepSeq , out string lotType , out string tcsID )
        //{
        //    partNo = string.Empty;
        //    mcpSeq = string.Empty;
        //    stepSeq = string.Empty;
        //    lotType = string.Empty;
        //    tcsID = string.Empty;

        //    int idx = lotId.IndexOf("_") ;
        //    if ( idx > 0 )
        //        lotId = lotId.Substring( 0 , idx );

        //    DataRow[] rows = dtWipLot.Select(string.Format("LOTID = '{0}'", lotId ));
        //    if ( rows.Count() == 0 )
        //        return false;

        //    partNo = rows[0]["PARTNUMBER"].ToString();
        //    mcpSeq = rows[0]["MCPSEQ"].ToString();
        //    stepSeq = rows[0]["STEPSEQ"].ToString();
        //    lotType = rows[0]["LOTTYPE"].ToString();

        //    return true;
        //}
                
        private void DetailBarInfoDialog_Shown(object sender, EventArgs e)
        {
            memoEdit1.SelectionStart = memoEdit1.Text.Length;
        }

        private void btnDispBar_Click(object sender, EventArgs e)
        {
            DataRow info = _bar.DispatchingInfo;
            if (info == null)
                return;

            try
            {
                var dialog = new DispatchingInfoViewPopup(this.ServiceProvider, _result, info, _bar.EqpInfo, _presetList);
                dialog.Show();
            }
            catch { }
        }

        private void btnDispEqp_Click(object sender, EventArgs e)
        {
            if (_bar == null)
                return;

            try
            {
                DispatchingAnalysisView control = new DispatchingAnalysisView(this.ServiceProvider, _result);

                var dialog = new PopUpForm(control);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Show();

                control.Query(_bar.EqpInfo.ShopID, _bar.EqpGroup, _bar.EqpID, _bar.SubEqpID);
            }
            catch { }
        }
    }
}
