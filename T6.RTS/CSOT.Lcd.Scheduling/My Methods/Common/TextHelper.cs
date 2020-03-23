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
    public static partial class TextHelper
    {
        // Ncf like pattern
        public static bool LikeNcf(string text, string pattern, out bool isEqual)
        {
            isEqual = false;

            if (string.IsNullOrEmpty(pattern))
                return false;

            bool isNotLikeFlag = pattern[0] == '~';
            if (isNotLikeFlag)
            {
                pattern = pattern.Substring(1);

                if (text == pattern)
                {
                    isEqual = false;
                    return false;
                }

                return !Mozart.Text.LikeUtility.Like(text, pattern);
            }
            else
            {
                if (text == pattern)
                {
                    isEqual = true;
                    return true;
                }

                if (pattern == Constants.PERCENT)
                    return true;

                return Mozart.Text.LikeUtility.Like(text, pattern);
            }
        }
    }
}
