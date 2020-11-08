using System.Linq;
using Microsoft.AspNetCore.Mvc;
//using System.Web.Http;
using Mozu.Api.WebToolKit.Models;

namespace Mozu.Api.WebToolKit.Controllers
{
    [Route("api/version")]
    [ApiController]
    public class VersionController:ControllerBase //: ApiController
    {

        public IActionResult Get()
        {
            var versions = Helper.GetVersions();
            return Ok(versions);
        }

        [Route("app/{appName}")]
        public IActionResult GetAppVersion(string appName)
        {
            var versions = Helper.GetVersions();
            var appVersion = versions.Assemblies.SingleOrDefault(x => x.Name.Equals(appName));
            if (appVersion != null)
                versions.AppVersion = appVersion.Version;
            return Ok(versions);
            
        }

    }
}
