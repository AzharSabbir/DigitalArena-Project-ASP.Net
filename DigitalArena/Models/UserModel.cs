using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalArena.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [RegularExpression("^[a-z0-9]{8}$", ErrorMessage = "Username must be exactly 8 lowercase letters or digits")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            ErrorMessage = "Password must include uppercase, lowercase, digit, special character, and be at least 8 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(?:\+880|880|0)(1[2-9]\d{8})$", ErrorMessage = "Enter a valid Bangladeshi phone number")]
        public string Phone { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        [Display(Name = "Profile Image Path")]
        public string ProfileImage { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name can't be longer than 100 characters")]
        public string FullName { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Login At")]
        public DateTime LastLoginAt { get; set; }
    }
}
