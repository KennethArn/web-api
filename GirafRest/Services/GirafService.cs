﻿using GirafRest.Data;
using GirafRest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using GirafRest.Services;

namespace GirafRest.Controllers
{
    /// <summary>
    /// The GirafService class implements the <see cref="IGirafService"/> interface and thus implements common
    /// functionality that is needed by most controllers.
    /// </summary>
    public class GirafService : IGirafService
    {
        /// <summary>
        /// A reference to the database context - used to access the database and query for data. Handled by asp.net's dependency injection.
        /// </summary>
        public GirafDbContext _context { get;  }
        /// <summary>
        /// Asp.net's user manager. Can be used to fetch user data from the request's cookie. Handled by asp.net's dependency injection.
        /// </summary>
        public UserManager<GirafUser> _userManager { get;  }
        /// <summary>
        /// A data-logger used to write messages to the console. Handled by asp.net's dependency injection.
        /// </summary>
        public ILogger _logger { get; set; }

        /// <summary>
        /// A constructor for the GirafService. This constructor is used when running in Production-mode.
        /// </summary>
        /// <param name="context">Reference to the database context.</param>
        /// <param name="userManager">Reference to asp.net's user-manager.</param>
        public GirafService(GirafMySqlDbContext context, UserManager<GirafUser> userManager) 
            : this(context as GirafDbContext, userManager)
        {}
        /// <summary>
        /// A constructor for the GirafService. This constructor is used when running in Development-mode.
        /// </summary>
        /// <param name="context">Reference to the database context.</param>
        /// <param name="userManager">Reference to asp.net's user-manager.</param>
        public GirafService(GirafSqliteDbContext context, UserManager<GirafUser> userManager)
            : this(context as GirafDbContext, userManager)
        {}

        /// <summary>
        /// The most general constructor for GirafService. This constructor is used by both the other constructors and the unit tests.
        /// </summary>
        /// <param name="context">Reference to the database context.</param>
        /// <param name="userManager">Reference to asp.net's user-manager.</param>
        public GirafService(GirafDbContext context, UserManager<GirafUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        /// <summary>
        /// Load the user from the <see cref="HttpContext"/> - both his information and all related data.
        /// </summary>
        /// <param name="principal">The security claim - i.e. the information about the currently authenticated user.</param>
        /// <returns>A <see cref="GirafUser"/> with related data.</returns>
        public async Task<GirafUser> LoadUserAsync(System.Security.Claims.ClaimsPrincipal principal)  {
            var usr = (await _userManager.GetUserAsync(principal));
            if(usr == null) return null;
            return await _context.Users
                    //First load the user from the database
                    .Where (u => u.Id == usr.Id)
                    //Then load his pictograms - both the relationship and the actual pictogram
                    .Include(u => u.Resources)
                    .ThenInclude(ur => ur.Resource)
                    //Then load his department and their pictograms
                    .Include(u => u.Department)
                    .ThenInclude(d => d.Resources)
                    .ThenInclude(dr => dr.Resource)
                    // then load his week schedule
                    .Include(u => u.WeekSchedule)
                    .ThenInclude(w => w.Weekdays)
                    .ThenInclude(wd => wd.Elements)
                    .Include(u => u.Settings)
                    .ThenInclude(lo => lo.appsUserCanAccess)
                    .Include(u => u.Guardians)
                    .Include(u => u.Citizens)
                    //And return it
                    .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Loads the user with the given username and also includes all related data.
        /// </summary>
        /// <param name="username">The username of the user to fetch.</param>
        /// <returns>A loaded user, i.e. a user with all related data.</returns>
        public async Task<GirafUser> LoadByNameAsync(string username)
        {
            var user = await  _context.Users
                    //First load the user from the database
                    .Where(u => u.UserName.ToLower() == username.ToLower())
                    //Then load his pictograms - both the relationship and the actual pictogram
                    .Include(u => u.Resources)
                    .ThenInclude(ur => ur.Resource)
                    //Then load his department and their pictograms
                    .Include(u => u.Department)
                    .ThenInclude(d => d.Resources)
                    // then load his week schedule
                    .Include(u => u.WeekSchedule)
                    .ThenInclude(w => w.Weekdays)
                    .ThenInclude(wd => wd.Elements)
                    .Include(u => u.Settings)
                    .ThenInclude(lo => lo.appsUserCanAccess).Include(t => t.Citizens)
                    .Include(u => u.Guardians)
                    .Include(u => u.Citizens)

                    //And return it
                    .FirstOrDefaultAsync();
            foreach (var relation in user.Citizens)
            {
                relation.Citizen = _context.Users.FirstOrDefault(u => u.Id == relation.CitizenId);
            }
            foreach (var relation in user.Guardians)
            {
                relation.Guardian = _context.Users.FirstOrDefault(u => u.Id == relation.GuardianId);
            }
            return user;
        }

        /// <summary>
        /// Reads an image from the current request's body and return it as a byte array.
        /// </summary>
        /// <param name="bodyStream">A byte-stream from the body of the request.</param>
        /// <returns>The image found in the request represented as a byte array.</returns>
        public async Task<byte[]> ReadRequestImage(Stream bodyStream)
        {
            try
            {
                byte[] image;
                using (var imageStream = new MemoryStream())
                {
                    await bodyStream.CopyToAsync(imageStream);
                    await bodyStream.FlushAsync();
                    image = imageStream.ToArray();
                }

                return image;
            }
            catch (Exception)
            {
                throw new ArgumentNullException("Input stream must not be null");
            }
        }

        /// <summary>
        /// Checks if the user owns the given <paramref name="pictogram"/>.
        /// </summary>
        /// <param name="pictogram">The pictogram to check the ownership for.</param>
        /// <param name="httpContext">A reference to the current HttpContext.</param>
        /// <returns>True if the user is authorized to see the resource and false if not.</returns>
        public async Task<bool> CheckPrivateOwnership(Pictogram pictogram, GirafUser user) {
            if (pictogram.AccessLevel != AccessLevel.PRIVATE)
                return false;

            //The pictogram was not public, check if the user owns it.
            if(user == null) return false;
            var ownedByUser = await _context.UserResources
                .Where(ur => ur.ResourceKey == pictogram.Id && ur.OtherKey == user.Id)
                .AnyAsync();

            return ownedByUser;
        }

        /// <summary>
        /// Checks if the current user's department owns the given resource.
        /// </summary>
        /// <param name="resource">The resource to check ownership for.</param>
        /// <param name="httpContext">The http context of the current request.</param>
        /// <returns>True if the user's department owns the pictogram, false if not.</returns>
        public async Task<bool> CheckProtectedOwnership(Pictogram resource, GirafUser user)
        {
            if (resource.AccessLevel != AccessLevel.PROTECTED)
                return false;

            if (user == null) return false;

            //The pictogram was not owned by user, check if his department owns it.
            var ownedByDepartment = await _context.DepartmentResources
                .Where(dr => dr.ResourceKey == resource.Id && dr.OtherKey == user.Department.Key)
                .AnyAsync();
                
            return ownedByDepartment;
        }
    }
}
