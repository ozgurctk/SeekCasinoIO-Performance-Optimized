using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SeekCasinoIO.Application.Common.Attributes;
using System.Text;
using System.Text.Json;

namespace SeekCasinoIO.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheAttribute = request.GetType().GetCustomAttributes(typeof(CacheableQueryAttribute), true)
            .FirstOrDefault() as CacheableQueryAttribute;

        if (cacheAttribute == null)
        {
            return await next();
        }

        // Daha detaylı önbellek anahtarı oluşturma
        var cacheKey = GenerateCacheKey(request);
        var cachedResponse = await _cache.GetAsync(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            _logger.LogInformation("Önbellekten veri çekiliyor: {CacheKey}", cacheKey);
            try
            {
                var cachedResult = JsonSerializer.Deserialize<TResponse>(
                    Encoding.UTF8.GetString(cachedResponse),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                
                if (cachedResult != null)
                {
                    return cachedResult;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Önbellekten veri çekilirken deserializasyon hatası: {CacheKey}", cacheKey);
                // Önbellek verisi bozuksa, silip devam edelim
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
        }

        // Önbellekte yoksa veya deserializasyon hatası olduysa, veriyi gerçekten çekelim
        var response = await next();

        // Önbellek süresini belirleme
        var slidingExpiration = GetSlidingExpirationTime(cacheAttribute, request);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) // Maksimum 1 saat önbellekte kalabilir
        };

        try
        {
            var serializedResponse = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            );
            
            await _cache.SetAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);
            _logger.LogInformation("Önbelleğe ekleniyor: {CacheKey}, Süre: {Expiration} dakika", 
                cacheKey, slidingExpiration.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Önbelleğe veri eklenirken hata oluştu: {CacheKey}", cacheKey);
        }

        return response;
    }

    private static string GenerateCacheKey(TRequest request)
    {
        // Daha spesifik önbellek anahtarı oluşturmak için request tipini ve özelliklerini kullan
        var requestType = request.GetType().Name;
        var requestHash = JsonSerializer.Serialize(request, new JsonSerializerOptions { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        }).GetHashCode();
        
        return $"{requestType}_{requestHash}";
    }

    private static TimeSpan GetSlidingExpirationTime(CacheableQueryAttribute attribute, TRequest request)
    {
        // Varsayılan süre
        var defaultExpiration = attribute.SlidingExpiration == default
            ? TimeSpan.FromMinutes(5)
            : attribute.SlidingExpiration;

        // İstek türüne göre özel süre belirleme
        var requestTypeName = request.GetType().Name;
        
        return requestTypeName switch
        {
            // Daha sık değişen veriler için daha kısa süreler
            "GetTopRatedCasinosQuery" => TimeSpan.FromMinutes(2),
            
            // Orta sıklıkta değişen veriler için orta süreler
            "GetCasinosQuery" => TimeSpan.FromMinutes(10),
            
            // Nadir değişen veriler için daha uzun süreler
            "GetCasinoByIdQuery" => TimeSpan.FromMinutes(30),
            "GetAllGameTypesQuery" => TimeSpan.FromHours(1),
            "GetAllLanguagesQuery" => TimeSpan.FromHours(2),
            "GetAllLicencesQuery" => TimeSpan.FromHours(2),
            "GetAllPaymentMethodsQuery" => TimeSpan.FromHours(2),
            "GetAllProvidersQuery" => TimeSpan.FromHours(2),
            
            // Diğer durumlarda varsayılan süre
            _ => defaultExpiration
        };
    }
}