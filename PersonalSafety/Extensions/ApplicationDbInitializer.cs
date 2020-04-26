using Microsoft.AspNetCore.Identity;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PersonalSafety.Contracts;
using System.Security.Claims;

namespace PersonalSafety.Extensions
{
    public class ApplicationDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IClientRepository _clientRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDistributionRepository _distributionRepository;

        public ApplicationDbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IClientRepository clientRepository, IPersonnelRepository personnelRepository, IDepartmentRepository departmentRepository, IDistributionRepository distributionRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _clientRepository = clientRepository;
            _personnelRepository = personnelRepository;
            _departmentRepository = departmentRepository;
            _distributionRepository = distributionRepository;
        }

        public void SeedUsers()
        {
            CreateRoles();

            CreateDistributions();

            #region Create Admin

            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                FullName = "Administrator",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(adminUser, "Admin@123", "Admin");
            if (_userManager.GetClaimsAsync(_userManager.FindByEmailAsync("admin@admin.com").Result).Result.Count == 0)
            {
                _userManager.AddClaimAsync(adminUser, new Claim(ClaimsStore.CLAIM_DISTRIBUTION_ACCESS, "1")).Wait(); // Admin can access all distributions
            }

            #endregion

            #region Create General User

            ApplicationUser testUser = new ApplicationUser
            {
                UserName = "test@test.com",
                Email = "test@test.com",
                FullName = "Test User",
                EmailConfirmed = true,
                PhoneNumber = "00000000000"
            };

            CreateUserAndSetupRole(testUser, "Test@123", null);
            
            if (!_clientRepository.GetAll().Any())
            {
                Client client = new Client
                {
                    ClientId = testUser.Id,
                    NationalId = "00000000000000"
                };

                _clientRepository.Add(client);
                _clientRepository.Save();
            }

            #endregion

            #region Create Departments

            CreateDepartments();
            var tantaCity = _distributionRepository.GetCityByName("Tanta");
            var tantaPoliceDepartment = _departmentRepository.GetAll().FirstOrDefault(d =>
                d.AuthorityType == (int) AuthorityTypesEnum.Police && d.DistributionId == tantaCity.Id);
            Debug.Assert(tantaPoliceDepartment != null, nameof(tantaPoliceDepartment) + " != null");

            #endregion

            #region Create Police Agent

            ApplicationUser policeAgentUser = new ApplicationUser
            {
                UserName = "agent@test.com",
                Email = "agent@test.com",
                FullName = "Police Agent",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(policeAgentUser, "Test@123", Roles.ROLE_PERSONNEL, Roles.ROLE_AGENT);

            if (_personnelRepository.GetAll().All(p => p.IsRescuer))
            {
                Personnel policeAgent = new Personnel
                {
                    PersonnelId = policeAgentUser.Id,
                    DepartmentId = tantaPoliceDepartment.Id,
                    IsRescuer = false
                };

                _personnelRepository.Add(policeAgent);
                _personnelRepository.Save();
            }

            #endregion

            #region Create Police Agent

            ApplicationUser rescuer1User = new ApplicationUser
            {
                UserName = "res1@test.com",
                Email = "res1@test.com",
                FullName = "Police Rescuer",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(rescuer1User, "Test@123", Roles.ROLE_PERSONNEL, Roles.ROLE_RESCUER);

            
            if (_personnelRepository.GetAll().All(p => !p.IsRescuer))
            {
                Personnel rescuer1 = new Personnel
                {
                    PersonnelId = rescuer1User.Id,
                    DepartmentId = tantaPoliceDepartment.Id,
                    IsRescuer = true
                };

                _personnelRepository.Add(rescuer1);
                _personnelRepository.Save();
            }

            #endregion
        }

        private void CreateUserAndSetupRole(ApplicationUser user, string password, params string[] roles)
        {
            if (_userManager.FindByEmailAsync(user.Email).Result == null)
            {
                IdentityResult result = _userManager.CreateAsync(user, password).Result;

                if (result.Succeeded && roles != null && roles.Length>0)
                {
                    foreach (var role in roles)
                    {
                        _userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }

        }

        private void CreateRoles()
        {
            if (!_roleManager.Roles.Any())
            {
                foreach (var role in Roles.GetRoles())
                {
                    IdentityRole identityRole = new IdentityRole(role);
                    var identityResult = _roleManager.CreateAsync(identityRole).Result;
                }
            }
        }

        private void CreateDepartments()
        {
            if (!_departmentRepository.GetAll().Any())
            {
                List<double> locations = new List<double>()
                {
                    {30.785576},                // Tanta        Police      Long
                    {30.997374},                // Tanta        Police      Lat
                    {30.055563},                // Cairo        Police      Long
                    {31.219640},                // Cairo        Police      Lat
                    {31.242536},                // Alexandria   Police      Long
                    {31.804148},                // Alexandria   Police      Lat
                    {30.785576},                // Tanta        Tow (Same as Police)    Long
                    {30.997374},                // Tanta        Tow (Same as Police)    Lat
                    {30.055563},                // Cairo        Tow (Same as Police)    Long
                    {31.219640 },               // Cairo        Tow (Same as Police)    Lat
                    {31.242536},                // Alexandria   Tow (Same as Police)    Long
                    {31.804148 },               // Alexandria   Tow (Same as Police)    Lat
                    {30.785660},                // Tanta        FireFighting            Long
                    {30.989420 },               // Tanta        FireFighting            Lat
                    {30.033687},                // Cairo        FireFighting            Long
                    {31.201933 },               // Cairo        FireFighting            Lat
                    {31.210805},                // Alexandria   FireFighting            Long
                    {29.916305 },               // Alexandria   FireFighting            Lat
                    {30.807549},                // Tanta        Ambulance            Long
                    {30.998694 },               // Tanta        Ambulance            Lat
                    {30.053779},                // Cairo        Ambulance            Long
                    {31.238588 },               // Cairo        Ambulance            Lat
                    {31.213648},                // Alexandria   Ambulance            Long
                    {29.950524 }                // Alexandria   Ambulance            Lat
                };

                var cities = _distributionRepository.GetCities().Take(3);

                int counter = 0;
                foreach (var city in cities)
                {
                    foreach (var authorityType in Enum.GetValues(typeof(AuthorityTypesEnum)))
                    {
                        _departmentRepository.Add(new Department()
                        {
                            DistributionId = city.Id,
                            AuthorityType = (int)authorityType,
                            Longitude = locations[counter],
                            Latitude = locations[counter+1],
                        });

                        counter += 2;
                    }
                }

                _departmentRepository.Save();
            }
        }

        private void CreateDistributions()
        {
            if (!_distributionRepository.GetAll().Any())
            {
                List<Distribution> distributions = new List<Distribution>
                {
                    new Distribution { Id = 1, Type = (int)DistributionTypesEnum.Country, Value = "Egypt", ParentId = null },
                    new Distribution { Id = 2, Type = (int)DistributionTypesEnum.Region, Value = "LowerEgypt", ParentId = 1 },
                    new Distribution { Id = 3, Type = (int)DistributionTypesEnum.Region, Value = "UpperEgypt", ParentId = 1 },
                    new Distribution { Id = 4, Type = (int)DistributionTypesEnum.Province, Value = "Gharbia", ParentId = 2 },
                    new Distribution { Id = 5, Type = (int)DistributionTypesEnum.Province, Value = "Cairo", ParentId = 2 },
                    new Distribution { Id = 6, Type = (int)DistributionTypesEnum.Province, Value = "Alexandria", ParentId = 2 },
                    new Distribution { Id = 7, Type = (int)DistributionTypesEnum.Province, Value = "Aswan", ParentId = 3 },
                    new Distribution { Id = 8, Type = (int)DistributionTypesEnum.City, Value = "Tanta", ParentId = 4 },
                    new Distribution { Id = 9, Type = (int)DistributionTypesEnum.City, Value = "Cairo", ParentId = 5 },
                    new Distribution { Id = 10, Type = (int)DistributionTypesEnum.City, Value = "Alexandria", ParentId = 6 },
                    new Distribution { Id = 11, Type = (int)DistributionTypesEnum.City, Value = "Mahala", ParentId = 4 },
                    new Distribution { Id = 12, Type = (int)DistributionTypesEnum.City, Value = "Aswan", ParentId = 7 },
                    new Distribution { Id = 13, Type = (int)DistributionTypesEnum.City, Value = "Idfo", ParentId = 7 },
                };

                _distributionRepository.AddWithIdentityInsert(distributions);
            }
        }
    }
}
