namespace Application.Feature.Common.Projections;

public class MessageOutput
{
    public string? Message { get; set; }

    public MessageOutput(string? message)
    {
        Message = message;
    }

    public MessageOutput() { }
}
