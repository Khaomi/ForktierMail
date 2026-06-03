namespace ForktierMail.Shared.Models;

public class SharedFork
{
    /// <summary>
    ///     Fork Id
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    ///     Fork name
    /// </summary>
    public string Name { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}