using AuthService.Domain.Users.Entity;
using Micro.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Users.Events
{
    public record UserLoggedInEvent(string id) : IDomainEvent
    {
        public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
        public object? Data { get; set; } = new { Id = id };
    }
}
