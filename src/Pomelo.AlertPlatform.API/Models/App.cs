using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.AlertPlatform.API.Models
{
    public class App
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Secret { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }
    }
}
