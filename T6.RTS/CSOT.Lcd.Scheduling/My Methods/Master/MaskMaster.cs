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
	[FeatureBind()]
	public static partial class MaskMaster
	{
        private static Dictionary<string, List<FabMask>> _locationInfo = new Dictionary<string, List<FabMask>>();

		internal static void AddTool(FabMask mask)
		{
			InputMart.Instance.FabMask.Add(mask.ToolID, mask);

			List<FabMask> list;
			if (_locationInfo.TryGetValue(mask.InitLoacation, out list) == false)
				_locationInfo.Add(mask.InitLoacation, list = new List<FabMask>());

			list.Add(mask);

            mask.AddLoadInfo(mask.InitEqpID, null, true);
        }

		internal static FabMask FindMask(string toolID)
		{
            if (toolID == null)
                return null;

			FabMask mask;
			InputMart.Instance.FabMask.TryGetValue(toolID, out mask);

			return mask;
		}

		internal static void AddToolArrange(FabStdStep stdStep, MaskArrange arr)
		{
			List<MaskArrange> list;
			if (stdStep.ToolArrange.TryGetValue(arr.EqpID, arr.ProductID, out list) == false)
				stdStep.ToolArrange.Add(arr.EqpID, arr.ProductID, list = new List<MaskArrange>());

			list.Add(arr);
		}

		internal static void AddToolArrange(FabMask mask, MaskArrange arr)
		{
			if (mask.AllSteps.Contains(arr.StepID) == false)
				mask.AllSteps.Add(arr.StepID);

			if (mask.AllEqps.Contains(arr.EqpID))
				mask.AllEqps.Add(arr.EqpID);

			List<string> list;
			if (mask.AllProduct.TryGetValue(arr.ProductID, out list) == false)
				mask.AllProduct.Add(arr.ProductID, list = new List<string>());

			if (list.Contains(arr.ProductVersion) == false)
				list.Add(arr.ProductVersion);
		}

        internal static void AddLimit(EqpArrangeInfo info)
        {
            if (info == null)
                return;

            string maskID = info.MaskID;

            var mask = FindMask(maskID);
            if (mask == null)
                return;

            mask.AddLimitInfo(info);
        }
               
		//internal static void InitMask_LocateWip(AoFactory factory, IHandlingBatch hb)
		//{
		//	FabLot lot = hb.ToFabLot();

		//	if (IsUseTool(lot) == false)
		//		return;

		//	if (lot.CurrentFabPlan.IsInitRunWip == false)
		//		return;

		//	var targetEqp = lot.CurrentPlan.LoadedResource as FabEqp;
		//	if (targetEqp == null)
		//		return;

		//	var mask = targetEqp.InitMask;
		//	if (mask == null)
		//		return;

		//	var aeqp = factory.GetEquipment(targetEqp.EqpID) as FabAoEquipment;
		//	var loableList = GetLoadableMaskList(aeqp, lot);

		//	if (loableList.Contains(mask))
		//	{
		//		aeqp.SeizeMaskTool(mask, lot);
		//	}
		//	else
		//	{
		//		ErrHist.WriteIf(mask.ToolID,
		//			ErrCategory.SIMULATION,
		//			ErrLevel.INFO,
		//			targetEqp.FactoryID,
		//			targetEqp.ShopID,
		//			lot.LotID,
		//			lot.CurrentProductID,
		//			lot.CurrentProductVersion ?? lot.Wip.ProductVersion,
		//			lot.CurrentProcessID,
		//			targetEqp.EqpID,
		//			lot.CurrentStepID,
		//			"CAN NOT INIT MASK_TOOL",
		//			string.Format("Eqp InitMask : {0}", mask.ToolID)
		//			);
		//	}
		//}

		internal static void InitLocate(AoEquipment aeqp, IHandlingBatch hb)
		{
			FabLot lot = hb.ToFabLot();
			if (IsUseTool(lot) == false)
				return;

			FabEqp targetEqp = aeqp.Target as FabEqp;

			var mask = targetEqp.InitMask;
			if (mask == null)
				return;

			var loableList = GetLoadableMaskList(aeqp, lot);

			if (loableList.Contains(mask))
			{
				lot.CurrentMask = mask;
			}
			else
			{
				ErrHist.WriteIf(mask.ToolID,
					ErrCategory.SIMULATION,
					ErrLevel.INFO,
					targetEqp.FactoryID,
					targetEqp.ShopID,
					lot.LotID,
					lot.CurrentProductID,
					lot.CurrentProductVersion ?? lot.Wip.ProductVersion,
					lot.CurrentProcessID,
					targetEqp.EqpID,
					lot.CurrentStepID,
					"CAN NOT INIT MASK_TOOL",
					string.Format("Eqp InitMask : {0}", mask.ToolID)
					);
			}
		}
        
		public static FabMask SelectBestMask(AoEquipment aeqp, FabLot lot)
		{
			var eqp = aeqp.ToFabAoEquipment();

			string productVersion = lot.CurrentProductVersion;

			FabMask selectMask = null;

			//연속진행 중인 경우
			if (eqp.IsLastPlan(lot))
			{
				var currMask = eqp.GetCurrentMask();
				if (currMask != null && currMask.IsLoadable_Limit(lot))
				{
					var currEA = lot.CurrentEqpArrange;
					if (currEA != null)
					{
						var last = eqp.GetLastPlan(); //eqp.LastPlan as FabPlanInfo;
						if (currEA.IsLoableByProductVersion(last.ProductVersion))
						{
							selectMask = currMask;
							productVersion = last.ProductVersion;
						}
					}
				}
			}

			if (selectMask == null)
			{
				var loadableList = GetLoadableMaskArrangeList(aeqp, lot, ToolType.MASK);

				if (loadableList == null || loadableList.Count == 0)
					return null;

				if (loadableList.Count > 1)
					loadableList.Sort(new CompareHelper.MaskArrangeComparer(eqp.EqpID));

				var best = loadableList[0];

				selectMask = best.Mask;
				productVersion = best.ProductVersion;
			}

			//2차 선택 (1차 : Mask 미고려 선택, 2차 : Mask 고려 후 선택)
			lot.CurrentEqpArrange.SetUsedArrange(productVersion);

			//if (eqp.NowDT >= LcdHelper.StringToDateTime("20190820153409"))
			//{
			//    if (lot.LotID == "LARRAYI0075" && lot.CurrentStepID == "1300")
			//        Console.WriteLine("B");
			//}

			return selectMask;
		}

		private static List<FabMask> GetLoadableMaskList(AoEquipment aeqp, FabLot lot)
		{
			List<FabMask> list = new List<FabMask>();

			var malist = GetLoadableMaskArrangeList(aeqp, lot, ToolType.MASK);
			if (malist == null || malist.Count == 0)
				return list;

			foreach (var item in malist)
			{
				var mask = item.Mask;
				if (mask == null)
					continue;

				list.Add(mask);
			}

			return list;
		}

		private static List<MaskArrange> GetLoadableMaskArrangeList(AoEquipment aeqp, FabLot lot, ToolType toolType)
		{
			List<MaskArrange> list = new List<MaskArrange>();

			string eqpID = aeqp.EqpID;
			DateTime now = aeqp.NowDT;

			var malist = GetMatchedMaskArrangeList(aeqp, lot, toolType);
			if (malist == null)
				return list;

			foreach (var item in malist)
			{
				var mask = item.Mask;
				if (mask == null)
					continue;

                //EqpArrange 제약 체크(LimitType = 'M')
                if (mask.IsLoadable_Limit(lot) == false)
                    continue;

				//타 설비에서 사용 중인 경우
				var currEqpID = mask.EqpID;
				if (mask.IsBusy && currEqpID != eqpID)
					continue;

				//다른위치에 마스크가 있을 경우(이동시간 체크)
				if (currEqpID != eqpID)
				{
					Time transferTime = GetMaskTransferTime(currEqpID, eqpID);
					DateTime usableTime = (DateTime)(mask.AvailableTime + transferTime);

					if (usableTime > now)
						continue;
				}

				list.Add(item);
			}

			return list;
		}

		public static List<MaskArrange> GetMatchedMaskArrangeList(AoEquipment aeqp, FabLot lot, ToolType toolType)
		{
			var targetEqp = aeqp.Target as FabEqp;
			//string eqpID = aeqp.EqpID;
			//string productID = lot.CurrentProductID;
			string productVer = lot.CurrentProductVersion;

			var step = lot.CurrentFabStep;
			var stdStep = step.StdStep;

			var malist = stdStep.GetMatchedMaskList(targetEqp, lot, toolType);
			if (malist == null)
				return new List<MaskArrange>();

			List<MaskArrange> list = new List<MaskArrange>();

			var currEA = lot.CurrentEqpArrange;
			if (currEA == null)
				return malist;

			bool isFixedProductVer = currEA.EqpArrrangeSet.IsFixedProductVer;
			if (isFixedProductVer)
			{
				foreach (var item in malist)
				{
					if (item.ProductVersion != productVer)
						continue;

					list.Add(item);
				}
			}
			else
			{
				var ealist = currEA.LoadableList;
				if (ealist == null || ealist.Count == 0)
					return malist;

				foreach (var item in malist)
				{
					if (IsUsableMask(ealist, item) == false)
						continue;

					list.Add(item);
				}
			}

			return list;
		}

		private static bool IsUsableMask(List<EqpArrangeInfo> ealist, MaskArrange item)
		{
			if (ealist == null || ealist.Count == 0)
				return true;

			bool isArrayShop = BopHelper.IsArrayShop(item.ShopID);

			bool isUasble = false;
			foreach (var info in ealist)
			{
				if (isArrayShop && info.HasLimitType(LimitType.B))
				{
					if (info.ProductVer != item.ProductVersion)
						continue;
				}

				if (info.HasLimitType(LimitType.M))
				{
					if (info.MaskID != item.ToolID)
						continue;
				}

				isUasble = true;
				break;
			}

			return isUasble;
		}

		private static Time GetMaskTransferTime(string from, string to)
		{
			if (LcdHelper.IsEmptyID(from))
				return Time.FromMinutes(30);

			return Time.FromMinutes(60);
		}

		public static bool IsUseTool(FabLot lot)
		{
			if (InputMart.Instance.GlobalParameters.ApplySecondResource == false)
				return false;

			var step = lot.CurrentFabStep;
			var stdStep = step.StdStep;

			return stdStep.IsUseMask;
		}
               
		public static void StartTask(FabLot lot, AoEquipment aeqp)
		{
			//if (IsUseTool(lot) == false)
			//	return;

			FabAoEquipment eqp = aeqp.ToFabAoEquipment();
			FabMask mask = lot.CurrentMask;
			
			eqp.SeizeMaskTool(mask, lot);

			if (mask != null)
			{
				mask.AddLoadInfo(eqp.EqpID, lot);
				mask.MoveQty(eqp, lot);
			}
		}

		public static void EndTask(FabLot lot, AoEquipment aeqp)
		{
			if (IsUseTool(lot))
			{
				var mask = lot.CurrentMask;
				if (mask != null)
				{
					var loadInfo = mask.FindLoadInfo(lot, aeqp);
					if (loadInfo != null)
						loadInfo.EndTime = aeqp.NowDT;

					mask.AvailableTime = aeqp.Now;
					mask.RemoveWorkInfo(lot);

					lot.CurrentMask = null;
				}
			}
		}

		public static void OnDone(AoFactory aoFactory)
		{
			if (SimHelper.IsCellRunning)
				return;

			foreach (var mask in InputMart.Instance.FabMask.Values)
			{
				if (mask.WorkInfos.Count == 0 && mask.LoadInfos.Count > 0)
				{
					var last = mask.LastPlan;

                    if (mask.EqpID != last.EqpID)
                    {
                        AddLoadInfo(mask, last.EqpID, null);
                    }
				}
			}
		}

        public static void OnDayChanged(DateTime now)
        {
            var list = InputMart.Instance.FabMask.Values;
            foreach (var mask in list)
            {
                if (mask.HasLimit() == false)
                    continue;

                foreach (var item in mask.Limits)
                {
                    if (item.ActivateType != ActivateType.M)
                        continue;

                    if (item.IsDailyMode == false)
                    {
                        OutCollector.WriteLimitMLog(item, null, null, now, "DAY_CHANGE");
                        continue;
                    }

                    item.MoveQty = 0;
                    item.Seq = 0;

                    OutCollector.WriteLimitMLog(item, null, null, now, "RESET");
                }
            }
        }

        #region FabMask Func

		public static void AddWorkInfo(this FabMask mask, FabLot lot)
		{
			if (mask.WorkInfos == null)
				mask.WorkInfos = new HashSet<FabLot>();

			mask.WorkInfos.Add(lot);
		}

		public static void RemoveWorkInfo(this FabMask mask, FabLot lot)
		{
			var isRemove = mask.WorkInfos.Remove(lot);
			if (isRemove == false)
				Logger.Warn("Invalid Mask WorkInfo : MaskID={0}, LotID={1}", mask.ToolID, lot.LotID);
		}

		public static void AddLoadInfo(this FabMask mask, string eqpID, FabLot lot, bool isInit = false)
		{
			MaskLoadInfo info = new MaskLoadInfo();

			if (lot != null)
			{
				info.EqpID = eqpID;
				info.AvailableTime = mask.AvailableTime;
				info.PlanInfo = lot.CurrentFabPlan;
				info.StartTime = AoFactory.Current.NowDT;

				info.LotID = lot.LotID;
				info.StepID = lot.CurrentStepID;
				info.ProductID = lot.CurrentProductID;
				info.ProductVersion = lot.CurrentProductVersion;

				string key = GetMaskLoadInfoKey(info.EqpID, info.LotID, info.StepID);

				if (mask.LoadInfosView.ContainsKey(key) == false)
					mask.LoadInfosView.Add(key, info);
			}
			else
			{
				info.StartTime = (DateTime)mask.AvailableTime;
				info.EndTime = isInit ? info.StartTime : info.StartTime.AddMinutes(GetMaskTransferTime(mask.EqpID, eqpID).TotalMinutes);

				info.EqpID = eqpID;
				info.LotID = isInit ? "INIT" : "MOVE";
				info.StepID = Constants.NULL_ID;
				info.ProductID = Constants.NULL_ID;
				info.ProductVersion = Constants.NULL_ID;
			}

			mask.LoadInfos.Add(info);
		}

		public static MaskLoadInfo FindLoadInfo(this FabMask mask, FabLot lot, AoEquipment eqp)
		{
			var key = GetMaskLoadInfoKey(eqp.EqpID, lot.LotID, lot.CurrentStepID);

			MaskLoadInfo info;
			mask.LoadInfosView.TryGetValue(key, out info);
			return info;
		}

		private static string GetMaskLoadInfoKey(string eqpID, string lotID, string stepID)
		{
			return LcdHelper.CreateKey(eqpID, lotID, stepID);
		}

        private static void AddLimitInfo(this FabMask mask, EqpArrangeInfo info)
        {
            if (info == null)
                return;

            mask.Limits.Add(info);
        }

        private static bool HasLimit(this FabMask mask)
        {
            var infos = mask.Limits;
            if (infos == null || infos.Count == 0)
                return false;

            return true;
        }

        private static void MoveQty(this FabMask mask, FabAoEquipment eqp, FabLot lot)
        {
            if (mask.HasLimit() == false)
                return;

            int qty = lot.UnitQty;
            if (qty <= 0)
                return;

            foreach (var item in mask.Limits)
            {
                if (item.ActivateType != ActivateType.M)
                    continue;

                item.MoveQty += qty;
                item.Seq++;

                OutCollector.WriteLimitMLog(item, eqp, lot, eqp.NowDT);
            }
        }

        private static bool IsLoadable_Limit(this FabMask mask, FabLot lot)        
        {
            if (mask.HasLimit() == false)
                return true;

            bool checkQty = true;
            if (InputMart.Instance.GlobalParameters.ApplyArrangeMType == false)
                checkQty = false;

            int lotSize = SeeplanConfiguration.Instance.LotUnitSize;
            int unitQty = lot.IsDummy ? lotSize : lot.UnitQty;

            int qty = checkQty ? Math.Max(unitQty, 1) : 0;

            var infos = mask.Limits;
            foreach (var item in infos)
            {
                if (item.IsLoadable(qty) == false)
                    return false;
            }

            return true;
        }

        #endregion

        #region FabStdStep Func

		private static List<MaskArrange> GetMatchedMaskList(this FabStdStep stdStep, FabEqp eqp, FabLot lot, ToolType toolType)
		{
			string eqpID = eqp.EqpID;
			string productID = lot.CurrentProductID;

			var toolArrange = toolType == ToolType.MASK ? stdStep.ToolArrange : stdStep.JigArrange;
			List<MaskArrange> list;
			if (toolArrange.TryGetValue(eqpID, productID, out list) == false)
				return null;

			string productVersion = lot.CurrentProductVersion;

			List<MaskArrange> matchList = new List<MaskArrange>();
			foreach (MaskArrange maskArr in list)
			{
				if (stdStep.IsArrayShop)
				{
					bool existX = productVersion.Contains("X") || maskArr.ProductVersion.Contains("X");
					if (stdStep.IsGatePhoto == false || existX)
					{
						if (maskArr.ProductVersion != productVersion)
							continue;
					}
				}

				matchList.Add(maskArr);
			}

			if (matchList == null)
				return null;

			return matchList;
		}

		#endregion

		#region FabAoEquipment Func

		private static FabMask GetCurrentMask(this FabAoEquipment eqp)
		{
			FabMask mask = eqp.InUseMask;

			//다른설비에 Mask가 할당된 경우 내가 사용하는거 아님. 추후 나중에 Detach 기능 구현필요
			if (mask != null && mask.EqpID != eqp.EqpID)
				return null;

			return mask;
		}

		private static void SeizeMaskTool(this FabAoEquipment eqp, FabMask mask, FabLot lot)
		{
			if (eqp.InUseMask != null)
			{
				if (mask == null || mask != eqp.InUseMask)
					eqp.ReleaseMask();
			}

			eqp.InUseMask = mask;

			if (mask != null)
			{
				mask.StateChangeTime = eqp.NowDT;
				mask.StateCode = ToolStatus.INUSE;

				if (mask.EqpID != eqp.EqpID)
				{
					ChangeLocation(mask, mask.EqpID, eqp.EqpID);
				}

				mask.EqpID = eqp.EqpID;
				mask.Location = mask.EqpID;

				mask.AvailableTime = GetNextAvailableTime(eqp, lot);
				mask.AddWorkInfo(lot);
			}
		}

		private static void ChangeLocation(FabMask mask, string fromEqpID, string toEqpID)
		{
			List<FabMask> list;
			if (_locationInfo.TryGetValue(fromEqpID, out list))
				list.Remove(mask);

			List<FabMask> toList;
			if (_locationInfo.TryGetValue(toEqpID, out toList) == false)
				_locationInfo.Add(toEqpID, toList = new List<FabMask>());

			if (toList.Contains(mask) == false)
				toList.Add(mask);

			AddLoadInfo(mask, toEqpID, null);
		}

		private static void ReleaseMask(this FabAoEquipment eqp)
		{
			FabMask oldMask = eqp.InUseMask;

			oldMask.LastReleaseTime = eqp.NowDT;
			oldMask.StateChangeTime = eqp.NowDT;
			oldMask.StateCode = ToolStatus.MOUNT;

			eqp.InUseMask = null;
		}

		private static Time GetNextAvailableTime(FabAoEquipment eqp, FabLot lot)
		{
			AoProcess proc = eqp.Processes[0];

			Time outTime = proc.GetUnloadingTime(lot);

			return outTime;
		}

        #endregion             

    }
}
