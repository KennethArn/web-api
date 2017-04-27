﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GirafRest.Models
{
    /// <summary>
    /// Used to indicate that the user is allowed to use a given application.
    /// </summary>
    [ComplexType]
    public class ApplicationOption
    {
        /// <summary>
        /// The id of the ApplicationOption entity.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// The name of the application that the user is allowed to use.
        /// </summary>
        [Required]
        public string ApplicationName { get; set; }
        /// <summary>
        /// The package in which the given application is located.
        /// </summary>
        [Required]
        public string ApplicationPackage { get; set; }

        /// <summary>
        /// Creates a new application option, that may be added to a given user.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="applicationPackage">The package of the application.</param>
        public ApplicationOption(string applicationName, string applicationPackage)
        {
            ApplicationName = applicationName;
            ApplicationPackage = applicationPackage;
        }

        /// <summary>
        /// DO NOT DELETE THIS.
        /// </summary>
        public ApplicationOption()
        {

        }
    }
}
