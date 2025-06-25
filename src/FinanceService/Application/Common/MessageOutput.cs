namespace Application.Common;

public class MessageOutput
{
    public string Message { get; set; } = string.Empty;

    public static MessageOutput Create(string message)
    {
        return new MessageOutput { Message = message };
    }

    public static MessageOutput Create()
    {
        return new MessageOutput();
    }
}
