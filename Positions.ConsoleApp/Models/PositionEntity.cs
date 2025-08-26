#nullable disable
namespace Positions.ConsoleApp.Models
{
    public sealed class PositionEntity
    {
        public string PositionId { get; private set; }
        public DateOnly Date { get; private set; }
        public string ProductId { get; private set; }
        public string ClientId { get; private set; }
        public decimal Value { get; private set; }
        public decimal Quantity { get; private set; }


        public PositionEntity(
            string positionId,
            string productId,
            string clientId,
            DateOnly date,
            decimal value,
            decimal quantity)
        {
            PositionId = positionId;
            ProductId = productId;
            ClientId = clientId;
            Date = date;
            Value = value;
            Quantity = quantity;
        }

        private PositionEntity() { }
    }
}
