using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSOT.UserInterface.UIComponentFactory
{
    public partial class ExportProgressDialog : DevExpress.XtraEditors.XtraForm
    {
        public ExportProgressDialog(Form parent)
        {
            InitializeComponent();
            this.ClientSize = new Size(
                pictureBox1.Image.Width + this.DockPadding.All * 2,
                pictureBox1.Image.Height + this.DockPadding.All * 2);
            if (parent != null)
            {
                Left = parent.Left + (parent.Width - Width) / 2;
                Top = parent.Top + (parent.Height - Height) / 2;
            }
        }
    }
}
