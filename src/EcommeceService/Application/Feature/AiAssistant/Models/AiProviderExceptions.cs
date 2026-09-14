namespace Application.Feature.AiAssistant.Models;

public class AiProviderException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class AiProviderConfigurationException(string message)
    : AiProviderException(message);
