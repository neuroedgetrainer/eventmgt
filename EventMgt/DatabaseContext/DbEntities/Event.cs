using EventMgt.Helpers;

namespace EventMgt.DatabaseContext.DbEntities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int CategoryId { get; set; }
    public int? VenueId { get; set; }
    public int OrganizerId { get; set; }

    public int MaxParticipants { get; set; }

    public EventStatus Status { get; set; }
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public string? EventImageUrl { get; set; }

    public EventMode EventMode { get; set; }
    public string? OnlineMeetingUrl { get; set; }

    public bool IsActive { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Category Category { get; set; } = null!;
    public Venue? Venue { get; set; }
    public User Organizer { get; set; } = null!;

    public ICollection<EventRegistration> Registrations { get; set; } =
        new List<EventRegistration>();
}
