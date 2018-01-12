using System.ComponentModel.DataAnnotations;

namespace Pomelo.AlertPlatform.CallCenter.Models
{
    public class Configuration
    {
        [MaxLength(64)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
