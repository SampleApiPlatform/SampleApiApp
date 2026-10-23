using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using NuGet.SampleSharedModels.DTO.ServiceBus;
using NuGet.SampleSharedModels.Enums.EnumsServiceBus;
using NuGet.SampleSharedModels.Interfaces.ServiceBus;




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
    public async Task SendAsync<T>(Guid messageId, MessageAction action, T? data)
    {
        //_logger.LogInformation("Sending message to Queue: {Queue}, Namespace: {Namespace}", 
        //                        _sender.EntityPath, _sender.FullyQualifiedNamespace);
        //                        _sender.EntityPath, _sender.FullyQualifiedNamespace);
        var payload = new BusPayloadDTO<T>
        {
            Action = action,
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
}



