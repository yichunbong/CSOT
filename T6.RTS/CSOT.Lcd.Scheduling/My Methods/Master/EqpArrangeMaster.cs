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
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class EqpArrangeMaster
	{
		private static Dictionary<string, List<EqpArrangeInfo>> AllArrange = new Dictionary<string, List<EqpArrangeInfo>>();
		public static Dictionary<string, EqpArrangeSet> Infos = new Dictionary<string, EqpArrangeSet>();

		private static EqpArrangeSet ImportEqpArrangeSet(string key, string eqpID, FabEqp targetEqp, string stepID,
			string productID, string productVer, FabStdStep stdStep, bool isFixedProductVer, bool isSubEqp)
		{
			//SubEqp의 경우는 defaultArrange 미지정
			string defaultArrange = isSubEqp ? null : stdStep.DefaultArrange;

			EqpArrangeSet setInfo = new EqpArrangeSet
			{
				Key = key,
				EqpID = eqpID,
				TargetEqp = targetEqp,
				StepID = stepID,
				ProductID = productID,
				ProductVer = productVer,
				StdStep = stdStep,
				DefaultArrange = defaultArrange,
				DefaultLimitTypeList = LcdHelper.ParseLimitType(defaultArrange),
				IsFixedProductVer = isFixedProductVer,
				IsSubEqp = isSubEqp
			};

			var list = GetEqpArrangeInfo(eqpID);

			if (list == null || list.Count() == 0)
				return setInfo;

			foreach (var arr in list)
			{
				
				if (arr.ActivateType == ActivateType.NONE)
					continue;

				//MainRunStep이 등록된 설비의 경우, 타 Step의 M은 무시(2019.06.16 - by.liujian(유건))
				if (arr.ActivateType == ActivateType.M)
				{
					var mainRunStep = targetEqp.MainRunSteps;
					if (mainRunStep != null && mainRunStep.Count > 0)
					{
						if (mainRunStep.Find(t => t.StepID == stepID) == null)
							continue;
					}
				}

				if (arr.IsMatched(stepID, productID, productVer, isFixedProductVer))
					setInfo.AddItem(arr);
			}

			return setInfo;
		}

		private static List<EqpArrangeInfo> GetEqpArrangeInfo(string eqpID)
		{
			List<EqpArrangeInfo> list;
			AllArrange.TryGetValue(eqpID, out list);

			return list;
		}
        
        internal static void AddEqpArrangeInfo(EqpArrangeInfo info)
        {
            var infos = EqpArrangeMaster.AllArrange;

            List<EqpArrangeInfo> list;
            if (infos.TryGetValue(info.EqpID, out list) == false)
            {
                list = new List<EqpArrangeInfo>();
                infos.Add(info.EqpID, list);
            }

            list.Add(info);
        }

		private static EqpArrangeSet GetEqpArrangeSet(string eqpID, FabEqp targetEqp, FabStdStep stdStep,
			string productID, string productVer, bool isSubEqp = false)
		{
			string stepID = stdStep.StepID;

			string key = CreateKey_EqpArrangeSet(eqpID, stepID, productID, productVer);

			//check cache
			EqpArrangeSet info;
			if (Infos.TryGetValue(key, out info) == false)
			{
				bool isFixedProductVer = IsFixedProductVer(stdStep, productVer);

				info = ImportEqpArrangeSet(key, eqpID, targetEqp, stepID, productID, productVer, stdStep, isFixedProductVer, isSubEqp);
				Infos.Add(key, info);

				return info;
			}

			return info;
		}

		public static List<string> GetLoadableEqpList(FabLot lot)
		{
			var step = lot.CurrentFabStep;
			var stdStep = step.StdStep;

			string productID = lot.CurrentProductID;
			string productVer = lot.CurrentProductVersion;

			List<string> list = new List<string>();
						
			var eqps = GetLoadableEqpList(stdStep, productID, productVer);
			foreach (string eqpID in eqps)
			{
				//2020.02.28 StepTime 정보 미존재시 투입 불가
				var st = step.GetStepTime(eqpID, productID);
				if (st == null || st.TactTime <= 0)
					continue;

				list.Add(eqpID);
			}

			return list;
		}

		public static List<string> GetLoadableEqpList(FabStdStep stdStep, string productID, string productVer)
		{
			List<string> list = new List<string>();
			if (stdStep == null)
				return list;

			var eqps = stdStep.AllEqps;
			if (eqps == null || eqps.Count == 0)
				return list;

			foreach (var eqp in eqps)
			{
				var setInfo = GetEqpArrangeSet(eqp.EqpID, eqp, stdStep, productID, productVer);

				if (setInfo.IsLoadable())
					list.Add(eqp.EqpID);
			}

			return list;
		}

		public static List<EqpArrangeSet> GetArrangeSet(FabStdStep stdStep, string productID, string productVer)
		{
			List<EqpArrangeSet> list = new List<EqpArrangeSet>();
			if (stdStep == null)
				return list;

			var eqps = stdStep.AllEqps;
			if (eqps == null || eqps.Count == 0)
				return list;

			foreach (var eqp in eqps)
			{
				var setInfo = GetEqpArrangeSet(eqp.EqpID, eqp, stdStep, productID, productVer);

				list.Add(setInfo);
			}

			return list;
		}

		public static List<string> GetProdVerList(FabStdStep stdStep, string productID, string productVer)
		{
			List<EqpArrangeSet> arrSet = GetArrangeSet(stdStep, productID, productVer);

			HashSet<string> list = new HashSet<string>();

			foreach (var set in arrSet)
			{
				foreach (var arr in set.DefaultGroups.Values)
				{
					foreach (var item in arr)
						list.Add(item.ProductVer);
				}
			}

			return list.ToList();
		}

		public static List<string> GetStepAssignedEqps(string stepID)
		{
			List<string> list = new List<string>();

			//bong - 현재 미사용 중

			return list;
		}

		public static bool IsLoadable(FabAoEquipment eqp, FabLot lot, bool checkQty = true, bool setCurrEA = true)
		{
			var step = lot.CurrentFabStep;
			var stdStep = step.StdStep;

			//if (eqp.NowDT >= LcdHelper.StringToDateTime("20190820153409"))
			//{
			//    if (lot.LotID == "LARRAYI0075" && lot.CurrentStepID == "1300")
			//        Console.WriteLine("B");
			//}

			string eqpID = eqp.EqpID;
			string productID = lot.CurrentProductID;
			string productVer = lot.CurrentProductVersion;

			var targetEqp = eqp.Target as FabEqp;

			var setInfo = GetEqpArrangeSet(eqpID, targetEqp, stdStep, productID, productVer);
			if (setInfo == null)
				return false;

			bool isLastPlan = eqp.IsLastPlan(lot);

			List<EqpArrangeInfo> loadableList;
			bool isLoadable = setInfo.IsLoadable(lot, checkQty, isLastPlan, out loadableList);

			if (isLoadable)
			{
				if (setCurrEA)
				{
					lot.CurrentEqpArrange = new CurrentArrange()
					{
						EqpArrrangeSet = setInfo,
						LoadableList = loadableList,
						UsedEqpArrange = null
					};
				}
			}

			return isLoadable;
		}

		public static bool IsLoadable_CheckOnly(FabAoEquipment eqp, FabLot lot, bool checkQty = true)
		{
			return IsLoadable(eqp, lot, checkQty, false);
		}
		public static bool IsLoadable_ParallelChamber(FabSubEqp subEqp, FabLot lot, bool checkQty = true)
		{
			if (InputMart.Instance.GlobalParameters.ApplyChamberArrange == false)
				return true;

			var step = lot.CurrentFabStep;
			var stdStep = step.StdStep;

			string eqpID = subEqp.SubEqpID;
			string productID = lot.CurrentProductID;
			string productVer = lot.CurrentProductVersion;

			var targetEqp = subEqp.Parent as FabEqp;

			var setInfo = GetEqpArrangeSet(eqpID, targetEqp, stdStep, productID, productVer, true);
			if (setInfo == null)
				return false;

			bool isLastPlan = subEqp.IsLastPlan(lot);

			List<EqpArrangeInfo> loadableList;
			bool isLoadable = setInfo.IsLoadable(lot, checkQty, isLastPlan, out loadableList);
			if (isLoadable)
			{
				lot.CurrentEqpArrange = new CurrentArrange()
				{
					EqpArrrangeSet = setInfo,
					LoadableList = loadableList,
					UsedEqpArrange = null
				};
			}

			return isLoadable;
		}

		public static void OnDispatched(FabAoEquipment eqp, FabLot lot)
		{
			var currEqpArrange = lot.CurrentEqpArrange;
			if (currEqpArrange == null)
				return;

			var setInfo = currEqpArrange.EqpArrrangeSet;
			if (setInfo != null)
				setInfo.MoveQty(eqp, lot);
		}

		public static void OnDayChanged(DateTime now)
		{
			foreach (var list in AllArrange.Values)
			{
				foreach (var item in list)
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

		public static void WriteHistory_Init()
		{

		}

		#region EqpArrangeSet Func

		private static bool HasItems(this EqpArrangeSet setInfo)
		{
			var items = setInfo.Items;
			if (items != null && items.Count > 0)
				return true;

			return false;
		}

		private static bool HasDefaultGroups(this EqpArrangeSet setInfo)
		{
			var group = setInfo.DefaultGroups;
			if (group != null && group.Count > 0)
				return true;

			return false;
		}

		private static bool HasEtcGroups(this EqpArrangeSet setInfo)
		{
			var group = setInfo.EtcGroups;
			if (group != null && group.Count > 0)
				return true;

			return false;
		}

		private static void AddItem(this EqpArrangeSet setInfo, EqpArrangeInfo item)
		{
			if (item == null)
				return;

			//All Items
			LcdHelper.AddSort(setInfo.Items, item, EqpArrangeInfoComparer.Default);

			//Group Items
			//SubEqp는 defaultArrange 미지정, 모두 Default로 처리(2019.09.07)
			bool isDefault = setInfo.IsSubEqp ? true : LcdHelper.Equals(item.LimitType, setInfo.DefaultArrange);

			var groups = isDefault ? setInfo.DefaultGroups : setInfo.EtcGroups;
			string groupKey = item.GetGroupKey();

			List<EqpArrangeInfo> list;
			if (groups.TryGetValue(groupKey, out list) == false)
				groups.Add(groupKey, list = new List<EqpArrangeInfo>());

			LcdHelper.AddSort(list, item, EqpArrangeInfoComparer.Default);
		}

		private static bool IsLoadable(this EqpArrangeSet setInfo)
		{
			List<EqpArrangeInfo> loadableList = new List<EqpArrangeInfo>();

			//M수량 제약 체크 X
			int qty = 0;

			return setInfo.IsLoadable(ref loadableList, qty);
		}

		private static bool IsLoadable(this EqpArrangeSet setInfo, FabLot lot, bool checkQty, bool isLastPlan,
			out List<EqpArrangeInfo> loadableList)
		{
			loadableList = new List<EqpArrangeInfo>();

			int lotSize = SeeplanConfiguration.Instance.LotUnitSize;
			int unitQty = lot.IsDummy ? lotSize : lot.UnitQty;

			int qty = checkQty ? Math.Max(unitQty, 1) : 0;

			//연속 투입인 경우에만 소량 투입 허용(2019.09.06 - by.liujian(유건))
			if (checkQty && isLastPlan == false)
				qty = Math.Max(qty, lotSize);

			return setInfo.IsLoadable(ref loadableList, qty);
		}

		private static bool IsLoadable(this EqpArrangeSet setInfo,
			ref List<EqpArrangeInfo> loadableList, int qty = 0)
		{
			//Default Group은 반드시 존재해야 함.
			if (setInfo.HasDefaultGroups() == false)
				return false;

			List<EqpArrangeInfo> list = new List<EqpArrangeInfo>();

			//Default Group
			var dgroup = setInfo.DefaultGroups;
			bool isLoadable = IsLoadable(dgroup, ref list, qty);
			if (isLoadable == false)
				return false;

			//Etc Group (추가 비가용 체크, 일치하는 비가용 정보 존재시 비가용 처리)
			if (setInfo.HasEtcGroups())
			{
				var egroup = setInfo.EtcGroups;
				list.RemoveAll(t => IsLoadable_Etc(t, egroup, qty) == false);
			}

			if (list.Count == 0)
				return false;

			if (loadableList != null)
				loadableList.AddRange(list);

			return isLoadable;
		}

		private static bool IsLoadable(Dictionary<string, List<EqpArrangeInfo>> group,
			ref List<EqpArrangeInfo> loadableList, int qty = 0)
		{
			List<EqpArrangeInfo> list = new List<EqpArrangeInfo>();

			foreach (var it in group.Values)
			{
				if (it == null || it.Count == 0)
					continue;

				//비가용 여부 체크
				var find = it.Find(t => t.IsLoadable(qty) == false);
				if (find != null)
					continue;

				list.Add(it[0]);
			}

			if (list.Count > 0)
			{
				if (loadableList != null)
					loadableList.AddRange(list);

				return true;
			}

			return false;
		}

		private static bool IsLoadable_Etc(EqpArrangeInfo item,
			Dictionary<string, List<EqpArrangeInfo>> group, int qty)
		{
			if (item == null)
				return false;

			if (group == null || group.Count == 0)
				return true;

			foreach (var it in group.Values)
			{
				//Etc Group에 일치하는 비가용 존재시 비가용 처리
				var find = it.Find(t => t.IsLoadable(item, qty) == false);
				if (find != null)
					return false;
			}

			return true;
		}        

		private static void MoveQty(this EqpArrangeSet setInfo, FabAoEquipment eqp, FabLot lot)
		{
			if (setInfo.HasItems() == false)
				return;

			int qty = lot.UnitQty;
			if (qty <= 0)
				return;

			string productVersion = lot.CurrentProductVersion;
            var mask = lot.CurrentMask;

			var list = setInfo.Items;
			foreach (var item in list)
			{
				if (item.ActivateType != ActivateType.M)
					continue;

				if (item.IsMatchedByProductVersion(productVersion) == false)
					continue;

                if (item.IsMatchedByMask(mask) == false)
                    continue;

                item.MoveQty += qty;
				item.Seq++;

				OutCollector.WriteLimitMLog(item, eqp, lot, eqp.NowDT);
			}
		}

		#endregion EqpArrangeSet Func

		#region EqpArrangeInfo Func

		private static bool IsMatched(this EqpArrangeInfo item,
			string stepID, string productID, string productVer, bool isFixedProductVer)
		{
			if (item.HasLimitType(LimitType.O))
			{
				if (IsMatched(item.StepID, stepID) == false)
					return false;
			}

			if (item.HasLimitType(LimitType.P))
			{
				if (IsMatched(item.ProductID, productID) == false)
					return false;
			}

			if (item.HasLimitType(LimitType.B))
			{
				if (isFixedProductVer)
				{
					if (IsMatched(item.ProductVer, productVer) == false)
						return false;
				}
			}

			return true;
		}

		private static bool IsMatchedByProductVersion(this EqpArrangeInfo item, string productVer)
		{
			if (item.HasLimitType(LimitType.B))
			{
				if (IsMatched(item.ProductVer, productVer) == false)
					return false;
			}

			return true;
		}

        private static bool IsMatchedByMask(this EqpArrangeInfo item, FabMask mask)
        {
            if (item.HasLimitType(LimitType.M))
            {
                if (mask == null)
                    return false;

                if (IsMatched(item.MaskID, mask.ToolID) == false)
                    return false;
            }

            return true;
        }

        public static bool IsLoadable(this EqpArrangeInfo item, int qty)
		{
			if (item.IsLoadable_ActivateType() == false)
				return false;

			//M 잔여 수량 미체크
			if (qty <= 0)
				return true;

			if (item.ActivateType == ActivateType.M)
			{
				//잔량보다 Capa가 작은 경우 미투입 처리
				if (qty > item.RemainQty)
					return false;
			}

			return true;
		}

		private static bool IsLoadable(this EqpArrangeInfo item, EqpArrangeInfo targetItem, int qty)
		{
			//check matched
			bool isFixedProductVer = targetItem.HasLimitType(LimitType.B);

			bool isMatched = item.IsMatched(targetItem.StepID,
										   targetItem.ProductID,
										   item.ProductVer,
										   isFixedProductVer);

			if (isMatched == false)
				return true;

			return item.IsLoadable(qty);
		}

		private static bool IsLoadable_ActivateType(this EqpArrangeInfo item)
		{
			if (item.ActivateType == ActivateType.N)
				return false;

			if (item.ActivateType == ActivateType.M)
				return true;

			if (item.ActivateType == ActivateType.Y)
				return true;

			return false;
		}

		private static int GetSupportAllCount(this EqpArrangeInfo item)
		{
			int count = 0;

			if (IsSupportAll(item.StepID))
				count++;

			if (IsSupportAll(item.ProductID))
				count++;

			if (IsSupportAll(item.ProductVer))
				count++;

			return count;
		}

		private static int GetLimitTypeCount(this EqpArrangeInfo item)
		{
			if (item.LimitTypeList == null)
				return 0;

			return item.LimitTypeList.Count;
		}

		public static bool HasLimitType(this EqpArrangeInfo item, LimitType limitType)
		{
			var list = item.LimitTypeList;

			return HasLimitType(list, limitType);
		}

		public static string GetGroupKey(this EqpArrangeInfo item)
		{
			bool hasB = item.HasLimitType(LimitType.B);
			string key = hasB ? item.ProductVer : Constants.NULL_ID;

            bool hasM = item.HasLimitType(LimitType.M);
            if (hasM)
                key = string.Format("{0}{1}", key, item.MaskID);

			return key;
		}

		#endregion EqpArrangeInfo Func

		#region CurrentEqpArrange Func

		public static void SetUsedArrange(this CurrentArrange item, string productVersion)
		{
			item.UsedMaskProductVersion = productVersion;

			var list = item.LoadableList;
			if (list == null || list.Count == 0)
				return;

			if (list.Count > 1)
				list.Sort(new EqpArrangeInfoComparer2(productVersion));

			item.UsedEqpArrange = list[0];

			if (string.IsNullOrEmpty(item.UsedMaskProductVersion))
			{
				//Mask 미사용 옵션, Arrange의 ProductVersion으로 대체(??)
				if (LcdHelper.Equals(item.UsedEqpArrange.ProductVer, "ALL") == false)
					item.UsedMaskProductVersion = item.UsedEqpArrange.ProductVer;
			}
		}

		public static bool IsLoableByProductVersion(this CurrentArrange item, string productVersion)
		{
			var list = item.LoadableList;
			if (list == null || list.Count == 0)
				return false;

			var find = list.Find(t => t.IsMatchedByProductVersion(productVersion));
			if (find != null)
				return true;

			return false;
		}

		#endregion CurrentEqpArrange Func

		#region Helper

		private static string CreateKey_EqpArrangeSet(string eqpID, string stepID, string productID, string productVer)
		{
			return LcdHelper.CreateKey(eqpID, stepID, productID, productVer);
		}

		private static bool IsMatched(string arrangeStr, string targetStr)
		{
			if (LcdHelper.Equals(arrangeStr, "ALL") || LcdHelper.Equals(targetStr, "ALL"))
				return true;

			return arrangeStr == targetStr;
		}

		private static bool HasLimitType(List<LimitType> list, LimitType limitType)
		{
			if (list == null || list.Count == 0)
				return false;

			if (list.Contains(limitType))
				return true;

			return false;
		}

		private static bool IsSupportAll(string str)
		{
			return LcdHelper.Equals(str, "ALL");
		}

		public static bool IsFixedProductVer(FabStdStep stdStep, string productVer)
		{
			string shopID = stdStep.ShopID;

			//Ver는 Array Shop에서만 유효
			if (BopHelper.IsArrayShop(shopID) == false)
				return false;

			//1300 ~ 5300 공정까지만 ProductVersion 유효(2020.01.13 - by.liujian(유건))
			//5300 이후 ProductVersion 무시
			var toStep = BopHelper.FindStdStep(Constants.ArrayShop, Constants.ITO_PHOTO_STEP);
			if (toStep != null)
			{
				if (stdStep.StepSeq > toStep.StepSeq)
					return false;
			}

			//1300 이전 공정 이면서 Ver에 X가 없는 Version은 변경 투입 가능함.			
			if (AvailableChangeProductVer(stdStep, productVer))
				return false;

			if (LcdHelper.IsEmptyID(productVer))
				return false;

			return true;
		}

		public static bool AvailableChangeProductVer(FabStdStep stdStep, string productVer)
		{
			string shopID = stdStep.ShopID;
			string stepID = stdStep.StepID;

			//1300 이전 공정 이면서 Ver에 X가 없는 Version은 변경 투입 가능함.			
			if (BopHelper.IsArrayShop(shopID))
			{
				if (IsProductVer_X(productVer) == false)
				{
					if (LcdHelper.Equals(Constants.GATE_PHOTO_STEP, stepID))
						return true;

					//1300 이전 공정은 ProductVersion 변경가능 함.
					var fromStep = BopHelper.FindStdStep(Constants.ArrayShop, Constants.GATE_PHOTO_STEP);
					if (fromStep != null)
					{
						if (stdStep.StepSeq <= fromStep.StepSeq)
							return true;
					}
				}
			}

			return false;
		}

		public static bool IsProductVer_X(string productVer)
		{
			if (string.IsNullOrEmpty(productVer))
				return false;

			return productVer.Contains("X");
		}

		#endregion Helper

		#region Comparer

		public class EqpArrangeInfoComparer : IComparer<EqpArrangeInfo>
		{
			public int Compare(EqpArrangeInfo x, EqpArrangeInfo y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = x.ActivateType.CompareTo(y.ActivateType);

				//M인 경우 RemainQty 적은 순
				if (cmp == 0 && x.ActivateType == ActivateType.M)
				{
					int remain_x = x.LimitQty - x.InitActQty;
					int remain_y = y.LimitQty - y.InitActQty;

					cmp = remain_x.CompareTo(remain_y);
				}

				//LimitType 많은 순(Detail 정보 우선)
				if (cmp == 0)
				{
					cmp = x.GetLimitTypeCount().CompareTo(y.GetLimitTypeCount()) * -1;
				}

				//ALL이 작은 순(Detail 정보 우선)
				if (cmp == 0)
				{
					cmp = x.GetSupportAllCount().CompareTo(y.GetSupportAllCount());
				}

				return cmp;
			}

			public static EqpArrangeInfoComparer Default = new EqpArrangeInfoComparer();
		}

		public class EqpArrangeInfoComparer2 : IComparer<EqpArrangeInfo>
		{
			private string ProductVerion { get; set; }

			public EqpArrangeInfoComparer2(string productVer)
			{
				this.ProductVerion = productVer;
			}

			public int Compare(EqpArrangeInfo x, EqpArrangeInfo y)
			{
				if (object.ReferenceEquals(x, y))
					return 0;

				int cmp = 0;

				if (cmp == 0)
				{
					bool ver_x = x.ProductVer == this.ProductVerion;
					bool ver_y = y.ProductVer == this.ProductVerion;

					cmp = ver_x.CompareTo(ver_y) * -1;
				}

				if (cmp == 0)
				{
					cmp = EqpArrangeInfoComparer.Default.Compare(x, y);
				}

				return cmp;
			}
		}



		#endregion Comparer        
	}
}
