using CSOT.Lcd.Scheduling.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class InOutProfileMaster
    {
        private const int BANK_TAT_ARRAY = 0;   //minute
        private const int BANK_TAT_CF = 0;  //minute

        public static Dictionary<IComparable, FabOutProfile> OutProfiles = new Dictionary<IComparable, FabOutProfile>();
        public static Dictionary<IComparable, CellInProfile> InProfiles = new Dictionary<IComparable, CellInProfile>();
        
        #region Out Profile

        public static void AddWip(FabWipInfo wip)
        {
            DateTime outTime = wip.WipStateTime;
            OutInfo info = CreateHelper.CreateOutInfo(wip, outTime);

            AddOut(info);
        }

        public static void AddOut(FabLot lot, DateTime outTime)
        {
            OutInfo info = CreateHelper.CreateOutInfo(lot, outTime);

            AddOut(info);
        }

        private static void AddOut(OutInfo info)
        {
            var outProfiles = InOutProfileMaster.OutProfiles;

            var key = LcdHelper.CreateKey(info.ProductID, info.ProductVersion, info.OwnerType);

            FabOutProfile op;
            if (outProfiles.TryGetValue(key, out op) == false)
            {
                op = CreateHelper.CreateFabOutProfile(info);
                outProfiles.Add(key, op);

                //set matched cell info
                op.CellCodeList = CellCodeMaster.GetCellCodeList(info.ProductID);
            }
            
            AddInfo(op.Infos, info);
        }

        #endregion

        internal static void GenerateCellInProfile()
        {
            CreateCellInProfile();

            Allocate();
        }

        private static void CreateCellInProfile()
        {
            var outProfiles = InOutProfileMaster.OutProfiles;
            var inProfiles = InOutProfileMaster.InProfiles;

            var targetView = InputMart.Instance.ShopInTargetProdView;

            foreach (var op in outProfiles.Values)
            {
                if (op.HasCellCodeList() == false)
                    continue;

                string shopID = op.ShopID;
                string ownerType = op.OwnerType;

                foreach (var cellCode in op.CellCodeList)
                {
                    var prod = BopHelper.FindProduct(Constants.CellShop, cellCode);
                    if (prod == null)
                        continue;

                    string key = CreateKey(cellCode, ownerType);

                    CellInProfile profile;
                    if (inProfiles.TryGetValue(key, out profile) == false)
                    {
                        profile = CreateHelper.CreateCellInProfile(prod, ownerType);
                        inProfiles.Add(key, profile);

                        var inTargets = targetView.FindRows(prod).ToList();
                        if (inTargets != null && inTargets.Count > 1)
                            inTargets.Sort(CompareHelper.ShopInTargetComparer.Default);

                        profile.InTargets = inTargets;
                    }

                    profile.AddOutInfos(shopID, op.Infos);
                }
            }
        }

        private static void Allocate()
        {
			var inProfiles = InOutProfileMaster.InProfiles;			
			var list = inProfiles.Values.ToList();

			while(true)
			{                
                var find = SelectCellInProfile(list);
                if (find == null)
                    break;

                find.Allocate();
            }
		}

        private static CellInProfile SelectCellInProfile(List<CellInProfile> list)
        {
            if (list == null || list.Count == 0)
                return null;

            var finds = list.FindAll(t => t.HasRemainMatQty());
            if (finds == null)
                return null;

            if (finds.Count > 1)
                finds.Sort(CellInProfileComparer);

            return finds.FirstOrDefault();
        }

        internal static List<FabLot> CreateCellInputLot()
        {
            List<FabLot> list = new List<FabLot>();

            var inProfiles = InOutProfileMaster.InProfiles;
            foreach (var profile in inProfiles.Values)
            {
                var lotList = profile.CreateCellInputLot();
                if (lotList == null || lotList.Count == 0)
                    continue;

                list.AddRange(lotList);
            }
            
            return list;
        }

        private static int InOutInfoComparer(ProfileItem x, ProfileItem y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;
                            
            int cmp = 0;

            if (cmp == 0)
                cmp = x.ReleaseTime.CompareTo(y.ReleaseTime);

            if (cmp == 0)
                cmp = y.Qty.CompareTo(x.Qty);

            if (cmp == 0)
                cmp = string.Compare(x.ProductID, y.ProductID);

            if (cmp == 0)
                cmp = string.Compare(x.ProductVersion, y.ProductVersion);

            if (cmp == 0)
                cmp = string.Compare(x.OwnerType, y.OwnerType);

            return cmp;
        }

        private static int CellInProfileComparer(CellInProfile x, CellInProfile y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            var xt = x.FirstTarget();
            var yt = y.FirstTarget();

            bool null_x = xt == null;
            bool null_y = yt == null;

            int cmp = null_x.CompareTo(null_y);

            if (null_x || null_y)
                return cmp;

            if (cmp == 0)
                cmp = xt.TargetDate.CompareTo(yt.TargetDate);

            if (cmp == 0)
                cmp = xt.TargetQty.CompareTo(yt.TargetQty) * -1;

            //low level
            if (cmp == 0)
                cmp = string.Compare(x.ProductID, y.ProductID);

            return cmp;
        }

        #region FabOutProfile Func

        private static bool HasCellCodeList(this FabOutProfile op)
        {
            if (op.CellCodeList == null || op.CellCodeList.Count == 0)
                return false;

            return true;
        }

        #endregion FabOutProfile Func

        #region CellInProfile Func

        private static void AddOutInfos(this CellInProfile profile, string shopID, List<ProfileItem> outInfos)
        {
            if (outInfos == null || outInfos.Count == 0)
                return;

            var list = BopHelper.IsArrayShop(shopID) ? profile.ArrayInfos : profile.CFInfos;

            foreach (var info in outInfos)
            {
                AddInfo(list, info);
            }
        }
		
		private static bool HasRemainMatQty(this CellInProfile profile)		
		{
            bool hasArray = HasRemainQty(profile.ArrayInfos);
            if (hasArray == false)
                return false;

            bool hasCf = HasRemainQty(profile.CFInfos);
            if (hasCf == false)
                return false;

            return true;		
        }

        private static ShopInTarget FirstTarget(this CellInProfile profile)
        {
            var list = profile.InTargets;
            if (list == null || list.Count == 0)
                return null;

            return list.FirstOrDefault();
        }

        private static void Allocate(this CellInProfile profile)
        {
            int lotSize = SeeplanConfiguration.Instance.LotUnitSize;
            int maxQty = int.MaxValue;

            var inTarget = profile.FirstTarget();
            if (inTarget != null)
            {
                maxQty = (int)inTarget.TargetQty;

                //remove in target
                profile.InTargets.Remove(inTarget);
            }

            int allocQty = 0;
            while (true)
            {
                if (allocQty >= maxQty)
                    break;

                var minfo = profile.GetMatQtyInfo(lotSize);

                DateTime matTime = minfo.Item1;
                int matQty = minfo.Item2;

                if (matQty <= 0)
                    break;

                profile.AddCellInQty(matTime, matQty, inTarget);

                //remove out profile
                profile.RemoveOutProfile(matQty);
            }
        }

        private static void AddCellInQty(this CellInProfile profile, DateTime matTime, int matQty, ShopInTarget inTarget)        
        {
            InInfo info = CreateHelper.CreateInInfo(profile, matTime, matQty, inTarget);
            AddInfo(profile.CellInfos, info);            
        }

        private static void RemoveOutProfile(this CellInProfile profile, int qty)
        {
            RemoveQty(profile.ArrayInfos, qty);
            RemoveQty(profile.CFInfos, qty);
        }        

        private static Tuple<DateTime, int> GetMatQtyInfo(this CellInProfile profile, int targetQty)
        {
            Time xtat = Time.FromMinutes(BANK_TAT_ARRAY);
            Time ytat = Time.FromMinutes(BANK_TAT_CF);

            //필요수량(targetQty) 기준으로 체크
            var x = GetQtyInfo(profile.ArrayInfos, targetQty);

            DateTime xtime = x.Item1.AddMinutes(xtat.TotalMinutes);
            int xqty = x.Item2;            

            //사용수량(array) 기준으로 체크
            var y = GetQtyInfo(profile.ArrayInfos, xqty);

            DateTime ytime = y.Item1.AddMinutes(ytat.TotalMinutes);
            int yqty = y.Item2;

            DateTime matTime = LcdHelper.Max(xtime, ytime);
            int matQty = yqty;

            Tuple<DateTime, int> info = new Tuple<DateTime, int>(matTime, matQty);
            
            return info;
        }

        private static List<FabLot> CreateCellInputLot(this CellInProfile profile)
        {
            List<FabLot> list = new List<FabLot>();

            var infos = profile.CellInfos;
            if (infos == null || infos.Count == 0)
                return list;

            var prod = profile.Product;
            FabStep step = prod.Process.FirstStep as FabStep;

            foreach (InInfo info in infos)
            {
                DateTime avalableTime = info.ReleaseTime;
                int unitQty = info.Qty;

                string lotID = EntityHelper.CreateCellInLotID(prod);
                FabWipInfo wip = CreateHelper.CreateWipInfo(lotID, prod, step, unitQty);

                FabLot lot = CreateHelper.CreateLot(wip, Mozart.SeePlan.Simulation.LotState.CREATE);

				lot.ReleaseTime = LcdHelper.Max((DateTime)avalableTime, ModelContext.Current.StartTime);
                lot.LotState = Mozart.SeePlan.Simulation.LotState.CREATE;
                lot.FrontInTarget = info.InTarget;

                list.Add(lot);
            }

            return list;
        }

        #endregion CellInProfile Func

        #region Helper

        private static string CreateKey(string cellCode, string ownerType)
        {
            return LcdHelper.CreateKey(cellCode, ownerType);
        }

        private static bool HasRemainQty(List<ProfileItem> list)
        {
            if (list == null)
                return false;

            var find = list.Find(t => t.RemainQty > 0);
            if (find != null)
                return true;

            return false;
        }

        private static Tuple<DateTime, int> GetQtyInfo(List<ProfileItem> list, int targetQty)
        {           
            if (list == null || targetQty <= 0)
                return new Tuple<DateTime, int>(DateTime.MaxValue, 0);

            DateTime availableTime = DateTime.MaxValue;

            int sum = 0;
            foreach (var it in list)            
            {
                if (sum >= targetQty)
                    break;

                if (it.RemainQty <= 0)
                    continue;

                sum += it.RemainQty;
                availableTime = it.ReleaseTime;                
            }

            int availableQty = Math.Min(sum, targetQty);

            Tuple<DateTime, int> info = new Tuple<DateTime, int>(availableTime, availableQty);

            return info;
        }

        private static int RemoveQty(List<ProfileItem> list, int qty)
        {
            int remainQty = qty;
            
            for (int i = 0; i < list.Count && remainQty > 0; i++)
            {
                var info = list[i];

                int minQty = Math.Min(remainQty, info.Qty);

                info.Qty -= minQty;
                info.UseQty += minQty;

                remainQty -= minQty;

                if (info.Qty <= 0.01)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }

            return remainQty;
        }

        //internal static void Subtract(this CellInProfile profile, int inQty)
        //{
        //    int qty = inQty;

        //    Subtract(profile.ArrayInfos, qty);
        //    Subtract(profile.CFInfos, qty);
        //}

        //private static void Subtract(List<ProfileItem> list, int qty)
        //{
        //    int remain = qty;

        //    foreach (InInfo info in list)
        //    {
        //        if (info.RemainQty == 0)
        //            continue;

        //        remain = info.Subtract(remain);
        //    }
        //}

        //private static int Subtract(this ProfileItem item, int remain)
        //{
        //    if (item.RemainQty == 0)
        //        return remain;

        //    if (remain >= item.RemainQty)
        //    {
        //        remain = remain - item.RemainQty;
        //        item.UseQty = item.RemainQty;
        //    }
        //    else
        //    {
        //        item.UseQty += remain;
        //        remain = 0;
        //    }

        //    return remain;
        //}

        private static void AddInfo(List<ProfileItem> list, ProfileItem info, bool allowMultiple = false)
        {
            int index = list.BinarySearch(info, InOutInfoComparer);

            if (allowMultiple || index < 0)
            {
                if (index < 0)
                    index = ~index;

                list.Insert(index, info);
            }
            else
            {
                list[index].UseQty += info.UseQty;
                list[index].Qty += info.Qty;
            }
        }


        #endregion Helper
    }
}