using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;



public class BusSender: IBusSender
{
        // 1. Feld deklarieren
    private readonly ServiceBusSender _sender;
    private readonly ILogger<BusSender> _logger;

    public BusSender(ServiceBusSender sender, ILogger<BusSender> logger)
    {
        _sender = sender; // this is the singleton that we have defined in Program.cs
        _logger = logger;
    }
    public async Task SendAsync<T>(MessageAction action, T data, string? partitionKey = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending message to Queue: {Queue}, Namespace: {Namespace}", 
                                _sender.EntityPath, _sender.FullyQualifiedNamespace);
        var payload = new BusPayload<T>
        {
            Action = action,
            Version = 1,
            Data = data
        };

        var id = GetIdString(data);
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Payload must have a non-empty Id.", nameof(data));

        var msg = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payload))
        {
            Subject       = action.ToString(),          // real Label -> Consumer-Dispatch & SQL-Filter
            ContentType   = "application/json",         // real Header
            //MessageId     = GetId(data)?.ToString(),    // Duplicate Detection
            MessageId     = id,
            PartitionKey  = partitionKey,
            TimeToLive    = TimeSpan.FromDays(7)
        };

        await _sender.SendMessageAsync(msg, ct);
    }

    private static Guid? GetId<T>(T data) =>
        data is null ? null
        : (data.GetType().GetProperty("Id")?.GetValue(data) as Guid?);
    private static string? GetIdString<T>(T data) =>
        data is null ? null
            : (data.GetType().GetProperty("Id")?.GetValue(data)?.ToString());
}

//TODO: this code needs to move somewhere else
public interface IBusSender
{
    Task SendAsync<T>(MessageAction action, T data, string? partitionKey = null, CancellationToken ct = default);
}
public enum MessageAction {Add, Update, Delete, None};
public record BusPayload<T>
{
    public required MessageAction Action { get; init; }
    public int Version { get; init; } = 1;
    public T? Data { get; init; }
}