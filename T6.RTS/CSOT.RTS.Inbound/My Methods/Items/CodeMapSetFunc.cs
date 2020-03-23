using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.RTS.Inbound.DataModel;
using CSOT.RTS.Inbound.Inputs;
using CSOT.RTS.Inbound.Outputs;
using CSOT.RTS.Inbound.Persists;
namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class CodeMapSetFunc
    {
        public static string CreateKey(string shopID, string productID, string productVersion)
        {
            return LcdHelper.CreateKey(shopID, productID, productVersion);
        }

        public static int Compare(CodeMapSet x, CodeMapSet y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = x.MapType.CompareTo(y.MapType);

            return cmp;
        }
    }
}
