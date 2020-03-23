using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using System.Data;


namespace CSOT.UserInterface.Utils
{
    public class ComboBindHelper
    {
        #region WinForm Combo

        public static void SetCombo(System.Windows.Forms.ComboBox combo, DataTable dt, string colName, bool insertAll)
        {
            combo.Items.Clear();

            if (insertAll)
                combo.Items.Add(Consts.ALL);

            if (dt != null)
            {
                string filter = string.Empty;
                DataView dv = new DataView(dt, filter, colName, DataViewRowState.CurrentRows);

                foreach (DataRow srow in dv.Table.Rows)
                {
                    string colValue = srow.GetString(colName, Consts.NULL_ID);
                    if (Consts.NULL_ID.Equals(colValue) || combo.Items.Contains(colValue))
                        continue;

                    combo.Items.Add(colValue);
                }
            }

            combo.SelectedIndex = 0;
        }

        public static void SetCombo(System.Windows.Forms.ComboBox combo, List<string> list, bool insertAll)
        {
            combo.Items.Clear();

            if (insertAll)
                combo.Items.Add("ALL");

            foreach (string areaID in list)
            {
                string itemCode = areaID.Trim();

                if (combo.Items.Contains(itemCode) == false)
                    combo.Items.Add(itemCode);
            }

            combo.SelectedIndex = 0;
        }

        #endregion

        #region DevExpress Combo

        public static void SetCombo(ComboBoxEdit combo, DataTable dt, string colName, bool insertAll)
        {
            combo.Properties.Items.Clear();

            if (insertAll)
                combo.Properties.Items.Add(Consts.ALL);

            if (dt != null)
            {
                string filter = string.Empty;
                DataView dv = new DataView(dt, filter, colName, DataViewRowState.CurrentRows);

                foreach (DataRow srow in dv)
                {
                    string colValue = srow.GetString(colName, Consts.NULL_ID);
                    if (Consts.NULL_ID.Equals(colValue) || combo.Properties.Items.Contains(colValue))
                        continue;

                    combo.Properties.Items.Add(colValue);
                }
            }

            combo.SelectedIndex = 0;

        }

        //public static void SetCombo(ComboBoxEdit combo, List<string> list, bool insertAll)
        //{
        //    SetCombo(combo, list, insertAll);
        //    //combo.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        //}

        public static void SetCombo(ComboBoxEdit combo, IEnumerable<string> list, bool insertAll)
        {
            combo.Properties.Items.Clear();

            if (insertAll)
                combo.Properties.Items.Add("ALL");

            foreach (string areaID in list)
            {
                string itemCode = areaID.Trim();

                if (combo.Properties.Items.Contains(itemCode) == false)
                    combo.Properties.Items.Add(itemCode);
            }

            combo.SelectedIndex = 0;
            //combo.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        private static void SetComboList(ComboBoxEdit combo, DataTable dt, string filter,
                                            string colName, string sortColName, string dateFormat, bool includeALL = false)
        {
            combo.Properties.Items.Clear();
            if (includeALL)
                combo.Properties.Items.Add(Consts.ALL);

            DataView dataViewTable = new DataView(dt, filter, sortColName, DataViewRowState.CurrentRows);

            foreach (DataRowView viewRow in dataViewTable)
            {
                string resultName = string.IsNullOrEmpty(dateFormat) ? viewRow[colName].ToString() :
                                            Convert.ToDateTime(viewRow[colName]).ToString(dateFormat);

                if (string.IsNullOrEmpty(resultName)) continue;

                if (combo.Properties.Items.Contains(resultName) == false)
                    combo.Properties.Items.Add(resultName);
            }

            combo.SelectedIndex = 0;
            combo.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        public static void SetComboList(CheckedComboBoxEdit combo, List<string> list, bool checkedALL = false)
        {
            combo.Properties.Items.Clear();

            foreach (string item in list)
            {
                //combo.Properties.Items.Add(item);
                combo.Properties.Items.Add(item, CheckState.Checked, checkedALL);
            }
            combo.Properties.SeparatorChar = ';';

            combo.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        public static void SetComboList(ComboBoxEdit combo, List<string> list, bool includeALL = false)
        {
            combo.Properties.Items.Clear();
            if (includeALL)
                combo.Properties.Items.Add(Consts.ALL);

            foreach (string item in list)
            {
                combo.Properties.Items.Add(item);
            }

            combo.SelectedIndex = 0;
            combo.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        public static int FindItemIndex(ComboBoxEdit combo, string sValue)
        {
            int i, n = combo.Properties.Items.Count;
            for (i = 0; i < n; i++)
            {
                string item = combo.Properties.Items[i].ToString();
                if (item.Contains(sValue))
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion
    }
}
