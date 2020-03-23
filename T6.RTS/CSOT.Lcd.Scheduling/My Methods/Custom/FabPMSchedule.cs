using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.SeePlan.DataModel;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
    public class FabPMSchedule : PMSchedule
    {
        public ScheduleType Type;

		public float AllowAheadTime;
		public float AllowDelayTime;

		public DateTime InitStartTime;
		public DateTime LimitDelayTime;

		public DateTime AheadStartTime;

		public Time InputDuration;

		public bool IsNeedAdjust = true;
		public string Description;

        public FabPMSchedule(DateTime eventTime, int stime, ScheduleType sType, float allowAheadTime, float allowDelayTime) 
            :base(PMType.Full, eventTime, 1, stime, 0)
        {
            this.Type = sType;

			this.InputDuration = Time.FromSeconds(stime);

			this.AllowAheadTime = allowAheadTime;
			this.AllowDelayTime = allowDelayTime;

			this.InitStartTime = eventTime;
			this.LimitDelayTime = eventTime.AddMinutes(allowDelayTime);
			
        }
    }
}
