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
    public partial class EqpInit
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<Mozart.SeePlan.DataModel.Resource> GET_EQP_LIST0(ref bool handled, IEnumerable<Mozart.SeePlan.DataModel.Resource> prevReturnValue)
        {
            List<Mozart.SeePlan.DataModel.Resource> list = new List<Mozart.SeePlan.DataModel.Resource>();

            foreach (FabEqp eqp in InputMart.Instance.FabEqp.Values)
            {
                if (SimHelper.IsTftRunning)
                {
                    if (eqp.ShopID == Constants.CellShop)
                        continue;
                }
                else
                {
                    if (eqp.ShopID != Constants.CellShop)
                        continue;
                }


                list.Add(eqp);
            }

            return list;
        }

        /// <summary>
        /// 설비 초기화를 합니다. GetEqpList 직후 호출됨
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="handled"/>
        public void INITIALIZE_EQUIPMENT0(Mozart.SeePlan.Simulation.AoEquipment aeqp, ref bool handled)
        {
            //※ Inline 설비의 경우 설정확인 (true : ProessTime 사용, False : FlowTime 사용)
            aeqp.UseProcessingTime = false;
            
            //AoEqp 초기화
            FabAoEquipment eqp = aeqp.ToFabAoEquipment();

            var now = eqp.NowDT;

            eqp.LoadInfos = new List<FabLoadInfo>();            
            eqp.LastIdleStartTime = now;
            eqp.LastIdleRunStartTime = now;
            eqp.AvailablePMTime = DateTime.MaxValue;

			eqp.InitAcidDensity();

			ResHelper.AddEqpByGroup(eqp);

            //FabEqp 초기화
            FabEqp targetEqp = eqp.TargetEqp;

            targetEqp.InitPM();
            targetEqp.SetInitEqpStatus(eqp);
            
            //패럴러챔버 초기화
            if (aeqp.IsParallelChamber)
                ChamberMaster.InitializeParallelChamber(eqp);

            //설비상태가 Down 설정
            if (targetEqp.StatusInfo.Status == ResourceState.Down)
                targetEqp.State = ResourceState.Down;
        }

        /// <summary>
        /// </summary>
        /// <param name="resource"/>
        /// <param name="stateChangeTime"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public DateTime GET_EQP_UP_TIME0(Mozart.SeePlan.DataModel.Resource resource, DateTime stateChangeTime, ref bool handled, DateTime prevReturnValue)
        {
            FabEqp eqp = resource as FabEqp;


#if DEBUG
            if (eqp.EqpID == "THPHL100")
                Console.WriteLine();


            if (eqp.State == ResourceState.Up)
                Console.WriteLine();
#endif


            return eqp.StatusInfo.EndTime;
        }
    }
}
