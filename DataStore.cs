using System;
using System.Collections.Generic;

namespace NexPOS
{
    public static class DataStore
    {
        public static List<string> Categories = new List<string>()
        {
            "Beverages",
            "Canned Goods",
            "Snacks",
            "Dairy"
        };

        public static List<Product> Products = new List<Product>()
        {
            new Product { ProductID = 1, ProductCode = "BEV001", ProductName = "Coca-Cola 1.5L", Category = "Beverages", UnitPrice = 65, StockQuantity = 50, ExpirationDate = new DateTime(2026, 12, 31), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 2, ProductCode = "BEV002", ProductName = "Mineral Water 500ml", Category = "Beverages", UnitPrice = 15, StockQuantity = 100, ExpirationDate = new DateTime(2027, 6, 30), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 3, ProductCode = "CAN001", ProductName = "Sardines in Tomato Sauce", Category = "Canned Goods", UnitPrice = 28, StockQuantity = 8, ExpirationDate = new DateTime(2027, 1, 15), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 4, ProductCode = "CAN002", ProductName = "Corned Beef 150g", Category = "Canned Goods", UnitPrice = 55, StockQuantity = 30, ExpirationDate = new DateTime(2027, 3, 20), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 5, ProductCode = "SNK001", ProductName = "Potato Chips Original", Category = "Snacks", UnitPrice = 35, StockQuantity = 40, ExpirationDate = new DateTime(2026, 9, 10), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 6, ProductCode = "SNK002", ProductName = "Cream-O Cookies", Category = "Snacks", UnitPrice = 10, StockQuantity = 5, ExpirationDate = new DateTime(2026, 11, 30), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 7, ProductCode = "DAI001", ProductName = "Fresh Milk 1L", Category = "Dairy", UnitPrice = 85, StockQuantity = 20, ExpirationDate = new DateTime(2026, 5, 1), ReorderLevel = 10, Status = "Active" },
            new Product { ProductID = 8, ProductCode = "DAI002", ProductName = "Cheese Eden 165g", Category = "Dairy", UnitPrice = 72, StockQuantity = 25, ExpirationDate = new DateTime(2026, 8, 15), ReorderLevel = 10, Status = "Active" }
        };

        public static List<TransactionRecord> Transactions = new List<TransactionRecord>();
    }
}