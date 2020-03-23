using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.SeePlan.DataModel;
using CSOT.Lcd.Scheduling.Inputs;
using Mozart.Task.Execution;

namespace CSOT.Lcd.Scheduling.Logic
{
    public partial class PersistInputs
    {
        private void PersistStepLayerInfo(StepTime st, FabStep step)
        {
            string key = step.GetLayerStepKey();

            StepLayerInfo layerStep;
            if (InputMart.Instance.StepLayerGroups.TryGetValue(key, out layerStep) == false)
            {
                layerStep = new StepLayerInfo();

                layerStep.ShopID = step.ShopID;
                layerStep.LayerID = step.LayerID;
                layerStep.EqpGroup = step.EqpGroup;
                //layerStep.StepPattern = step.GetStepPatternKey();
                layerStep.Key = key;
                layerStep.Count = 0;

                InputMart.Instance.StepLayerGroups.Add(key, layerStep);
            }

            layerStep.CumHarmonicTime += (1f / st.TactTime);
            layerStep.Count++;
        }


        private void BuildStepLayerGroup()
        {
            if (InputMart.Instance.StepLayerGroups.Count <= 0)
                return;

            foreach (var item in InputMart.Instance.StepLayerGroups.Values)
                item.StepGroupTactTime = Convert.ToDecimal(1f / item.CumHarmonicTime) * item.Count;
        }


        private RawWeightFactor FindFactor(string factorID)
        {
            RawWeightFactor factor;
            InputMart.Instance.RawWeightFactor.TryGetValue(factorID, out factor);

            return factor;
        }


        private void BuildWeightFactor(WeightFactor f, WeightPresets item)
        {
            if (f.Name == Constants.WF_ALLOW_RUN_DOWN_TIME)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                decimal inflowHour;
                if (decimal.TryParse(criteria[0], out inflowHour) == false)
                    inflowHour = 0;

                f.Criteria = new object[] { inflowHour };
            }
            else if (f.Name == Constants.WF_MAX_MOVE_LIMIT_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                decimal limitQty;
                if (decimal.TryParse(criteria[0], out limitQty) == false)
                    limitQty = decimal.MaxValue;

                f.Criteria = new object[] { limitQty };
            }
            else if (f.Name == Constants.WF_MIN_MOVEQTY_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);

                int minMoveQty;
                if (int.TryParse(criteria[0], out minMoveQty) == false)
                    minMoveQty = 0;


                f.Criteria = new object[] { minMoveQty };
            }
            else if (f.Name == Constants.WF_STEP_TARGET_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);

                int precedeDay;
                int delayDay;

                if (int.TryParse(criteria[0], out precedeDay) == false)
                    precedeDay = 0;

                if (int.TryParse(criteria[1], out delayDay) == false)
                    delayDay = 0;

                f.Criteria = new object[] { precedeDay, delayDay };
            }
            else if (f.Name == Constants.WF_NEW_EQP_ASSIGN_FILTERING)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);
                double param1;
                if (double.TryParse(criteria[0], out param1) == false)
                    param1 = 0;

                double param2;
                if (double.TryParse(criteria[1], out param2) == false)
                    param2 = 0;

                f.Criteria = new object[] { param1, param2 };
            }
            else if (f.Name == Constants.WF_SETUP_FILTERING)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                decimal arg;
                if (decimal.TryParse(criteria[0], out arg) == false)
                    arg = 0;

                f.Criteria = new object[] { arg };
            }
            else if (f.Name == Constants.WF_REQUIRED_EQP_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);

                double inflowHour;
                if (double.TryParse(criteria[0], out inflowHour) == false)
                    inflowHour = 8;

                double advantage;
                if (double.TryParse(criteria[1], out advantage) == false)
                    advantage = 0;

                f.Criteria = new object[] { inflowHour, advantage };
            }
            else if (f.Name == Constants.WF_SMALL_BATCH_MERGE_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);

                int arg1;
                if (int.TryParse(criteria[0], out arg1) == false)
                    arg1 = 80;

                int arg2;
                if (int.TryParse(criteria[1], out arg2) == false)
                    arg2 = 2;

                f.Criteria = new object[] { arg1, arg2 };
            }
            else if (f.Name == Constants.WF_LOT_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);

                int arg1;
                if (!int.TryParse(criteria[0], out arg1))
                    arg1 = 1;

                int arg2;
                if (!int.TryParse(criteria[1], out arg2))
                    arg2 = 0;

                f.Criteria = new object[] { arg1, arg2 };
            }
            else if (f.Name == Constants.WF_LAST_RUN)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                float value;
                if (float.TryParse(criteria[0], out value) == false)
                    value = 1;

                f.Criteria = new object[] { value };
            }
            else if (f.Name == Constants.WF_SETUP_TIME_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                int value;
                if (int.TryParse(criteria[0], out value) == false)
                    value = 1;

                f.Criteria = new object[] { value };
            }
            else if (f.Name == Constants.WF_MAX_QTIME_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 3);

                int value0;
                if (int.TryParse(criteria[0], out value0) == false)
                    value0 = 1;

                int value1;
                if (int.TryParse(criteria[1], out value1) == false)
                    value1 = 1;

                int value2;
                if (int.TryParse(criteria[2], out value2) == false)
                    value2 = 1;

                f.Criteria = new object[] { value0, value1, value2 };
            }
            else if (f.Name == Constants.WF_LAYER_BALANCE_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                decimal value;
                if (decimal.TryParse(criteria[0], out value) == false)
                    value = 1;

                f.Criteria = new object[] { value };
            }
            else if (f.Name == Constants.WF_NEXT_STEP_CONTINUOUS_PRODUCTION_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 3);

                int value0;
                if (int.TryParse(criteria[0], out value0) == false)
                    value0 = 3;

                int value1;
                if (int.TryParse(criteria[1], out value1) == false)
                    value1 = 120;

                int value2;
                if (int.TryParse(criteria[2], out value2) == false)
                    value2 = 400;

                f.Criteria = new object[] { value0, value1, value2 };
            }
            else if (f.Name == Constants.WF_LAYER_BALANCE_FOR_PHOTO)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                int value;
                if (int.TryParse(criteria[0], out value) == false)
                    value = 1;

                f.Criteria = new object[] { value };
            }
            else if (f.Name == Constants.WF_ASSIGN_STEP_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                int value;
                if (int.TryParse(criteria[0], out value) == false)
                    value = 1;

                f.Criteria = new object[] { value };
            }
            else if (f.Name == Constants.WF_OWNER_TYPE_PRIORITY)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 3);

                f.Criteria = new object[] { criteria };
            }
            else if (f.Name == Constants.WF_CU_DENSITY_3400)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                int acidLimit1;
                if (int.TryParse(criteria[0], out acidLimit1) == false)
                    acidLimit1 = 0;

                f.Criteria = new object[] { acidLimit1 };
            }
            else if (f.Name == Constants.WF_CU_DENSITY_3402)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                int acidLimit2;
                if (int.TryParse(criteria[0], out acidLimit2) == false)
                    acidLimit2 = 0;

                f.Criteria = new object[] { acidLimit2 };
            }
            else if (f.Name == Constants.WF_SMALL_LOT)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 2);

                int unitQyt;
                if (!int.TryParse(criteria[0], out unitQyt))
                    unitQyt = 0;

                int stepCount;
                if (!int.TryParse(criteria[1], out stepCount))
                    stepCount = 0;

                f.Criteria = new object[] { unitQyt, stepCount };
            }
            else if (f.Name == Constants.WF_ALLOW_SMALL_LOT_FILTER)
            {
                var criteria = WeightHelper.ParseCriteria(item.CRITERIA, 1);

                decimal inflowHour;
                if (decimal.TryParse(criteria[0], out inflowHour) == false)
                    inflowHour = 0;

                f.Criteria = new object[] { inflowHour };
            }
        }

        #region Wip
        private void SetAvailableTime(FabWipInfo wip)
        {
            if (wip.IsSubStepWip)
            {
                string holdCode = wip.HoldCode;

                DateTime holdStartTime = LcdHelper.Max(wip.LastTrackInTime, wip.LastTrackOutTime);

                wip.AvailableTime = GetAvailableTime(wip, holdCode, holdStartTime, true);

                CheckWipStateByHoldInfo(wip, holdCode);

                return;
            }

            if (wip.IsHold)
            {
                string holdCode = wip.HoldCode;
                wip.HoldEndTime = GetAvailableTime(wip, holdCode, wip.HoldTime);
                wip.SheetIDEndTime = GetAvailableTime(wip, holdCode, wip.SheetIDTime);
                wip.InspSheetEndTime = GetAvailableTime(wip, holdCode, wip.InspSheetIDTime);

                List<Tuple<string, DateTime>> list = new List<Tuple<string, DateTime>>();
                list.Add(Tuple.Create<string, DateTime>("HOLD", wip.HoldEndTime));
                list.Add(Tuple.Create<string, DateTime>("SHEET_ID", wip.SheetIDEndTime));
                list.Add(Tuple.Create<string, DateTime>("INSP_SHEET_ID", wip.InspSheetEndTime));

                list.Sort(new CompareHelper.WipHoldInfoCompare());

                wip.AvailableTime = list[0].Item2;

                CheckWipStateByHoldInfo(wip, list[0].Item1);
            }
        }

        private DateTime GetAvailableTime(FabWipInfo wip, string holdCode, DateTime holdStartTime, bool isSubStepHold = false)
        {
            DateTime planStartTime = ModelContext.Current.StartTime;

            if (holdStartTime == DateTime.MinValue)
                holdStartTime = planStartTime;

            float holdTime = HoldMaster.GetHoldTime(wip.ShopID, holdCode);

            DateTime holdEndTime = holdStartTime.AddMinutes(holdTime);

            if (holdEndTime > planStartTime)
                return holdEndTime;

            //SubStep Wip의 경우 기준 HoldTime 이후에도 SubStep에 존재시 기본 HoldTime을 추가 반영함(2020.03.05 - by.liujian(유건))
            if (isSubStepHold)
            {
                var defaultSubStepHoldTime = SiteConfigHelper.GetDefaultSubStepHoldTime();
                return planStartTime.AddMinutes(defaultSubStepHoldTime.TotalMinutes);
            }

            return planStartTime;
        }

        private void CheckWipStateByHoldInfo(FabWipInfo wip, string holdType)
        {
            //AvailabaleTime이 MinValue일 경우 바로 가용( DateTime MinValue : HoldEndTime < PlanStartTime 경우임.)
            if (wip.AvailableTime == DateTime.MinValue)
            {
                ErrHist.WriteIf(string.Format("{0}/{1}", wip.LotID, "WipHold"),
                    ErrCategory.PERSIST,
                    ErrLevel.INFO,
                    wip.FactoryID,
                    wip.ShopID,
                    wip.LotID,
                    wip.WipProductID,
                    wip.ProductVersion,
                    wip.WipProcessID,
                    wip.WipEqpID,
                    wip.WipStepID,
                    string.Format("{0} : HoldEndTime < PlanStartTime", holdType),
                    string.Format("Change WIP_STATE : {0} → {1}", wip.CurrentState.ToString(), "WAIT")
                );

                wip.CurrentState = Mozart.SeePlan.Simulation.EntityState.WAIT;
            }
            else if (wip.IsSubStepWip)
            {
                wip.CurrentState = Mozart.SeePlan.Simulation.EntityState.MOVE;
            }
            else
                wip.CurrentState = Mozart.SeePlan.Simulation.EntityState.HOLD;

        }

        private bool IsUnpredictWip(Wip item)
        {
            string shopID = item.SHOP_ID;

            ICollection<UnpredictWip> stepList;
            if (InputMart.Instance.UnpredictWips.TryGetValue(UnpredictType.STEP, out stepList))
            {
                if (IsMatched_UnpredictWip(stepList, shopID, item.STEP_ID))
                    return true;
            }

            ICollection<UnpredictWip> lotList;
            if (InputMart.Instance.UnpredictWips.TryGetValue(UnpredictType.LOT, out lotList))
            {
                if (IsMatched_UnpredictWip(lotList, shopID, item.LOT_ID))
                    return true;
            }

            ICollection<UnpredictWip> cstList;
            if (InputMart.Instance.UnpredictWips.TryGetValue(UnpredictType.CASSETTE, out cstList))
            {
                if (IsMatched_UnpredictWip(cstList, shopID, item.BATCH_ID))
                    return true;
            }

            ICollection<UnpredictWip> ownerIDList;
            if (InputMart.Instance.UnpredictWips.TryGetValue(UnpredictType.OWNER_ID, out ownerIDList))
            {
                if (IsMatched_UnpredictWip(ownerIDList, shopID, item.OWNER_ID))
                    return true;
            }

            return false;
        }

        private bool IsMatched_UnpredictWip(ICollection<UnpredictWip> list, string shopID, string checkValue)
        {
            if (list == null || list.Count == 0)
                return false;

            foreach (var it in list)
            {
                if (it.SHOP_ID != shopID)
                    continue;

                string pattern = it.PATTERN;

                if (string.IsNullOrEmpty(pattern) || LcdHelper.Equals(pattern, "EMPTY"))
                {
                    if (string.IsNullOrEmpty(checkValue) || LcdHelper.IsEmptyID(pattern))
                        return true;
                }
                else
                {
                    if (LcdHelper.Like(checkValue, pattern))
                        return true;
                }
            }

            return false;
        }


        #endregion


        #region Check Data

        private FabProduct CheckProduct(string factoryID, string shopID, string productID, string where, ref bool hasError)
        {
            return CheckProduct(factoryID, shopID, productID, Constants.NULL_ID, where, ref hasError);
        }

        private FabProduct CheckProduct(string factoryID, string shopID, string productID, string productVer, string where, ref bool hasError)
        {
            FabProduct prod = BopHelper.FindProduct(shopID, productID);

            if (prod == null)
            {
                hasError = true;

                ErrHist.WriteIf(where + productID,
                    ErrCategory.PERSIST,
                    ErrLevel.WARNING,
                    factoryID,
                    shopID,
                    Constants.NULL_ID,
                    productID,
                    productVer,
                    Constants.NULL_ID,
                    Constants.NULL_ID,
                    Constants.NULL_ID,
                    "NOT FOUND PRODUCT",
                    string.Format("Table:{0}", where)
                    );
            }

            return prod;
        }

        private string CheckMainProcessID(string factoryID, string shopID, string productID, string stepID, Wip wip, string where, ref bool hasError)
        {
            var proc = BopHelper.FindProcess2(productID, stepID);

            if (proc == null)
            {
                hasError = true;

                ErrHist.WriteIf(where + proc,
                    ErrCategory.PERSIST,
                    ErrLevel.WARNING,
                    factoryID,
                    shopID,
                    Constants.NULL_ID,
                    productID,
                    Constants.NULL_ID,
                    Constants.NULL_ID,
                    Constants.NULL_ID,
                    stepID,
                    "NOT FOUND MAIN_PROCESS",
                    string.Format("Table:{0}", where)
                    );

                return null;
            }

            return proc.ProcessID;
        }


        private FabStep CheckStep(string factoryID, string shopID, string processID, string stepID, string where, ref bool hasError)
        {
            FabStep step = BopHelper.FindStep(shopID, processID, stepID);

            if (step == null)
            {
                hasError = true;

                ErrHist.WriteIf(where + processID + stepID,
                   ErrCategory.PERSIST,
                   ErrLevel.WARNING,
                   factoryID,
                   shopID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   processID,
                   Constants.NULL_ID,
                   stepID,
                   "NOT FOUND STEP",
                   string.Format("Table:{0}", where)
                   );
            }

            return step;
        }

        private FabStdStep CheckStdStep(string factoryID, string shopID, string stepID, string where, ref bool hasError)
        {
            FabStdStep step = BopHelper.FindStdStep(shopID, stepID);

            if (step == null)
            {
                hasError = true;

                ErrHist.WriteIf(where + stepID,
                   ErrCategory.PERSIST,
                   ErrLevel.WARNING,
                   factoryID,
                   shopID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   stepID,
                   "NOT FOUND STD_STEP",
                   string.Format("Table:{0}", where)
                   );
            }

            return step;
        }

        private FabProcess CheckProcess(string factoryID, string shopID, string processID, string where, ref bool hasError)
        {
            FabProcess process = BopHelper.FindProcess(shopID, processID);

            if (process == null)
            {
                hasError = true;

                ErrHist.WriteIf(where + processID,
                   ErrCategory.PERSIST,
                   ErrLevel.WARNING,
                   factoryID,
                   shopID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   processID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   "NOT FOUND PROCESS",
                   string.Format("Table:{0}", where)
                   );
            }

            return process;
        }

        private FabEqp CheckEqp(string factoryID, string shopID, string eqpID, string where, ref bool hasError)
        {
            FabEqp eqp = ResHelper.FindEqp(eqpID);

            if (eqp == null)
            {
                hasError = true;

                ErrHist.WriteIf(where + eqpID,
                   ErrCategory.PERSIST,
                   ErrLevel.WARNING,
                   factoryID,
                   shopID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   Constants.NULL_ID,
                   eqpID,
                   Constants.NULL_ID,
                   "NOT FOUND EQP",
                   string.Format("Table:{0}", where)
                   );
            }

            return eqp;
        }

        #endregion

        private bool IsPhotoStep(Dictionary<string, string> photoStep, FabStdStep stdStep)
        {
            string steps;
            photoStep.TryGetValue(stdStep.ShopID, out steps);

            if (LcdHelper.IsEmptyID(steps))
                return false;

            return steps.Contains(stdStep.StepID);
        }

        private Dictionary<string, string> GetPhotoStepFromConfig()
        {
            string array = SiteConfigHelper.GetDefaultPhotoStep(Constants.ArrayShop);
            string cf = SiteConfigHelper.GetDefaultPhotoStep(Constants.CfShop);

            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add(Constants.ArrayShop, array);
            dic.Add(Constants.CfShop, cf);

            return dic;
        }


        #region STD_STEP
        private void LinkStdStep(Dictionary<string, List<FabStdStep>> dic)
        {
            foreach (List<FabStdStep> list in dic.Values)
            {
                bool isAfterCOA = false;
                FabStdStep prev = null;
                for (int i = 0; i < list.Count; i++)
                {
                    FabStdStep step = list[i];

                    if (i == 0)
                        step.IsInputStep = true;

                    if (prev != null)
                        step.PrevStep = prev;

                    prev = step;

                    if (i + 1 < list.Count)
                        step.NexStep = list[i + 1];

                    if (step.LayerID == "COA")
                        isAfterCOA = true;

                    if (isAfterCOA && BopHelper.IsArrayShop(step.ShopID))
                        step.IsAferCOA = true;
                }
            }
        }

        private void SetLayerBalance(Dictionary<FabStdStep, string> balance)
        {
            foreach (var item in balance)
            {
                FabStdStep step = item.Key;
                FabStdStep toStep = BopHelper.FindStdStep(step.ShopID, item.Value);

                if (toStep == null)
                    toStep = step.Layer.LastStep;

                if (toStep == null)
                    continue;

                step.BalanceToStep = toStep;

                var curr = step;
                while (curr != null)
                {
                    step.BalanceSteps.Add(curr);

                    if (curr == toStep)
                        break;

                    curr = curr.NexStep;
                }
            }
        }

        private void SetMixRun()
        {
            ConfigGroup mixRuns = SiteConfigHelper.GetChamberMixRun();
            ConfigGroup lossInfos = SiteConfigHelper.GetChamberMixRunLoss();

            if (mixRuns == null)
                return;

            foreach (var item in mixRuns.Item.Values)
            {
                string[] steps = item.CodeValue.Split(',');
                if (steps.Length < 2)
                    continue;

                List<FabStdStep> list = new List<FabStdStep>();
                foreach (var stdStepID in steps)
                {
                    FabStdStep step = BopHelper.FindStdStep(Constants.ArrayShop, stdStepID);
                    if (step != null)
                        list.Add(step);
                }

                if (list.Count < 2)
                    continue;

                float lossValue = 1f;

                if (lossInfos != null)
                {
                    ConfigInfo loss;
                    lossInfos.Item.TryGetValue(item.CodeName, out loss);

                    if (loss != null)
                    {
                        if (float.TryParse(loss.CodeValue, out lossValue) == false)
                            lossValue = 1f;
                    }
                }


                foreach (var step in list)
                {
                    foreach (var otherStep in list)
                    {
                        if (step == otherStep)
                            continue;

                        if (step.MixRunPairSteps == null)
                            step.MixRunPairSteps = new List<FabStdStep>();

                        step.MixRunPairSteps.Add(otherStep);
                    }

                    step.IsMixRunStep = true;
                    step.MixCriteria = lossValue;
                }

            }
        }
        #endregion


        #region Mask
        private void BuildFabMask(Tool item, Dictionary<string, FabEqp> dic)
        {
            FabMask mask = CreateHelper.CreateFabMask(item);

            #region 중복체크
            if (InputMart.Instance.FabMask.ContainsKey(mask.ToolID))
            {

                ErrHist.WriteIf(string.Format("LoadTool{0}", mask.ToolID),
                      ErrCategory.PERSIST,
                      ErrLevel.INFO,
                      item.FACTORY_ID,
                      item.SHOP_ID,
                      Constants.NULL_ID,
                      Constants.NULL_ID,
                      Constants.NULL_ID,
                      Constants.NULL_ID,
                      item.EQP_ID,
                      Constants.NULL_ID,
                      "DUPLICATE TOOL_ID",
                      string.Format("Table:Tool → TOOL_ID:{0}", item.TOOL_ID)
                      );

                return;
            }
            #endregion

            if (mask.StateCode == ToolStatus.INUSE)
            {
                bool hasError = false;
                FabEqp eqp = CheckEqp(item.FACTORY_ID, item.SHOP_ID, item.EQP_ID, "Tool", ref hasError);

                if (hasError)
                {
                    #region Write ErrorHist
                    ErrHist.WriteIf(string.Format("LoadTool{0}", mask.ToolID),
                                       ErrCategory.PERSIST,
                                       ErrLevel.INFO,
                                       item.FACTORY_ID,
                                       item.SHOP_ID,
                                       Constants.NULL_ID,
                                       Constants.NULL_ID,
                                       Constants.NULL_ID,
                                       Constants.NULL_ID,
                                       item.EQP_ID,
                                       Constants.NULL_ID,
                                       "NOT FOUND EQP",
                                       string.Format("Table:Tool → TOOL_ID:{0} Change STATE_CODE {1} → {2} ", item.TOOL_ID, item.STATE_CODE, ToolStatus.WAIT.ToString())
                                   );
                    #endregion

                    mask.StateCode = ToolStatus.WAIT;
                }
                else
                {
                    //설비중복체크
                    FabEqp target;
                    if (dic.TryGetValue(mask.ToolID, out target))
                    {
                        #region Write ErrorHist
                        ErrHist.WriteIf(string.Format("LoadTool_EqpCheck{0}", mask.ToolID),
                                     ErrCategory.PERSIST,
                                     ErrLevel.INFO,
                                     item.FACTORY_ID,
                                     item.SHOP_ID,
                                     Constants.NULL_ID,
                                     Constants.NULL_ID,
                                     Constants.NULL_ID,
                                     Constants.NULL_ID,
                                     item.EQP_ID,
                                     Constants.NULL_ID,
                                     "ALREADY USED TOOL",
                                     string.Format("Table:Tool → TOOL_ID:{0} was uesd to EQP_ID:{1}", item.TOOL_ID, target.EqpID)
                                      );
                        #endregion

                        mask.EqpID = Constants.NULL_ID;
                        mask.StateCode = ToolStatus.WAIT;
                    }
                    else
                    {
                        dic.Add(mask.ToolID, target);

                        if (eqp.InitMask == null)
                        {
                            eqp.InitMask = mask;
                        }
                        else
                        {
                            if (eqp.InitMask.StateChangeTime < mask.StateChangeTime)
                            {
                                eqp.InitMask.StateCode = ToolStatus.MOUNT;
                                eqp.InitMask = mask;
                            }
                        }
                    }
                }
            }

            MaskMaster.AddTool(mask);
        }


        private void BuildFabJig(Tool item)
        {
            int qty = item.QTY;

            for (int i = 1; i <= qty; i++)
            {
                string jigID = string.Format("{0}_{1}", item.TOOL_ID, i);

                FabMask mask = CreateHelper.CreateFabMask(item, jigID);

                JigMaster.AddTool(mask);
            }
        }


        #endregion
    }
}

