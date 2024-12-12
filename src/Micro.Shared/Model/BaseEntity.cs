using System.ComponentModel.DataAnnotations;

namespace Micro.Shared.Model;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
