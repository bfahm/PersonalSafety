using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Models;

namespace PersonalSafety.Services.Location
{
    public interface ILocationService
    {
        Department GetNearestDepartment(Location requestLocation, int authorityType);
    }
}
