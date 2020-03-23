using Mozart.Studio.TaskModel.UserInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSOT.Lcd.UserInterface.Common
{
    public partial class PopUpForm : Form
    {
        public PopUpForm(XtraUserControlView view)
        {
            InitializeComponent();

            view.Dock = DockStyle.Fill;

            this.panel1.Controls.Clear();
            this.panel1.Controls.Add(view);
        }
    }
}
