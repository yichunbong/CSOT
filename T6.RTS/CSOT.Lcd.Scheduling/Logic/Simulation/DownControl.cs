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
using Mozart.Simulation.Engine;
using Mozart.Simulation.EnterpriseLibrary;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
	[FeatureBind()]
	public partial class DownControl
	{
		/// <summary>
		/// </summary>
		/// <param name="fe"/>
		/// <param name="aeqp"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public IEnumerable<Mozart.SeePlan.DataModel.PMSchedule> GET_PMLIST0(PMEvents fe, AoEquipment aeqp, ref bool handled, IEnumerable<PMSchedule> prevReturnValue)
		{
			FabEqp eqp = aeqp.Target as FabEqp;

			return eqp.PMList;

		}

		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="fs"/>
		/// <param name="det"/>
		/// <param name="handled"/>
		public void ON_PMEVENT3(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.DataModel.PMSchedule fs, Mozart.SeePlan.Simulation.DownEventType det, ref bool handled)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			if (eqp.SetParallelChamberPM(fs, det))
			{
				if (det == DownEventType.Start)
					eqp.OnStateChanged(LoadingStates.PM);

				return;
			}

			if (det == DownEventType.Start)
			{
				ResHelper.SetLastLoadingInfo(aeqp, null);

				aeqp.Loader.Block();
				aeqp.WriteHistory(LoadingStates.PM);

				FabPMSchedule pm = fs as FabPMSchedule;

				DownMaster.AdjustAheadPMProcessing(eqp, pm);

				//PM의 경우 OnStateChange 함수를 별도로 호출 필요함.
				LoadingStates state = GetPMLoadingState(pm.Type);
				eqp.OnStateChanged(state);

				FabLoadInfo loadInfo = eqp.LoadInfos.Last();

				if (loadInfo.State == LoadingStates.PM)
				{
					if (pm.Type == ScheduleType.RENT)
						loadInfo.StateInfo = "RENT";

					if (loadInfo.StateInfo != "AHEAD" && LcdHelper.IsEmptyID(pm.Description) == false)
						loadInfo.StateInfo = pm.Description;
				}

			}
			else
			{
				aeqp.Loader.Unblock();
				aeqp.WriteHistoryAfterBreak();
				aeqp.SetModified();
				eqp.OnStateChanged(LoadingStates.IDLE);

				eqp.AvailablePMTime = DateTime.MaxValue;
			}
		}

		private LoadingStates GetPMLoadingState(ScheduleType type)
		{
			if (type == ScheduleType.PM)
				return LoadingStates.PM;
			else if (type == ScheduleType.RENT)
				return LoadingStates.PM;

			return LoadingStates.DOWN;
		}



		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="fs"/>
		/// <param name="det"/>
		/// <param name="handled"/>
		public void ON_FAILURE_EVENT1(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.DataModel.FailureSchedule fs, Mozart.SeePlan.Simulation.DownEventType det, ref bool handled)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			if (det == DownEventType.End)
			{
				aeqp.Loader.Unblock();
				aeqp.WriteHistoryAfterBreak();
			}
			else
			{
				ResHelper.SetLastLoadingInfo(aeqp, null);

				aeqp.WriteHistory(LoadingStates.DOWN);
				aeqp.Loader.Block();

				//eqp.AddLoadPlan(LoadStates.DOWN, aeqp.NowDT);
				eqp.OnStateChanged(LoadingStates.DOWN);
			}
		}
	}
}
