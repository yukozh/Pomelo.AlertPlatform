using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Pomelo.AlertPlatform.API.Models
{
    public class User : IdentityUser<Guid>
    {
        public virtual ICollection<App> Apps { get; set; }
    }
}
