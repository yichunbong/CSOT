using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Data.Entity;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.UserInterface.Utils
{
    public static class EntityExtensions
    {
        public static EntityTable<T> GetEntityTable<T>(this IExperimentResultItem result, string key, string filter = "") where T : IEntityObject
        {
            IEnumerable<T> item;
            try
            {
                item = result.LoadOutput<T>(key, filter);
            }
            catch
            {
                item = result.LoadInput<T>(key, filter);
                return item.ToEntityTable();
            }

            return item.ToEntityTable();
        }

        public static EntityView<T> GetEntityView<T>(this EntityTable<T> table, string rowFilter = "", params string[] key)
        {
            string keyStr = "";
            foreach (var SVal in key)
            {
                if (StringUtility.IsEmptyID(keyStr)) keyStr = SVal;
                else keyStr += "," + SVal;
            }
            
            return new EntityView<T>(table, rowFilter, keyStr, Mozart.Data.Entity.IndexType.Hashtable);
        }

        public static EntityView<T> GetEntityView<T>(this IExperimentResultItem result, string tableName, string rowFilter = "", params string[] key) where T : IEntityObject
        {
            EntityTable<T> table = result.GetEntityTable<T>(tableName);
            return table.GetEntityView(rowFilter, key);
        }

    }
}
