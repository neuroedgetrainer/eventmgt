using EventMgt.Helpers;

namespace EventMgt.DatabaseContext.DbEntities;

public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }

    public DateTime RegistrationDate { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime? CancelledDate { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}
