namespace Application.Jobs
{
    public class CheckCustomerLoyal : IJob
    {
        public Task ExecuteAsync()
        {
            Console.WriteLine("Updating user status...");
            return Task.CompletedTask;
        }
    }
}
