using System.ComponentModel.DataAnnotations;

namespace BMS.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
