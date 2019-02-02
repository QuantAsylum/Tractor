using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Com.QuantAsylum.Tractor.Ui.Extensions
{
    public static class Extensions
    {

        // From https://bitbucket.org/Superbest/superbest-random/src/f067e1dc014c31be62c5280ee16544381e04e303/Superbest%20random/RandomExtensions.cs?at=master&fileviewer=file-view-default
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        public static string ToEngineeringNotation(this double d, string formatString = "F4")
        {
            double exponent = Math.Log10(Math.Abs(d));
            if (Math.Abs(d) >= 1)
            {
                switch ((int)Math.Floor(exponent))
                {
                    case 0:
                    case 1:
                    case 2:
                        return d.ToString(formatString);
                    case 3:
                    case 4:
                    case 5:
                        return (d / 1e3).ToString(formatString) + "k";
                    case 6:
                    case 7:
                    case 8:
                        return (d / 1e6).ToString(formatString) + "M";
                    case 9:
                    case 10:
                    case 11:
                        return (d / 1e9).ToString(formatString) + "G";
                    case 12:
                    case 13:
                    case 14:
                        return (d / 1e12).ToString(formatString) + "T";
                    case 15:
                    case 16:
                    case 17:
                        return (d / 1e15).ToString(formatString) + "P";
                    case 18:
                    case 19:
                    case 20:
                        return (d / 1e18).ToString(formatString) + "E";
                    case 21:
                    case 22:
                    case 23:
                        return (d / 1e21).ToString(formatString) + "Z";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (Math.Abs(d) > 0)
            {
                switch ((int)Math.Floor(exponent))
                {
                    case -1:
                    case -2:
                    case -3:
                        return (d * 1e3).ToString(formatString) + "m";
                    case -4:
                    case -5:
                    case -6:
                        return (d * 1e6).ToString(formatString) + "μ";
                    case -7:
                    case -8:
                    case -9:
                        return (d * 1e9).ToString(formatString) + "n";
                    case -10:
                    case -11:
                    case -12:
                        return (d * 1e12).ToString(formatString) + "p";
                    case -13:
                    case -14:
                    case -15:
                        return (d * 1e15).ToString(formatString) + "f";
                    case -16:
                    case -17:
                    case -18:
                        return (d * 1e15).ToString(formatString) + "a";
                    case -19:
                    case -20:
                    case -21:
                        return (d * 1e15).ToString(formatString) + "z";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return "0";
            }
        }
    }
}
