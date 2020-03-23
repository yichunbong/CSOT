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
    public partial class FactoryEvents
    {
        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_BEGIN_INITIALIZE0(AoFactory aoFactory, ref bool handled)        
        {                    
            OutCollector.OnStart();

            //DebugTime에 이벤트 예약
            DebugHelper.AddDebugTime(aoFactory);
        }

        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_END_INITIALIZE0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {
            InFlowAgent.InitConstruct(aoFactory);

            OutCollector.WriteStepWip();
            WeightHelper.WriteWeightPresetLog();
        }

        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_SHIFT_CHANGE0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {
            Logger.MonitorInfo(string.Format("{0}..... {1}", aoFactory.NowDT.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_SHIFT_CHANGED0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {

        }        

        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_DAY_CHANGED0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {
            DateTime now = aoFactory.NowDT;

            if (InputMart.Instance.GlobalParameters.ApplyArrangeMType)
            {
                EqpArrangeMaster.OnDayChanged(now);
                MaskMaster.OnDayChanged(now);
            }
            
            ReleasePlanMaster.OnDayChanged(now);
        }

        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_START0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {
            if (InputMart.Instance.GlobalParameters.ApplyArrangeMType)
                EqpArrangeMaster.WriteHistory_Init();
        }
        
        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_DONE0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {
			MaskMaster.OnDone(aoFactory);

            OutCollector.OnDone(aoFactory);

            InFlowMaster.Reset();
        }
    }
}
