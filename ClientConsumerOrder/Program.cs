using ClientConsumerOrder.Consumers;
using ClientConsumerOrder.Hubs;
using ClientConsumerOrder.Services;
using MassTransit;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// CORS para Vue / desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ MASS TRANSIT - UNA SOLA CONFIGURACIÓN
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<OrderUpdatedConsumer>();
    x.AddConsumer<OrderDeletedConsumer>();
    x.AddConsumer<NotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("host.docker.internal", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // ✅ CONFIGURACIÓN GLOBAL PARA JSON PLANO
        cfg.ClearSerialization();
        cfg.UseRawJsonSerializer();

        // Configurar opciones JSON
        cfg.ConfigureJsonSerializerOptions(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            return options;
        });

        // Configurar colas específicas
        cfg.ReceiveEndpoint("order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("order-updated-queue", e =>
        {
            e.ConfigureConsumer<OrderUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("order-deleted-queue", e =>
        {
            e.ConfigureConsumer<OrderDeletedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-queue", e =>
        {
            e.ConfigureConsumer<NotificationConsumer>(context);
        });
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueApp");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notifications");
app.MapGet("/health", () => "Healthy");

app.Run();