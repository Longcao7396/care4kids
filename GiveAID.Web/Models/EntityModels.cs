using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Web.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Index(IsUnique = true)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string Profession { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public string Permissions { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVerified { get; set; } = false;

        [MaxLength(100)]
        public string VerificationToken { get; set; }

        public DateTime? TokenExpiry { get; set; }

        public DateTime? LastLogin { get; set; }

        // SECURITY: tokens issued before this timestamp are rejected on every
        // authenticated request. Updated whenever the user changes their password
        // so any leaked token is invalidated at the next request.
        public DateTime? PasswordChangedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("Causes")]
    public class Cause
    {
        [Key]
        public int CauseId { get; set; }

        [MaxLength(20)]
        public string CauseCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string CauseName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(50)]
        public string Icon { get; set; }

        public decimal TargetAmount { get; set; }

        public decimal RaisedAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // ── 2-level hierarchy: a Cause can be a top-level category
        //    (ParentCauseId IS NULL) or a sub-item of a parent (e.g.
        //    "Mua sách vở" under "Giáo dục cho trẻ em").
        public int? ParentCauseId { get; set; }

        [ForeignKey("ParentCauseId")]
        public virtual Cause ParentCause { get; set; }

        public virtual ICollection<Cause> SubCauses { get; set; }

        // Campaigns that target THIS cause (only meaningful for leaf causes).
        public virtual ICollection<Campaign> Campaigns { get; set; }

        [NotMapped]
        public decimal PercentageReached
        {
            get
            {
                if (TargetAmount > 0)
                    return (RaisedAmount / TargetAmount) * 100;
                return 0;
            }
        }

        [NotMapped]
        public bool IsParentCause => ParentCauseId == null;
    }

    [Table("Campaigns")]
    public class Campaign
    {
        [Key]
        public int CampaignId { get; set; }

        [Required]
        public int CauseId { get; set; }

        [ForeignKey("CauseId")]
        public virtual Cause Cause { get; set; }

        public int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        [Required]
        [MaxLength(200)]
        public string CampaignName { get; set; }

        [MaxLength(50)]
        public string CampaignCode { get; set; }

        // ─── Merged from Programme ─────────────────────────
        // 'Education', 'HealthCare', 'ChildWelfare', 'WomenEmpowerment', etc.
        // Null = donation-only campaign (no event/activity classification).
        [MaxLength(50)]
        public string ProgrammeType { get; set; }

        // Whether users can register to participate (in addition to donating).
        public bool RegistrationRequired { get; set; } = false;

        // Capacity cap when RegistrationRequired = true.
        public int? MaxParticipants { get; set; }

        // Targeted beneficiary count for non-monetary impact tracking.
        public int? TargetBeneficiaries { get; set; }

        // Budget tracking — preserved from Programme for accounting parity.
        public decimal? ExpectedBudget { get; set; }
        public decimal? ActualBudget { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal GoalAmount { get; set; }

        public decimal RaisedAmount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public int? BeneficiariesCount { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public bool IsFeatured { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public decimal PercentageReached
        {
            get
            {
                if (GoalAmount > 0)
                    return Math.Min((RaisedAmount / GoalAmount) * 100, 100);
                return 0;
            }
        }

        [NotMapped]
        public int? DaysRemaining
        {
            get
            {
                if (EndDate.HasValue)
                {
                    var days = (EndDate.Value - DateTime.Now).Days;
                    return days >= 0 ? days : 0;
                }
                return null;
            }
        }

        [NotMapped]
        public int DonorCount { get; set; }

        // Navigation property for the Campaign → Donations one-to-many
        // relationship. EF6 infers this from Donation.CampaignId (the FK
        // declared on Donation) — no fluent config required. Populated
        // eagerly via .Include(c => c.Donations) or lazily on demand.
        public virtual ICollection<Donation> Donations { get; set; }

        // Indicates this campaign accepts donations (Campaign type).
        // Always true for unified Campaign model — kept as a NotMapped
        // convenience to make code that branched on Campaign vs Programme
        // easier to migrate without surprises.
        [NotMapped]
        public bool AcceptsDonations => true;

        // True when this campaign also accepts user registration/participation
        // (the merged Programme behaviour).
        [NotMapped]
        public bool AcceptsRegistration => RegistrationRequired;
    }

    [Table("CampaignRegistrations")]
    public class CampaignRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Registered";

        [MaxLength(500)]
        public string Notes { get; set; }

        public bool AttendanceConfirmed { get; set; } = false;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }

    [Table("CampaignReports")]
    public class CampaignReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [Required]
        public decimal TotalReceived { get; set; }

        [Required]
        public decimal TotalSpent { get; set; }

        [NotMapped]
        public decimal RemainingAmount
        {
            get { return TotalReceived - TotalSpent; }
        }

        public int? BeneficiariesReached { get; set; }

        [MaxLength(200)]
        public string ReportTitle { get; set; }

        public string ReportContent { get; set; }

        public string ExpenseBreakdown { get; set; }  // JSON

        public string Photos { get; set; }  // JSON array

        public string Documents { get; set; }  // JSON array

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedDate { get; set; }

        public int? PublishedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }

    [Table("Donations")]
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int CauseId { get; set; }

        [ForeignKey("CauseId")]
        public virtual Cause Cause { get; set; }

        public int? CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        public int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";

        [MaxLength(4)]
        public string CardLastFour { get; set; }

        [MaxLength(20)]
        public string CardType { get; set; }

        [MaxLength(100)]
        public string TransactionId { get; set; }

        [MaxLength(500)]
        public string Message { get; set; }

        public bool IsAnonymous { get; set; } = false;

        public bool ReceiptSent { get; set; } = false;

        public DateTime DonationDate { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Idempotency key — client-supplied; if a Donation already exists for
        // (UserId, IdempotencyKey), that donation is returned instead of creating
        // a duplicate. Prevents double-tap / double-submit / network retry bugs.
        // See DonationsController.Create for the duplicate-check logic.
        [MaxLength(100)]
        public string IdempotencyKey { get; set; }

        // ── Payment-gateway tracking (PCI-DSS friendly) ──
        // External transaction id from the payment processor (Stripe / VNPay / MoMo).
        // Null until the gateway webhook confirms the charge.
        [MaxLength(100)]
        public string GatewayTransactionId { get; set; }

        // Pending until gateway webhook flips it. The old behaviour of immediately
        // marking "Completed" was a financial-reporting bug: donations were counted
        // as money received even though no payment gateway had charged anything.
        // Now: POST /api/donations creates a "Pending" row, the gateway webhook
        // (or a manual admin confirm) flips it to "Completed" or "Failed".
        public DateTime? PaymentConfirmedAt { get; set; }

        // ── Payment Gateway Integration ──────────────────────────────────────────
        // Name of the gateway used for this donation ("stripe", "vnpay", "momo", "mock").
        [MaxLength(20)]
        public string PaymentGateway { get; set; }

        // Client secret returned by the gateway (e.g. Stripe pi_xxx_secret_xxx).
        // Frontend uses this with Stripe.js to confirm the payment in the browser.
        // Null for non-Stripe gateways or when gateway is disabled.
        [MaxLength(500)]
        public string ClientSecret { get; set; }
    }

    [Table("Organizations")]
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }

        [Required]
        [MaxLength(150)]
        public string OrganizationName { get; set; }

        [Required]
        [MaxLength(20)]
        public string OrganizationType { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string LogoUrl { get; set; }

        [MaxLength(200)]
        public string WebsiteUrl { get; set; }

        [MaxLength(100)]
        public string ContactEmail { get; set; }

        [MaxLength(20)]
        public string ContactPhone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string RegistrationNumber { get; set; }

        [MaxLength(500)]
        public string Mission { get; set; }

        [MaxLength(500)]
        public string Vision { get; set; }

        public decimal? ContributionAmount { get; set; }

        [MaxLength(50)]
        public string ContributionType { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("Conversations")]
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }

        [MaxLength(50)]
        public string ConversationType { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        public int? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public DateTime? ClosedAt { get; set; }
    }

    [Table("ConversationMessages")]
    public class ConversationMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string MessageText { get; set; }

        public bool IsInternalNote { get; set; } = false;

        public string Attachments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("cms_pages")]
    public class CmsPage
    {
        [Key]
        public int PageId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PageKey { get; set; }

        [MaxLength(100)]
        public string PageSlug { get; set; }

        [Required]
        [MaxLength(100)]
        public string PageTitle { get; set; }

        public string Content { get; set; }

        [MaxLength(255)]
        public string MetaDescription { get; set; }

        [MaxLength(255)]
        public string MetaKeywords { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsInMenu { get; set; } = true;

        public int? ParentPageId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? UpdatedBy { get; set; }
    }

    [Table("Careers")]
    public class Career
    {
        [Key]
        public int CareerId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PositionTitle { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        public string Description { get; set; }

        public string Requirements { get; set; }

        public string Responsibilities { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(50)]
        public string EmploymentType { get; set; }

        [MaxLength(100)]
        public string SalaryRange { get; set; }

        public int Vacancies { get; set; } = 1;

        public DateTime PostedDate { get; set; } = DateTime.Today;

        public DateTime? ClosingDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }
    }

    [Table("CareerApplications")]
    public class CareerApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int CareerId { get; set; }

        [ForeignKey("CareerId")]
        public virtual Career Career { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApplicantName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string ResumeUrl { get; set; }

        public string CoverLetter { get; set; }

        [Column("linkedin_url")]
        [MaxLength(200)]
        public string LinkedInUrl { get; set; }

        [MaxLength(200)]
        public string PortfolioUrl { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Submitted";

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string Notes { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }

    [Table("Gallery")]
    public class Gallery
    {
        [Key]
        public int GalleryId { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(255)]
        public string PhotoUrl { get; set; }

        [MaxLength(255)]
        public string ThumbnailUrl { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        [MaxLength(255)]
        public string Tags { get; set; }

        public int? OrganizationId { get; set; }

        public int? ProgrammeId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public int? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }

    [Table("ContactMessages")]
    public class ContactMessage
    {
        [Key]
        public int ContactId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public int? RepliedBy { get; set; }

        public string ReplyMessage { get; set; }

        public DateTime? RepliedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// User Invite Friends — tracks referral invitations sent by users.
    /// Email is queued/recorded here but actual SMTP send is delegated to a service
    /// (which can be a mock until a real provider is wired in).
    /// </summary>
    [Table("Invitations")]
    public class Invitation
    {
        [Key]
        public int InvitationId { get; set; }

        public int? InviterUserId { get; set; }

        [ForeignKey("InviterUserId")]
        public virtual User Inviter { get; set; }

        [Required]
        [MaxLength(150)]
        public string InviteeName { get; set; }

        [Required]
        [MaxLength(150)]
        public string InviteeEmail { get; set; }

        [MaxLength(500)]
        public string PersonalMessage { get; set; }

        /// <summary>
        /// Pending | Sent | Failed | Registered | Cancelled
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Unique token used in the invitation link.
        /// </summary>
        [MaxLength(64)]
        public string InvitationToken { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? RegisteredAt { get; set; }

        public string FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // SECURITY/AUDIT: UpdatedAt added so Cancel / status mutations can be
        // audited. Previously the controller tried to call a non-existent
        // UpdatedAtSafe() method on this entity, throwing at runtime.
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// About Us module - Our Team page
    /// </summary>
    [Table("team_members")]
    public class TeamMember
    {
        [Key]
        public int TeamMemberId { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(150)]
        public string RoleTitle { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        public string Bio { get; set; }

        [MaxLength(500)]
        public string PhotoUrl { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string LinkedInUrl { get; set; }

        [MaxLength(255)]
        public string TwitterUrl { get; set; }

        [MaxLength(255)]
        public string FacebookUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public DateTime? JoinedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; }
    }

    /// <summary>
    /// About Us module - Our Achievements page
    /// </summary>
    [Table("Achievements")]
    public class Achievement
    {
        [Key]
        public int AchievementId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public string Description { get; set; }

        public decimal? MetricValue { get; set; }

        [MaxLength(100)]
        public string MetricLabel { get; set; }

        [MaxLength(20)]
        public string MetricSuffix { get; set; }

        public DateTime? AchievementDate { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(50)]
        public string Icon { get; set; }

        [MaxLength(150)]
        public string AwardBy { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        public int? Beneficiaries { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; }
    }

    /// <summary>
    /// Help Centre — Frequently Asked Questions
    /// </summary>
    [Table("Faqs")]
    public class Faq
    {
        [Key]
        public int FaqId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; }
    }

    /// <summary>
    /// Webhook event log — written by the payment gateway webhook endpoint.
    /// Useful for debugging gateway delivery issues, auditing events, and
    /// detecting duplicate deliveries (idempotency checks).
    /// </summary>
    [Table("WebhookLogs")]
    public class WebhookLog
    {
        [Key]
        public long WebhookLogId { get; set; }

        /// <summary>Gateway that sent this event: "stripe", "vnpay", "momo", "mock".</summary>
        [Required]
        [MaxLength(20)]
        public string Gateway { get; set; }

        /// <summary>Event type from the gateway, e.g. "payment_intent.succeeded".</summary>
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; }

        /// <summary>Unique event ID from the gateway (for idempotency / deduplication).</summary>
        [MaxLength(100)]
        public string EventId { get; set; }

        /// <summary>
        /// Raw webhook payload, truncated to 4000 chars.
        /// Stored so we can replay / audit events if needed.
        /// </summary>
        [MaxLength(4000)]
        public string RawPayload { get; set; }

        /// <summary>Signature header value received (for debugging failures).</summary>
        [MaxLength(500)]
        public string Signature { get; set; }

        /// <summary>
        /// Whether signature verification passed. A false value means the event
        /// was still processed (with a warning) but the status is untrusted.
        /// </summary>
        public bool SignatureValid { get; set; }

        /// <summary>
        /// Processed | Failed | Ignored | Duplicate
        /// </summary>
        [MaxLength(20)]
        public string ProcessingStatus { get; set; } = "Processed";

        /// <summary>
        /// Human-readable error message when ProcessingStatus = Failed.
        /// </summary>
        [MaxLength(500)]
        public string ErrorMessage { get; set; }

        /// <summary>The donation's internal TransactionId if one was matched.</summary>
        [MaxLength(100)]
        public string DonationTransactionId { get; set; }

        /// <summary>The donation's internal DonationId if one was matched.</summary>
        public int? DonationId { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // EmailLog — persistent log of every outbound email attempt.
    // Provides audibility and retry capability without a background worker.
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records every email dispatch attempt (success or failure) for audit,
    /// retry, and admin visibility.
    /// </summary>
    [Table("EmailLogs")]
    public class EmailLog
    {
        [Key]
        public int EmailLogId { get; set; }

        /// <summary>The RFC 5321 To: address.</summary>
        [Required]
        [MaxLength(254)]
        public string ToEmail { get; set; }

        /// <summary>Email subject line.</summary>
        [Required]
        [MaxLength(500)]
        public string Subject { get; set; }

        /// <summary>Full HTML body that was (or would have been) sent.</summary>
        public string Body { get; set; }

        /// <summary>
        /// Logical category used for filtering and reporting.
        /// Values: invitation | donation_receipt | registration_confirmation |
        /// contact_reply | general.
        /// </summary>
        [MaxLength(50)]
        public string Category { get; set; }

        /// <summary>
        /// ID of the related entity (DonationId, InvitationId, etc.), if any.
        /// </summary>
        public int? RelatedId { get; set; }

        /// <summary>
        /// Sent | Failed | MockSent | PendingRetry.
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>UTC timestamp when the email was successfully dispatched.</summary>
        public DateTime? SentAt { get; set; }

        /// <summary>Error message from the SMTP transport on failure.</summary>
        [MaxLength(2000)]
        public string ErrorMessage { get; set; }

        /// <summary>Number of times the service has attempted to resend this entry.</summary>
        public int RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
