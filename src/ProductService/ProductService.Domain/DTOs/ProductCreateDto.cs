using System.Text.Json.Serialization;

namespace ProductService.Domain.DTOs;

public class ProductCreateDto
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
