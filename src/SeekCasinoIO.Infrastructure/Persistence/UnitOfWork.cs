using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SeekCasinoIO.Application.Common.Interfaces;

namespace SeekCasinoIO.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(
        ApplicationDbContext context,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Entity Framework'ün değişiklik izleme mekanizmasını etkin kullanmak için
            // değişiklikleri log'lama (geliştirme/debug için)
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    _logger.LogDebug("Entity: {EntityType}, State: {State}", 
                        entry.Entity.GetType().Name, entry.State);
                }
            }

            var result = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Veritabanına {count} değişiklik kaydedildi", result);
            
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Eşzamanlılık hatası oluştu");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Veritabanı güncelleme hatası");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritabanı işlemi sırasında beklenmeyen hata");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            _logger.LogWarning("İşlem zaten başlatılmış");
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Yeni veritabanı işlemi başlatıldı");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("İşlem başarıyla commit edildi");
            }
            else
            {
                _logger.LogWarning("Commit edilecek aktif bir işlem bulunamadı");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlem commit edilirken hata oluştu, rollback gerçekleştiriliyor");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("İşlem geri alındı (rollback)");
            }
            else
            {
                _logger.LogWarning("Geri alınacak (rollback) aktif bir işlem bulunamadı");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlem geri alınırken (rollback) hata oluştu");
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Yönetilen kaynakları serbest bırak
            _transaction?.Dispose();
            _context.Dispose();
        }

        // Yönetilmeyen kaynakları serbest bırak ve büyük alanları null olarak ayarla
        _disposed = true;
    }
}