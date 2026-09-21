namespace EventMgt.DatabaseContext.DbEntities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
