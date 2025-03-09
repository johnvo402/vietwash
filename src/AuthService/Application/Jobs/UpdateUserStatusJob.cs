using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Jobs
{
    public class UpdateUserStatusJob : IJob
    {
        public Task ExecuteAsync()
        {
            Console.WriteLine("Updating user status...");
            return Task.CompletedTask;
        }
    }
}
