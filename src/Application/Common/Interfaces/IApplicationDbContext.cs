using Microsoft.EntityFrameworkCore;
using GiveAID.Domain.Entities;

namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for the application's database context.
/// This interface is implemented by Infrastructure layer to provide access to all DbSets.
/// Application layer depends only on this interface - never on concrete DbContext.
/// </summary>
public interface IApplicationDbContext
{
    // User & Authentication
    DbSet<User> Users { get; }

    // Causes & Campaigns
    DbSet<Cause> Causes { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<CampaignRegistration> CampaignRegistrations { get; }
    DbSet<CampaignReport> CampaignReports { get; }

    // Donations
    DbSet<Donation> Donations { get; }

    // Organizations (Partners, NGOs, Supporters)
    DbSet<Organization> Organizations { get; }

    // Conversations & Support
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }

    // CMS & Content
    DbSet<CmsPage> CmsPages { get; }

    // Careers
    DbSet<Career> Careers { get; }
    DbSet<CareerApplication> CareerApplications { get; }

    // Gallery
    DbSet<Gallery> Gallery { get; }

    // Contact
    DbSet<ContactMessage> ContactMessages { get; }

    // Invitations
    DbSet<Invitation> Invitations { get; }

    // About Us
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<Achievement> Achievements { get; }

    // Help Centre
    DbSet<Faq> Faqs { get; }

    // Email & Webhooks
    DbSet<EmailLog> EmailLogs { get; }
    DbSet<WebhookLog> WebhookLogs { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }

    // Programmes
    DbSet<Programme> Programmes { get; }
    DbSet<ProgrammeRegistration> ProgrammeRegistrations { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
