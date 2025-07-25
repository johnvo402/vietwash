using System.Collections;
using System.Reflection;
using Application.Common.Interfaces.Services.Aws;
using Application.Common.Security;
using Mediator;
using Serilog;

namespace Application.Common.Behaviors;

public class ProcessImageKeyBehavior<TMessage, TResponse>(
    ILogger logger,
    IAmazonS3Service storageService
) : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : notnull
{
    private void ProcessDataPropertiesWithFileUrl(
        object data,
        HashSet<object>? processedObjects = null
    )
    {
        if (data == null)
            return;

        processedObjects ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!processedObjects.Add(data))
            return;

        IEnumerable<PropertyInfo> fileProps = GetFileAttributeProperties(data.GetType());

        foreach (PropertyInfo prop in fileProps)
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;
            object? value = prop.GetValue(data);
            if (value is not string str || string.IsNullOrWhiteSpace(str))
                continue;

            string? key = ExtractKeyFromUrl(str);
            if (key != null)
            {
                prop.SetValue(data, key);
                logger.Information("✅ Converted {FullUrl} => {Key}", str, key);
            }
        }
        var props = data.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0);
        foreach (PropertyInfo prop in props)
        {
            if (prop.PropertyType == typeof(string))
                continue;

            object? value = prop.GetValue(data);
            if (value == null)
                continue;

            if (value is IEnumerable enumerable && prop.PropertyType != typeof(string))
            {
                foreach (var item in enumerable)
                    ProcessDataPropertiesWithFileUrl(item, processedObjects);
            }
            else if (!prop.PropertyType.IsValueType)
            {
                ProcessDataPropertiesWithFileUrl(value, processedObjects);
            }
        }
    }

    private string? ExtractKeyFromUrl(string fullUrl)
    {
        try
        {
            var uri = new Uri(fullUrl);
            var publicUrl = storageService.GetPublicUrl();
            if (string.IsNullOrEmpty(publicUrl))
                return null;

            var publicUri = new Uri(publicUrl);
            var bucket = storageService.GetBucketName();
            if (string.IsNullOrEmpty(bucket))
                return null;

            var path = uri.AbsolutePath.TrimStart('/');

            if (path.StartsWith(bucket + "/", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(bucket.Length + 1);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<PropertyInfo> GetFileAttributeProperties(Type type) =>
        type.GetProperties().Where(p => p.GetCustomAttributes(typeof(FileAttribute), true).Any());

    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        ProcessDataPropertiesWithFileUrl(message);
        return ValueTask.CompletedTask;
    }
}
