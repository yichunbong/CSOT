using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Task.Execution;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public class FabManager
    {
        private bool _isRunManager = false;
        public string DspEqpGroupID { get; private set; }

        //DoubleDictionary<string, string, List<FabStep>> _stepList;

        //제품별
        Dictionary<string, List<FabStep>> _distinctSteps;
        Dictionary<string, EqpState> _eqpDic = new Dictionary<string, EqpState>();

        public int EqpCount { get { return _eqpDic == null ? 0 : _eqpDic.Count; } }

        public FabManager(string dspEqpGroupID)
        {
            _isRunManager = true;

            this.DspEqpGroupID = dspEqpGroupID;
        }

        public void Build()
        {
            if (_isRunManager)
            {
                BuildStepList();
                Initialize();
            }
        }

        private void BuildStepList()
        {
            //_stepList = new DoubleDictionary<string, string, List<FabStep>>();
            _distinctSteps = new Dictionary<string, List<FabStep>>();

            foreach (EqpState state in _eqpDic.Values)
            {
                //WeightSum에만 해당하는 Step만 찾도록 변경
                //만약 다른 Dispatcher에서도 사용해야 한다면 해당 로직 삭제 필요!
                if (state.Eqp.DispatcherType != DispatcherType.WeightSorted
                    && state.Eqp.DispatcherType != DispatcherType.WeightSum)
                    continue;

                //List<InFlowSteps> list1 = InFlowHelper.GetInFlowStepsValues(state.EqpID);
                List<InFlowSteps> list = state.GetInFlowStepsList();
                if (list == null)
                    continue;

                foreach (InFlowSteps it in list)
                {
                    string productID = it.ProductID;

                    List<FabStep> steps;
                    if (_distinctSteps.TryGetValue(productID, out steps) == false)
                        _distinctSteps.Add(productID, steps = new List<FabStep>());

                    foreach (FabStep step in it.Steps)
                    {
                        if (steps.Contains(step) == false)
                            steps.Add(step);
                    }
                }
            }
        }

        private void Initialize()
        {
            if (_isRunManager == false)
                return;
        }

        public void Management(DateTime now)
        {
            if (_isRunManager == false)
                return;

            //TODO : 주기별 프로파일 계산
            CalcWipProfile();

            WakeUp();
        }


        private void CalcWipProfile()
        {
            if (_eqpDic.Count == 0)
                return;

            //AoFactory.Current.Logger.MonitorInfo("Wip Profile을 계산합니다 {0}/{1}", AoFactory.Current.NowDT, DateTime.Now);


            // -- Wip Profile 계산
            foreach (JobState jobState in InFlowMaster.JobStateDic.Values)
            {

                EqpState sampleState = _eqpDic.Values.ElementAt(0);

                List<FabStep> list;
                _distinctSteps.TryGetValue(jobState.ProductID, out list);

                if (list == null)
                    continue;

                foreach (FabStep step in list)
                {
                    jobState.CalcWipProfile(step, sampleState.WPreset, null);
                    
                }
            }

           // AoFactory.Current.Logger.MonitorInfo("Wip Profile을 종료합니다 {0}", DateTime.Now);
        }

        private void WakeUp()
        {
            foreach (EqpState eqpState in _eqpDic.Values)
            {
                var eqp = eqpState.Equipment;

                eqp.WakeUp();
            }
        }

        //public void UpdateLoadedEqpList(FabStep step, string productID)
        //{
        //    JobState job = GetJobState(productID);
        //    if (job == null)
        //        return;

        //    job.UpdateLoadedEqpList(step);
        //}

        #region For Dispatcher


        public EqpState GetEqpState(string eqpID)
        {
            EqpState eqpState;
            if (_eqpDic == null || _eqpDic.TryGetValue(eqpID, out eqpState) == false)
                return null;

            return eqpState;
        }

        public void AddEqpState(EqpState eqpState)
        {
            string eqpID = eqpState.EqpID;

            EqpState state;

            if (_eqpDic.TryGetValue(eqpID, out state) == false)
                _eqpDic.Add(eqpID, eqpState);
        }

        #endregion For Dispatcher

        public override string ToString()
        {
            return this.DspEqpGroupID + "/" + this.EqpCount.ToString();
        }
    }

    public class EqpState
    {
        //ProductID, List<Step>
        private Dictionary<string, InFlowSteps> _stepList;
        private AoEquipment _equipment;
        private FabManager _fabManager;

        DateTime _lastAssignTime = DateTime.MinValue;
        DateTime _lastTrackInTime = DateTime.MinValue;
        DateTime _lastReservationTime = DateTime.MinValue;

        #region Properties

        public DateTime LastAssignTime
        {
            get { return _lastAssignTime; }
            set { _lastAssignTime = value; }
        }

        public DateTime LastTrackInTime
        {
            get { return _lastTrackInTime; }
            set { _lastTrackInTime = value; }
        }

        public DateTime LastReservationTime
        {
            get { return _lastReservationTime; }
            set { _lastReservationTime = value; }
        }

        public AoEquipment Equipment
        {
            get { return _equipment; }
        }

        public FabEqp Eqp
        {
            get { return _equipment.Target as FabEqp; }
        }

        public FabWeightPreset WPreset
        {
            get { return this.Eqp.Preset as FabWeightPreset; }
        }

        public string EqpID
        {
            get { return _equipment.EqpID; }
        }

        public bool IsIdle
        {
            get
            {
                foreach (AoProcess aop in _equipment.Processes)
                {
                    if (aop.LoadingState == LoadingStates.IDLE || aop.LoadingState == LoadingStates.IDLERUN)
                        return true;
                }

                return false;
            }
        }

        public Time NextInTime
        {
            get
            {
                Time nextInTime = _equipment.GetNextInTime();

                if (float.IsInfinity((float)nextInTime) || float.IsNaN((float)nextInTime))
                    return Time.Zero;

                return nextInTime;
            }
        }

        public DateTime NowDT
        {
            get { return _equipment.Factory.NowDT; }
        }

        #endregion Properties

        public EqpState(FabManager manager, AoEquipment equipment, Dictionary<string, InFlowSteps> steps)
        {
            _fabManager = manager;

            _stepList = steps;
            _equipment = equipment;

            Initialize();
        }

        private void Initialize()
        {
            _fabManager.AddEqpState(this);
        }

        public List<InFlowSteps> GetInFlowStepsList()
        {
            if (_stepList == null)
                return null;

            return _stepList.Values.ToList();
        }

        //public List<WeightHelper.JobStep> GetInFlowList(decimal flowHours, bool checkConstraint, bool writeLog = true)
        //{
        //    List<WeightHelper.JobStep> list = new List<WeightHelper.JobStep>();

        //    if (_stepList == null)
        //        return list;

        //    if (flowHours <= 0m)
        //        return list;

        //    Dictionary<string, JobState> jobStateDic = InFlowAgent.GetJobStates();
        //    if (jobStateDic == null)
        //        return list;

        //    WeightHelper.JobStep currJob = this.NextJobStep != null ? this.NextJobStep : this.CurrJobStep;

        //    //전체 JobList
        //    foreach (JobState jobState in jobStateDic.Values)
        //    {
        //        string productID = jobState.ProductID;

        //        InFlowSteps inflow;
        //        if (_stepList.TryGetValue(productID, out inflow) == false)
        //            continue;

        //        foreach (LcdStep step in inflow.Steps)
        //        {
        //            SecondResourcePool pool = AoFactory.Current.GetResourcePool("Mask");
        //            //if (checkConstraint)
        //            //{
        //            //    #region Tool, Capa Constraint Check
        //            //    //Exist Mask
        //            //    if (ResHelper.CheckUseSecondResource(step, jobState.ProductID)
        //            //        && this.ToolMgr.GetAvailableToolKit(step, jobState.ProductID, this.Equipment) == null)
        //            //    {

        //            //        if (writeLog)
        //            //        {
        //            //            //DispatchLog.Write(
        //            //            //    this.Equipment,
        //            //            //    jobState,
        //            //            //    step,
        //            //            //    this.NowDT,
        //            //            //    0,
        //            //            //    runDownComingWipQty,
        //            //            //    DispatchCategory.Filtering,
        //            //            //    FilterCategory.MaskConstraint
        //            //            //    );
        //            //        }
        //            //        continue;
        //            //    }

        //            //    #endregion
        //            //}

        //            WeightHelper.JobStep jStep = new WeightHelper.JobStep(jobState, jobState.SampleProd, step);
        //            list.Add(jStep);
        //        }
        //    }

        //    return list;
        //}
    }
}
