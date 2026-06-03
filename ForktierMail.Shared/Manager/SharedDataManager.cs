using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ForktierMail.Shared.Models;

namespace ForktierMail.Shared.Manager;

public class DefaultDataManager : DataManager<DefaultDataFork, DefaultDataPlayer, DefaultDataCharacter, DefaultDataMail>
{
    protected override bool TryTransformFork(SharedFork fork, [NotNullWhen(true)] out DefaultDataFork? transformedFork)
    {
        transformedFork = new DefaultDataFork
        {
            Id = fork.Id,
            Name = fork.Name
        };

        return true;
    }

    protected override bool TryTransformPlayer(SharedPlayer player,
        [NotNullWhen(true)] out DefaultDataPlayer? transformedPlayer)
    {
        transformedPlayer = new DefaultDataPlayer
        {
            Id = player.Id
        };

        return true;
    }

    protected override bool TryTransformCharacter(SharedCharacter character,
        [NotNullWhen(true)] out DefaultDataCharacter? transformedCharacter)
    {
        transformedCharacter = null;

        if (!Forks.TryGetValue(character.ForkId, out var fork))
            return false;

        if (!Players.TryGetValue(character.PlayerId, out var player))
            return false;

        transformedCharacter = new DefaultDataCharacter
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

    protected override bool TryTransformMail(SharedMail mail, [NotNullWhen(true)] out DefaultDataMail? transformedMail)
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

        transformedMail = new DefaultDataMail
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

/// <summary>
///     A dataclass used for handling relation between entities
/// </summary>
/// <typeparam name="F"></typeparam>
/// <typeparam name="C"></typeparam>
/// <typeparam name="P"></typeparam>
/// <typeparam name="M"></typeparam>
public abstract class DataManager<F, P, C, M>
    where F : DataFork<F, P, C, M>
    where P : DataPlayer<F, P, C, M>
    where C : DataCharacter<F, P, C, M>
    where M : DataMail<F, P, C, M>
{
    public ConcurrentDictionary<(int ForkId, int CharacterId), C> Characters = new();
    public ConcurrentDictionary<int, F> Forks = new();
    public ConcurrentDictionary<int, M> Mails = new();
    public ConcurrentDictionary<Guid, P> Players = new();


    /// <summary>
    ///     Transform from a standard SharedFork to F type
    /// </summary>
    /// <param name="fork"></param>
    /// <returns></returns>
    protected abstract bool TryTransformFork(SharedFork fork, [NotNullWhen(true)] out F? transformedFork);

    /// <summary>
    ///     Transform from a standard SharedPlayer to P type
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected abstract bool TryTransformPlayer(SharedPlayer player, [NotNullWhen(true)] out P? transformedPlayer);

    /// <summary>
    ///     Transform from a standard SharedCharacter to C type
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    protected abstract bool TryTransformCharacter(SharedCharacter character,
        [NotNullWhen(true)] out C? transformedCharacter);

    /// <summary>
    ///     Transform from a standard SharedMail to M type
    /// </summary>
    /// <param name="mail"></param>
    /// <returns></returns>
    protected abstract bool TryTransformMail(SharedMail mail, [NotNullWhen(true)] out M? transformedMail);

    public F? TrackFork(SharedFork fork)
    {
        if (Forks.TryGetValue(fork.Id, out var existingFork))
            return existingFork;

        if (!TryTransformFork(fork, out var createdFork))
            return null;

        foreach (var character in Characters.Values)
        {
            if (character.ForkId != fork.Id)
                continue;

            createdFork.Characters.TryAdd(character.CharacterId, character);
            character.Fork = createdFork;
        }

        Forks.TryAdd(createdFork.Id, createdFork);
        return createdFork;
    }

    public P? TrackPlayer(SharedPlayer player)
    {
        if (Players.TryGetValue(player.Id, out var existingPlayer))
            return existingPlayer;

        if (!TryTransformPlayer(player, out var createdPlayer))
            return null;

        foreach (var character in Characters.Values)
        {
            if (character.PlayerId != player.Id)
                continue;

            var forkCharacters = createdPlayer.Characters.GetOrAdd(
                character.ForkId,
                _ => new ConcurrentDictionary<int, C>()
            );

            forkCharacters.TryAdd(character.CharacterId, character);

            character.Player = createdPlayer;
        }

        Players.TryAdd(createdPlayer.Id, createdPlayer);
        return createdPlayer;
    }

    public C? TrackCharacter(SharedCharacter character)
    {
        if (Characters.TryGetValue((character.ForkId, character.CharacterId), out var existingCharacter))
            return existingCharacter;

        if (!TryTransformCharacter(character, out var createdCharacter))
            return null;

        if (Players.TryGetValue(character.PlayerId, out var player))
        {
            createdCharacter.Player = player;

            var forkCharacters = player.Characters.GetOrAdd(
                character.ForkId,
                _ => new ConcurrentDictionary<int, C>()
            );

            forkCharacters.TryAdd(createdCharacter.CharacterId, createdCharacter);
        }

        if (Forks.TryGetValue(character.ForkId, out var fork))
        {
            createdCharacter.Fork = fork;

            fork.Characters.TryAdd(createdCharacter.CharacterId, createdCharacter);
        }

        Characters.TryAdd((createdCharacter.ForkId, createdCharacter.CharacterId), createdCharacter);
        return createdCharacter;
    }

    public M? TrackMail(SharedMail mail)
    {
        if (Mails.TryGetValue(mail.Id, out var existingMail))
            return existingMail;

        if (!TryTransformMail(mail, out var createdMail))
            return null;

        if (Characters.TryGetValue((mail.SenderForkId, mail.SenderId), out var senderCharacter))
        {
            senderCharacter.SentMails.Add(createdMail);
            senderCharacter.Player.SentMails.Add(createdMail);
        }

        if (Characters.TryGetValue((mail.RecipientForkId, mail.RecipientId), out var recipientCharacter))
        {
            recipientCharacter.RecievedMails.Add(createdMail);
            recipientCharacter.Player.RecievedMails.Add(createdMail);
        }

        Mails.TryAdd(createdMail.Id, createdMail);
        return createdMail;
    }

    //

    public bool TryGetFork(int forkId, [NotNullWhen(true)] out F? fork)
    {
        if (Forks.TryGetValue(forkId, out var existingFork))
        {
            fork = existingFork;
            return true;
        }

        fork = null;
        return false;
    }

    public bool TryGetPlayer(Guid playerId, [NotNullWhen(true)] out P? player)
    {
        if (Players.TryGetValue(playerId, out var existingPlayer))
        {
            player = existingPlayer;
            return true;
        }

        player = null;
        return false;
    }

    public bool TryGetCharacter(int forkId, int characterId, [NotNullWhen(true)] out C? character)
    {
        if (Characters.TryGetValue((forkId, characterId), out var existingCharacter))
        {
            character = existingCharacter;
            return true;
        }

        character = null;
        return false;
    }

    public bool TryGetMail(int mailId, [NotNullWhen(true)] out M? mail)
    {
        if (Mails.TryGetValue(mailId, out var existingMail))
        {
            mail = existingMail;
            return true;
        }

        mail = null;
        return false;
    }

    //////////////////////////////////////////////////////

    public bool DeleteFork(int forkId)
    {
        if (!Forks.TryRemove(forkId, out var fork)) return false;

        var charactersToDelete = Characters.Values
            .Where(c => c.ForkId == forkId)
            .ToList();

        foreach (var character in charactersToDelete) DeleteCharacter(character.ForkId, character.CharacterId);

        var mailsToDelete = Mails.Values
            .Where(m => m.SenderForkId == forkId || m.RecipientForkId == forkId)
            .ToList();

        foreach (var mail in mailsToDelete) DeleteMail(mail.Id);

        fork.Characters.Clear();

        return true;
    }

    public bool DeletePlayer(Guid playerId)
    {
        if (!Players.TryRemove(playerId, out var player)) return false;

        var charactersToDelete = Characters.Values
            .Where(c => c.PlayerId == playerId)
            .ToList();

        foreach (var character in charactersToDelete) DeleteCharacter(character.ForkId, character.CharacterId);

        var mailsToDelete = player.SentMails.Concat(player.RecievedMails).ToList();

        foreach (var mail in mailsToDelete)
            DeleteMail(mail.Id);


        foreach (var forkCharacters in player.Characters.Values)
            forkCharacters.Clear();

        player.Characters.Clear();

        player.SentMails.Clear();
        player.RecievedMails.Clear();

        return true;
    }

    public bool DeleteCharacter(int forkId, int characterId)
    {
        if (!Characters.TryRemove((forkId, characterId), out var character)) return false;

        var mailsToDelete = Mails.Values
            .Where(m => (m.SenderForkId == forkId && m.SenderId == characterId) ||
                        (m.RecipientForkId == forkId && m.RecipientId == characterId))
            .ToList();

        foreach (var mail in mailsToDelete) DeleteMail(mail.Id);

        if (Forks.TryGetValue(character.ForkId, out var fork))
            fork.Characters.TryRemove(character.CharacterId, out _);

        if (Players.TryGetValue(character.PlayerId, out var player))
            if (player.Characters.TryGetValue(character.ForkId, out var forkCharacters))
            {
                forkCharacters.TryRemove(character.CharacterId, out _);

                if (forkCharacters.IsEmpty)
                    player.Characters.TryRemove(character.ForkId, out _);
            }

        return true;
    }

    public bool DeleteMail(int mailId)
    {
        if (!Mails.TryRemove(mailId, out var mail)) return false;

        if (Characters.TryGetValue((mail.SenderForkId, mail.SenderId), out var senderCharacter))
        {
            senderCharacter.SentMails.Remove(mail);
            senderCharacter.Player?.SentMails.Remove(mail);
        }

        if (Characters.TryGetValue((mail.RecipientForkId, mail.RecipientId), out var recipientCharacter))
        {
            recipientCharacter.RecievedMails.Remove(mail);
            recipientCharacter.Player?.RecievedMails.Remove(mail);
        }

        return true;
    }

    public bool UpdateFork(int forkId, SharedFork updated)
    {
        if (!Forks.TryGetValue(forkId, out var fork)) return false;
        if (updated.Id != forkId) return false;
        fork.Name = updated.Name;
        return true;
    }

    public bool UpdatePlayer(Guid playerId, SharedPlayer updated)
    {
        if (!Players.TryGetValue(playerId, out _)) return false;
        if (updated.Id != playerId) return false;
        return true;
    }

    public bool UpdateCharacter(int oldForkId, int oldCharacterId, SharedCharacter updated)
    {
        if (!Characters.TryGetValue((oldForkId, oldCharacterId), out var existing)) return false;

        var oldKey = (existing.ForkId, existing.CharacterId);
        var oldPlayerId = existing.PlayerId;
        var oldForkIdLocal = existing.ForkId;

        if (Forks.TryGetValue(oldForkIdLocal, out var oldFork))
            oldFork.Characters.TryRemove(existing.CharacterId, out _);

        if (Players.TryGetValue(oldPlayerId, out var oldPlayer))
            if (oldPlayer.Characters.TryGetValue(oldForkIdLocal, out var oldForkChars))
            {
                oldForkChars.TryRemove(existing.CharacterId, out _);
                if (oldForkChars.IsEmpty)
                    oldPlayer.Characters.TryRemove(oldForkIdLocal, out _);
            }

        var sent = existing.SentMails.ToList();
        var received = existing.RecievedMails.ToList();

        foreach (var mail in sent)
        {
            mail.SenderForkId = updated.ForkId;
            mail.SenderId = updated.CharacterId;
        }

        foreach (var mail in received)
        {
            mail.RecipientForkId = updated.ForkId;
            mail.RecipientId = updated.CharacterId;
        }

        existing.ForkId = updated.ForkId;
        existing.CharacterId = updated.CharacterId;
        existing.PlayerId = updated.PlayerId;
        existing.Name = updated.Name;

        if (!Forks.TryGetValue(updated.ForkId, out var newFork)) return false;
        newFork.Characters.TryAdd(updated.CharacterId, existing);

        if (!Players.TryGetValue(updated.PlayerId, out var newPlayer)) return false;
        var newForkChars = newPlayer.Characters.GetOrAdd(updated.ForkId, _ => new ConcurrentDictionary<int, C>());
        newForkChars.TryAdd(updated.CharacterId, existing);

        var newKey = (updated.ForkId, updated.CharacterId);
        if (!newKey.Equals(oldKey))
        {
            Characters.TryRemove(oldKey, out _);
            Characters.TryAdd(newKey, existing);
        }

        return true;
    }

    public bool UpdateMail(int mailId, SharedMail updated)
    {
        if (!Mails.TryGetValue(mailId, out var existing)) return false;

        if (Characters.TryGetValue((existing.SenderForkId, existing.SenderId), out var oldSender))
        {
            oldSender.SentMails.Remove(existing);
            oldSender.Player?.SentMails.Remove(existing);
        }

        if (Characters.TryGetValue((existing.RecipientForkId, existing.RecipientId), out var oldRecipient))
        {
            oldRecipient.RecievedMails.Remove(existing);
            oldRecipient.Player?.RecievedMails.Remove(existing);
        }

        existing.Content = updated.Content;
        existing.MailType = updated.MailType;
        existing.SenderForkId = updated.SenderForkId;
        existing.SenderId = updated.SenderId;
        existing.RecipientForkId = updated.RecipientForkId;
        existing.RecipientId = updated.RecipientId;

        if (Characters.TryGetValue((existing.SenderForkId, existing.SenderId), out var newSender))
        {
            newSender.SentMails.Add(existing);
            newSender.Player?.SentMails.Add(existing);
        }

        if (Characters.TryGetValue((existing.RecipientForkId, existing.RecipientId), out var newRecipient))
        {
            newRecipient.RecievedMails.Add(existing);
            newRecipient.Player?.RecievedMails.Add(existing);
        }

        return true;
    }
}