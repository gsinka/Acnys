using System;
using Acnys.Core.ValueObjects;
using NCrontab;

namespace Acnys.Core.Scheduler
{
    public interface IJobTrigger
    {
        DateTime? NextRun(DateTime now, DateTime lastRun);
    }

    public class CronTrigger : IJobTrigger
    {
        public string CronExpression { get; }
        public TimeInterval ValidityPeriod { get; }

        public CronTrigger(string cronExpression, TimeInterval validityPeriod)
        {
            CronExpression = cronExpression;
            ValidityPeriod = validityPeriod;
        }

        public DateTime? NextRun(DateTime now, DateTime lastRun)
        {
            // If the trigger is not valid return null
            if (!ValidityPeriod.Contains(now)) return null;

            // Get next
            var next = CrontabSchedule.Parse(CronExpression);
            return next.GetNextOccurrence(lastRun);
        }
    }
}