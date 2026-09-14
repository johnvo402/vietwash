namespace Application.Feature.AiAssistant.Interfaces;

public sealed class AiToolValidationException(string message) : Exception(message);

public sealed class AiToolForbiddenException(string message) : Exception(message);

public sealed class AiToolExecutionException(string message) : Exception(message);
