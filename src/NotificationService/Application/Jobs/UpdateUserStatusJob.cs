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
