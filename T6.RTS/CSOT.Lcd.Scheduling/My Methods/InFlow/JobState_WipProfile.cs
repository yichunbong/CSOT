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
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling
{
	public partial class JobState : IEquatable<JobState>
	{

		private string GetWipProfileKey(FabStep step, string productVer)
		{
			return LcdHelper.CreateKey(step.StepKey, productVer);
		}


		public WipProfile GetWipProfile(FabStep step, string productVer)
		{
			string key = GetWipProfileKey(step, productVer);

			WipProfile wipProfile;
			_wipProfile.TryGetValue(key, out wipProfile);

			if (wipProfile == null)
				return null;

			wipProfile.CalcProfile();

			return wipProfile;
		}


		public void CalcWipProfile(FabStep step, FabWeightPreset wp, AoEquipment inputEqp)
		{

			StepRouteInfo info = GetStepRouteInfo(step);

			if (info == null)
				return;


			foreach (var prodVer in info.VersionList)
			{
				string key = GetWipProfileKey(step, prodVer);

				WipProfile wipProfile = CreateWipProfile(step, prodVer, 0, wp, inputEqp, 0, false);
				_wipProfile[key] = wipProfile;
			}

		}


		public WipProfile CreateWipProfile(
			FabStep step,
			string productVersion,
			int exCludeStepCnt,
			FabWeightPreset wp,
			AoEquipment inputEqp,
			decimal allowRundonwTime,
			bool excludePreRun = false
			)
		{

			decimal percent = 1;
			List<JobState> jobList = new List<JobState>(1);
			jobList.Add(this);

			string prodVer = GetProductVersion(step, productVersion);

			int loadedEqpCount = GetLoadedEqpCount(step, prodVer, false);

			WipProfile profile = new WipProfile(this.ProductID, step, loadedEqpCount, wp);

			AddTargetWaitWip(profile, jobList, step, prodVer, exCludeStepCnt, inputEqp, allowRundonwTime);

			int stepCount = 1;
			FabStep firstStep = step;
			foreach (FabStep rStep in GetPrevSteps(step))
			{
				//if (step.IsCFShop)
				//    prodVer = Constants.NULL_ID;
				//else
				//    prodVer = GetProductVersion(rStep, prodVer);

				decimal qtyRun = 0, qtyWait = 0, tatRun = 0, tatWait = 0, tact = 0;

				bool isExclude = stepCount <= exCludeStepCnt;
				if (isExclude == false)
				{
					if (stepCount == 1 && excludePreRun)
						qtyRun = 0;
					else
						qtyRun = GetStepWips(jobList, rStep, WipType.Run, prodVer);

					qtyWait = GetStepWips(jobList, rStep, WipType.Wait, prodVer);
				}

				tatRun = GetAverageFlowTime(jobList, rStep, prodVer);
				tatWait = GetAverageWaitTAT(jobList, rStep, prodVer);
                
				tact = GetAverageTactTime(jobList, rStep, prodVer);
				int eqpCount = GetLoadedEqpCount(jobList, rStep, prodVer, false);
				if (eqpCount > 1)
					tact /= eqpCount;

				if (rStep.HasStepTime == false)
				{
					StepTat tat = rStep.GetTat(this.ProductID, true);
					if (tat != null)
					{
						tatRun = (decimal)tat.RunTat * 60;
						tatWait = (decimal)tat.WaitTat * 60;
						tact = (decimal)(tat.TAT * 60) / SeeplanConfiguration.Instance.LotUnitSize;
					}
				}

                decimal stayTime = 0;
                if (qtyWait > 0)
                    stayTime = GetStayWaitTime(jobList, rStep, prodVer);

                WipStep wipStep = new WipStep(rStep, tact, tatRun, tatWait, percent * qtyRun, percent * qtyWait, stayTime);
				profile.AddWipStep(wipStep);


				firstStep = step;
				stepCount++;
			}

			WipStep inPlanWipStep = new WipStep();
			inPlanWipStep.AddReleaePlan(this, firstStep, inputEqp);


			if (inPlanWipStep.Pts.Count > 0)
				profile.AddWipStep(inPlanWipStep);

			return profile;
		}

		private decimal GetStayWaitTime(List<JobState> jobList, FabStep rStep, string prodVer)
		{
			var waitList = GetStepWipList(rStep, WipType.Wait, prodVer);

			DateTime minDate = DateTime.MaxValue;
			foreach (var lot in waitList)
			{
				DateTime targetTime = lot.DispatchInTime;

				if (lot.CurrentState == EntityState.HOLD)
					targetTime =  lot.HoldStartTime.AddMinutes(lot.HoldTime.TotalMinutes);

				if (targetTime == DateTime.MinValue)
					continue; //나오면안됨.

				if (targetTime < minDate)
					minDate = targetTime;
			}

			if (minDate == DateTime.MaxValue)
				return 0;

			if (AoFactory.Current.NowDT < minDate)
				return 0;

			var stay = AoFactory.Current.NowDT - minDate;
			if(stay.TotalSeconds > 0)
				Console.WriteLine();

			return Convert.ToDecimal(stay.TotalSeconds);
		}

		private void AddTargetWaitWip(WipProfile profile,
			List<JobState> jobList,
			FabStep step,
			string prodVer,
			int exCludeStepCnt,
			AoEquipment inputEqp,
			decimal allowRunDonwTime
			)
		{
            FabAoEquipment eqp = inputEqp.ToFabAoEquipment();
            decimal tact = GetAverageTactTime(step, prodVer);			

			if (exCludeStepCnt > 0) //자신의 Wait 제외시
			{
				WipStep ws = new WipStep(step, tact, 0);
				profile.AddWipStep(ws);
			}
			else
			{
                decimal stepWaitQty = GetCurrenStepWaitWipQty(eqp, step, prodVer, allowRunDonwTime);

				WipStep ws = new WipStep(step, tact, stepWaitQty);
				profile.AddWipStep(ws);
			}
		}

		private string GetProductVersion(FabStep step, string productVersion)
		{
			string prodVer = productVersion;

			if (step.IsGatePhoto || step.NeedVerCheck == false || step.IsCFShop)
				prodVer = Constants.NULL_ID;

			return prodVer;
		}

		private int GetLoadedEqpCount(List<JobState> jobList, FabStep step, string productVersion, bool recalculate)
		{
			if (jobList == null || jobList.Count == 0)
				return 0;

			int cnt = 0;
			foreach (JobState jobState in jobList)
				cnt += jobState.GetLoadedEqpCount(step, productVersion, recalculate);

			return cnt;
		}

		public decimal GetAverageTactTime(FabStep step, string productVersion)
		{
			StepRouteInfo dsi = GetStepRouteInfo(step);

			if (dsi == null)
				return 0;

			return dsi.GetTactSec(productVersion);
		}



		public decimal GetAverageFlowTime(FabStep step, string productVersion)
		{
			StepRouteInfo dsi = GetStepRouteInfo(step);

			if (dsi == null)
				return 0;

			return dsi.GetRunTAT(productVersion);
		}

		public decimal GetAverageWaitTAT(FabStep step, string productVersion)
		{
			StepRouteInfo dsi = GetStepRouteInfo(step);

			if (dsi == null)
				return 0;

			return dsi.GetWaitTAT(productVersion);
		}

		private decimal GetAverageTactTime(List<JobState> jobList, FabStep step, string productVersion)
		{
			if (jobList == null || jobList.Count == 0)
				return 0;

			decimal tactSum = 0;
			int count = 0;
			foreach (JobState jobState in jobList)
			{
				decimal avgTactTime = jobState.GetAverageTactTime(step, productVersion);

				if (avgTactTime <= 0)
					continue;

				tactSum += avgTactTime;
				count++;
			}

			if (count == 0)
				return 0;

			return tactSum / count;
		}

		private decimal GetAverageFlowTime(List<JobState> jobList, FabStep step, string productVersion)
		{
			if (jobList == null || jobList.Count == 0)
				return 0;

			decimal flowTimeSum = 0;
			int count = 0;
			foreach (JobState jobState in jobList)
			{
				decimal avgFlowTime = jobState.GetAverageFlowTime(step, productVersion);

				if (avgFlowTime <= 0)
					continue;

				flowTimeSum += avgFlowTime;
				count++;
			}

			if (count == 0)
				return 0;

			return flowTimeSum / count;

		}

		private decimal GetAverageWaitTAT(List<JobState> jobList, FabStep step, string productVersion)
		{
			if (jobList == null || jobList.Count == 0)
				return 0;

			decimal waitTATSum = 0;
			int count = 0;
			foreach (JobState jobState in jobList)
			{
				decimal avgWaitTAT = jobState.GetAverageWaitTAT(step, productVersion);

				if (avgWaitTAT <= 0)
					continue;

				waitTATSum += avgWaitTAT;
				count++;
			}

			if (count == 0)
				return 0;

			return waitTATSum / count;

		}


	}
}
