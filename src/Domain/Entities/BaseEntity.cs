using System;

namespace GiveAID.Domain.Entities
{
    /// <summary>
    /// Abstract base class for all domain entities.
    /// Provides common audit fields. Each concrete entity defines its own
    /// primary-key property (UserId, CauseId, CampaignId, ...) annotated with
    /// [Key] so EF Core can map it to the snake_case column expected by the
    /// canonical SQL schema.
    /// </summary>
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft-delete flag. When true, the record is excluded from normal
        /// queries via a global query filter but remains in the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// UTC timestamp when the record was soft-deleted. Null while active.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User ID of the user who created this record.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// User ID of the user who last updated this record.
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
