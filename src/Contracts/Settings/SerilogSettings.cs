namespace Contracts.Settings;

public class SerilogSettings
{
    /// <summary>
    /// Put log to distributed server or not
    /// </summary>
    public bool IsDistributeLog { get; set; }

    public string? SeqUrl { get; set; }

    public string? ServiceName { get; set; }
}
