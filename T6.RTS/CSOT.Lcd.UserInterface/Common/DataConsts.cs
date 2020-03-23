using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CSOT.Lcd.UserInterface.Common
{
    class DataConsts
    {
        public static readonly double Slope = 20.0F;

        #region Inputs

        // 0) BOP
        public static readonly string In_Process = "Process";
        public static readonly string In_StdStep = "StdStep";
        public static readonly string In_Product = "Product";
        public static readonly string In_StepRoute = "StepRoute";


        // 1) PEG
        public static readonly string In_Plan = "Plan";
        public static readonly string In_StepTat = "StepTat";
        public static readonly string In_Wip = "Wip";

        // 2) Simulation
        public static readonly string In_EqpArrange = "EqpArrange";
        public static readonly string In_EqpStepTime = "EqpStepTime";
        public static readonly string In_Equipment = "Eqp";
        public static readonly string In_Tool = "Tool";
        public static readonly string In_ToolArr = "ToolArrange";
        public static readonly string In_PresetInfo = "WeightPreset";


        #endregion

        #region OutPuts

        // 0) General
        public static readonly string Out_ErrHist = "ErroHistory";


        // 1) Pegging
        public static readonly string Out_PegHist = "PegHistory";
        public static readonly string Out_StepTarget = "StepTarget";
        public static readonly string Out_UnPegHist = "UnPegHistory";

        // 2) Simulation
        public static readonly string Out_DispatchInfo = "EqpDispatchInfo";
        public static readonly string Out_LoadingHistory = "LoadingHistory";
        public static readonly string Out_LoadStat = "LoadStat";
        public static readonly string Out_StepMove = "StepMove";
        public static readonly string Out_StepWip = "StepWip";
        public static readonly string Out_EqpPlan = "EqpPlan";
        public static readonly string Out_LotHistory = "LotHistory";
        public static readonly string Out_LotInfo = "LotInfo";

        public static readonly string Out_StepPlan = "StepPlan";



        #endregion

        #region SaveFieldName

        static Dictionary<string, string> _mappingFields;
        static readonly string _mappingFieldStr = "PROCESS_GROUP:PRC_GROUP;STEP_TAT_TIME:STEP_TAT";

        private static void InitFiledMapping()
        {
            if (_mappingFields != null)
                return;
            else
                _mappingFields = new Dictionary<string, string>();

            string mappings = _mappingFieldStr;

            if (mappings.Length <= 0)
                return;

            string[] mappingPairs = mappings.Split(';');

            foreach (string map in mappingPairs)
            {
                string[] item = map.Split(':');

                if (item.Length != 2)
                    continue;

                _mappingFields.Add(item[0], item[1]);
                _mappingFields.Add(item[1], item[0]);
            }
        }

        public static string SafeFieldName(DataTable dt, string input)
        {
            //초기화
            InitFiledMapping();

            string output = input;

            if (dt.Columns[input] == null)
                output = GetMappingFieldName(input);

            return output;
        }

        private static string GetMappingFieldName(string input)
        {
            if (_mappingFields.ContainsKey(input) == true)
                return _mappingFields[input];

            return input;
        }

        #endregion SaveFieldName
        
    }
}
