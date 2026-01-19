using System.ComponentModel.DataAnnotations;

namespace EfCoreLab.DTOs
{
    public class InvoiceDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "InvoiceNumber is required.")]
        [RegularExpression(@"^INV.*", ErrorMessage = "InvoiceNumber must start with 'INV'.")]
        [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters.")]
        public string InvoiceNumber { get; set; } = string.Empty;

        public long CustomerId { get; set; }

        public DateTime InvoiceDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0.")]
        public decimal Amount { get; set; }
    }

    public class CreateInvoiceDto
    {
        [Required(ErrorMessage = "InvoiceNumber is required.")]
        [RegularExpression(@"^INV.*", ErrorMessage = "InvoiceNumber must start with 'INV'.")]
        [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters.")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, long.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public long CustomerId { get; set; }

        [Required(ErrorMessage = "InvoiceDate is required.")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0.")]
        public decimal Amount { get; set; }
    }

    public class UpdateInvoiceDto
    {
        [Required(ErrorMessage = "InvoiceNumber is required.")]
        [RegularExpression(@"^INV.*", ErrorMessage = "InvoiceNumber must start with 'INV'.")]
        [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters.")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "InvoiceDate is required.")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0.")]
        public decimal Amount { get; set; }
    }
}
