using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enum moved to Enums/PaymentEnums.cs
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        public PaymentResultStatus Status { get; set; }

        [Required]
        public DateTime PaymentTime { get; set; } = DateTime.UtcNow;

        // Navigation and workflow fields
        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Event? Event { get; set; }
    }
}
