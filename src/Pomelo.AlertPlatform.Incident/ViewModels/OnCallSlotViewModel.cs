using System;

namespace Pomelo.AlertPlatform.Incident.ViewModels
{
    public class OnCallSlotViewModel
    {
        public DateTime Begin { get; set; }
        
        public DateTime End { get; set; }

        public string Primary { get; set; }

        public string Backup { get; set; }

        public string IncidentManager { get; set; }

        public string Project { get; set; }
    }
}
