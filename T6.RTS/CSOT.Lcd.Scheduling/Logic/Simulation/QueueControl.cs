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
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class QueueControl
    {
        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_HOLD0(DispatchingAgent da, IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            FabLot lot = hb.ToFabLot();
            //FabWipInfo wip = lot.Wip;
            //FabPlanInfo plan = lot.CurrentFabPlan;

            if (lot.IsInitHold)
            {
                InFlowMaster.ChangeWipLocation(hb, EventType.StartTOWait);
                return true;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Time GET_HOLD_TIME0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, Mozart.Simulation.Engine.Time prevReturnValue)
        {
            Time t = Time.Zero;

            FabLot lot = hb.ToFabLot();
            FabWipInfo wip = lot.Wip;
            //FabPlanInfo plan = lot.CurrentFabPlan;
            
			if (lot.IsInitHold)
            {
                t = wip.AvailableTime - da.NowDT;

                lot.HoldStartTime = da.NowDT;
                lot.HoldTime = t;


                lot.IsInitHold = false; // Hold → ExitHold  → IsHold 이므로 false로 설정해야됨. 바꿔주지 않을 경우 계속 Hold됨.
            }

            return t;
        }

        /// <summary>
        /// </summary>
        /// <param name="dispatchingAgent"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_HOLD_EXIT0(Mozart.SeePlan.Simulation.DispatchingAgent dispatchingAgent, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {
            FabLot lot = hb.ToFabLot();

            lot.HoldTime = 0;
        }


        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="destCount"/>
        /// <param name="handled"/>
        public void ON_NOT_FOUND_DESTINATION0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb, int destCount, ref bool handled)
        {
            FabLot lot = hb.ToFabLot();


            //TODO:
            if (lot.CurrentFabStep.StdStep.IsMandatory)
            {
                ErrHist.WriteIf(string.Format("{0}/{1}/{2}", "NotFoundArrange", lot.CurrentFabStep.StepID, lot.CurrentProductID),
                    ErrCategory.SIMULATION,
                    ErrLevel.INFO,
                    lot.CurrentFactoryID,
                    lot.CurrentShopID,
                    lot.LotID,
                    lot.CurrentProductID,
                    lot.CurrentProductVersion ?? lot.Wip.ProductVersion,
                    lot.CurrentProcessID,
                    Constants.NULL_ID,
                    lot.CurrentStepID,
                    "ON NOT FOUND DESTINATION0",
                    string.Format("Check Arrange → LOT_ID:{0}",lot.ToString())
					);

                return;
            }

            da.Factory.AddToBucketer(hb);
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_BUCKET_PROCESSING0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {

            FabLot lot = hb.ToFabLot();

            if (lot.LotID == "TH930454N01")
                Console.WriteLine();

            FabStep step = hb.CurrentStep as FabStep;

            

            if (step.IsMandatoryStep == false)
                return true;

            //if(step.StdStep != null && step.StdStep.)
            //    return true;

            return false;
        }

		public object GET_LOT_GROUP_KEY0(DispatchingAgent da, IHandlingBatch hb, ref bool handled, object prevReturnValue)
		{
			if (InputMart.Instance.GlobalParameters.ApplyLotGroupDispatching == false)
				return null;

			FabLot lot = hb.ToFabLot();
			FabStep step = lot.CurrentFabStep;

			string stepID = step.StepID;
			string productID = lot.CurrentProductID;
			string prodVer = step.NeedVerCheck ? lot.CurrentProductVersion : Constants.NULL_ID;
			string onwerType = lot.OwnerType;
			string onwerID = lot.OwnerID;

			return LcdHelper.CreateKey(stepID, productID, prodVer, onwerType, onwerID);
			
		}
	}
}
