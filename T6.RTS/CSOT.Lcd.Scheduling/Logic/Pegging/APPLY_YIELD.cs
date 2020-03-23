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
    public partial class APPLY_YIELD
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public double GET_YIELD0(Mozart.SeePlan.Pegging.PegPart pegPart, ref bool handled, double prevReturnValue)
        {
            FabPegPart pp = pegPart.ToFabPegPart();

            FabStep step = pp.Current.Step;

            return step.GetYield(pp.FabProduct.ProductID);
        }

        /// <summary>
        /// </summary>
        /// <param name="qty"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public double ROUND_RESULT0(double qty, ref bool handled, double prevReturnValue)
        {
            return qty.ToRound(2);
        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="stage"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool USE_TARGET_YIELD0(PegPart pegPart, PegStage stage, ref bool handled, bool prevReturnValue)
        {
            return false;
        }
    }
}
