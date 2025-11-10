using ClientConsumerOrder.Services;
using Ordering.Infraestructure.EventMessage;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<OrderStorageService>();
builder.Services.AddSingleton<RabbitMQPublisherService>();

// Registrar la conexión RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "host.docker.internal",
        Port = 5672,
        UserName = "guest",
        Password = "guest"
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Registrar el consumidor
builder.Services.AddHostedService<RabbitMQOrderConsumer>();

// Configurar Swagger con más detalles
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Client Consumer Order API",
        Version = "v1",
        Description = "API para consumir órdenes desde RabbitMQ"
    });

    // Incluir comentarios XML (opcional)
    // var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Client Consumer Order API v1");
        c.RoutePrefix = string.Empty; // Hace que Swagger esté en la raíz: http://localhost:8000/
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Client Consumer Order API v1");
        c.RoutePrefix = "swagger"; // En producción: http://localhost:8000/swagger
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();