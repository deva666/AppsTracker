namespace AppsTracker.Domain.Model
{
    public abstract class SummaryBase : SelectableBase
    {
        public double Usage { get; set; }
        public long Duration { get; set; }
        public bool IsRequested { get; set; }
    }
}
