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
    public static partial class CellCodeMaster
    {
        //Cell_Code 기준
        static DoubleDictionary<string, CellActionType, List<CellBom>> _maps = new DoubleDictionary<string, CellActionType, List<CellBom>>();

        //ActionType별 FromProduct기준 List
        static Dictionary<string, List<CellBom>> _fromListByType = new Dictionary<string, List<CellBom>>();

        private static string GetFromListKey(string productID, CellActionType actionType)
        {
            return LcdHelper.CreateKey(productID, actionType.ToString());
        }

        public static CellActionType ParseActionType(string p)
        {
            switch (p.ToUpper())
            {
                case "ASSY":
                    return CellActionType.Assy;

                case "PAIR":
                    return CellActionType.Pair;

                case "SHIP":
                    return CellActionType.Ship;

                case "STB1":
                    return CellActionType.STB1;

                case "STBCUTBANK":
                    return CellActionType.STBCUTBANK;
            }

            return CellActionType.None;
        }

        internal static void AddMaps(CellBom bom)
        {           
            Dictionary<CellActionType, List<CellBom>> dic;
            if (_maps.TryGetValue(bom.CellCode, out dic) == false)
            {
                dic = new Dictionary<CellActionType, List<CellBom>>();
                _maps.Add(bom.CellCode, dic);
            }

            List<CellBom> list;
            if (dic.TryGetValue(bom.ActionType, out list) == false)
                dic.Add(bom.ActionType, list = new List<CellBom>());

            list.Add(bom);
        }

        internal static void AddByActionType(CellBom bom)
        {
            string key = GetFromListKey(bom.FromProductID, bom.ActionType);

            List<CellBom> list;
            if (_fromListByType.TryGetValue(key, out list) == false)
                _fromListByType.Add(key, list = new List<CellBom>());

            list.Add(bom);
        }
        
        internal static CellBom FindCellBomAtFrom(string productID, string prodVer, CellActionType actionType)
        {
            List<CellBom> list = GetCellBomAtFrom(productID, actionType);

            if (list == null || list.Count == 0) 
                return null;

            CellBom bom = null;
            foreach (var item in list)
            {
                if (actionType != CellActionType.None && item.ActionType != actionType)
                    continue;

                if (LcdHelper.IsEmptyID(item.CellCode))
                    continue;

                if (prodVer != Constants.ALL && prodVer != item.FromProductVer)
                    continue;

                bom = item;
                break;
            }

            return bom;

        }
        
        private static List<CellBom> GetCellBomAtFrom(string productID, CellActionType actionType)
        {
            List<CellBom> list = new List<CellBom>();

            if (string.IsNullOrEmpty(productID))
                return list;

            if (actionType == CellActionType.None)
            {
                foreach (var bomList in _fromListByType.Values)
                {
                    foreach (var bom in bomList)
                    {
                        //2019.10.29 From(Array/CF) 기준에선 ShopID 체크 X
                        //if (bom.FromShopID != shopID)
                        //    continue;

                        if (bom.FromProductID != productID)
                            continue;

                        list.Add(bom);
                    }
                }
            }
            else
            {
                string key = GetFromListKey(productID, actionType);
                _fromListByType.TryGetValue(key, out list);
            }

            return list;
        }        

        internal static string GetCellCode(string productID, string productVer)
        {
            return GetCellCode(productID, productVer, CellActionType.None);
        }

        internal static string GetCellCode(string productID, string productVer, CellActionType actionType)
        {
            CellBom bom = FindCellBomAtFrom(productID, productVer, actionType);

            if (bom == null)
                return productID;

            return bom.CellCode;
        }

        internal static List<CellBom> GetCellBomByCellCode(string cellCode, CellActionType actionType)
        {
            if (string.IsNullOrEmpty(cellCode))
                return null;

            List<CellBom> list;
            _maps.TryGetValue(cellCode, actionType, out list);

            return list;
        }

        internal static string GetArrayCode(string cellCode)
        {
            var list = GetCellBomByCellCode(cellCode, CellActionType.Ship);

            if (list == null || list.Count == 0)
                return null;

            foreach (var item in list)
            {
                if (BopHelper.IsArrayShop(item.FromShopID))
                    return item.FromProductID;

            }

            return null;
        }

        internal static string GetCfCode(string cellCode)
        {
            var list = GetCellBomByCellCode(cellCode, CellActionType.Ship);

            if (list == null || list.Count == 0)
                return null;

            foreach (var item in list)
            {
                if (BopHelper.IsCfShop(item.FromShopID))
                    return item.FromProductID;

            }

            return null;
        }

        internal static List<string> GetCellCodeList(string productID, CellActionType actionType = CellActionType.None)
        {
            List<string> list = new List<string>();

            var cellBoms = GetCellBomAtFrom(productID, actionType);
            if (cellBoms == null || cellBoms.Count == 0)
                return list;           

            foreach (var cellBom in cellBoms)
            {
                string cellCode = cellBom.CellCode;

                if (string.IsNullOrEmpty(cellCode))
                    continue;

                if (list.Contains(cellCode) == false)
                    list.AddSort(cellCode);
            }
                        
            return list;
        }
    }
}
