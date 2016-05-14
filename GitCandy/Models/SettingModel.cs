using GitCandy.App_GlobalResources;
using System.ComponentModel.DataAnnotations;

namespace GitCandy.Models
{
    public class SettingModel
    {
        [Display(ResourceType = typeof(SR), Name = "Setting_IsPublicServer")]
        public bool IsPublicServer { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_ForceSsl")]
        public bool ForceSsl { get; set; }

        [Range(1, 65534, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_NumberRange")]
        [Display(ResourceType = typeof(SR), Name = "Setting_SslPort")]
        public int SslPort { get; set; }

        [Range(1, 65534, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_NumberRange")]
        [Display(ResourceType = typeof(SR), Name = "Setting_SshPort")]
        public int SshPort { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_EnableSsh")]
        public bool EnableSsh { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_LocalSkipCustomError")]
        public bool LocalSkipCustomError { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_AllowRegisterUser")]
        public bool AllowRegisterUser { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_AllowRepositoryCreation")]
        public bool AllowRepositoryCreation { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_RepositoryPath")]
        public string RepositoryPath { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_CachePath")]
        public string CachePath { get; set; }

        [Display(ResourceType = typeof(SR), Name = "Setting_GitCorePath")]
        public string GitCorePath { get; set; }

        [Range(5, 50, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_NumberRange")]
        [Display(ResourceType = typeof(SR), Name = "Setting_NumberOfCommitsPerPage")]
        public int NumberOfCommitsPerPage { get; set; }

        [Range(5, 50, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_NumberRange")]
        [Display(ResourceType = typeof(SR), Name = "Setting_NumberOfItemsPerList")]
        public int NumberOfItemsPerList { get; set; }

        [Range(10, 100, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "Validation_NumberRange")]
        [Display(ResourceType = typeof(SR), Name = "Setting_NumberOfRepositoryContributors")]
        public int NumberOfRepositoryContributors { get; set; }
    }
}