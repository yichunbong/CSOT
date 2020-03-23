using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSOT.Lcd.UserInterface.LotGantts
{
    public partial class DetailBarInfoDialog : Form
    {
        public DetailBarInfoDialog()
        {
            InitializeComponent();
        }

        public void SetBarInfo(GanttBar bar)
        {
            var sb = new StringBuilder();

            //sb.AppendLine("+ -----------------------------------------");
            sb.AppendLine(string.Format("LotID\t : {0}", bar.LotID));
            sb.AppendLine(string.Format("T/I Qty\t : {0}", bar.TIQty));
            sb.AppendLine(string.Format("T/O Qty\t : {0}", bar.TOQty));

            sb.AppendLine(string.Format("Process\t : {0}", bar.ProcessID));
            sb.AppendLine(string.Format("Step\t : {0}", bar.StepSeq));

            sb.AppendLine(string.Format("Product\t : {0}", bar.ProductID));

            sb.AppendLine();
            sb.AppendLine(string.Format("{0} -> {1}", bar.TkinTime, bar.TkoutTime));
            sb.AppendLine(string.Format("Gap\t : {0}", bar.TkoutTime - bar.TkinTime));

            memoEdit1.Text = sb.ToString();
        }
    }
}
