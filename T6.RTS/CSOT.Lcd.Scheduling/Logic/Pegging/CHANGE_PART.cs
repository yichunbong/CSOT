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
    public partial class CHANGE_PART
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public List<object> GET_PART_CHANGE_INFOS0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun, ref bool handled, List<object> prevReturnValue)
        {

            if (isRun)
                return null;

            FabPegPart pp = pegPart as FabPegPart;
            FabProduct product = pp.Product as FabProduct;
            FabStep step = pp.CurrentStage.Tag as FabStep;

            if (product.HasPrevInterBom == false)
                return null;

            FabInterBom interbom;
            if (product.TryGetPrevInterRoute(step, out interbom) == false)
                return null;

            List<object> result = new List<object>() { interbom };

            return result;

        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="partChangeInfo"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart APPLY_PART_CHANGE_INFO0(PegPart pegPart, object partChangeInfo, bool isRun, ref bool handled, PegPart prevReturnValue)
        {

            FabInterBom bom = partChangeInfo as FabInterBom;

            FabPegPart pp = pegPart.ToFabPegPart();


            pp.AddCurrentPlan(bom.Product, bom.CurrentStep);

            pp.Product = bom.ChangeProduct;
            pp.InterBom = bom;

            return pp;
        }
    }
}
