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
using Mozart.SeePlan.Pegging;

namespace CSOT.Lcd.Scheduling.Logic.Pegging
{
    [FeatureBind()]
    public partial class Fab
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public Step GETLASTPEGGINGSTEP(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            return PegHelper.GetLastPeggingStgep(pegPart);

        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="currentStep"/>
        /// <returns/>
        public Step GETPREVPEGGINGSTEP(PegPart pegPart, Step currentStep)
        {
            return PegHelper.GetPrevPeggingStep(pegPart, currentStep);
        }


        /// <summary>
        /// </summary>
        /// <param name="x"/>
        /// <param name="y"/>
        /// <returns/>
        public int COMPAREPEGPART(PegPart x, PegPart y)
        {
            return PegHelper.ComparePegPart(x, y);
        }


        /// <summary>
        /// </summary>
        /// <param name="x"/>
        /// <param name="y"/>
        /// <returns/>
        public int COMPAREPEGTARGET(PegTarget x, PegTarget y)
        {
            return PegHelper.ComparePegTarget(x, y);
        }


    }
}
