using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GiveAID.Domain.Enums;

namespace GiveAID.Domain.Entities;

public class Donation : BaseEntity
{
    [Key]
    public int DonationId { get; set; }

    // M-10: Payment status state machine
    // Valid transitions:
    //   Pending → Completed (on payment success)
    //   Pending → Failed (on payment failure)
    //   Completed → Refunded (on refund)
    // Invalid transitions are blocked with exceptions
    
    private string _paymentStatus = "Pending";
    
    [Required]
    public int? UserId { get; set; }

    [Required]
    public int CauseId { get; set; }

    public int? CampaignId { get; set; }
    
    public int? OrganizationId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PaymentStatus 
    { 
        get => _paymentStatus;
        set => _paymentStatus = value;
    }

    [MaxLength(4)]
    public string? CardLastFour { get; set; }

    [MaxLength(20)]
    public string? CardType { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }

    public bool IsAnonymous { get; set; }
    public bool ReceiptSent { get; set; }
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }

    [MaxLength(100)]
    public string? GatewayTransactionId { get; set; }

    public DateTime? PaymentConfirmedAt { get; set; }

    // M-10: Domain methods for state transitions
    // These enforce valid transitions and prevent invalid ones
    
    public void MarkAsCompleted()
    {
        if (_paymentStatus == "Completed")
            return; // Idempotent - already completed
            
        if (_paymentStatus == "Refunded")
            throw new InvalidOperationException("Cannot mark a refunded donation as completed.");
            
        if (_paymentStatus == "Failed")
            throw new InvalidOperationException("Cannot mark a failed donation as completed. Create a new donation instead.");
            
        // Only Pending → Completed is valid
        if (_paymentStatus != "Pending")
            throw new InvalidOperationException($"Cannot transition from '{_paymentStatus}' to 'Completed'.");
            
        _paymentStatus = "Completed";
        PaymentConfirmedAt = DateTime.UtcNow;
    }
    
    public void MarkAsFailed()
    {
        if (_paymentStatus == "Failed")
            return; // Idempotent - already failed
            
        if (_paymentStatus == "Refunded")
            throw new InvalidOperationException("Cannot mark a refunded donation as failed.");
            
        if (_paymentStatus == "Completed")
            throw new InvalidOperationException("Cannot mark a completed donation as failed. Process a refund instead.");
            
        // Only Pending → Failed is valid
        if (_paymentStatus != "Pending")
            throw new InvalidOperationException($"Cannot transition from '{_paymentStatus}' to 'Failed'.");
            
        _paymentStatus = "Failed";
    }
    
    public void MarkAsRefunded()
    {
        if (_paymentStatus == "Refunded")
            return; // Idempotent - already refunded
            
        if (_paymentStatus == "Pending")
            throw new InvalidOperationException("Cannot refund a pending donation. Wait for payment to complete first.");
            
        if (_paymentStatus == "Failed")
            throw new InvalidOperationException("Cannot refund a failed donation.");
            
        // Only Completed → Refunded is valid
        if (_paymentStatus != "Completed")
            throw new InvalidOperationException($"Cannot transition from '{_paymentStatus}' to 'Refunded'.");
            
        _paymentStatus = "Refunded";
    }
    
    public bool CanTransitionTo(string newStatus)
    {
        return (_paymentStatus, newStatus) switch
        {
            ("Pending", "Completed") => true,
            ("Pending", "Failed") => true,
            ("Completed", "Refunded") => true,
            _ => false
        };
    }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("CauseId")]
    public virtual Cause? Cause { get; set; }

    [ForeignKey("CampaignId")]
    public virtual Campaign? Campaign { get; set; }

    [ForeignKey("OrganizationId")]
    public virtual Organization? Organization { get; set; }
}
