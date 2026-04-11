namespace Blockchain.Application.DTO
{
    public class BlockchainDataDto
    {
        public int Id { get; set; }
        public string BlockchainType { get; set; } = string.Empty;
        public string JsonData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
