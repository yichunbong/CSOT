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
using Mozart.SeePlan.Pegging;

namespace CSOT.Lcd.Scheduling.Logic.Pegging
{
    [FeatureBind()]
    public partial class BUILD_INPLAN
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart BUILD_IN_PLAN0(PegPart pegPart, ref bool handled, PegPart prevReturnValue)
        {
            foreach (FabPegPart pp in (pegPart as MergedPegPart).Items)
            {
                FabProduct prod = pp.Product as FabProduct;
                FabStep step = pp.Current.Step;

                foreach (FabPegTarget pt in pp.PegTargetList)
                {                    
                    PegHelper.WriteStepTarget(pt, false, Constants.IN, true);

                    if (pt.Qty > 0)
                    {
                        ShopInTarget inTarget = InputMart.Instance.ShopInTargetView.FindRows(step.ShopID, prod.ProductID, pt.CalcDate).FirstOrDefault();

                        if (inTarget == null)
                        {
                            inTarget = CreateHelper.CreateShopInTarget(pt, prod, step);
                            InputMart.Instance.ShopInTarget.Rows.Add(inTarget);
                        }

                        inTarget.TargetQty += (int)pt.CalcQty;

                        var mo = pt.MoPlan as FabMoPlan;
                        if (inTarget.Mo.Contains(mo) == false)
                            inTarget.Mo.Add(mo);

                        if (inTarget.Targets.Contains(pt) == false)
                            inTarget.Targets.Add(pt);
                    }
                }
            }

            return pegPart;
        }
    }
}
