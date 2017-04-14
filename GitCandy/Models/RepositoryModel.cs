using GitCandy.App_GlobalResources;
using GitCandy.Base;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GitCandy.Models
{
    public class RepositoryModel
    {
        [Required(ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLengthRange")]
        [RegularExpression(RegularExpression.Repositoryname, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_Name")]
        [Display(ResourceType = typeof(SR), Name = "Repository_Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_StringLength")]
        [Display(ResourceType = typeof(SR), Name = "Repository_Description")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_HowInit")]
        public string HowInit { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_RemoteUrlTitle")]
        public string RemoteUrl { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_IsPrivate")]
        [UIHint("YesNo")]
        public bool IsPrivate { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_AllowAnonymousRead")]
        [UIHint("YesNo")]
        public bool AllowAnonymousRead { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_AllowAnonymousWrite")]
        [UIHint("YesNo")]
        public bool AllowAnonymousWrite { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_Collaborators")]
        [UIHint("Members")]
        [AdditionalMetadata("Controller", "Account")]
        public string[] Collaborators { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_Teams")]
        [UIHint("Members")]
        [AdditionalMetadata("Controller", "Team")]
        public string[] Teams { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Repository_DefaultBranch")]
        public string DefaultBranch { get; set; }

        public string[] LocalBranches { get; set; }

        public bool CurrentUserIsOwner { get; set; }
    }
}