using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.DataModel;
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

namespace CSOT.Lcd.Scheduling.Logic.Pegging
{
    [FeatureBind()]
    public partial class CellBank
    {
        public Step GETLASTPEGGINGSTEP(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            if (InputMart.Instance.GlobalParameters.ApplyCellOutPlan == false)
                return null;


            FabPegPart pp = pegPart.ToFabPegPart();

            return pp.Current.Step; ;
        }

        public Step GETPREVPEGGINGSTEP(PegPart pegPart, Step currentStep)
        {
            if (InputMart.Instance.GlobalParameters.ApplyCellOutPlan == false)
                return null;

            FabPegPart pp = pegPart.ToFabPegPart();

            if (pp.Current.StepID == "0000")
            {
                string stepID = BopHelper.IsArrayShop(pp.FabProduct.ShopID) ? "9900" : "9990";

                FabStep step = BopHelper.GetSafeDummyStep(pp.Current.FactoryID, pp.Current.ShopID, stepID);

                pp.AddCurrentPlan(pp.FabProduct, step);

                return step;
            }

            return null;
        }
    }
}