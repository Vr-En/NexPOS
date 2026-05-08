using System;

namespace NexPOS
{
    public class TransactionRecord
    {
        public string ReceiptNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Cashier { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        public string DateTimeText
        {
            get { return TransactionDate.ToString("M/d/yyyy h:mm tt"); }
        }

        public string TotalText
        {
            get { return "₱" + TotalAmount.ToString("0.00"); }
        }

        public string ChangeText
        {
            get { return "₱" + ChangeAmount.ToString("0.00"); }
        }
    }
}