using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class MeterChange
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }
        public int TimePoint { get; set; }

        public MeterChange(int num, int denom, int time)
        {
            this.Numerator = num;
            this.Denominator = denom;
            this.TimePoint = time;
        }

        public override string ToString()
        {
            return "Metrum " + Numerator.ToString() + "/" + Denominator.ToString() + " at " + TimePoint.ToString();
        }
    }
}
