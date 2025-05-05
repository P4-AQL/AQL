public class Event : IComparable<Event>
{
    public double Time { get; set; }
    public Action Action { get; set; }

    public Event(double time, Action action)
    {
        Time = time;
        Action = action;
    }

    public int CompareTo(Event other)
    {
        return Time.CompareTo(other.Time);
    }
}