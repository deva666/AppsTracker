namespace AppsTracker.Domain.Logs
{
    public sealed class LogSummary
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string DateCreated { get; set; }
        public string DateEnded { get; set; }
        public long Duration { get; set; }
        public bool IsRequested { get; set; }
        public bool IsSelected { get; set; }
    }
}
