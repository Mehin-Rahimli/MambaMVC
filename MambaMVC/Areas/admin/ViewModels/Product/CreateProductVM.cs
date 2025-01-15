using MambaMVC.Models;

namespace MambaMVC.Areas.admin.ViewModels
{
    public class CreateProductVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public int CategoryId { get; set; }
        public ICollection<Category>Categories { get; set; }

    }
}
