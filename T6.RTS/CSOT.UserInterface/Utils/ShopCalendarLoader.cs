using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.Projects;
using System.Data;

namespace CSOT.UserInterface.Utils
{
    public class ShopCalendarLoader
    {
        public static void InitFactoryTime(IExperimentResultItem result)
        {
            FactoryConfiguration.Check(result.Model, InitializeFacotryTime);
        }

        public static void InitFactoryTime(IModelProject project)
        {
            FactoryConfiguration.Check(project, InitializeFacotryTime);
        }

        private static FactoryTimeInfo InitializeFacotryTime(IModelProject project)
        {
            FactoryTimeInfo info = new FactoryTimeInfo();

            info.Default = true;
            info.Name = project.Name;
            info.StartOffset = TimeSpan.FromHours(-2);
            info.StartOfWeek = DayOfWeek.Monday;

            info.ShiftNames = new string[] { "GY", "DY", "SW" };
            int shiftCount = info.ShiftNames.Length;
            info.ShiftHours = 24 / shiftCount;

            return info;
        }
    }
}
