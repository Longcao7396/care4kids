using System;
using System.Collections.Generic;

namespace GiveAID.Web.Models
{
    /// <summary>
    /// Wire-format DTO for /api/campaigns.
    /// Used so consumers can access fields by name (instead of going through
    /// 'object' or 'dynamic' which the Web API serializer loses on the way out).
    /// </summary>
    public class CampaignDto
    {
        public int campaignId { get; set; }
        public int? causeId { get; set; }
        public CampaignCauseDto cause { get; set; }
        public string campaignName { get; set; }
        public string campaignCode { get; set; }
        public string description { get; set; }
        public decimal goalAmount { get; set; }
        public decimal raisedAmount { get; set; }
        public decimal percentageReached { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public int? daysRemaining { get; set; }
        public string imageUrl { get; set; }
        public int? beneficiariesCount { get; set; }
        public string location { get; set; }
        public string status { get; set; }
        public bool isFeatured { get; set; }
        public int displayOrder { get; set; }
        public int donorCount { get; set; }
        public DateTime? createdAt { get; set; }

        // Merged Programme fields
        public string programmeType { get; set; }
        public bool registrationRequired { get; set; }
        public int? maxParticipants { get; set; }
        public int? currentParticipants { get; set; }
        public int? targetBeneficiaries { get; set; }
        public decimal? expectedBudget { get; set; }
        public decimal? actualBudget { get; set; }
        public int? organizationId { get; set; }
        public string organizationName { get; set; }

        // Convenience discriminator for frontend
        public string campaignKind { get; set; }

        // Detail-only fields (populated on /api/campaigns/{id})
        public int? recentDonationCount { get; set; }
        public List<CampaignDonationItem> recentDonations { get; set; }
        public List<CampaignDonationBreakdown> donationBreakdown { get; set; }
    }

    public class CampaignCauseDto
    {
        public int causeId { get; set; }
        public string causeName { get; set; }
        public string causeCode { get; set; }
        public string icon { get; set; }
    }

    public class CampaignDonationItem
    {
        public string fullName { get; set; }
        public decimal amount { get; set; }
        public string message { get; set; }
        public DateTime donationDate { get; set; }
    }

    public class CampaignDonationBreakdown
    {
        public string range { get; set; }
        public int count { get; set; }
        public decimal total { get; set; }
    }
}
