using System;
using System.Web;

namespace GitCandy.Configuration
{
    public class StoragePathResloverAttribute : RecommendedValueResloverAttribute
    {
        private StoragePathType storageType;

        public StoragePathResloverAttribute(StoragePathType val)
        {
            storageType = val;
        }

        public override object GetValue()
        {
            var val = "";
            switch (storageType)
            {
                case StoragePathType.Repository:
                    val = "Repos";
                    break;
                case StoragePathType.Cache:
                    val = "Caches";
                    break;
                default:
                    throw new ArgumentException(nameof(storageType));
            }

            return HttpContext.Current.Server.MapPath("~/App_Data/" + val);
        }
    }
}
