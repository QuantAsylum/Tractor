using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor
{
    static class Maths
    {
        static public void StdDev(List<double> doubles, out double average, out double stdDev)
        {
            double avg = 0;
            average = 0;
            stdDev = 0;
            int count = doubles.Count();
            if (count > 1)
            {
                avg = doubles.Average();

                double sum = doubles.Sum(d => (d - avg) * (d - avg));

                stdDev = Math.Sqrt(sum / (count - 1));
                average = avg;
            }
        }
    }
}
