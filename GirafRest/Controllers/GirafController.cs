﻿using GirafRest.Data;
using GirafRest.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using GirafRest.Services;

namespace GirafRest.Controllers
{
    public class GirafController : IGirafService
    {
        /// <summary>
        /// A reference to the database context - used to access the database and query for data. Handled by Asp.net's dependency injection.
        /// </summary>
        public GirafDbContext _context { get;  }
        /// <summary>
        /// Asp.net's user manager. Can be used to fetch user data from the request's cookie. Handled by Asp.net's dependency injection.
        /// </summary>
        public UserManager<GirafUser> _userManager { get;  }
        /// <summary>
        /// A data-logger used to write messages to the console. Handled by Asp.net's dependency injection.
        /// </summary>
        public ILogger _logger { get; set; }

        /// <summary>
        /// A constructor for the PictogramController. This is automatically called by Asp.net when receiving the first request for a pictogram.
        /// </summary>
        /// <param name="context">Reference to the database context.</param>
        /// <param name="userManager">Reference to Asp.net's user-manager.</param>
        /// <param name="env">Reference to an implementation of the IHostingEnvironment interface.</param>
        /// <param name="loggerFactory">Reference to an implementation of a logger.</param>
        public GirafController(GirafDbContext context, UserManager<GirafUser> userManager)
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
                    .Include(u => u.AvailableApplications)
                    //And return it
                    .FirstAsync();
        }

        /// <summary>
        /// Copies the content of the request's body into the specified file.
        /// </summary>
        /// <param name="bodyStream">A byte-stream from the body of the request.</param>
        /// <param name="targetFile">The target file for the copy.</param>
        /// <returns>Actually nothing - Task return-type in order to make the method async.</returns>
        public async Task<byte[]> ReadRequestImage(Stream bodyStream) {
            byte[] image;
            using (var imageStream = new MemoryStream()) {
                await bodyStream.CopyToAsync(imageStream);
                await bodyStream.FlushAsync();
                image = imageStream.ToArray();
            }

            return image;
        }

        /// <summary>
        /// Checks if the user owns the given <paramref name="pictogram"/> and returns true if so.
        /// Returns false if the user or his department does not own the <see cref="Pictogram"/>. 
        /// </summary>
        /// <param name="pictogram">The pictogram to check the ownership for.</param>
        /// <returns>True if the user is authorized to see the resource and false if not.</returns>
        public async Task<bool> CheckPrivateOwnership(Frame pictogram, HttpContext httpContext) {
            //The pictogram was not public, check if the user owns it.
            var usr = await LoadUserAsync(httpContext.User);
            if(usr == null) return false;
            
            var ownedByUser = await _context.UserResources
                .Where(ur => ur.ResourceKey == pictogram.Id && ur.OtherKey == usr.Id)
                .AnyAsync();
            if (ownedByUser)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> CheckProtectedOwnership(Frame resource, HttpContext httpContext)
        {
            var usr = await LoadUserAsync(httpContext.User);
            if (usr == null) return false;

            //The pictogram was not owned by user, check if his department owns it.
            var ownedByDepartment = await _context.DepartmentResources
                .Where(dr => dr.ResourceKey == resource.Id && dr.OtherKey == usr.Department.Key)
                .AnyAsync();
            if (ownedByDepartment) return true;

            return false;
        }
    }
}
