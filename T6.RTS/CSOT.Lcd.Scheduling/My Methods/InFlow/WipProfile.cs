using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Task.Execution;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public class WipProfile
    {
        private bool _isCalc = false;
        public string ProductID { get; private set; }
        public FabStep TargetStep { get; private set; }
        public int LoadedEqpCount { get; private set; }
        private List<WipStep> WipStepList { get; set; }

        public Pt2D LastPt2D;
        public Pt2D LastContPt;     // 연속WIP Pt (gap 이상 떨어진 경우)

        private FabWeightPreset _wpset;

        public decimal Tact { get; private set; }

        public decimal InRate
        {
            get { return (this.LastContPt == null) ? 0 : (this.LastContPt.X < 3600 ? this.LastContPt.Y : this.LastContPt.Slope * 3600); }
        }

        public decimal OutRate
        {
            get { return this.Tact > 0 ? 3600 / this.Tact : 0; }
        }

        // 소요 설비대수 = inRate/outRate
        public decimal ReqEqpCount
        {
            get { return this.OutRate > 0 ? this.InRate / this.OutRate : 0; }
        }

        // 현재 wip을 소요하는데 걸리는 시간
        public decimal ConsumingHour
        {
            get { return (this.LastContPt == null || this.OutRate == 0) ? 0 : this.LastContPt.Y / this.OutRate; }
        }

        // (할당필요 대수 - 할당대수)/(할당대수+1) >> In-Flow WIP 총량(or 소진시간)
        public double ReqEqpRate
        {
            get { return (double)(this.ReqEqpCount - this.LoadedEqpCount) / (double)(this.LoadedEqpCount + 1); }
        }

        public bool IsOverAssigned
        {
            get { return this.InRate < this.OutRate * this.LoadedEqpCount; }
        }

        public WipProfile(string productID, FabStep targetStep, int loadedEqpCount, FabWeightPreset wp)
        {
            this.WipStepList = new List<WipStep>();

            Reset(productID, targetStep, loadedEqpCount, wp);
        }

        public void Reset(string productID, FabStep targetStep, int loadedEqpCount, FabWeightPreset wp)
        {
            this.ProductID = productID;
            this.TargetStep = targetStep;
            this.LoadedEqpCount = loadedEqpCount;
            this._wpset = wp;

            this.LastPt2D = null;
            this.LastContPt = null;
            this.WipStepList.Clear();
        }

        public void AddWipStep(WipStep wipStep)
        {
            this.WipStepList.Add(wipStep);
        }

        public void CalcProfile()
        {
            if (_isCalc)
                return;

            Pt2D pt = new Pt2D(0, 0);
            if (WipStepList.Count < 1)
            {
                LastPt2D = pt;
                return;
            }

            // -- tact time 설정
            WipStep wipStep = this.WipStepList.Count > 0 ? this.WipStepList[0] : null;
            if (wipStep != null)
                this.Tact = wipStep.Tact;

            // -- 누적 graph 전개
            List<Pt2D> pts = new List<Pt2D>();
            pts.Add(pt);
            
            foreach (WipStep ws in this.WipStepList)
            {
                ws.MovePts(pt);
                pts.AddRange(ws.Pts);
                pt = ws.LastPt2D;
            }

            LastPt2D = pt;

            if (_wpset == null)
                return;

            LastContPt = LastPt2D;

            _isCalc = true;
        }

        public decimal RundownHour(decimal qty)
        {
            foreach (WipStep ws in this.WipStepList)
            {
                if (ws.LastPt2D.Y < qty)
                    continue;

                return ws.RundownHour(qty);
            }
            return LastPt2D.X / 3600;
        }

        public decimal ArriveWithin(decimal hr)
        {
            decimal sec = (decimal)hr * 3600;

            WipStep prev = null;

            foreach (WipStep ws in this.WipStepList)
            {
                WipStep select = ws;

                if (ws.LastPt2D.X < sec)
                {
                    prev = ws;
                    continue;
                }

                if (ws.FirstPt2D.X > sec)
                    select = prev;

                if (select == null)
                    return 0;

                return select.ArriveWithin(hr);
            }

            return LastPt2D.Y;
        }

        public decimal ArriveWithin(FabStep step, WipType type)
        {
            decimal arriveInQty = 0;
            foreach (WipStep ws in this.WipStepList)
            {
                if (ws.TargetStep != step)
                {
                    arriveInQty += ws.LastPt2D.Y;
                    continue;
                }

                return arriveInQty + ws.ArriveWithin(type);
            }

            return LastPt2D.Y;
        }


        public bool ConsumeWithin(decimal hr, int additionalEqps)
        {
            // -- hr 시간동안 유입량과 유출량
            int loadedEqpCount = LoadedEqpCount + additionalEqps;
            decimal inQty = LastContPt != null ? LastContPt.Y : 0; // ArriveWithin(hr);
            decimal outQty = OutRate * loadedEqpCount * hr;

            return inQty <= outQty;
        }

        public bool ConsumeWithin(WipProfile otherFlow, decimal hr, int additionalEqps)
        {
            // -- hr 시간동안 유입량과 유출량
            decimal inQty = LastContPt != null ? LastContPt.Y : 0;
            decimal inQty2 = otherFlow.LastContPt != null ? otherFlow.LastContPt.Y : 0;

            decimal loadedEqpCount = otherFlow.LoadedEqpCount + LoadedEqpCount + additionalEqps;
            decimal outQty = OutRate * loadedEqpCount * hr;

            return inQty + inQty2 <= outQty;
        }

        public decimal ReqOutTime(int additionalEqps)
        {
            decimal loadedEqpCount = LoadedEqpCount + additionalEqps;
            if (loadedEqpCount == 0) return decimal.MaxValue;

            return this.ConsumingHour / loadedEqpCount;
        }

        public Dictionary<FabStep, WipType> GetStepPositions(Time time)
        {
            return GetStepPositions(ArriveWithin((decimal)time.TotalHours));
        }

        public Dictionary<FabStep, WipType> GetStepPositions(decimal qty)
        {
            Dictionary<FabStep, WipType> stepPositions = new Dictionary<FabStep, WipType>();
            foreach (WipStep ws in this.WipStepList)
            {
                WipType wipType = WipType.Total;
                if (TargetStep == ws.TargetStep)
                    wipType = WipType.Wait;

                if (ws.LastPt2D.Y >= qty)
                {
                    wipType = ws.GetWipPosition(qty);
                    stepPositions.Add(ws.TargetStep, wipType);
                    return stepPositions;
                }

                stepPositions.Add(ws.TargetStep, wipType);
            }

            return stepPositions;
        }

        public decimal GetContinuityWips(decimal flowHours)
        {
            if (this.LastContPt == null)
                return 0;

            //int continuityWipQty = Convert.ToInt32(this.LastContPt.Y);
            decimal inFlowQty = this.ArriveWithin(flowHours);

            return inFlowQty;//Math.Min(inFlowQty, continuityWipQty);
        }

        public override string ToString()
        {
            string msg = string.Format("{0}\t{1}\n", ProductID, TargetStep);
            foreach (WipStep ws in this.WipStepList)
                msg += ws.ToString();

            return msg;
        }
    }

    public class WipStep
    {
        public List<Pt2D> Pts { get; set; }

        public FabStep TargetStep { get; private set; }
        public decimal Tact { get; private set; }

        private decimal _tatRun;
        private decimal _tatWait;
        private decimal _qtyRun;
        private decimal _qtyWait;
        private decimal _stayWait;        

        public Pt2D LastPt2D
        {
            get 
            {
                var pts = this.Pts;
                return pts.Count < 1 ? null : pts[pts.Count - 1]; 
            }
        }

        public Pt2D FirstPt2D
        {
            get 
            {
                var pts = this.Pts;
                return pts.Count < 1 ? null : pts[0]; 
            }
        }              

        public decimal RemainTatRun 
        {
            get { return _tatRun; } 
        }

        public decimal RemianTatWait 
        { 
            get { return Math.Max(_tatWait - _stayWait, 0); } 
        }

        public WipStep(FabStep step, decimal tact, decimal tatRun, decimal tatWait, decimal qtyRun, decimal qtyWait, decimal stayWait)
        {
            this.TargetStep = step;
            this.Tact = tact > 0 ? tact : 1;

            _tatRun = tatRun;
            _tatWait = tatWait;
            _qtyRun = qtyRun;
            _qtyWait = qtyWait;
            _stayWait = stayWait;

            if (step.IsMandatoryStep)
                CalcStepOutProfile();
            else
                CalcStepOutProfile_Tat();
        }

        public WipStep(FabStep step, decimal tact, decimal qtyWait)
        {
            this.TargetStep = step;
            this.Tact = tact > 0 ? tact : 1;

            _qtyWait = qtyWait;

            this.Pts = new List<Pt2D>();
            this.Pts.Add(new Pt2D(0, _qtyWait));
        }

        public WipStep(FabStep step)
        {
            this.TargetStep = step;

            this.Pts = new List<Pt2D>();
            this.Pts.Add(new Pt2D(0, 0));
        }

        public WipStep()
        {
            this.Pts = new List<Pt2D>();
            this.Pts.Add(new Pt2D(0, 0));
        }

        //TODO : jung DCN투입물량 Inflow 작성필요
        public void AddReleaePlan(JobState jobState, FabStep firstStep, AoEquipment equipment)
        {
            //Current 현재 3시간치 분량
            var plans = ReleasePlanMaster.DcnMst.Current;

            foreach (var plan in plans)
            {
                if (plan.IsRelease)
                    continue;
            }
        }

        public void AddReleaePlanInputAgent(JobState jobState, FabStep firstStep, AoEquipment equipment)
        {
        }

        private void AddRelasePlan_Gap(Time now, Time startTime, Time endTime, decimal qty)
        {
            decimal x = (decimal)(startTime - now).TotalSeconds;
            this.Pts.Add(new Pt2D(x, qty));
        }

        private void CalcStepOutProfile()
        {
            var pts = this.Pts = new List<Pt2D>();
            decimal now = 0;            

            Pt2D.SetPt2D(pts, now, 0);

            decimal cumQty = 0;

            //Run Wip (Tact / Tat 중 짧은 기준)
            decimal qtyRun = _qtyRun;
            decimal tactRun = qtyRun * this.Tact;
            decimal minTatRun = Math.Min(tactRun, this.RemainTatRun);
            decimal maxTatRun = Math.Max(tactRun, this.RemainTatRun);

            if (qtyRun > 0)
            {
                now = minTatRun;
                cumQty += qtyRun;
                Pt2D.SetPt2D(pts, now, cumQty);
            }

            //maxTatRun 기준 기록
            if (maxTatRun > minTatRun)
            {
                now = maxTatRun;
                Pt2D.SetPt2D(pts, now, cumQty);                
            }

            //Wait Wip (Tat 기준 최소 1CST 도착, 나머지는 수량은 Tact / Tat 중 긴 기준)            
            decimal qtyWait = _qtyWait;
            decimal remainQty = qtyWait;
            decimal tactWait = qtyWait * this.Tact;
                        
            //Tat 기준 최소 1CST
            if (tactWait > this.RemianTatWait)
            {
                decimal lotSize = SeeplanConfiguration.Instance.LotUnitSize;
                decimal minQty = Math.Min(remainQty, lotSize);

                now = maxTatRun + this.RemianTatWait;
                cumQty += minQty;

                Pt2D.SetPt2D(pts, now, cumQty);
                
                remainQty = remainQty - minQty;
            }

            //나머지 수량, Tact / Tat 중 긴 기준
            decimal maxTatWait = Math.Max(tactWait, this.RemianTatWait);
            now = maxTatRun + maxTatWait;
            cumQty += remainQty;

            Pt2D.SetPt2D(pts, now, cumQty);

            //다음 Step의 Start Point 시간을 위해 기록 필요 (누적 그래프)
            if(maxTatWait < _tatWait)
            {
                now = maxTatRun + _tatWait;
                Pt2D.SetPt2D(pts, now, cumQty);
            }            
        }

        private void CalcStepOutProfile_Tat()
        {
            var pts = this.Pts = new List<Pt2D>();
            decimal now = 0;            

            Pt2D.SetPt2D(pts, now, 0);

            now = _tatRun;
            Pt2D.SetPt2D(pts, now, _qtyRun);

            now += _tatWait;
            Pt2D.SetPt2D(pts, now, _qtyRun + _qtyWait);
        }

        public void MovePts(Pt2D sp)
        {
            var pts = this.Pts;
            foreach (Pt2D pt in pts)
            {
                pt.MovePt(sp);
            }
        }

        public decimal RundownHour(decimal qty)
        {
            var pts = this.Pts;
            decimal sec = LastPt2D.X;

            int i, n = pts.Count;
            if (n < 2) return sec / 3600;

            for (i = 1; i < n; i++)
            {
                if (pts[i].Y < qty)
                    continue;

                decimal dx = pts[i].X - pts[i - 1].X;
                decimal dy = pts[i].Y - pts[i - 1].Y;
                if (dy == 0)
                    return pts[i - 1].X / 3600;

                decimal sy = qty - pts[i - 1].Y;
                return (pts[i - 1].X + sy * dx / dy) / 3600;
            }

            return sec / 3600;
        }

        public decimal ArriveWithin(decimal hr)
        {
            var pts = this.Pts;

            decimal sec = hr * 3600;
            decimal qty = LastPt2D.Y;

            int i, n = pts.Count;
            if (n == 1) return pts[0].Y;

            for (i = 0; i < n; i++)
            {
                if (pts[i].X < sec)
                    continue;

                return pts[i - 1].Y;
            }

            return qty;
        }

        public decimal ArriveWithin(WipType type)
        {
            if (type == WipType.Run)
                return _qtyRun;

            if (type == WipType.Wait)
                return _qtyWait;

            return _qtyRun + _qtyWait;
        }

        //public string GetWipPosition(double qty, ref bool isRun)
        //{
        //    if (LastPt2D.Y - _qtyRun < qty)
        //        isRun = true;
        //    else
        //        isRun = false;

        //    return _stdStepSeq;
        //}

        public WipType GetWipPosition(decimal qty)
        {
            if (LastPt2D.Y - _qtyWait < qty)
                return WipType.Wait;

            return WipType.Total;
        }

        public override string ToString()
        {
            var pts = this.Pts;

            string msg = string.Format("{0} ", this.TargetStep.ToString());
            foreach (Pt2D pt in pts)
            {
                msg += string.Format("{0:F2},{1:F2}  ", pt.X, pt.Y);
            }
            return msg;
        }
    }

    public class Pt2D : IComparable<Pt2D>
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Slope { get { return X > 0 ? Y / X : decimal.MaxValue; } }
        public Time Now { get { return Time.FromSeconds((double)this.X); } }
            
        public Pt2D(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public void MovePt(Pt2D pt)
        {
            this.X += pt.X;
            this.Y += pt.Y;
        }

        //public bool IsSamePt(Pt2D pt)
        //{
        //    if (pt.X != X) return false;
        //    if (pt.Y != Y) return false;

        //    return true;
        //}

        public static void SetPt2D(List<Pt2D> outs, decimal x, decimal y)
        {
            if (outs == null)
                return;

            int count = outs.Count;
            var last = count == 0 ? null : outs[count - 1];

            bool isLast = last != null && last.X == x;
            if (isLast)
            {
                last.Y = y;
                return;
            }

            outs.Add(new Pt2D(x, y));
        }

        public override string ToString()
        {
            return string.Format("({0:F2},{1:F2})", X, Y);
        }

        public int CompareTo(Pt2D other)
        {
            int cmp = this.X.CompareTo(other.X);

            if (cmp == 0)
                cmp = this.Y.CompareTo(other.Y);

            return cmp;
        }
    }
}
