using System;
using MassTransit.Scheduling;

namespace SolarLab.BusManager.Implementation
{
    internal class RecurringScheduleWithImplicitScheduleId : RecurringSchedule
    {
        public RecurringScheduleWithImplicitScheduleId(string scheduleId, string scheduleGroup, string cronExpression)
        {
            this.ScheduleId = scheduleId;
            this.ScheduleGroup = scheduleGroup;
            this.TimeZoneId = TimeZoneInfo.Local.Id;
            this.StartTime = DateTime.Now;

            this.CronExpression = cronExpression;
        }

        public MissedEventPolicy MisfirePolicy { get; protected set; }

        public string TimeZoneId { get; protected set; }

        public DateTimeOffset StartTime { get; protected set; }

        public DateTimeOffset? EndTime { get; protected set; }

        public string ScheduleId { get; }

        public string ScheduleGroup { get; }

        public string CronExpression { get; protected set; }

        public string Description { get; protected set; }
    }
}