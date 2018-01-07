using System.ComponentModel.DataAnnotations;

namespace Pomelo.AlertPlatform.API.Models
{
    public class Configuration
    {
        [MaxLength(64)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
