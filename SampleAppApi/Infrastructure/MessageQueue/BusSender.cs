using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;



public class BusSender: IBusSender
{
        // 1. Feld deklarieren
    private readonly ServiceBusSender _sender;
    private readonly ILogger<BusSender> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() } // instead of 0 it will convert to Add, etc.
    };


    public BusSender(ServiceBusSender sender, ILogger<BusSender> logger)
    {
        _sender = sender; // this is the singleton that we have defined in Program.cs
        _logger = logger;
    }
    public async Task SendAsync<T>(Guid messageId, string recordId , MessageAction action, T data)
    {
        //_logger.LogInformation("Sending message to Queue: {Queue}, Namespace: {Namespace}", 
        //                        _sender.EntityPath, _sender.FullyQualifiedNamespace);
        var payload = new BusPayload<T>
        {
            Action = action,
            RecordId = recordId,
            Data = data
        };

        //var id = GetIdString(data);
        //    if (string.IsNullOrWhiteSpace(id))
        //        throw new ArgumentException("Payload must have a non-empty Id.", nameof(data));


        var msg = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payload,JsonOptions))
        {
            Subject       = action.ToString(),          // real Label -> Consumer-Dispatch & SQL-Filter
            ContentType   = "application/json",         // real Header
            //MessageId     = GetId(data)?.ToString(),    // Duplicate Detection
            MessageId     = messageId.ToString() , // We do not check dupliactes Duplicate Detection . TODO: but it is inactive in azure
            //PartitionKey  = partitionKey,
            TimeToLive    = TimeSpan.FromDays(7)
        };

        await _sender.SendMessageAsync(msg);
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
    Task SendAsync<T>(Guid messageId, string recordId , MessageAction action, T data);
}
public enum MessageAction {Add, Update, Delete, None};
public record BusPayload<T>
{
    public required MessageAction Action { get; init; }
    public string? RecordId; // id for the record to update or for a delete. For an add id=0
    public T? Data { get; init; } // for a delete this is null
}