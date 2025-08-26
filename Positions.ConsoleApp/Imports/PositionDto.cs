namespace Positions.ConsoleApp.Imports
{
    public record PositionDto(
        string PositionId,
        string ProductId,
        string ClientId,
        DateOnly Date,
        decimal Value,
        decimal Quantity
    );
}
