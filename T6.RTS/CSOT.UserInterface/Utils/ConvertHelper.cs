using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace CSOT.UserInterface.Utils
{
    public class ConvertHelper
    {
        public static DataTable ToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        public static DataTable ToDataTable<T>(List<T> items, Dictionary<string, int> prevDic)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length - 1; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                string stage = values[0].ToString();
                string siteID = values[1].ToString();
                string stepID = values[2].ToString();
                string prodID = values[3].ToString();
                string modelCode2 = values[4].ToString();
                string mpSalesID = values[5].ToString();
                string mpProductID = values[6].ToString();
                string eqpID = values[7].ToString();

                string key = CommonHelper.CreateKey(stage, siteID, stepID, prodID, modelCode2, mpSalesID, mpProductID, eqpID);
                if (prevDic.Keys.Contains(key) == false)
                    values[props.Length - 1] = 0;
                else
                    values[props.Length - 1] = prevDic[key];

                tb.Rows.Add(values);
            }

            return tb;
        }
    }
}
