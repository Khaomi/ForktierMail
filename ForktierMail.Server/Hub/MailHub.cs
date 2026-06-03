using System.Collections.Concurrent;
using ForktierMail.Database;
using ForktierMail.Server.Manager;
using ForktierMail.Shared.Interface;
using ForktierMail.Shared.Models;
using Microsoft.AspNetCore.SignalR;

public class MailHub : Hub<IMailClient>, IMailHub
{
    private static readonly ConcurrentDictionary<int, string> ConnectedForks = new();
    private readonly ServerDataManager _data;

    private readonly ServerDbContext _db;

    public MailHub(ServerDbContext db, ServerDataManager data)
    {
        _db = db;
        _data = data;
    }

    public async Task<SharedFork> GetIdentity()
    {
        var forkId = (int)Context.Items["ForkId"];
        var forkName = (string)Context.Items["ForkName"];

        return new SharedFork
        {
            Id = forkId,
            Name = forkName
        };
    }

    public Task<bool> SendHandshake(ClientHandshakeData handshake)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        if (!_data.TryGetFork(sourceForkId, out _))
            return Task.FromResult(false);

        var logger = Context.GetHttpContext()?.RequestServices.GetRequiredService<ILogger<MailHub>>();
        logger?.LogInformation("Received handshake from ForkId {ForkId} with data {@Handshake}", sourceForkId,
            handshake);

        foreach (var player in handshake.Players) _data.TrackPlayer(player);

        foreach (var character in handshake.Characters)
        {
            if (character.ForkId != sourceForkId)
                continue;

            _data.TrackCharacter(character);
        }

        return Task.FromResult(true);
    }

    public async Task<bool> SendMail(SharedMail mail)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return false;

        if (mail.SenderForkId != sourceForkId)
            return false;

        if (!_data.TryGetFork(mail.SenderForkId, out _))
            return false;

        if (!_data.TryGetFork(mail.RecipientForkId, out _))
            return false;

        if (!_data.TryGetCharacter(mail.SenderForkId, mail.SenderId, out _))
            return false;

        if (!_data.TryGetCharacter(mail.RecipientForkId, mail.RecipientId, out _))
            return false;

        if (!ConnectedForks.TryGetValue(mail.RecipientForkId, out var recipientConnectionId))
            return false;

        var accepted = await Clients.Client(recipientConnectionId).OnMailRecieved(mail);
        if (!accepted)
            return false;

        return _data.TrackMail(mail) is not null;
    }

    public Task<bool> AddPlayer(SharedPlayer player)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        var result = _data.TrackPlayer(player);
        if (result is null)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnPlayerAdded(player);
        return Task.FromResult(true);
    }

    public Task<bool> AddCharacter(SharedCharacter character)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        if (character.ForkId != sourceForkId)
            return Task.FromResult(false);

        var result = _data.TrackCharacter(character);
        if (result is null)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnCharacterAdded(character);
        return Task.FromResult(true);
    }

    public Task<bool> RemovePlayer(Guid playerId)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        var result = _data.DeletePlayer(playerId);
        if (!result)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnPlayerRemoved(playerId);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveCharacter(int characterId)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        var result = _data.DeleteCharacter(sourceForkId, characterId);
        if (!result)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnCharacterRemoved(sourceForkId, characterId);
        return Task.FromResult(true);
    }

    public Task<bool> UpdatePlayer(SharedPlayer player)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        var result = _data.UpdatePlayer(player.Id, player);
        if (!result)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnPlayerUpdated(player);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateCharacter(SharedCharacter character)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        if (character.ForkId != sourceForkId)
            return Task.FromResult(false);

        var result = _data.UpdateCharacter(character.ForkId, character.CharacterId, character);
        if (!result)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnCharacterUpdated(character);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteMail(int mailId)
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult(false);

        var result = _data.DeleteMail(mailId);
        if (!result)
            return Task.FromResult(false);

        _ = Clients.AllExcept(Context.ConnectionId).OnMailRemoved(mailId);
        return Task.FromResult(true);
    }

    public Task<ServerHandshakeData?> GetHandshakeData()
    {
        if (!Context.Items.TryGetValue("ForkId", out var forkIdValue) || forkIdValue is not int sourceForkId)
            return Task.FromResult<ServerHandshakeData?>(null);

        var handshake = new ServerHandshakeData
        {
            ForkId = sourceForkId,
            Forks =
            [
                .. _data.Forks.Values.Select(f => new SharedFork
                {
                    Id = f.Id,
                    Name = f.Name
                })
            ],
            Players =
            [
                .. _data.Players.Values.Select(p => new SharedPlayer
                {
                    Id = p.Id
                })
            ],
            Characters =
            [
                .. _data.Characters.Values.Select(c => new SharedCharacter
                {
                    Id = c.Id,
                    ForkId = c.ForkId,
                    PlayerId = c.PlayerId,
                    CharacterId = c.CharacterId,
                    Name = c.Name
                })
            ],
            Mails =
            [
                .. _data.Mails.Values.Select(m => new SharedMail
                {
                    Id = m.Id,
                    SenderForkId = m.SenderForkId,
                    SenderId = m.SenderId,
                    RecipientForkId = m.RecipientForkId,
                    RecipientId = m.RecipientId,
                    MailType = m.MailType,
                    Content = m.Content
                })
            ]
        };

        return Task.FromResult<ServerHandshakeData?>(handshake);
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null || !httpContext.Request.Query.TryGetValue("apiKey", out var apiKey))
            throw new UnauthorizedAccessException("API Key is required");

        var fork = _db.Forks.FirstOrDefault(f => f.ApiKey == apiKey.ToString());
        if (fork is null) throw new UnauthorizedAccessException("Invalid API Key");

        Context.Items["ForkId"] = fork.Id;
        Context.Items["ForkName"] = fork.Name;
        ConnectedForks.AddOrUpdate(fork.Id, Context.ConnectionId, (_, _) => Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("ForkId", out var forkIdValue) && forkIdValue is int forkId &&
            ConnectedForks.TryGetValue(forkId, out var connectionId) && connectionId == Context.ConnectionId)
        {
            ConnectedForks.TryRemove(forkId, out _);
            await Clients.All.OnForkRemoved(forkId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}