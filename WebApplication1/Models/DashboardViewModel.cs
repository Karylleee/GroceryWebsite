namespace WebApplication1.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<CartItem> CartItems { get; set; }
    }
}
