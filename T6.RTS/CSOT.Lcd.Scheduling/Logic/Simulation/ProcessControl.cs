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
using Mozart.SeePlan.Simulation;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class ProcessControl
    {
        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public ProcTimeInfo GET_PROCESS_TIME0(AoEquipment aeqp, IHandlingBatch hb, ref bool handled, ProcTimeInfo prevReturnValue)
        {
            FabStep step = hb.CurrentStep as FabStep;
            FabEqp eqp = aeqp.Target as FabEqp;

            FabLot lot = hb.Sample as FabLot;
            StepTime st = step.GetStepTime(eqp.EqpID, lot.CurrentProductID);

            float mixRunRatio = 1;
            if (aeqp.IsParallelChamber)
            {
                if (eqp.IsMixRunning())
                    mixRunRatio = lot.CurrentFabStep.StdStep.MixCriteria;

                if (mixRunRatio == 0)
                    mixRunRatio = 1;
            }

            ProcTimeInfo time = new ProcTimeInfo();

            if (st != null)
            {
                time.FlowTime = TimeSpan.FromSeconds(st.ProcTime * mixRunRatio);
                time.TactTime = TimeSpan.FromSeconds(st.TactTime * mixRunRatio);
            }
            else
            {
                time.FlowTime = TimeSpan.FromMinutes(10);
                time.TactTime = TimeSpan.FromMinutes(10);
            }

            return time;
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_TRACK_IN0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {
            ResHelper.CheckContinueousQty(aeqp, hb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_NEED_SETUP0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            FabLot lot = hb.Sample as FabLot;
            FabAoEquipment eqp = aeqp.ToFabAoEquipment();

            bool isNeedSetup = ResHelper.IsNeedSetup(eqp, lot);
            
            return isNeedSetup;
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_TRACK_OUT0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {
            //FabAoEquipment feqp = aeqp as FabAoEquipment;
            //FabLot flot = hb.Sample as FabLot;
            //FabStep step = flot.CurrentStep as FabStep;

            //PreInspector.UpdateLastEventTime(aeqp, step, flot.CurrentProductID);

            //※TrackOut시점에는 Step이 바껴있지 않음.
            //InFlowMaster.ChangeWipLocation(hb, EventType.TrackOut);

            //if (aeqp.EqpID == "THCVD500")
            //    Console.WriteLine();
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public double GET_PROCESS_UNIT_SIZE1(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, double prevReturnValue)
        {
            double unitSize = SeeplanConfiguration.Instance.LotUnitSize;
            if (aeqp.IsBatchType() == false)
                unitSize = hb.UnitQty;

            return unitSize;
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="proc"/>
        /// <param name="handled"/>
        public void ON_BEGIN_PROCESSING0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.AoProcess proc, ref bool handled)
        {
            IHandlingBatch hb = proc.Entity as IHandlingBatch;
            FabLot lot = hb.Sample as FabLot;

            lot.CurrentFabPlan.EqpInStartTime = aeqp.NowDT;

            if (lot.IsRunWipFirstPlan())
            {
                lot.CurrentFabPlan.EqpInStartTime = lot.Wip.LastTrackInTime;
                lot.CurrentFabPlan.IsInitRunWip = true;
            }

            AcidMaster.OnBegingProcessing(aeqp, lot);
        }

        /// <summary>
        /// </summary>
        /// <param name="cproc"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public string[] GET_LOADABLE_CHAMBERS1(Mozart.SeePlan.Simulation.AoChamberProc2 cproc, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, string[] prevReturnValue)
        {
            var eqp = cproc.Parent as FabAoEquipment;
            var lot = hb.ToFabLot();

            //if (eqp.EqpID == "THCVD300" && lot != null && lot.LotID == "TH011661N0H")
            //    Console.WriteLine("B");

            var info = eqp.EqpDispatchInfo;

            //var loadableList = eqp.GetLoadableSubEqps(lot, false);
            var loadableList = eqp.GetLoadableSubEqps(lot);

            int count = loadableList == null ? 0 : loadableList.Count;
            string[] arr = new string[count];

            //loadable sub eqp별 EqpDispatchInfo 설정
            if (loadableList != null)
            {
                for (int i = 0; i < count; i++)
                {
                    var subEqp = loadableList[i];                    

                    arr[i] = subEqp.SubEqpID;
                    subEqp.EqpDispatchInfo = info;
                }
            }

            eqp.EndDispatch_ParallelChamber();          

            return arr;
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="loadableChambers"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public ISet<string> GET_NEED_SETUP_CHAMBERS0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.ChamberInfo[] loadableChambers, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, ISet<string> prevReturnValue)
        {
            FabLot lot = hb.ToFabLot();
            FabEqp eqp = aeqp.Target as FabEqp;

			List<FabSubEqp> list = ChamberMaster.GetSubEqps(eqp, loadableChambers);

            return ChamberMaster.GetNeedSetupChamberIDs(list, lot);
        }
    }
}
