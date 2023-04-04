using System.Collections.Generic;
using PlatformService.Models; // import necessary namespaces

namespace PlatformService.Data
{
    public class PlatformRepo : IPlatformRepo // define the PlatformRepo class that implements the IPlatformRepo interface
    {
        private readonly AppDbContext _context; // define private instance variable for AppDbContext

        public PlatformRepo(AppDbContext context) { // define constructor that takes an AppDbContext parameter
            _context = context; // assign the AppDbContext parameter to the instance variable
        }

        public void CreatePlatform(Platform plat) // define method to create a new platform
        {
            if (plat == null) // check if input parameter is null
            {
                throw new System.ArgumentNullException(nameof(plat)); // throw an exception if input parameter is null
            }

            _context.Platforms.Add(plat); // add the platform to the context's Platforms collection
        }

        public IEnumerable<Platform> GetAllPlatforms() // define method to get all platforms
        {
            return _context.Platforms.ToList(); // return all platforms in the context's Platforms collection
        }

        public Platform GetPlatformById(int id) // define method to get platform by ID
        {
            return _context.Platforms.FirstOrDefault(p => p.Id == id); // return the first platform in the context's Platforms collection with a matching ID, or null if no matching ID is found
        }

        public bool SaveChanges() // define method to save changes to the context
        {
            return (_context.SaveChanges() >= 0); // save changes to the context and return a boolean indicating whether the changes were successfully saved or not
        }
    }
}