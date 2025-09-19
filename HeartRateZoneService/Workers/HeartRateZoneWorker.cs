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
    //private readonly ConsumerConfig _config;
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
        // ConsumerKafka(stoppingToken);
        _logger.LogInformation(" To CONSUM ");

        _producer.InitTransactions(DefaultTimeout);
        _consumer.Subscribe(BiometricsImportedTopicName);
        _logger.LogInformation("START CONSUMER");
        //_consumer.Subscribe(BiometricsImportedTopicName);

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
        //_consumer.Close();
        //_logger.LogInformation("FINISHING");
    }


    protected async Task ConsumerKafka(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig()
        {
            BootstrapServers = "pkc-56d1g.eastus.azure.confluent.cloud:9092",
            ClientId = "lkc-x3y75q",
            GroupId = "consumer-group",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SaslUsername = "IAPAMYFT57BMXIN3",
            SaslPassword = "cfltr1GTLhYPoqOQp9ZZ5Gn+N+kWvgtdh82p2t8WAN1vdMjaxVzyiSp4NaysdwSQ"
        };

        _consumer.Subscribe(BiometricsImportedTopicName);
        _logger.LogInformation("START CONSUMER");

        var consumer = new ConsumerBuilder<String, Biometrics>(consumerConfig)
         .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
        .Build();

        _logger.LogInformation("CONSUMING");

        _producer.InitTransactions(DefaultTimeout);
        consumer.Subscribe("BiometricsImported");

        while (!stoppingToken.IsCancellationRequested)
        {
            var biometrics1 = consumer.Consume(TimeSpan.FromSeconds(10));
            Biometrics biometrics = biometrics1.Message.Value;

            if (biometrics != null)
            {
                _logger.LogInformation("CONSUMING 1");
                // await HandleMessage(biometrics1.Message.Value, stoppingToken);

                var offsets = consumer.Assignment.Select(topicPartition =>
                 new TopicPartitionOffset(
                      topicPartition,
                      consumer.Position(topicPartition)
                      ));

                _producer.BeginTransaction();
                _producer.SendOffsetsToTransaction(offsets, consumer.ConsumerGroupMetadata, DefaultTimeout);


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
            else
            {
                _logger.LogInformation("There is not Message to consume ");
            }
        }

        consumer.Close();
        consumer.Close();
        _logger.LogInformation("FINISHING");

    }

    protected async Task Consumer(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig()
        {
            BootstrapServers = "pkc-56d1g.eastus.azure.confluent.cloud:9092",
            ClientId = "lkc-x3y75q",
            GroupId = "consumer-group",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SaslUsername = "IAPAMYFT57BMXIN3",
            SaslPassword = "cfltr1GTLhYPoqOQp9ZZ5Gn+N+kWvgtdh82p2t8WAN1vdMjaxVzyiSp4NaysdwSQ"
        };

        //_consumer.Subscribe(BiometricsImportedTopicName);
        _logger.LogInformation("START CONSUMER");

        var consumer = new ConsumerBuilder<String, Biometrics>(consumerConfig)
         .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
        .Build();

        _logger.LogInformation("CONSUMING");
        consumer.Subscribe("BiometricsImported");

        while (!stoppingToken.IsCancellationRequested)
        {
            var cr = consumer.Consume(TimeSpan.FromSeconds(10));

            if (cr != null)
            {
                _logger.LogInformation("CONSUMING 1");
                // await HandleMessage(cr.Message.Value, stoppingToken);
            }
            else
            {
                _logger.LogInformation("There is not Message to consume ");
            }
        }

        //_consumer.Close();
        _logger.LogInformation("FINISHING");

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
