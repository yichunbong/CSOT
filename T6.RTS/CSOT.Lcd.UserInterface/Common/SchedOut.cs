using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Common
{
    class SchedOut : PackedTable
    {
        public class LoadInfo
        {
            [PackedIndex(0)]
            public String State { get; set; }

            [PackedIndex(1)]
            public String StartTime { get; set; }

            public String EndTime { get; set; }

            [PackedIndex(2)]
            public String LotID { get; set; }

            [PackedIndex(3)]
            public string ProductID { get; set; }

            [PackedIndex(4)]
            public string ProcessID { get; set; }

            [PackedIndex(5)]
            public String StepID { get; set; }

            [PackedIndex(6)]
            public String Qty { get; set; }

            [PackedIndex(7)]
            public string ProductVersion { get; set; }

            [PackedIndex(8)]
            public string AreaID { get; set; }

            [PackedIndex(9)]
            public string ShopID { get; set; }

            [PackedIndex(10)]
            public string OwnerType { get; set; }

            [PackedIndex(11)]
            public string ToolID { get; set; }

            public bool IsSame(LoadInfo that)
            {
                return this.State == that.State 
                       && this.LotID == that.LotID 
                       && this.StepID == that.StepID;
            }

            public static string[] DetectNoise(string[] datas, DateTime targetDate)
            {                
                DateTime prevStartT = DateTime.MinValue;

                List<string> dataList = new List<string>();   
                foreach (string item in datas)
                {
                    var loadInfo = PackedTable.Split<SchedOut.LoadInfo>(item);
                    
                    DateTime startT = SchedOut.LHStateTime(targetDate, loadInfo.StartTime);

                    if (prevStartT.CompareTo(startT) <= 0)
                    {
                        dataList.Add(item);
                        prevStartT = startT;
                    }
                }

                return dataList.ToArray();
            }
        }                
    }
}
