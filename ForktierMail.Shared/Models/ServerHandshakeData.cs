namespace ForktierMail.Shared.Models;

public class ServerHandshakeData
{
    public required int ForkId { get; set; }
    public required List<SharedFork> Forks { get; set; }
    public required List<SharedPlayer> Players { get; set; }
    public required List<SharedCharacter> Characters { get; set; }
    public required List<SharedMail> Mails { get; set; }
}