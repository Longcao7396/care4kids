using System.ComponentModel.DataAnnotations;

namespace GiveAID.Web.Areas.Admin.ViewModels;

/// <summary>
/// Admin login view-model. Users sign in with their username only
/// (email is no longer accepted as a login identifier).
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 64 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}
