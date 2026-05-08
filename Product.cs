using System;

namespace NexPOS
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int ReorderLevel { get; set; }
        public string Status { get; set; }

        public string ExpirationText
        {
            get { return ExpirationDate.ToString("M/d/yyyy"); }
        }

        public string StockLevel
        {
            get
            {
                if (StockQuantity == 0)
                {
                    return "Out";
                }

                if (StockQuantity <= ReorderLevel)
                {
                    return "Low";
                }

                return "High";
            }
        }

        public string StockDisplay
        {
            get
            {
                if (StockQuantity == 0)
                {
                    return "Out";
                }

                if (StockQuantity <= ReorderLevel)
                {
                    return "⚠ " + StockQuantity;
                }

                return StockQuantity.ToString();
            }
        }
    }

}