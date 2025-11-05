public class PlayerProgress
{
    public int Id { get; set; }
    public string PlayerName { get; set; }
    public int CheckpointIndex { get; set; }
    public int SummitCount { get; set; }

    public float CurrentElapsedTime { get; set; }
    public float BestTimeInSeconds { get; set; }

    public System.DateTime LastSaved { get; set; }
}