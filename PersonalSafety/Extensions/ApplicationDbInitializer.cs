﻿using Microsoft.AspNetCore.Identity;
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
            
            if (!_clientRepository.GetAll().Any())
            {
                _clientRepository.Add(client);
                _clientRepository.Save();
            }

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
                Department = policeDpt,
                IsRescuer = false,
                IsFirstLogin = false
            };

            if (!_personnelRepository.GetAll().Any())
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
