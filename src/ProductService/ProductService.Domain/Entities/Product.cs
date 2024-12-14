using System.ComponentModel.DataAnnotations;
using Micro.Shared.Common;

namespace ProductService.Domain.Entities;

public class Product : BaseAuditableEntity<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Product() { }
    public Product(string name, decimal price, int stockQuantity, string createdBy)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        CreatedBy = createdBy;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Quantity cannot be negative.");
        StockQuantity = quantity;
    }
}
