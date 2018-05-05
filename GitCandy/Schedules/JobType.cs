namespace GitCandy.Schedules
{
    public enum JobTypes
    {
        /// <summary>
        /// A job run one time ASAP and shall finish up quickly
        /// </summary>
        OnceQuickly,
        /// <summary>
        /// A job run one time ASAP and may keep running duration long time
        /// </summary>
        OnceLongly,
        /// <summary>
        /// A job not run immediately and shall finish up quickly
        /// </summary>
        ScheduledQuickly,
        /// <summary>
        /// A job not run immediately and may keep running duration long time
        /// </summary>
        ScheduledLongly,
    }
}
