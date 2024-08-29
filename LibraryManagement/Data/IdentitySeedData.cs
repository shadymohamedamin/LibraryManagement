using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LibraryManagement.Data
{
    public static class IdentitySeedData
    {
        private const string AdminUserEmail = "admin@gmail.com";
        private const string AdminPassword = "Admin@123456";
        private const string AdminRoleName = "Admin";
        private const string UserRoleName = "User";

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure Admin role exists
                if (!await roleManager.RoleExistsAsync(AdminRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
                }
                if (!await roleManager.RoleExistsAsync(UserRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoleName));
                }

                // Ensure Admin user exists
                var adminUser = await userManager.FindByEmailAsync(AdminUserEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = AdminUserEmail,
                        Email = AdminUserEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(adminUser, AdminPassword);
                }

                // Ensure Admin user has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, AdminRoleName))
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRoleName);
                }
            }
        }
    }
}



