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
using Mozart.SeePlan.Simulation;
using System.Text;
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class DispatchLogHelper
	{
		internal static void InitLotLogDetail(IList<IHandlingBatch> wips)
		{
			if (InputMart.Instance.GlobalParameters.ApplyLotGroupDispatching == false)
				return;

			foreach (IHandlingBatch hb in wips)
			{
				if (hb.HasContents)
				{
					foreach (FabLot lot in hb.Contents)
					{
						lot.LogDetail = Constants.NULL_ID;
					}
				}
				else
				{
					FabLot lot = hb.ToFabLot();
					lot.LogDetail = Constants.NULL_ID;
				}
			}
		}

		internal static void AddDispatchInfo(FabAoEquipment eqp, IList<IHandlingBatch> lotList, IHandlingBatch[] selected, WeightPreset preset)
		{
			if (InputMart.Instance.GlobalParameters.ApplyLotGroupDispatching)
			{
				bool isAllWrite = false;
				if (isAllWrite)
				{
					AddAllDispatchInfo(eqp, lotList, selected, preset);
				}
				else
				{
					AddGroupDispatchInfo(eqp, lotList, selected, preset);
				}
			}
			else
			{
				eqp.EqpDispatchInfo.AddDispatchInfo(lotList, selected, preset);
			}
		}

		private static void AddGroupDispatchInfo(FabAoEquipment eqp, IList<IHandlingBatch> lotList, IHandlingBatch[] selected, WeightPreset preset)
		{
			for (int i = 0; i < lotList.Count; i++)
			{
				var hb = lotList[i];

				FabLot lot = hb.ToFabLot();

				if (hb.HasContents)
				{
					StringBuilder sb = new StringBuilder();
					foreach (FabLot item in hb.Contents)
					{
						if (sb.Length == 0)
							sb.Append(item.LotID);
						else
							sb.AppendFormat(",{0}", item.LotID);
					}

					lot.LogDetail = string.Format("{0} [{1}→{2}]", hb.UnitQty, hb.Count, sb);
				}
				else

					Console.WriteLine();
			}

			eqp.EqpDispatchInfo.AddDispatchInfo(lotList, selected, preset);
		}

		private static void AddAllDispatchInfo(FabAoEquipment eqp, IList<IHandlingBatch> lotList, IHandlingBatch[] selected, WeightPreset preset)
		{
			foreach (var hb in lotList)
			{
				if (hb.HasContents)
				{
					FabLot sample = hb.ToFabLot();

					List<IHandlingBatch> list = new List<IHandlingBatch>(hb.Contents.Count);
					foreach (FabLot lot in hb.Contents)
					{
						if (lot.Equals(sample) == false)
							lot.WeightInfo = sample.WeightInfo;

						list.Add(lot);
					}

					eqp.EqpDispatchInfo.AddDispatchInfo(list, selected, preset);
				}
			}
		}

		public static string GetDispatchWipLog(FabEqp targeEqp, EntityDispatchInfo info, FabLot lot, WeightPreset wp)
		{
			var slot = lot as FabLot;

			StringBuilder sb = new StringBuilder();

			SetDefaultLotInfo(sb, slot);

			if (wp != null)
			{
				foreach (var factor in wp.FactorList)
				{
					var vdata = slot.WeightInfo.GetValueData(factor);

					sb.Append("/");

					if (string.IsNullOrEmpty(vdata.Description))
						sb.Append(vdata.Value);
					else
						sb.Append(vdata.Value + "@" + vdata.Description);
				}
			}

			if (lot.CurrentFabPlan.LotFilterInfo != null && lot.CurrentFabPlan.LotFilterInfo.FilterType == DispatchFilter.Revaluate)
				sb.Append("/0@[IS_REVALUATE：Y]");
			else
				sb.Append("/0@[IS_REVALUATE：N]");

			return sb.ToString();
		}

		public static string GetSelectedWipLog(FabEqp targeEqp, IHandlingBatch[] sels)
		{
			StringBuilder sb = new StringBuilder();

			if (sels == null)
				return string.Empty;

			foreach (IHandlingBatch hb in sels)
			{
				if (sb.Length > 0)
					sb.Append(";");

				var lot = hb.Sample as FabLot;

				SetDefaultLotInfo(sb, lot);
			}

			return sb.ToString();
		}

		public static void UpdateDispatchLogByAheadSetup(FabAoEquipment eqp, EqpDispatchInfo info)
		{
			if (eqp == null || info == null)
				return;

			//TODO : [bong] ParallelChamber는 제외
			if (eqp.IsParallelChamber)
				return;

			string eqpID = eqp.EqpID;
			string subEqpID = string.Empty;

			string setupStartTime = LcdHelper.DbToString(eqp.AvailableSetupTime);
			string dispatchTime = LcdHelper.DbToString(info.DispatchTime);

			//Dispach시간과 AheadSetup 으로 인해 시간이 다름
			if (setupStartTime != dispatchTime)
			{
				var table = OutputMart.Instance.EqpDispatchLog;

				EqpDispatchLog origRow = table.Find(eqpID, subEqpID, dispatchTime);
				EqpDispatchLog dubRow = table.Find(eqpID, subEqpID, setupStartTime);

				string otherInfo = string.Format("ORIGIN_TIME:{0}", dispatchTime);

				if (dubRow != null)
				{
					dubRow.DISPATCHING_TIME = setupStartTime;

					dubRow.INIT_WIP_CNT = origRow.INIT_WIP_CNT;
					dubRow.FILTERED_WIP_CNT = origRow.FILTERED_WIP_CNT;
					dubRow.SELECTED_WIP_CNT = origRow.SELECTED_WIP_CNT;
					dubRow.SELECTED_WIP = origRow.SELECTED_WIP;
					dubRow.FILTERED_WIP_LOG = origRow.FILTERED_WIP_LOG;
					dubRow.DISPATCH_WIP_LOG = origRow.DISPATCH_WIP_LOG;
					dubRow.PRESET_ID = origRow.PRESET_ID;
					dubRow.OTHER_INFO = otherInfo;
				}
				else if (origRow != null)
				{
					origRow.DISPATCHING_TIME = setupStartTime;
					origRow.OTHER_INFO = otherInfo;
				}
			}
		}

		public static void WriteDispatchLog(FabAoEquipment eqp, EqpDispatchInfo info)
		{
			if (eqp == null || info == null)
				return;

			WriteDispatchLog(eqp, info, null);
		}

		public static void WriteDispatchLog_ParallelChamber(FabAoEquipment eqp, FabLot lot)
		{
			if (eqp == null)
				return;

			var subEqpList = eqp.SubEqps;
			if (subEqpList == null)
				return;

			foreach (var subEqp in subEqpList)
			{
				var info = subEqp.EqpDispatchInfo;
				if (info == null)
					continue;

				var currLot = subEqp.ChamberInfo.Current as FabLot;
				if (currLot != lot)
					continue;

				WriteDispatchLog(eqp, info, subEqp);

				//기록 후 삭제 처리
				subEqp.EqpDispatchInfo = null;
			}
		}

		public static void WriteDispatchLog(FabAoEquipment eqp, EqpDispatchInfo info, FabSubEqp subEqp)
		{
			if (eqp == null || info == null)
				return;

			DateTime dispatchTime = info.DispatchTime;
			if (dispatchTime == DateTime.MinValue || dispatchTime == DateTime.MaxValue)
				return;

			string dispatchTimeStr = LcdHelper.DbToString(dispatchTime);
			var last = eqp.LastPlan as FabPlanInfo;
						
			string eqpID = eqp.EqpID;
			string subEqpID = string.Empty;

			if (subEqp != null)
			{
				subEqpID = subEqp.SubEqpID;
				last = subEqp.LastPlan as FabPlanInfo;
			}

			var table = OutputMart.Instance.EqpDispatchLog;
			var row = table.Find(eqpID, subEqpID, dispatchTimeStr);

			//parent EqpID 존재시 
			if (row == null)
			{
				if (eqp.IsParallelChamber && subEqp != null)
					row = table.Find(eqpID, string.Empty, dispatchTimeStr);
			}

			if (row == null)
			{
				row = new EqpDispatchLog();

				row.EQP_ID = eqpID;
				row.SUB_EQP_ID = subEqpID;
				row.DISPATCHING_TIME = dispatchTimeStr;

				table.Add(row);
			}

			FabEqp targetEqp = eqp.TargetEqp;

			row.VERSION_NO = ModelContext.Current.VersionNo;

			row.FACTORY_ID = targetEqp.FactoryID;
			row.SHOP_ID = targetEqp.ShopID;
			row.EQP_GROUP = targetEqp.EqpGroup;

			row.SUB_EQP_ID = subEqpID;

			if (last != null)
			{
				StringBuilder sb = GetDefaultLotInfo(last.LotID,
													 last.ProductID,
													 last.ProductVersion,
													 last.StepID,
													 last.UnitQty.ToString(),
													 last.OwnerType,
													 last.OwnerID,
													 Constants.NULL_ID);
				row.LAST_WIP = sb.ToString();
			}

			row.SELECTED_WIP = info.SelectedWipLog;
			row.DISPATCH_WIP_LOG = ParseDispatchWipLog(info);

			int filteredWipCnt = 0;
			row.FILTERED_WIP_LOG = ParseFilteredInfo(info, ref filteredWipCnt);

			row.FILTERED_WIP_CNT = filteredWipCnt;
			row.SELECTED_WIP_CNT = string.IsNullOrWhiteSpace(info.SelectedWipLog) ? 0 : info.SelectedWipLog.Split(';').Length;
			row.INIT_WIP_CNT = row.FILTERED_WIP_CNT + info.Batches.Count;

			if (targetEqp.Preset != null)
				row.PRESET_ID = targetEqp.Preset.Name;
		}

		private static void SetDefaultLotInfo(StringBuilder sb, FabLot lot)
		{
			var sb2 = GetDefaultLotInfo(lot.LotID,
										lot.CurrentProductID,
										lot.CurrentProductVersion ?? lot.Wip.ProductVersion,
										lot.CurrentStepID,
										lot.UnitQty.ToString(),
										lot.OwnerType,
										lot.OwnerID,
										lot.LogDetail
										);

			sb.Append(sb2);
		}

		private static StringBuilder GetDefaultLotInfo(string lotID, string productID, string productVersion,
			string stepID, string unitQty, string ownerType, string ownerID, string logDetail)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(lotID);
			sb.AppendFormat("/{0}", productID);
			sb.AppendFormat("/{0}", productVersion);
			sb.AppendFormat("/{0}", stepID);
			sb.AppendFormat("/{0}", unitQty);
			sb.AppendFormat("/{0}", ownerType);
			sb.AppendFormat("/{0}", ownerID);
			sb.AppendFormat("/{0}", logDetail);

			return sb;
		}
		
		private static string ParseDispatchWipLog(EqpDispatchInfo info)
		{
			StringBuilder dsb = new StringBuilder();
			foreach (var di in info.Batches)
			{
				if (dsb.Length > 0)
					dsb.Append(";");

				dsb.Append(di.Log);
			}

			return dsb.ToString();
		}

		private static string ParseFilteredInfo(EqpDispatchInfo info, ref int filteredWipCnt)
		{
			if (info.FilterInfos.Count == 0)
				return string.Empty;

			StringBuilder result = new StringBuilder();

			foreach (KeyValuePair<string, EntityFilterInfo> filtered in info.FilterInfos)
			{
				EntityFilterInfo filterInfo = filtered.Value;

				filteredWipCnt += filterInfo.FilterWips.Count;

				StringBuilder fsb = new StringBuilder();

				fsb.Append(filterInfo.Reason);
				fsb.Append(':');

				bool first = true;

				foreach (FabLot fw in filterInfo.FilterWips)
				{
					StringBuilder sb = new StringBuilder();

					if (!first)
						sb.Append(";");
					else
						first = false;

					SetDefaultLotInfo(sb, fw);
					
					fsb.Append(sb);
				}

				fsb.Append("\t");

				result.Append(fsb);
			}

			return result.ToString();
		}
	}
}

