namespace EventMgt.DatabaseContext.DbEntities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Role Role { get; set; } = null!;

    // User -> Events through Events.OrganizerId
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();

    // User -> EventRegistrations
    public ICollection<EventRegistration> EventRegistrations { get; set; } =
        new List<EventRegistration>();
}
