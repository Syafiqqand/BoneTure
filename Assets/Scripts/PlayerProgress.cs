// Ini adalah file DTO di Unity
public class PlayerProgress
{
    public int Id { get; set; }
    public string PlayerName { get; set; }
    public int CheckpointIndex { get; set; }

    public int SummitCount { get; set; }

    public System.DateTime LastSaved { get; set; }
}