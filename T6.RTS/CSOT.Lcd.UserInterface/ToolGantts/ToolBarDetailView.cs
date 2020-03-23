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

namespace CSOT.Lcd.UserInterface.ToolGantts
{
    public partial class ToolBarDetailView : XtraUserControlView
    {
        ToolBar _bar;

        public ToolBarDetailView()
        {
            InitializeComponent();
        }

        public void SetBarInfo(ToolBar bar)
        {
            this._bar = bar;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("ToolID :    \t {0}", bar.ToolID));
            sb.AppendLine(string.Format("EqpID :    \t {0}", bar.EqpId));
            sb.AppendLine(string.Format("ProductID :    \t {0}", bar.ProductId));
            sb.AppendLine(string.Format("Layer :        \t {0}", bar.Layer));

            string state = bar.State == EqpState.SETUP ? "MOVE" : bar.State.ToString();
            sb.AppendLine(string.Format("State :      \t {0}", state));
            sb.AppendLine(string.Format("Qty :        \t {0}", bar.TIQty));
            sb.AppendLine(string.Format("Step :       \t {0}", bar.StepId)); 

            sb.AppendLine();
            sb.AppendLine(string.Format("{0} -> {1}", bar.TkinTime, bar.TkoutTime));
            sb.AppendLine(string.Format("Gap :           \t {0}", bar.TkoutTime - bar.TkinTime));

            memoEdit1.Text = sb.ToString();
        }

        private bool TryGetWipLot(DataTable dtWipLot, string lotId,
            out string partNo, out string mcpSeq, out string stepSeq, out string lotType, out string tcsID)
        {
            partNo = string.Empty;
            mcpSeq = string.Empty;
            stepSeq = string.Empty;
            lotType = string.Empty;
            tcsID = string.Empty;

            int idx = lotId.IndexOf("_");
            if (idx > 0)
                lotId = lotId.Substring(0, idx);

            DataRow[] rows = dtWipLot.Select(string.Format("LOTID = '{0}'", lotId));
            if (rows.Count() == 0)
                return false;

            partNo = rows[0]["PARTNUMBER"].ToString();
            mcpSeq = rows[0]["MCPSEQ"].ToString();
            stepSeq = rows[0]["STEPSEQ"].ToString();
            lotType = rows[0]["LOTTYPE"].ToString();

            return true;
        }

        private void DetailBarInfoDialog_Shown(object sender, EventArgs e)
        {
            memoEdit1.SelectionStart = memoEdit1.Text.Length;
        }
    }
}
