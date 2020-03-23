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
using Mozart.Task.Framework;

namespace CSOT.Lcd.Scheduling
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
                runState.VERSION_DATE = LcdHelper.GetVersionDate();
                runState.VERSION_TYPE = LcdHelper.GetParameter(context.Arguments, "versionType", "RTS-t6");
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

                runner.INBOUND_STATE = runState.INBOUND_STATE;
                runner.INBOUND_START_TIME = LcdHelper.DbNullDateTime(runState.INBOUND_START_TIME);
                runner.INBOUND_END_TIME = LcdHelper.DbNullDateTime(runState.INBOUND_END_TIME);

                runner.OUTBOUND_STATE = runState.OUTBOUND_STATE;
                runner.OUTBOUND_START_TIME = LcdHelper.DbNullDateTime(runState.OUTBOUND_START_TIME);
                runner.OUTBOUND_END_TIME = LcdHelper.DbNullDateTime(runState.OUTBOUND_END_TIME);

                //runner.ENGINE_STATE = runState.ENGINE_STATE;
                runner.ENGINE_START_TIME = LcdHelper.DbNullDateTime(runState.ENGINE_START_TIME);
                //runner.ENGINE_END_TIME = DbDateTime(runState.ENGINE_END_TIME);

                runner.EXE_OPTION = runState.EXE_OPTION;
                runner.ISCONFIRMED = runState.ISCONFIRMED;

                OutputMart.Instance.JobRunState.Add(runner);
            }

            runner.ENGINE_STATE = RunStateType.P.ToString();

            if (runner.ENGINE_START_TIME == null || runner.ENGINE_START_TIME == DateTime.MinValue)
                runner.ENGINE_START_TIME = LcdHelper.DbNullDateTime(DateTime.Now);

            runner.UPDATE_TIME = DateTime.Now;

            SaveState(runStateMst, null);
        }

        public static void OnDone(this RunStateMaster runStateMst)
        {
            Logger.Info("[Info] EngineState : OnDone");

            var runner = runStateMst.Runner;
            if (runner == null)
                return;

            runner.ENGINE_STATE = RunStateType.C.ToString();
            runner.ENGINE_END_TIME = LcdHelper.DbNullDateTime(DateTime.Now);

            runner.UPDATE_TIME = DateTime.Now;

            //SaveState(runStateMst, null);
        }

        public static void OnError(this RunStateMaster runStateMst, Exception ex)
        {
            Logger.Info("[Info] EngineState : OnError");

            var runner = runStateMst.Runner;
            if (runner == null)
                return;

            runner.ENGINE_STATE = RunStateType.F.ToString();
            runner.ENGINE_END_TIME = LcdHelper.DbNullDateTime(DateTime.Now);

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


    }
}
