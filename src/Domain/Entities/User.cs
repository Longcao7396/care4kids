using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class User : BaseEntity
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Profession { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User";

    public string? Permissions { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    [MaxLength(100)]
    public string? VerificationToken { get; set; }

    public DateTime? LastLogin { get; set; }
    public DateTime? PasswordChangedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public virtual ICollection<CampaignRegistration> CampaignRegistrations { get; set; } = new List<CampaignRegistration>();
    public virtual ICollection<ProgrammeRegistration> ProgrammeRegistrations { get; set; } = new List<ProgrammeRegistration>();
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public virtual ICollection<CareerApplication> CareerApplications { get; set; } = new List<CareerApplication>();
    public virtual ICollection<Invitation> SentInvitations { get; set; } = new List<Invitation>();
    public virtual ICollection<TeamMember> CreatedTeamMembers { get; set; } = new List<TeamMember>();
    public virtual ICollection<Achievement> CreatedAchievements { get; set; } = new List<Achievement>();
    public virtual ICollection<Faq> CreatedFaqs { get; set; } = new List<Faq>();
}
