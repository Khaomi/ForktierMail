using System.Diagnostics.CodeAnalysis;
using ForktierMail.Client.Models;
using ForktierMail.Shared.Manager;
using ForktierMail.Shared.Models;

namespace ForktierMail.Client.Manager;

public class ClientDataManger(ForktierMailClient clientMail)
    : DataManager<ClientFork, ClientPlayer, ClientCharacter, ClientMail>
{
    protected override bool TryTransformFork(SharedFork fork, [NotNullWhen(true)] out ClientFork? transformedFork)
    {
        transformedFork = new ClientFork(clientMail)
        {
            Id = fork.Id,
            Name = fork.Name
        };

        return true;
    }

    protected override bool TryTransformPlayer(SharedPlayer player,
        [NotNullWhen(true)] out ClientPlayer? transformedPlayer)
    {
        transformedPlayer = new ClientPlayer(clientMail)
        {
            Id = player.Id
        };

        return true;
    }

    protected override bool TryTransformCharacter(SharedCharacter character,
        [NotNullWhen(true)] out ClientCharacter? transformedCharacter)
    {
        transformedCharacter = null;

        if (!Forks.TryGetValue(character.ForkId, out var fork))
            return false;

        if (!Players.TryGetValue(character.PlayerId, out var player))
            return false;

        transformedCharacter = new ClientCharacter(clientMail)
        {
            Id = character.Id,

            ForkId = character.ForkId,
            CharacterId = character.CharacterId,

            PlayerId = character.PlayerId,

            Name = character.Name,

            Fork = fork,
            Player = player
        };

        return true;
    }

    protected override bool TryTransformMail(SharedMail mail, [NotNullWhen(true)] out ClientMail? transformedMail)
    {
        transformedMail = null;

        if (!Forks.TryGetValue(mail.SenderForkId, out var sourceFork))
            return false;

        if (!Forks.TryGetValue(mail.RecipientForkId, out var recipientFork))
            return false;

        if (!Characters.TryGetValue((mail.SenderForkId, mail.SenderId), out var sourceCharacter))
            return false;

        if (!Characters.TryGetValue((mail.RecipientForkId, mail.RecipientId), out var recipientCharacter))
            return false;

        transformedMail = new ClientMail(clientMail)
        {
            Id = mail.Id,

            MailType = mail.MailType,
            Content = mail.Content,

            SenderForkId = mail.SenderForkId,
            SenderId = mail.SenderId,

            RecipientForkId = mail.RecipientForkId,
            RecipientId = mail.RecipientId,

            SenderFork = sourceFork,
            Sender = sourceCharacter,

            RecipientFork = recipientFork,
            Recipient = recipientCharacter
        };

        return true;
    }
}