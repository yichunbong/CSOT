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
namespace CSOT.Lcd.Scheduling.Logic
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
			//ModelContext.Current.Inputs.UseDatabase = true;

			DownloadMonitor cdm = new CustomDownloadMonitor(task.Context.GetLog(MConstants.LoggerPersists), null);
			UploadMonitor cum = new CustomUploadMonitor(task.Context.GetLog(MConstants.LoggerPersists), null);

			ServiceLocator.RegisterInstance<DownloadMonitor>(cdm);
			ServiceLocator.RegisterInstance<UploadMonitor>(cum);

			InputMart.Instance.RunStateMst = new DataModel.RunStateMaster();
			InputMart.Instance.Dashboard = new DataModel.DashboardMaster();
		}


		/// <summary>
		/// </summary>
		/// <param name="context"/>
		/// <param name="handled"/>
		public void RUN0(Mozart.Task.Execution.ModelContext context, ref bool handled)
		{
			List<string> excuetionModuleNames = new List<string>();
			excuetionModuleNames.Add("Pegging");

			if (InputMart.Instance.GlobalParameters.OnlyBackward == false)
			{
				excuetionModuleNames.Add("Simulation");

				if (InputMart.Instance.GlobalParameters.TargetShopList.Contains(Constants.CellShop))
					excuetionModuleNames.Add("Simulation");
			}

			InputMart.Instance.SimulationRunCount = 0;
			InputMart.Instance.SimulationRunType = SimRunType.TFT_CF;

			foreach (var name in excuetionModuleNames)
			{
				var module = context.GetExecutionModule(name);

				module.Execute(context);

				if (string.Equals(name, "Simulation", StringComparison.CurrentCultureIgnoreCase))
				{
					InputMart.Instance.SimulationRunCount++;
					InputMart.Instance.SimulationRunType = SimRunType.CELL;
				}

				if (context.HasErrors)
					return;
			}
		}



		/// <summary>
		/// </summary>
		/// <param name="task"/>
		/// <param name="name"/>
		/// <param name="runTime"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public VersionInfo SETUP_VERSION1(Mozart.Task.Execution.ModelTask task, string name, DateTime runTime, ref bool handled, Mozart.Task.Execution.VersionInfo prevReturnValue)
		{
			VersionInfo vinfo = new VersionInfo(name, runTime);

			object ver;
			task.Context.Arguments.TryGetValue("versionNo", out ver);

			string forceVer = ver == null ? string.Empty : (string)ver;

			if (string.IsNullOrEmpty(forceVer) || forceVer == "-")
			{
				runTime = CheckRunTime(runTime);

				vinfo.VersionNo = string.Format("{0}_{1}", name, runTime.ToString("yyyyMMddHHmmss"));
			}
			else
				vinfo.VersionNo = forceVer;

			return vinfo;
		}


		private DateTime CheckRunTime(DateTime dt)
		{
			bool isAuto = ModelContext.Current.TaskContext.Scheduled;

			if (dt.Second > 0 && isAuto)
			{
				dt = dt.AddSeconds(-dt.Second);
			}

			return dt;
		}

		/// <summary>
		/// </summary>
		/// <param name="task"/>
		/// <param name="context"/>
		/// <param name="handled"/>
		public void SETUP_QUERY_ARGS1(Mozart.Task.Execution.ModelTask task, Mozart.Task.Execution.ModelContext context, ref bool handled)
		{
			context.QueryArgs["ARG_VERSION_NO"] = ModelContext.Current.VersionNo;
			context.QueryArgs["ARG_VERSION_DATE"] = LcdHelper.DbToString(Mozart.SeePlan.ShopCalendar.SplitDate(context.StartTime));
		}

		/// <summary>
		/// </summary>
		/// <param name="task"/>
		/// <param name="handled"/>
		public void END_SETUP0(Mozart.Task.Execution.ModelTask task, ref bool handled)
		{		
			//task.Context.TaskContext.HostProps.Set("ZipFileName", task.Context.VersionNo);

            WriteQueryArgs(task.Context);
        }

		private void WriteQueryArgs(ModelContext task)
		{
			Logger.MonitorInfo("QueryArgs");
			Logger.MonitorInfo("---------------");

			foreach (var key in task.QueryArgs.Keys)
			{
				var value = task.QueryArgs[key];
				Logger.MonitorInfo("{0} = {1}", key, value);
			}

			Logger.MonitorInfo("---------------");
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		/// <param name="handled"/>
		public void ON_INITIALIZE0(Mozart.Task.Execution.ModelContext context, ref bool handled)
		{
			ActivateDataItem(context);

			

			BopHelper.WriteProductRoute();
		}

		private void ActivateDataItem(ModelContext context)
		{
			//Interface - RTD_RtsPlanningLot DataAction 활성화 옵션
			List<string> list = GetRtdTableLis();

			if (GlobalParameters.Instance.InterfaceRTD)
			{
				ActivateDataItem(context, list);
			}
			else
			{
				DeActivateDataItem(context, list);
			}
		}

		private void ActivateDataItem(ModelContext context, List<string> list)
		{
			foreach (var name in list)
			{
				var item = context.Outputs.GetItem(name);

				if (item != null)
				{
					var dataAction = item.GetAction("Default");
					item.ActiveAction = dataAction;

				}
			}
		}

		private void DeActivateDataItem(ModelContext context, List<string> list)
		{
			foreach (var name in list)
			{
				var item = context.Outputs.GetItem(name);

				if (item != null)
					item.ActiveAction = null;
			}
		}



		/// <summary>
		/// </summary>
		/// <param name="context"/>
		/// <param name="handled"/>
		public void ON_DONE0(Mozart.Task.Execution.ModelContext context, ref bool handled)
		{
			//여기서 OutPut.DataAction을 비활성화 할 경우 DB에 Write 되지 않음, Shoutdown에서 처리
		}

		/// <summary>
		/// </summary>
		/// <param name="task"/>
		/// <param name="handled"/>
		public void SHUTDOWN0(Mozart.Task.Execution.ModelTask task, ref bool handled)
		{
			//Interface - RTD_RtsPlanningLot DataAction 비활성화
			if (GlobalParameters.Instance.InterfaceRTD)
			{
				List<string> list = GetRtdTableLis();
				DeActivateDataItem(task.Context, list);
			}

			if (task.HasErrors)
				InputMart.Instance.RunStateMst.OnError(task.Exception);
			else
				InputMart.Instance.RunStateMst.OnDone();
		}

		private List<string> GetRtdTableLis()
		{
			List<string> list = new List<string>();

			list.Add("RTD_RtsPlanningLot");
			list.Add("RTD_RtsPlanningUnload");
			list.Add("RTD_RtsPlanngLotRecord");
			list.Add("RTD_RtsPlanningChangeSpec");
			list.Add("RTD_RtsPlanningMaskTransfer");
			list.Add("RTD_RtsAutoUpkTraking");
			list.Add("RTD_RtsPcPlanDay");

			return list;
		}
	}
}
