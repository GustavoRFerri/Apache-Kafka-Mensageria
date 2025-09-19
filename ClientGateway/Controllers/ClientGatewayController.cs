using System.Diagnostics.Metrics;
using System.Net;
using ClientGateway.Domain;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ClientGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientGatewayController : ControllerBase
{
    //private string BiometricsImportedTopicName = "RawBiometricsImported";
    private string BiometricsImportedTopicName = "BiometricsImported";
    private IProducer<String,Biometrics> _producer;
    private readonly IConsumer<String, Biometrics> _consumer;

    private readonly ILogger<ClientGatewayController> _logger;

    public ClientGatewayController( IProducer<String, Biometrics> producer, ILogger<ClientGatewayController> logger)
    {
        
        _producer = producer;
        _logger = logger;
        logger.LogInformation("ClientGatewayController is Active.");
    }

    [HttpGet("Hello")]
    [ProducesResponseType(typeof(String), (int)HttpStatusCode.OK)]
    public String Hello()
    {
        _logger.LogInformation("Hello World");
        return "Hello World";
    }


    [HttpPost(Name = "Biometrics")]
    [ProducesResponseType(typeof(Biometrics), (int)HttpStatusCode.Accepted)]    
    public async Task<AcceptedResult> RecordMeasurements(Biometrics metrics)
    {
        try
        {
            _logger.LogInformation("Accepted biometrics");

            var message = new Message<String, Biometrics>
            {
                Key = metrics.DeviceId.ToString(),
                Value = metrics
            };

            var result = await _producer.ProduceAsync(BiometricsImportedTopicName, message);
            _producer.Flush();



            //CancellationToken stoppingToken = CancellationToken.None;
            //_consumer.Subscribe(BiometricsImportedTopicName);
            //_logger.LogInformation("START CONSUMER");
            //_consumer.Subscribe(BiometricsImportedTopicName);

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    var result1 = _consumer.Consume(TimeSpan.FromSeconds(30));                
            //}

            //_consumer.Close();
            //_logger.LogInformation("FINISHING");

            return Accepted(result.Value);
        }
        catch (Exception ex)
        {
            throw;
        }        
    }

}



