using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using SeekCasinoIO.Application.Common.Abstractions.Messaging;
using SeekCasinoIO.Application.Common.Interfaces;

namespace SeekCasinoIO.Application.Common.Behaviors;

public class EventualConsistencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IEventualConsistencyContext _eventualConsistencyContext;
    private readonly ILogger<EventualConsistencyBehavior<TRequest, TResponse>> _logger;

    public EventualConsistencyBehavior(
        IEventualConsistencyContext eventualConsistencyContext,
        ILogger<EventualConsistencyBehavior<TRequest, TResponse>> logger)
    {
        _eventualConsistencyContext = eventualConsistencyContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Sadece transactional işlem gerektiren Command'ler için transaction başlat
        // bazı komutlar veritabanını değiştirmediği için, bunlara transaction gereksiz
        if (!NeedsTransaction(request))
        {
            return await next();
        }

        // Her istek için eşsiz bir transaction ID oluştur
        var transactionId = Guid.NewGuid();

        try
        {
            _logger.LogInformation(
                "{RequestType} için transaction {TransactionId} başlatılıyor",
                typeof(TRequest).Name,
                transactionId);

            _eventualConsistencyContext.BeginTransaction(transactionId);

            var response = await next();

            if (!response.IsError)
            {
                await _eventualConsistencyContext.CommitTransactionAsync(cancellationToken);
                _logger.LogInformation(
                    "Transaction {TransactionId} başarıyla commit edildi",
                    transactionId);
            }
            else
            {
                _eventualConsistencyContext.RollbackTransaction();
                _logger.LogWarning(
                    "Transaction {TransactionId} hatalar nedeniyle geri alındı",
                    transactionId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _eventualConsistencyContext.RollbackTransaction();
            _logger.LogError(
                ex,
                "Transaction {TransactionId} işlemi sırasında hata oluştu. İşlem geri alınıyor.",
                transactionId);
            throw;
        }
    }

    private static bool NeedsTransaction(TRequest request)
    {
        // Command sınıflarının çoğu veritabanını değiştirir ve transaction gerektirir
        // Ancak bazı özel durumların transaction gerektirmediğini belirtmek için bu metodu kullanabiliriz
        
        // 1. Ekleme, Güncelleme, Silme komutları için transaction gereklidir
        if (request is ICommand || request is ICommand<TResponse>)
        {
            // İstisnai durumlar - transaction gerektirmeyen komutlar
            var typeName = request.GetType().Name;
            
            if (typeName.EndsWith("ReportCommand") ||  // Raporlama komutları
                typeName.EndsWith("ExportCommand") ||  // Dışa aktarma komutları 
                typeName.EndsWith("NotificationCommand")) // Bildirim komutları
            {
                return false;
            }
            
            return true;
        }
        
        // 2. Sorgular genellikle transaction gerektirmez
        return false;
    }
}