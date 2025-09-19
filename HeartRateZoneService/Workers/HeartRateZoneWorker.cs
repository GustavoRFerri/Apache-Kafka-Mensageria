using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using HeartRateZoneService.Domain;
using Microsoft.Extensions.Options;

namespace HeartRateZoneService.Workers;

public class HeartRateZoneWorker : BackgroundService
{
    private readonly String BiometricsImportedTopicName = "BiometricsImported";
    private readonly String HeartRateZoneReachedTopicName = "HeartRateZoneReached";

    private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private readonly IConsumer<String, Biometrics> _consumer;
    private readonly ILogger<HeartRateZoneWorker> _logger;
    private readonly IProducer<String, HeartRateZoneReached> _producer;
    private readonly ProducerConfig _producerConfig;

    public HeartRateZoneWorker(IOptions<ProducerConfig> prConfig, IProducer<String, HeartRateZoneReached> producer, IConsumer<String, Biometrics> consumer, ILogger<HeartRateZoneWorker> logger)
    {
        _producerConfig = prConfig.Value;
        _producer = producer;
        _consumer = consumer;
        _logger = logger;
        logger.LogInformation("HeartRateZoneWorker is Active.");
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(" To CONSUM ");
        _producer.InitTransactions(DefaultTimeout);
        _consumer.Subscribe(BiometricsImportedTopicName);
        _logger.LogInformation("START CONSUMER");      

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(TimeSpan.FromSeconds(10));
            if (result is null)
            {
                _logger.LogInformation("Not to consume");
            }
            else
            {
                _logger.LogInformation("CONSUMING");
                await HandleMessage(result.Message.Value, stoppingToken);
            }               
        }
        _consumer.Close();
        _logger.LogInformation("FINISHED");
    }      

    protected virtual async Task HandleMessage(Biometrics biometrics, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Message Received: " + biometrics.DeviceId);

        var offsets = _consumer.Assignment.Select(topicPartition =>
               new TopicPartitionOffset(
                    topicPartition,
                    _consumer.Position(topicPartition)));

        _producer.BeginTransaction();
        _producer.SendOffsetsToTransaction(offsets, _consumer.ConsumerGroupMetadata, DefaultTimeout);

        try
        {
            await Task.WhenAll(biometrics.HeartRates
             .Where(hr => hr.GetHeartRateZone(biometrics.MaxHeartRate) != HeartRateZone.None)
             .Select(hr =>
             {
                 var zone = hr.GetHeartRateZone(biometrics.MaxHeartRate);

                 var heartRateZoneReached = new HeartRateZoneReached(
                     biometrics.DeviceId,
                     zone,
                     hr.DateTime,
                     hr.Value,
                     biometrics.MaxHeartRate
                 );

                 var message = new Message<String, HeartRateZoneReached>
                 {
                     Key = biometrics.DeviceId.ToString(),
                     Value = heartRateZoneReached
                 };

                 return _producer.ProduceAsync(HeartRateZoneReachedTopicName, message, stoppingToken);
             }));

            _producer.CommitTransaction();
        }
        catch (Exception ex)
        {
            _producer.AbortTransaction();
            throw new Exception("Transaction Failed", ex);
        }
    }
}
