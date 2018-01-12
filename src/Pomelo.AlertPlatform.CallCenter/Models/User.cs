using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Pomelo.AlertPlatform.CallCenter.Models
{
    public class User : IdentityUser<Guid>
    {
        public virtual ICollection<App> Apps { get; set; }
    }
}
