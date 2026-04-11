using System.ComponentModel.DataAnnotations;

namespace Blockchain.Application.Validators
{
    public class FetchBlockchainRequest
    {
        [Required(ErrorMessage = "Blockchain type is required.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "Blockchain type must be between 3 and 10 characters.")]
        public string Type { get; set; } = string.Empty;
    }

    public class BlockchainRequestValidator
    {
        private static readonly HashSet<string> ValidTypes = new() { "BTC", "ETH", "LTC", "DASH" };

        public static bool IsValidBlockchainType(string type)
        {
            return !string.IsNullOrWhiteSpace(type) && ValidTypes.Contains(type.ToUpperInvariant());
        }

        public static string GetValidationError(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "Blockchain type is required.";

            if (!IsValidBlockchainType(type))
                return $"Invalid blockchain type. Valid types are: {string.Join(", ", ValidTypes)}";

            return string.Empty;
        }
    }
}
