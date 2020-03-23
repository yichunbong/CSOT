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
using Mozart.SeePlan.DataModel;
namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class InputBatchInit
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<Mozart.SeePlan.Simulation.ILot> INSTANCING0(ref bool handled, IEnumerable<Mozart.SeePlan.Simulation.ILot> prevReturnValue)
        {
            List<FabLot> list = new List<FabLot>();

            //if (SimHelper.IsTftRunning)
            //    list = GetFrontInLots();

            if (SimHelper.IsCellRunning)
            {
                InOutProfileMaster.GenerateCellInProfile();
                list = InOutProfileMaster.CreateCellInputLot();

                OutCollector.WriteRelasePlan_Cell(list);
            }

            return list;
        }

        //private List<FabLot> GetFrontInLots()
        //{
        //    return new List<FabLot>();

        //    //int maxUnitSize = SeeplanConfiguration.Instance.LotUnitSize;
        //    //DateTime planStartTime = ModelContext.Current.StartTime;

        //    //foreach (FrontInTarget target in InputMart.Instance.FrontInTarget.DefaultView)
        //    //{
        //    //    FabProduct prod = target.Product;
        //    //    FabStep step = target.Step;


        //    //    if (SimHelper.IsTftRunning && step.IsCellShop)
        //    //        continue;
        //    //    else if (SimHelper.IsCellRunning && step.IsCellShop == false)
        //    //        continue;


        //    //    DateTime rDate = target.TargetDate;

        //    //    if (rDate < planStartTime)
        //    //        rDate = planStartTime;


        //    //    double inQty = target.TargetQty;

        //    //    while (inQty > 0)
        //    //    {
        //    //        double unitQty = 0;
        //    //        if (maxUnitSize >= inQty)
        //    //        {
        //    //            if (inQty < 1)
        //    //            {
        //    //                inQty = 0;
        //    //                continue;
        //    //            }

        //    //            unitQty = maxUnitSize;
        //    //            inQty = 0;
        //    //        }
        //    //        else if (maxUnitSize < inQty)
        //    //        {
        //    //            inQty -= maxUnitSize;
        //    //            unitQty = maxUnitSize;
        //    //        }

        //    //        string lotID = EntityHelper.CreateFrontInLotID(prod);

        //    //        FabWipInfo wip = CreateHelper.CreateWipInfo(lotID, prod, step, unitQty);

        //    //        //CF 투입Lot 버전 생성룰 확인 필요 TODO
        //    //        if (step.IsCFShop)
        //    //            wip.ProductVersion = "00001";

        //    //        FabLot lot = CreateHelper.CreateLot(wip, Mozart.SeePlan.Simulation.LotState.CREATE);
        //    //        lot.ReleaseTime = rDate;
        //    //        lot.LotState = Mozart.SeePlan.Simulation.LotState.CREATE;



        //    //        list.Add(lot);

        //    //        OutCollector.WriteLotInPlan(lot, target);
        //    //    }
        //    //}
        //}
    }
}
