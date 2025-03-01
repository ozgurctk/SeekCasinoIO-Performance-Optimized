using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using SeekCasinoIO.API.Middleware;
using SeekCasinoIO.Application;
using SeekCasinoIO.Infrastructure;
using SeekCasinoIO.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Performans optimizasyonları için ek konfigürasyonlar
builder.WebHost.ConfigureKestrel(options =>
{
    // Thread pool yönetimi için optimizasyonlar
    ThreadPool.SetMinThreads(100, 100);

    // HTTP/2 desteği - Taleplerin hızlı işlenmesi için
    options.AllowSynchronousIO = false;
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Controller'lar için performans optimizasyonları
    options.MaxModelValidationErrors = 50;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SeekCasinoIO API", Version = "v1" });

    // JWT yetkilendirme için Swagger yapılandırması
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add application layer services
builder.Services.AddApplicationServices();

// Add infrastructure layer services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Distributed Caching (Redis for production)
if (builder.Environment.IsProduction())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "SeekCasinoIO:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Add HTTP Logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml", "text/plain" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Output Caching
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
    options.AddPolicy("ShortCache", builder => builder.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy("MediumCache", builder => builder.Expire(TimeSpan.FromMinutes(1)));
    options.AddPolicy("LongCache", builder => builder.Expire(TimeSpan.FromHours(1)));
});

var app = builder.Build();

// Initialize database with default roles and admin user
await DatabaseInitializer.InitializeDatabaseAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware'lerin sıralaması performans için önemlidir
app.UseResponseCompression(); // İlk sırada
app.UseOutputCache(); // İkinci sırada

// Use CORS
app.UseCors("AllowAll");

// Use HTTP Logging (yalnızca geliştirme ortamında)
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
}

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();