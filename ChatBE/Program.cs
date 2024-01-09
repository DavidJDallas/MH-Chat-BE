using DataStoreApi.Models;
using DataStoreApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

//builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("WeatherServerDatabase"));

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ChatServerDatabase"));

//Below the DataService Class is registered with depdency injection to support constructor injection in consuming classes.
builder.Services.AddSingleton<DataService>();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => 
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    }
    );
}
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
    
};


app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("CorsPolicy");

app.MapControllers();

app.UseWebSockets(webSocketOptions);

// KeepAliveInterval: How frequently to send "ping" frames to the client to nsure proxies keep the connection open.
// AllowedOrigins: A list of allowed origin header values for WebSocket requests. By default, all origins are allowed. 

app.Run();
