// Models/ContactMessage.cs
using System.ComponentModel.DataAnnotations;

namespace Techninxa.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = default!;

    [Required]
    public ProjectType ProjectType { get; set; }

    [Required, MaxLength(2000)]
    public string Message { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ProjectType
{
    WebDevelopment,
    MobileApp,
    CustomSoftware,
    IoTEmbedded,
    Other
}