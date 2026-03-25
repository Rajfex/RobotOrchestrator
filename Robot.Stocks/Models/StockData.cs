using System;
using System.Collections.Generic;
using System.Text;

namespace Robot.Stocks.Models
{
    public class StockData
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string ReferencePrice { get; set; }

        public string ReferencePriceDate { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string ChangePercent { get; set; }

        public string Change { get; set; }

        public string Min { get; set; }

        public string MinDate { get; set; }

        public string Max { get; set; }

        public string MaxDate { get; set; }

        public string AveragePrice { get; set; }

        public string Volume { get; set; }

        public string AverageVolume { get; set; }

        public string Turnover { get; set; }

        public string AverageTurnover { get; set; }
    }
}
