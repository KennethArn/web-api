using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GirafRest.Models;
using GirafRest.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GirafRest.Services;
using System;
using GirafRest.Models.Responses;

namespace GirafRest.Controllers
{
    /// <summary>
    /// The WeekController allows the user to view and update his week schedule along with deleting it.
    /// </summary>
    [Route("v1/[controller]")]
    public class WeekController : Controller
    {
        /// <summary>
        /// A reference to GirafService, that contains common functionality for all controllers.
        /// </summary>
        private readonly IGirafService _giraf;

        private readonly IAuthenticationService _authentication;

        /// <summary>
        /// Constructor for the Week-controller. This is called by the asp.net runtime.
        /// </summary>
        /// <param name="giraf">A reference to the GirafService.</param>
        /// <param name="loggerFactory">A reference to an implementation of ILoggerFactory. Used to create a logger.</param>
        public WeekController(IGirafService giraf, ILoggerFactory loggerFactory, IAuthenticationService authentication)
        {
            _giraf = giraf;
            _giraf._logger = loggerFactory.CreateLogger("Week");
            _authentication = authentication;
        }

        /// <summary>
        /// Gets all week schedule name and ids containing activities for the currently authenticated citizen.
        /// </summary>
        /// All WeekScheduleNameDTOs if succesfull request
        /// ErrorCode.UserNotFound if we cannot find any user in the DB
        /// ErrorCode.NoWeekScheduleFound if we can not find any weekschedule on the user
        /// <returns>
        /// </returns>
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<Response<IEnumerable<WeekNameDTO>>> ReadWeekSchedules(string userId)
        {
            var user = await _giraf.LoadByIdAsync(userId);
            if (user == null)
                return new ErrorResponse<IEnumerable<WeekNameDTO>>(ErrorCode.UserNotFound);

            // check access rightss
            if (!(await _authentication.CheckUserAccess(await _giraf._userManager.GetUserAsync(HttpContext.User), user)))
                return new ErrorResponse<IEnumerable<WeekNameDTO>>(ErrorCode.NotAuthorized);
            
            if (!user.WeekSchedule.Any())
                return new ErrorResponse<IEnumerable<WeekNameDTO>>(ErrorCode.NoWeekScheduleFound);
            
            return new Response<IEnumerable<WeekNameDTO>>(user.WeekSchedule.Select(w => new WeekNameDTO(w.WeekYear, w.WeekNumber, w.Name)));
        }

        /// <summary>
        /// Gets the schedule with the specified week number and year.
        /// </summary>
        /// <param name="weekYear">The year of the week schedule to fetch.</param>
        /// <param name="weekNumber">The week number of the week schedule to fetch.</param>
        /// <returns>NotFound if the user does not have a week with the given id or
        /// Ok and a serialized version of the week if he does.</returns>
        [HttpGet("{userId}/{weekYear}/{weekNumber}")]
        [Authorize]
        public async Task<Response<WeekDTO>> ReadUsersWeekSchedule(string userId, int weekYear, int weekNumber)
        {
            var user = await _giraf.LoadByIdAsync(userId);
            if (user == null)
                return new ErrorResponse<WeekDTO>(ErrorCode.UserNotFound);

            // check access rightss
            if (!(await _authentication.CheckUserAccess(await _giraf._userManager.GetUserAsync(HttpContext.User), user)))
                return new ErrorResponse<WeekDTO>(ErrorCode.NotAuthorized);
            
            var week = user.WeekSchedule.FirstOrDefault(w => w.WeekYear == weekYear && w.WeekNumber == weekNumber);
            if (week != null)
            {
                return new Response<WeekDTO>(new WeekDTO(week));
            }
            else
            {
                //Create default thumbnail
                var emptyThumbnail = _giraf._context.Pictograms.FirstOrDefault(r => r.Title == "default");
                if (emptyThumbnail == null)
                {
                    _giraf._context.Pictograms.Add(new Pictogram("default", AccessLevel.PUBLIC));
                    await _giraf._context.SaveChangesAsync();
                }
                emptyThumbnail = _giraf._context.Pictograms.FirstOrDefault(r => r.Title == "default");

                return new Response<WeekDTO>(new WeekDTO() { WeekYear = weekYear, Name = $"{weekYear} - {weekNumber}", WeekNumber = weekNumber, Thumbnail = new Models.DTOs.WeekPictogramDTO(emptyThumbnail), Days = new int[] { 1, 2, 3, 4, 5, 6, 7 }.Select(d => new WeekdayDTO() { Activities = new List<ActivityDTO>(), Day = (Days)d }).ToArray() });
            }
        }

        /// <summary>
        /// Updates the entire information of the week with the given year and week number.
        /// </summary>
        /// <param name="userId">id of user you want to get weekschedule for.</param>
        /// <param name="weekYear">year of the week you want to update</param>
        /// <param name="weekNumber">weeknr of week you want to update.</param>
        /// <param name="newWeek">A serialized Week with new information.</param>

        [HttpPut("{userId}/{weekYear}/{weekNumber}")]
        [Authorize]
        public async Task<Response<WeekDTO>> UpdateWeek(string userId, int weekYear, int weekNumber, [FromBody]WeekDTO newWeek)
        {
            //return Ok(newWeek);
            if (newWeek == null) return new ErrorResponse<WeekDTO>(ErrorCode.MissingProperties);

            var user = await _giraf.LoadByIdAsync(userId);
            if (user == null) return new ErrorResponse<WeekDTO>(ErrorCode.UserNotFound);

            // check access rightss
            if (!(await _authentication.CheckUserAccess(await _giraf._userManager.GetUserAsync(HttpContext.User), user)))
                return new ErrorResponse<WeekDTO>(ErrorCode.NotAuthorized);

            var week = user.WeekSchedule.FirstOrDefault(w => w.WeekYear == weekYear && w.WeekNumber == weekNumber);
            if (week == null)
            {
                week = new Week() { WeekYear = weekYear, WeekNumber = weekNumber };
                user.WeekSchedule.Add(week);
            }
            if (newWeek.Thumbnail != null)
            {
                var thumbnail = await _giraf._context.Pictograms.Where(p => p.Id == newWeek.Thumbnail.Id).FirstOrDefaultAsync();
                if (thumbnail == null)
                    return new ErrorResponse<WeekDTO>(ErrorCode.ThumbnailDoesNotExist);
                week.Thumbnail = thumbnail;
            }
            else
            {
                return new ErrorResponse<WeekDTO>(ErrorCode.MissingProperties, "thumbnail");
            }

            week.Name = newWeek.Name;
            var modelErrorCode = newWeek.ValidateModel();
            if (modelErrorCode.HasValue)
                return new ErrorResponse<WeekDTO>(modelErrorCode.Value, "Week should contain at least 1 day and no more than 7 days.");
            var orderedDays = week.Weekdays.OrderBy(w => w.Day).ToArray();
            foreach (var day in newWeek.Days)
            {
                var wkDay = new Weekday(day);
                if (!(await CreateWeekDayHelper(wkDay, day)))
                    return new ErrorResponse<WeekDTO>(ErrorCode.ResourceNotFound);
                week.UpdateDay(wkDay);
            }
            var toBeDeleted = week.Weekdays.Where(wd => !newWeek.Days.Any(d => d.Day == wd.Day)).ToList();
            foreach (var deletedDay in toBeDeleted)
            {
                week.Weekdays.Remove(deletedDay);
            }
            _giraf._context.Weeks.Update(week);
            await _giraf._context.SaveChangesAsync();
            return new Response<WeekDTO>(new WeekDTO(week));
        }

        /// <summary>
        /// Deletes all information for the entire week with the given year and week number.
        /// </summary>
        /// <param name="id">If of the week to update information for.</param>
        /// <param name="newWeek">A serialized Week with new information.</param>
        /// <returns>NotFound if the user does not have a week schedule or
        /// Ok and a serialized version of the updated week if everything went well.</returns>
        [HttpDelete("{userId}/{weekYear}/{weekNumber}")]
        [Authorize]
        public async Task<Response<IEnumerable<WeekDTO>>> DeleteWeek(string userId, int weekYear, int weekNumber)
        {
            var user = await _giraf.LoadByIdAsync(userId);
            if (user == null) return new ErrorResponse<IEnumerable<WeekDTO>>(ErrorCode.UserNotFound);
            // check access rightss
            if (!(await _authentication.CheckUserAccess(await _giraf._userManager.GetUserAsync(HttpContext.User), user)))
                return new ErrorResponse<IEnumerable<WeekDTO>>(ErrorCode.NotAuthorized);
            
            if (user.WeekSchedule.Any(w => w.WeekYear == weekYear && w.WeekNumber == weekNumber))
            {
                var week = user.WeekSchedule.FirstOrDefault(w => w.WeekYear == weekYear && w.WeekNumber == weekNumber);
                if (week == null) return new ErrorResponse<IEnumerable<WeekDTO>>(ErrorCode.NoError);
                user.WeekSchedule.Remove(week);
                await _giraf._context.SaveChangesAsync();
                return new Response<IEnumerable<WeekDTO>>(user.WeekSchedule.Select(w => new WeekDTO(w)));
            }
            else
                return new ErrorResponse<IEnumerable<WeekDTO>>(ErrorCode.NoWeekScheduleFound);
        }

        #region helpers

        /// <summary>
        /// Take pictograms and choices from DTO and add them to weekday object.
        /// </summary>
        /// <returns>True if all pictograms and choices were found and added, and false otherwise.</returns>
        /// <param name="to">Pictograms and choices will be added to this object.</param>
        /// <param name="from">Pictograms and choices will be read from this object.</param>
        private async Task<bool> CreateWeekDayHelper(Weekday to, WeekdayDTO from){
            if(from.Activities != null) 
            {
                foreach (var activityDTO in from.Activities)
                {
                    var picto = await _giraf._context.Pictograms.Where(p => p.Id == activityDTO.Pictogram.Id).FirstOrDefaultAsync();
                    if (picto != null)
                        to.Activities.Add(new Activity(to, picto, activityDTO.Order, activityDTO.State));
                }
            }
            return true;
        }


        #endregion
    }
}
