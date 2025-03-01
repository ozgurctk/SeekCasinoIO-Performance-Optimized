using ErrorOr;
using Microsoft.Extensions.Logging;
using SeekCasinoIO.Application.Common.Abstractions.Messaging;
using SeekCasinoIO.Application.Common.Interfaces;
using SeekCasinoIO.Application.Common.Interfaces.Repositories;

namespace SeekCasinoIO.Application.Casinos.Commands.BatchUpdateCasinoPoint;

public class BatchUpdateCasinoPointCommandHandler : ICommandHandler<BatchUpdateCasinoPointCommand, int>
{
    private readonly ICasinoRepository _casinoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BatchUpdateCasinoPointCommandHandler> _logger;

    public BatchUpdateCasinoPointCommandHandler(
        ICasinoRepository casinoRepository,
        IUnitOfWork unitOfWork,
        ILogger<BatchUpdateCasinoPointCommandHandler> logger)
    {
        _casinoRepository = casinoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<int>> Handle(
        BatchUpdateCasinoPointCommand request, 
        CancellationToken cancellationToken)
    {
        if (request.Updates.Count == 0)
        {
            return Error.Validation("Updates.Empty", "Güncellenecek casino bulunamadı");
        }

        _logger.LogInformation("{Count} casino için toplu puan güncellemesi başlatılıyor", request.Updates.Count);

        try
        {
            // Dictionary oluştur
            var casinoPointDict = request.Updates
                .ToDictionary(u => u.CasinoId, u => u.NewPoint);

            // Toplu güncelleme yapılıyor
            var updatedCount = await _casinoRepository.BatchUpdatePointAsync(
                casinoPointDict, 
                cancellationToken);

            // Değişiklikleri kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{Count} casino puanı başarıyla güncellendi", updatedCount);

            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu casino puanı güncelleme sırasında hata oluştu");
            return Error.Failure("BatchUpdate.Failed", "Toplu güncelleme işlemi başarısız oldu");
        }
    }
}