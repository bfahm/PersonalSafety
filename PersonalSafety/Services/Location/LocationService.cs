using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Models;

namespace PersonalSafety.Services.Location
{
    public class LocationService : ILocationService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public LocationService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public Department GetNearestDepartment(Location requestLocation, int authorityType)
        {
            var listOfDepartments = _departmentRepository.GetDepartmentsByAuthority(authorityType);
            var dictOfDepartmentDistances = new Dictionary<int, double>();

            foreach (var department in listOfDepartments)
            {
                var dptLocation = new Location(department.Longitude, department.Latitude);
                var distance = CalculateDistance(requestLocation, dptLocation);
                dictOfDepartmentDistances.Add(department.Id, distance);
            }

            var nearestDepartmentEntry = dictOfDepartmentDistances.OrderBy(d => d.Value).FirstOrDefault();

            return _departmentRepository.GetById(nearestDepartmentEntry.Key.ToString());
        }

        private double CalculateDistance(Location p1, Location p2)
        {
            var R = 6378137; // Earth’s mean radius in meter
            var dLat = ToRadian(p2.Latitude - p1.Latitude);
            var dLong = ToRadian(p2.Longitude - p1.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadian(p1.Latitude)) * Math.Cos(ToRadian(p2.Latitude)) *
                    Math.Sin(dLong / 2) * Math.Sin(dLong / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadian(double input)
        {
            return input * Math.PI / 180;
        }
    }

    public class Location
    {
        public double Longitude { get; }
        public double Latitude { get; }

        public Location(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
