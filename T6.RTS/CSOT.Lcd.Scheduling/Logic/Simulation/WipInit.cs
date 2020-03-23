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
using Mozart.SeePlan;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
	[FeatureBind()]
	public partial class WipInit
	{
		/// <summary>
		/// </summary>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public IList<Mozart.SeePlan.Simulation.IHandlingBatch> GET_WIPS0(ref bool handled, IList<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
		{
			List<IHandlingBatch> list = new List<IHandlingBatch>();

			foreach (FabWipInfo wip in InputMart.Instance.FabWipInfo.Values)
			{
				FabStep step = wip.InitialStep as FabStep;

				if (SimHelper.IsTftRunning && step.IsCellShop)
					continue;
				else if (SimHelper.IsCellRunning && step.IsCellShop == false)
					continue;

				FabLot lot = CreateHelper.CreateLot(wip, LotState.WIP);
				list.Add(lot);

				if (wip.IsRun)
				{
					string eqpID = wip.WipEqpID ?? string.Empty;

					FabEqp eqp;
					InputMart.Instance.FabEqp.TryGetValue(eqpID, out eqp);
					if (eqp != null)
						eqp.InitRunWips.AddSort(lot, CompareHelper.RunWipComparer.Default);
				}

				if (SimHelper.IsTftRunning)
				{
					WipCollector.AddProductByVersion(lot);
				}
			}

			//TODO :  임시로직, CF에 있는 Array제품 버전문제
			if (SimHelper.IsTftRunning)
			{
				foreach (FabLot lot in list)
				{
					FabProduct prod = lot.FabProduct;


					if (BopHelper.IsCfShop(lot.Wip.ShopID) && prod.IsArrayShopProduct())
					{
						string version = WipCollector.GetVersion(prod.ProductID);

						if (LcdHelper.IsEmptyID(version) == false)
							lot.OrigProductVersion = version;
						else
							Console.WriteLine();
					}
				}
			}

			return list;
		}

		/// <summary>
		/// </summary>
		/// <param name="x"/>
		/// <param name="y"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public int COMPARE_WIP0(IHandlingBatch x, IHandlingBatch y, ref bool handled, int prevReturnValue)
		{
			if (object.ReferenceEquals(x, y))
				return 0;

			var a = (FabLot)x.Sample;
			var b = (FabLot)y.Sample;

			// RUN-INBUF->WAIT->HOLD
			int cmp = b.CurrentState.CompareTo(a.CurrentState);
			if (cmp != 0)
				return cmp;

			//cmp = a.WipInfo.LastTrackInTime.CompareTo(b.WipInfo.LastTrackInTime);
			cmp = a.WipInfo.WipStateTime.CompareTo(b.WipInfo.WipStateTime);

			return cmp;
		}

		/// <summary>
		/// </summary>
		/// <param name="aeqp"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public DateTime FIX_START_TIME0(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, DateTime prevReturnValue)
		{
			FabLot lot = hb.ToFabLot();

			var now = aeqp.NowDT;
			var trackInTime = lot.Wip.LastTrackInTime;

			//한개의 RunWip만 설비에 투입가능하도록 처리(LOCATE_FOR_RUN0)하여 조정 필요없음.
			////RunWip 수량을 고려하여 마지막 RunWip의 TrackInTime을 기준으로 이전 RunWip의 TrackInTime 조정
			//var runList = (aeqp.Target as FabEqp).InitRunWips;            
			//if(runList != null)
			//{
			//    //해당 Lot 삭제
			//    if (runList.Remove(lot) == false)
			//        Logger.MonitorInfo("[WARNING] mismatch run wip : LOT_ID={0}", lot.LotID);

			//    int count = runList.Count;
			//    if (runList != null && count > 0)
			//    {
			//        var prevlot = runList[count - 1];
			//        DateTime outTime = prevlot.Wip.LastTrackInTime;

			//        for (int i = count - 2; i >= 0; i--)
			//        {
			//            var currLot = runList[i];

			//            if (currLot.UnitQty <= 0)
			//                continue;

			//            FabStep wipStep = currLot.Wip.InitialStep as FabStep;
			//            float currTactTime = wipStep.GetTactTime(eqpID, currLot.CurrentProductID);

			//            DateTime availableTime = outTime.AddSeconds(-(currTactTime * currLot.UnitQty));

			//            DateTime tkTime = currLot.Wip.LastTrackInTime;
			//            outTime = LcdHelper.Min(tkTime, availableTime);
			//        }

			//        float tactTime = lot.CurrentFabStep.GetTactTime(eqpID, lot.CurrentProductID);
			//        outTime = outTime.AddSeconds(-(tactTime * lot.UnitQty));

			//        trackInTime = LcdHelper.Min(outTime, trackInTime);
			//    }
			//}

			//TODO : WriteErrorHist
			if (trackInTime == DateTime.MinValue || trackInTime == DateTime.MaxValue)
			{
				trackInTime = aeqp.NowDT;
			}
			else if (trackInTime > now)
			{
				trackInTime = now;
			}

			return trackInTime;
		}

		/// <summary>
		/// </summary>
		/// <param name="hb"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public string GET_LOADING_EQUIPMENT0(Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, string prevReturnValue)
		{
			FabLot lot = hb.Sample as FabLot;

			return lot.WipInfo.WipEqpID;
		}

		/// <summary>
		/// </summary>
		/// <param name="factory"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		public void ON_BEGIN_LOCATE_WIP0(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
		{

		}

		/// <summary>
		/// </summary>
		/// <param name="factory"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		public void ON_END_LOCATE_WIP0(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
		{
			if (SimHelper.IsCellRunning)
				Console.WriteLine();
		}

		/// <summary>
		/// </summary>
		/// <param name="factory"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		public void LOCATE_FOR_RUN0(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
		{
			var wipInitiator = ServiceLocator.Resolve<WipInitiator>();

			FabLot lot = hb.Sample as FabLot;

			string eqpID = wipInitiator.GetLoadingEquipment(hb);
			AoEquipment aeqp = factory.GetEquipment(eqpID);

			if (aeqp == null)
			{
				//If there is not Equipment, handle through Bucketing.
				factory.AddToBucketer(hb);
				Logger.Warn("Eqp {0} is invalid, so locate running wip to dummy bucket. check input data!", eqpID ?? "-");
			}
			else
			{
				//// Checks WIP state that is Run, but processing is completed and located in Outport. 
				//bool trackOut = wipInitiator.CheckTrackOut(factory, hb);
				//if (trackOut)
				//{
				//    aeqp.AddOutBuffer(hb);
				//}
				//else
				//{
				//    aeqp.AddRun(hb);
				//}

				var eqp = aeqp.Target as FabEqp;
				var runWips = eqp.InitRunWips;

				bool lastRunWip = runWips[runWips.Count - 1] == lot;
				if (eqp.State == ResourceState.Up && lastRunWip)
				{
					MaskMaster.InitLocate(aeqp, hb);
					JigMaster.InitLocate(aeqp, hb);

					aeqp.AddRun(hb); //※초기Run재공은 OnTrackIn 이벤트 발생안함.
				}
				else
				{
					DateTime tkInTime = lot.Wip.LastTrackInTime;
					var procTimeInfo = aeqp.GetProcessTime(hb);
					double processTime = procTimeInfo.FlowTime.TotalSeconds + (procTimeInfo.TactTime.TotalSeconds * (hb.UnitQty - 1));
					DateTime tkOutTime = tkInTime.AddSeconds(processTime);

					Time delay = Time.Max((tkOutTime - aeqp.NowDT), Time.Zero);
					if (delay > Time.Zero)
					{
						object[] args = new object[2] { aeqp, hb };
						aeqp.AddTimeout(delay, SimHelper.OnEqpOutBuffer, args);
						InFlowMaster.ChangeWipLocation(hb, EventType.TrackIn);

						lot.CurrentPlan.LoadedResource = eqp;
					}
					else
					{
						aeqp.AddOutBuffer(hb);

					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="factory"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		/// <param name="prevReturnValue"/>
		/// <returns/>
		public bool LOCATE_FOR_OTHERS0(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, bool prevReturnValue)
		{
			FabLot lot = hb.ToFabLot();

			if (lot.LotID == "TH9A1308N3B")
				Console.WriteLine();

			if (lot.IsHold || lot.IsMove)
			{
				var router = EntityControl.Instance;

				string dispatchKey = router.GetLotDispatchingKey(hb);
				DispatchingAgent da = factory.GetDispatchingAgent(dispatchKey);

				lot.IsInitHold = true;
				da.Take(hb);

				return true;
			}

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="factory"/>
		/// <param name="hb"/>
		/// <param name="handled"/>
		public void LOCATE_FOR_DISPATCH1(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
		{
			FabLot lot = hb.ToFabLot();

			if (CheckSimulationRunType(lot) == false)
				return;

			if (hb.IsFinished)
			{
				factory.Router.AddInitial((Entity)hb, hb.IsFinished);
			}
			else
			{
				//InPortWip 처리
				if (InitInPortWip(factory, hb))
					return;

				var router = EntityControl.Instance;
				string dispatchKey = router.GetLotDispatchingKey(hb);

				DispatchingAgent da = factory.GetDispatchingAgent(dispatchKey);

				if (da == null)
				{
					if (factory.DispatchingAgents.Count > 0)
					{
						ModelContext.Current.ErrorLister.Write("Entity/WipInit/LocateForDispatch", Mozart.DataActions.ErrorType.Warning, Strings.CAT_SIM_SECONDRESOURCE,
							string.Format(Strings.WARN_INVALID_IMPLEMENTATION, "Entity/WipInit/LocateForDispatch"));
						da = factory.DispatchingAgents.FirstOrDefault().Value;
					}
					else
						throw new InvalidOperationException(Strings.EXCEPTION_NO_REGISTERED_DISPATCHINGAGENT);
				}

				InFlowMaster.ChangeWipLocation(hb, EventType.StartTOWait);

				da.Take(hb);

			}
		}

		private bool InitInPortWip(AoFactory factory, IHandlingBatch hb)
		{
			FabLot lot = hb.ToFabLot();

			//if (lot.LotID == "TH961377N00")
			//	Console.WriteLine();

			if (lot.IsInPortWip == false)
				return false;

			var wipInitiator = ServiceLocator.Resolve<WipInitiator>();
			string eqpID = wipInitiator.GetLoadingEquipment(hb);

			AoEquipment eqp;
			if (string.IsNullOrEmpty(eqpID) || factory.Equipments.TryGetValue(eqpID, out eqp) == false)
			{
				Logger.Warn("Can't Locate InportWip to Eqp {0}, check input data!", eqpID ?? "-");

				#region Write ErrorHistory
				ErrHist.WriteIf(string.Format("LocateInportWip{0}", lot.LotID),
					ErrCategory.SIMULATION,
					ErrLevel.INFO,
					lot.CurrentFactoryID,
					lot.CurrentShopID,
					lot.LotID,
					lot.CurrentProductID,
					lot.CurrentProductVersion ?? lot.Wip.ProductVersion,
					lot.CurrentProcessID,
					eqpID,
					lot.CurrentStepID,
					"NOT FOUND EQP",
					"Can't Locate InportWip");
				#endregion

				return false;
			}
			else
			{
				FabAoEquipment feqp = eqp.ToFabAoEquipment();

				//Inport Wip (M잔여 수량 체크 X)
				if (EqpArrangeMaster.IsLoadable(feqp, lot, false) == false)
				{
					#region Write ErrorHistory
					ErrHist.WriteIf(string.Format("LocateInportWip{0}", lot.LotID),
						ErrCategory.SIMULATION,
						ErrLevel.INFO,
						lot.CurrentFactoryID,
						lot.CurrentShopID,
						lot.LotID,
						lot.CurrentProductID,
						lot.CurrentProductVersion ?? lot.Wip.ProductVersion,
						lot.CurrentProcessID,
						eqpID,
						lot.CurrentStepID,
						"NOT FOUND EQP_ARRANGE",
						"Can't Locate InportWip");
					#endregion

					return false;
				}

				if (feqp.InitInPortWips == null)
					feqp.InitInPortWips = new List<IHandlingBatch>();

				feqp.InitInPortWips.Add(hb);

				InFlowMaster.ChangeWipLocation(hb, EventType.StartTOWait);

				return true;
			}

		}

		private bool CheckSimulationRunType(FabLot lot)
		{

			if (SimHelper.IsTftRunning)
			{
				if (lot.CurrentShopID == Constants.CellShop)
					return false;
			}
			else
			{
				if (lot.CurrentShopID != Constants.CellShop)
					return false;
			}

			return true;
		}

		public void ON_BEGIN_INIT0(AoFactory factory, IList<IHandlingBatch> wips, ref bool handled)
		{
			foreach (var hb in wips)
			{
				var lots = hb.ToList();
				QTimeMaster.SetWipStayHours(lots);
			}
		}

	}
}
