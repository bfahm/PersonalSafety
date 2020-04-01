using Microsoft.AspNetCore.Identity;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;

namespace PersonalSafety.Extensions
{
    public class ApplicationDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public ApplicationDbInitializer(UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IPersonnelRepository personnelRepository, IDepartmentRepository departmentRepository)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _personnelRepository = personnelRepository;
            _departmentRepository = departmentRepository;
        }

        public void SeedUsers()
        {
            #region Create Admin

            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                FullName = "Administrator",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(adminUser, "Admin@123", "Admin");

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
            
            Client client = new Client
            {
                ClientId = testUser.Id,
                NationalId = "00000000000000"
            };
            
            if (!_clientRepository.GetAll().Any())
            {
                _clientRepository.Add(client);
                _clientRepository.Save();
            }

            #endregion

            #region Create Police Department

            Department policeDpt = new Department
            {
                AuthorityType = (int) AuthorityTypesEnum.Police,
                Longitude = 0.0,
                Latitude = 0.0
            };

            if (!_departmentRepository.GetAll().Any())
            {
                _departmentRepository.Add(policeDpt);
                _departmentRepository.Save();
            }

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

            Personnel policeAgent = new Personnel
            {
                PersonnelId = policeAgentUser.Id,
                Department = policeDpt,
                IsRescuer = false
            };

            if (!_personnelRepository.GetAll().Where(p => p.IsRescuer == false).Any())
            {
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

            Personnel rescuer1 = new Personnel
            {
                PersonnelId = rescuer1User.Id,
                Department = policeDpt,
                IsRescuer = true
            };

            if (!_personnelRepository.GetAll().Where(p => p.IsRescuer == true).Any())
            {
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
    }
}
