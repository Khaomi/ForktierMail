namespace ForktierMail.Shared.Models;

public class SharedCharacter
{
    public required int Id { get; set; }

    public required int ForkId { get; set; }

    public required int CharacterId { get; set; }
    // public required int Id { get; set; }

    public required string Name { get; set; }
    public required Guid PlayerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}