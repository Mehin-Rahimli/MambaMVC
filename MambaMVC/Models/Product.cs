namespace MambaMVC.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public Category? Category { get; set; }
        public int CategoryId { get; set; }
    }
}
