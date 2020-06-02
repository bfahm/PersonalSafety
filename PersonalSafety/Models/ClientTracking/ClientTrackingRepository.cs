using Microsoft.Extensions.Logging;
using PersonalSafety.Services.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class ClientTrackingRepository : BaseRepository<ClientTracking>, IClientTrackingRepository
    {
        private readonly AppDbContext context;
        private readonly ILocationService _locationService;
        private readonly ILogger<ClientTrackingRepository> _logger;
        
        private int minutesSkew = 5;
        private int metersSkew = 10;

        public ClientTrackingRepository(AppDbContext context, ILocationService locationService, ILogger<ClientTrackingRepository> logger) : base(context)
        {
            this.context = context;
            _locationService = locationService;
            _logger = logger;
        }


        /// <param name="victimUserId">The userId of the client that was marked as Corona Positive</param>
        /// <returns>The userIds that where in range of the victim in similar times, to be marked as susceptible</returns>
        public List<string> GetSusceptibleAccounts(string victimUserId)
        {
            var test = Get2WeeksRecords(victimUserId);
            return GetSusceptibleRecords(test);
        }

        private List<ClientTracking> Get2WeeksRecords(string userId)
        {
            var twoWeekAgo = DateTime.Now.AddDays(-14);
            return context.ClientTrackings.Where(ct => ct.ClientId == userId && ct.Time > twoWeekAgo && ct.Time < DateTime.Now).ToList();
        }

        private List<string> GetSusceptibleRecords(List<ClientTracking> victimTracking)
        {
            var victimIds = victimTracking.Select(vt => vt.ClientId).Distinct();
            
            var susceptibleRecords = context.ClientTrackings.Where(ct => !victimIds.Contains(ct.ClientId));

            var susceptibleRecordsJoin = (from victim in victimTracking
                                                   from susceptible in susceptibleRecords
                                                       select new
                                                       {
                                                           victimTime = victim.Time,
                                                           susceptibleTime = susceptible.Time,

                                                           victimLocation = new Location(victim.Longitude, victim.Latitude),
                                                           susceptibleLocation = new Location(susceptible.Longitude, susceptible.Latitude),

                                                           susceptibleUserId = susceptible.ClientId
                                                       }).ToList();

            // Only select Records that happened within +-5 minutes of the victim's record
            var susceptibleRecordsFilter = susceptibleRecordsJoin.Where(sj => sj.susceptibleTime > sj.victimTime.AddMinutes(-minutesSkew) &&
                                                    sj.susceptibleTime < sj.victimTime.AddMinutes(minutesSkew)).ToList();

            // Records outside the range of 10 meters are removed.
            foreach (var record in susceptibleRecordsJoin)
            {
                var distance = _locationService.CalculateDistance(record.victimLocation, record.susceptibleLocation);
                if (distance > metersSkew)
                    susceptibleRecordsFilter.Remove(record);
            }

            return susceptibleRecordsFilter.Select(snbrj => snbrj.susceptibleUserId).Distinct().ToList();
        }

        
        // Adminstrative

        public void SetMinutesSkew(int newValue)
        {
            minutesSkew = newValue;
            _logger.LogInformation($"Minutes Skew set to {minutesSkew}");
        }

        public int GetMinutesSkew()
        {
            return minutesSkew;
        }

        public void SetMetersSkew(int newValue)
        {
            metersSkew = newValue;
            _logger.LogInformation($"Meters Skew set to {minutesSkew}");
        }

        public int GetMetersSkew()
        {
            return metersSkew;
        }
    }
}
