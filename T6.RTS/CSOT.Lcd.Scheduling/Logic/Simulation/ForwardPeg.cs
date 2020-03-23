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
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class ForwardPeg
    {
        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<Tuple<Mozart.SeePlan.DataModel.Step, object>> GET_STEP_PLAN_KEYS0(Mozart.SeePlan.Simulation.ILot lot, ref bool handled, IEnumerable<Tuple<Mozart.SeePlan.DataModel.Step, object>> prevReturnValue)
        {
            var slot = lot as FabLot;

            FabProduct prod = slot.FabProduct;
			string productID = prod.IsTestProduct ? prod.MainProductID : prod.ProductID;

			var keys = new List<Tuple<Step, object>>();
            var current = Tuple.Create<Step, object>(slot.CurrentStep, productID);
            keys.Add(current);

            return keys; 
        }

        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public double GET_FORWARD_PEGGING_QTY0(ILot lot, ref bool handled, double prevReturnValue)
        {
            var slot = lot as FabLot;

            return slot.UnitQty;
        }

        /// <summary>
        /// </summary>
        /// <param name="x"/>
        /// <param name="y"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public int COMPARE_STEP_TARGET0(Mozart.SeePlan.DataModel.StepTarget x, Mozart.SeePlan.DataModel.StepTarget y, ref bool handled, int prevReturnValue)
        {
            int cmp = x.DueDate.CompareTo(y.DueDate);

            return cmp;
        }

        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="st"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool FILTER_STEP_TARGET0(Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.StepTarget st, ref bool handled, bool prevReturnValue)
        {
            FabLot flot = lot as FabLot;

            if (flot.OwnerType == Constants.OwnerE)
            {
                //E Type Demand 생성시 비교필요 2019.6.27기준으로 Demand는 P Type만 존재, 패깅 E Type은 패깅하지 않음.
                return true;
            }
            
            return false;
        }
    }
}
