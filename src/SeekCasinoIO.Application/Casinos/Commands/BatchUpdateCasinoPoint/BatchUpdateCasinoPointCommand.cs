using SeekCasinoIO.Application.Common.Abstractions.Messaging;

namespace SeekCasinoIO.Application.Casinos.Commands.BatchUpdateCasinoPoint;

public record CasinoPointUpdate(Guid CasinoId, decimal NewPoint);

public record BatchUpdateCasinoPointCommand : ICommand<int>
{
    public List<CasinoPointUpdate> Updates { get; init; } = new();
}