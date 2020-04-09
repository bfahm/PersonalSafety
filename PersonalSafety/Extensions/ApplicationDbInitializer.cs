﻿using Microsoft.AspNetCore.Identity;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IClientRepository _clientRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public ApplicationDbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IClientRepository clientRepository, IPersonnelRepository personnelRepository, IDepartmentRepository departmentRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _clientRepository = clientRepository;
            _personnelRepository = personnelRepository;
            _departmentRepository = departmentRepository;
        }

        public void SeedUsers()
        {
            #region CreateRoles
            CreateRoles();
            #endregion

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

            #region Create Departments

            CreateDepartments();
            var tantaPoliceDepartment = _departmentRepository.GetAll().FirstOrDefault(d =>
                d.AuthorityType == (int) AuthorityTypesEnum.Police && d.City == (int) CitiesEnum.Tanta);

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
                DepartmentId = tantaPoliceDepartment.Id,
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
                DepartmentId = tantaPoliceDepartment.Id,
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


                int counter = 0;
                foreach (var city in Enum.GetValues(typeof(CitiesEnum)))
                {
                    foreach (var authorityType in Enum.GetValues(typeof(AuthorityTypesEnum)))
                    {
                        _departmentRepository.Add(new Department()
                        {
                            City = (int)city,
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
    }
}
