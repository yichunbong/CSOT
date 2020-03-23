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
using CSOT.Lcd.Scheduling.Persists;
using Mozart.Task.Execution.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using Mozart.SeePlan;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.Lcd.DataModel;
using Mozart.Simulation.Engine;


namespace CSOT.Lcd.Scheduling.Logic
{
	[FeatureBind()]
	public partial class PersistInputs
	{
		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_JobRunMonitor(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			if (RunStateMasterFunc.IsAutoRun() == false)
				return;

			Inputs.JobRunMonitor runState = InputMart.Instance.RunStateMst.SafeGet();

			InputMart.Instance.RunStateMst.OnStart(runState);
		}

		public void OnAction_Config(IPersistContext context)
		{
			ConfigHelper.Init();

			//Config Arge엔 Group이 없음, vxml에서 찾아야 할듯
			//var conf = ConfigHelper.GetConfigParameters();
			//foreach (var item in conf.GetType().GetProperties())
			//{
			//    string name = item.Name;
			//    Type type = item.GetType();
			//    var value = item.GetValue(conf);

			//    ConfigGroup group = ConfigHelper.SafeGetConfigGorup(name);
			//}

			foreach (var item in InputMart.Instance.Config.DefaultView)
			{
				ConfigGroup group = ConfigHelper.SafeGetConfigGorup(item.CODE_GROUP);
				group.AddItem(item);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_StdStep(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			Dictionary<string, string> photoStep = GetPhotoStepFromConfig();

			InputMart.Instance.StdStep.DefaultView.Sort = "AREA_ID, STEP_SEQ";

			Dictionary<string, List<FabStdStep>> dic = new Dictionary<string, List<FabStdStep>>();
			Dictionary<FabStdStep, string> balance = new Dictionary<FabStdStep, string>();

			foreach (StdStep item in InputMart.Instance.StdStep.DefaultView)
			{
				FabStdStep stdStep = CreateHelper.CreateStdStep(item);
				
				Layer layer = BopHelper.GetSafeStdLayer(item.SHOP_ID, item.LAYER_ID);
				layer.AddStdStep(stdStep);

				stdStep.Layer = layer;

				if (stdStep.DefaultArrange.IsNullOrEmpty())
				{
					if (stdStep.StepID == "1300" && stdStep.ShopID == Constants.ArrayShop)
						stdStep.DefaultArrange = "LPOBM";
					else
						stdStep.DefaultArrange = "LPO";
				}

				if (stdStep.RecipeLPOBM.IsNullOrEmpty())
					stdStep.RecipeLPOBM = stdStep.DefaultArrange;

				if (IsPhotoStep(photoStep, stdStep))
					stdStep.IsPhoto = true;

				InputMart.Instance.FabStdStep.ImportRow(stdStep);

				List<FabStdStep> list;
				if (dic.TryGetValue(item.AREA_ID, out list) == false)
					dic.Add(item.AREA_ID, list = new List<FabStdStep>());

				list.Add(stdStep);

				if (LcdHelper.IsEmptyID(item.BALANCE_TO_STEP) == false)
					balance[stdStep] = item.BALANCE_TO_STEP;

				if (stdStep.IsCFShop)
					stdStep.BalanceToStep = stdStep;

				SimHelper.AddDspEqpGroup(stdStep);
			}

			//기본 정보 설정
			LinkStdStep(dic);

			//LayerBalance to_Step 설정
			SetLayerBalance(balance);

			//MixRun
			SetMixRun();
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_ProcStep(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			InputMart.Instance.ProcStep.DefaultView.Sort = "PROCESS_ID, STEP_SEQ, STEP_ID";

			DoubleDictionary<FabProcess, string, PrpInfo> prpInfos = new DoubleDictionary<FabProcess, string, PrpInfo>();

			FabProcess proc = null;

			foreach (ProcStep item in InputMart.Instance.ProcStep.DefaultView)
			{
				string key = BopHelper.GetProcessKey(item.SHOP_ID, item.PROCESS_ID);

				if (proc == null || proc.Key != key)
				{
					proc = CreateHelper.GetSafeProcess(item.FACTORY_ID, item.SHOP_ID, item.PROCESS_ID);
				}

				FabStdStep stdStep = BopHelper.FindStdStep(item.SHOP_ID, item.STEP_ID);

				if (stdStep == null)
				{
					#region Write ErrorHistory
					ErrHist.WriteIf(string.Format("{0}/{1}", item.PROCESS_ID, item.STEP_ID),
								ErrCategory.PERSIST,
								ErrLevel.ERROR,
								item.FACTORY_ID,
								item.SHOP_ID,
								Constants.NULL_ID,
								Constants.NULL_ID,
								Constants.NULL_ID,
								item.PROCESS_ID,
								Constants.NULL_ID,
								item.STEP_ID,
								"NOT FOUND STD_STEP",
								"Table:ProcStep"
								);
					#endregion

					continue;
				}

				//생성자
				FabStep step = CreateHelper.CreateStep(item, stdStep);
				proc.Steps.Add(step);


				#region Single Flow 구성 : Step - NextStep

				PrpInfo info;
				if (prpInfos.TryGetValue(proc, step.StepID, out info) == false)
				{
					info = new PrpInfo(step.StepID);
					prpInfos.Add(proc, step.StepID, info);
				}

				string pathType = step.StepType == "MAIN" ? "Pass" : "Partial";

				PrpInfo.PrpTo prpTo = new PrpInfo.PrpTo(item.NEXT_STEP_ID, pathType, 1);

				if (prpTo.Type == PrpPathType.Pass)
					info.ToList.Insert(0, prpTo);
				else
					info.ToList.Add(prpTo);


				if (proc.Mappings.ContainsKey(item.STEP_ID) == false)
					proc.Mappings.Add(item.STEP_ID, step);

				#endregion
			}


			List<string> invalidProcs = new List<string>();

			foreach (FabProcess item in InputMart.Instance.FabProcess.Values)
			{
				if (item.Steps.Count == 0)
				{

					invalidProcs.Add(item.Key);

					ErrHist.WriteIf(string.Format("{0}/{1}", item.ProcessID, "LoadProcess"),
						 ErrCategory.PERSIST,
						 ErrLevel.ERROR,
						 item.FactoryID,
						 item.ShopID,
						 Constants.NULL_ID,
						 Constants.NULL_ID,
						 Constants.NULL_ID,
						 item.ProcessID,
						 Constants.NULL_ID,
						 Constants.NULL_ID,
						 "NO-STEPS",
						 "Process don't have steps"
						 );

					continue;
				}

				Dictionary<string, PrpInfo> prps = prpInfos.GetDictionary(item);

				BopHelper.BuildProcess(item, item.Mappings, prps);
			}

			foreach (string item in invalidProcs)
				InputMart.Instance.FabProcess.Remove(item);

			foreach (var process in InputMart.Instance.FabProcess.Values)
			{
				if (BopHelper.IsArrayShop(process.ShopID))
				{
					FabStep step = process.FirstStep as FabStep;

					int safe = 0;
					bool isAfterGatePhoto = false;
					while (step != null)
					{
						if (isAfterGatePhoto == false)
							step.NeedVerCheck = false;

						if (isAfterGatePhoto == false && step.IsGatePhoto)
							isAfterGatePhoto = true;

						step = step.GetDefaultNextStep() as FabStep;

						safe++;
						if (safe > 10000)
							break;
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_Product(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (var item in InputMart.Instance.Product.DefaultView)
			{
				bool hasError = false;

				FabProcess proc = CheckProcess(item.FACTORY_ID, item.SHOP_ID, item.PROCESS_ID, "Product", ref hasError);

				if (hasError)
					continue;

				string key = LcdHelper.CreateKey(item.SHOP_ID, item.PRODUCT_ID);

				if (InputMart.Instance.FabProduct.ContainsKey(key))
				{
					#region Write ErrorHist
					ErrHist.WriteIf(
							string.Format("LoadProd{0}", item.PRODUCT_ID),
							ErrCategory.PERSIST,
							ErrLevel.WARNING,
							item.FACTORY_ID,
							item.SHOP_ID,
							Constants.NULL_ID,
							item.PRODUCT_ID,
							Constants.NULL_ID,
							item.PROCESS_ID,
							Constants.NULL_ID,
							Constants.NULL_ID,
							"DUPLICATION PRODUCT",
							"Table:Product");
					#endregion

					continue;
				}

				FabProduct prod = CreateHelper.CreateProduct(item, proc);
				InputMart.Instance.FabProduct.Add(key, prod);

				AddProcessMap(prod, proc);
			}
		}

		private void AddProcessMap(FabProduct prod, FabProcess proc)
		{
			if (prod == null)
				return;

			if (proc == null || proc.Steps.Count == 0)
				return;

			var maps = InputMart.Instance.ProcessMaps;

			string productID = prod.ProductID;

			foreach (FabStep step in proc.Steps)
			{
				string stepID = step.StepID;

				string key = BopHelper.GetProcessKey2(productID, stepID);
				if (maps.ContainsKey(key) == false)
				{
					maps.Add(key, proc);
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_InOutPlan(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			int priority = 0;

			Dictionary<string, FabMoMaster> preMoMater = new Dictionary<string, FabMoMaster>();

			foreach (var item in InputMart.Instance.InOutPlan.DefaultView)
			{

				if (item.IN_OUT != "OUT")
					continue;

				if (item.PLAN_QTY == 0)
					continue;

				if (item.PLAN_DATE < ShopCalendar.SplitDate(context.ModelContext.StartTime))
					continue;

				string prodID = CellCodeMaster.GetCellCode(item.PRODUCT_ID, "00001", CellActionType.STBCUTBANK);

				bool hasError = false;
				FabProduct prod = CheckProduct(item.FACTORY_ID, item.SHOP_ID, prodID, Constants.NULL_ID, "InOutPlan", ref hasError);

				if (hasError)
					continue;

				PreMoPlan mo = CreateHelper.CreatePreMoPlan(item);
				if (mo.LineType == LineType.NONE)
					continue;

				string key = LcdHelper.CreateKey(item.SHOP_ID, item.PRODUCT_ID);

				FabMoMaster mm;
				if (preMoMater.TryGetValue(key, out mm) == false)
				{
					mm = new FabMoMaster(prod, string.Empty);
					mm.FactoryID = item.FACTORY_ID;
					mm.ShopID = item.SHOP_ID;

					preMoMater.Add(key, mm);
				}
				mo.MoMaster = mm;
				mo.DemandID = string.Format("{0}_{1}_{2}_{3}_{4}", item.FACTORY_ID, item.SHOP_ID, item.PRODUCT_ID, item.LINE_TYPE, item.PLAN_DATE.ToString("yyyyMMddHHmmss"));
				mo.Priority = priority++.ToString();

				int index = mm.MoPlanList.BinarySearch(mo as MoPlan, CompareHelper.PlanDateComparer.Default);
				if (index < 0)
					index = ~index;

				mm.MoPlanList.Insert(index, mo);
			}

			//실적 차감
			PegMaster.ActPeg(preMoMater.Values);

			int idx = 0;

			foreach (var master in preMoMater.Values)
			{
				foreach (PreMoPlan item in master.MoPlanList)
				{
					string key = LcdHelper.CreateKey(item.ShopID, item.ProductID);

					FabMoMaster mm;
					if (InputMart.Instance.FabMoMaster.TryGetValue(key, out mm) == false)
					{
						mm = new FabMoMaster(master.Product, string.Empty);
						mm.FactoryID = master.FactoryID;
						mm.ShopID = master.ShopID;

						InputMart.Instance.FabMoMaster.Add(key, mm);
					}

					FabMoPlan mp = CreateHelper.CreateMoPlan(item, mm);
					mp.TargetKey = PegHelper.CreateTargetKey(idx++.ToString(), string.Empty);

					int index = mm.MoPlanList.BinarySearch(mp as MoPlan, CompareHelper.PlanDateComparer.Default);
					if (index < 0)
						index = ~index;
					mm.MoPlanList.Insert(index, mp);
				}


			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_Eqp(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (var item in InputMart.Instance.Eqp.DefaultView)
			{
				if (LcdHelper.ToBoolYN(item.IS_ACTIVE) == false)
					continue;

				if (InputMart.Instance.FabEqp.ContainsKey(item.EQP_ID))
					continue;

				var simType = SimHelper.GetSimType(item.SIM_TYPE);
				if (simType == SimEqpType.None)
				{
					Logger.MonitorInfo("Invaild SIM_TYPE : {0},{1}", item.EQP_ID, item.SIM_TYPE);
					continue;
				}

				if (simType == SimEqpType.Chamber || simType == SimEqpType.ParallelChamber)
				{
					if (item.CHAMBER_COUNT <= 0)
					{
						Logger.MonitorInfo("Invaild CHAMBER_COUNT : {0},{1}", item.EQP_ID, item.CHAMBER_COUNT);
						continue;
					}
				}

				if (item.OPERATING_RATIO <= 0)
				{
					Logger.MonitorInfo("Invaild OPERATING_RATIO : {0},{1}", item.EQP_ID, item.OPERATING_RATIO);
					continue;
				}

				FabEqp eqp = CreateHelper.CreateEqp(item, simType);

				if (InputMart.Instance.FabEqp.ContainsKey(eqp.EqpID) == false)
					InputMart.Instance.FabEqp.Add(eqp.EqpID, eqp);
			}

			////유효하지 않은 EQP 제외
			//CheckVaildEqp();
		}

		//private void CheckVaildEqp()
		//{
		//    var eqps = InputMart.Instance.FabEqp.Values.ToList();
		//    if(eqps == null || eqps.Count == 0)
		//        return;

		//    //1. ParallelChamber
		//    var finds = eqps.FindAll(t => t.SimType == SimEqpType.ParallelChamber);
		//    foreach (var eqp in finds)
		//    {
		//        eqp
		//    }
		//}

		/// <summary>
		/// </summary>
		/// <param name="entity"/>
		/// <returns/>
		public bool OnAfterLoad_PresetInfo(PresetInfo entity)
		{
			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_WeightFactor(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (WeightFactors item in InputMart.Instance.WeightFactors.DefaultView)
			{
				RawWeightFactor factor = new RawWeightFactor();
				factor.FactorID = item.FACTOR_ID;
				factor.IsActive = LcdHelper.ToBoolYN(item.IS_ACTIVE);
				factor.FactorKind = LcdHelper.ToEnum(item.FACTOR_KIND, WeightFactorType.FACTOR);
				factor.FactorDesc = item.FACTOR_DESC;

				if (InputMart.Instance.RawWeightFactor.ContainsKey(item.FACTOR_ID) == false)
					InputMart.Instance.RawWeightFactor.Add(item.FACTOR_ID, factor);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_WeightPresets(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (WeightPresets item in InputMart.Instance.WeightPresets.DefaultView)
			{
				//if (item.FACTOR_ID == "PREVENT_LAYER_CHANGE_FILTER")
				//	Console.WriteLine("B");

				FabWeightPreset preset = WeightHelper.GetSafeWeightPreset(item.PRESET_ID);

				if (preset == null)
					continue;

				RawWeightFactor raw = FindFactor(item.FACTOR_ID);

				if (raw == null || raw.IsActive == false)
					continue;

				if (raw.FactorKind == WeightFactorType.FACTOR)
				{
					FabWeightFactor factor = CreateHelper.CreateWeightFactor(item);

					BuildWeightFactor(factor, item);

					preset.FactorList.Add(factor);
				}
				else
				{
					FabWeightFilter filter = new FabWeightFilter();
					filter.FilterID = raw.FactorID;

					if (preset.FilterList.ContainsKey(filter.FilterID) == false)
						preset.FilterList.Add(filter.FilterID, filter);
				}

			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_Tat(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			InputMart.Instance.Tat.DefaultView.Sort = "PROCESS_ID, PRODUCT_ID, STEP_ID ASC";

			foreach (Tat item in InputMart.Instance.Tat.DefaultView)
			{
				bool hasError = false;

				FabStep step = CheckStep(item.FACTORY_ID, item.SHOP_ID, item.PROCESS_ID, item.STEP_ID, "Tat", ref hasError);

				if (hasError)
					continue;

				StepTat tat = CreateHelper.CreateStepTat(item, item.RUN_TAT, item.WAIT_TAT, true);
				StepTat uTat = CreateHelper.CreateStepTat(item, item.U_RUN_TAT, item.U_WAIT_TAT, false);

				step.AddStepTat(tat);
				step.AddStepTat(uTat);

				step.AddYield(item.PRODUCT_ID, item.YIELD);

			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_Wip(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (Wip item in InputMart.Instance.Wip.DefaultView)
			{
				if (IsUnpredictWip(item))
				{
					PegHelper.WriteUnpegHistory(item, "UNPREDICT_WIP", null);
					ErrHist.WriteLoadWipError(item.LOT_ID, item, ErrLevel.INFO, "UNPREDICT_WIP", "Table:Wip");
					continue;
				}

				string shopID = item.SHOP_ID;
				string productID = item.PRODUCT_ID;

				if (BopHelper.IsCellShop(shopID))
				{
					productID = CellCodeMaster.GetCellCode(item.PRODUCT_ID, item.PRODUCT_VERSION);

					//CELL의 CF 재공 사용안함.
					if (item.PRODUCT_ID.StartsWith("F"))
					{
						ErrHist.WriteLoadWipError(item.PRODUCT_ID, item, ErrLevel.INFO, "CELL:CF WIP", "Table:Wip");
						continue;
					}
				}

				string processID = string.Empty;

				#region Vaild Check
				bool hasError = false;

				FabProduct prod = CheckProduct(item.FACTORY_ID, item.SHOP_ID, productID, item.PRODUCT_VERSION, "Wip", ref hasError);

				if (hasError)
				{
					PegHelper.WriteUnpegHistory(item, "NOT FOUND PRODUCT", null);
					ErrHist.WriteLoadWipError(item.PRODUCT_ID, item, ErrLevel.ERROR, "NOT FOUND PRODUCT", "Table:Wip");
					continue;
				}

				processID = prod.FabProcess.ProcessID;

				if (processID != item.PROCESS_ID)
				{
					//SubStep의 Process 변화 확인
					string key = string.Format("Wip_ProcCheck{0}", item.LOT_ID);
					ErrHist.WriteLoadWipError(key, item, ErrLevel.INFO, "FIND MAIN PROCESS", string.Format("{0} → {1}", item.PROCESS_ID, processID));
				}

				bool isSubStep = IsSubStepWip(item);
				string stepID = isSubStep ? item.MAIN_STEP_ID : item.STEP_ID;

				FabStep step = CheckStep(item.FACTORY_ID, item.SHOP_ID, processID, stepID, "Wip", ref hasError);

				if (hasError)
				{
					PegHelper.WriteUnpegHistory(item, "NOT FOUND STEP", step);
					ErrHist.WriteLoadWipError(item.STEP_ID, item, ErrLevel.ERROR, "NOT FOUND STEP", "Table:Wip");
					continue;
				}

				int qty = (int)item.GLASS_QTY;
				if (LcdHelper.Equals(item.LOT_TYPE, ProductUnit.Panel.ToString()))
					qty = (int)item.PANEL_QTY;

				if (qty <= 0)
				{
					PegHelper.WriteUnpegHistory(item, "Wip Qty is ZERO", step);
					continue;
				}
				#endregion

				FabWipInfo wip = CreateHelper.CreateWip(item, step, prod, qty, isSubStep);

				SetAvailableTime(wip);

				if (InputMart.Instance.FabWipInfo.ContainsKey(wip.LotID) == false)
				{
					InputMart.Instance.FabWipInfo.Add(wip.LotID, wip);
				}
				else
				{
					PegHelper.WriteUnpegHistory(item, "DUPLICATION LOT_ID", step);
					ErrHist.WriteLoadWipError(item.LOT_ID, item, ErrLevel.ERROR, "DUPLICATION LOT_ID", "Table:Wip");
				}
			}
		}

		private bool IsSubStepWip(Wip item)
		{
			string stepID = item.STEP_ID;
			string mainStepID = item.MAIN_STEP_ID;

			bool hasPreInfo = LcdHelper.IsEmptyID(mainStepID) == false;

			if (hasPreInfo)
			{
				if (stepID == mainStepID)
					return false;

				var stdStep = BopHelper.FindStdStep(item.SHOP_ID, stepID);
				if (stdStep != null)
					return false;

				return true;
			}

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_SetupTimes(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (SetupTimes item in InputMart.Instance.SetupTimes.DefaultView)
			{
				if (string.IsNullOrEmpty(item.EQP_GROUP) || string.IsNullOrEmpty(item.EQP_ID))
					continue;

				if (string.IsNullOrEmpty(item.CHANGE_TYPE))
					continue;

				SetupTime info = CreateHelper.CreateSetupTime(item);								
				InputMart.Instance.SetupTime.Add(info.EqpGroup, info);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpStepTime(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (EqpStepTime item in InputMart.Instance.EqpStepTime.DefaultView)
			{
				if (item.TACT_TIME <= 0 || item.PROC_TIME <= 0)
					continue;

				bool hasError = false;

				FabStep step = CheckStep(item.FACTORY_ID, item.SHOP_ID, item.PROCESS_ID, item.STEP_ID, "EqpStepTime", ref hasError);

				if (hasError)
					continue;

				FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "EqpStepTime", ref hasError);
				if (hasError)
					continue;

				//for EqpArrnage
				FabStdStep stdStep = step.StdStep;
				stdStep.AddEqp(eqp);

				StepTime st = CreateHelper.CreateStepTime(item, step);

				List<StepTime> list;
				if (step.StepTimes.TryGetValue(item.PRODUCT_ID, out list) == false)
					step.StepTimes.Add(item.PRODUCT_ID, list = new List<StepTime>());

				list.Add(st);

				if (step.IsArrayShop || step.IsCFShop)
					PersistStepLayerInfo(st, step);

				InFlowMaster.AddStep(item.EQP_ID, item.PRODUCT_ID, step);
			}

			BuildStepLayerGroup();
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_InterShopBom(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (InterShopBom item in InputMart.Instance.InterShopBom.DefaultView)
			{
				bool hasError = false;

				FabProduct fromProd = CheckProduct(item.FROM_SHOP_ID, item.FROM_SHOP_ID, item.FROM_PRODUCT_ID, Constants.NULL_ID, "InterShopBom", ref hasError);

				if (hasError)
					continue;

				FabProduct toProd = CheckProduct(item.TO_SHOP_ID, item.TO_SHOP_ID, item.TO_PRODUCT_ID, Constants.NULL_ID, "InterShopBom", ref hasError);

				if (hasError)
					continue;

				FabStep fromStep = CheckStep(item.FACTORY_ID, item.FROM_SHOP_ID, item.FROM_PROCESS_ID, item.FROM_STEP_ID, "InterShopBom", ref hasError);

				if (hasError)
					continue;

				FabStep toStep = CheckStep(item.FACTORY_ID, item.TO_SHOP_ID, item.TO_PROCESS_ID, item.TO_STEP_ID, "InterShopBom", ref hasError);

				if (hasError)
					continue;

				FabInterBom fromToBom = CreateHelper.CreateFabInterBom(fromProd, fromStep, toProd, toStep, true);
				FabInterBom toFromBom = CreateHelper.CreateFabInterBom(toProd, toStep, fromProd, fromStep, false);

				fromProd.AddNextInterBom(fromToBom);
				toProd.AddPrevInterBom(toFromBom);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_HoldTime(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (HoldTime item in InputMart.Instance.HoldTime.DefaultView)
			{
				HoldMaster.AddHoldInfo(item);
			}

			if (HoldMaster.DefaultHoldTime == 0)
			{
				Time defaultHoldTime = SiteConfigHelper.GetDefaultHoldTime();
				HoldMaster.SetDefaultHoldTime(defaultHoldTime);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpStatus(Mozart.Task.Execution.Persists.IPersistContext context)
		{			
			Time defaultDownTime = SiteConfigHelper.GetDefaultDownTime();
			Time statusCheckTime = SiteConfigHelper.GetDefaultEqpStateCheckTime();

			DateTime planStartTime = ModelContext.Current.StartTime;

			foreach (EqpStatus item in InputMart.Instance.EqpStatus.DefaultView)
			{
				string eqpID = item.EQP_ID;
				if (string.IsNullOrEmpty(eqpID))
					continue;

				EqpStatusInfo info = CreateHelper.CreateEqpStatus(item, planStartTime, statusCheckTime, defaultDownTime);

				//DCN
				var find = InputMart.Instance.EqpDcnbyEqpID.FindRows(eqpID).FirstOrDefault();
				if (find != null)
				{
					ReleasePlanMaster.AddEqpState(info);
					continue;
				}

				//Chamber
				FabSubEqp subEqp = ResHelper.FindSubEqp(eqpID);
				if (subEqp != null)
				{
					subEqp.StatusInfo = info;
					subEqp.State = info.Status;
					continue;
				}

				//Eqp
				var eqp = ResHelper.FindEqp(eqpID);
				if (eqp != null)
				{
					eqp.StatusInfo = info;
					eqp.State = info.Status;
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_PMSchedule(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			if (InputMart.Instance.GlobalParameters.ApplyPMSchedule == false)
				return;

			DateTime planStartTime = ModelContext.Current.StartTime;

			foreach (PMSchedules item in InputMart.Instance.PMSchedules.DefaultView)
			{
				//START_TIME이 과거이면 제외 (2019.12.25 - by.liujian(유건))
				if (item.START_TIME < planStartTime)
					continue;

				bool hassError = false;
				FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "PMSchedules", ref hassError);

				FabSubEqp subEqp = null;
				if (eqp == null)
				{
					subEqp = ResHelper.FindSubEqp(item.EQP_ID);

					if (subEqp == null)
						continue;

					eqp = subEqp.Parent as FabEqp;
				}

				FabPMSchedule pm = CreateHelper.CreateFabPMSchedule(item.START_TIME, (int)item.DURATION * 60, ScheduleType.PM, item.ALLOW_AHEAD_TIME, item.ALLOW_DELAY_TIME);
				pm.ScheduleType = DownScheduleType.Custom;

				if (InputMart.Instance.GlobalParameters.ApplyFlexialePMSchedule == false)
					pm.ScheduleType = DownScheduleType.ShiftBackward;

				if (eqp.IsParallelChamber && subEqp != null)
					pm.ComponentID = subEqp.SubEqpID;

				DownMaster.AddPM(eqp, pm);
			}

			//시간 중복 제거
			DownMaster.AddAdjustPmList();
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpRentSchedules(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			DateTime planStartTime = ModelContext.Current.StartTime;
			foreach (var item in InputMart.Instance.EqpRentSchedules.DefaultView)
			{
				//START_TIME이 과거이면 제외 (2019.12.25 - by.liujian(유건))
				if (item.START_TIME < planStartTime)
					continue;

				bool hassError = false;
				FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "EqpRentSchedules", ref hassError);

				if (hassError)
					continue;

				FabPMSchedule pm = CreateHelper.CreateFabPMSchedule(item.START_TIME, (int)item.DURATION * 60, ScheduleType.RENT, item.ALLOW_AHEAD_TIME, item.ALLOW_DELAY_TIME);
				pm.ScheduleType = DownScheduleType.Custom;

				DownMaster.AddPM(eqp, pm);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpMoveTime(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (EqpMoveTime item in InputMart.Instance.EqpMoveTime.DefaultView)
			{
				bool hasError = false;

				FabEqp toEqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.TO_EQP_ID, "EqpMoveTime", ref hasError);

				if (hasError)
					continue;

				FabEqp fromEqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.FROM_EQP_ID, "EqpMoveTime", ref hasError);

				if (hasError)
					continue;

				toEqp.AddTransferTime(item.FROM_EQP_ID, item.TRANSFER_TIME, true);
				fromEqp.AddTransferTime(item.TO_EQP_ID, item.TRANSFER_TIME, false);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_FabInOutAct(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			DateTime planSartTime = ModelContext.Current.StartTime;

			DateTime actStartTime = ShopCalendar.StartTimeOfDayT(planSartTime);
			DateTime actEndTime = TimeHelper.GetRptDate_1Hour(planSartTime);

			InputMart.Instance.FabInOutAct.DefaultView.Sort = "SHOP_ID, RPT_DATE DESC";

			foreach (FabInOutAct item in InputMart.Instance.FabInOutAct.DefaultView)
			{
				if (item.RPT_DATE <= actStartTime || item.RPT_DATE > actEndTime)
					continue;

				InOutAct inAct = CreateHelper.CreateInOutActInfo(item);
				InOutAct outAct = CreateHelper.CreateInOutActInfo(item);

				inAct.Qty = item.IN_QTY;
				outAct.Qty = item.OUT_QTY;

				PegMaster.AddAct(inAct, false);
				PegMaster.AddAct(outAct, true);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpInlineMap(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			InputMart.Instance.EqpInlineMap.DefaultView.Sort = "MAP_TYPE, FROM_STEP, FROM_EQP_ID"; //MapType(sort) : Eqp,Hard 순서 이름이 바뀔경우 코딩필요(현재 문자오름차순) 

			foreach (var item in InputMart.Instance.EqpInlineMap.DefaultView)
			{
				InlineMapType type = LcdHelper.ToEnum(item.MAP_TYPE, InlineMapType.NONE);

				if (type == InlineMapType.NONE)
					continue;

				#region Vaildation
				bool hasError = false;

				FabEqp fromEqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.FROM_EQP_ID, "EqpInlineMap", ref hasError);
				FabEqp toEqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.FROM_EQP_ID, "EqpInlineMap", ref hasError);

				FabStdStep fromStep = CheckStdStep(item.FACTORY_ID, item.SHOP_ID, item.FROM_STEP, "EqpInLineMap", ref hasError);
				FabStdStep toStep = CheckStdStep(item.FACTORY_ID, item.SHOP_ID, item.TO_STEP, "EqpInLineMap", ref hasError);


				if (fromEqp == null || toEqp == null)
					continue;

				if (fromStep == null || toStep == null)
					continue;


				if (fromStep.NexStep != toStep)
				{
					#region WriteErrorHist
					ErrHist.WriteIf("EqpInLineMap" + item.FROM_STEP,
								  ErrCategory.PERSIST,
								  ErrLevel.INFO,
								  item.FACTORY_ID,
								  item.SHOP_ID,
								  Constants.NULL_ID,
								  Constants.NULL_ID,
								  Constants.NULL_ID,
								  Constants.NULL_ID,
								  item.FROM_EQP_ID,
								  item.FROM_STEP,
								  "From-To Step Notmatch",
								  string.Format("FromStep:{0}/ToStep{1}", item.FROM_STEP, item.TO_STEP)
								  );
					#endregion

					continue;
				}
				#endregion

				bool isFrom = item.BASE_POINT.Equals("FROM") ? true : false;

				if (type == InlineMapType.EQP_INLINE)
				{
					fromEqp.MapType = type;
					toEqp.MapType = type;

					fromEqp.ClearFromToEqp();
					toEqp.ClearFromToEqp();

					fromEqp.MapBase = isFrom ? MapBase.FROM : MapBase.TO;
					toEqp.MapBase = isFrom ? MapBase.FROM : MapBase.TO;

					fromEqp.AddToEqp(toEqp);
					toEqp.AddFromEqp(fromEqp);

					fromStep.AddBaseEqp(fromEqp);

					//EqpPairInfo pair = CreateHelper.CreateEqpPairInfo(fromEqp, toEqp, fromStep, toStep);

					//InLineMapMaster.AddEqpPairInfo(pair);
				}
				else
				{
					if (fromEqp.MapType == InlineMapType.EQP_INLINE || toEqp.MapType == InlineMapType.EQP_INLINE)
						continue;

					fromEqp.MapType = type;
					toEqp.MapType = type;

					fromEqp.AddToEqp(toEqp);
					toEqp.AddFromEqp(fromEqp);
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_UnpredictWip(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (var item in InputMart.Instance.UnpredictWip.DefaultView)
			{
				var ctype = LcdHelper.ToEnum(item.CATEGORY, UnpredictType.NONE);
				if (ctype == UnpredictType.NONE)
					continue;

				InputMart.Instance.UnpredictWips.Add(ctype, item);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_Tool(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			if (InputMart.Instance.GlobalParameters.ApplySecondResource == false)
				return;

			Dictionary<string, FabEqp> dic = new Dictionary<string, FabEqp>();

			foreach (Tool item in InputMart.Instance.Tool.DefaultView)
			{
				if (item.TOOL_TYPE == ToolType.MASK.ToString())
					BuildFabMask(item, dic);
				else
					BuildFabJig(item);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_ToolArrange(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			if (InputMart.Instance.GlobalParameters.ApplySecondResource == false)
				return;

			foreach (ToolArrange item in InputMart.Instance.ToolArrange.DefaultView)
			{
				if (LcdHelper.IsEmptyID(item.TOOL_ID))
					continue;

				bool isMask = item.TOOL_TYPE.Equals(ToolType.MASK.ToString());

				FabMask mask = MaskMaster.FindMask(item.TOOL_ID);

				if (mask == null && isMask)
				{
					#region WriteErrorHist
					ErrHist.WriteIf(string.Format("{0}", item.TOOL_ID),
							ErrCategory.PERSIST,
							ErrLevel.INFO,
							item.FACTORY_ID,
							item.SHOP_ID,
							Constants.NULL_ID,
							item.PRODUCT_ID,
							item.PRODUCT_VERSION,
							Constants.NULL_ID,
							item.EQP_ID,
							item.STEP_ID,
							"NOT FOUND TOOL",
							string.Format("Table:ToolArrange → TOOL_ID{0}", item.TOOL_ID)
							);
					#endregion

					return;
				}

				bool hasError = false;
				FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "ToolArrnage", ref hasError);

				if (hasError)
					return;

				FabStdStep stdStep = CheckStdStep(item.FACTORY_ID, item.SHOP_ID, item.STEP_ID, "ToolArrange", ref hasError);

				if (hasError)
					return;

				MaskArrange arr = CreateHelper.CreateMaskArrange(item, mask);

				if (isMask)
				{
					MaskMaster.AddToolArrange(stdStep, arr);
					MaskMaster.AddToolArrange(mask, arr);
				}
				else
				{
					JigMaster.AddToolArrange(stdStep, arr);
					JigMaster.AddMaskToJigArrange(arr, item);
				}

			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_StayHours(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (var item in InputMart.Instance.StayHours.DefaultView)
			{
				bool hasError = false;
				FabProduct prod = CheckProduct(item.FACTORY_ID, item.SHOP_ID, item.PRODUCT_ID, "StayHours", ref hasError);

				if (hasError)
					continue;

				Time minQTime = Time.FromMinutes(item.MIN_HOLD_TIME);
				Time maxQTime = Time.FromMinutes(item.MAX_Q_TIME);

				if (minQTime <= Time.Zero && maxQTime <= Time.Zero)
					continue;

				FabStep fromStep = CheckStep(item.FACTORY_ID, item.SHOP_ID, prod.ProcessID, item.STEP_ID, "StayHours", ref hasError);
				FabStep toStep = fromStep;

				if (string.IsNullOrEmpty(item.TO_STEP_ID) == false)
					toStep = CheckStep(item.FACTORY_ID, item.TO_SHOP_ID, item.TO_PROCESS_ID, item.TO_STEP_ID, "StayHours", ref hasError);

				if (fromStep == null || toStep == null)
					continue;

				//if (fromStep.StepSeq > toStep.StepSeq)
				//    continue;

				if (minQTime > Time.Zero)
				{
					StayHour sh = CreateHelper.CreateStayHour(item, prod, fromStep, toStep, minQTime, QTimeType.MIN);
					QTimeMaster.Add(sh);
				}

				if (maxQTime > Time.Zero)
				{
					StayHour sh = CreateHelper.CreateStayHour(item, prod, fromStep, toStep, maxQTime, QTimeType.MAX);
					QTimeMaster.Add(sh);
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpChamber(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			HashSet<FabEqp> eqpList = new HashSet<FabEqp>();
			var fabSubEqpInfos = InputMart.Instance.FabSubEqp;

			Dictionary<string, string> runModeDic = new Dictionary<string, string>();

			foreach (var item in InputMart.Instance.EqpChamber.DefaultView)
			{
				bool hasError = false;
				FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "ParallelChamber", ref hasError);

				if (hasError)
					continue;

				if (eqp.SimType != SimEqpType.ParallelChamber)
					continue;

				FabSubEqp subEqp = CreateHelper.CreateFabSubEqp(item, eqp);

				string subEqpID = subEqp.SubEqpID;
				if (eqp.SubEqps.ContainsKey(subEqpID))
					continue;

				if (fabSubEqpInfos.ContainsKey(subEqpID))
					continue;

				//Chamber
				eqp.AddSubEqp(subEqp);

				eqp.SubEqps.Add(subEqpID, subEqp);
				fabSubEqpInfos.Add(subEqpID, subEqp);

				runModeDic[subEqpID] = item.RUN_MODE;

				eqpList.Add(eqp);
			}

			//설비에 Chamber 그룹설정
			foreach (FabEqp eqp in eqpList)
			{
				eqp.ChildCount = eqp.SubEqps.Count;
				eqp.SubEqpGroups = new HashSet<SubEqpGroup>();

				Dictionary<string, SubEqpGroup> groupInfo = new Dictionary<string, SubEqpGroup>();
				foreach (FabSubEqp subEqp in eqp.SubEqps.Values)
				{
					string subEqpID = subEqp.SubEqpID;
					string stepID = subEqp.ArrangeStep != null ? subEqp.ArrangeStep.StepID : Constants.NULL_ID;

					SubEqpGroup group;
					if (groupInfo.TryGetValue(stepID, out group) == false)
					{
						group = new SubEqpGroup();

						group.GroupID = stepID;
						group.Parent = eqp;

						groupInfo.Add(stepID, group);
					}

					subEqp.SubEqpGroup = group;
					group.SubEqps.Add(subEqpID, subEqp);

					string runMode;
					if(runModeDic.TryGetValue(subEqpID, out runMode))
						group.CurrentRunMode = runMode;
				}

				eqp.SubEqpGroups.AddRange(groupInfo.Values);
			}

			//filter invaild (SubEqp가 없으면 제외 처리)
			List<FabEqp> list = InputMart.Instance.FabEqp.Values.ToList();
			foreach (var item in list)
			{
				if (item.IsParallelChamber == false)
					continue;

				if (item.SubEqpCount == 0)
					InputMart.Instance.FabEqp.Remove(item.EqpID);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_CellCodeMap(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			InputMart.Instance.CellCodeMap.DefaultView.Sort = "ACTIONTYPE";

			foreach (var item in InputMart.Instance.CellCodeMap.DefaultView)
			{
				CellActionType type = CellCodeMaster.ParseActionType(item.ACTIONTYPE);

				if (type == CellActionType.None)
					continue;

				CellBom bom = CreateHelper.CreateCellBom(item, type);

				CellCodeMaster.AddMaps(bom);
				CellCodeMaster.AddByActionType(bom);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_BankWip(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			foreach (var item in InputMart.Instance.BankWip.DefaultView)
			{
				bool hasError = false;
				FabProduct prod = CheckProduct(item.FACTORY_ID, item.SHOP_ID, item.PRODUCT_ID, "BankWip", ref hasError);

				if (hasError)
				{
					PegHelper.WriteUnpegHistory(item, "NOT FOUND PRODUCT");
					continue;
				}

				BankWipStatus wipStatus = LcdHelper.ToEnum(item.LOT_STATUS, BankWipStatus.None);
				if (wipStatus != BankWipStatus.Released && wipStatus != BankWipStatus.Shipped)
				{
					PegHelper.WriteUnpegHistory(item, string.Format("LOT_STATUS:{0}", item.LOT_STATUS));
					continue;
				}

				FabStep step = BopHelper.GetSafeDummyStep(item.FACTORY_ID, item.SHOP_ID, item.STEP_ID);

				FabWipInfo wip = CreateHelper.CreateWipInfo(item, prod, step, wipStatus);

				if (InputMart.Instance.CellBankWips.ContainsKey(item.LOT_ID))
				{
					ErrHist.WriteIf(string.Format("LoadBankWip{0}", item.LOT_ID),
						ErrCategory.PERSIST,
						ErrLevel.ERROR,
						item.FACTORY_ID,
						item.SHOP_ID,
						item.LOT_ID,
						item.PRODUCT_ID,
						item.PRODUCT_VERSION,
						item.PROCESS_ID,
						Constants.NULL_ID,
						item.STEP_ID,
						"DUPLICATION LOT",
						"Table:BankWip");


					PegHelper.WriteUnpegHistory(item, "DUPLICATION LOT");
					continue;
				}
				else
				{
					InputMart.Instance.CellBankWips.Add(item.LOT_ID, wip);

					InOutProfileMaster.AddWip(wip);
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="context"/>
		public void OnAction_EqpDcn(Mozart.Task.Execution.Persists.IPersistContext context)
		{
			var table = InputMart.Instance.EqpDcn;

			foreach (var entity in table.DefaultView)
			{
				ReleasePlanMaster.AddDcnBucket(entity);
			}
		}

		public void OnAction_EqpRecipeTime(IPersistContext context)
		{
			DateTime planStartTime = ModelContext.Current.StartTime;

			List<string> dupCheck = new List<string>();

			foreach (var item in InputMart.Instance.EqpRecipeTime.DefaultView)
			{
				bool hasError = false;
				FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "EqpRecipeTime", ref hasError);

				if (hasError)
					continue;

				EqpRecipeInfo info = CreateHelper.CreateEqpRecipeInfo(eqp, item);

				if (info.Flag == RecipeFlag.None || info.Flag == RecipeFlag.N)
					continue;

				if (info.DueDate < planStartTime.AddDays(-14))
					continue;

				string key = item.EQP_ID + item.PRODUCT_ID + item.PRODUCT_VERSION + item.STEP_ID + item.CHECK_FLAG + item.TOOL_ID;

				List<EqpRecipeInfo> list;
				if (eqp.RecipeTime.TryGetValue(info.ProductID, out list) == false)
					eqp.RecipeTime.Add(info.ProductID, list = new List<EqpRecipeInfo>());

				if (dupCheck.Contains(key) == false)
				{
					dupCheck.Add(key);
					list.Add(info);
				}
			}
		}

		public void OnAction_ProductMap(IPersistContext context)
		{
			foreach (var item in InputMart.Instance.ProductMap.DefaultView)
			{
				FabProduct prod = BopHelper.FindProduct(item.SHOP_ID, item.PRODUCT_ID);

				if (prod == null)
				{
					ErrHist.WriteIf(string.Format("{0}/{1}", item.PRODUCT_ID, item.MAIN_PRODUCT_ID),
									ErrCategory.PERSIST,
									ErrLevel.INFO,
									item.FACTORY_ID,
									item.SHOP_ID,
									Constants.NULL_ID,
									item.PRODUCT_ID,
									Constants.NULL_ID,
									Constants.NULL_ID,
									Constants.NULL_ID,
									Constants.NULL_ID,
									"NOT FOUND PRODUCT",
									string.Format("Table:ProductMap → MAIN_PRODUCT_ID:{0}", item.MAIN_PRODUCT_ID)
								  );
					continue;
				}

				prod.MainProductID = item.MAIN_PRODUCT_ID;
			}
		}

		public void OnAction_OwnerLimit(IPersistContext context)
		{
			foreach (var item in InputMart.Instance.OwnerLimit.DefaultView)
			{
				string eqpID = item.EQP_ID;
				if (string.IsNullOrEmpty(eqpID))
					continue;

				string key = LcdHelper.CreateKey(item.SHOP_ID, item.STEP_ID, item.OWNER_ID);

				OwnerLimitInfo info;
				if (InputMart.Instance.OwnerLimitInfo.TryGetValue(key, out info) == false)
				{
					info = CreateHelper.CreateOwnerLimitInfo(item);
					InputMart.Instance.OwnerLimitInfo.Add(key, info);
				}

				var list = LcdHelper.Equals(item.LIMIT_TYPE, "Y") ? info.YList : info.NList;
				if (list.Contains(eqpID))
					continue;

				list.Add(eqpID);
			}
		}

		public void OnAction_EqpArrange(IPersistContext context)
		{
			foreach (var item in InputMart.Instance.EqpArrange.DefaultView)
			{
				var info = CreateHelper.CreateEqpArrangeInfo(item);

                if(LcdHelper.Equals(info.LimitType, "M"))
                {
                    MaskMaster.AddLimit(info);
                }
                else
                {
                    EqpArrangeMaster.AddEqpArrangeInfo(info);
                }
			}
		}

		public void OnAction_FixPlanDCN(IPersistContext context)
		{
			var table = InputMart.Instance.FixPlanDCN;

			foreach (var entity in table.DefaultView)
			{
				ReleasePlanMaster.AddFixPlan(entity);
			}
		}

		public void OnAction_BranchStep(IPersistContext context)
		{
			var table = InputMart.Instance.BranchStep;

			foreach (var entity in table.DefaultView)
			{
				BranchStepMaster.AddBranchStep(entity);
			}
		}
    }
}
