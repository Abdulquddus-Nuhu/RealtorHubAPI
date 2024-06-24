using CryptoProject.SeedDatabase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtorHubAPI.Data;
using RealtorHubAPI.Entities.Identity;

namespace RealtorHubAPI.SeedDatabase
{
    public class SeedDb : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedDb(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedDb>>();
            try
            {
                logger.LogInformation("Applying RealtorHub_Db Migration!");
                //await context.Database.EnsureCreatedAsync();
                await context.Database.MigrateAsync(cancellationToken: cancellationToken);
                logger.LogInformation("RealtorHub_Db Migration Successful!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to apply Crypto_Db Migration!");
            }
            var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
            try
            {
                logger.LogInformation("Seeding RealtorHub_Db Data!");
                await SeedIdentity.SeedAsync(userManager, roleManager, config);
                logger.LogInformation("Seeding RealtorHub_Db Successful!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to execute RealtorHub_Db Data Seeding!");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
