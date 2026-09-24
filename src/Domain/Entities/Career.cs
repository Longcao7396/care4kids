using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Career : BaseEntity
{
    [Key]
    public int CareerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string PositionTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? EmploymentType { get; set; }

    [MaxLength(100)]
    public string? SalaryRange { get; set; }

    public int Vacancies { get; set; } = 1;
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosingDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<CareerApplication> Applications { get; set; } = new List<CareerApplication>();
}
