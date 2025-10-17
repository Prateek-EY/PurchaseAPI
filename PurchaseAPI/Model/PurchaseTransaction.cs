using System.ComponentModel.DataAnnotations;

namespace PurchaseAPI.Model
{
    public class PurchaseTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); 
        [Required]
        [MaxLength(50, ErrorMessage = "Description cannot exceed 50 characters")]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Purchase amount must be positive")]
        public decimal AmountUSD { get; set; }
    }
}
