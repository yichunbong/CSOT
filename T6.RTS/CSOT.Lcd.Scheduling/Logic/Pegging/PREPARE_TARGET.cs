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
    public partial class PREPARE_TARGET
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart PREPARE_TARGET0(PegPart pegPart, ref bool handled, PegPart prevReturnValue)
        {
            //초기화
            PegMaster.InitPegMaster();

            //최초 MergedPegPart 생성.
            MergedPegPart mpp = pegPart as MergedPegPart;

            switch (pegPart.CurrentStage.StageID)
            {
                case("InitStage"):
                    PrePareTarget_Fab(mpp);
                    break;
              
                case("CellPreStage"):
                    PrePareTarget_Cell(mpp);
                    break;
               
                default:
                    break;
            }


            return mpp;
        }

        private void PrePareTarget_Cell(MergedPegPart mpp)
        {
            foreach (var mm in InputMart.Instance.FabMoMaster.Values)
            {
                if (BopHelper.IsCellShop(mm.ShopID) == false)
                    continue;

                FabPegPart pp = new FabPegPart(mm, mm.Product);

                foreach (FabMoPlan mo in mm.MoPlanList)
                {
                    FabPegTarget pt = CreateHelper.CreateFabPegTarget(pp, mo);

                    pp.AddPegTarget(pt);

                    if (pp.SampleMs == null)
                        pp.SampleMs = pt;
                }

                mpp.Merge(pp);
            }
        }

        private void PrePareTarget_Fab(MergedPegPart mpp)
        {
            if (InputMart.Instance.GlobalParameters.ApplyCellOutPlan == false)
            {
                mpp.Items.Clear();

                foreach (var mm in InputMart.Instance.FabMoMaster.Values)
                {
                    if (BopHelper.IsCellShop(mm.ShopID))
                        continue;

                    FabPegPart pp = new FabPegPart(mm, mm.Product);

                    foreach (FabMoPlan mo in mm.MoPlanList)
                    {
                        FabPegTarget pt = CreateHelper.CreateFabPegTarget(pp, mo);

                        pp.AddPegTarget(pt);

                        if (pp.SampleMs == null)
                            pp.SampleMs = pt;
                    }

                    mpp.Merge(pp);
                }
            }
            
        }
    }
}
