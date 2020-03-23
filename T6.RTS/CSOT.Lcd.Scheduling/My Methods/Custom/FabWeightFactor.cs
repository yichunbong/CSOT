using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSOT.Lcd.Scheduling
{
    public class FabWeightFactor : WeightFactor
    {       
        public string OrigCriteria { get; private set; }
        public bool IsAllowFilter { get; private set; }

        public FabWeightFactor(string name, float weightFactor, float sequence, FactorType type, OrderType orderType, string criteria, bool isAllowFilter)
            : base(name, weightFactor, sequence, type, orderType)
        {
            this.OrigCriteria = criteria;
            this.IsAllowFilter = IsAllowFilter;            
        }
    }
}
