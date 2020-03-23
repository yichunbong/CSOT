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
    [ FeatureBind()]
    public static partial class FabProductExt
    {

        #region Properties

        internal static bool IsFrontProduct(this FabProduct product, bool withCellIn = false)
        {
            if (!withCellIn && product.ShopID != Constants.CfShop
                && product.ShopID != Constants.ArrayShop)
                return false;

            if (product.ProductGroup != Constants.CF
                && product.ProductGroup != Constants.TFT)
                return false;

            return true;
        }


        internal static bool IsArrayShopProduct(this FabProduct product)
        {
            return product.ProductID.StartsWith("TH");
        }


        internal static bool IsCFShopProduct(this FabProduct product)
        {
            return product.ProductID.StartsWith("FH");
        }


        #endregion


        #region BOM Function
        
        public static bool TryGetPrevStepInfo(this FabProduct product, ref FabStep step, ref FabProduct prod)
        {
            bool isRouteChanged = false;

            FabStep orgStep = step;


            FabInterBom shopbom;
            if (product.TryGetInterRoute(orgStep, out shopbom, false))
            {
                step = shopbom.ChangeStep;
                prod = shopbom.ChangeProduct;
                isRouteChanged = true;
            }

            if (isRouteChanged == false && step != null)
            {
                //step = orgStep.PrevStep as FabStep;

                if (step.IsAlignStep == false)
                {
                    List<FabInnerBom> list = product.GetPrevInnerBom(orgStep.PrevStep as FabStep, Position.IN);

                    if (list != null)
                    {
                        step = list[0].ChangeStep;
                        isRouteChanged = true;
                    }
                }
            }

            if (isRouteChanged == false)
                step = orgStep.GetPrevMainStep(product, false); //orgStep.GetDefaultPrevStep() as FabStep;

            return isRouteChanged;
        } 

        #endregion


        #region InterBom

        public static void AddNextInterBom(this FabProduct prod, FabInterBom bom)
        {
            if (LcdHelper.ArrayContains(prod.NextInterBoms, bom))
                return;

            FabInterBom[] ps = prod.NextInterBoms;

            LcdHelper.ArrayAdd(ref ps, bom);

            prod.NextInterBoms = ps;

        }


        public static void AddPrevInterBom(this FabProduct prod, FabInterBom bom)
        {
            if (LcdHelper.ArrayContains(prod.PrevInterBoms, bom))
                return;

            FabInterBom[] ps = prod.PrevInterBoms;

            LcdHelper.ArrayAdd(ref ps, bom);

            prod.PrevInterBoms = ps;

        }


        public static bool TryGetNextInterRoute(this FabProduct product, FabStep step, out FabInterBom interBom)
        {
            return product.TryGetInterRoute(step, out interBom, true);
        }

        public static bool TryGetPrevInterRoute(this FabProduct product, FabStep step, out FabInterBom interBom)
        {
            return product.TryGetInterRoute(step, out interBom, false);
        }
        
        private static bool TryGetInterRoute(this FabProduct product, FabStep step, out FabInterBom interBom, bool isFromTo)
        {
            interBom = null;

            FabInterBom[] list = isFromTo ? product.NextInterBoms : product.PrevInterBoms;

            if (step == null)
            {
                interBom = null;
                return false;
            }

            foreach (FabInterBom it in list)
            {
                if (step.ShopID != it.CurrentShopID
                    || it.CurrentStep.StepID != step.StepID)
                    continue;

                interBom = it;
                break;

            }

            return interBom != null;
        }


        internal static FabProduct GetPrevFirst(this FabProduct prod)
        {
            if (prod.PrevInterBoms == null)
                return null;

            foreach (FabInterBom bom in prod.PrevInterBoms)
            {
                return bom.Product;
            }

            return null;
        }

       


        #endregion


        #region Inner Bom
        public static List<FabProduct> FindPrevAllProducts(this FabProduct product)
        {
            if (product.PrevInnerBoms == null)
                return null;

            List<FabProduct> result = new List<FabProduct>();
            FillPrevProduct(product, result);

            return result.Count > 0 ? result : null;
        }


        private static void FillPrevProduct(this FabProduct product, List<FabProduct> result)
        {
            if (product.PrevInnerBoms.Count > 0)
            {
                foreach (var innerbom in product.PrevInnerBoms)
                {
                    if (!result.Contains(innerbom.Product))
                    {
                        result.Add(innerbom.Product);
                        FillPrevProduct(innerbom.Product, result);
                    }
                }
            }

            int currShopIdx = GetShopIndex(product.ShopID);
            if (product.PrevInterBoms.Count() > 0)
            {
                foreach (FabInterBom interbom in product.PrevInterBoms)
                {
                    int prevShopID = GetShopIndex(interbom.Product.ShopID);
                    if (prevShopID < 0 || currShopIdx <= prevShopID)
                        continue;

                    if (!result.Contains(interbom.Product))
                    {
                        result.Add(interbom.Product);
                        FillPrevProduct(interbom.Product, result);
                    }
                }
            }
        }


        private static int GetShopIndex(string shopID)
        {
            if (shopID.Equals(Constants.ArrayShop) || shopID.Equals(Constants.CfShop))
                return 0;
            else if (shopID.Equals(Constants.SortShop))
                return 1;
            else if (shopID.Equals(Constants.CellShop))
                return 2;

            return -1;
        }


        public static List<FabInnerBom> GetNextInnerBom(this FabProduct product, FabStep step)
        {
            List<FabInnerBom> bom = new List<FabInnerBom>();

            if (SimHelper.IsCellRunning && step.StepID == Constants.CellInStepID)
                return null;

            foreach (FabInnerBom it in product.NextInnerBoms)
            {
                if (it.OriginStep == step)
                    bom.Add(it);
            }

            return bom.Count == 0 ? null : bom;

        }


        public static List<FabInnerBom> GetPrevInnerBom(this FabProduct product, FabStep step, Position pos)
        {
            List<FabInnerBom> bom = new List<FabInnerBom>();

            foreach (FabInnerBom it in product.PrevInnerBoms)
            {
                if (it.OriginStep == step/* && it.Position == pos*/)
                    bom.Add(it);
            }

            return bom.Count == 0 ? null : bom;
        } 
        #endregion
       
    }
}
