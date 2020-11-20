using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
//using System.Web.Http;
using Mozu.Api.ToolKit.Config;
using Mozu.Api.WebToolKit.Filters;

namespace Mozu.Api.WebToolKit.Controllers
{
  
    [Route("api/application")]
    [ApiController]
    public class ApplicationController
    {
        private readonly IAppSetting _appSetting;

        public ApplicationController(IAppSetting appSetting)
        {
            _appSetting = appSetting;
        }


        [Route("info")]
        [HttpGet]
        public ActionResult<AppInfo> GetAppInfo()
        {
            return new AppInfo
            {
                NameSpace = _appSetting.Namespace,
                Version = _appSetting.Version,
                Package = _appSetting.PackageName
            };
        }
    }

    public class AppInfo
    {
        public string NameSpace;
        public string Version;
        public string Package;
    }
}
