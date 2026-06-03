namespace ForktierMail.Shared.Models;

public enum MailType
{
    UNKNOWN = 0,
    LETTER = 1,
    PACKAGE = 2
}

public class SharedMail
{
    /// <summary>
    ///     Database Id
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    ///     Type of mail
    /// </summary>
    public required MailType MailType { get; set; } = MailType.LETTER;

    /// <summary>
    ///     Content of the mail
    /// </summary>
    public required string Content { get; set; } = "";

    /// <summary>
    ///     The fork that sent the mail
    /// </summary>
    public required int SenderForkId { get; set; }

    /// <summary>
    ///     The character ID from `SenderForkId` that sent the message
    /// </summary>
    public required int SenderId { get; set; }

    /// <summary>
    ///     The fork to send mail to
    /// </summary>
    public required int RecipientForkId { get; set; }

    /// <summary>
    ///     The character id that the mail is for
    /// </summary>
    public required int RecipientId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}