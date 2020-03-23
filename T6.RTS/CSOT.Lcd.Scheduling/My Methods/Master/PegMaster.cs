using CSOT.Lcd.Scheduling.DataModel;
using Mozart.SeePlan.Pegging;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class PegMaster
    {
        static bool _isInit;

        static Dictionary<string, List<IMaterial>> _inAct = new Dictionary<string, List<IMaterial>>();
        static Dictionary<string, List<IMaterial>> _outAct = new Dictionary<string, List<IMaterial>>();

        public static MergedPegPart CellBankPegParts { get; private set; }

        public static Dictionary<string, List<FabPlanWip>> CellBankPlanWips { get; private set; }

        public static PlanStep DummyStep { get; private set; }

        public static void InitPegMaster()
        {
            if (_isInit == false)
            {

                CellBankPlanWips = new Dictionary<string, List<FabPlanWip>>();

                _isInit = true;
            }
        }




        #region 실적 패깅
        internal static void AddAct(InOutAct act, bool isOut)
        {
            Dictionary<string, List<IMaterial>> dic = isOut ? _outAct : _inAct;

            List<IMaterial> list;
            if (dic.TryGetValue(act.ProductID, out list) == false)
            {
                list = new List<IMaterial>();
                dic.Add(act.ProductID, list);
            }

            list.Add(act);
        }


        internal static List<IMaterial> GetOutAct(FabPegPart pp)
        {
            string shopID = pp.FabProduct.ShopID;
            string productID = pp.FabProduct.ProductID;

            return GetOutAct(shopID, productID);
        }

        internal static List<IMaterial> GetOutAct(string shopID, string productID)
        {
            List<IMaterial> list;
            if (_outAct.TryGetValue(productID, out list) == false)
                return list = new List<IMaterial>();

            List<IMaterial> rlist = new List<IMaterial>();

            foreach (InOutAct item in list)
            {
                if (item.ShopID != shopID)
                    continue;

                rlist.Add(item);
            }

            return rlist;
        }

     
        internal static void ActPeg(ICollection<FabMoMaster> preMoMater)
        {

            foreach (FabMoMaster mm in preMoMater)
            {
                List<IMaterial> wips = GetOutAct(mm.ShopID, mm.ProductID);

                if (wips == null || wips.Count == 0)
                    continue;

                ActPeg(mm.MoPlanList, wips);
            }
        }

        private static void ActPeg(List<MoPlan> moList, List<IMaterial> wips)
        {
            List<MoPlan> targets = new List<MoPlan>(moList);

            foreach (PreMoPlan target in targets)
            {
                for (int j = 0; j < wips.Count; j++)
                {
                    var m = wips[j];
                    if (m.Qty == 0)
                        continue;

                    if (target.Qty > m.Qty)
                    {
                        float qty = (float)m.Qty;
                        target.Qty -= qty;
                        m.Qty = 0;
                        wips.RemoveAt(j--);

                        PegHelper.WriteActPeg(target, m, qty);
                    }
                    else
                    {
                        double qty = target.Qty;
                        m.Qty -= qty;
                        target.Qty = 0;

                        if (m.Qty == 0)
                            wips.RemoveAt(j--);

                        PegHelper.WriteActPeg(target, m, qty);

                        moList.Remove(target);

                        break;
                    }
                }
            }
        }
        #endregion


        internal static FabPegPart CreateCellBankPegPart(FabPegPart pp, FabProduct prod)
        {
            FabPegPart newPP = new FabPegPart(pp.MoMaster as FabMoMaster, prod);
            newPP.Steps = new List<PlanStep>(pp.Steps);

            foreach (FabPegTarget item in pp.PegTargetList)
            {
                FabPegTarget pt = item.Clone(newPP) as FabPegTarget;
                pt.TargetKey = PegHelper.CreateTargetKey(pt.TargetKey, prod.ProductID.Substring(0,2));

                newPP.AddPegTarget(pt);

                if (newPP.SampleMs == null)
                    newPP.SampleMs = pt;

            }

            return newPP;
        }

        internal static void PrePareTargetbyCell_InTarget(MergedPegPart mg)
        {
            List<PegPart> list = new List<PegPart>(mg.Items);

            foreach (FabPegPart pp in list)
            {
                string arrayCode = CellCodeMaster.GetArrayCode(pp.Product.ProductID);
                string cfCode = CellCodeMaster.GetCfCode(pp.Product.ProductID);

                var arrayProd = BopHelper.FindProduct(Constants.ArrayShop, arrayCode);
                var cfProd = BopHelper.FindProduct(Constants.CF, cfCode);

                if (arrayProd == null || cfProd == null)
                {
                    ErrHist.WriteIf(string.Format("BuildFabOutTarget{0}", pp.Product.ProductID),
                        ErrCategory.PEGGING,
                        ErrLevel.ERROR,
                        pp.Current.Step.FactoryID,
                        pp.Current.Step.ShopID,
                        Constants.NULL_ID,
                        pp.Product.ProductID,
                        Constants.NULL_ID,
                        Constants.NULL_ID,
                        Constants.NULL_ID,
                        pp.Current.ShopID,
                        "NOT FOUND PRODUCT",
                        string.Format("Do not build TFT OutTarget")
						);

                    mg.Items.Remove(pp);

                    continue;
                }

                FabStep arrayStep = BopHelper.GetSafeDummyStep(pp.Current.FactoryID, Constants.ArrayShop, "0000");
                FabStep cfStep = BopHelper.GetSafeDummyStep(pp.Current.FactoryID, Constants.CfShop, "0000");

                FabPegPart cfPP = PegMaster.CreateCellBankPegPart(pp, cfProd);
                cfPP.AddCurrentPlan(cfProd, cfStep);


                pp.AddCurrentPlan(arrayProd, arrayStep);
                pp.Product = arrayProd;

                foreach (FabPegTarget pt in pp.PegTargetList)
                {
                    pt.TargetKey = PegHelper.CreateTargetKey(pt.TargetKey, arrayProd.ProductID.Substring(0, 2));
                }




                mg.Items.Add(cfPP);
            }
        }



        internal static void PrePareBankWip()
        {
            if (CellBankPlanWips == null)
                CellBankPlanWips = new Dictionary<string, List<FabPlanWip>>();


            foreach (var item in InputMart.Instance.CellBankWips.Values)
            {
                FabPlanWip wip = CreateHelper.CreatePlanWip(item);

                string key = LcdHelper.CreateKey(item.ShopID, item.WipStepID);

                List<FabPlanWip> list;
                if (CellBankPlanWips.TryGetValue(key, out list) == false)
                    CellBankPlanWips.Add(key, list = new List<FabPlanWip>());

                list.Add(wip);
            }
        }


        internal static IList<IMaterial> GetBankWip(PegPart pegPart)
        {
            FabPegPart pp = pegPart.ToFabPegPart();
            FabProduct prod = pp.Current.Product;
            FabStep step = pp.Current.Step;

            string key = LcdHelper.CreateKey(step.ShopID, step.StepID);

            List<FabPlanWip> list;
            CellBankPlanWips.TryGetValue(key, out list);

            if (list == null)
                return null;

            List<IMaterial> result = new List<IMaterial>();

            foreach (var item in list)
            {
                if (item.ProductID == prod.ProductID)
                    result.Add(item);
            }

            return result;
        }

        public class PegPartComparer : IComparer<PegPart>
        {

            public int Compare(PegPart x, PegPart y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                return PegHelper.ComparePegPart(x, y);
            }

        }

    }
}
