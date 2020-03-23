using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using Northwoods.Go;

using Mozart.Common;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class LotPathInfoView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        
        public LotPathInfoView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override DevExpress.XtraGrid.Views.Grid.GridView GetGridView()
        {
            return gridView1;
        }

        protected override void LoadDocument()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];
        }

        private void DrawingLotPath(string seletedLotID)
        {
            List<string> lotMappingList = GetLotMapingInfo(seletedLotID);
            Dictionary<string, LotHistoryItem> lotHistoryInfoFromLot = GetLotHistoryInfo(LotHistoryInfoKey.ISFromLot);
            Dictionary<string, LotHistoryItem> lotHistoryInfoToLot = GetLotHistoryInfo(LotHistoryInfoKey.ISToLot);

            DrawingLotPath(lotMappingList, lotHistoryInfoFromLot, lotHistoryInfoToLot, seletedLotID);

            Dictionary<string, SortedDictionary<string, LotInfoItem>> lotsInfo = GetLotsInfo();
            PrintLotInfoText(lotsInfo, seletedLotID);
            PrintLotInfoGrid(lotsInfo, lotMappingList);
        }

        private Dictionary<string, SortedDictionary<string, LotInfoItem>> GetLotsInfo()
        {
            Dictionary<string, SortedDictionary<string, LotInfoItem>> lotsInfo = new Dictionary<string, SortedDictionary<string, LotInfoItem>>();
            DataTable dtLotInfo = _result.LoadOutput(DataConsts.Out_EqpPlan);

            foreach (DataRow row in dtLotInfo.Rows)
            {
                string versionNo = row.GetString("VERSION_NO");
                string factoryID = row.GetString("FACTORY_ID");
                string shopID = row.GetString("SHOP_ID");
                string eqpID = row.GetString("EQP_ID");
                string eqpGroupID = row.GetString("EQP_GROUP_ID");
                string stateCode = row.GetString("STATE_CODE");
                //string eventID = row.GetString("EVENT_ID");
                string lotID = row.GetString("LOT_ID");
                //string slpsLine = row.GetString("SLPS_LINE");
                string stepID = row.GetString("STEP_ID");
                string layerID = row.GetString("LAYER_ID");
                DateTime startTime = row.GetDateTime("START_TIME");
                DateTime endTime = row.GetDateTime("END_TIME");
                string toolID = row.GetString("TOOL_ID");
                string processID = row.GetString("PROCESS_ID");
                string productID = row.GetString("PRODUCT_ID");
                string productKind = row.GetString("PRODUCT_KIND");
                //string kop = row.GetString("KOP");
                //int seqNo = row.GetInt32("SEQ_NO");
                //int jcSeqNo = row.GetInt32("JC_SEQNO");
                //string lotAttributes = row.GetString("LOT_ATTRIBUTES");
                //string lotUserID = row.GetString("LOT_USER_ID");
                //string toProductID = row.GetString("TO_PRODUCT_ID");
                //string linkSiteID = row.GetString("LINK_SITE_ID");
                //string shipMethod = row.GetString("SHIP_METHOD");
                //string stepGroup = row.GetString("STEP_GROUP");
                string stepType = row.GetString("STEP_TYPE");
                int unitQty = row.GetInt32("UNIT_QTY");
                //int glassQty = row.GetInt32("GLASS_QTY");
                string wipStepSeq = row.GetString("WIP_STEP_ID");
                //string wipLotAttribute = row.GetString("WIP_LOT_ATTRIBUTE");
                //string wipLaneCode = row.GetString("WIP_LANE_CODE");
                string day = row.GetString("TARGET_DAY");

                DateTime targetDay;
                if(DateTime.TryParse(day, out targetDay) == false)
                    targetDay = DateTime.MinValue;

                string targetProductID = row.GetString("TARGET_PRODUCT_ID");
                string tarDemandID = row.GetString("TAR_DEMAND_ID");
                string tarSeqNo = row.GetString("TAR_SEQ_NO");
                //string tarModelCode = row.GetString("TAR_MODEL_CODE");
                DateTime tarPlanDate = row.GetDateTime("TAR_PLAN_DATE");
                int tarPlanQty = row.GetInt32("TAR_PLAN_QTY");
                //float adjustFactor = row.GetFloat("ADJUST_FACTOR");
                //int apdValue = row.GetInt32("APD_VALUE");

                if (lotsInfo.ContainsKey(lotID) == false)
                {
                    lotsInfo[lotID] = new SortedDictionary<string, LotInfoItem>();
                }

                string shopNo = "9";
                if (shopID.Equals("T"))
                {
                    shopNo = "1";
                }
                else if (shopID.Equals("F"))
                {
                    shopNo = "2";
                }
                else if (shopID.Equals("C"))
                {
                    shopNo = "3";
                }
                else if (shopID.Equals("M"))
                {
                    shopNo = "4";
                }

                string stepKey = CommonHelper.CreateKey(shopNo, stepID);

                if (lotsInfo[lotID].ContainsKey(stepKey) == false)
                {
                    lotsInfo[lotID][stepKey] = new LotInfoItem();
                }

                lotsInfo[lotID][stepKey].AddItem(versionNo, factoryID, shopID, eqpID, eqpGroupID, stateCode, 
                    lotID, stepID, layerID, startTime, endTime, toolID, processID, productID, productKind, 
                    stepType, unitQty, wipStepSeq, targetDay, targetProductID, tarDemandID, tarSeqNo, 
                    tarPlanDate, tarPlanQty);
            }

            return lotsInfo;
        }

        private void PrintLotInfoGrid(Dictionary<string, SortedDictionary<string, LotInfoItem>> lotsInfo, List<string> lotMappingList)
        {
            DataTable dtLotInfo = SetSchema();
            List<string> pathLotList = GetPathLotList(lotMappingList);

            foreach (string lotID in pathLotList)
            {
                SortedDictionary<string, LotInfoItem> lotInfoItems;
                if (!lotsInfo.TryGetValue(lotID, out lotInfoItems))
                    continue;

                foreach (LotInfoItem lotInfoItem in lotInfoItems.Values)
                {
                    DataRow newRow = dtLotInfo.NewRow();
                    lotInfoItem.Add_RowItem(ref newRow);
                    dtLotInfo.Rows.Add(newRow);
                }
            }

            gridView1.Columns.Clear();
            gcLotInfoList.DataSource = dtLotInfo;
            gridView1.PopulateColumns();

            foreach (GridColumn column in gridView1.Columns)
            {

                if (column.ColumnType == typeof(DateTime))
                {
                    column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    column.DisplayFormat.FormatString = "yyyy/MM/dd HH:mm:ss"; 
                }
            }

            gridView1.BestFitColumns();
        }

        private List<string> GetPathLotList(List<string> lotMappingList)
        {
            List<string> pathLotList = new List<string>();

            foreach (string lots in lotMappingList)
            {
                foreach (string lotID in lots.Split('@'))
                {
                    if (pathLotList.Contains(lotID) == false)
                    {
                        pathLotList.Add(lotID.Trim());
                    }
                }
            }

            return pathLotList;
        }

        private DataTable SetSchema()
        {
            DataTable lotInfoDt = new DataTable();
            lotInfoDt.Columns.Add("VERSION_NO", typeof(string));
            lotInfoDt.Columns.Add("FACTORY_ID", typeof(string));
            lotInfoDt.Columns.Add("SHOP_ID", typeof(string));
            lotInfoDt.Columns.Add("EQP_ID", typeof(string));
            lotInfoDt.Columns.Add("EQP_GROUP_ID",typeof(string));
            lotInfoDt.Columns.Add("STATE_CODE", typeof(string));
            //lotInfoDt.Columns.Add("EVENT_ID", typeof(string));
            lotInfoDt.Columns.Add("LOT_ID", typeof(string));
            //lotInfoDt.Columns.Add("SLPS_LINE", typeof(string));
            lotInfoDt.Columns.Add("STEP_ID", typeof(string));
            lotInfoDt.Columns.Add("LAYER_ID", typeof(string));
            lotInfoDt.Columns.Add("START_TIME", typeof(DateTime));
            lotInfoDt.Columns.Add("END_TIME",  typeof(DateTime));
            lotInfoDt.Columns.Add("TOOL_ID", typeof(string));
            lotInfoDt.Columns.Add("PROCESS_ID", typeof(string));
            lotInfoDt.Columns.Add("PRODUCT_ID", typeof(string));
            lotInfoDt.Columns.Add("PRODUCT_KIND", typeof(string));
            //lotInfoDt.Columns.Add("KOP",typeof(string));
            //lotInfoDt.Columns.Add("SEQ_NO", typeof(int));
            //lotInfoDt.Columns.Add("JC_SEQ_NO", typeof(int));
            //lotInfoDt.Columns.Add("LOT_ATTRIBUTES", typeof(string));
            //lotInfoDt.Columns.Add("LOT_USER_ID", typeof(string));
            //lotInfoDt.Columns.Add("TO_PRODUCT_ID", typeof(string));
            //lotInfoDt.Columns.Add("LINK_SITE_ID", typeof(string));
            //lotInfoDt.Columns.Add("SHIP_METHOD", typeof(string));
            //lotInfoDt.Columns.Add("STEP_GROUP",typeof(string));
            lotInfoDt.Columns.Add("STEP_TYPE", typeof(string));
            lotInfoDt.Columns.Add("UNIT_QTY", typeof(int));
            //lotInfoDt.Columns.Add("GLASS_QTY", typeof(int));
            lotInfoDt.Columns.Add("WIP_STEP_ID", typeof(string));
            //lotInfoDt.Columns.Add("WIP_LOT_ATTRIBUTE", typeof(string));
            //lotInfoDt.Columns.Add("WIP_LANE_CODE", typeof(string));
            lotInfoDt.Columns.Add("TARGET_DAY", typeof(string));
            lotInfoDt.Columns.Add("TARGET_PRODUCT_ID", typeof(string));
            lotInfoDt.Columns.Add("TAR_DEMAND_ID", typeof(string));
            lotInfoDt.Columns.Add("TAR_SEQ_NO", typeof(string));
            //lotInfoDt.Columns.Add("TAR_MODEL_CODE", typeof(string));
            lotInfoDt.Columns.Add("TAR_PLAN_DATE", typeof(DateTime));
            lotInfoDt.Columns.Add("TAR_PLAN_QTY", typeof(int));
            //lotInfoDt.Columns.Add("ADJUST_FACTOR", typeof(float));
            //lotInfoDt.Columns.Add("APD_VALUE", typeof(int));

            //lotInfoDt.Columns.Add("FACTORY_ID", typeof(string));
            //lotInfoDt.Columns.Add("SHOP_ID", typeof(string));
            //lotInfoDt.Columns.Add("LOT_ID", typeof(string));
            //lotInfoDt.Columns.Add("SLPS_LINE", typeof(string));
            //lotInfoDt.Columns.Add("STEP_SEQ", typeof(string));
            //lotInfoDt.Columns.Add("START_TIME", typeof(DateTime));
            //lotInfoDt.Columns.Add("END_TIME", typeof(DateTime));
            //lotInfoDt.Columns.Add("EQP_ID", typeof(string));
            //lotInfoDt.Columns.Add("PROCESS_ID", typeof(string));
            //lotInfoDt.Columns.Add("PRODUCT_ID", typeof(string));
            //lotInfoDt.Columns.Add("PRODUCTION_TYPE_CODE", typeof(string));
            //lotInfoDt.Columns.Add("KOP", typeof(string));
            //lotInfoDt.Columns.Add("STEP_GROUP", typeof(string));
            //lotInfoDt.Columns.Add("PANEL_QTY", typeof(int));
            //lotInfoDt.Columns.Add("GLASS_QTY", typeof(int));
            //lotInfoDt.Columns.Add("TARGET_DAY", typeof(DateTime));
            //lotInfoDt.Columns.Add("TARGET_PRODUCT_ID", typeof(string));
            //lotInfoDt.Columns.Add("TAR_DEMAND_ID", typeof(string));
            //lotInfoDt.Columns.Add("TAR_SEQ_NO", typeof(string));
            //lotInfoDt.Columns.Add("TAR_MODEL_CODE", typeof(string));
            //lotInfoDt.Columns.Add("TAR_PLAN_DATE", typeof(DateTime));
            //lotInfoDt.Columns.Add("TAR_PLAN_QTY", typeof(int));

            return lotInfoDt;
        }

        private void PrintLotInfoText(Dictionary<string, SortedDictionary<string, LotInfoItem>> lotsInfo, string seletedLotID)
        {
            if (lotsInfo.ContainsKey(seletedLotID) == false)
            {
                return;
            }

            List<LotInfoItem> lotList = new List<LotInfoItem>(lotsInfo[seletedLotID].Values);
            StringBuilder sbLotInfo = lotList[0].GetLotInfoText();
            meLotInfo.Text = sbLotInfo.ToString();
        }

        private void DrawingLotPath(List<string> lotMappingList
            , Dictionary<string, LotHistoryItem> lotHistoryInfoFromLot
            , Dictionary<string, LotHistoryItem> lotHistoryInfoToLot
            , string seletedLotID)
        {
            this.goView1.Document.Clear();
            Dictionary<string, GoBasicNode> nodeInfo = new Dictionary<string, GoBasicNode>();
            Dictionary<string, int> xLevelInfo = GetXLevelInfo(lotMappingList);
            Dictionary<string, string> yLevelInfo = GetYLevelInfo(xLevelInfo);
            List<string> printedLotList = new List<string>();

            foreach (string fromToLotID in lotMappingList)
            {
                string fromLotID = fromToLotID.Split('@')[0];
                string toLotID = fromToLotID.Split('@')[1];

                int foreachIdx = 0;
                foreach (string lotID in new List<string>() { fromLotID, toLotID })
                {
                    foreachIdx++;

                    int xLevelNo = xLevelInfo[lotID];
                    string yLevelNo = yLevelInfo[lotID];
                    LotHistoryItem lotInfo = GetLotHistoryItem(lotHistoryInfoFromLot, lotHistoryInfoToLot, lotID);

                    if (printedLotList.Contains(lotID) == false
                        || (fromLotID.Equals(toLotID) && foreachIdx == 2))
                    {
                        if (fromLotID.Equals(toLotID))
                        {
                            lotInfo.SetLotHistoryInfoKey(LotHistoryInfoKey.ISToLot);
                            xLevelNo++;
                        }

                        GoBasicNode node = Drowing_Node(lotID, xLevelNo, yLevelNo, lotInfo, seletedLotID);
                        printedLotList.Add(lotID);

                        if (fromLotID.Equals(toLotID) && nodeInfo.ContainsKey(CommonHelper.CreateKey(lotID, "M")) == false)
                        {
                            nodeInfo.Add(CommonHelper.CreateKey(lotID, "M"), node);
                        }
                        else if (nodeInfo.ContainsKey(lotID) == false)
                        {
                            nodeInfo.Add(lotID, node);
                        }
                    }
                }

                if (fromLotID.Equals(toLotID))
                {
                    toLotID = CommonHelper.CreateKey(toLotID, "M");
                }

                DrawingLink(nodeInfo[fromLotID], nodeInfo[toLotID]);
            }
        }

        private LotHistoryItem GetLotHistoryItem(
            Dictionary<string, LotHistoryItem> lotHistoryInfoFromLot
            , Dictionary<string, LotHistoryItem> lotHistoryInfoToLot
            , string lotID)
        {

            LotHistoryItem lotInfo = new LotHistoryItem();
            if (lotHistoryInfoFromLot.ContainsKey(lotID))
            {
                lotInfo = lotHistoryInfoFromLot[lotID];
            }
            else if (lotHistoryInfoToLot.ContainsKey(lotID))
            {
                lotInfo = lotHistoryInfoToLot[lotID];
            }

            return lotInfo;
        }

        private void DrawingLink(GoBasicNode fromNode, GoBasicNode toNode)
        {
            GoLink link = new GoLink();
            link.ToArrow = true;
            link.PenColor = Color.Red;
            link.FromPort = fromNode.Port;
            link.ToPort = toNode.Port;
            goView1.Document.Add(link);
        }

        private Dictionary<string, string> GetYLevelInfo(Dictionary<string, int> xLevelInfo)
        {
            Dictionary<string, string> yLevelInfo = new Dictionary<string, string>();
            Dictionary<int, int> levelCntInfo = new Dictionary<int, int>();
            Dictionary<int, int> remainCntInfo = new Dictionary<int, int>();

            foreach (KeyValuePair<string, int> xLevelItem in xLevelInfo)
            {
                if (levelCntInfo.ContainsKey(xLevelItem.Value))
                {
                    levelCntInfo[xLevelItem.Value]++;
                    remainCntInfo[xLevelItem.Value]++;
                }
                else
                {
                    levelCntInfo[xLevelItem.Value] = 1;
                    remainCntInfo[xLevelItem.Value] = 1;
                }
            }

            foreach (KeyValuePair<string, int> xLevelItem in xLevelInfo)
            {
                int levelNo = levelCntInfo[xLevelItem.Value];
                int remainCnt = remainCntInfo[xLevelItem.Value]--;

                yLevelInfo[xLevelItem.Key] = CommonHelper.CreateKey(levelNo.ToString(), remainCnt.ToString());
            }

            return yLevelInfo;
        }

        private Dictionary<string, int> GetXLevelInfo(List<string> lotMappingList)
        {
            Dictionary<string, int> nodeLevelInfo = new Dictionary<string, int>();
            int levelIdx = 0;

            foreach (string lotID in lotMappingList)
            {
                string fromLotID = lotID.Split('@')[0];
                string toLotID = lotID.Split('@')[1];

                if (nodeLevelInfo.ContainsKey(fromLotID) == false
                    && nodeLevelInfo.ContainsKey(toLotID) == false)
                {
                    nodeLevelInfo[fromLotID] = levelIdx++;
                    nodeLevelInfo[toLotID] = levelIdx++;
                }
                else if (nodeLevelInfo.ContainsKey(fromLotID) == false
                    && nodeLevelInfo.ContainsKey(toLotID) == true)
                {
                    nodeLevelInfo[fromLotID] = nodeLevelInfo[toLotID] - 1;
                }
                else if (nodeLevelInfo.ContainsKey(toLotID) == false
                    && nodeLevelInfo.ContainsKey(fromLotID) == true)
                {
                    nodeLevelInfo[toLotID] = nodeLevelInfo[fromLotID] + 1;
                }
            }

            return nodeLevelInfo;
        }

        private GoBasicNode Drowing_Node(string lotID, Int32 xLevelNo
            , string yLevelNo, LotHistoryItem lotInfo, string seletedLotID)
        {
            int denominator = Convert.ToInt32(yLevelNo.Split('@')[0]);
            int numerator = Convert.ToInt32(yLevelNo.Split('@')[1]);

            GoBasicNode node = new GoBasicNode();
            node.Shape = new GoRoundedRectangle();
            node.Location = new PointF(xLevelNo * 250 + 150, 100 * numerator);
            node.Text = lotInfo.GetNodeText(lotID);
            node.Label.Multiline = true;

            if (seletedLotID.Equals(lotID))
            {
                node.Shape.BrushColor = Color.LightPink;
            }
            else
            {
                node.Shape.BrushColor = Color.LightGray;
            }

            node.LabelSpot = GoObject.MiddleCenter;
            goView1.Document.Add(node);
          
            return node;
        }

        private List<string> GetLotMapingInfo(string seletedLotID)
        {
            Dictionary<string, LotHistoryItem> lotHistoryInfo = GetLotHistoryInfo(LotHistoryInfoKey.ISFromToLot);
            List<string> lotMappingList = new List<string>();
            GetLotMapingInfo(seletedLotID, lotHistoryInfo, ref lotMappingList, true);
            GetLotMapingInfo(seletedLotID, lotHistoryInfo, ref lotMappingList, false);

            return lotMappingList;
        }

        private Dictionary<string, LotHistoryItem> GetLotHistoryInfo(LotHistoryInfoKey keyType)
        {
            Dictionary<string, LotHistoryItem> lotHistoryInfo = new Dictionary<string, LotHistoryItem>();
            DataTable lotHistory = _result.LoadOutput(DataConsts.Out_LotHistory);

            foreach (DataRow row in lotHistory.Rows)
            {
                string versionNo = row.GetString("VERSION_NO");
                string factoryID = row.GetString("FACTORY_ID");
                string fromShopID = row.GetString("FROM_SHOP_ID");
                string fromLotID = row.GetString("FROM_LOT_ID");
                string fromProcessID = row.GetString("FROM_PROCESS_ID");
                string fromProductID = row.GetString("FROM_PRODUCT_ID");
                string toLotID = row.GetString("TO_LOT_ID");
                string toProcessID = row.GetString("TO_PROCESS_ID");
                string toProductID = row.GetString("TO_PRODUCT_ID");
                DateTime fromDate = row.GetDateTime("FROM_DATE");
                DateTime toDate = row.GetDateTime("TO_DATE");
                Int32 fromQty = row.GetInt32("FROM_QTY");
                Int32 toQty = row.GetInt32("TO_QTY");
                string targetDemandID = row.GetString("TAR_DEMAND_ID");
                DateTime targetplanDate = row.GetDateTime("TARGET_PLAN_DATE");
                Int32 targetPlanQty = row.GetInt32("TARGET_PLAN_QTY");

                string key = string.Empty;

                if (keyType == LotHistoryInfoKey.ISFromLot)
                {
                    key = fromLotID;
                }
                else if (keyType == LotHistoryInfoKey.ISToLot)
                {
                    key = toLotID;
                }
                else if (keyType == LotHistoryInfoKey.ISFromToLot)
                {
                    key = CommonHelper.CreateKey(fromLotID, toLotID);
                }

                if (lotHistoryInfo.ContainsKey(key) == false)
                {
                    lotHistoryInfo[key] = new LotHistoryItem();
                }

                lotHistoryInfo[key].AddItem(versionNo, factoryID, fromShopID, fromLotID, fromProcessID
                    , fromProductID, toLotID, toProcessID, toProductID, fromDate, toDate
                    , fromQty, toQty, targetDemandID, targetplanDate, targetPlanQty, keyType);
            }

            return lotHistoryInfo;
        }

        private void GetLotMapingInfo(string seletedLot, Dictionary<string, LotHistoryItem> lotHistoryInfo
            , ref List<string> lotPathList, bool isFrom)
        {
            foreach (KeyValuePair<string, LotHistoryItem> lotHistoryItem in lotHistoryInfo)
            {
                string fromLotID = lotHistoryItem.Key.Split('@')[0];
                string toLotID = lotHistoryItem.Key.Split('@')[1];
                string fromCategory = lotHistoryItem.Value.GetCategory();

                string curtLotID = isFrom ? fromLotID : toLotID;
                string nextLotID = isFrom ? toLotID : fromLotID;

                if (seletedLot.Equals(curtLotID))
                {
                    string curtNextLotID = string.Empty;

                    if (isFrom)
                    {
                        curtNextLotID = CommonHelper.CreateKey(curtLotID, nextLotID);

                        if (lotPathList.Contains(curtNextLotID) == false)
                        {
                            lotPathList.Add(curtNextLotID);
                        }
                    }
                    else
                    {
                        curtNextLotID = CommonHelper.CreateKey(nextLotID, curtLotID);

                        if (lotPathList.Contains(curtNextLotID) == false)
                        {
                            lotPathList.Insert(0, curtNextLotID);
                        }
                    }

                    if (curtLotID.Equals(nextLotID)
                        || string.IsNullOrEmpty(fromLotID)
                        || string.IsNullOrEmpty(toLotID))
                    {
                        return;
                    }

                    GetLotMapingInfo(nextLotID, lotHistoryInfo, ref lotPathList, isFrom);
                }
            }
        }

        private void goView1_ObjectDoubleClicked(object sender, GoObjectEventArgs e)
        {
            GoBasicNode bn = e.GoObject.ParentNode as GoBasicNode;
            string seletedLotID = bn.Text.Split('\n')[0].Trim();
            seletedLotID = seletedLotID.Split(':')[1].Trim();
            DrawingLotPath(seletedLotID);
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            string seletedLotID = this.lotID_TextBox.Text;
            DrawingLotPath(seletedLotID);
        }
    }

    class LotHistoryItem
    {
        private string versionNo;
        private string factoryID;
        private string fromShopID;
        private string fromLotID;
        private string fromProcessID;
        private string fromProductID;
        private string toLotID;
        private string toProcessID;
        private string toProductID;
        private DateTime fromDate;
        private DateTime toDate;
        private int fromQty;
        private int toQty;
        private string targetPlanID;
        private DateTime targetPlanDate;
        private int targetPlanQty;
        private LotHistoryInfoKey keyType;

        internal void AddItem(string versionNo, string factoryID, string fromShopID, string fromLotID, string fromProcessID
            , string fromProductID, string toLotID, string toProcessID, string toProductID, DateTime fromDate
            , DateTime toDate, int fromQty, int toQty, string targetPlanID, DateTime targetplanDate
            , int target_Plan_Qty, LotHistoryInfoKey keyType)
        {
            this.versionNo = versionNo;
            this.factoryID = factoryID;
            this.fromShopID = fromShopID;
            this.fromLotID = fromLotID;
            this.fromProcessID = fromProcessID;
            this.fromProductID = fromProductID;
            this.toLotID = toLotID;
            this.toProcessID = toProcessID;
            this.toProductID = toProductID;
            this.fromDate = fromDate;
            this.toDate = toDate;
            this.fromQty = fromQty;
            this.toQty = toQty;
            this.targetPlanID = targetPlanID;
            this.targetPlanDate = targetplanDate;
            this.targetPlanQty = target_Plan_Qty;
            this.keyType = keyType;
        }

        internal string GetCategory()
        {
            return this.versionNo;
        }

        internal string GetNodeText(string lotID)
        {
            string text = string.Empty;

            if (keyType == LotHistoryInfoKey.ISToLot)
            {
                text = string.Format("LotID: {0}\nShopID: {1}\nPartNo: {2}\nGlassQty: {3}", lotID, fromShopID, toProductID, toQty);
            }
            else
            {
                text = string.Format("LotID: {0}\nShopID: {1}\nPartNo: {2}\nGlassQty: {3}", lotID, fromShopID, fromProductID, fromQty);
            }

            return text;
        }

        internal void SetLotHistoryInfoKey(LotHistoryInfoKey keyType)
        {
            this.keyType = keyType;
        }
    }

    class LotInfoItem
    {
        private string versionNo;
        private string factoryID;
        private string shopID;
        private string eqpID;
        private string eqpGroupID;
        private string stateCode;
        //private string eventID;
        private string lotID;
        //private string slpsLine;
        private string stepID;
        private string layerID;
        private DateTime startTime;
        private DateTime endTime;
        private string toolID;
        private string processID;
        private string productID;
        private string productKind;
        //private string kop;
        //private int seqNo;
        //private int jcSeqno;
        //private string lotAttributes;
        //private string lotUserID;
        //private string toProductID;
        //private string linkSiteID;
        //private string shipMethod;
        //private string stepGroup;
        private string stepType;
        private int unitQty;
        //private int glass_Qty;
        private string wipStepID;
        //private string wipLotAttribute;
        //private string wipLaneCode;
        private DateTime targetDay;
        private string targetProductID;
        private string tarDemandID;
        private string tarSeqNo;
        //private string tarModelCode;
        private DateTime tarPlanDate;
        private int tarPlanQty;
        //private float adjustFactor;
        //private int apdValue;

        internal void AddItem(string versionNo, string factoryID, string shopID, string eqpID
            , string eqpGroupID, string stateCode, string lotID, string stepID, string layerID, DateTime startTime, DateTime endTime
            , string toolID, string processID, string productID, string productKind
            , string stepType, int unitQty, string wipStepID, DateTime targetDay, string targetProductId, string tarDemandID
            , string tarSeqNo, DateTime tarPlanDate, int tarPlanQty)
        {
            this.versionNo = versionNo;
            this.factoryID = factoryID;
            this.shopID = shopID;
            this.eqpID = eqpID;
            this.eqpGroupID = eqpGroupID;
            this.stateCode = stateCode;
            //this.eventID = eventID;
            this.lotID = lotID;
            //this.slpsLine = slpsLine;
            this.stepID = stepID;
            this.layerID = layerID;
            this.startTime = startTime;
            this.endTime = endTime;
            this.toolID = toolID;
            this.processID = processID;
            this.productID = productID;
            this.productKind = productKind;
            //this.kop = kop;
            //this.seqNo = seqNo;
            //this.jcSeqno = jcSeqNo;
            //this.lotAttributes = lotAttributes;
            //this.lotUserID = lotUserID;
            //this.toProductID = toProductId;
            //this.linkSiteID = linkSiteID;
            //this.shipMethod = shipMethod;
            //this.stepGroup = stepGroup;
            this.stepType = stepType;
            this.unitQty = unitQty;
            //this.glass_Qty = glassQty;
            this.wipStepID = wipStepID;
            //this.wipLotAttribute = wipLotAttribute;
            //this.wipLaneCode = wipLaneCode;
            this.targetDay = targetDay;
            this.targetProductID = targetProductId;
            this.tarDemandID = tarDemandID;
            this.tarSeqNo = tarSeqNo;
            //this.tarModelCode = tarModelCode;
            this.tarPlanDate = tarPlanDate;
            this.tarPlanQty = tarPlanQty;
            //this.adjustFactor = adjustFactor;
            //this.apdValue = apdValue;
        }

        internal void Add_RowItem(ref DataRow newRow)
        {
            newRow["FACTORY_ID"] = factoryID;
            newRow["SHOP_ID"] = shopID;
            newRow["EQP_ID"] = eqpID;
            newRow["EQP_GROUP_ID"] = eqpGroupID;
            newRow["STATE_CODE"] = stateCode;
            //newRow["EVENT_ID"] = eventID;
            newRow["LOT_ID"] = lotID;
            //newRow["SLPS_LINE"] = slpsLine;
            newRow["STEP_ID"] = stepID;
            newRow["LAYER_ID"] = layerID;
            newRow["START_TIME"] = startTime;
            newRow["END_TIME"] = endTime;
            newRow["EQP_ID"] = eqpID;
            newRow["PROCESS_ID"] = processID;
            newRow["PRODUCT_ID"] = productID;
            newRow["PRODUCT_KIND"] = productKind;
            //newRow["KOP"] = kop;
            //newRow["STEP_GROUP"] = stepGroup;
            newRow["UNIT_QTY"] = unitQty;
            //newRow["GLASS_QTY"] = glass_Qty;
            newRow["TARGET_DAY"] = targetDay;
            newRow["TARGET_PRODUCT_ID"] = targetProductID;
            newRow["TAR_DEMAND_ID"] = tarDemandID;
            newRow["TAR_SEQ_NO"] = tarSeqNo;
            //newRow["TAR_MODEL_CODE"] = tarModelCode;
            newRow["TAR_PLAN_DATE"] = tarPlanDate;
            newRow["TAR_PLAN_QTY"] = tarPlanQty;
            //newRow["ADJUST_FACTOR"] = adjustFactor;
            //newRow["APD_VALUE"] = apdValue;

        }

        internal StringBuilder GetLotInfoText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("LotID\t : {0}", lotID));
            sb.AppendLine();
            sb.AppendLine(string.Format("Product\t : {0}", productID));
            sb.AppendLine();
            sb.AppendLine(string.Format("Process\t : {0}", processID));
            sb.AppendLine();
            sb.AppendLine(string.Format("Step_Seq\t : {0}", stepID));
            sb.AppendLine();
            sb.AppendLine(string.Format("StartTime\t : {0}", startTime.ToString()));
            sb.AppendLine();
            sb.AppendLine(string.Format("Unit Qty\t : {0}", unitQty));

            return sb;
        }
    }

    public enum LotHistoryInfoKey
    {
        ISFromLot = 0,
        ISToLot = 1,
        ISFromToLot = 2
    }
}
