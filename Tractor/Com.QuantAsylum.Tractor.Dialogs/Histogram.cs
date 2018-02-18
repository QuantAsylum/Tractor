using Com.QuantAsylum.Tractor.Ui.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace Tractor.Com.QuantAsylum.Tractor.Dialogs
{
    class Histogram
    {
        List<double> DataList;
        double UnitsPerBin;
        int BinCount;

        public Histogram(List<double> data, double voltsPerBin, int binCount)
        {
            DataList = data;
            UnitsPerBin = voltsPerBin;
            BinCount = binCount;
        }

        public void Plot(string title, ZedGraphControl zg)
        {
            zg.GraphPane.CurveList.Clear();


            int[] hist = new int[BinCount];

            double mean = DataList.Average();
            double stdDev = DataList.StandardDeviation();
            zg.GraphPane.Title.Text = string.Format("{0}\nMean={1} SD={2}\nBins={3} Span={4}",
                                            title,
                                            mean.ToEngineeringNotation("0.00"),
                                            stdDev.ToEngineeringNotation("0.00"),
                                            BinCount.ToString(),
                                            (BinCount * UnitsPerBin).ToEngineeringNotation("0.00")
                                            );

            double binWidth = UnitsPerBin;


            int bucket;
            for (int i = 0; i < DataList.Count; i++)
            {

                bucket = (int)Math.Round((DataList[i] - mean) / binWidth);
                bucket = bucket + (int)Math.Ceiling(BinCount / 2f);

                if (bucket >= 0 && bucket < BinCount)
                    ++hist[bucket];

            }

            PointPairList ppl = new PointPairList();
            for (int i = 0; i < BinCount; i++)
            {
                ppl.Add((i - BinCount / 2) * binWidth + mean, hist[i]);
            }

            BarItem bars = new BarItem("", ppl, Color.LimeGreen);
            bars.Bar.Fill.Type = FillType.Solid;

            //LineItem line = new LineItem("", ppl, Color.LimeGreen, SymbolType.None);
            zg.GraphPane.CurveList.Add(bars);
            zg.GraphPane.XAxis.Scale.Min = mean - binWidth * (BinCount / 2);
            zg.GraphPane.XAxis.Scale.Max = mean + binWidth * (BinCount / 2);
            zg.GraphPane.YAxis.Scale.Max = hist.Max();
            zg.AxisChange();
            zg.Invalidate();

        }

    }
}
