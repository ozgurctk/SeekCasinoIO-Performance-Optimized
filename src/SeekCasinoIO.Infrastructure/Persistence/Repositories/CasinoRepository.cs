using Microsoft.EntityFrameworkCore;
using SeekCasinoIO.Application.Common.Interfaces.Repositories;
using SeekCasinoIO.Application.Casinos.Queries.GetCasinos;
using SeekCasinoIO.Application.Casinos.Queries.GetCasinoById;
using SeekCasinoIO.Core.Entities;
using System.Linq.Expressions;

namespace SeekCasinoIO.Infrastructure.Persistence.Repositories;

public class CasinoRepository : Repository<Casino>, ICasinoRepository
{
    public CasinoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Casino?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Split queries for better performance
        return await _dbSet
            .AsSplitQuery()
            .Include(c => c.PaymentMethods)
            .Include(c => c.Licences)
            .Include(c => c.GameTypes)
            .Include(c => c.Languages)
            .Include(c => c.Providers)
            .Include(c => c.ScreenShots)
            .Include(c => c.Positives)
            .Include(c => c.Negatives)
            .Include(c => c.InterestedFacts)
            .Include(c => c.WithdrawLimits)
            .Include(c => c.NoDepositBonuses)
            .Include(c => c.DepositBonuses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetAllWithDetailsAsync(
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.PaymentMethods)
            .Include(c => c.Licences)
            .Include(c => c.GameTypes)
            .Include(c => c.Languages)
            .Include(c => c.Providers)
            .Include(c => c.Positives)
            .Include(c => c.Negatives)
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CasinoDto>> GetAllCasinoDtosAsync(
        int? pageSize = null,
        int? pageNumber = null,
        Expression<Func<Casino, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.PaymentMethods)
            .Include(c => c.Licences)
            .Include(c => c.GameTypes)
            .Include(c => c.Languages)
            .Include(c => c.Providers)
            .Include(c => c.Positives)
            .Include(c => c.Negatives);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        query = query.OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        // Return as DTO directly to avoid object mapping
        return await query.Select(c => new CasinoDto(
            c.Id,
            c.Name,
            c.Point,
            c.Owner,
            c.Operator,
            c.EstablishedDate,
            c.AnnualRevenue,
            c.CasinoLogoNameOnDisk,
            c.Description,
            c.CasinoExternalAddress,
            c.PaymentMethods.Select(pm => pm.Name).ToList(),
            c.Licences.Select(l => l.Name).ToList(),
            c.GameTypes.Select(gt => gt.Name).ToList(),
            c.Languages.Select(l => l.Name).ToList(),
            c.Providers.Select(p => p.Name).ToList(),
            c.Positives.Select(p => p.Detail).ToList(),
            c.Negatives.Select(p => p.Detail).ToList()
        )).ToListAsync(cancellationToken);
    }

    public async Task<CasinoDetailsDto?> GetCasinoDetailsDtoAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(c => c.Id == id)
            .Select(c => new CasinoDetailsDto(
                c.Id,
                c.Name,
                c.Point,
                c.Owner,
                c.Operator,
                c.EstablishedDate,
                c.AnnualRevenue,
                c.CasinoLogoNameOnDisk,
                c.Description,
                c.CasinoInternalAddress,
                c.CasinoExternalAddress,
                c.CasinoGuruAddress,
                c.BonusDetailHtml,
                
                c.PaymentMethods.Select(p => new PaymentMethodDto(
                    p.Id, p.Name, p.ImageNameOnDisc, p.CasinoGuruImageAddress)).ToList(),
                
                c.Licences.Select(l => new LicenceDto(
                    l.Id, l.Name, l.ImageNameOnDisc)).ToList(),
                
                c.GameTypes.Select(g => new GameTypeDto(
                    g.Id, g.Name, g.ImageNameOnDisc)).ToList(),
                
                c.Languages.Select(l => new LanguageDto(
                    l.Id, l.Name, l.ImageNameOnDisc)).ToList(),
                
                c.Providers.Select(p => new ProviderDto(
                    p.Id, p.Name, p.ImageNameOnDisc, p.CasinoGuruIconAddress)).ToList(),
                
                c.ScreenShots.Select(s => new ScreenShotDto(
                    s.Id, s.Name, s.ImageNameOnDisc)).ToList(),
                
                c.Positives.Select(p => new PositiveDto(
                    p.Id, p.Detail)).ToList(),
                
                c.Negatives.Select(n => new NegativeDto(
                    n.Id, n.Detail)).ToList(),
                
                c.InterestedFacts.Select(i => new InterestedFactDto(
                    i.Id, i.Detail)).ToList(),
                
                c.WithdrawLimits.Select(w => new WithdrawLimitDto(
                    w.Id, w.Detail)).ToList(),
                
                c.NoDepositBonuses.Select(n => new NoDepositBonusDto(
                    n.Id, n.Detail)).ToList(),
                
                c.DepositBonuses.Select(d => new DepositBonusDto(
                    d.Id, d.Detail)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetByLicenceAsync(
        Guid licenceId, 
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .Where(c => c.Licences.Any(l => l.Id == licenceId))
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetByProviderAsync(
        Guid providerId, 
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .Where(c => c.Providers.Any(p => p.Id == providerId))
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetByGameTypeAsync(
        Guid gameTypeId, 
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .Where(c => c.GameTypes.Any(g => g.Id == gameTypeId))
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetByLanguageAsync(
        Guid languageId, 
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .Where(c => c.Languages.Any(l => l.Id == languageId))
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetByPaymentMethodAsync(
        Guid paymentMethodId, 
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .Where(c => c.PaymentMethods.Any(p => p.Id == paymentMethodId))
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> SearchAsync(
        string searchTerm, 
        int? pageSize = null, 
        int? pageNumber = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Casino> query = _dbSet
            .AsNoTracking()
            .Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm))
            .OrderByDescending(c => c.Point);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Casino>> GetTopRatedCasinosAsync(
        int count, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(c => c.Point)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<int> GetFilteredCountAsync(
        string? searchTerm = null,
        Guid? licenceId = null,
        Guid? providerId = null,
        Guid? gameTypeId = null,
        Guid? languageId = null,
        Guid? paymentMethodId = null,
        CancellationToken cancellationToken = default)
    {
        // Prepare the base query
        IQueryable<Casino> query = _dbSet.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm));
        }
        else if (licenceId.HasValue)
        {
            query = query.Where(c => c.Licences.Any(l => l.Id == licenceId));
        }
        else if (providerId.HasValue)
        {
            query = query.Where(c => c.Providers.Any(p => p.Id == providerId));
        }
        else if (gameTypeId.HasValue)
        {
            query = query.Where(c => c.GameTypes.Any(g => g.Id == gameTypeId));
        }
        else if (languageId.HasValue)
        {
            query = query.Where(c => c.Languages.Any(l => l.Id == languageId));
        }
        else if (paymentMethodId.HasValue)
        {
            query = query.Where(c => c.PaymentMethods.Any(p => p.Id == paymentMethodId));
        }

        return await query.CountAsync(cancellationToken);
    }

    // Batch processing method for bulk operations
    public async Task<int> BatchUpdatePointAsync(
        Dictionary<Guid, decimal> casinoPoints, 
        CancellationToken cancellationToken = default)
    {
        // Get all casino IDs to update
        var casinoIds = casinoPoints.Keys.ToList();
        
        // Fetch casinos to update in a single query
        var casinos = await _dbSet
            .Where(c => casinoIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        // Update points
        foreach (var casino in casinos)
        {
            if (casinoPoints.TryGetValue(casino.Id, out var newPoint))
            {
                casino.Point = newPoint;
                casino.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        // Context will be saved by the UnitOfWork
        return casinos.Count;
    }
}