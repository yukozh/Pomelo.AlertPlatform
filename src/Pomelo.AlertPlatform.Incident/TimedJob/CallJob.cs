using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pomelo.AspNetCore.TimedJob;
using Pomelo.AlertPlatform.Incident.Lib;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident.TimedJob
{
    public class CallJob : Job
    {
        [Invoke(Begin = "2018-01-01", Interval = 1000 * 30, SkipWhileExecuting = true)]
        public void TriggerAlert(IncidentContext db, CallCenter callCenter)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] 正在扫描未触发报警的故障记录...");
            var nonAckedIncidents = db.Incidents
                .Include(x => x.Project)
                .Include(x => x.CallHistories)
                .Where(x => x.UserId == null)
                .ToList();
            var slots = GetOnCallSlots(db);
            Parallel.ForEach(nonAckedIncidents, x => {
                if (x.CallHistories.LastOrDefault()?.CreatedTime.AddMinutes(3) >= DateTime.UtcNow)
                {
                    return;
                }

                if (x.CallHistories.Where(y => !y.Ignore).Count() > 0 && x.Severity > 2)
                {
                    return;
                }

                if (x.CallHistories.Where(y => !y.Ignore).Count() > 0 && !callCenter.IsMessageSendOutAsync(x.CallHistories.Last().CallCenterId.Value).Result)
                {
                    return;
                }

                var callHistory = GenerateCall(slots.Where(y => y.ProjectId == x.ProjectId), x);
                var phone = slots.First(y => y.UserId == callHistory.UserId).User.PhoneNumber;
                callHistory.Type = x.Severity <= 2 ? "Voice" : "Sms";
                callHistory.CallCenterId = callCenter.TriggerAlertAsync($"您好，这里是柚子故障监测平台，在{x.Project.Name}中发生了严重程度为{x.Severity}的{x.Title}故障，故障编号{x.Id}，请您及时查看，谢谢。", callHistory.Type, phone).Result;
                db.CallHistories.Add(callHistory);
            });
            db.SaveChanges();
        }

        private CallHistory GenerateCall(IEnumerable<OnCallSlot> slots, Models.Incident incident)
        {
            var lastCall = incident.CallHistories.Where(y => !y.Ignore).LastOrDefault();
            if (lastCall == null)
            {
                return new CallHistory
                {
                    IncidentId = incident.Id,
                    CreatedTime = DateTime.UtcNow,
                    UserId = slots.First().UserId,
                    Role = SlotRole.Primary
                };
            }
            else
            {
                var role = (int)slots.Single(x => x.UserId == lastCall.UserId).Role;
                role = (role + 1) % Enum.GetNames(typeof(SlotRole)).Length;
                return new CallHistory
                {
                    IncidentId = incident.Id,
                    CreatedTime = DateTime.UtcNow,
                    UserId = slots.Single(x => x.Role == (SlotRole)role).UserId,
                    Role = (SlotRole)role
                };
            }
        }

        private IEnumerable<OnCallSlot> GetOnCallSlots(IncidentContext db)
        {
            var now = DateTime.UtcNow;
            return db.OnCallSlots
                .Include(x => x.User)
                .Where(x => x.Begin <= now && now < x.End)
                .ToList();
        }
    }
}
