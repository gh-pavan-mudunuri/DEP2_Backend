using System;
using System.Collections.Generic;
using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enum moved to Enums/UserEnums.cs
    public class User
    {
        public int UserId { get; set; } // PK
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Removed EmailVerified; use IsEmailVerified only
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        // Forgot password fields
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? Phone { get; set; }
        public UserRole  Role { get; set; } = UserRole.User;
        public string? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Stripe Express account ID for payouts
    public string? StripeAccountId { get; set; }

    // Navigation and workflow properties
    public ICollection<Registration>? Registrations { get; set; }
    public ICollection<Ticket>? Tickets { get; set; }
    public ICollection<Notification>? Notifications { get; set; }
    public ICollection<Bookmark>? Bookmarks { get; set; }
    public ICollection<Event>? OrganizedEvents { get; set; }
    }
}
