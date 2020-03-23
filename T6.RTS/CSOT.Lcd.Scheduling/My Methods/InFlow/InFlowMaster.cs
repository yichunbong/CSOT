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

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class InFlowMaster
    {
        //eqpGroup
        static Dictionary<string, FabManager> _fabManagerDic = new Dictionary<string, FabManager>();

        static DoubleDictionary<string, string, InFlowSteps> _arrangeStepsByEqpID = new DoubleDictionary<string, string, InFlowSteps>();

        static Dictionary<string, JobState> _jobStateDic = new Dictionary<string, JobState>();

        static Dictionary<FabLot, LotLocation> _lotLocationDic = new Dictionary<FabLot, LotLocation>();


        public static DoubleDictionary<string, string, InFlowSteps> ArrangeStepsByEqpID { get { return _arrangeStepsByEqpID; } }

        public static Dictionary<string, JobState> JobStateDic { get { return _jobStateDic; } }


        public static void Reset()
        {
            _fabManagerDic.Clear();
            _jobStateDic.Clear();
        }

        #region JOB State/Manager
        public static void AddFabMananger(string eqpGroup)
        {
            if (_fabManagerDic.ContainsKey(eqpGroup) == false)
                _fabManagerDic.Add(eqpGroup, new FabManager(eqpGroup));
        }

        public static FabManager GetManager(string eqpGroup)
        {
            FabManager fm;

            _fabManagerDic.TryGetValue(eqpGroup, out fm);

            return fm;
        }

        public static void Management(DateTime now)
        {
            foreach (var fm in _fabManagerDic.Values)
                fm.Management(now);
        }

        public static void RemoveJob(LotLocation lotLocation)
        {
            FabLot lot = lotLocation.Lot;

            string key = JobState.GetKey(lot);

            JobState jobState;
            if (_jobStateDic.TryGetValue(key, out jobState))
            {
                jobState.RemoveWipVar(lotLocation);
            }
        }

        internal static JobState GetJobState(FabLot lot)
        {
            string key = JobState.GetKey(lot);
            return GetJobState(key);
        }

        internal static JobState GetJobState(string productID, string ownerType)
        {
            string key = JobState.CreateKey(productID, ownerType);
            return GetJobState(key);
        }

        private static JobState GetJobState(string jobKey)
        {
            JobState state;
            JobStateDic.TryGetValue(jobKey, out state);

            return state;
        }

        public static void AddJob(LotLocation lotLocation)
        {
            FabLot lot = lotLocation.Lot;

            string key = JobState.GetKey(lot);

            JobState jobState;
            if (_jobStateDic.TryGetValue(key, out jobState) == false)
                _jobStateDic.Add(key, jobState = new JobState(lot));


            jobState.AddWipVar(lotLocation);
        }


        public static void UpdateLoadedEqpList(Dictionary<string, AoEquipment> eqpDic)
        {
            foreach (JobState job in _jobStateDic.Values)
                job.UpdateLoadedEqpList(eqpDic);
        }

        #endregion

        #region  Lot Change Event
        public static void ChangeWipLocation(IHandlingBatch hb, EventType eventType)
        {
            FabLot lot = hb.Sample as FabLot;

            if (hb is FabLot)
                ChangeWipLocation(lot, eventType);
            else
                hb.Apply((t, ct) => ChangeWipLocation(t as FabLot, eventType));
        }

        private static void ChangeWipLocation(FabLot lot, EventType eventType)
        {
            if (lot.CurrentPlan == null)
                return;

            FabLot prevLot = lot;

            LotLocation prevLocation;
            if (_lotLocationDic.TryGetValue(prevLot, out prevLocation))
                RemoveJob(prevLocation);

            LotLocation lotLocation = new LotLocation(lot, eventType);

            AddJob(lotLocation);

            _lotLocationDic[lot] = lotLocation;
        }

        public static void OnDoneWipLocation(IHandlingBatch hb)
        {
            FabLot lot = hb.Sample as FabLot;

            if (hb is FabLot)
                OnDoneWipLocation(lot);
            else
                hb.Apply((t, ct) => OnDoneWipLocation(t as FabLot));
        }

        private static void OnDoneWipLocation(FabLot lot)
        {
            JobState job = InFlowMaster.GetJobState(JobState.GetKey(lot));

            if (job != null)
                job.RemoveWip(WipType.Wait, lot, lot.CurrentFabStep);

        }
        #endregion               

        //Input Perstist EqpStepTime 에서 가능설비 로딩
        public static void AddStep(string eqpID, string productID, FabStep step)
        {
            InFlowSteps inflowSteps = ArrangeStepsByEqpID.Get(eqpID, productID);

            if (inflowSteps == null)
            {
                inflowSteps = CreateHelper.CreateInFlowSteps(eqpID, productID);
                ArrangeStepsByEqpID.Set(eqpID, productID, inflowSteps);
            }

            inflowSteps.Steps.Add(step);
        }
        
        public static Dictionary<string, InFlowSteps> GetInFlowSteps(string eqpID)
        {
            Dictionary<string, InFlowSteps> steps = new Dictionary<string, InFlowSteps>();
            ArrangeStepsByEqpID.TryGetValue(eqpID, out steps);

            return steps;
        }

        public static List<InFlowSteps> GetInFlowStepsValues(string eqpID)
        {
            Dictionary<string, InFlowSteps> steps = GetInFlowSteps(eqpID);

            if (steps == null)
                return new List<InFlowSteps>();

            return steps.Values.ToList();
        }
                
        public static int GetStepWipQtyforLayerBalance(List<FabStdStep> steps)
        {
            int qty = 0;

            int stepcnt = steps.Count;
            for (int i = 0; i < stepcnt; i++)
            {
                WipType type = WipType.Total;

                if (i == 0)
                    type = WipType.Run;
                else if (i == stepcnt - 1)
                    type = WipType.Wait;

                qty += GetStepWipQty(steps[i], type);
            }

            return qty;
        }

        public static int GetStepWipQty(FabStdStep stdStep, WipType type)
        {
            int qty = 0;
            foreach (var item in InFlowMaster.JobStateDic.Values)
            {
                foreach (var step in item.StepWips.Keys)
                {
                    if (step.StdStep != stdStep)
                        continue;

                    qty += item.StepWips[step].GetWips(type, Constants.NULL_ID);
                }
            }

            return qty;
        }
        
        public static decimal GetAllowRunDownWip(AoEquipment aeqp, string productID, string productVersion, string ownerType, FabStep step, decimal remainRundown)
        {
            var job = InFlowMaster.GetJobState(productID, ownerType);
            if (job == null)
                return 0m;
                                                    
            //Wip Profile
            WipProfile iflow = job.CreateWipProfile(step, productVersion, 0, aeqp.Target.Preset as FabWeightPreset, aeqp, remainRundown, false);
            iflow.CalcProfile();
            var qty = job.GetInflowWip(iflow, remainRundown);
            
            ////자신 + 직전
            ////자신의 대기재공중 Load가능한 수량
            //qty += job.GetCurrenStepWaitWipQty(aeqp, step, productVersion, remainRundown);
            
            ////자신의 직전의 Run중 Load가능한 수량
            //DateTime targetTime = aeqp.NowDT.AddHours((double)-remainRundown);
            //qty += job.GetPrevStepRunWipQty(aeqp, step, productVersion, targetTime);
            
            return qty;
        }

        public static bool ExistPrevStepRunWip(AoEquipment aeqp, string productID, string productVersion, string ownerType, FabStep step, decimal remainRundown)
        {
            var job = InFlowMaster.GetJobState(productID, ownerType);
            if (job == null)
                return false;
                               
            //자신의 대기재공중 Load가능한 수량
            float qty = job.GetCurrenStepWaitWipQty(aeqp, step, productVersion, remainRundown);
            if (qty > 0)
                return true;

            //자신의 직전의 Run중 Load가능한 수량
            DateTime targetTime = aeqp.NowDT.AddHours((double)remainRundown);
            qty += job.GetPrevStepRunWipQty(aeqp, step, productVersion, targetTime);
            if (qty > 0)
                return true;

            return false;
        }

        public static int GetContinuousQty(FabLot lot, FabStep baseStep, bool includeFromStep = false)
        {
            if (baseStep == null)
                return 0;

            var jobState = GetJobState(lot);
            if (jobState == null)
                return 0;

            string productID = lot.CurrentProductID;
            string productVersion = lot.CurrentProductVersion;

            bool isFixedProductVer = EqpArrangeMaster.IsFixedProductVer(baseStep.StdStep, productVersion);
            if (isFixedProductVer == false)
                productVersion = Constants.NULL_ID;

            int continuousQty = 0;
            if (includeFromStep)
            {
                //run
                int runWipQty = jobState.GetStepWips(baseStep, WipType.Run, productVersion);
                if (runWipQty <= 0)
                    return continuousQty;

                continuousQty += runWipQty;

                //wait
                int waitWipQty = jobState.GetStepWips(baseStep, WipType.Wait, productVersion);
                if (waitWipQty <= 0)
                    return continuousQty;

                continuousQty += waitWipQty;
            }
                        
            var pevStepList = baseStep.GetPrevSteps(productID);
            if (pevStepList != null && pevStepList.Count > 0)
            {
                //run
                int runWipQty = 0;
                foreach (var prevStep in pevStepList)
                    runWipQty += jobState.GetStepWips(prevStep, WipType.Run, productVersion);

                if (runWipQty <= 0)
                    return continuousQty;

                continuousQty += runWipQty;

                //wait
                int waitWipQty = 0;
                foreach (var prevStep in pevStepList)
                    waitWipQty += jobState.GetStepWips(prevStep, WipType.Wait, productVersion);

                if (waitWipQty <= 0)
                    return continuousQty;

                continuousQty += waitWipQty;
            }

            return continuousQty;
        }

    }

}
