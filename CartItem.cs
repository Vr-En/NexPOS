namespace NexPOS
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal
        {
            get
            {
                return Product.UnitPrice * Quantity;
            }
        }
    }
}