using ForktierMail.Database;
using ForktierMail.Shared.Manager;
using ForktierMail.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MailType = ForktierMail.Shared.Models.MailType;

namespace ForktierMail.Server.Manager;

public class ServerDataManager : DefaultDataManager
{
    private readonly IDbContextFactory<ServerDbContext> _dbFactory;

    public ServerDataManager(IDbContextFactory<ServerDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <summary>
    ///     Seeds the database with initial data for development/testing purposes
    /// </summary>
    public async Task SeedDevelopment()
    {
        using var db = _dbFactory.CreateDbContext();
        if (await db.Forks.AnyAsync()) return;

        var fork1 = new Fork { Id = 1, Name = "Fork 1", ApiKey = "key1" };
        var fork2 = new Fork { Id = 2, Name = "Fork 2", ApiKey = "key2" };

        db.Forks.AddRange(fork1, fork2);
        await db.SaveChangesAsync();
    }

    public async Task LoadFromDatabase()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();

        var forks = (await dbContext.Forks.ToListAsync()).Where(f => f is not null);
        var players = (await dbContext.Players.ToListAsync()).Where(p => p is not null);
        var characters = (await dbContext.Characters.ToListAsync()).Where(c => c is not null);
        var mails = (await dbContext.Mails.ToListAsync()).Where(m => m is not null);

        foreach (var fork in forks)
            TrackFork(new SharedFork
            {
                Id = fork.Id,
                Name = fork.Name
            });

        foreach (var player in players)
            TrackPlayer(new SharedPlayer
            {
                Id = player.Id
            });

        foreach (var character in characters)
            TrackCharacter(new SharedCharacter
            {
                Id = character.Id,
                Name = character.Name,
                ForkId = character.ForkId,
                CharacterId = character.CharacterId,
                PlayerId = character.PlayerId
            });

        foreach (var mail in mails)
            TrackMail(new SharedMail
            {
                Id = mail.Id,
                MailType = (MailType)mail.Type,
                Content = mail.Content,
                SenderForkId = mail.SenderForkId,
                SenderId = mail.SenderId,
                RecipientForkId = mail.RecipientForkId,
                RecipientId = mail.RecipientId
            });
    }

    public new DefaultDataFork? TrackFork(SharedFork fork)
    {
        var result = base.TrackFork(fork);
        try
        {
            if (result == null) return null;
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Forks.Find(result.Id);
            if (existing == null)
            {
                db.Forks.Add(new Fork { Id = result.Id, Name = result.Name, ApiKey = "" });
                db.SaveChanges();
            }
            else if (existing.Name != result.Name)
            {
                existing.Name = result.Name;
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new DefaultDataPlayer? TrackPlayer(SharedPlayer player)
    {
        var result = base.TrackPlayer(player);
        try
        {
            if (result == null) return null;
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Players.Find(result.Id);
            if (existing == null)
            {
                db.Players.Add(new Player { Id = result.Id });
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new DefaultDataCharacter? TrackCharacter(SharedCharacter character)
    {
        var result = base.TrackCharacter(character);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Characters.FirstOrDefault(c =>
                c.ForkId == character.ForkId && c.CharacterId == character.CharacterId);
            if (existing == null)
            {
                db.Characters.Add(new Character
                {
                    ForkId = character.ForkId,
                    CharacterId = character.CharacterId,
                    PlayerId = character.PlayerId,
                    Name = character.Name
                });
                db.SaveChanges();
            }
            else
            {
                var dirty = false;
                if (existing.PlayerId != character.PlayerId)
                {
                    existing.PlayerId = character.PlayerId;
                    dirty = true;
                }

                if (existing.Name != character.Name)
                {
                    existing.Name = character.Name;
                    dirty = true;
                }

                if (dirty) db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new DefaultDataMail? TrackMail(SharedMail mail)
    {
        var result = base.TrackMail(mail);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Mails.Find(mail.Id);
            if (existing == null)
            {
                db.Mails.Add(new Mail
                {
                    Id = mail.Id,
                    Type = (Database.MailType)mail.MailType,
                    Content = mail.Content,
                    SenderForkId = mail.SenderForkId,
                    SenderId = mail.SenderId,
                    RecipientForkId = mail.RecipientForkId,
                    RecipientId = mail.RecipientId
                });
                db.SaveChanges();
            }
            else
            {
                var dirty = false;
                if (existing.Content != mail.Content)
                {
                    existing.Content = mail.Content;
                    dirty = true;
                }

                if (existing.RecipientForkId != mail.RecipientForkId)
                {
                    existing.RecipientForkId = mail.RecipientForkId;
                    dirty = true;
                }

                if (existing.RecipientId != mail.RecipientId)
                {
                    existing.RecipientId = mail.RecipientId;
                    dirty = true;
                }

                if (existing.SenderForkId != mail.SenderForkId)
                {
                    existing.SenderForkId = mail.SenderForkId;
                    dirty = true;
                }

                if (existing.SenderId != mail.SenderId)
                {
                    existing.SenderId = mail.SenderId;
                    dirty = true;
                }

                if (dirty) db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool UpdateFork(int forkId, SharedFork updated)
    {
        var result = base.UpdateFork(forkId, updated);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Forks.Find(forkId);
            if (existing != null)
            {
                existing.Name = updated.Name;
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool UpdatePlayer(Guid playerId, SharedPlayer updated)
    {
        var result = base.UpdatePlayer(playerId, updated);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Players.Find(playerId);
            if (existing == null && result)
            {
                db.Players.Add(new Player { Id = playerId });
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool UpdateCharacter(int oldForkId, int oldCharacterId, SharedCharacter updated)
    {
        var result = base.UpdateCharacter(oldForkId, oldCharacterId, updated);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            using var tx = db.Database.BeginTransaction();

            var existing = db.Characters.FirstOrDefault(c => c.ForkId == oldForkId && c.CharacterId == oldCharacterId);
            if (existing != null)
            {
                var targetFork = db.Forks.Find(updated.ForkId);
                var targetPlayer = db.Players.Find(updated.PlayerId);
                if (targetFork == null) throw new InvalidOperationException($"Target fork {updated.ForkId} not found");
                if (targetPlayer == null)
                    throw new InvalidOperationException($"Target player {updated.PlayerId} not found");

                var mailsAsSender = db.Mails.Where(m => m.SenderForkId == oldForkId && m.SenderId == oldCharacterId)
                    .ToList();
                foreach (var m in mailsAsSender)
                {
                    m.SenderForkId = updated.ForkId;
                    m.SenderId = updated.CharacterId;
                }

                var mailsAsRecipient = db.Mails
                    .Where(m => m.RecipientForkId == oldForkId && m.RecipientId == oldCharacterId).ToList();
                foreach (var m in mailsAsRecipient)
                {
                    m.RecipientForkId = updated.ForkId;
                    m.RecipientId = updated.CharacterId;
                }

                existing.ForkId = updated.ForkId;
                existing.CharacterId = updated.CharacterId;
                existing.PlayerId = updated.PlayerId;
                existing.Name = updated.Name;

                db.SaveChanges();
                tx.Commit();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool UpdateMail(int mailId, SharedMail updated)
    {
        var result = base.UpdateMail(mailId, updated);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Mails.Find(mailId);
            if (existing != null)
            {
                existing.Content = updated.Content;
                existing.Type = (Database.MailType)updated.MailType;
                existing.SenderForkId = updated.SenderForkId;
                existing.SenderId = updated.SenderId;
                existing.RecipientForkId = updated.RecipientForkId;
                existing.RecipientId = updated.RecipientId;
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool DeleteMail(int mailId)
    {
        var result = base.DeleteMail(mailId);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var existing = db.Mails.Find(mailId);
            if (existing != null)
            {
                db.Mails.Remove(existing);
                db.SaveChanges();
            }
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool DeleteCharacter(int forkId, int characterId)
    {
        var result = base.DeleteCharacter(forkId, characterId);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            using var tx = db.Database.BeginTransaction();

            var mailsToDelete = db.Mails.Where(m =>
                (m.SenderForkId == forkId && m.SenderId == characterId) ||
                (m.RecipientForkId == forkId && m.RecipientId == characterId)).ToList();
            if (mailsToDelete.Count != 0) db.Mails.RemoveRange(mailsToDelete);

            var existing = db.Characters.FirstOrDefault(c => c.ForkId == forkId && c.CharacterId == characterId);
            if (existing != null) db.Characters.Remove(existing);

            db.SaveChanges();
            tx.Commit();
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool DeletePlayer(Guid playerId)
    {
        var result = base.DeletePlayer(playerId);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            using var tx = db.Database.BeginTransaction();

            var charactersToDelete = db.Characters.Where(c => c.PlayerId == playerId).ToList();

            var mailsSent = db.Mails.Join(db.Characters,
                    m => new { ForkId = m.SenderForkId, CharacterId = m.SenderId },
                    c => new { c.ForkId, c.CharacterId },
                    (m, c) => new { Mail = m, Character = c })
                .Where(x => x.Character.PlayerId == playerId)
                .Select(x => x.Mail)
                .ToList();

            var mailsReceived = db.Mails.Join(db.Characters,
                    m => new { ForkId = m.RecipientForkId, CharacterId = m.RecipientId },
                    c => new { c.ForkId, c.CharacterId },
                    (m, c) => new { Mail = m, Character = c })
                .Where(x => x.Character.PlayerId == playerId)
                .Select(x => x.Mail)
                .ToList();

            var mailsToDelete = mailsSent.Concat(mailsReceived).Distinct().ToList();
            if (mailsToDelete.Count != 0) db.Mails.RemoveRange(mailsToDelete);

            if (charactersToDelete.Count != 0) db.Characters.RemoveRange(charactersToDelete);

            var existingPlayer = db.Players.Find(playerId);
            if (existingPlayer != null) db.Players.Remove(existingPlayer);

            db.SaveChanges();
            tx.Commit();
        }
        catch (Exception)
        {
        }

        return result;
    }

    public new bool DeleteFork(int forkId)
    {
        var result = base.DeleteFork(forkId);
        try
        {
            using var db = _dbFactory.CreateDbContext();
            using var tx = db.Database.BeginTransaction();

            var mailsToDelete = db.Mails.Where(m => m.SenderForkId == forkId || m.RecipientForkId == forkId).ToList();
            if (mailsToDelete.Count != 0) db.Mails.RemoveRange(mailsToDelete);

            var charactersToDelete = db.Characters.Where(c => c.ForkId == forkId).ToList();
            if (charactersToDelete.Count != 0) db.Characters.RemoveRange(charactersToDelete);

            var existingFork = db.Forks.Find(forkId);
            if (existingFork != null) db.Forks.Remove(existingFork);

            db.SaveChanges();
            tx.Commit();
        }
        catch (Exception)
        {
        }

        return result;
    }
}