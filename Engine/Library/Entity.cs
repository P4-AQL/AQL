public class Entity
{
    public double ArrivalTime { get; set; }
    public readonly double CreationTime;

    public Entity(double creationTime)
    {
        CreationTime = creationTime;
        ArrivalTime = creationTime;
    }
}

