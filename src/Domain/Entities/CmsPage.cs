using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

[Table("cms_pages")]
public class CmsPage : BaseEntity
{
    [Key]
    public int PageId { get; set; }

    [Required]
    [MaxLength(50)]
    public string PageKey { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PageSlug { get; set; }

    [Required]
    [MaxLength(100)]
    public string PageTitle { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(255)]
    public string? MetaDescription { get; set; }

    [MaxLength(255)]
    public string? MetaKeywords { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsInMenu { get; set; } = true;
    public int? ParentPageId { get; set; }
    public int DisplayOrder { get; set; }
    public int? UpdatedBy { get; set; }
}
