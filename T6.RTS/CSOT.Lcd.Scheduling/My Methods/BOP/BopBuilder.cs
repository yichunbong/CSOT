using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.SeePlan.Lcd;
using Mozart.SeePlan.Lcd.DataModel;
using Mozart.Task.Execution;
using Mozart.SeePlan.DataModel;
using Mozart.Extensions;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public class BopBuilder
    {
        public BopType bopType;

        public BopBuilder(BopType type)
        {
            this.bopType = type;
        }

        public ComparePrevSteps ComparePrevSteps = null;
        public CompareSteps CompareSteps = null;

        public void BuildBop(Process proc, IList<LcdStep> steps, IEnumerable<IPrp> prps)
        {
            Dictionary<string, LcdStep> steplist = new Dictionary<string, LcdStep>();
            foreach (LcdStep step in steps)
                steplist.Add(step.StepID, step);

            Dictionary<string, PrpInfo> prplist = new Dictionary<string, PrpInfo>();
            foreach (IPrp path in prps)
            {
                string stepId = path.FromStep;

                PrpInfo prp;
                if (prplist.TryGetValue(stepId, out prp) == false)
                    prplist.Add(stepId, prp = new PrpInfo(stepId));

                prp.AddPrpTo(path.ToStep, 1);
            }

            BuildBop(proc, steplist, prplist);

        }
        public void BuildBop(Process proc, Dictionary<string, LcdStep> steps, Dictionary<string, PrpInfo> prps)
        {
            if (prps == null || prps.Count <= 0)
                BuildBop(proc, steps.Values.ToList<LcdStep>());
            else
                LinkRoute(proc, steps, prps);
        }

        public void BuildBop(Process proc, IList<LcdStep> steps)
        {
            if (proc == null)
            {

                return;
            }
            if (steps == null || steps.Count <= 0)
                return;

            if (CompareSteps == null)
            {
            }

            steps.QuickSort(this.CompareStepSeq);

            foreach (LcdStep step in steps)
            {
                proc.Steps.Add(step);
            }

            proc.LinkSteps();
        }

        private int CompareStepSeq(LcdStep x, LcdStep y)
        {
            return CompareSteps(x, y);
        }

        private void LinkRoute(Process proc, Dictionary<string, LcdStep> steps, Dictionary<string, PrpInfo> prps)
        {
            List<PrpPath> list = new List<PrpPath>();

            LcdStep from;
            LcdStep to;
            foreach (PrpInfo prp in prps.Values)
            {
                from = FindStep(proc, steps, prps, prp.StepId, true);

                foreach (PrpInfo.PrpTo pto in prp.ToList)
                {

                    if (pto.StepID == "-" || string.IsNullOrEmpty(pto.StepID))
                        continue;

                    to = FindStep(proc, steps, prps, pto.StepID, false);
                    if (to != null)
                    {
                        if (from == null)
                        {
                            Logger.MonitorInfo(string.Format("{0} -> {1} : ({0}) not found", prp.StepId, to.StepID));
                            steps[prp.StepId] = to;
                            break;
                        }
                        //recursive case check
                        if (to.NextStep != null && to.NextStep == from)
                        {
                            Logger.MonitorInfo(string.Format("recursive prp info at {0}", from.StepID));
                            continue;
                        }

                        if (from != to)
                        {
                            PrpPath path = new PrpPath(from, to, pto.Type, pto.Rate);
                            //Logger.MonitorInfo(string.Format("{0} -> {1}", from.StepID, to.StepID));

                            list.Add(path);
                        }
                    }
                    else
                    {
                        if (from == null)
                            Logger.MonitorInfo(string.Format("{0} -> {1} :  not found", prp.StepId, pto.StepID));
                        if (to == null)
                            Logger.MonitorInfo(string.Format("{0} -> {1} : ({1}) not found", prp.StepId, pto.StepID));
                    }
                }
            }

            List<LcdStep> startingSteps = FindFirstSteps(proc, steps);

            AddSteps(proc, startingSteps);

            InitRoute(proc, steps);


            if (proc.FirstStep == null)
            {
                Logger.Warn(proc.ProcessID + ": First Step is NULL ");
            }

        }
        private void InitRoute(Process proc, Dictionary<string, LcdStep> steps)
        {
            FixRoute(proc);

            InitializeRoute(proc);

            DeleteStepNonMapProcess(proc, steps);
        }

        private void FixRoute(Process proc)
        {
            LcdStep step = proc.FirstStep as LcdStep;

            //무한루프 방지(동일Process안에 Key인 동일 StepID가 존재하면 Key를 변경필요(그럴일 없겠지만)
            Dictionary<string, LcdStep> traversed = new Dictionary<string, LcdStep>();

            while (step != null)
            {
                LcdStep next = GetNonPathNextStep(proc, step, traversed);
                if (next == null)
                    break;

                new PrpPath(step, next, PrpPathType.Pass, 1);
                while (next != null)
                {
                    
                    if (traversed.ContainsKey(step.StepID))
                    {
                        Logger.MonitorInfo("!!!!!!!!! Error Check ProcStep : ProcessID:{0}/ StepID:{1}", proc.ProcessID, step.StepID);
                        return;
                    }
                    else
                        traversed.Add(step.StepID, step);

                    step = next;
                    next = step.NextStep as LcdStep;
                }
            }
        }

        private LcdStep GetNonPathNextStep(Process proc, LcdStep step, Dictionary<string, LcdStep> traversed)
        {
            SortedList<string, LcdStep> steps = new SortedList<string, LcdStep>();

            foreach (LcdStep it in proc.Steps)
            {

                if (traversed.ContainsKey(it.StepID))
                    continue;


                if (this.CompareSteps(step, it) < 0)
                    steps.Add(it.StepID, it);

                if (steps.Count > 0)
                    return steps.Values[0];
            }

            return null;
        }
        private void InitializeRoute(Process proc)
        {
            LcdStep step = proc.FirstStep as LcdStep;
            if (step == null)
                return;

            int index = 0;


            Dictionary<LcdStep, object> traversed = new Dictionary<LcdStep, object>();
            traversed.Add(step, null);

            LcdStep prev = null;

            while (step != null)
            {
                if (step.StepType == "MAIN")
                    step.Sequence = index++;
                else
                    step.Sequence = -1;

                prev = step;
                step = step.NextStep as LcdStep;

                if (step != null)
                {
                    if (traversed.ContainsKey(step))
                    {
                        // remove endless loop link ?
                        step.Splits.Clear();

                        break;
                    }

                    traversed.Add(step, null);
                }
            }

            foreach (LcdStep it in proc.Steps)
            {
                SortPrevSteps(it);
            }
        }

        private void SortPrevSteps(LcdStep step)
        {
            if (step.HasJoins && step.Joins.Count > 1)
                step.Joins.Sort(this.ComparePath);
        }

        private int ComparePath(Mozart.SeePlan.DataModel.Transition a, Mozart.SeePlan.DataModel.Transition b)
        {
            if (object.ReferenceEquals(a, b))
                return 0;

            PrpPath x = (PrpPath)a;
            PrpPath y = (PrpPath)b;

            int cmp = x.Type.CompareTo(y.Type);

            if (cmp == 0)
            {
                LcdStep xStep = x.Step as LcdStep;
                LcdStep yStep = y.Step as LcdStep;

                cmp = this.ComparePrevSteps(xStep, yStep);
            }

            return cmp;
        }

        private void DeleteStepNonMapProcess(Process proc, Dictionary<string, LcdStep> steps)
        {
            if (steps == null)
                return;

            List<string> removeKey = new List<string>();

            foreach (KeyValuePair<string, LcdStep> item in steps)
            {
                if (item.Value.Process == null)
                    removeKey.Add(item.Key);
            }

            foreach (string key in removeKey)
            {
                steps.Remove(key);
            }
        }
        private void AddSteps(Process process, List<LcdStep> startingSteps)
        {
            Dictionary<LcdStep, LcdStep> traversed = new Dictionary<LcdStep, LcdStep>();
            foreach (LcdStep step in startingSteps)
            {
                AddStep(process, step, traversed);
            }
        }
        private void AddStep(Process process, LcdStep step, Dictionary<LcdStep, LcdStep> traversed)
        {
            if (traversed.ContainsKey(step))
                return;

            process.RootActivity.Steps.Add(step);
            traversed.Add(step, step);

            if (step.HasSplits == false)
                return;

            foreach (PrpPath path in step.Splits)
                AddStep(process, path.ToStep as LcdStep, traversed);
        }
        private List<LcdStep> FindFirstSteps(Route process, Dictionary<string, LcdStep> steps)
        {
            Dictionary<LcdStep, LcdStep> startingSteps = new Dictionary<LcdStep, LcdStep>();
            foreach (LcdStep step in steps.Values)
            {
                if (step == null || step.HasJoins)
                    continue;
                if (!startingSteps.ContainsKey(step))
                    startingSteps.Add(step, step);
            }

            List<LcdStep> list = new List<LcdStep>(startingSteps.Values);
            list.QuickSort(this.CompareStepSeq);

            return list;
        }

        private LcdStep FindStep(Process proc, Dictionary<string, LcdStep> steps,
            Dictionary<string, PrpInfo> prps,
            string stepSeq, bool exactly)
        {
            LcdStep step;

            if (steps == null)
                return null;

            if (steps.TryGetValue(stepSeq, out step))
            {
                if (exactly && stepSeq != step.StepID)
                    return null;

                return step;
            }

            if (exactly)
                return null;

            PrpInfo prp;
            if (prps.TryGetValue(stepSeq, out prp) == false)
                return null;

            step = FindStep(proc, steps, prps, prp.Pass.StepID, exactly);

            if (step == null)
            {
                Logger.Info(" --- Path - step is null :{2}:{0}-->{1}", prp.StepId, prps, prp.Pass.StepID, proc.ProcessID);
                return null;
            }

            steps.Add(stepSeq, step);

            return step;
        }
    }
}
