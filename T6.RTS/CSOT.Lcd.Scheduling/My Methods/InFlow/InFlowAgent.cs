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
    [FeatureBind()]
    public static partial class InFlowAgent
    {
        //private static float _interval = 10.0f / 60.0f;   //10분 주기 실행
        //private static bool _isRunAgent = false;

        //public static InOutAgent InputAgent { get; set; }
        public static AoFactory Factory { get; set; }

        public static DateTime NowDT
        {
            get { return Factory.NowDT; }
        }

        //DoubleDictionary<EqpGroup, Dictionary<EqpID, EqpState>>
        private static DoubleDictionary<string, string, EqpState> _eqpDic
            = new DoubleDictionary<string, string, EqpState>();

        public static void InitConstruct(AoFactory factory)
        {
            Factory = factory;

            CreateFabManager();

            BuildEqpStates();
            BuildFabManager();
        }

        //주기적으로 실행
        public static void Management()
        {

            InFlowMaster.UpdateLoadedEqpList(Factory.Equipments);

            InFlowMaster.Management(NowDT);
        }

        private static void BuildEqpStates()
        {
            foreach (FabAoEquipment eqp in Factory.Equipments.Values)
            {                
                if (SimHelper.IsTftRunning)
                {
                    if (eqp.ShopID == Constants.CellShop)
                        continue;
                }
                else
                {
                    if (eqp.ShopID != Constants.CellShop)
                        continue;
                }

                string eqpID = eqp.EqpID;
                string dspEqpGroupID = eqp.DspEqpGroupID;

                Dictionary<string, EqpState> eqpDic;
                if (_eqpDic.TryGetValue(dspEqpGroupID, out eqpDic) == false)
                    _eqpDic[dspEqpGroupID] = eqpDic = new Dictionary<string, EqpState>();

                FabManager fabMgr = InFlowMaster.GetManager(dspEqpGroupID);

                if (fabMgr == null)
                    continue;

                eqpDic[eqpID] = new EqpState(fabMgr, eqp, InFlowMaster.GetInFlowSteps(eqpID));
            }
        }

        private static ICollection<string> GetDspEqpGroups()
        {
            List<string> result = new List<string>();

            foreach (var item in ResHelper.GetAllDspEqpGroup())
                result.Add(item);

            return result;
        }

        private static void CreateFabManager()
        {
            foreach (var item in GetDspEqpGroups())
            {
                InFlowMaster.AddFabMananger(item);
            }
        }

        private static void BuildFabManager()
        {
            foreach (string resGrpID in GetDspEqpGroups())
            {
                var mgr = InFlowMaster.GetManager(resGrpID);

                if (mgr == null)
                    continue;

                mgr.Build();
            }
        }

        public static FabManager GetFabManager(string dspEqpGroupID)
        {
            if (string.IsNullOrEmpty(dspEqpGroupID))
                return null;

            return InFlowMaster.GetManager(dspEqpGroupID);
        }

        //public static Dictionary<string, EqpState> GetEqpDics(string eqpGroup)
        //{
        //    if (eqpGroup == null)
        //        return new Dictionary<string, EqpState>();

        //    Dictionary<string, EqpState> eqpDic;
        //    if (_eqpDic.TryGetValue(eqpGroup, out eqpDic))
        //        return eqpDic;

        //    return new Dictionary<string, EqpState>();
        //}        

        public static decimal GetInflowQty(FabLot lot, AoEquipment aeqp, decimal inflowHour, int excludeStepCnt)
        {
            string productID = lot.CurrentProductID;
            string prodVer = lot.CurrentProductVersion;
            string owerType = lot.OwnerType;
            FabStep step = lot.CurrentFabStep;

            return GetInflowQty(productID, prodVer, owerType, step, aeqp, inflowHour, excludeStepCnt);
        }

        public static decimal GetInflowQty(JobFilterInfo info, AoEquipment aeqp, decimal inflowHour, int excludeStepCnt)
        {
            string productID = info.ProductID;
            string prodVer = info.ProductVersion;
            string owerType = info.OwnerType;
            FabStep step = info.Step;

            return GetInflowQty(productID, prodVer, owerType, step, aeqp, inflowHour, excludeStepCnt);
        }

        //CHECK : jung (버전별 프로파일 확인여부)
        public static decimal GetInflowQty(string productID, string productVer, string ownerType, FabStep step, AoEquipment aeqp, decimal inflowHour, int excludeStepCnt)
        {
            var job = InFlowMaster.GetJobState(productID, ownerType);

            if (job == null || step == null)
                return 0;

            WipProfile profile = job.GetWipProfile(step, productVer);

            if (profile == null)
            {
                profile = job.CreateWipProfile(step, productVer, excludeStepCnt, aeqp.Target.Preset as FabWeightPreset, aeqp, inflowHour);
                profile.CalcProfile();
            }

            return job.GetInflowWip(profile, inflowHour);
        }

    }
}
