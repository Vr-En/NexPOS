using System;

namespace NexPOS
{
    public class InventoryProductRow
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int ReorderLevel { get; set; }
        public string Status { get; set; }

        public string PriceText
        {
            get { return "₱" + UnitPrice.ToString("0.00"); }
        }

        public string ExpirationText
        {
            get
            {
                if (ExpirationDate == null)
                {
                    return "";
                }

                return ExpirationDate.Value.ToString("M/d/yyyy");
            }
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