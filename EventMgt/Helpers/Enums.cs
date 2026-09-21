namespace EventMgt.Helpers
{
    public enum EventMode : byte
    {
        InPerson = 1,
        Online = 2,
        Hybrid = 3
    }
    public enum EventStatus
    {
        Draft,
        Registered,
        Published,
        Completed,
        Scheduled,
        Cancelled
    }
    public enum RegistrationStatus
    {
        Registered,
        Cancelled
    }
}
