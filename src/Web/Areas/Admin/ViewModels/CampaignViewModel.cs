using System.ComponentModel.DataAnnotations;

namespace GiveAID.Web.Areas.Admin.ViewModels;

public class CampaignViewModel
{
    public int CampaignId { get; set; }

    [Required(ErrorMessage = "Campaign name is required")]
    [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    public int? CauseId { get; set; }
    public string? CauseName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Goal must be a positive number")]
    public decimal GoalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Raised amount must be positive")]
    public decimal RaisedAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    public int? MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CampaignListItem
{
    public int CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CauseName { get; set; }
    public decimal GoalAmount { get; set; }
    public decimal RaisedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CauseViewModel
{
    public int CauseId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int? ParentCauseId { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int CampaignCount { get; set; }
}

public class CauseListItem
{
    public int CauseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ParentName { get; set; }
    public int CampaignCount { get; set; }
    public bool IsActive { get; set; }
}
