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
namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class ConfigHelper
    {
        static bool _isRun;
        public static Dictionary<string, ConfigGroup> Data { get; private set; }

        public static void Init()
        {
            if (_isRun == false)
            {
                if (Data == null)
                    Data = new Dictionary<string, ConfigGroup>();

                _isRun = true;
            }
        }

        public static ConfigParameters GetConfigParameters(string key = null)
        {
            if(key == null)
                key = "DEFAULT";

            return InputMart.Instance.GetConfigParameters(key);
        }

        internal static ConfigGroup SafeGetConfigGorup(string name)
        {
            string key = name ?? "";

            ConfigGroup group;
            if (Data.TryGetValue(key, out group) == false)
            {
                group = new ConfigGroup();
                group.CodeGroup = key;
                group.Item = new Dictionary<string, ConfigInfo>();

                Data.Add(key, group);
            }

            return group;
        }

        internal static void AddItem(this ConfigGroup group, Config item)
        {
            string key = item.CODE_NAME;
            if (key == null)
                return;

            ConfigInfo info;
            if (group.Item.TryGetValue(key, out info) == false)
            {
                info = CreateHelper.CreateConfigInfo(item);
                group.Item.Add(key, info);
            }
        }

		internal static ConfigGroup GetConfigByGroup(string codeGroup)
		{
            if (codeGroup == null)
                return null;

			ConfigGroup group;
			Data.TryGetValue(codeGroup, out group);

			return group;
		}

        public static string GetCodeMap(string codeGroup, string codeName)
        {
            if (codeGroup == null || codeName == null)
                return codeName;

            ConfigGroup group = GetConfigByGroup(codeGroup);
            if (group == null)
                return codeName;

            var infos = group.Item;
            if (infos == null || infos.Count == 0)
                return codeName;

            ConfigInfo find;
            if(infos.TryGetValue(codeName, out find))
                return find.CodeValue;

            return codeName;
        }

        public static bool TryGetValue(string codeGroup, string codeName, out string codeValue)
        {
            codeValue = null;

            if (codeGroup == null || codeName == null)
                return false;

            ConfigGroup group = GetConfigByGroup(codeGroup);
            if (group == null)
                return false;

            var infos = group.Item;
            if (infos == null || infos.Count == 0)
                return false;

            ConfigInfo find;
            if (infos.TryGetValue(codeName, out find))
            {                
                codeValue = find.CodeValue;
                return true;
            }

            return false;
        }
    }
}
