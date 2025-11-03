// (Kosongkan saja file ini jika kamu tidak pakai namespace)
// atau 'using GameApi.Models;' jika kamu menambahkannya

// Ini adalah DTO (Data Transfer Object)
public class PlayerProgress
{
    public int Id { get; set; }
    public string PlayerName { get; set; }
    public int CheckpointIndex { get; set; }
    public System.DateTime LastSaved { get; set; }
}