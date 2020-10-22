using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozu.Api.WebToolKit.Filters
{
    public class AuthCookie
    {
        public int TenantId { get; set; }
        public string FormToken { get; set; }
        public string CookieToken { get; set; }
        public string Hash { get; set; }
    }
}
