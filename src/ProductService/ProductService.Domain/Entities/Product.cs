using System.ComponentModel.DataAnnotations;
using Micro.Shared.Domain;

namespace ProductService.Domain.Entities;

public class Product : BaseAuditableEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Keywords { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Product() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Product(string name, decimal price, int stockQuantity, string keywords)
    {
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        Keywords = keywords;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Quantity cannot be negative.");
        StockQuantity = quantity;
    }
}
