using Microsoft.AspNetCore.Builder; // import necessary namespaces
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PrepDb // define the PrepDb class
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd) // define static method to prepare data population
        {
            using (var serviceScope = app.ApplicationServices.CreateScope()) // create a service scope to get the AppDbContext
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd); // call the SeedData method with the AppDbContext
            }
        }

        private static void SeedData(AppDbContext context, bool isProd) // define static method to seed data
        {

            if (isProd)
            {
                Console.WriteLine("--> Applying Migrations..."); // output a message indicating that migrations are being applied
                try
                {
                    context.Database.Migrate(); // apply migrations to the context
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}"); // output a message indicating that migrations could not be applied
                }
            }

            if (!context.Platforms.Any()) // check if there are any existing platforms in the context
            {
                Console.WriteLine("--> Seeding Data..."); // output a message indicating that data is being seeded

                context.Platforms.AddRange( // add a list of platforms to the context's Platforms collection
                    new Platform() { Name = "Dot Net", Publisher = "Microsoft", Cost = "Free" },
                    new Platform() { Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free" },
                    new Platform() { Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free" }
                );

                context.SaveChanges(); // save changes to the context

            }
            else
            {
                Console.WriteLine("--> We already have data"); // output a message indicating that data already exists
            }
        }
    }
}