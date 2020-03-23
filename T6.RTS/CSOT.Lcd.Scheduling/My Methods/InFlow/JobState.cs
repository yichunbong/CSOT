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
namespace CSOT.Lcd.Scheduling
{
	/// <summary>
	/// Product/Process
	/// </summary>
	[FeatureBind()]
	public partial class JobState : IEquatable<JobState>
	{
		public string ProductID { get; private set; }
		public string OwnerType { get; private set; }

		public FabProduct SampleProd { get; private set; }

		#region CreateKey
		public static string GetKey(FabLot lot)
		{
			return CreateKey(lot.CurrentProductID, lot.OwnerType);
		}

		public static string CreateKey(string productID, string ownerType)
		{
			return LcdHelper.CreateKey(productID, ownerType);
		}

		public override string ToString()
		{
			return LcdHelper.CreateKey(this.ProductID, this.OwnerType);
		}

		#endregion

		private Dictionary<FabStep, WipVar> _stepWips;
		private Dictionary<string, WipProfile> _wipProfile;
		private Dictionary<FabStep, StepRouteInfo> _stepRouteInfoDic;
		

		public Dictionary<FabStep, WipVar> StepWips { get { return _stepWips; } }

		List<FabStep> _backwardRoute = new List<FabStep>();
		List<FabStep> _forwardRoute = new List<FabStep>();

		Dictionary<FabStep, List<FabStep>> _cachePrevStepList = new Dictionary<FabStep, List<FabStep>>();
		Dictionary<FabStdStep, List<FabStep>> _cacheNextStepList = new Dictionary<FabStdStep, List<FabStep>>();

		Dictionary<FabStep, FabStep> _cachePrevStep = new Dictionary<FabStep, FabStep>();

		public JobState(FabLot lot)
			: this(lot.CurrentShopID, lot.FabProduct, lot.CurrentFabStep, lot.OwnerType)
		{

		}

		public JobState(string shopID, FabProduct product, FabStep step, string ownerType)
		{
			this.SampleProd = product;
			this.ProductID = product.ProductID;
			this.OwnerType = ownerType;			

			_stepWips = new Dictionary<FabStep, WipVar>();
			_wipProfile = new Dictionary<string, WipProfile>();
			_stepRouteInfoDic = new Dictionary<FabStep, StepRouteInfo>();

			// 순서 중요함
			//이전 Step
			List<FabStep> list = BuildBackwardStepChange(step, product);
			BuildStepRouteInfo(list);

			//기준 Step
			BuildStepRouteInfo(step);

			AddRoute(step, true);

			//Next Step
			list = BuildForwardStepChange(step, product);
			BuildStepRouteInfo(list);
		}

		#region Build

		private void BuildStepRouteInfo(List<FabStep> list)
		{
			if (list == null)
				return;

			foreach (FabStep step in list)
			{
				BuildStepRouteInfo(step);
			}
		}

		private void BuildStepRouteInfo(FabStep step)
		{
			if (this.ProductID == "TH425A2AB000" && step.StepID == "B300")
				Console.WriteLine();


			StepRouteInfo info = new StepRouteInfo(this, step);
			_stepRouteInfoDic[step] = info;


		}

		private List<FabStep> BuildForwardStepChange(FabStep step, FabProduct prod)
		{
			List<FabStep> list = new List<FabStep>();
			FabProduct orgProd = prod;

			FabStep prevStep = step;

			while (true)
			{
				FabStep nextStep = prevStep.GetNextStep(prod, ref prod);

				if (nextStep == null)
					break;

				if (orgProd.ProductID != prod.ProductID)
					break;

				if (list.Contains(nextStep))
					break;

				AddRoute(nextStep, true);

				list.Add(nextStep);

				prevStep = nextStep;
			}

			return list;
		}

		private List<FabStep> BuildBackwardStepChange(FabStep step, FabProduct prod)
		{
			List<FabStep> list = new List<FabStep>();
			FabProduct orgProd = prod;

			while (true)
			{
				FabStep prevStep = step;
				FabProduct prevProd = prod;

				bool changeProd = prod.TryGetPrevStepInfo(ref step, ref prod);

				if (step == null)
					break;

				if (changeProd && (orgProd.ProductID != prod.ProductID))
					break;

				if (list.Contains(step))
					break;

				AddRoute(step, false);

				list.Add(step);
			}

			return list;
		}

		private void AddRoute(FabStep step, bool isFirst)
		{
			AddBackwardRoute(step, isFirst);

			AddForwardRoute(step, isFirst);
		}

		private void AddBackwardRoute(FabStep step, bool isFirst)
		{
			if (_backwardRoute.Contains(step))
				return;

			int index = isFirst ? 0 : _backwardRoute.Count;

			_backwardRoute.Insert(index, step);
		}

		private void AddForwardRoute(FabStep step, bool isFirst)
		{
			if (_forwardRoute.Contains(step))
				return;

			int index = isFirst ? _forwardRoute.Count : 0;

			_forwardRoute.Insert(index, step);
		}
		#endregion

		public void AddWipVar(LotLocation lotLocation)
		{
			FabLot lot = lotLocation.Lot;
			FabStep step = lotLocation.Location;

			WipVar wipVar;
			if (_stepWips.TryGetValue(step, out wipVar) == false)
				_stepWips.Add(step, wipVar = new WipVar(this, step));

			wipVar.Add(lotLocation);
		}

		public void RemoveWipVar(LotLocation lotLocation)
		{
			RemoveWip(lotLocation.WipType, lotLocation.Lot, lotLocation.Location);
		}

		public void RemoveWip(WipType type, FabLot lot, FabStep step)
		{
			WipVar wipVar;

			_stepWips.TryGetValue(step, out wipVar);

			if (wipVar == null)
				return;

			wipVar.Remove(type, lot);
		}
		
		public int GetLoadedEqpCount(FabStep step, string productVersion, bool recalculate)
		{
			string stdStepSeq = step.StdStepID;
			StepRouteInfo dsi = GetStepRouteInfo(step);
			if (dsi == null)
				return 0;

			if (recalculate)
			{
				//InFlowAgent.GetFabManager(step.StdStep.DspEqpGroup);
			}


			return dsi.GetLoadedCout(productVersion);
		}

		public StepRouteInfo GetStepRouteInfo(FabStep step)
		{
			StepRouteInfo info;
			_stepRouteInfoDic.TryGetValue(step, out info);

			return info;
		}

		#region Agent Update


		public void UpdateLoadedEqpList(Dictionary<string, AoEquipment> eqpDic)
		{
			foreach (StepRouteInfo it in _stepRouteInfoDic.Values)
			{
				it.UpdateLoadedEqpList();
			}
		}


		#endregion

		public List<FabLot> GetHoldWipList(FabStep step, string prodVer)
		{
			List<FabLot> result = new List<FabLot>();
			var list = GetStepWipList(step, WipType.Wait, prodVer);

			foreach (var item in list)
			{
				if (item.IsHold)
					result.Add(item);
			}

			return result;
		}

		public List<FabLot> GetStepWipList(FabStep step, WipType wipType)
		{
			WipVar wipVar;
			if (_stepWips.TryGetValue(step, out wipVar))
				return wipVar.GetWipList(wipType);

			return new List<FabLot>();
		}

		public List<FabLot> GetStepWipList(FabStep step, WipType wipType, string productVer)
		{
			List<FabLot> list = GetStepWipList(step, wipType);
			List<FabLot> result = new List<FabLot>();

			if (list == null)
				return result;

			foreach (var lot in list)
			{
				if (LcdHelper.IsEmptyID(productVer))
					result.Add(lot);
				else if (lot.OrigProductVersion == productVer)
					result.Add(lot);
			}

			return result;
		}

		//TODO : jung, Master에 있어야함.
		public List<FabLot> GetStepWipList(List<JobState> jobList, FabStep step, WipType wipType, string ProductVer)
		{
			List<FabLot> result = new List<FabLot>();

			if (jobList == null || jobList.Count == 0)
				return result;

			foreach (var item in jobList)
				result.AddRange(item.GetStepWipList(step, wipType, ProductVer));

			return result;
		}

		//TODO : jung, Master에 있어야 함
		private int GetStepWips(List<JobState> jobList, FabStep step, WipType wipType, string productVersion)
		{
			if (jobList == null || jobList.Count == 0)
				return 0;

			int wips = 0;
			foreach (JobState jobState in jobList)
				wips += jobState.GetStepWips(step, wipType, productVersion);

			return wips;
		}


		/// <summary>
		/// Step의 수량 구하는 함수
		/// </summary>
		public int GetStepWips(FabStep step, WipType wipType, string productVersion = null)
		{
			if(productVersion == null)
				productVersion = Constants.NULL_ID;

			WipVar wipVar;
			if (_stepWips.TryGetValue(step, out wipVar))
			{
				return wipVar.GetWips(wipType, productVersion);
			}

			return 0;
		}

		public decimal GetInflowWip(WipProfile profile, decimal flowHours)
		{
			if (profile == null || profile.LastContPt == null)
				return 0;

			decimal inFlowQty = (int)profile.ArriveWithin(flowHours);

			return inFlowQty;
		}
			   
		/// <summary>
		/// 기준 Step 앞 Step을 찾습니다.
		/// </summary>
		//private FabStep GetPrevStep(FabStep step)
		//{
		//	FabStep prev = null;
		//	if (_cachePrevStep.TryGetValue(step, out prev) == false)
		//	{
		//		bool isPrev = false;

		//		foreach (FabStep rStep in _backwardRoute)
		//		{
		//			if (step == rStep)
		//				isPrev = true;

		//			if (isPrev == false || step == rStep)
		//				continue;

		//			prev = rStep;
		//			break;
		//		}

		//		_cachePrevStep.Add(step, prev);
		//	}

		//	return prev;
		//}

		/// <summary>
		/// TargetStep 이전 Step들을 반환함.
		/// </summary>
		private IEnumerable<FabStep> GetPrevSteps(FabStep targetStep)
		{
			List<FabStep> list;
			if (_cachePrevStepList.TryGetValue(targetStep, out list))
				return list;

			_cachePrevStepList.Add(targetStep, list = new List<FabStep>());

			bool isPrev = false;
			foreach (FabStep rStep in _backwardRoute)
			{
				if (targetStep == rStep)
					isPrev = true;

				if (isPrev == false || targetStep == rStep)
					continue;

				list.Add(rStep);
			}

			return list;
		}

		internal List<FabLot> GetPrevStepWipList(FabStep currentStep, WipType wipType, string productVersion)
		{
			List<FabLot> list = new List<FabLot>();
			var prevSteps = currentStep.GetPrevSteps(this.ProductID);

			foreach (FabStep prev in prevSteps)
			{
				if (currentStep.NeedVerCheck == false || prev.NeedVerCheck == false)
					productVersion = Constants.NULL_ID;

				list.AddRange(GetStepWipList(prev, wipType, productVersion));
			}

			return list;
		}

		//internal List<FabLot> GetPrevStepWaitWipList(FabStep currentStep, string productVersion)
		//{
		//	return GetPrevStepWipList(currentStep, WipType.Wait, productVersion);
		//}

		internal List<FabLot> GetPrevStepRunWipList(FabStep currentStep, string productVersion)
		{
			return GetPrevStepWipList(currentStep, WipType.Run, productVersion);
		}

		/// <summary>
		/// 나의 직전 Step(Sub포함) Target시간+Move시간(잔여시간) 이내에 TrackOut 될 수 있는 Lot수량
		/// </summary>
		internal int GetPrevStepRunWipQty(AoEquipment aeqp, FabStep currentStep, string productVersion, DateTime targetTime)
		{
			List<FabLot> runWips = GetPrevStepWipList(currentStep, WipType.Run, productVersion);

			int qty = 0;
			foreach (var lot in runWips)
			{
				if (EqpArrangeMaster.IsLoadable_CheckOnly(aeqp as FabAoEquipment, lot))
					continue;

				FabPlanInfo plan = lot.CurrentFabPlan;

				if (plan.IsLoaded)
				{
					AoEquipment prevEqp = AoFactory.Current.GetEquipment(plan.LoadedResource.Key);
					AoProcess proc = prevEqp.Processes[0];
					Time tkOutTime = proc.GetUnloadingTime(lot);
					tkOutTime += TransferMaster.GetTransferTime(prevEqp, aeqp);

					if (targetTime < tkOutTime)
						continue;
				}
				else
				{
					Time tkOutTime = plan.TrackInTime + plan.AoBucketTime;

					if (targetTime < tkOutTime)
						continue;
				}

				qty += lot.UnitQty;
			}

			return qty;
		}


		internal int GetCurrenStepWaitWipQty(AoEquipment aeqp, FabStep currentStep, string productVersion, decimal allowRunDownTime)
		{
			if (allowRunDownTime <= 0)
				return 0;

			FabAoEquipment eqp = aeqp.ToFabAoEquipment();

			List<FabLot> wips = GetStepWipList(currentStep, WipType.Wait, productVersion);

			int qty = 0;
			foreach (var lot in wips)
			{
				//해당 설비에서 로딩가능한 대기재공인지 Lot
				if (eqp != null && EqpArrangeMaster.IsLoadable_CheckOnly(eqp, lot) == false)
					continue;

				//잔여 AllowTime 안에 Hold가 풀리는지
				if (lot.IsHold)
				{
					Time remainHold = lot.HoldTime - (eqp.NowDT - lot.HoldStartTime);

					if ((decimal)remainHold.TotalHours > allowRunDownTime)
						continue;
				}

				//Min Qtime (최소대기시간) 내에 풀리는지?
				if (lot.CurrentFabPlan.LotFilterInfo.FilterType == DispatchFilter.MinQtime)
				{
					if ((decimal)lot.CurrentFabPlan.LotFilterInfo.RemainMinStayTime.TotalHours > allowRunDownTime)
						continue;
				}

				qty += lot.UnitQty;
			}

			return qty;
		}
			   		 
		//public int GetIsolatedWips(FabStep step)
		//{
		//    WipProfile inflow = GetWipProfile(step);

		//    if (inflow == null || inflow.LastContPt == null)
		//        return 0;

		//    //float flowSec = 0;

		//    int inFlowQty = 0;
		//    FabStep prev = GetPrevStep(step);
		//    if (prev != null)
		//        inFlowQty = (int)inflow.ArriveWithin(prev, WipType.Run);

		//    return inFlowQty;
		//}

		#region IEquatable<JobState> 멤버

		public bool Equals(JobState other)
		{
			if (other == null)
				return false;

			if (object.ReferenceEquals(this, other))
				return true;

			return this.ProductID == other.ProductID;
		}

		public override int GetHashCode()
		{
			return this.ProductID.GetHashCode();
		}

		#endregion

		public int GetMatchQty(List<FabLot> Wips, FabLot lot)
		{
			string shopID = lot.CurrentShopID;
			string productID = lot.CurrentProductID;
			string productVer = lot.CurrentProductVersion;
			string ownerType = lot.OwnerType;

			int qty = 0;
			foreach (var item in Wips)
			{
				if (item.CurrentShopID != shopID)
					continue;

				if (item.CurrentProductID != productID)
					continue;

				if (item.CurrentProductVersion != productVer)
					continue;

				if (item.OwnerType != ownerType)
					continue;

				qty += lot.UnitQty;
			}

			return qty;
		}
	}

	/// <summary>
	/// Product/Process/Step
	/// </summary>
	public class WipVar
	{
		public JobState Parent { get; private set; }
		public FabStep Step { get; private set; }

		public Dictionary<WipType, List<FabLot>> WipList { get; private set; }

		public List<FabLot> WaitList { get { return GetWipList(WipType.Wait); } }
		public List<FabLot> RunList { get { return GetWipList(WipType.Run); } }

		public int WaitQty { get { return this.WaitList == null ? 0 : this.WaitList.Sum(p => p.UnitQty); } }
		public int RunQty { get { return this.RunList == null ? 0 : this.RunList.Sum(p => p.UnitQty); } }


		public WipVar(JobState state, FabStep step)
		{
			this.Parent = state;
			this.Step = step;

			this.WipList = new Dictionary<WipType, List<FabLot>>();
		}


		public void Add(LotLocation location)
		{
			List<FabLot> list = GetWipList(location.WipType, true);

			int index = list.BinarySearch(location.Lot, new CompareHelper.WipVarComparer());

			if (index < 0)
				index = ~index;

			list.Insert(index, location.Lot);
		}

		public void Remove(LotLocation location)
		{
			Remove(location.WipType, location.Lot);
		}

		public void Remove(WipType type, FabLot lot)
		{
			List<FabLot> list = GetWipList(type);

			if (list != null)
				list.Remove(lot);
		}


		public List<FabLot> GetWipList(WipType wipType, bool create)
		{
			List<FabLot> list;

			WipList.TryGetValue(wipType, out list);

			if (create && list == null)
				WipList.Add(wipType, list = new List<FabLot>());

			return list;
		}

		public List<FabLot> GetWipList(WipType wipType)
		{
			if (wipType == WipType.Total)
			{
				List<FabLot> list = new List<FabLot>();

				List<FabLot> wait = GetWipList(WipType.Wait, false);
				List<FabLot> run = GetWipList(WipType.Run, false);

				if (wait != null)
					list.AddRange(wait);

				if (run != null)
					list.AddRange(run);


				return list;
			}
			else
				return GetWipList(wipType, false);
		}


		public int GetWips(WipType wipType, string productVersion)
		{
			List<FabLot> list = GetWipList(wipType);

			if (list == null)
				return 0;

			int wipQty = 0;
			foreach (FabLot lot in list)
			{
				if (LcdHelper.IsEmptyID(productVersion))
				{
					wipQty += lot.UnitQty;
				}
				else if (lot.OrigProductVersion == productVersion)
				{
					wipQty += lot.UnitQty;
				}
			}

			return wipQty;
		}

		public override string ToString()
		{
			return string.Format("{0}/{1}", this.Parent.ToString(), this.Step.ToString());
		}



	}

	public class StepRouteInfo
	{
		public JobState Parent { get; private set; }
		public FabStep Step { get; private set; }

		public FabProduct Product { get { return this.Parent.SampleProd; } }
		public FabStdStep StdStep { get { return this.Step.StdStep; } }

		public string ProcessID { get { return this.Product.ProcessID; } }
		public string ProductID { get { return this.Parent.ProductID; } }
		public string OwnerType { get { return this.Parent.OwnerType; } }		

		public decimal TactSec;
		public decimal RunTAT;   //Sec
		public decimal WaitTAT;  //Sec

		public List<string> LoadableEqps = new List<string>();
		public List<AoEquipment> LoadableEquipments = new List<AoEquipment>();
		public List<string> LoadedEqps = new List<string>(); // FabManager에서 Update 함

		public List<string> VersionList = new List<string>();
		public Dictionary<string, StepRouteByProdVersion> _verList = new Dictionary<string, StepRouteByProdVersion>();

		public StepRouteInfo(JobState state, FabStep step, string productVersion)
		{
			this.Parent = state;
			this.Step = step;
		}

		public StepRouteInfo(JobState state, FabStep step)
		{
			this.Parent = state;
			this.Step = step;

			BuildStepRouteInfo(Constants.NULL_ID);

			CreateByVersionInfo();
		}

		public void BuildStepRouteInfo(string productVer)
		{
			List<string> eqps = EqpArrangeMaster.GetLoadableEqpList(this.StdStep, this.ProductID, productVer);

			if (eqps != null)
			{
				this.LoadableEqps.AddRange(eqps);
				AddLoadableAoEqp(AoFactory.Current.Equipments);
			}

			//CHECK : jung : LoadableEqp 수량 중복으로 계산함.바로 위에도 있고 해당 함수안에도 있음.
			this.TactSec = TimeHelper.GetAvgTactTime(this.Step, this.Product, productVer);
			this.RunTAT = TimeHelper.GetAvgProcTime(this.Step, this.Product, productVer);
			
			//step.LeadTime Hour --> Sec 변환
			var tatInfo = this.Step.GetTat(this.ProductID, true);
			if (tatInfo != null)
				this.WaitTAT = Convert.ToDecimal(tatInfo.TAT * 60) - this.RunTAT;

			if (this.WaitTAT < 0)
				this.WaitTAT = 600;
		}

		private void CreateByVersionInfo()
		{
			List<string> vers = EqpArrangeMaster.GetProdVerList(this.StdStep, this.ProductID, Constants.NULL_ID);

			foreach (var item in vers)
			{
				string ver = item;
				if (LcdHelper.IsAllID(item))
					ver = Constants.NULL_ID;

				if (this.VersionList.Contains(ver) == false)
					this.VersionList.Add(ver);
			}

			foreach (var item in VersionList)
			{
				StepRouteByProdVersion info;
				if (_verList.TryGetValue(item, out info) == false)
					_verList.Add(item, info = new StepRouteByProdVersion(this, item));
			}
		}

		public void AddLoadableAoEqp(Dictionary<string, AoEquipment> list)
		{
			foreach (string eqpID in this.LoadableEqps)
			{
				AoEquipment eqp;
				if (list.TryGetValue(eqpID, out eqp) == false)
					continue;

				LoadableEquipments.Add(eqp);
			}
		}

		public virtual void UpdateLoadedEqpList()
		{
			LoadedEqps.Clear();

			foreach (FabAoEquipment eqp in this.LoadableEquipments)
			{
				bool isLast = eqp.IsLastPlan(this.Step.ShopID, 
											 this.Step.StepID, 
											 this.ProductID, 
											 Constants.NULL_ID, //productVersion
											 this.OwnerType, 
											 Constants.NULL_ID,	//ownerID
											 false);

				if (isLast)
					LoadedEqps.Add(eqp.EqpID);
			}

			foreach (var item in this._verList.Values)
			{
				item.UpdateLoadedEqpList();
			}
		}

		internal int GetLoadedCout(string productVersion)
		{
			StepRouteByProdVersion info = GetStepProdVersion(productVersion);

			if (info != null)
				return info.LoadedEqps.Count;

			return this.LoadedEqps.Count;
		}

		internal decimal GetTactSec(string productVersion)
		{

			StepRouteByProdVersion info = GetStepProdVersion(productVersion);
			if (info == null)
				return this.TactSec;

			return info.TactSec;
		}

		internal decimal GetRunTAT(string productVersion)
		{

			StepRouteByProdVersion info = GetStepProdVersion(productVersion);
			if (info == null)
				return this.RunTAT;

			return info.RunTAT;
		}

		internal decimal GetWaitTAT(string productVersion)
		{

			StepRouteByProdVersion info = GetStepProdVersion(productVersion);
			if (info == null)
				return this.WaitTAT;

			return info.WaitTAT;
		}

		private StepRouteByProdVersion GetStepProdVersion(string productVersion)
		{
			if (LcdHelper.IsEmptyID(productVersion))
				return null;

			StepRouteByProdVersion info;
			_verList.TryGetValue(productVersion, out info);

			return info;
		}
	}

	public class StepRouteByProdVersion : StepRouteInfo
	{
		public string ProductVersion { get; private set; }

		public StepRouteByProdVersion(StepRouteInfo info, string productVer)
			: base(info.Parent, info.Step, productVer)
		{
			this.ProductVersion = productVer;

			base.BuildStepRouteInfo(productVer);
		}

		public override void UpdateLoadedEqpList()
		{
			LoadedEqps.Clear();

			foreach (FabAoEquipment eqp in this.LoadableEquipments)
			{
				bool isLast = eqp.IsLastPlan(this.Step.ShopID,
											 this.Step.StepID, 
											 this.ProductID, 
											 this.ProductVersion, 
											 this.OwnerType, 
											 Constants.NULL_ID,	//ownerID
											 true);

				if (isLast)
					LoadedEqps.Add(eqp.EqpID);
			}
		}
	}

	public class LotLocation
	{
		public FabLot Lot { get; private set; }

		public FabStep Location { get; private set; }

		public EventType EventType { get; private set; }

		public WipType WipType { get; private set; }

		public LotLocation(FabLot lot, EventType type)
		{
			this.Lot = lot;
			this.Location = lot.CurrentFabStep;
			this.EventType = type;
			this.WipType = SetWipType();
		}

		private WipType SetWipType()
		{
			WipType type = WipType.Wait;

			if (this.EventType == EventType.TrackIn)
				type = WipType.Run;


			return type;
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}/{3}", this.WipType, this.Location.ToString(), this.Lot.LotID, this.Lot.CurrentProductID);
		}
	}
}
