using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class Metrum : TimeSpanEvent
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }

        public Metrum(int num, int denom, int time)
        {
            this.Numerator = num;
            this.Denominator = denom;
            this.startTick = time;
        }

        public override string ToString()
        {
            return "Metrum " + Numerator.ToString() + "/" + Denominator.ToString() +
                " from " + startTick.ToString() + " to " + endTick.ToString();
        }
    }
}
