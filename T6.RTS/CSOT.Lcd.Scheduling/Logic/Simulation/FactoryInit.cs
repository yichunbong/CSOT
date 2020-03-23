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

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class FactoryInit
    {
        /// <summary>
        /// </summary>
        /// <param name="factory"/>
        /// <param name="wipManager"/>
        /// <param name="handled"/>
        public void INITIALIZE_WIP_GROUP0(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IWipManager wipManager, ref bool handled)
        {
            factory.WipManager.AddGroup("StepWips",
                "CurrentStepID", "CurrentProductID");

            //factory.WipManager.AddGroup("StepWips",
            //    "CurrentShopID", "CurrentStepID", "CurrentProductID", "CurrentProductVersion", "OwnerType");
        }

        /// <summary>
        /// </summary>
        /// <param name="factory"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<Mozart.SeePlan.DataModel.WeightPreset> GET_WEIGHT_PRESETS0(AoFactory factory, ref bool handled, IEnumerable<Mozart.SeePlan.DataModel.WeightPreset> prevReturnValue)
        {
            return InputMart.Instance.FabWeightPreset.Values.ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="factory"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<InOutAgent> INITIALIZE_IN_OUT_AGENTS0(Mozart.SeePlan.Simulation.AoFactory factory, ref bool handled, IList<Mozart.SeePlan.Simulation.InOutAgent> prevReturnValue)
        {
            List<InOutAgent> agents = new List<InOutAgent>();

            if (SimHelper.IsCellRunning)
            {
                InOutAgent cellAgent = new InOutAgent(factory, Constants.AgentKey_Cell);
                cellAgent.Duration = TimeSpan.FromHours(2);
                cellAgent.FireAtStart = true;
                //agent.Priority = -100;
                agents.Add(cellAgent);
            }
            else
            {
                InOutAgent rpAgent = new InOutAgent(factory, ReleasePlanMaster.AGENT_KEY);
                rpAgent.Duration = ReleasePlanMaster.CYCLE_TIME;
                rpAgent.FireAtStart = true;
                //agent.Priority = -100;
                agents.Add(rpAgent);

                InOutAgent frontAgent = new InOutAgent(factory, Constants.AgentKey_Front);
                frontAgent.Duration = TimeSpan.FromHours(1);
                frontAgent.FireAtStart = true;
                //agent.Priority = -100;
                agents.Add(frontAgent);
            }

            return agents;
        }

        /// <summary>
        /// </summary>
        /// <param name="factory"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<SecondResourcePool> GET_SECOND_RESOURCE_POOLS0(Mozart.SeePlan.Simulation.AoFactory factory, ref bool handled, IList<Mozart.SeePlan.Simulation.SecondResourcePool> prevReturnValue)
        {
            List<SecondResourcePool> pools = new List<SecondResourcePool>();

            // SecondResourcePool 객체 생성
            SecondResourcePool pool = new SecondResourcePool(factory, "Mask");

            // TOOL 정보를 기준으로 SecondResource 생성 및 Pool 에 추가
            foreach (FabMask mask in InputMart.Instance.FabMask.Values)
            {
                SecondResource res = new SecondResource(mask.ToolID, mask);
                
                res.Capacity = mask.Qty;
                res.Uses = 0;
                res.Pool = pool;
               
                pool.Add(res);
            }

            pools.Add(pool);

            return pools;
        }
    }
}
