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
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
	[FeatureBind()]
	public partial class SetupControl
	{
		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public Time GET_SETUP_TIME0(AoEquipment aeqp, IHandlingBatch hb, ref bool handled, Time prevReturnValue)
		{
			FabAoEquipment eqp = aeqp as FabAoEquipment;
			FabLot lot = hb.ToFabLot();

			//if (eqp.EqpID == "THCVD300" && lot.LotID == "TH9C3749N00")
			//	Console.WriteLine("B");

			float setupTime = SetupMaster.GetSetupTimeWithAhead(eqp, lot);

			return TimeSpan.FromMinutes(setupTime);
		}

		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="proc"/>
		/// <param name="handled"/>
		public void ON_BEGIN_SETUP0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.AoProcess proc, ref bool handled)
		{
			FabAoEquipment eqp = aeqp.ToFabAoEquipment();
			EqpDispatchInfo info = aeqp.EqpDispatchInfo;
			FabLot lot = proc.Entity as FabLot;

			bool isAheadSetup = eqp.AvailableSetupTime < aeqp.NowDT;
			if (isAheadSetup)
			{
				DispatchLogHelper.UpdateDispatchLogByAheadSetup(eqp, info);
			}
			

			if (eqp.IsAcidConst && eqp.AcidDensity.IsSetupMark)
			{
				DateTime inTime = isAheadSetup ? eqp.AvailableSetupTime : eqp.NowDT;
				
				AcidMaster.ResetAcidDensity(eqp, lot, inTime);
			}

		}

		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="proc"/>
		/// <param name="handled"/>
		public void ON_END_SETUP0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.AoProcess proc, ref bool handled)
		{

		}

		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public LoadInfo SET_LAST_LOADING_INFO1(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, Mozart.SeePlan.DataModel.LoadInfo prevReturnValue)
		{
			if (hb == null)
				return null;

			var lot = hb.Sample;
			return lot.CurrentPlan;
		}

		public ISet<string> GET_NEED_SETUP_CHAMBERS0(AoEquipment aeqp, ChamberInfo[] loadableChambers, IHandlingBatch hb, ref bool handled, ISet<string> prevReturnValue)
		{
			var eqp = aeqp.ToFabAoEquipment();
			var lot = hb.ToFabLot();

			//if (eqp.EqpID == "THCVD300" && lot != null && lot.LotID == "TH011661N0H")
			//	Console.WriteLine("B");

			HashSet<string> list = new HashSet<string>();
			foreach (var c in loadableChambers)
			{				
				var subEqp = eqp.FindSubEqp(c);
				if (subEqp == null)
					continue;

				var setupTime = Time.FromMinutes(subEqp.GetSetupTime(lot));
				if (setupTime <= Time.Zero)
					continue;

				c.SetSetupTime(setupTime);
				list.Add(subEqp.SubEqpID);
			}

			return list;
		}
    }
}
