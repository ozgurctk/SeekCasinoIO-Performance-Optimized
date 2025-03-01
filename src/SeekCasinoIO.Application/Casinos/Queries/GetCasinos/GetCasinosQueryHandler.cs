using ErrorOr;
using SeekCasinoIO.Application.Common.Abstractions.Messaging;
using SeekCasinoIO.Application.Common.Interfaces.Repositories;
using System.Linq.Expressions;
using SeekCasinoIO.Core.Entities;

namespace SeekCasinoIO.Application.Casinos.Queries.GetCasinos;

public class GetCasinosQueryHandler : IQueryHandler<GetCasinosQuery, CasinoListResponse>
{
    private readonly ICasinoRepository _casinoRepository;

    public GetCasinosQueryHandler(ICasinoRepository casinoRepository)
    {
        _casinoRepository = casinoRepository;
    }

    public async Task<ErrorOr<CasinoListResponse>> Handle(
        GetCasinosQuery request,
        CancellationToken cancellationToken)
    {
        // Filtreleme için Expression oluştur
        Expression<Func<Casino, bool>>? predicate = BuildFilterPredicate(request);
        
        // CasinoDto'ları doğrudan repository'den al (veri dönüşümünü veritabanı seviyesinde yap)
        var casinoDtos = await _casinoRepository.GetAllCasinoDtosAsync(
            request.PageSize,
            request.PageNumber,
            predicate,
            cancellationToken);

        // Filtreleme için kullanılacak alanı belirle
        int totalCount;
        if (predicate != null)
        {
            totalCount = await _casinoRepository.CountAsync(predicate, cancellationToken);
        }
        else
        {
            totalCount = await _casinoRepository.GetTotalCountAsync(cancellationToken);
        }

        // Sayfalama bilgilerini oluştur
        int pageSize = request.PageSize ?? 10;
        int pageNumber = request.PageNumber ?? 1;
        int pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new CasinoListResponse(
            casinoDtos.ToList(),
            new PaginationInfo(
                totalCount,
                pageCount,
                pageNumber,
                pageSize
            )
        );

        return response;
    }

    private static Expression<Func<Casino, bool>>? BuildFilterPredicate(GetCasinosQuery request)
    {
        // Sadece bir filtre uygulanabilir, en yüksek öncelikle başlayarak kontrol et
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            return c => c.Name.Contains(request.SearchTerm) || c.Description.Contains(request.SearchTerm);
        }
        
        if (request.LicenceId.HasValue)
        {
            Guid licenceId = request.LicenceId.Value;
            return c => c.Licences.Any(l => l.Id == licenceId);
        }
        
        if (request.ProviderId.HasValue)
        {
            Guid providerId = request.ProviderId.Value;
            return c => c.Providers.Any(p => p.Id == providerId);
        }
        
        if (request.GameTypeId.HasValue)
        {
            Guid gameTypeId = request.GameTypeId.Value;
            return c => c.GameTypes.Any(g => g.Id == gameTypeId);
        }
        
        if (request.LanguageId.HasValue)
        {
            Guid languageId = request.LanguageId.Value;
            return c => c.Languages.Any(l => l.Id == languageId);
        }
        
        if (request.PaymentMethodId.HasValue)
        {
            Guid paymentMethodId = request.PaymentMethodId.Value;
            return c => c.PaymentMethods.Any(p => p.Id == paymentMethodId);
        }
        
        return null; // Filtre yok
    }
}