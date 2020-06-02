using PersonalSafety.Models;

namespace PersonalSafety.Services.Location
{
    public interface ILocationService
    {
        Department GetNearestDepartment(Location requestLocation, int authorityType);
        Distribution GetNearestCity(Location eventLocation);
        double CalculateDistance(Location p1, Location p2);
    }
}
