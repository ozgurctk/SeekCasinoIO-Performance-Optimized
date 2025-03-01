using ErrorOr;
using SeekCasinoIO.Application.Common.Abstractions.Messaging;
using SeekCasinoIO.Application.Common.Interfaces.Repositories;

namespace SeekCasinoIO.Application.Casinos.Queries.GetCasinoById;

public class GetCasinoByIdQueryHandler : IQueryHandler<GetCasinoByIdQuery, CasinoDetailsDto>
{
    private readonly ICasinoRepository _casinoRepository;

    public GetCasinoByIdQueryHandler(ICasinoRepository casinoRepository)
    {
        _casinoRepository = casinoRepository;
    }

    public async Task<ErrorOr<CasinoDetailsDto>> Handle(
        GetCasinoByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Doğrudan DTO olarak veriyi al - projeksiyonu veritabanı seviyesinde yap
        var casinoDetails = await _casinoRepository.GetCasinoDetailsDtoAsync(
            request.Id, 
            cancellationToken);

        if (casinoDetails is null)
        {
            return Error.NotFound("Casino.NotFound", "Casino bulunamadı");
        }

        return casinoDetails;
    }
}