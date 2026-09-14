using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GiveAID.Web.Models
{
    // =====================================================
    // VIEW MODELS FOR API REQUESTS/RESPONSES
    // =====================================================

    // AUTH VIEW MODELS
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Phone]
        public string Phone { get; set; }

        public string Address { get; set; }

        public string Profession { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Profession { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    // CAUSE VIEW MODELS
    public class CauseDto
    {
        public int CauseId { get; set; }
        public string CauseName { get; set; }
        public string CauseCode { get; set; }
        public string Description { get; set; }
        public decimal? TargetAmount { get; set; }
        public decimal RaisedAmount { get; set; }
        public decimal PercentageReached { get; set; }
        public string ImageUrl { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
        public int TotalDonations { get; set; }
    }

    public class CreateCauseViewModel
    {
        [Required]
        [StringLength(100)]
        public string CauseName { get; set; }

        // Optional. Auto-generated from CauseName (uppercased, slugified)
        // server-side when blank. Frontend no longer needs to compute a code.
        [StringLength(20)]
        public string CauseCode { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public decimal? TargetAmount { get; set; }

        public string ImageUrl { get; set; }

        public string Icon { get; set; }

        public int DisplayOrder { get; set; }
    }

    // DONATION VIEW MODELS
    public class CreateDonationViewModel
    {
        [Required]
        public int CauseId { get; set; }

        public int? OrganizationId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } // CreditCard, DebitCard, NetBanking

        // Card details (for validation only, not stored directly)
        [Required]
        [CreditCard]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Invalid expiry format (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Invalid CVV")]
        public string CVV { get; set; }

        public bool IsAnonymous { get; set; }

        [StringLength(500)]
        public string Message { get; set; }
    }

    public class DonationDto
    {
        public int DonationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CauseId { get; set; }
        public string CauseName { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public string CardLastFour { get; set; }
        public string CardType { get; set; }
        public bool IsAnonymous { get; set; }
        public string Message { get; set; }
        public bool ReceiptSent { get; set; }
    }

    // PROGRAMME VIEW MODELS
    public class ProgrammeDto
    {
        public int ProgrammeId { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Title { get; set; }
        public string ProgrammeType { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
        public int? TargetBeneficiaries { get; set; }
        public decimal? ExpectedBudget { get; set; }
        public decimal? ActualBudget { get; set; }
        public string Status { get; set; }
        public string ImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public bool RegistrationRequired { get; set; }
        public int? MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public bool IsRegistrationOpen { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProgrammeViewModel
    {
        public int? OrganizationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string ProgrammeType { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(255)]
        public string Location { get; set; }

        public int? TargetBeneficiaries { get; set; }

        public decimal? ExpectedBudget { get; set; }

        public string ImageUrl { get; set; }

        public bool RegistrationRequired { get; set; } = true;

        public int? MaxParticipants { get; set; }

        public bool IsFeatured { get; set; }
    }

    public class RegisterForProgrammeViewModel
    {
        [Required]
        public int ProgrammeId { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    public class ProgrammeRegistrationDto
    {
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int ProgrammeId { get; set; }
        public string ProgrammeTitle { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool AttendanceConfirmed { get; set; }
    }

    // ORGANIZATION VIEW MODELS
    public class OrganizationDto
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationType { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public bool IsFeatured { get; set; }
        public decimal? ContributionAmount { get; set; }
    }

    // STATISTICS VIEW MODELS
    public class DashboardStatsDto
    {
        public decimal TotalDonations { get; set; }
        public int TotalDonors { get; set; }
        public int TotalProgrammes { get; set; }
        public int ActiveCauses { get; set; }
        public int TotalBeneficiaries { get; set; }
        public int RegisteredUsers { get; set; }
    }

    public class CauseStatsDto
    {
        public int CauseId { get; set; }
        public string CauseName { get; set; }
        public decimal TotalRaised { get; set; }
        public int DonationCount { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal PercentageReached { get; set; }
    }

    // RESPONSE WRAPPERS
    public class ApiResponse : ApiResponse<object> { }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, object errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
