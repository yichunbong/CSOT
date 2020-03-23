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
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class ReleasePlanMaster
    {
        public const string AGENT_KEY = Constants.AgentKey_ReleasePlan;
        public static Time CYCLE_TIME = Time.FromHours(3);
        public static string ARRAY_STEP = SiteConfigHelper.GetDefaultDcnStep(Constants.ArrayShop);
        public static string CF_STEP = SiteConfigHelper.GetDefaultDcnStep(Constants.CfShop);
        public static string CELL_STEP = SiteConfigHelper.GetDefaultDcnStep(Constants.CellShop);

        public static DcnMaster DcnMst
        {
            get { return InputMart.Instance.Dashboard.DcnMst; }        
        }                

        public static void Initialize()        
        {
            AddDcnTarget();

            var dcnMst = ReleasePlanMaster.DcnMst;

            dcnMst.Initialize();
        }

        public static void AddDcnBucket(EqpDcn entity)
        {
            if (entity == null)
                return;

            var dcnMst = ReleasePlanMaster.DcnMst;

            string eqpID = entity.EQP_ID;
            if (string.IsNullOrEmpty(eqpID))
                return;

            if (dcnMst.Buckets.Find(t => t.EqpID == eqpID) != null)
                return;

            int dailyCapa = entity.DAILY_CAPA;
            if (dailyCapa <= 0)
                return;

            DateTime planStartTime = ModelContext.Current.StartTime;

            double capacity = 60 * 60 * 24; //24 Hours
            double tactTime = capacity / dailyCapa;

            DcnBucket bucket = new DcnBucket()
            {
                FactoryID = entity.FACTORY_ID,
                EqpGroupID = entity.EQP_GROUP_ID,
                EqpID = eqpID,                
                MainRunShopID = entity.MAIN_RUN_SHOP,
                DailyCapa = dailyCapa,
                Capacity = capacity,
                TactTime = tactTime,
                LastEqpEndTime = planStartTime
            };

            dcnMst.Buckets.Add(bucket);
        }

        public static void AddEqpState(EqpStatusInfo info)
        {
            if (info == null)
                return;

            string eqpID = info.EqpID;
            if (string.IsNullOrEmpty(eqpID))
                return;
                            
            if (info.Status == ResourceState.Up)
                return;

            var dcnMst = ReleasePlanMaster.DcnMst;

            if (dcnMst.EqpStates.ContainsKey(eqpID))
                return;

            dcnMst.EqpStates.Add(eqpID, info);
        }

        private static void AddDcnTarget()
        {
            var entity = InputMart.Instance.ShopInTarget;
            foreach (var inTarget in entity.DefaultView)
            {
                AddDcnTarget(inTarget);
            }
        }

        private static void AddDcnTarget(ShopInTarget inTarget)
        {
            if (inTarget == null)
                return;

            //CELL 제외
            if (BopHelper.IsCellShop(inTarget.ShopID))
                return;

            int qty = (int)inTarget.TargetQty;
            if (qty <= 0)
                return;

            var product = inTarget.Product;
            if(product == null)
                return;

            FabStep inTargetStep = inTarget.TargetStep;
            if (inTargetStep == null)
                return;

            var dcnMst = ReleasePlanMaster.DcnMst;

            string shopID = product.ShopID;

            //ARRAY=A000, CF=C000
            string stepID = ReleasePlanMaster.GetShopStep(shopID);
            DateTime targetDate = inTarget.TargetDate;

            DcnTarget target = new DcnTarget()
            {
                InTarget = inTarget,
                TargetStep = inTargetStep,
                Product = product,
                StepID = stepID,
                TargetDate = targetDate,
                TargetQty = qty
            };

            dcnMst.AllTargets.Add(target);
        }

        public static void AddFixPlan(FixPlanDCN entity)
        {
            if (entity == null)
                return;

            if (entity.PLAN_QTY <= 0)
                return;

            var dcnMst = ReleasePlanMaster.DcnMst;

            string eqpID = entity.EQP_ID;
            if (string.IsNullOrEmpty(eqpID))
                return;

            var prod = BopHelper.FindProduct(entity.SHOP_ID, entity.PRODUCT_ID);
            if (prod == null)
                return;

            List<FixPlanDCN> list;
            if (dcnMst.FixPlans.TryGetValue(eqpID, out list) == false)
                dcnMst.FixPlans.Add(eqpID, list = new List<FixPlanDCN>());

            LcdHelper.AddSort(list, entity, FixPlanComparer.Default);
        }

        public static void Allocate(InOutAgent agent)
        {
            DateTime planEndTime = ModelContext.Current.EndTime;

            var now = agent.NowDT;
            if (now >= planEndTime)
                return;

            var dcnMst = ReleasePlanMaster.DcnMst;

            DateTime startTime = now;
            DateTime endTime = startTime.AddHours(ReleasePlanMaster.CYCLE_TIME.TotalHours);            

            if (endTime > planEndTime)
                endTime = planEndTime;

            dcnMst.Allocate_Init(now, startTime, endTime);

            dcnMst.Allocate_FixPlan(now);

            dcnMst.Allocate(now);

            dcnMst.DoReserveBatch(now);
        }

        public static void OnDayChanged(DateTime now)
        {
            var dcnMst = ReleasePlanMaster.DcnMst;

            dcnMst.OnDayChanged(now);
        }        

        #region DcnMaster Func

        public static void Initialize(this DcnMaster mst)
        {
            mst.SetSupportSteps();
            mst.SetSupportEqps();
        }

        private static void SetSupportSteps(this DcnMaster mst)
        {
            var gate = BopHelper.FindStdStep(Constants.ArrayShop, Constants.GATE_PHOTO_STEP);
            if (gate != null)
                mst.SupportSteps.Add(gate);

            var bm = BopHelper.FindStdStep(Constants.CfShop, Constants.BM__PHOTO_STEP);
            if (bm != null)
                mst.SupportSteps.Add(bm);
        }

        private static void SetSupportEqps(this DcnMaster mst)
        {
            var stepList = mst.SupportSteps;
            if (stepList == null || stepList.Count == 0)
                return;

            foreach (var it in stepList)
            {
                foreach (var targetEqp in it.AllEqps)
	            {
		            var eqp = AoFactory.Current.GetEquipment(targetEqp.EqpID) as FabAoEquipment;
                    if (eqp == null)
                        continue;

                    mst.SupportEqps.Add(eqp);
	            } 
            }
        }

        private static void Allocate_Init(this DcnMaster mst, DateTime now, DateTime startTime, DateTime endTime)
        {
            mst.StartTime = startTime;
            mst.EndTime = endTime;
            mst.Current = new List<DcnPlan>();

            foreach (var bck in mst.Buckets)
            {
                bck.Allocate_Init(now);

                //apply eqp state
                var eqpStates = mst.EqpStates;
                if (eqpStates != null && eqpStates.Count > 0)
                {
                    string eqpID = bck.EqpID;
                    EqpStatusInfo info;
                    if (eqpStates.TryGetValue(eqpID, out info))
                    {
                        bck.Allocate_State(info, startTime, endTime, now);

                        //remove
                        if (info.Duration <= 0)
                            eqpStates.Remove(eqpID);
                    }
                }
            }
        }

        private static void Allocate_FixPlan(this DcnMaster mst, DateTime now)
        {
            var plans = mst.FixPlans;
            if (plans == null || plans.Count == 0)
                return;

            DateTime startTime = mst.StartTime;
            DateTime endTime = mst.EndTime;

            foreach (var it in plans)
            {
                string eqpID = it.Key;                
                var bck = mst.GetBucket(eqpID);
                if (bck == null)
                    continue;

                var list = it.Value;                
                if (list == null || list.Count == 0)
                    continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var fixPlan = list[i];
                    var target = mst.SelectTarget_FixPlan(fixPlan);

                    if (bck.CanAllocate(startTime, endTime, target, false) == false)
                        break;

                    if (mst.Allocate_FixPlan(bck, target, fixPlan, now))
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }                                  
            }
        }

        private static bool Allocate_FixPlan(this DcnMaster mst, DcnBucket bck, DcnTarget target, FixPlanDCN fixPlan, DateTime now)
        {            
            int allocQty = fixPlan.PLAN_QTY;
            if (allocQty <= 0)
                return false;

            FabProduct prod = target == null ? null : target.Product;
            if (prod == null)
                prod = BopHelper.FindProduct(fixPlan.SHOP_ID, fixPlan.PRODUCT_ID);

            if (prod == null)
                return false;

            var plan = bck.Allocate(prod, allocQty, target, now);           

            //전체 Alloc Plan 기록
            if (plan != null)
            {
                plan.FixPlan = fixPlan;
                plan.AllocSeq = mst.AllPlans.Count + 1;

                mst.AllPlans.Add(plan);
                mst.Current.Add(plan);                
            }

            bool isNoAlloc = plan == null;
            if (isNoAlloc)
                return false;

            return true;
        }

        private static DcnTarget SelectTarget_FixPlan(this DcnMaster mst, FixPlanDCN fixPlan)
        {
            if (fixPlan == null)
                return null;

            var targetList = mst.AllTargets;
            if (targetList == null || targetList.Count == 0)
                return null;

            var find = targetList.Find(t => t.ShopID == fixPlan.SHOP_ID 
                                            && t.ProductID == fixPlan.PRODUCT_ID 
                                            && t.RemainQty > 0);

            return find;
        }

        private static void Allocate(this DcnMaster mst, DateTime now)
        {                                    
            var bckList = mst.Buckets.ToList();
            if (bckList == null || bckList.Count == 0)
                return;

            var targetList = mst.AllTargets;
            if (targetList == null || targetList.Count == 0)
                return;

            int lotSize = SeeplanConfiguration.Instance.LotUnitSize;

            while(true)
            {
                var bck = mst.SelectBucket(bckList);
                if (bck == null)
                    break;
                
                var target = mst.SelectTarget(bck, targetList);
                if (target == null)
                {
                    bckList.Remove(bck);
                    continue;
                }                    
                
                //선투입 수량 반영
                int remainQty = mst.FixedTargetQty(target);
                if (remainQty <= 0)
                    continue;
                 
                int diff;
                if (mst.Allocate(bck, target, lotSize, now, out diff))
                    mst.UpdateDiff(target, diff);
            }            
        }        

        private static bool Allocate(this DcnMaster mst, DcnBucket bck, DcnTarget target, int lotSize, DateTime now, out int diff)
        {
            diff = 0;

            int targetQty = target.RemainQty;

            if (lotSize <= 0)
                lotSize = targetQty;

            //lotSize 단위로 Alloc
            int remain = targetQty;

            int allocQty = lotSize;
            var plan = bck.Allocate(target.Product, allocQty, target, now);

            //전체 Alloc Plan 기록
            if (plan != null)
            {
                plan.AllocSeq = mst.AllPlans.Count + 1;

                mst.AllPlans.Add(plan);
                mst.Current.Add(plan);
            }

            remain = remain - allocQty;

            if (remain < 0)
                diff = Math.Abs(remain);

            bool isNoAlloc = remain == targetQty;
            if(isNoAlloc)
                return false;

            return true;
        }
                
        private static int FixedTargetQty(this DcnMaster mst, DcnTarget target)
        {
            if (target == null)
                return 0;

            string key = target.ProductID;

            int prevDiff;
            if(mst.Diffs.TryGetValue(key, out prevDiff))
            {
                int targetQty = target.RemainQty;

                int min = Math.Min(targetQty, prevDiff);
                int allocQty = min;

                target.AllocQty += allocQty;

                int remainDiff = prevDiff - allocQty;

                if (remainDiff <= 0)
                    mst.Diffs.Remove(key);
                else
                    mst.Diffs[key] = remainDiff;                
            }

            return target.RemainQty;
        }

        private static void UpdateDiff(this DcnMaster mst, DcnTarget target, int diffQty)
        {
            if (diffQty == 0)
                return;

            string key = target.ProductID;

            int prev;
            if (mst.Diffs.TryGetValue(key, out prev))
                mst.Diffs[key] = prev + diffQty;
            else
                mst.Diffs.Add(key, diffQty);
        }

        private static DcnBucket GetBucket(this DcnMaster mst, string eqpID)
        {
            if (string.IsNullOrEmpty(eqpID))
                return null;

            var list = mst.Buckets;
            if (list == null || list.Count == 0)
                return null;

            var find = list.Find(t => t.EqpID == eqpID);

            return find;
        }

        private static DcnBucket SelectBucket(this DcnMaster mst, List<DcnBucket> list)
        {
            if (list == null || list.Count == 0)
                return null;

            //sort bucket
            if (list.Count > 1)
                list.Sort(DcnBucketComparer.Default);

            return list[0];
        }

        private static DcnTarget SelectTarget(this DcnMaster mst, DcnBucket bck, List<DcnTarget> targetList)
        {
            if (bck == null)
                return null;

            var list = mst.GetLoadableTargetList(bck, targetList);
            if (list == null || list.Count == 0)
                return null;

            if (list.Count > 1)
                list.Sort(new DcnTargetComparer(bck, mst.SupportSteps, mst.SupportEqps));

            return list[0];
        }

        private static List<DcnTarget> GetLoadableTargetList(this DcnMaster mst, DcnBucket bck, List<DcnTarget> targetList)
        {
            if (targetList == null || targetList.Count == 0)
                return null;

            targetList.RemoveAll(t => t.RemainQty <= 0);
            if (targetList == null || targetList.Count == 0)
                return null;

            DateTime startTime = mst.StartTime;
            DateTime endTime = mst.EndTime;

            List<DcnTarget> list = new List<DcnTarget>();

            foreach (var target in targetList)
            {
                bool hasMainShopTarget = mst.HasRemainTarget(bck.MainRunShopID);

                if (bck.CanAllocate(startTime, endTime, target, hasMainShopTarget) == false)
                    continue;

                list.Add(target);
            }

            return list;
        }

        private static bool HasRemainTarget(this DcnMaster mst, string targetShopID)
        {
            if (string.IsNullOrEmpty(targetShopID))
                return false;

            var targetList = mst.AllTargets;
            if (targetList == null || targetList.Count == 0)
                return false;
            
            var find = targetList.Find(t => t.ShopID == targetShopID && t.RemainQty > 0);
            if (find != null)
                return true;

            return false;
        }

        private static void OnDayChanged(this DcnMaster mst, DateTime now)
        {
            foreach (var bck in mst.Buckets)
            {
                bck.OnDayChanged(now);
            }
        }

        private static void DoReserveBatch(this DcnMaster mst, DateTime now)
        {
            AoBatchRelease inputBatch = AoFactory.Current.BatchRelease;

            foreach (DcnPlan plan in mst.Current)
            {                
                FabProduct prod = plan.Product;
                if (prod == null)
                    continue;
                    
                FabStep step = prod.Process.FirstStep as FabStep;
                if (step == null)
                    continue;

                string lotID = EntityHelper.CreateFrontInLotID(prod);
                double unitQty = plan.AllocQty;

                FabWipInfo wip = CreateHelper.CreateWipInfo(lotID, prod, step, unitQty);
                FabLot lot = CreateHelper.CreateLot(wip, LotState.CREATE);

                DateTime releaseTime = LcdHelper.Max(plan.EqpEndTime, now);                                           

                lot.ReleaseTime = releaseTime;
                lot.LotState = LotState.CREATE;
                lot.ReleasePlan = plan;

                var inTarget = plan.Target == null ? null : plan.Target.InTarget;
                lot.FrontInTarget = inTarget;

                inputBatch.AddEntity(releaseTime, lot);
            }
        }

        #endregion DcnMaster Func

        #region DcnBucket Func

        private static DcnPlan Allocate(this DcnBucket bck, FabProduct prod, int allocQty, DcnTarget target, DateTime now)
        {
            if (prod == null)
                return null;

            if (allocQty <= 0)
                return null;
                            
            double tactTime = bck.TactTime;
            
            DcnPlan plan = new DcnPlan()
            {
                Product = prod,
                Target = target,
				AllocQty = allocQty,
				TactTime = tactTime,
				EqpStartTime = bck.LastEqpEndTime,                
                AllocTime = now,
				EqpID = bck.EqpID
            };

            plan.EqpEndTime = plan.EqpStartTime.AddSeconds(plan.EqpRunTime.TotalSeconds);

            if (target != null)
                target.AllocQty += allocQty;

            bck.Usage += allocQty * tactTime;
            bck.LastEqpEndTime = plan.EqpEndTime;

            bck.Plans.Add(plan);

            return plan;
        }

        private static bool CanAllocate(this DcnBucket bck, DateTime startTime, DateTime endTime, DcnTarget target, bool checkMainShopTarget)
        {
            //MainShopTarget이 남은 경우에만 체크
            if (checkMainShopTarget && bck.HasMainRunShop())
            {
                if (target != null && bck.MainRunShopID != target.ShopID)
                    return false;
            }

            if (bck.Remain <= 0)
                return false;

            if (bck.LastEqpEndTime >= endTime)
                return false;
            
            return true;
        }

        private static bool HasAllocPlan(this DcnBucket bck)
        {
            if (bck.Plans == null || bck.Plans.Count == 0)
                return false;

            return true;
        }

        private static bool HasMainRunShop(this DcnBucket bck)
        {
            return string.IsNullOrEmpty(bck.MainRunShopID) == false;
        }

        private static void ClearAllocPlan(this DcnBucket bck)
        {
            if (bck.Plans == null || bck.Plans.Count == 0)
                return;

            bck.Plans.Clear();
        }

        private static void Allocate_Init(this DcnBucket bck, DateTime now)
        {            
            bck.LastEqpEndTime = now;
        }

        private static void Allocate_State(this DcnBucket bck, EqpStatusInfo info, DateTime startTime, DateTime endTime, DateTime now)
        {
            if (info == null)
                return;

            if (info.Duration <= 0)
                return;
                                    
            DateTime st = LcdHelper.Max(info.StartTime, startTime);
            DateTime et = LcdHelper.Min(info.EndTime, endTime);

            if (st < et)
            {
                DcnPlan plan = new DcnPlan()
                {
                    Product = null,
                    Target = null,
                    AllocQty = 0,
                    TactTime = 0,
                    EqpStartTime = st,
                    EqpEndTime = et,
                    AllocTime = now,
                    EqpID = bck.EqpID,
                    EqpState = info.MesStatus.ToString()
                };
                                
                bck.LastEqpEndTime = et;
                bck.Plans.Add(plan);
            }

            if (info.EndTime > et)
            {
                info.StartTime = et;                
            }
            else
            {
                //clear
                info.StartTime = DateTime.MinValue;
                info.EndTime = DateTime.MinValue;
            }
        }

        private static void OnDayChanged(this DcnBucket bck, DateTime now)
        {
            bck.Usage = 0;
            bck.LastEqpEndTime = now;
        }

        #endregion DcnBucket Func

        #region DcnTarget Func

        private static bool IsEquals_Key(this DcnTarget target, string productID)
        {
            if (productID != target.ProductID)
                return false;

            return true;
        }

        #endregion DcnTarget Func

        #region Helper

        private static string GetShopStep(string shopID)
        {
            if (BopHelper.IsArrayShop(shopID))
                return ARRAY_STEP;

            if (BopHelper.IsCfShop(shopID))
                return CF_STEP;

            if (BopHelper.IsCellShop(shopID))
                return CELL_STEP;

            return Constants.NULL_ID;
        }

        //private static int GetAllocQty(int qty, int lotSize, out int diff)
        //{
        //    diff = 0;

        //    int d = qty / lotSize;
        //    int r = qty % lotSize;

        //    int allocQty = (r > 0 ? d + 1 : d) * lotSize;
        //    diff = allocQty - qty;

        //    return allocQty;
        //}

        #endregion Helper

        #region Comparer

        public class DcnTargetComparer : IComparer<DcnTarget>
        {
            private DcnBucket Bucket { get; set; }
            private List<FabStdStep> SupportSteps { get; set; }
            private List<FabAoEquipment> SupportEqps { get; set; }

            public DcnTargetComparer(DcnBucket bck, List<FabStdStep> supportSteps, List<FabAoEquipment> supportEqps)
            {
                this.Bucket = bck;
                this.SupportSteps = supportSteps;
                this.SupportEqps = supportEqps;
            }

            public int Compare(DcnTarget x, DcnTarget y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;

                //TargetDate
                if (cmp == 0)
                    cmp = x.TargetDate.CompareTo(y.TargetDate);

                //support 대상 제품
                if(cmp == 0)
                {
                    bool support_x = IsSupport(x);
                    bool support_y = IsSupport(y);

                    cmp = support_x.CompareTo(support_y) * -1;
                }

                //연속진행
                if(cmp == 0)
                {
                    var bck = this.Bucket;
                    bool last_x = IsLastPlan(bck, x);
                    bool last_y = IsLastPlan(bck, y);

                    cmp = last_x.CompareTo(last_y) * -1;
                }

                //수량 많은 순
                if (cmp == 0)
                    cmp = x.TargetQty.CompareTo(y.TargetQty) * -1;

                //low level
                if (cmp == 0)
                    cmp = string.Compare(x.ProductID, y.ProductID);

                return cmp;
            }

            private bool IsSupport(DcnTarget target)
            {
                if(this.SupportEqps == null || this.SupportEqps.Count == 0)
                    return false;

                var find = this.SupportEqps.Find(t => IsSupportRun(t, target));
                if (find != null)
                    return true;

                return false;
            }

            private bool IsSupportRun(FabAoEquipment eqp, DcnTarget target)
            {
                if (eqp == null || target == null)
                    return false;

                var targetEqp = eqp.Target as FabEqp;
                if (target.ShopID != target.ShopID)
                    return false;

                var last = eqp.GetLastPlan(); //eqp.LastPlan as FabPlanInfo;
                if (last == null)
                    return false;

                //SupportStep 진행 중 여부 체크
                var find = this.SupportSteps.Find(t => t.StepID == last.StepID);
                if (find == null)
                    return false;

                if (last.ProductID == target.ProductID)
                    return true;
                
                return false;
            }

            private bool IsLastPlan(DcnBucket bck, DcnTarget target)
            {
                if (bck == null)
                    return false;

                if (target.IsEquals_Key(bck.LastProductID))
                    return true;

                return false;
            }
        }

        public class DcnBucketComparer : IComparer<DcnBucket>
        {            
            public DcnBucketComparer()
            {
            }

            public int Compare(DcnBucket x, DcnBucket y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;

                //가장 빨리 투입 가능한 설비(잔여 capa 큰것)
                if (cmp == 0)
                {
                    cmp = x.LastEqpEndTime.CompareTo(y.LastEqpEndTime);
                }

                ////TactTime
                //if (cmp == 0)l
                //{
                //    var time_x = x.TactTime;
                //    var time_y = y.TactTime;

                //    cmp = time_x.CompareTo(time_y);
                //}

                //잔여 capa 큰것 우선
                if (cmp == 0)
                {
                    cmp = x.Remain.CompareTo(y.Remain) * -1;
                }

                //low level
                if (cmp == 0)
                    cmp = string.Compare(x.EqpID, y.EqpID);

                return cmp;
            }

            public static DcnBucketComparer Default = new DcnBucketComparer();
        }

        public class FixPlanComparer : IComparer<FixPlanDCN>
        {
            public FixPlanComparer()
            {
            }

            public int Compare(FixPlanDCN x, FixPlanDCN y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;
                                
                //state : processing > wait
                if (cmp == 0)
                {
                    var state_x = LcdHelper.ToEnum(x.PLAN_STATE, DcnPlanState.NONE);
                    var state_y = LcdHelper.ToEnum(y.PLAN_STATE, DcnPlanState.NONE);

                    cmp = state_x.CompareTo(state_y);
                }

                //PLAN_DATE
                if (cmp == 0)
                    cmp = x.PLAN_DATE.CompareTo(y.PLAN_DATE);

                //SEQ
                if (cmp == 0)
                {
                    var seq_x = x.PLAN_SEQ;
                    var seq_y = y.PLAN_SEQ;

                    cmp = seq_x.CompareTo(seq_y);
                }
                
                //low level
                if (cmp == 0)
                    cmp = string.Compare(x.PRODUCT_ID, y.PRODUCT_ID);

                return cmp;
            }

            public static FixPlanComparer Default = new FixPlanComparer();
        }

        #endregion Comparer

        #region Write Output

        public static void WriteRelasePlan()
        {           
            var dcnMst = ReleasePlanMaster.DcnMst;

            var bckList = dcnMst.Buckets;
            if (bckList == null || bckList.Count == 0)
                return;

            string versionNo = ModelContext.Current.VersionNo;

            foreach (var bck in bckList)
            {
                if (bck.HasAllocPlan() == false)
                    continue;

                string factoryID = bck.FactoryID;
                string eqpGroupID = bck.EqpGroupID;
                string eqpID = bck.EqpID;

                int count = bck.Plans.Count;
                for (int i = 0; i < count; i++)
                {
                    var plan = bck.Plans[i];

                    Outputs.ReleasePlan row = new ReleasePlan();

                    row.VERSION_NO = versionNo;
                    row.FACTORY_ID = factoryID;                    
                    row.EQP_GROUP_ID = eqpGroupID;
                    row.EQP_ID = eqpID;
                                        
                    var target = plan.Target;
                    if (target != null)
                    {                        
                        row.STEP_ID = target.StepID;
                        row.TARGET_DATE = target.TargetDate;
                        row.TARGET_QTY = target.TargetQty;
                    }

                    row.SHOP_ID = plan.ShopID;
                    row.PRODUCT_ID = plan.ProductID;

                    DateTime planDate = ShopCalendar.SplitDate(plan.EqpStartTime);
                    row.PLAN_DATE = LcdHelper.DbToString(planDate, false);

                    row.START_TIME = plan.EqpStartTime;
                    row.END_TIME = plan.EqpEndTime;
                    row.UNIT_QTY = plan.AllocQty;

                    if (target != null)
                    {
                        var inTarget = target.InTarget;
                        if (inTarget != null)
                        {
                            row.DEMAND_ID = inTarget.DemandID;
                            row.DEMAND_PLAN_DATE = inTarget.DueDate;
                        }
                    }

                    row.ALLOC_EQP_SEQ = i + 1;
                    row.ALLOC_SEQ = plan.AllocSeq;
                    row.ALLOC_TIME = LcdHelper.DbToString(plan.AllocTime);

                    var fixPlan = plan.FixPlan;
                    if(fixPlan != null)
                    {
                        row.PLAN_STATE = fixPlan.PLAN_STATE;
                    }
                    else
                    {
                        row.PLAN_STATE = plan.EqpState;
                    }
                    
                    OutputMart.Instance.ReleasePlan.Add(row);
                }

                //clear
                bck.ClearAllocPlan();
            }
        }     

        #endregion Write Output
    }
}
