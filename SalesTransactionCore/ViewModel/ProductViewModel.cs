namespace SalesTransactionCore.ViewModel
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Rate { get; set; }
        public string? AvailableStock { get; set; }
    }
    public class ProductSelectItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Rate { get; set; }
    }
}
