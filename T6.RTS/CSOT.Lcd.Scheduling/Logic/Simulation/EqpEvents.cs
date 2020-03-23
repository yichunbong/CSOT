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
using System.Text;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class EqpEvents
    {

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="handled"/>
        public void ON_EQP_START0(AoEquipment aeqp, ref bool handled)
        {
            FabAoEquipment eqp = aeqp.ToFabAoEquipment();

            var eqpState = eqp.Target.State;
            if(eqpState == ResourceState.Up)
            {
                if (eqp.IsProcessing == false)
                    eqp.OnStateChanged(LoadingStates.IDLE);
            }
            else if(eqpState == ResourceState.Down)
            {
                eqp.OnStateChanged(LoadingStates.DOWN);
            }            
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="state">WaitSetup/StartSetup/EndSetup/FirstLoading/LastLoading/FirstUnloading/LastUnloading</param>
        /// <param name="handled"/>
        public void PROCESS_STATE_CHANGED0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.ProcessStates state, ref bool handled)
        {
            FabAoEquipment eqp = aeqp.ToFabAoEquipment();
            FabLot lot = hb.ToFabLot();
            var now = aeqp.NowDT;

            if (eqp.EqpID == "THCVD300")
                Console.WriteLine();

            //Setup이 있을 경우 FirstLoading이 없음.
            if (state == ProcessStates.FirstLoading)
            {
                DispatchLogHelper.WriteDispatchLog_ParallelChamber(eqp, lot);

                eqp.LastLoadingTime = now;
                //if (eqp.LastLoadingTime >= eqp.LastIdleStartTime)
                //    eqp.LastIdleStartTime = DateTime.MinValue;

                //eqp.LastIdleRunStartTime = DateTime.MinValue;

                eqp.LoadCount++;
            }
            //else if (state == ProcessStates.LastLoading)
            //{
            //    eqp.LastIdleRunStartTime = now;
            //}
            //else if (state == ProcessStates.LastUnloading)
            //{
            //    eqp.LastIdleStartTime = now;
            //}
            else if (state == ProcessStates.StartSetup)
            {
                DispatchLogHelper.WriteDispatchLog_ParallelChamber(eqp, lot);
            }
            else if (state == ProcessStates.EndSetup)
            {
                eqp.LoadCount = 1;
                eqp.SetupCount++;
                //eqp.LastIdleStartTime = DateTime.MinValue;
                //eqp.LastIdleRunStartTime = DateTime.MinValue;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="state">SETUP/BUSY/IDLERUN/IDLE/WAIT_SETUP(PM/DOUWN 호출 안됨)</param>
        /// <param name="handled"/>
        public void LOADING_STATE_CHANGED0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.LoadingStates state, ref bool handled)
        {
            var eqp = aeqp.ToFabAoEquipment();                                  
            var now = eqp.NowDT;

            if (eqp.IsParallelChamber)
                return;

            if (ModelContext.Current.EndTime == now)
                return;

            //PM/Down 이벤트 예외사항 처리
            if (SimHelper.IgnoreStateChange(eqp, state))
                return;

            var lot = hb.ToFabLot();

            if (state == LoadingStates.SETUP || state == LoadingStates.BUSY)
                SetCurrentMask(eqp, lot);

            eqp.OnStateChanged(state, lot);
        }

        private void SetCurrentMask(FabAoEquipment eqp, FabLot lot)
        {
            if (SimHelper.IsTftRunning)
            {
                var mask = eqp.InUseMask;
                if (mask != null)
                {
                    mask.WorkInfos.Add(lot);

                    lot.CurrentMask = mask;
                    lot.CurrentFabPlan.MaskID = mask.ToolID;
                }
            }
            else
            {
                var jig = eqp.InUseJig;
                if (jig != null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var item in jig.Masks)
                    {
                        item.WorkInfos.Add(lot);

                        if (sb.Length > 0)
                            sb.Append(",");
                        sb.Append(item.JigID);
                    }

                    lot.CurrentFabPlan.MaskID = sb.ToString();
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="state">UP/DOWN</param>
        /// <param name="handled"/>
        public void RESOURCE_STATE_CHANGED0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.DataModel.ResourceState state, ref bool handled)
        {
            var eqp = aeqp.ToFabAoEquipment();

            if (state == ResourceState.Up)
                eqp.OnStateChanged(LoadingStates.IDLE);
        }

        public void CHAMBER_LOADING_STATE_CHANGED0(Mozart.SeePlan.Simulation.AoEquipment aeqp, ChamberInfo chamber, IHandlingBatch hb, LoadingStates state, ref bool handled)
        {            
            var eqp = aeqp.ToFabAoEquipment();            
            var now = eqp.NowDT;

            if (ModelContext.Current.EndTime == now)
                return;

            var subEqp = eqp.FindSubEqp(chamber);
            if (subEqp == null)
                return;

            var lot = hb.ToFabLot();

            //if (eqp.EqpID == "THCVD300" && lot != null && lot.LotID == "TH011661N0H")
            //    Console.WriteLine("B");

            //PM/Down 이벤트 예외사항 처리
            if (SimHelper.IgnoreStateChange(eqp, state))
                return;

            if (state == LoadingStates.SETUP || state == LoadingStates.BUSY)
                SetCurrentMask(eqp, lot);

            subEqp.OnStateChanged(eqp, state, lot);
        }
    }
}
