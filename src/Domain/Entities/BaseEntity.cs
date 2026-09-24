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
    }
}
