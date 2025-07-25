namespace Application.Jobs
{
    public interface IJob
    {
        Task ExecuteAsync();
    }
}
