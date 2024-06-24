using Microsoft.AspNetCore.Identity;
using RealtorHubAPI.Entities.Enums;
using RealtorHubAPI.Entities.Identity;

namespace CryptoProject.SeedDatabase
{
    public static class SeedIdentity
    {
        public static async Task SeedAsync(UserManager<User>? userManager, RoleManager<Role>? roleManager, IConfiguration config)
        {
            if (userManager is null || roleManager is null) return;
            await SeedRoles(roleManager);

            //Seed Super Admin
            var superAdminEmail = config["ROOT_ADMIN:Email"];
            if (await userManager.FindByEmailAsync(superAdminEmail) is null)
            {
                var superAdmin = new User()
                {
                    //Id = Guid.NewGuid(),
                    Email = superAdminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    PhoneNumber = config["ROOT_ADMIN:PhoneNumber"],
                    UserName = superAdminEmail,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Role = RoleType.SuperAdmin,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(superAdmin, config["ROOT_ADMIN:Password"]);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, nameof(RoleType.SuperAdmin));
                }
            }
            
            //ADMIN
            var Admin = config["DEMO_ADMIN:Email"];
            if (await userManager.FindByEmailAsync(Admin) is null)
            {
                var admin = new User()
                {
                    //Id = Guid.NewGuid(),
                    Email = Admin,
                    FirstName = "Demo",
                    LastName = "Admin",
                    PhoneNumber = config["DEMO_ADMIN:PhoneNumber"],
                    UserName = Admin,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Role = RoleType.Admin,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, config["DEMO_ADMIN:Password"]);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, nameof(RoleType.Admin));
                }
            }

        }
        private static async Task SeedRoles(RoleManager<Role> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(nameof(RoleType.SuperAdmin)))
            {
                await roleManager.CreateAsync(new Role(nameof(RoleType.SuperAdmin)));
            }
            if (!await roleManager.RoleExistsAsync(nameof(RoleType.Admin)))
            {
                await roleManager.CreateAsync(new Role(nameof(RoleType.Admin)));
            }
            if (!await roleManager.RoleExistsAsync(nameof(RoleType.User)))
            {
                await roleManager.CreateAsync(new Role(nameof(RoleType.User)));
            }
            if (!await roleManager.RoleExistsAsync(nameof(RoleType.Realtor)))
            {
                await roleManager.CreateAsync(new Role(nameof(RoleType.Realtor)));
            }
        }
    }

}
