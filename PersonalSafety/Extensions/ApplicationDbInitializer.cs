using Microsoft.AspNetCore.Identity;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PersonalSafety.Contracts;
using System.Security.Claims;
using PersonalSafety.Options;
using System.IO;

namespace PersonalSafety.Extensions
{
    public class ApplicationDbInitializer : IApplicationDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppSettings _appSettings;
        private readonly IClientRepository _clientRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDistributionRepository _distributionRepository;
        private readonly IEventCategoryRepository _categoryRepository;

        public ApplicationDbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppSettings appSettings, IClientRepository clientRepository, IPersonnelRepository personnelRepository, IDepartmentRepository departmentRepository, IDistributionRepository distributionRepository, IEventCategoryRepository categoryRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _appSettings = appSettings;
            _clientRepository = clientRepository;
            _personnelRepository = personnelRepository;
            _departmentRepository = departmentRepository;
            _distributionRepository = distributionRepository;
            _categoryRepository = categoryRepository;
        }

        public void SeedData()
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

            #region Create Manager

            ApplicationUser managerUser = new ApplicationUser
            {
                UserName = "manager@test.com",
                Email = "manager@test.com",
                FullName = "Manager",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(managerUser, "Test@123", "Manager");
            if (_userManager.GetClaimsAsync(_userManager.FindByEmailAsync("manager@test.com").Result).Result.Count == 0)
            {
                _userManager.AddClaimAsync(managerUser, new Claim(ClaimsStore.CLAIM_DISTRIBUTION_ACCESS, "1")).Wait(); // Admin can access all distributions
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

            CreateCategories();
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
                var cities = _distributionRepository.GetCities().Take(3);

                List<Department> departments = new List<Department>()
                {
                    #region Tanta Departments
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Tanta").Id,
                            Latitude = 30.785576,
                            Longitude = 30.997374,
                            AuthorityType = (int)AuthorityTypesEnum.Police
                        } 
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Tanta").Id,
                            Latitude = 30.796410,
                            Longitude = 31.010563,
                            AuthorityType = (int)AuthorityTypesEnum.TowTruck
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Tanta").Id,
                            Latitude = 30.785660,
                            Longitude = 30.989420,
                            AuthorityType = (int)AuthorityTypesEnum.Firefighting
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Tanta").Id,
                            Latitude = 30.807549,
                            Longitude = 30.998694,
                            AuthorityType = (int)AuthorityTypesEnum.Ambulance
                        }
                    },
                    #endregion
                    
                    #region Cairo Departments
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Cairo").Id,
                            Latitude = 30.055563,
                            Longitude = 31.219640,
                            AuthorityType = (int)AuthorityTypesEnum.Police
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Cairo").Id,
                            Latitude = 29.992153,
                            Longitude = 31.310135,
                            AuthorityType = (int)AuthorityTypesEnum.TowTruck
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Cairo").Id,
                            Latitude = 30.033687,
                            Longitude = 31.201933,
                            AuthorityType = (int)AuthorityTypesEnum.Firefighting
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Cairo").Id,
                            Latitude = 30.053779,
                            Longitude = 31.238588,
                            AuthorityType = (int)AuthorityTypesEnum.Ambulance
                        }
                    },
                    #endregion

                    #region Alexandria Departments
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Alexandria").Id,
                            Latitude = 31.268075,
                            Longitude = 29.988351,
                            AuthorityType = (int)AuthorityTypesEnum.Police
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Alexandria").Id,
                            Latitude = 31.261083,
                            Longitude = 29.989100,
                            AuthorityType = (int)AuthorityTypesEnum.TowTruck
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Alexandria").Id,
                            Latitude = 31.210805,
                            Longitude = 29.916305,
                            AuthorityType = (int)AuthorityTypesEnum.Firefighting
                        }
                    },
                    {
                        new Department{
                            DistributionId = cities.First(c => c.Value == "Alexandria").Id,
                            Latitude = 31.213648,
                            Longitude = 29.950524,
                            AuthorityType = (int)AuthorityTypesEnum.Ambulance
                        }
                    },
                    #endregion
                };

                
                foreach (var department in departments)
                {
                    _departmentRepository.Add(department);
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

        private void CreateCategories()
        {
            if(_categoryRepository.GetAll().Count() == 0)
            {
                EventCategory coronaCategory = new EventCategory
                {
                    Title = "Corona Virus",
                    Description = "Events related to the novel pandemic",
                    ThumbnailUrl = Path.Combine(_appSettings.AttachmentsLocation, "cat_corona.jpg")
                };

                _categoryRepository.Add(coronaCategory);

                EventCategory nearbyCategory = new EventCategory
                {
                    Title = "Nearby Stories",
                    Description = "Events that reside in the neighborhood",
                    ThumbnailUrl = Path.Combine(_appSettings.AttachmentsLocation, "cat_nearby.jpg")
                };

                _categoryRepository.Add(nearbyCategory);

                EventCategory userStories = new EventCategory
                {
                    Title = "Your Stories",
                    Description = "Your Events..",
                    ThumbnailUrl = Path.Combine(_appSettings.AttachmentsLocation, "cat_your.png")
                };

                _categoryRepository.Add(userStories);

                _categoryRepository.Save();
            }
        }
    }
}
