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

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class QTimeMaster
	{
		//QTime = FromStep OUT ~ ToStep IN (2019.12.18 - by.liujian(유건))
		private static Dictionary<FabProduct, List<StayHour>> dic = new Dictionary<FabProduct, List<StayHour>>();

		internal static void Add(StayHour sh)
		{
			if (dic == null)
				dic = new Dictionary<FabProduct, List<StayHour>>();

			List<StayHour> list;
			if (dic.TryGetValue(sh.Product, out list) == false)
			{
				list = new List<StayHour>();
				dic.Add(sh.Product, list);
			}

			list.Add(sh);
		}

		private static List<StayHour> GetStayHours(FabProduct prod)
		{
			List<StayHour> list;
			if (dic.TryGetValue(prod, out list))
				return list;

			return null;
		}

		internal static void SetQTimeInfo(FabLot lot, FabStep initStep)
		{
			if (lot == null)
				return;

			FabStep currStep = initStep;
			if (currStep == null)
				return;

			FabProduct prod = lot.FabProduct;
			List<StayHour> finds = QTimeMaster.GetStayHours(prod);
			if (finds == null)
				return;

			QTimeInfo info = CreateHelper.CreateQTimeInfo(lot);
			lot.QtimeInfo = info;

			foreach (var it in finds)
			{
				var clone = it.Clone();
				clone.Lot = lot;

				info.List.Add(clone);
			}

			DateTime now = AoFactory.Current.NowDT;
			info.StepChange(null, currStep, now);
		}

		internal static void StepChange(ILot[] lots, DateTime now)
		{
			if (lots == null || lots.Length == 0)
				return;

			foreach (FabLot lot in lots)
			{
				if (lot.QtimeInfo == null)
					continue;

				var prevStep = lot.PreviousFabStep;
				var currStep = lot.CurrentFabStep;

				lot.QtimeInfo.StepChange(prevStep, currStep, now);

				if(lot.CurrentFabPlan.QTimeInfo == null)
				{
					var dic = lot.CurrentFabPlan.QTimeInfo = new Dictionary<string, List<StayHour>>();					
					dic.Add("MIN", lot.QtimeInfo.MinList);
					dic.Add("MAX", lot.QtimeInfo.MaxList);
				}
			}
		}

		internal static void SetWipStayHours(List<ILot> list)
		{
			if (list == null || list.Count == 0)
				return;

			DateTime planStartTime = ModelContext.Current.StartTime;

			foreach (FabLot lot in list)
			{
				if (lot.QtimeInfo == null)
					continue;

				lot.QtimeInfo.SetWipStayHours(lot.LotID, planStartTime);
			}
		}


		internal static void WriteStayHourHist(FabLot lot)
		{
			if (lot.QtimeInfo == null)
				return;

			DateTime planEndTime = ModelContext.Current.EndTime;

			foreach (StayHour item in lot.QtimeInfo.List)
			{
				if (item.FromStepOutTime == DateTime.MinValue)
					continue;

				if (IsTraverseStep(item, lot) == false)
					continue;
			
				Outputs.QTimeHistory row = new QTimeHistory();

				row.VERSION_NO = ModelContext.Current.VersionNo;
				row.FACTORY_ID = item.FatoryID;

				row.LOT_ID = lot.LotID;
				row.PRODUCT_ID = lot.CurrentProductID;

				row.Q_TYPE = item.QType.ToString();

				row.FROM_SHOP_ID = item.FromStep.ShopID;
				row.FROM_PROCESS_ID = item.FromStep.ProcessID;
				row.FROM_STEP_ID = item.FromStep.StepID;

				row.TO_SHOP_ID = item.ToStep.ShopID;
				row.TO_PROCESS_ID = item.ToStep.ProcessID;
				row.TO_STEP_ID = item.ToStep.StepID;

				row.FROM_STEP_OUT_TIME = item.FromStepOutTime.DbNullDateTime();
				row.TO_STEP_IN_TIME = item.ToStepInTime.DbNullDateTime();

				row.Q_TIME = item.QTime.TotalMinutes;

				DateTime lastStateTime = item.ToStepInTime.IsMinValue() ? planEndTime : item.ToStepInTime;

				var stayTime = (lastStateTime - item.FromStepOutTime).TotalMinutes;

				row.STAY_TIME = LcdHelper.ToRound(stayTime, 3);

				row.ISSUE = "N";
				if (item.QType == QTimeType.MAX)
				{
					if (stayTime > item.QTime.TotalMinutes)
						row.ISSUE = "Y";
				}
				else if (item.QType == QTimeType.MIN)
				{
					if (item.ToStepInTime.IsMinValue() == false)
					{
						if (stayTime < item.QTime.TotalMinutes)
							row.ISSUE = "Y";
					}
				}

				OutputMart.Instance.QTimeHistory.Add(row);
			}

		}

		private static bool IsTraverseStep(StayHour item, FabLot lot)
		{
			bool isInclude = false;
			foreach (var step in item.StepList)
			{
				if (lot.PlanSteps.Contains(step.StepKey))
				{
					isInclude = true;
					break;
				}
			}

			return isInclude;
		}

		#region QTimeInfo Func

		public static bool HasQTime(this QTimeInfo info)
		{
			if (info.List != null && info.List.Count > 0)
				return true;

			return false;
		}

		public static bool HasMinQTime(this QTimeInfo info)
		{
			if (info.MinList != null && info.MinList.Count > 0)
				return true;

			return false;
		}

		public static bool HasMaxQTime(this QTimeInfo info)
		{
			if (info.MaxList != null && info.MaxList.Count > 0)
				return true;

			return false;
		}

		private static void SetWipStayHours(this QTimeInfo info, string lotID, DateTime planStartTime)
		{
			if (info.HasQTime() == false)
				return;

			foreach (var it in info.List)
			{
				string fromStepID = it.FromStep.StepID;
				var finds = InputMart.Instance.WipStayHoursDict.FindRows(lotID, fromStepID);
				if (finds == null)
					continue;

				var find = finds.FirstOrDefault();
				if (find == null)
					continue;

				DateTime fromStepOutTime = LcdHelper.Min(find.FROM_STEP_OUT_TIME, planStartTime);
				if (fromStepOutTime == DateTime.MinValue)
					continue;

				it.FromStepOutTime = fromStepOutTime;
			}
		}

		private static void StepChange(this QTimeInfo info, FabStep prevStep, FabStep currStep, DateTime now)
		{
			var lot = info.Lot;
			if (lot == null)
				return;

			if (info.HasQTime() == false)
				return;

			info.StepChange_Prev(prevStep, now);

			info.SetMatchedList(currStep);
			info.StepChange_Curr(currStep, now);
		}

		private static void StepChange_Prev(this QTimeInfo info, FabStep prevStep, DateTime now)
		{
			if (info.HasMinQTime())
				info.MinList.ForEach(t => t.StepChange_Prev(prevStep, now));

			if (info.HasMaxQTime())
				info.MaxList.ForEach(t => t.StepChange_Prev(prevStep, now));
		}

		private static void StepChange_Curr(this QTimeInfo info, FabStep currStep, DateTime now)
		{
			if (info.HasMinQTime())
				info.MinList.ForEach(t => t.StepChange_Curr(currStep, now));

			if (info.HasMaxQTime())
				info.MaxList.ForEach(t => t.StepChange_Curr(currStep, now));
		}

		private static void SetMatchedList(this QTimeInfo info, FabStep step)
		{
			if (step == null)
				return;

			if (info.HasQTime() == false)
				return;

			info.MinList = info.List.FindAll(t => t.IsMatched(step, QTimeType.MIN));
			info.MaxList = info.List.FindAll(t => t.IsMatched(step, QTimeType.MAX));
		}

		internal static StayHour FindMinimumRemainTime(this QTimeInfo info, DateTime now)
		{
			if (info.HasMaxQTime() == false)
				return null;

			var list = info.MaxList.FindAll(x => x.FromStepOutTime != DateTime.MinValue);
			if (list == null || list.Count == 0)
				return null;

			var lot = info.Lot;
			var currStep = lot.CurrentFabStep;

			if (list.Count > 1)
				list.Sort(new CompareHelper.MaxQTimeComparer(lot, currStep, now, OrderType.ASC));

			return list[0];
		}

		internal static Time GetMinimumRemainTime(this FabLot lot, DateTime now)
		{
			if (lot.QtimeInfo == null)
				return Time.MinValue;

			StayHour sh = lot.QtimeInfo.FindMinimumRemainTime(now);

			if (sh == null)
				return Time.MaxValue;

			return sh.RemainTime(lot, lot.CurrentFabStep, now);
		}	

		internal static StayHour FindMaximumMinHoldTime(this QTimeInfo info, DateTime now)
		{
			if (info.HasMinQTime() == false)
				return null;

			var lot = info.Lot;
			var currStep = lot.CurrentFabStep;

			var list = info.MinList.FindAll(x => x.ToStep == currStep);

			if (list.Count == 0)
				return null;

			if (list.Count > 1)
				list.Sort(new CompareHelper.MinQTimeComparer(lot, currStep, now, OrderType.DESC));

			return list[0];
		}

		#endregion QTimeInfo Func

		#region StayHour Func

		private static StayHour Clone(this StayHour info)
		{
			var clone = info.ShallowCopy();
			clone.Reset();

			return clone;
		}

		private static bool Reset(this StayHour info)
		{
			info.FromStepOutTime = DateTime.MinValue;
			info.CurrStepInTime = DateTime.MinValue;

			return false;
		}

		private static void StepChange_Prev(this StayHour info, FabStep prevStep, DateTime now)
		{
			if (prevStep != null)
			{
				if (prevStep.StepID == info.FromStep.StepID)
					info.FromStepOutTime = now;
									
				if (prevStep.StepID == info.ToStep.StepID)
					info.ToStepInTime = now;
			}
		}

		private static void StepChange_Curr(this StayHour info, FabStep currStep, DateTime now)
		{
			if (currStep != null)
			{
				//FromStepOutTime 미설정된 상태로 중간 Step에 진입한 경우(예외처리)
				if (info.FromStep.IsMainStep && info.FromStepOutTime == DateTime.MinValue)
				{
					if (info.IsMatchedStep(currStep))
					{
						DateTime planStartTime = ModelContext.Current.StartTime;
						info.FromStepOutTime = planStartTime;

						#region WriteErrorHistory. 
						string key = string.Format("Qtime{0}/{1}", info.Lot.LotID, currStep.StepKey);
						ErrHist.WriteIf(key,
								 ErrCategory.SIMULATION,
								 ErrLevel.INFO,
								 currStep.FactoryID,
								 currStep.ShopID,
								 info.Lot.LineID,
								 info.Product.ProductID,
								 info.Lot.CurrentProductVersion,
								 info.Lot.CurrentProcessID,
								 Constants.NULL_ID,
								 currStep.StepID,
								 "NOT FOUND FROM_STEP_OUT_TIME",
								 "Qtime:StepChange_Current"); 
						#endregion
					}
				}

				info.CurrStepInTime = now;
			}
		}

		private static bool IsMatched(this StayHour info, FabStep step, QTimeType qtype)
		{
			if (step == null)
				return false;

			if (info.QType != qtype)
				return false;
		

			return info.IsMatchedStep(step);
		}

		private static bool IsMatchedStep(this StayHour info, FabStep step)
		{
			if (step == null)
				return false;

			var find = info.StepList.Find(t => t.StepID == step.StepID);
			if (find != null)
				return true;			

			return false;
		}

        public static int RemainStepCount(this StayHour info, FabStep currStep)
        {
            //제약 발생 전
            if (info.FromStepOutTime == DateTime.MinValue || info.FromStepOutTime == DateTime.MaxValue)
                return 0;

            if (currStep == null)
                return 0;

            string key = currStep.StepKey;           

            int count = 0;
            if (info.AtfterStepCount.TryGetValue(key, out count) == false)
            {
                bool flag = false;
                foreach (var step in info.StepList)
                {
                    if (flag)
                        count++;

                    if (step.StepID == currStep.StepID)
                        flag = true;
                }
            }

            return count;
        }
        
        public static Time RemainTime(this StayHour info, FabLot lot, FabStep step, DateTime now)
        {
            //제약 발생 전
            if (info.FromStepOutTime == DateTime.MinValue || info.FromStepOutTime == DateTime.MaxValue)
                return Time.MaxValue;
							
			string productID = lot.CurrentProductID;
			Time currStepRunTat = GetRunTat(step, productID);
			Time afterTat = info.GetAfterTat(lot, step);
			Time toStepRunTat = info.GetToStepRunTat(productID);
			Time afterStepNeedTat = currStepRunTat + afterTat - toStepRunTat;

			Time stayTime = (now - info.FromStepOutTime);
			Time remainTime = info.QTime - stayTime - afterStepNeedTat;

            return remainTime;
        }

        private static Time GetAfterTat(this StayHour info, FabLot lot, FabStep currStep)
		{
			if (currStep == null)
				return Time.Zero;

			string key = currStep.StepKey;
			string productID = lot.CurrentProductID;

			Time t;
			if (info.AfterTats.TryGetValue(key, out t) == false)
			{
				bool flag = false;
				foreach (var step in info.StepList)
				{
					if (flag)
					{
						var tat = step.GetTat(productID, true);
						if (tat != null)
							t += Time.FromMinutes(tat.TAT);
					}

					if (step.StepID == currStep.StepID)
						flag = true;
				}
			}

			return t;
		}

		private static Time GetToStepRunTat(this StayHour info, string productID)
		{
			var stepList = info.StepList;
			if (stepList == null || stepList.Count == 0)
				return Time.Zero;

			var last = stepList.LastOrDefault();

			return GetRunTat(last, productID);
		}

		private static Time GetRunTat(FabStep step, string productID)
		{			
			if (step == null)
				return Time.Zero;

			var tat = step.GetTat(productID, true);
			if (tat == null)
				return Time.Zero;

			return tat.RunTat;
		}

		public static Time RemainMinHoldTime(this StayHour info, FabLot lot, FabStep step, DateTime now)
		{
			Time stayTime = now - info.FromStepOutTime;

			Time remainTime = info.QTime - stayTime;

			return remainTime;
		}

		#endregion StayHour Func

	}
}
