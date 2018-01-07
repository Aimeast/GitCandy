using GitCandy.Resources;
using System.ComponentModel.DataAnnotations;

namespace GitCandy.Models
{
    public class LoginModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(ResourceType = typeof(SR), Name = "User_Username")]
        public string Name { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "User_Password")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
