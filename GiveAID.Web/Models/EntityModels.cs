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

        public DateTime? LastLogin { get; set; }

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
                    return (RaisedAmount / GoalAmount) * 100;
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
                    return days >= 0 ? days : (int?)null;
                }
                return null;
            }
        }

        [NotMapped]
        public int DonorCount { get; set; }

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
    }

    [Table("Programmes")]
    public class Programme
    {
        [Key]
        public int ProgrammeId { get; set; }

        public int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProgrammeType { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        public int? TargetBeneficiaries { get; set; }

        public decimal? ExpectedBudget { get; set; }

        public decimal? ActualBudget { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Upcoming";

        public bool IsFeatured { get; set; } = false;

        public bool RegistrationRequired { get; set; } = true;

        public int? MaxParticipants { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("ProgrammeRegistrations")]
    public class ProgrammeRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required]
        public int ProgrammeId { get; set; }

        [ForeignKey("ProgrammeId")]
        public virtual Programme Programme { get; set; }

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

    [Table("ProgrammePhotos")]
    public class ProgrammePhoto
    {
        [Key]
        public int PhotoId { get; set; }

        [Required]
        public int ProgrammeId { get; set; }

        [ForeignKey("ProgrammeId")]
        public virtual Programme Programme { get; set; }

        [Required]
        [MaxLength(255)]
        public string PhotoUrl { get; set; }

        [MaxLength(200)]
        public string Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
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

    [Table("CmsPages")]
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

        public int? ProgrammeId { get; set; }

        public int? OrganizationId { get; set; }

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
    }

    /// <summary>
    /// About Us module - Our Team page
    /// </summary>
    [Table("TeamMembers")]
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
}
