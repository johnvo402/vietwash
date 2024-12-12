using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Entities;

public class Product
{
    [Key]
    public Guid Id { get; private set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public Product(string name, decimal price, int stockQuantity)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Quantity cannot be negative.");
        StockQuantity = quantity;
    }
}
