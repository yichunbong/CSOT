using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class DaData
    {
        public class Schema
        {
            public const string SHOP_ID = "SHOP_ID";
            public const string EQP_GROUP = "EQP_GROUP";
            public const string EQP_ID = "EQP_ID";
            public const string SUB_EQP_ID = "SUB_EQP_ID";
            
            public const string DISPATCHING_TIME = "DISPATCHING_TIME";
            public const string SELECTED_LOT = "SELECTED_LOT";
            public const string SELECTED_PRODDUCT = "SELECTED_PRODDUCT";
            public const string SELECTED_STEP = "SELECTED_STEP";

            public const string INIT_WIP_CNT = "INIT_WIP_CNT";
            public const string FILTERED_WIP_CNT = "FILTERED_WIP_CNT";
            public const string SELECTED_WIP_CNT = "SELECTED_WIP_CNT";

            public const string FILTERED_REASON = "FILTERED_REASON";
            public const string FILTERED_LOT = "FILTERED_LOT";
            public const string FILTERED_PRODUCT = "FILTERED_PRODUCT";
            public const string DISPATCH_LOT = "DISPATCH_LOT";
            public const string DISPATCH_PRODUCT = "DISPATCH_PRODUCT";
            public const string PRESET_ID = "PRESET_ID";
        }

        public class DispatchingInfo
        {
            public DataRow DispatchInfoRow { get; private set; }
            public SimResultData.EqpDispatchLog Log { get; private set; }

            public string SeleLotID { get; private set; }
            public string SeleProdID { get; private set; }
            public string SeleStepID { get; private set; }
            public string FilteredReason { get; private set; }
            public string FilteredLotID { get; private set; }
            public string FilteredProdID { get; private set; }
            public string DispatchLotID { get; private set; }
            public string DispatchProdID { get; private set; }

            public string ShopID
            {
                get { return this.Log.ShopID; }
            }

            public string EqpID
            {
                get { return this.Log.EqpID; }
            }

            public string SubEqpID
            {
                get { return this.Log.SubEqpID; }
            }

            public string EqpGroupID
            {
                get { return this.Log.EqpGroup; }
            }

            public string PresetID
            {
                get { return this.Log.PresetID; }
            }
            public DateTime DispatchingTime
            {
                get { return this.Log.DispatchingTime; }
            }

            public int InitWipCnt
            {
                get { return this.Log.InitWipCount; }
            }

            public int FilteredWipCnt
            {
                get { return this.Log.FilteredWipCount; }
            }

            public int SelectedWipCnt
            {
                get { return this.Log.SelectedWipCount; }
            }
            public string FilteredWipLog
            {
                get { return this.Log.FilteredWipLog; }
            }
            public string DispatchWipLog
            {
                get { return this.Log.DispatchWipLog; }
            }

            public DispatchingInfo(DataRow dispatchInfoRow)
            {                
                SimResultData.EqpDispatchLog log = new SimResultData.EqpDispatchLog(dispatchInfoRow);

                this.DispatchInfoRow = dispatchInfoRow;
                this.Log = log;
               
                string seleLotID = string.Empty;
                string[] seleLotInfos = log.SelectedWip.Split(';');
                foreach (string info in seleLotInfos)
                {
                    if (string.IsNullOrEmpty(info))
                        continue;

                    seleLotID += string.IsNullOrEmpty(seleLotID) ? info.Split('/')[0] : ";" + info.Split('/')[0];
                }

                this.SeleLotID = seleLotID;

                string seleProdID = string.Empty;
                foreach (string info in seleLotInfos)
                {
                    if (string.IsNullOrEmpty(info))
                        continue;

                    seleProdID += string.IsNullOrEmpty(seleProdID) ? info.Split('/')[1] : ";" + info.Split('/')[1];
                }

                this.SeleProdID = seleProdID;

                string seleStepID = string.Empty;
                foreach (string info in seleLotInfos)
                {
                    if (string.IsNullOrEmpty(info))
                        continue;

                    seleStepID += string.IsNullOrEmpty(seleStepID) ? info.Split('/')[3] : ";" + info.Split('/')[3];
                }

                this.SeleStepID = seleStepID;

                #region Filter Information

                string filteredReasons = string.Empty;
                string filteredLotID = string.Empty;
                string filteredProdID = string.Empty;

                List<string> filteredReasonList = new List<string>();
                List<string> filteredProdList = new List<string>();
                
                string[] blobs = log.FilteredWipLog.Split('\t');
                foreach (string blob in blobs)
                {
                    string[] group = blob.Split(':');

                    if (group.Length < 2)
                        continue;

                    string reason = group[0];

                    string[] wips = group[1].Split(';');

                    foreach (string wip in wips)
                    {
                        string[] wipArr = wip.Split('/');

                        string lotID = wipArr[0];

                        filteredLotID += string.IsNullOrEmpty(filteredLotID) ? lotID : ";" + lotID;

                        if (filteredReasonList.Contains(reason) == false)
                        {
                            filteredReasonList.Add(reason);
                            filteredReasons += string.IsNullOrEmpty(filteredReasons) ? reason : ";" + reason;
                        }

                        string prodID = wipArr.Count() >= 2 ? wip.Split('/')[1] : Consts.NULL_ID;
                        
                        if (filteredProdList.Contains(prodID) == false)
                        {
                            filteredProdList.Add(prodID);
                            filteredProdID += string.IsNullOrEmpty(filteredProdID) ? prodID : ";" + prodID;
                        }
                    }
                }

                this.FilteredReason = filteredReasons;
                this.FilteredLotID = filteredLotID;
                this.FilteredProdID = filteredProdID;

                #endregion

                # region Dispatching Information

                string dispatchLotIDs = string.Empty;
                string dispatchProdIDs = string.Empty;

                List<string> dispatchProdList = new List<string>();

                string dispatchWipLog = log.DispatchWipLog;         // DISPATCH_WIP_LOG
                string[] dispatchInfoList = dispatchWipLog.Split(';');

                foreach (string info in dispatchInfoList)
                {
                    string[] split = info.Split('/');

                    string lotID = split.Count() >= 1 ? split[0] : string.Empty;
                    string prodID = split.Count() >= 2 ? split[1] : string.Empty; ;
                    
                    dispatchLotIDs += string.IsNullOrEmpty(dispatchLotIDs) ? lotID : ";" + lotID;

                    if (dispatchProdList.Contains(prodID) == false)
                    {
                        dispatchProdList.Add(prodID);
                        dispatchProdIDs += string.IsNullOrEmpty(dispatchProdIDs) ? prodID : ";" + prodID;
                    }
                }

                this.DispatchLotID = dispatchLotIDs;
                this.DispatchProdID = dispatchProdIDs;

                #endregion
            }
        }        

        public class FilteredInfo
        {
            public string LotID { get; private set; }
            public string StepID { get; private set; }
            public string ProductID { get; private set; }
            public string ProductVersion { get; private set; }
            public int LotQty { get; private set; }
            public string OwnerType { get; private set; }
            public string OwnerID { get; private set; }
            public string Reason { get; private set; }

            public FilteredInfo(string log, string reason)
            {
                if (string.IsNullOrEmpty(log))
                    return;

                string[] arr = log.Split('/');

                int count = arr.Length;

                string lotID = arr[0];
                string productID = count >= 2 ? arr[1] : Consts.NULL_ID;
                string productVer = count >= 3 ? arr[2] : Consts.NULL_ID;
                string stepID = count >= 4 ? arr[3] : Consts.NULL_ID;
                int qty = count >= 5 ? Convert.ToInt32(arr[4]) : 0;
                string ownerType = count >= 6 ? arr[5] : Consts.NULL_ID;
                string ownerID = count >= 7 ? arr[6] : Consts.NULL_ID;

                this.LotID = lotID;
                this.StepID = stepID;
                this.ProductID = productID;
                this.ProductVersion = productVer;                
                this.LotQty = qty;
                this.OwnerType = ownerType;
                this.OwnerID = ownerID;

                this.Reason = reason;
            }

            public static List<FilteredInfo> Parse(string log)
            {
                List<FilteredInfo> list = new List<FilteredInfo>();
                if (string.IsNullOrEmpty(log))
                    return list;

                string[] arr = log.Split('\t');
                foreach (string str in arr)
                {
                    string[] group = str.Split(':');

                    if (group.Length < 2)
                        continue;

                    string reason = group[0];
                    string[] wips = group[1].Split(';');

                    foreach (string wip in wips)
                    {
                        FilteredInfo item = new FilteredInfo(wip, reason);
                        list.Add(item);
                    }
                }

                return list;
            }
        }

        public class WeightFactorInfo
        {
            public string LotID { get; private set; }
            public string StepID { get; private set; }
            public string ProductID { get; private set; }
            public string ProductVersion { get; private set; }
            public int LotQty { get; private set; }
            public string OwnerType { get; private set; }
            public string OwnerID { get; private set; }
            public int Seq { get; private set; }
            public float Sum { get; private set; }

            public WeightFactorInfo(string log)
            {
                if (string.IsNullOrEmpty(log))
                    return;

                string[] arr = log.Split('/');

                int count = arr.Length;

                string lotID = arr[0];
                string productID = count >= 2 ? arr[1] : Consts.NULL_ID;
                string productVer = count >= 3 ? arr[2] : Consts.NULL_ID;
                string stepID = count >= 4 ? arr[3] : Consts.NULL_ID;
                int qty = count >= 5 ? Convert.ToInt32(arr[4]) : 0;
                string ownerType = count >= 6 ? arr[5] : Consts.NULL_ID;
                string ownerID = count >= 7 ? arr[6] : Consts.NULL_ID;

                this.LotID = lotID;
                this.StepID = stepID;
                this.ProductID = productID;
                this.ProductVersion = productVer;
                this.LotQty = qty;
                this.OwnerType = ownerType;
                this.OwnerID = ownerID;         
            }

            public static List<WeightFactorInfo> Parse(string log)
            {
                List<WeightFactorInfo> list = new List<WeightFactorInfo>();
                if (string.IsNullOrEmpty(log))
                    return list;

                string[] arr = log.Split(';');

                int count = arr.Length;
                for (int i = 0; i < count; i++)
                {
                    string str = arr[i];

                    var item = new WeightFactorInfo(str);

                    item.Seq = i + 1;
                    item.Sum = GetWeightSum(str);

                    list.Add(item);               
                }

                return list;
            }

            private static float GetWeightSum(string log)
            {
                if (string.IsNullOrEmpty(log))
                    return 0;

                string[] arr = log.Split('/');
                
                int count = arr.Length;
                int startIndex = 7;

                float sum = 0;
                for (int i = startIndex; i < count; i++)
                {
                    var str = arr[i];
                    var values = str.Split('@');

                    //string desc = string.Empty;
                    //if (arrVaules.Length > 1)
                    //    desc = arrVaules[1];

                    float score = CommonHelper.ToFloat(values[0]);

                    //마이너스(-) 값 존재시 대상제외 (sum -1 표시로 처리)
                    if (score < 0)
                        return score;

                    sum += score;
                }

                return sum;
            }
        }
    }
}
