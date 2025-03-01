using System.Linq.Expressions;
using SeekCasinoIO.Application.Casinos.Queries.GetCasinos;
using SeekCasinoIO.Application.Casinos.Queries.GetCasinoById;
using SeekCasinoIO.Core.Entities;

namespace SeekCasinoIO.Application.Common.Interfaces.Repositories;

public interface ICasinoRepository : IRepository<Casino>
{
    Task<Casino?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetAllWithDetailsAsync(int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    // Yeni eklenen doğrudan DTO döndüren metotlar
    Task<IReadOnlyList<CasinoDto>> GetAllCasinoDtosAsync(
        int? pageSize = null,
        int? pageNumber = null,
        Expression<Func<Casino, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
    
    Task<CasinoDetailsDto?> GetCasinoDetailsDtoAsync(
        Guid id, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetByLicenceAsync(Guid licenceId, int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetByProviderAsync(Guid providerId, int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetByGameTypeAsync(Guid gameTypeId, int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetByLanguageAsync(Guid languageId, int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetByPaymentMethodAsync(Guid paymentMethodId, int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> SearchAsync(string searchTerm, int? pageSize = null, int? pageNumber = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Casino>> GetTopRatedCasinosAsync(int count, CancellationToken cancellationToken = default);
    
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    
    Task<int> GetFilteredCountAsync(
        string? searchTerm = null,
        Guid? licenceId = null,
        Guid? providerId = null,
        Guid? gameTypeId = null,
        Guid? languageId = null,
        Guid? paymentMethodId = null,
        CancellationToken cancellationToken = default);
    
    // Toplu işlem için yeni metot
    Task<int> BatchUpdatePointAsync(
        Dictionary<Guid, decimal> casinoPoints, 
        CancellationToken cancellationToken = default);
}