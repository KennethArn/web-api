using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GirafRest.Models.DTOs
{
    public class LauncherOptionsDTO
    {
        /// <summary>
        /// A flag indicating whether to run applications in grayscale or not.
        /// </summary>
        [Required]
        public bool UseGrayscale { get; set; }
        /// <summary>
        /// A flag indicating whether to display animations in the launcher or not.
        /// </summary>
       [Required]
        public bool DisplayLauncherAnimations { get; set; }
        /// <summary>
        /// A collection of all the user's applications.
        /// </summary>
        [Required]
        public virtual ICollection<ApplicationOption> appsUserCanAccess { get; set; }

        /// <summary>
        /// A field for storing how many rows to display in the GirafLauncher application.
        /// </summary>
        [Required]
        public int appGridSizeRows { get; set; }
        /// <summary>
        /// A field for storing how many columns to display in the GirafLauncher application.
        /// </summary>
        [Required]
        public int appGridSizeColumns { get; set; }

        public LauncherOptionsDTO(LauncherOptions options)
        {
            this.appGridSizeColumns = options.appGridSizeColumns;
            this.appGridSizeRows = options.appGridSizeRows;
            this.appsUserCanAccess = options.appsUserCanAccess;
            this.DisplayLauncherAnimations = options.DisplayLauncherAnimations;
            this.UseGrayscale = options.UseGrayscale;
        }

        public LauncherOptionsDTO()
        {
            appsUserCanAccess = new List<ApplicationOption>();
        }
    }
}