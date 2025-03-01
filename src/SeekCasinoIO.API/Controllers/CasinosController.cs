using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SeekCasinoIO.Application.Casinos.Commands.BatchUpdateCasinoPoint;
using SeekCasinoIO.Application.Casinos.Commands.CreateCasino;
using SeekCasinoIO.Application.Casinos.Queries.GetCasinoById;
using SeekCasinoIO.Application.Casinos.Queries.GetCasinos;

namespace SeekCasinoIO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CasinosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CasinosController> _logger;

    public CasinosController(
        IMediator mediator, 
        ILogger<CasinosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [OutputCache(PolicyName = "MediumCache")] // Liste sorguları için 1 dakikalık önbellek
    public async Task<IActionResult> GetCasinos(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? licenceId,
        [FromQuery] Guid? providerId,
        [FromQuery] Guid? gameTypeId,
        [FromQuery] Guid? languageId,
        [FromQuery] Guid? paymentMethodId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Casino listesi istendi");

        var query = new GetCasinosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            LicenceId = licenceId,
            ProviderId = providerId,
            GameTypeId = gameTypeId,
            LanguageId = languageId,
            PaymentMethodId = paymentMethodId
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            casinos => {
                _logger.LogInformation("Casino listesi döndürüldü, {ItemCount} adet", casinos.Casinos.Count);
                return Ok(casinos);
            },
            errors => {
                _logger.LogWarning("Casino listesi döndürülürken hata: {ErrorCount} adet", errors.Count);
                return BadRequest(errors);
            });
    }

    [HttpGet("{id:guid}")]
    [OutputCache(PolicyName = "LongCache")] // Detay sorguları için 1 saatlik önbellek
    public async Task<IActionResult> GetCasino(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Casino detayı istendi: {Id}", id);
        
        var query = new GetCasinoByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            casino => {
                _logger.LogInformation("Casino detayı döndürüldü: {Id}", id);
                return Ok(casino);
            },
            errors => {
                if (errors.FirstOrDefault().Type == ErrorOr.ErrorType.NotFound) {
                    _logger.LogWarning("Casino bulunamadı: {Id}", id);
                    return NotFound(errors);
                }
                
                _logger.LogWarning("Casino detayı alınırken hata: {ErrorCount} adet", errors.Count);
                return BadRequest(errors);
            });
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> CreateCasino(
        [FromBody] CreateCasinoCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni casino oluşturma isteği: {Name}", command.Name);
        
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            casinoId => {
                _logger.LogInformation("Casino başarıyla oluşturuldu: {Id}", casinoId);
                return CreatedAtAction(
                    nameof(GetCasino),
                    new { id = casinoId },
                    new { id = casinoId });
            },
            errors => {
                _logger.LogWarning("Casino oluşturulurken hata: {ErrorCount} adet", errors.Count);
                return BadRequest(errors);
            });
    }
    
    [HttpPost("batch-update-points")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> BatchUpdateCasinoPoints(
        [FromBody] BatchUpdateCasinoPointCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toplu casino puanı güncelleme isteği: {Count} adet", command.Updates.Count);
        
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            updatedCount => {
                _logger.LogInformation("Toplu casino puanı güncelleme başarılı: {Count} casino güncellendi", updatedCount);
                return Ok(new { UpdatedCount = updatedCount });
            },
            errors => {
                _logger.LogWarning("Toplu casino puanı güncelleme hatası: {ErrorCount} adet", errors.Count);
                return BadRequest(errors);
            });
    }
}