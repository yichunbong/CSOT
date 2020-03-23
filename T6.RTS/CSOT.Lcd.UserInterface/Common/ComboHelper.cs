using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using System.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Common
{
    class ComboHelper
    {
        public static readonly string ALL = "ALL";

        internal static void SetComboEdit(ComboBoxEdit control, ICollection<string> list, bool includeALL = false, TextEditStyles style = TextEditStyles.Standard)
        {
            if (control == null)
                return;

            control.Properties.Items.Clear();

            if (includeALL)
                control.Properties.Items.Add(ALL);

            foreach (string item in list)
                control.Properties.Items.Add(item);

            if (control.Properties.Items.Count > 0)
                control.SelectedIndex = 0;

            control.Properties.TextEditStyle = style;
        }

        internal static void SetCheckedComboEdit(CheckedComboBoxEdit control, ICollection<string> list, bool includeALL = false, TextEditStyles style = TextEditStyles.Standard)
        {
            if (control == null)
                return;

            control.Properties.Items.Clear();

            if (includeALL)
                control.Properties.Items.Add(ALL);

            foreach (string item in list)
                control.Properties.Items.Add(item);

            if (control.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in control.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }
        }

        public static void AddDataToComboBox(ComboBoxEdit control, IExperimentResultItem result, string tableName, string colName, bool addALL = true)
        {
            var dtable = result.LoadInput(tableName, null);

            var datas = Distinct(dtable, colName, "");
            
            foreach (string data in datas)
            {
                if (control.Properties.Items.Contains(data) == false)
                    control.Properties.Items.Add(data);
            }

            if (addALL)
                control.Properties.Items.Insert(0, Consts.ALL);

            if (control.Properties.Items.Count > 0)
                control.SelectedIndex = 0;

            control.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        public static void ShiftName(ComboBoxEdit control, DateTime planStartTime)
        {
            control.Properties.Items.Clear();
            foreach (string name in ShopCalendar.ShiftNames)
            {
                control.Properties.Items.Add(name);
            }

            if (control.Properties.Items.Count > 2)
            {
                if (planStartTime.Hour >= 6 && planStartTime.Hour < 14)
                    control.SelectedIndex = 0;
                else if (planStartTime.Hour >= 14 && planStartTime.Hour < 22)
                    control.SelectedIndex = 1;
                else
                    control.SelectedIndex = 2;
            }
            else if (control.Properties.Items.Count == 2)
            {
                if (planStartTime.Hour >= 6 && planStartTime.Hour < 18)
                    control.SelectedIndex = 0;
                else
                    control.SelectedIndex = 1;
            }
            else
                control.SelectedIndex = 0;

            control.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        public static void ShiftName(System.Windows.Forms.ComboBox combo, DateTime planStartTime)
        {
            combo.Items.Clear();
            foreach (string name in ShopCalendar.ShiftNames)
            {
                combo.Items.Add(name);
            }
            
            if (combo.Items.Count > 2)
            {
                if (planStartTime.Hour >= 6 && planStartTime.Hour < 14)
                    combo.SelectedIndex = 0;
                else if (planStartTime.Hour >= 14 && planStartTime.Hour < 22)
                    combo.SelectedIndex = 1;
                else
                    combo.SelectedIndex = 2;
            }
            else if (combo.Items.Count == 2)
            {
                if (planStartTime.Hour >= 6 && planStartTime.Hour < 18)
                    combo.SelectedIndex = 0;
                else
                    combo.SelectedIndex = 1;
            }
            else
                combo.SelectedIndex = 0;

            //combo.TextEditStyle = TextEditStyles.DisableTextEditor;
        }
        
        public static ICollection<string> Distinct(DataTable dtable, string columnName, string filter)
        {
            var dict = new SortedSet<string>();

            if (string.IsNullOrEmpty(columnName) || dtable == null)
                return dict;

            if (dtable.Columns.Contains(columnName) == false)
                return dict;
                        
            var dview = new DataView(dtable, filter, columnName, DataViewRowState.CurrentRows);                        
            if (dview == null)
                return dict;

            int column = dtable.Columns.IndexOf(columnName);
            foreach (DataRowView drow in dview)
            {
                if (drow.Row.IsNull(column))
                    continue;

                string value = Convert.ToString(drow[column]);
                if (value == null || dict.Contains(value))
                    continue;

                dict.Add(value);
            }

            return dict;
        }
    }
}
