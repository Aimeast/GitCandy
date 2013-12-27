using GitCandy.App_GlobalResources;
using GitCandy.Base;
using System.ComponentModel.DataAnnotations;

namespace GitCandy.Models
{
    public class UserModel
    {
        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [RegularExpression(RegularExpression.Username, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Name")]
        [Display(ResourceType = typeof(SR), Name = "Account_Username")]
        public string Name { get; set; }

        [StringLength(20, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(SR), Name = "Account_Nickname")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Nickname { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [RegularExpression(RegularExpression.Email, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Email")]
        [DataType(DataType.EmailAddress)]
        [Display(ResourceType = typeof(SR), Name = "Account_Email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "Account_Password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [Compare("Password", ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Compare")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "Account_ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(SR), Name = "Account_Description")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Account_IsSystemAdministrator")]
        [UIHint("YesNo")]
        public bool IsSystemAdministrator { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Account_Teams")]
        [UIHint("Members")]
        [System.Web.Mvc.AdditionalMetadata("Controller", "Team")]
        public string[] Teams { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Account_Repositories")]
        [UIHint("Members")]
        [System.Web.Mvc.AdditionalMetadata("Controller", "Repository")]
        public string[] Respositories { get; set; }
    }
}
