using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor
{
    static class Maths
    {
        static public void StdDev(List<double> data, out double avg, out double stdDev)
        {
            if (data.Count == 0)
                throw new InvalidOperationException("There must be at least 1 data point in the array presented to StdDev");

            if (data.Count == 1)
            {
                avg = data[0];
                stdDev = 0;
                return;
            }

            // Can't use ref in anonymous method. Need to create mirror.
            double average = data.Average();
            double sum = data.Sum(d => (d - average) * (d - average));

            stdDev = Math.Sqrt(sum / (data.Count - 1));
            avg = average; 
        }
    }
}
