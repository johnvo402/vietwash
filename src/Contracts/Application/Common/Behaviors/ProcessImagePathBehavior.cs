using System.Collections;
using System.Reflection;
using Application.Common.Interfaces.Services.Aws;
using Application.Common.Security;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Mediator;
using Serilog;

namespace Application.Common.Behaviors;

public class ProcessImagePathBehavior<TMessage, TResponse>(
    ILogger logger,
    IAmazonS3Service storageService
) : MessagePostProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : notnull
{
    protected override ValueTask Handle(
        TMessage message,
        TResponse response,
        CancellationToken cancellationToken
    )
    {
        Type responseType = typeof(TResponse);
        if (
            !responseType.IsGenericType
            || responseType.GetGenericTypeDefinition() != typeof(Result<>)
        )
        {
            return default!;
        }

        object? value = ResultTypeHelper.ExtractValue(response);
        if (value == null)
        {
            return default!;
        }

        // Check if the response is a PaginationResponse and handle accordingly
        Type resultType = responseType.GetGenericArguments()[0];
        if (
            resultType.IsGenericType
            && resultType.GetGenericTypeDefinition() == typeof(PaginationResponse<>)
        )
        {
            ProcessPaginationResponse(value);
            return default!;
        }

        if (typeof(IEnumerable).IsAssignableFrom(resultType) && resultType.IsGenericType)
        {
            ProcessEnumerableResponse(value);
            return default!;
        }

        // Handle non-pagination responses
        ProcessSingleResponse(value);
        return default!;
    }

    private void ProcessEnumerableResponse(object response)
    {
        if (response is IEnumerable dataEnumerable)
        {
            foreach (object data in dataEnumerable)
            {
                ProcessDataPropertiesWithFileAttribute(data);
            }
        }
    }

    // Processes responses of type PaginationResponse<>
    private void ProcessPaginationResponse(object response)
    {
        PropertyInfo? dataProperty = response
            .GetType()
            .GetProperty(nameof(PaginationResponse<object>.Data));
        if (dataProperty == null)
        {
            return;
        }

        object? dataPropertyValue = dataProperty.GetValue(response);
        if (dataPropertyValue is IEnumerable dataEnumerable)
        {
            foreach (object data in dataEnumerable)
            {
                ProcessDataPropertiesWithFileAttribute(data);
            }
        }
    }

    // Processes individual response properties with the [File] attribute
    private void ProcessSingleResponse(object response) =>
        ProcessDataPropertiesWithFileAttribute(response);

    // Processes the properties of a data object within a pagination response
    private void ProcessDataPropertiesWithFileAttribute(
        object data,
        HashSet<object> processedObjects = null
    )
    {
        if (data == null)
            return;

        // Khởi tạo HashSet để tránh vòng lặp vô hạn
        processedObjects ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!processedObjects.Add(data))
            return;

        // Xử lý các thuộc tính trực tiếp có attribute [File]
        IEnumerable<PropertyInfo> propertiesWithFileAttribute = GetFileAttributeProperties(
            data.GetType()
        );
        foreach (PropertyInfo prop in propertiesWithFileAttribute)
        {
            object? propValue = prop.GetValue(data);
            if (propValue == null)
                continue;

            // Nếu thuộc tính là chuỗi và có attribute [File], cập nhật đường dẫn
            if (prop.PropertyType == typeof(string))
            {
                UpdatePropertyIfNotPublicUrl(data, prop, propValue);
            }
        }

        // Xử lý các thuộc tính là IEnumerable (bao gồm ICollection, List, v.v.)
        IEnumerable<PropertyInfo> enumerableProperties = data.GetType()
            .GetProperties()
            .Where(prop =>
                typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)
                && prop.PropertyType != typeof(string)
            );

        foreach (PropertyInfo prop in enumerableProperties)
        {
            object? propValue = prop.GetValue(data);
            if (propValue is IEnumerable enumerable && propValue != null)
            {
                foreach (object item in enumerable)
                {
                    // Đệ quy vào từng phần tử của danh sách (bao gồm cả ICollection)
                    ProcessDataPropertiesWithFileAttribute(item, processedObjects);
                }
            }
        }

        // Xử lý các thuộc tính là đối tượng phức hợp (không phải IEnumerable, không phải string)
        IEnumerable<PropertyInfo> complexProperties = data.GetType()
            .GetProperties()
            .Where(prop =>
                !prop.PropertyType.IsValueType
                && prop.PropertyType != typeof(string)
                && !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)
            );

        foreach (PropertyInfo prop in complexProperties)
        {
            object? propValue = prop.GetValue(data);
            if (propValue != null)
            {
                // Đệ quy vào đối tượng phức hợp
                ProcessDataPropertiesWithFileAttribute(propValue, processedObjects);
            }
        }
    }

    // Updates the property value if the key does not already have http url
    private void UpdatePropertyIfNotPublicUrl(object target, PropertyInfo property, object key)
    {
        string imageKeyStr = key.ToString()!;
        if (!imageKeyStr.StartsWith(storageService.GetPublicUrl()!))
        {
            string? fullPath = storageService.GetFullpath(imageKeyStr);

            property.SetValue(target, fullPath);
        }
    }

    // Retrieves all properties with the [File] attribute from the given type
    private static IEnumerable<PropertyInfo> GetFileAttributeProperties(Type type) =>
        type.GetProperties()
            .Where(prop =>
                prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(FileAttribute))
            );
}
