using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class SimpleGridPopUp : Form
    {
        public SimpleGridPopUp(string formName, DataTable dt)
        {
            InitializeComponent();

            this.Text = formName;

            FillGrid(dt);
        }
        
        private void FillGrid(DataTable dt)
        {
            this.gridControl1.BeginUpdate();

            this.gridView1.BestFitColumns();

            this.gridControl1.DataSource = dt;

            this.gridControl1.EndUpdate();
        }

        public void SetFormSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public void SetFooter(bool show)
        {
            this.gridView1.OptionsView.ShowFooter = show;
        }

        public void SetColumnDisplayFormat(string colName, DevExpress.Utils.FormatType formatType, string formatString)
        {
            this.gridView1.Columns[colName].DisplayFormat.FormatType = formatType;
            this.gridView1.Columns[colName].DisplayFormat.FormatString = formatString;
            //this.gridView1.Columns[colName].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            //column.DisplayFormat.FormatString = "{0:##0.0%}"; // "###,###,###.##"
        }
    }
}
