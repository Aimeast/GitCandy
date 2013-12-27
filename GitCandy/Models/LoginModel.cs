using GitCandy.App_GlobalResources;
using System.ComponentModel.DataAnnotations;

namespace GitCandy.Models
{
    public class LoginModel
    {
        [Display(ResourceType = typeof(SR), Name = "Account_UsernameOrEmail")]
        public string ID { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Account_Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
