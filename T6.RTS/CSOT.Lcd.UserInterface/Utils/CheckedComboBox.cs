using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSOT.Lcd.UserInterface.Utils
{
    public partial class CheckedComboBox : UserControl
    {
        bool _first = true;
        public CheckedComboBox()
        {
            InitializeComponent();
            this.Height = comboBox.Height;
            checkedListBox.Items.ListChanged += new ListChangedEventHandler(Items_ListChanged);
        }

        void Items_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (_first == false)
                comboBox.Refresh();
            _first = false;
        }

        public DevExpress.XtraEditors.Controls.CheckedListBoxItemCollection Items
        {
            get { return checkedListBox.Items; }
        }

        public DevExpress.XtraEditors.BaseCheckedListBoxControl.CheckedItemCollection CheckedItems
        {
            get { return checkedListBox.CheckedItems; }
        }

        public DevExpress.XtraEditors.CheckedListBoxControl ListBox
        {
            get { return checkedListBox; }
        }

        private void comboBox_QueryDisplayText(object sender, DevExpress.XtraEditors.Controls.QueryDisplayTextEventArgs e)
        {
            if (checkedListBox.CheckedItems.Count == 0)
                e.DisplayText = " (Select)";
            else if (checkedListBox.CheckedItems.Count == 1)
                e.DisplayText = " " + checkedListBox.CheckedItems[0].ToString();
            else
                e.DisplayText = " " + checkedListBox.CheckedItems[0].ToString() + ", " + checkedListBox.CheckedItems[1].ToString() + " ...";
        }

        public void ShowPopup()
        {
            comboBox.ShowPopup();
        }

        internal void FindStringExact(string prcGroup)
        {
            throw new NotImplementedException();
        }
    }

}
