using GitCandy.App_GlobalResources;
using System.ComponentModel.DataAnnotations;

namespace GitCandy.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "Account_OldPassword")]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "Account_NewPassword")]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Compare")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(SR), Name = "Account_ConformPassword")]
        public string ConformPassword { get; set; }
    }
}
