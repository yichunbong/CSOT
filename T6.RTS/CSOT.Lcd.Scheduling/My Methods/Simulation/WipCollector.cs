using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.Lcd.Scheduling.DataModel;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Persists;
namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class WipCollector
    {

        static Dictionary<string, ProductVal> _prodValDic = new Dictionary<string, ProductVal>();


        public static void AddProductByVersion(FabLot lot)
        {
            string productID = lot.CurrentProductID;

            ProductVal val;
            if (_prodValDic.TryGetValue(productID, out val) == false)
                _prodValDic.Add(productID, val = new ProductVal(productID));

            val.AddLot(lot);
        }

        public static ProductVal GetProductByVersion(string productID)
        {
            ProductVal val;
            _prodValDic.TryGetValue(productID, out val);

            return val;
        }

        internal static string GetVersion(string productID)
        {
            ProductVal val = GetProductByVersion(productID);

            if (val == null)
                return null;

            return val.GetMaxQtyVersion();

        }

        public class ProductVal
        {
            public string ProductID { get; private set; }

            public Dictionary<string, List<FabLot>> WipList = new Dictionary<string, List<FabLot>>();
            public Dictionary<string, List<FabLot>> AfterCOAWips = new Dictionary<string, List<FabLot>>();


            public bool _isSelect;

            public string MaxVersion = Constants.NULL_ID;

            public ProductVal(string productID)
            {
                this.ProductID = productID;
            }

            public void AddLot(FabLot lot)
            {
                string version = lot.CurrentProductVersion;

                List<FabLot> list;
                if (WipList.TryGetValue(version, out list) == false)
                    WipList.Add(version, list = new List<FabLot>());

                list.Add(lot);

                FabStep step = lot.Wip.InitialStep as FabStep;

                if (step.StdStep.IsAferCOA)
                {
                    List<FabLot> list2;
                    if(AfterCOAWips.TryGetValue(version, out list2) == false)
                        AfterCOAWips.Add(version, list2 = new List<FabLot>());

                    list2.Add(lot);
                }

            }

            public string GetMaxQtyVersion()
            {
                if (_isSelect == false)
                {
                    string selectVersion = FindMaxVersion(this.AfterCOAWips);

                    if (LcdHelper.IsEmptyID(selectVersion))
                        selectVersion = FindMaxVersion(this.WipList);

                    this.MaxVersion = selectVersion;
                    _isSelect = true;
                }

                return this.MaxVersion;

            }

            private string FindMaxVersion(Dictionary<string, List<FabLot>> dic)
            {
                int max = 0;
                string selectVersion = string.Empty;

                foreach (var item in dic)
                {
                    int qty = item.Value.Sum(p => p.UnitQty);

                    if (LcdHelper.IsEmptyID(item.Key) == false && qty > max)
                    {
                        max = qty;
                        selectVersion = item.Key;
                    }
                }

                return selectVersion;
            }



        }




    }
}
