using ClientGateway;
using ClientGateway.Controllers;
using ClientGateway.Domain;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* TODO: Load a config of type ProducerConfig from the "Kafka" section of the config:
builder.Services.Configure<MyConfig>(builder.Configuration.GetSection("MyConfigSection"));
*/

builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("SchemaRegistry"));


/* TODO: Register an IProducer of type <String, String>:
builder.Services.AddSingleton<IProducer<Key, Value>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<MyConfig>>();

    return new ProducerBuilder<Key, Value>(config.Value)
        .Build();
});
*/


builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();

    return new CachedSchemaRegistryClient(config.Value);
});


builder.Services.AddSingleton<IProducer<String, Biometrics>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
    var schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();
    return new ProducerBuilder<String, Biometrics>(config.Value)   
    .SetValueSerializer(new JsonSerializer<Biometrics>(schemaRegistry))
    .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();