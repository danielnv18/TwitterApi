using TwitterCloneApi.Domain.Common;

namespace TwitterCloneApi.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Guid? ProfileImageId { get; set; }
    public Guid? BackgroundImageId { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    // Validation
    public void ValidateUsername()
    {
        if (string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("Username cannot be empty");

        if (Username.Length < 3 || Username.Length > 100)
            throw new ArgumentException("Username must be between 3 and 100 characters");

        if (!System.Text.RegularExpressions.Regex.IsMatch(Username, "^[a-zA-Z0-9_]+$"))
            throw new ArgumentException("Username can only contain letters, numbers, and underscores");
    }

    public void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new ArgumentException("Email cannot be empty");

        if (!System.Text.RegularExpressions.Regex.IsMatch(Email, 
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format");
    }

    public void ValidateBio()
    {
        if (Bio != null && Bio.Length > 500)
            throw new ArgumentException("Bio cannot exceed 500 characters");
    }
}
