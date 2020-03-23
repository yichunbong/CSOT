using System;
using Mozart.Task.Execution;
using Mozart.Task.Framework;
using CSOT.RTS.Inbound.DataModel;

namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class RunStateMasterFunc
    {
        public static Inputs.JobRunMonitor SafeGet(this RunStateMaster runStateMst)
        {
            var context = ModelContext.Current;

            Inputs.JobRunMonitor runState = null;
            if (InputMart.Instance.JobRunMonitor.Rows.Count == 0)
            {                
                runState = new Inputs.JobRunMonitor();
                
                runState.JOB_TYPE = "RTS";
                runState.VERSION_TYPE = LcdHelper.GetParameter(context.Arguments, "versionType", LcdHelper.DEFAULT_VERSION_TYPE);
                runState.VERSION_DATE = LcdHelper.GetVersionDate(); 

                runState.VERSION_NO = context.VersionNo;
                runState.PLAN_START_TIME = context.StartTime;

                runState.CREATE_TIME = DateTime.Now;                                
            }
            else
            {
                runState = InputMart.Instance.JobRunMonitor.Rows[0];
            }

            return runState;
        }

        public static void OnStart(this RunStateMaster runStateMst, Inputs.JobRunMonitor runState)
        {
            if (runState == null)
                return;

            Logger.Info("[Info] InboundState : OnStart");

            var runner = runStateMst.Runner;
            if (runner == null)
            {
                runner = runStateMst.Runner = new Outputs.JobRunState();

                runner.JOB_TYPE = runState.JOB_TYPE;
                runner.VERSION_TYPE = runState.VERSION_TYPE;
                runner.VERSION_DATE = runState.VERSION_DATE;
                runner.VERSION_NO = runState.VERSION_NO;

                runner.PLAN_START_TIME = runState.PLAN_START_TIME;

                runner.INBOUND_STATE = runState.INBOUND_STATE;
                runner.INBOUND_START_TIME = DbDateTime(runState.INBOUND_START_TIME);
                runner.INBOUND_END_TIME = DbDateTime(runState.INBOUND_END_TIME);

                //runner.OUTBOUND_STATE = runState.OUTBOUND_STATE;
                //runner.OUTBOUND_START_TIME = DbDateTime(runState.OUTBOUND_START_TIME);
                //runner.OUTBOUND_END_TIME = DbDateTime(runState.OUTBOUND_END_TIME);

                //runner.ENGINE_STATE = runState.ENGINE_STATE;
                //runner.ENGINE_START_TIME = DbDateTime(runState.ENGINE_START_TIME);
                //runner.ENGINE_END_TIME = DbDateTime(runState.ENGINE_END_TIME);

                runner.EXE_OPTION = runState.EXE_OPTION;
                runner.ISCONFIRMED = runState.ISCONFIRMED;                

                OutputMart.Instance.JobRunState.Add(runner);
            }

            var context = ModelContext.Current;

            if (runner.PLAN_START_TIME == null || runner.PLAN_START_TIME == DateTime.MinValue)
                runState.PLAN_START_TIME = context.StartTime;

            runner.INBOUND_STATE = RunStateType.P.ToString();

            if (runner.INBOUND_START_TIME == null || runner.INBOUND_START_TIME == DateTime.MinValue)
                runner.INBOUND_START_TIME = DbDateTime(DateTime.Now);

            runner.UPDATE_TIME = DateTime.Now;

            SaveState(runStateMst, null);
        }

        public static void OnDone(this RunStateMaster runStateMst)
        {
            Logger.Info("[Info] EngineState : OnDone");

            var runner = runStateMst.Runner;
            if (runner == null)
                return;

            runner.INBOUND_STATE = RunStateType.C.ToString();
            runner.INBOUND_END_TIME = DbDateTime(DateTime.Now);

            runner.UPDATE_TIME = DateTime.Now;

            //SaveState(runStateMst, null);
        }

        public static void OnError(this RunStateMaster runStateMst, Exception ex)
        {
            Logger.Info("[Info] EngineState : OnError");

            var runner = runStateMst.Runner;
            if (runner == null)
                return;

            runner.INBOUND_STATE = RunStateType.F.ToString();
            runner.INBOUND_END_TIME = DbDateTime(DateTime.Now);

            runner.UPDATE_TIME = DateTime.Now;

            if (ex != null)
                runner.DESCRIPTION = ex.ToString();

            //SaveState(runStateMst, ex);
        }

        private static void SaveState(this RunStateMaster runStateMst, Exception ex)
        {
            if (IsAutoRun())
            {
                if (Save())
                    Logger.Info("+save run state : {0}, {1}", runStateMst.State.ToString(), DateTime.Now);
            }
        }

        private static bool Save()
        {
            try
            {
                PropertyDictionary args = new PropertyDictionary();
                ModelContext.Current.Outputs.Save<Outputs.JobRunState>("JobRunState", OutputMart.Instance.JobRunState.Table.Rows, args);

                return true;
            }
            catch (Exception ex)
            {
                var currProcess = System.Diagnostics.Process.GetCurrentProcess();
                string processName = currProcess.ProcessName;

                System.Diagnostics.EventLog.WriteEntry(processName, ex.ToString());

                return false;
            }
        }

        public static bool IsAutoRun()
        {
            return ModelContext.Current.TaskContext.Scheduled;
        }

        private static DateTime? DbDateTime(DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return null;

            return date;
        }
    }
}
