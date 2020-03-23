using Microsoft.AspNetCore.Identity;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Extensions
{
    public class ApplicationDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IPersonnelRepository _personnelRepository;

        public ApplicationDbInitializer(UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IPersonnelRepository personnelRepository)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _personnelRepository = personnelRepository;
        }

        public void SeedUsers()
        {
            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                FullName = "Adminstrator",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(adminUser, "Admin@123", "Admin");

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
            
            if (_clientRepository.GetAll().Count() == 0)
            {
                _clientRepository.Add(client);
                _clientRepository.Save();
            }
            
            ApplicationUser personnelUser = new ApplicationUser
            {
                UserName = "personnel@personnel.com",
                Email = "personnel@personnel.com",
                FullName = "Personnel Police",
                EmailConfirmed = true
            };

            CreateUserAndSetupRole(personnelUser, "Test@123", "Personnel");
            Personnel personnel = new Personnel
            {
                PersonnelId = personnelUser.Id,
                AuthorityType = (int)AuthorityTypesEnum.Police
            };

            if (_personnelRepository.GetAll().Count() == 0)
            {
                _personnelRepository.Add(personnel);
                _personnelRepository.Save();
            }
        }

        private void CreateUserAndSetupRole(ApplicationUser user, string password, string role)
        {
            if (_userManager.FindByEmailAsync(user.Email).Result == null)
            {
                IdentityResult result = _userManager.CreateAsync(user, password).Result;

                if (result.Succeeded && !string.IsNullOrEmpty(role))
                {
                    _userManager.AddToRoleAsync(user, role).Wait();
                }
            }

        }
    }
}
