using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.RTS.Inbound.DataModel;
using CSOT.RTS.Inbound.Inputs;
using CSOT.RTS.Inbound.Outputs;
using CSOT.RTS.Inbound.Persists;
namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class DataVaildHelper
    {
        public static bool IsVaildEqp(string eqpID)
        {
            //DCN
            if (IsVaildEqp_DCN(eqpID))
                return true;

            //ChamberID(UnitID) 
            if (IsVaildEqp_Chamber(eqpID))
                return true;

            if (string.IsNullOrEmpty(eqpID))
                return false;

            var eqps = InputMart.Instance.RTS_EQPbyEqpID;
            if (eqps == null || eqps.Count == 0)
                return false;

            var find = eqps.FindRows(eqpID).FirstOrDefault();
            if (find != null)
                return true;

            return false;
        }

        public static bool IsVaildEqp_Chamber(string chamberID)
        {            
            if (string.IsNullOrEmpty(chamberID))
                return false;

            //chamberID = ConfigHelper.GetCodeMap_ChamberID(chamberID);

            var eqps = InputMart.Instance.IF_MACHINESPEC_CHAMBERbyEqpID;
            if (eqps == null || eqps.Count == 0)
                return false;

            var find = eqps.FindRows(chamberID).FirstOrDefault();
            if (find != null)
                return true;

            return false;
        }

        public static bool IsVaildEqp_DCN(string eqpID)
        {
            if (string.IsNullOrEmpty(eqpID))
                return false;
                            
            var eqps = InputMart.Instance.RTS_EQP_DCNbyEqpID;
            if (eqps == null || eqps.Count == 0)
                return false;

            var find = eqps.FindRows(eqpID).FirstOrDefault();
            if (find != null)
                return true;

            return false;
        }

        public static bool IsVaildStdStep(string shopID, string stepID)
        {
            var find = GetStdStep(shopID, stepID);

            if (find != null)
                return true;
                        
            return false;
        }

        public static RTS_STD_STEP GetStdStep(string shopID, string stepID)
        {
            if (string.IsNullOrEmpty(shopID)
                || string.IsNullOrEmpty(stepID))
            {
                return null;
            }

            var table = InputMart.Instance.RTS_STD_STEP_DICT;
            var find = table.FindRows(shopID, stepID).FirstOrDefault();

            return find;
        }

        public static bool IsVaildStdStep(string stepID)
        {
            if (string.IsNullOrEmpty(stepID))
                return false;

            var table = InputMart.Instance.RTS_STD_STEP;
            bool exist = table.Rows.Any(t => t.STEP_ID == stepID);
            if (exist)
                return true;

            return false;
        }

        public static bool IsVaildProductID(string productID)
        {
            if (string.IsNullOrEmpty(productID))
                return false;

            var products = InputMart.Instance.RTS_PRODUCTByProductID;
            if (products == null || products.Count == 0)
                return false;

            var find = products.FindRows(productID).FirstOrDefault();
            if (find != null)
                return true;

            return false;
        }

        public static string ToVaildProductID(string shopID, string productID)
        {
            if (LcdHelper.IsCellShop(shopID) == false)
                return productID;

            var dashboard = InputMart.Instance.Dashboard;
            string cellCode = dashboard.GetCellCode(productID);
            if (string.IsNullOrEmpty(cellCode) == false)
                return cellCode;

            return productID;
        }

        public static bool ExcludeData(string shopID, string productID)
        {
            if (LcdHelper.IsCellShop(shopID))
            {

                if (string.IsNullOrEmpty(productID))
                    return true;

                string prodShopID = LcdHelper.GetShopIDByProductID(productID, shopID);
                if (LcdHelper.IsCfShop(prodShopID))
                    return true;
            }

            return false;
        }

        //public static List<string> GetCfPhotoEqpList()
        //{
        //    List<string> list = new List<string>();

        //    var table = InputMart.Instance.RTS_EQP;
        //    foreach (var entity in table.DefaultView)
        //    {
        //        string shopID = entity.SHOP_ID;
        //        if (LcdHelper.IsCfShop(shopID) == false)
        //            continue;

        //        if (LcdHelper.IsPhotoEqpGroup(entity.EQP_GROUP_ID))
        //            list.Add(entity.EQP_ID);
        //    }

        //    return list;
        //}               

        public static bool IsVaildTool(string toolID)
        {        
            if (string.IsNullOrEmpty(toolID))
                return false;

            var tools = InputMart.Instance.RTS_TOOLbyEqpID;
            if (tools == null || tools.Count == 0)
                return false;

            var find = tools.FindRows(toolID).FirstOrDefault();
            if (find != null)
                return true;

            return false;
        }

        public static List<string> GetEqpListByStepID(string shopID, string stepID)
        {
            List<string> list = new List<string>();

            var find = GetStdStep(shopID, stepID);
            if (find == null)
                return list;

            return GetEqpListByDispGroupID(find.DSP_EQP_GROUP_ID);
        }

        public static List<string> GetEqpListByDispGroupID(string dispGroupID)
        {
            List<string> list = new List<string>();

            var table = InputMart.Instance.RTS_EQP;
            foreach (var entity in table.DefaultView)
            {
                if (entity.DSP_EQP_GROUP_ID != dispGroupID)
                    continue;

                list.Add(entity.EQP_ID);
            }

            return list;
        }

        public static RTS_EQP GetEqp(string eqpID)
        {
            var eqps = InputMart.Instance.RTS_EQPbyEqpID.FindRows(eqpID);
            if (eqps != null)
                return eqps.FirstOrDefault();

            return null;
        }

        public static RTS_EQP_DCN GetDcnEqp(string eqpID)
        {
            var eqps = InputMart.Instance.RTS_EQP_DCNbyEqpID.FindRows(eqpID);
            if (eqps != null)
                return eqps.FirstOrDefault();

            return null;
        }
    }
}
