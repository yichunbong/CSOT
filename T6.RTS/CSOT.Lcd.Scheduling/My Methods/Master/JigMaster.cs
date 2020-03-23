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

namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class JigMaster
	{
		static Dictionary<string, FabMask> _jigs = new Dictionary<string, FabMask>();
		static Dictionary<string, List<FabMask>> _jigSets = new Dictionary<string, List<FabMask>>();

		public static Dictionary<string, FabMask> Jigs { get { return _jigs; } }

		internal static void AddTool(FabMask mask)
		{
			if (_jigs.ContainsKey(mask.JigID) == false)
				_jigs.Add(mask.JigID, mask);

			List<FabMask> jigList;
			if (_jigSets.TryGetValue(mask.ToolID, out jigList) == false)
				_jigSets.Add(mask.ToolID, jigList = new List<FabMask>());

			jigList.Add(mask);

            mask.AddLoadInfo(mask.InitEqpID, null, true);
        }

		internal static void AddToolArrange(FabStdStep stdStep, MaskArrange arr)
		{
			List<MaskArrange> list;
			if (stdStep.JigArrange.TryGetValue(arr.EqpID, arr.ProductID, out list) == false)
				stdStep.JigArrange.Add(arr.EqpID, arr.ProductID, list = new List<MaskArrange>());

			list.Add(arr);
		}

		public static bool IsUseJig(FabLot lot)
		{
			if (InputMart.Instance.GlobalParameters.ApplySecondResource == false)
				return false;

			var step = lot.CurrentFabStep;
			var stdStep = step.StdStep;

			return stdStep.IsUseJig;
		}

		internal static void InitLocate(AoEquipment aeqp, IHandlingBatch hb)
		{
			var eqp = aeqp.ToFabAoEquipment();

			FabLot lot = hb.ToFabLot();
			if (IsUseJig(lot) == false)
				return;

			var loableList = GetLoadableJigArrangeList(aeqp, lot);

			if (loableList == null || loableList.Count == 0)
				return;

			foreach (var item in loableList[0].Jigs)
			{
				if (item.IsBusy)
					continue;

				lot.CurrentJig.Add(item);

				if (lot.CurrentJig.Count == eqp.UseJigCount)
					break;
			}
		}

		public static void StartTask(FabLot lot, AoEquipment aeqp)
		{
			if (IsUseJig(lot) == false)
				return;

			if (lot.CurrentJig == null || lot.CurrentJig.Count == 0)
				return;

			var eqp = aeqp.ToFabAoEquipment();
			eqp.InUseJig.Masks.Clear();

			foreach (var item in lot.CurrentJig)
			{
				item.AddWorkInfo(lot);
				eqp.InUseJig.Masks.Add(item);

				item.AddLoadInfo(eqp.EqpID, lot);
			}
		}

		public static void EndTask(FabLot lot, AoEquipment aeqp)
		{
			if (IsUseJig(lot))
			{
				//var eqp = aeqp.ToFabAoEquipment();
				var jig = lot.CurrentJig;

				if (jig != null)
				{
					foreach (var item in jig)
					{
						var loadInfo = item.FindLoadInfo(lot, aeqp);
						if (loadInfo != null)
							loadInfo.EndTime = aeqp.NowDT;

						item.RemoveWorkInfo(lot);
					}

					lot.CurrentJig.Clear();
				}
			}
		}

		public static void AddMaskToJigArrange(MaskArrange arr, ToolArrange item)
		{
			List<FabMask> list;
			_jigSets.TryGetValue(item.TOOL_ID, out list);

			if (list == null)
				return;

			arr.Jigs.AddRange(list);
		}
        
		internal static List<FabMask> SelectJigMask(AoEquipment aeqp, FabLot lot)
		{
			List<FabMask> result = new List<FabMask>();

			var eqp = aeqp.ToFabAoEquipment();

			if (eqp.IsLastPlan(lot))
			{
				var currentJig = eqp.GetCurrentJig();
				if (currentJig != null)
					result.AddRange(currentJig.Masks);
			}

			if (result.Count < eqp.UseJigCount)
			{
				var loadableList = GetLoadableJigArrangeList(aeqp, lot);

				if (loadableList == null || loadableList.Count == 0)
					return null;

				foreach (var item in loadableList[0].Jigs)
				{
					if (item.IsBusy)
						continue;

					result.Add(item);

					if (result.Count == eqp.UseJigCount)
						break;
				}
			}

			return result;
		}

		private static List<MaskArrange> GetLoadableJigArrangeList(AoEquipment aeqp, FabLot lot)
		{
			List<MaskArrange> list = new List<MaskArrange>();

			var eqp = aeqp.ToFabAoEquipment();

			var malist = MaskMaster.GetMatchedMaskArrangeList(aeqp, lot, ToolType.JIG);
			if (malist == null)
				return list;

			foreach (var item in malist)
			{
				int ableQty = 0;
				foreach (var mask in item.Jigs)
				{
					if (mask.IsBusy)
						continue;

					ableQty++;
				}

				if (ableQty >= eqp.UseJigCount)
					list.Add(item);
			}

			return list;
		}

		private static JigSet GetCurrentJig(this FabAoEquipment eqp)
		{
			JigSet jig = eqp.InUseJig;

			if (jig != null && jig.EqpID != jig.EqpID)
				return null;

			return jig;
		}

		public static void OnDone(AoFactory aoFactory)
		{
			foreach (var mask in _jigs.Values )
			{
				if (mask.WorkInfos.Count == 0 && mask.LoadInfos.Count > 0)
				{
					var last = mask.LastPlan;

					mask.AddLoadInfo(last.EqpID, null);
				}
			}
		}
	}
}