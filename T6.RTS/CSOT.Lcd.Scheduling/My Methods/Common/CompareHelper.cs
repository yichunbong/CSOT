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
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Lcd.DataModel;
using Mozart.SeePlan.Simulation;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class CompareHelper
	{

		public class WeightSumComparer : IComparer<IHandlingBatch>
		{

			private FabAoEquipment Eqp;

			private bool IsLast(FabLot lot)
			{
				return this.Eqp.IsLastPlan(lot);
			}


			public WeightSumComparer(FabAoEquipment eqp)
			{
				this.Eqp = eqp;
			}


			public int Compare(IHandlingBatch x, IHandlingBatch y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				var lotX = x.Sample as FabLot;
				var lotY = y.Sample as FabLot;

				float weightx = lotX != null ? lotX.WeightInfo.GetWeightedSum() : 0.0f;
				float weighty = lotY != null ? lotY.WeightInfo.GetWeightedSum() : 0.0f;

				int cmp = weightx.CompareTo(weighty) * -1;  // desc

				if (cmp == 0)
				{
					var last = this.Eqp.GetLastPlan();
					var prodVer = last != null ? last.ProductVersion : Constants.NULL_ID;
					var stepID = last != null ? last.FabStep.StepKey : Constants.NULL_ID;

					bool isLastX = IsLast(lotX);
					bool isLastY = IsLast(lotY);

					cmp = isLastX.CompareTo(isLastY) * -1;

					if (cmp == 0 && isLastX && isLastY)
						cmp = IsEquals(prodVer, lotX).CompareTo(IsEquals(prodVer, lotY)) * -1;

					if (cmp == 0)
						cmp = IsEquals(lotX.CurrentFabStep.StepKey, lotX).CompareTo(IsEquals(lotY.CurrentFabStep.StepKey, lotY)) * -1; 

					if (cmp == 0)
						cmp = lotX.DispatchInTime.CompareTo(lotY.DispatchInTime) * -1; 

				}

				return cmp;
			}

			private bool IsEquals(string targetString, FabLot lotX)
			{
				return targetString.Equals(lotX.CurrentProductVersion);
			}
		}

		public class FifoDispatcherSort : IComparer<IHandlingBatch>
		{
			public int Compare(IHandlingBatch x, IHandlingBatch y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				FabLot a = x.ToFabLot();
				FabLot b = y.ToFabLot();

				int cmp = x.DispatchInTime.CompareTo(y.DispatchInTime);

				if (cmp == 0)
					cmp = a.Priority.CompareTo(b.Priority);

				if (cmp == 0)
					cmp = a.CurrentProductID.CompareTo(b.CurrentProductID);

				if (cmp == 0)
					cmp = a.CurrentProductVersion.CompareTo(b.CurrentProductVersion);

				return cmp;
			}
		}

		public class WipVarComparer : IComparer<FabLot>
		{
			public int Compare(FabLot x, FabLot y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.DispatchInTime.CompareTo(y.DispatchInTime);

				return cmp;
			}
		}

		public class RunWipComparer : IComparer<FabLot>
		{
			public int Compare(FabLot x, FabLot y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				var wip_x = x.Wip;
				var wip_y = y.Wip;

				int cmp = wip_x.LastTrackInTime.CompareTo(wip_y.LastTrackInTime);

				return cmp;
			}

			public static IComparer<FabLot> Default = new RunWipComparer();
		}

		public class PlanDateComparer : IComparer<MoPlan>
		{

			public int Compare(MoPlan x, MoPlan y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				FabMoPlan a = x as FabMoPlan;
				FabMoPlan b = y as FabMoPlan;

				int cmp = x.DueDate.CompareTo(y.DueDate);

				if (cmp == 0)
					cmp = a.LineType.CompareTo(b.LineType);

				return cmp;
			}


			public static IComparer<MoPlan> Default = new PlanDateComparer();
		}




		public class SetupTimeComparer : IComparer<SetupTime>
		{
			private int GetChangeTypeCount(SetupTime info)
			{
				if (info.ChangeTypeList.Count > 1)
					return 0;
				return 1;
			}
			public int Compare(SetupTime x, SetupTime y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.Priority.CompareTo(y.Priority);

				if (cmp == 0)
					cmp = GetChangeTypeCount(x).CompareTo(GetChangeTypeCount(y));

				if (cmp == 0)
				{
					if (GetChangeTypeCount(x) == 1 && GetChangeTypeCount(y) == 1)
					{
						cmp = x.ChangeTypeList[0].CompareTo(y.ChangeTypeList[0]);
					}
				}

				return cmp;
			}

			public static IComparer<SetupTime> Default = new SetupTimeComparer();

		}

		public class WipHoldInfoCompare : IComparer<Tuple<string, DateTime>>
		{
			public int Compare(Tuple<string, DateTime> x, Tuple<string, DateTime> y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				return y.Item2.CompareTo(x.Item2);
			}
		}

		public class MaxQTimeComparer : IComparer<StayHour>
		{
			private FabLot Lot { get; set; }
			private FabStep Step { get; set; }

			private DateTime NowDT { get; set; }

			private OrderType OrderType { get; set; }

			public MaxQTimeComparer(FabLot lot, FabStep step, DateTime now, OrderType orderType)
			{
				this.Lot = lot;
				this.Step = step;
				this.OrderType = orderType;
			}

			public int Compare(StayHour x, StayHour y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				var remain_x = x.RemainTime(this.Lot, this.Step, this.NowDT);
				var remain_y = y.RemainTime(this.Lot, this.Step, this.NowDT);

				return remain_x.CompareTo(remain_y) * (int)this.OrderType;
			}
		}


		public class MinQTimeComparer : IComparer<StayHour>
		{
			private FabLot Lot { get; set; }
			private FabStep Step { get; set; }
			private OrderType OrderType { get; set; }
			private DateTime Now { get; set; }

			public MinQTimeComparer(FabLot lot, FabStep step, DateTime now, OrderType orderType)
			{
				this.Lot = lot;
				this.Step = step;
				this.OrderType = orderType;
				this.Now = now;
			}

			public int Compare(StayHour x, StayHour y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				var remain_x = x.RemainMinHoldTime(this.Lot, this.Step, this.Now);
				var remain_y = x.RemainMinHoldTime(this.Lot, this.Step, this.Now);

				return remain_x.CompareTo(remain_y) * (int)this.OrderType;
			}
		}

		public class FabLoadInfoComparer : IComparer<FabLoadInfo>
		{
			public int Compare(FabLoadInfo x, FabLoadInfo y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.StartTime.CompareTo(y.StartTime);
				if (cmp == 0)
					cmp = x.EndTime.CompareTo(y.EndTime);

				return cmp;
			}

			public static IComparer<FabLoadInfo> Default = new FabLoadInfoComparer();
		}

		public class SubEqpLoadableUnitComparer : IComparer<FabSubEqp>
		{
			private FabLot Lot { get; set; }

			public SubEqpLoadableUnitComparer(FabLot lot)
			{
				this.Lot = lot;
			}

			private int IsLastPlan(FabSubEqp x)
			{
				if (x.IsLastPlan(this.Lot))
					return 0;

				return 1;
			}

			public int Compare(FabSubEqp x, FabSubEqp y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				//연속진행
				int cmp = IsLastPlan(x).CompareTo(IsLastPlan(y));

				//챔버에 로딩되어 있는 제품의 OutTime
				if (cmp == 0)
					cmp = x.ChamberInfo.OutTime.CompareTo(y.ChamberInfo.OutTime);

				//이름
				if (cmp == 0)
					cmp = x.SubEqpID.CompareTo(y.SubEqpID);

				return cmp;
			}
		}



		public class FabStdStepComparer : IComparer<FabStdStep>
		{

			public int Compare(FabStdStep x, FabStdStep y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				return x.StepSeq.CompareTo(y.StepSeq);
			}

			public static IComparer<FabStdStep> Default = new FabStdStepComparer();
		}

		public class MaskArrangeComparer : IComparer<MaskArrange>
		{
			public string EqpID { get; set; }

			public MaskArrangeComparer(string eqpID)
			{
				this.EqpID = eqpID;
			}

			public int Compare(MaskArrange a, MaskArrange b)
			{
				if (object.ReferenceEquals(a, b))
					return 0;

				var x = a.Mask;
				var y = b.Mask;

				int cmp = 0;

				//1.위치                
				if (cmp == 0)
				{
					int location_x = GetLocationEval(x);
					int location_y = GetLocationEval(y);

					cmp = location_x.CompareTo(location_y);
				}

				//2.대상 Step이 작은순
				if (cmp == 0)
					cmp = x.AllSteps.Count.CompareTo(y.AllSteps.Count);

				//3.대상 Product가 작은순
				if (cmp == 0)
					cmp = x.AllProduct.Keys.Count.CompareTo(y.AllProduct.Keys.Count);

				//대상 Eqp가 작은것
				if (cmp == 0)
					cmp = x.AllEqps.Count.CompareTo(y.AllEqps.Count);

				//4.IDLE 시간이 긴 순
				if (cmp == 0)
					cmp = x.AvailableTime.CompareTo(y.AvailableTime);

				//base sort
				if (cmp == 0)
					cmp = string.Compare(a.ToolID, b.ToolID);

				return cmp;
			}

			private int GetLocationEval(FabMask mask)
			{
				//현재 설비 위치
				if (mask.EqpID == this.EqpID)
				{
					//사용 중
					if (mask.StateCode == ToolStatus.INUSE)
						return 0;

					return 1;
				}

				//대기 상태
				if (mask.StateCode == ToolStatus.WAIT)
				{
					//설비 밖에 위치
					if (string.IsNullOrEmpty(mask.EqpID))
						return 2;

					//타 설비에 위치
					return 3;
				}

				return 99;
			}
		}

		public class CellBomComparer : IComparer<CellBom>
		{
			public int Compare(CellBom x, CellBom y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.ActionType.CompareTo(y.ActionType);

				return cmp;
			}
		}

		public class EqpRecipeTimeComparer : IComparer<EqpRecipeInfo>
		{
			public int Compare(EqpRecipeInfo x, EqpRecipeInfo y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;
				int cmp = x.Flag.CompareTo(y.Flag);

				if (cmp == 0)
					cmp = x.DueDate.CompareTo(y.DueDate);

				if (cmp == 0)
					cmp = x.ProductVersion.CompareTo(y.ProductVersion);

				return cmp;

			}
		}

		public class ShopInTargetComparer : IComparer<ShopInTarget>
		{
			public int Compare(ShopInTarget x, ShopInTarget y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.TargetDate.CompareTo(y.TargetDate);

				if (cmp == 0)
					cmp = x.TargetQty.CompareTo(y.TargetQty) * -1;

				if (cmp == 0)
					cmp = x.ProductID.CompareTo(y.ProductID);

				return cmp;
			}

			public static ShopInTargetComparer Default = new ShopInTargetComparer();
		}

		public class SetupTimeIdleComparer : IComparer<SetupTimeIdle>
		{
			public int Compare(SetupTimeIdle x, SetupTimeIdle y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.IDLE_TIME.CompareTo(y.IDLE_TIME) * -1;

				if (cmp == 0)
					cmp = x.SETUP_TIME.CompareTo(y.SETUP_TIME) * -1;

				return cmp;
			}
		}

		public class RtsUnloadLotComparer : IComparer<FabPlanInfo>
		{
			public int Compare(FabPlanInfo x, FabPlanInfo y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.TrackInTime.CompareTo(y.TrackInTime);

				if (cmp == 0)
					cmp = x.StartTime.CompareTo(y.StartTime);

				if (cmp == 0)
					cmp = x.ProductID.CompareTo(y.ProductID);

				if (cmp == 0)
					cmp = x.ProductVersion.CompareTo(y.ProductVersion);

				return cmp;
			}
		}
	}
}
