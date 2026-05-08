namespace NexPOS
{
    public class CategoryComboItem
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public override string ToString()
        {
            return CategoryName;
        }
    }
}