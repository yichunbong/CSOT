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
    public partial class Rules
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public PegPart WRITE_LOADED_DEMAND(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            foreach (var item in InputMart.Instance.FabMoMaster.Values)
            {
                foreach (FabMoPlan mo in item.MoPlanList)
                {
                    Outputs.LoadedDemand row = new Outputs.LoadedDemand();

                    row.VERSION_NO = ModelContext.Current.VersionNo;

                    row.FACTORY_ID = mo.FactoryID;
                    row.SHOP_ID = mo.ShopID;
                    row.DEMAND_ID = mo.DemandID;
                    row.PRODUCT_ID = mo.PreMoPlan.ProductID;
                    row.PLAN_DATE = mo.DueDate;
                    row.PLAN_QTY = Convert.ToDecimal(mo.Qty);
                    row.PRIORITY = (string)mo.Priority;
                    row.TARGET_KEY = mo.TargetKey;

                    OutputMart.Instance.LoadedDemand.Add(row);
                }
            }

            return pegPart;
        }



        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public PegPart CELL_POST(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            return pegPart;
        }

     

       

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public PegPart CELL_PART_CHANGE(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            //MergedPegPart mp = pegPart as MergedPegPart;

            //foreach (FabPegPart pp in mp.Items)
            //{
            //    CellBom bom = CellCodeMaster.FindCellBomAtToList(pp.FabProduct.ShopID, pp.FabProduct.ProductID, "00001");

            //    if (bom != null)
            //    {
            //        FabProduct prod
            //        BopHelper.FindProduct(pp.Product.sho

            //    }

            //}



            return pegPart;
        }

        public PegPart CELL_BANK_PREPARE(PegPart pegPart)
        {
            MergedPegPart mg = pegPart as MergedPegPart;

            //PREPARE_TARGET
            PegMaster.PrePareTargetbyCell_InTarget(mg);

            //PREPARE_WIP
            PegMaster.PrePareBankWip();

            return pegPart;
        }
    }
}
