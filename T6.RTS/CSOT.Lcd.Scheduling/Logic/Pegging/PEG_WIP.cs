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
using Mozart.SeePlan.Pegging.Rule;

namespace CSOT.Lcd.Scheduling.Logic.Pegging
{
	[FeatureBind()]
	public partial class PEG_WIP
	{
		/// <summary>
		/// </summary>
		/// <param name="pegPart"/>
		/// <param name="isRun"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public IList<Mozart.SeePlan.Pegging.IMaterial> GET_WIPS0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun, ref bool handled, IList<IMaterial> prevReturnValue)
		{
			if (pegPart.CurrentStage.State == "CellBankStage")
				return PegMaster.GetBankWip(pegPart);

			var pp = pegPart as FabPegPart;
			var prod = pp.Product;
			var step = pegPart.CurrentStage.Tag as FabStep;

			var rows = InputMart.Instance.FabPlanWipView.FindRows(step);

			List<IMaterial> result = new List<IMaterial>();

			foreach (FabPlanWip planWip in rows)
			{
				if (planWip.Qty == 0)
					continue;

				if (isRun != planWip.IsRunWip)
					continue;

				if (planWip.ProductID != prod.ProductID)
				{
					if (planWip.Product.IsTestProduct == false)
						continue;

					if (planWip.Product.MainProductID != prod.ProductID)
						continue;
				}

				planWip.MapCount++;

				result.Add(planWip);

			}

			return result;
		}

		/// <summary>
		/// </summary>
		/// <param name="target"/>
		/// <param name="m"/>
		/// <param name="isRun"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public double AVAIL_PEG_QTY0(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, bool isRun, ref bool handled, double prevReturnValue)
		{
			return m.Qty;
		}

		/// <summary>
		/// </summary>
		/// <param name="target"/>
		/// <param name="m"/>
		/// <param name="qty"/>
		/// <param name="handled"/>
		public void WRITE_PEG0(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, double qty, ref bool handled)
		{
			PegHelper.WritePeg(target, m, qty);
		}

		/// <summary>
		/// </summary>
		/// <param name="pegpart"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public bool IS_REMOVE_EMPTY_TARGET0(Mozart.SeePlan.Pegging.PegPart pegpart, ref bool handled, bool prevReturnValue)
		{
			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="target"/>
		/// <param name="m"/>
		/// <param name="isRun"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public bool CAN_PEG_MORE0(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, bool isRun, ref bool handled, bool prevReturnValue)
		{
			FabPlanWip wip = m.ToFabPlanWip();
			FabPegTarget pt = target.ToFabPegTarget();

			if (wip.IsHold == false)
				return true;

			// Hold Peg 방지 옵션
			//if (pt.CalcDate < wip.AvailableTime)
			//    return false;

			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="x"/>
		/// <param name="y"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public int SORT_WIP0(MaterialInfo x, MaterialInfo y, ref bool handled, int prevReturnValue)
		{
			FabPlanWip a = x.Material as FabPlanWip;
			FabPlanWip b = y.Material as FabPlanWip;

			int cmp = 0;

			if (cmp == 0)
				cmp = a.IsHold.CompareTo(b.IsHold);

			//둘다 Hold일 경우
			if (cmp == 0 && a.IsHold && b.IsHold)
			{
				//Hold > Move
				cmp = a.LotState.CompareTo(b.LotState);

				//가용시간 빠른 것
				if (cmp == 0)
					cmp = a.AvailableTime.CompareTo(b.AvailableTime);
			}

			if (cmp == 0)
				a.WipInfo.WipStateTime.CompareTo(b.WipInfo.WipStateTime);

			if (cmp == 0)
				cmp = a.WipInfo.Priority.CompareTo(b.WipInfo.Priority);

			if (cmp == 0)
				cmp = a.Qty.CompareTo(b.Qty);

			if (cmp == 0)
				cmp = a.ProductID.CompareTo(b.ProductID);

			if (cmp == 0)
				cmp = a.ProductVersion.CompareTo(b.ProductVersion);

			return cmp;
		}
	}
}
