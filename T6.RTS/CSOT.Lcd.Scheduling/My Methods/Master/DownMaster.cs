using CSOT.Lcd.Scheduling.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Mozart.SeePlan.DataModel;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.EnterpriseLibrary;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class DownMaster
	{
		static Dictionary<FabEqp, List<PMSchedule>> _pmDic = new Dictionary<FabEqp, List<PMSchedule>>();
		static Dictionary<FabEqp, List<PMSchedule>> _pmList = new Dictionary<FabEqp, List<PMSchedule>>();

		internal static void AddPM(FabEqp eqp, PMSchedule pm)
		{
			List<PMSchedule> list;
			if (_pmDic.TryGetValue(eqp, out list) == false)
			{
				list = new List<PMSchedule>();
				_pmDic.Add(eqp, list);
			}

			list.Add(pm);
		}


		internal static List<PMSchedule> GetPmList(FabEqp eqp)
		{
			List<PMSchedule> list;
			_pmDic.TryGetValue(eqp, out list);

			if (list == null)
				return list = new List<PMSchedule>();

			return list;
		}



		internal static void AddAdjustPmList()
		{
			DateTime planStartTime = ModelContext.Current.StartTime;
			DateTime planEndTime = ModelContext.Current.EndTime;

			foreach (KeyValuePair<FabEqp, List<PMSchedule>> item in _pmDic)
			{
				FabEqp eqp = item.Key;
				item.Value.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

				PMSchedule lastPMSched = null;
				foreach (PMSchedule pm in item.Value)
				{
					if (pm.EndTime < planStartTime)
						continue;

					if (pm.StartTime > planEndTime)
						break;

					if (lastPMSched != null)
					{
						if (lastPMSched.EndTime >= pm.StartTime && lastPMSched.EndTime < pm.EndTime)
						{
							lastPMSched.EndTime = pm.EndTime;
							continue;
						}
					}

					lastPMSched = pm;

					List<PMSchedule> list;
					if (_pmList.TryGetValue(eqp, out list) == false)
					{
						list = new List<PMSchedule>();
						_pmList.Add(eqp, list);
					}

					list.Add(pm);
				}
			}
		}

		internal static bool AdjustAheadPMProcessing(FabAoEquipment eqp, FabPMSchedule pm)
		{
			if (InputMart.Instance.GlobalParameters.ApplyFlexialePMSchedule == false)
				return false;

			if (pm.IsNeedAdjust == false)
				return false;

			if (pm.AllowAheadTime <= 0)
				return false;

			Time idleTime = eqp.GetIdleTime();

			if (AllowAheadPM(pm, idleTime) == false)
				return false;

			AoScheduleItem aoItem = null;
			foreach (var item in eqp.DownManager.ScheduleTable)
			{
				if (pm.Equals(item.Tag))
				{
					if (item.Value == 1)
					{
						aoItem = item;
						break;
					}
				}
			}

			if (aoItem == null)
				return false;

			float aheadTime = Math.Min((float)idleTime.TotalMinutes, pm.AllowAheadTime);

			var adjSchedule = aoItem.Tag as PeriodSection;
			adjSchedule.EndTime = adjSchedule.EndTime.AddMinutes(-aheadTime);

			pm.AheadStartTime = adjSchedule.StartTime.AddMinutes(-aheadTime);
			eqp.AvailablePMTime = pm.AheadStartTime;

			aoItem.EventTime = adjSchedule.EndTime;
			return true;
		}

		private static bool AllowAheadPM(FabPMSchedule pm, Time idleTime)
		{
			if (pm == null)
				return false;

			if (idleTime.TotalMinutes <= 0)
				return false;

			var pmTime = pm.EndTime - pm.StartTime;
			if (idleTime.TotalMinutes > (pmTime.TotalMinutes / 2))
				return true;

			if (pm.StartTime < AoFactory.Current.NowDT.AddDays(1))
			{
				Time allowIdleTime = SiteConfigHelper.GetAllowAdjustPMIdleTime();
				if (idleTime.TotalMinutes > allowIdleTime)
					return true;
			}
			return false;
		}

		internal static void ModifiyDownScheduleAfterStart(this FabAoEquipment eqp)
		{
			var downManager = eqp.DownManager;
			if (downManager == null)
				return;

			DateTime now = eqp.NowDT;
			var startItems = downManager.GetStartScheduleItems(Time.MaxValue);
			var lastEndTime = Time.MinValue;
			var ignoreBlock = new List<string>();
			foreach (var item in startItems)
			{
				var schedule = item.Tag as PeriodSection;
				var rule = schedule.ScheduleType;
				if (rule != DownScheduleType.ShiftBackward
					&& rule != DownScheduleType.ShiftBackwardStartTimeOnly
					&& rule != DownScheduleType.Cancel)
					continue;

				var componentID = string.Empty;
				var isPM = schedule is PMSchedule;
				if (isPM)
				{
					var pm = schedule as PMSchedule;
					if (pm.PMType == PMType.Component)
						componentID = pm.ComponentID;
				}

				if (!ignoreBlock.Contains(componentID) && eqp.IsBlocked(componentID))
					continue;

				var remainEndTime = eqp.GetRemainTimeToEnd(componentID);
				if (lastEndTime > remainEndTime)
					remainEndTime = lastEndTime;

				var wait = item.EventTime - now;

				if (remainEndTime != Time.MaxValue && wait >= Time.Zero && wait < remainEndTime)
				{
					var adjStartTime = now.AddSeconds(remainEndTime.TotalSeconds);
					var adjEndTime = DateTime.MaxValue;
					var adjSchedule = (PeriodSection)schedule.Clone();
					var adjusted = false;
					switch (rule)
					{
						case DownScheduleType.ShiftBackward:
							adjEndTime = adjStartTime.Add(schedule.Duration);
							adjSchedule.StartTime = adjStartTime;
							adjSchedule.EndTime = adjEndTime;

							downManager.AdjustEvent(item, adjSchedule);
							adjusted = true;
							lastEndTime = adjEndTime - now;
							break;
						case DownScheduleType.ShiftBackwardStartTimeOnly:
							var newDownDuration = schedule.EndTime - adjStartTime;
							downManager.CancelEvent(item);

							if (newDownDuration > TimeSpan.Zero)
							{
								//adjEndTime = now.Add(newDownDuration);
								adjSchedule.StartTime = adjStartTime;
								//adjSchedule.EndTime = adjEndTime;

								downManager.AddEvent(adjSchedule);
								adjusted = true;
								lastEndTime = adjSchedule.EndTime - now;
							}
							break;
						case DownScheduleType.Cancel:
							downManager.CancelEvent(item);
							break;
					}

					if (adjusted && adjSchedule is PMSchedule && eqp.IsParallelChamber)
					{
						var pm = adjSchedule as PMSchedule;
						if (pm.PMType == PMType.Full)
							eqp.Loader.Block();
						else
						{
							var cproc = eqp.Processes[0] as AoChamberProc2;
							var block = true;
							foreach (var c in cproc.Chambers)
							{
								if (c.Label == componentID)
								{
									if (c.BlockEndTime < now && !ignoreBlock.Contains(componentID))
										ignoreBlock.Add(componentID);

									c.BlockEndTime = pm.EndTime;
								}
								else if (c.Active)
									block = false;
							}

							if (block)
								eqp.Loader.Block();
						}
					}
					else
						eqp.Loader.Block();
				}
			}
		}


		internal static bool IsPeriodSection(this PMSchedule pm, DateTime date)
		{
			return pm.StartTime <= date && date <= pm.EndTime;
		}
	}
}