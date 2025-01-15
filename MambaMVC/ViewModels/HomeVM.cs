using MambaMVC.Models;

namespace MambaMVC.ViewModels
{
    public class HomeVM
    {
        public ICollection<Product>? Products { get; set; }
    }
}
