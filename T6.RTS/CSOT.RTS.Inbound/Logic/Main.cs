using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan;
using Mozart.SeePlan.DataModel;
using CSOT.RTS.Inbound.Inputs;
using CSOT.RTS.Inbound.Persists;
using CSOT.RTS.Inbound.Outputs;
using CSOT.RTS.Inbound.DataModel;
using Mozart.DataActions;
using System.Diagnostics;

namespace CSOT.RTS.Inbound.Logic
{
    [FeatureBind()]
    public partial class Main
    {
        /// <summary>
        /// </summary>
        /// <param name="task"/>
        /// <param name="handled"/>
        public void BEGIN_SETUP0(ModelTask task, ref bool handled)
        {            
            InputMart.Instance.RunStateMst = new DataModel.RunStateMaster();
            InputMart.Instance.Dashboard = new DataModel.DashboardMaster();
        }

        /// <summary>
        /// </summary>
        /// <param name="task"/>
        /// <param name="name"/>
        /// <param name="runTime"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public VersionInfo SETUP_VERSION1(ModelTask task, string name, DateTime runTime, ref bool handled, VersionInfo prevReturnValue)
        {
            var args = task.Context.Arguments;

            DateTime startTime = LcdHelper.Trim(runTime, "mm");
            if (args.ContainsKey("start-time"))
                startTime = LcdHelper.Trim(LcdHelper.GetArguments(args, "start-time", runTime), "mm");

            args["start-time"] = startTime;
            Logger.MonitorInfo("Reset start_time : {0}", runTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                
            string prefix = LcdHelper.GetParameter(args, "versionType", LcdHelper.DEFAULT_VERSION_TYPE);
            string postfix = LcdHelper.GetParameter(args, "versionPostFix", "");

            VersionInfo info = new VersionInfo(prefix, startTime);

            string versionNo = LcdHelper.GetArguments(args, "versionNo", string.Empty);

            if (string.IsNullOrEmpty(versionNo))
                versionNo = GetVersionInfo(prefix, startTime, postfix);

            info.VersionNo = versionNo;

            task.TaskContext.Arguments["versionNo"] = versionNo;
            Logger.MonitorInfo("Reset versionNo : {0}", versionNo);

            return info;
        }

        private string GetVersionInfo(string prefix, DateTime startTime, string postfix)
        {
            string versionNo = string.Format("{0}-{1}", prefix, startTime.ToString("yyyyMMddHHmmss"));

            if (string.IsNullOrEmpty(postfix) == false)
                versionNo = string.Format("{0}-{1}", versionNo, postfix);

            return versionNo;
        }

        /// <summary>
        /// </summary>
        /// <param name="task"/>
        /// <param name="context"/>
        /// <param name="handled"/>
        public void SETUP_QUERY_ARGS1(ModelTask task, ModelContext context, ref bool handled)
        {
            FactoryConfiguration.Current.Initialize();

            string runServer = LcdHelper.GetArguments(task.Context.Arguments, "RunServer", string.Empty);;         
            string varsionDate = context.StartTime.SplitDate().ToString("yyyyMMdd");

            DateTime planStartTime = context.StartTime;
            DateTime planEndTime = context.EndTime;
            DateTime planStartOfDayT = ShopCalendar.StartTimeOfDayT(planStartTime);

            string actStartTime = LcdHelper.DbToString(planStartOfDayT.AddDays(-1));
            string actEndTime = LcdHelper.DbToString(planStartTime);
            
            var args = context.QueryArgs;

            args["ARG_RUN_SERVER"] = runServer;
            args["ARG_VERSION_DATE"] = varsionDate;
            args["ARG_VERSION_NO"] = task.Context.VersionNo;
            args["ARG_TARGET_SHOP_LIST"] = LcdHelper.GetTargetShopList();
            args["ARG_ACT_START_TIME"] = actStartTime;
            args["ARG_ACT_END_TIME"] = actEndTime;

            args["ARG_PLAN_START_TIME"] = LcdHelper.DbToString(planStartTime);
            args["ARG_PLAN_END_TIME"] = LcdHelper.DbToString(planEndTime);

            args["ARG_ACT_FIXED_DATE"] = LcdHelper.DbToString(LcdHelper.GetActFixedDate_Default());
        }

        /// <summary>
        /// </summary>
        /// <param name="task"/>
        /// <param name="handled"/>
        public void END_SETUP0(ModelTask task, ref bool handled)
        {            
            SetDashboard();

            var dashboard = InputMart.Instance.Dashboard;

            MainHelper.SetActiveAction(dashboard.RunType);

            var context = task.Context;            
            WriteLog(context, context.QueryArgs, "QueryArgs");            
        }

        private void WriteLog(ModelContext context, System.Collections.IDictionary args, string title)
        {
            var logger = Logger.Handler;
            if (logger == null)
                logger = context.GetLog(MConstants.LoggerExecution);

            if (logger == null)
                return;

            logger.MonitorInfo(title);
            logger.MonitorInfo("---------------");

            if (args != null)
            {
                foreach (var key in args.Keys)
                {
                    logger.MonitorInfo("{0} = {1}", key, args[key]);
                }
            }

            logger.MonitorInfo("---------------");
        }

        private void WriteLog(ModelContext context, IDictionary<string, object> args, string title)
        {
            var logger = Logger.Handler;
            if (logger == null)
                logger = context.GetLog(MConstants.LoggerExecution);

            if (logger == null)
                return;

            logger.MonitorInfo(title);
            logger.MonitorInfo("---------------");

            if (args != null)
            {
                foreach (var key in args.Keys)
                {
                    logger.MonitorInfo("{0} = {1}", key, args[key]);
                }
            }

            logger.MonitorInfo("---------------");
        }

        private void SetDashboard()
        {
            var dashboard = InputMart.Instance.Dashboard;

            dashboard.RunType = LcdHelper.GetRunType();
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        /// <param name="handled"/>
        public void RUN1(ModelContext context, ref bool handled)
        {
            Logger.MonitorInfo("Main Run Start : {0}",
                string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));

            var stopwatch = new Stopwatch();            
            stopwatch.Start();

            MainHelper.RUN();

            stopwatch.Stop();
            TimeSpan elapsedTime = new TimeSpan((long)(stopwatch.ElapsedTicks / (Stopwatch.Frequency / 10000000f)));

            Logger.MonitorInfo(string.Format("\t\tElapsed Time : {0}", elapsedTime));

            Logger.MonitorInfo("Main Run End : {0}", 
                string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"/>
        /// <param name="handled"/>
        public void ON_DONE0(ModelContext context, ref bool handled)
        {
            SetReturnResult(context);
        }

        private void SetReturnResult(ModelContext context)
        {
            bool isIfRun = true;

            //no run Dependent trigger(#return-if-true = false)
            if (context.HasErrors)
                isIfRun = false;

            IDictionary<string, object> result = new Dictionary<string, object>();

            result.Add("#return-if-true", isIfRun);
            result.Add("versionNo", context.VersionNo);
            result.Add("start-time", context.StartTime);

            var args = context.Arguments;
            if (args != null)
            {
                //ex)"UI$TargetShopList", "UI$ApplyPMSchedule"
                string prefix = "UI$";
                foreach (var it in args)
                {
                    string uiKey = it.Key;
                    if (uiKey.StartsWith(prefix) == false)
                        continue;

                    string key = uiKey.Replace(prefix, "");
                    if (result.ContainsKey(key))
                        continue;

                    result.Add(key, it.Value);
                }
            }

            context.Result = result;

            WriteLog(context, result, "ReturnResult");
        }
                                
        /// <summary>
        ///
        /// </summary>
        /// <param name="task"/>
        /// <param name="handled"/>
        public void SHUTDOWN0(ModelTask task, ref bool handled)
        {
            if (task.HasErrors)
                InputMart.Instance.RunStateMst.OnError(task.Exception);
            else
                InputMart.Instance.RunStateMst.OnDone();

            Logger.MonitorInfo("Shutdown : {0}",
                string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));
        }
    }
}