using GitCandy.Base;
using GitCandy.Resources;
using System.ComponentModel.DataAnnotations;

namespace GitCandy.Models
{
    public class UserModel
    {
        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [RegularExpression(RegularExpressions.UserName, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Name")]
        [Display(ResourceType = typeof(SR), Name = "User_Username")]
        public string Name { get; set; }

        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(ResourceType = typeof(SR), Name = "User_Nickname")]
        public string Nickname { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [RegularExpression(RegularExpressions.Email, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Email")]
        [DataType(DataType.EmailAddress)]
        [Display(ResourceType = typeof(SR), Name = "User_Email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "User_Password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [Compare("Password", ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Compare")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "User_ConformPassword")]
        public string ConformPassword { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(ResourceType = typeof(SR), Name = "User_Description")]
        public string Description { get; set; }

        [UIHint("YesNo")]
        [Display(ResourceType = typeof(SR), Name = "User_IsSystemAdministrator")]
        public bool IsSystemAdministrator { get; set; }
    }
}
