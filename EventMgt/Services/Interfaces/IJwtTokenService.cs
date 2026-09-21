using EventMgt.DatabaseContext.DbEntities;

namespace EventMgt.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        DateTime GetExpirationDate();
    }
}
