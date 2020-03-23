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
    public class CustomDownloadMonitor : DownloadMonitor
    {
        IModelLog _log;
        ISet<string> criticals;
        public bool HasCritical { get; private set; }

        public IModelLog Log
        {
            get { return _log; }
        }

        public CustomDownloadMonitor(IModelLog log, ICollection<string> criticals = null)
            : base(log, criticals)
        {
            _log = log;
            if (criticals != null)
                this.criticals = new HashSet<string>(criticals);
        }
        /// <summary>
        /// Exception이 발생 할 때마다 호출이 됩니다. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ex"></param>
        protected override void OnException(string name, Exception ex)
        {
            if (this.criticals != null)
                this.HasCritical = this.criticals.Contains(name);

            if (_log != null)
            {
                //persist-task에 로그를 남기기 위한 용도 
                _log.MonitorInfo("Download '" + name + "' table error.\r\n" + ex.ToString());

                Outputs.DBError info = new DBError();
                info.TABLE = name;
                info.ERROR_LOG = string.Format("Download '{0}' talble error.\r\n {1}", name, ex.ToString());

                OutputMart.Instance.DBError.Add(info);
            }

        }
    }
}
