﻿using System.ComponentModel.DataAnnotations;

namespace GirafRest.Models.DTOs.AccountDTOs
{
    /// <summary>
    /// This class defines the structure of the expected json when a user wishes to reset his password.
    /// </summary>
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Indtast venligst dit brugernavn her.")]
        [Display(Name = "Brugernavn")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Indtast venligst dit kodeord her.")]
        [DataType(DataType.Password)]
        [Display(Name = "Kodeord")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Gentag venligst dit kodeord her.")]
        [DataType(DataType.Password)]
        [Display(Name = "Bekræft Kodeord")]
        [Compare("Password", ErrorMessage = "De indtastede kodeord passer ikke sammen.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}