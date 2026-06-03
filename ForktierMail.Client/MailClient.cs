using ForktierMail.Client.Manager;
using ForktierMail.Shared.Interface;
using ForktierMail.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ForktierMail.Client;

public class ForktierMailClient : IAsyncDisposable
{
    protected string ApiKey;
    protected HubConnection connection;
    public ClientDataManger dataManger;

    public ForktierMailClient(string address, string apiKey)
    {
        ApiKey = apiKey;

        dataManger = new ClientDataManger(this);

        connection = new HubConnectionBuilder()
            .WithUrl($"{address}/Mail?apiKey={apiKey}")
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();

        connection.On<SharedMail, bool>(nameof(IMailClient.OnMailRecieved), OnMailRecieved);
        connection.On<ServerHandshakeData, bool>(nameof(IMailClient.OnServerHandshake), OnServerHandshake);

        connection.On<SharedPlayer>(nameof(IMailClient.OnPlayerAdded), OnPlayerAdded);
        connection.On<SharedCharacter>(nameof(IMailClient.OnCharacterAdded), OnCharacterAdded);
        connection.On<Guid>(nameof(IMailClient.OnPlayerRemoved), OnPlayerRemoved);
        connection.On<int, int>(nameof(IMailClient.OnCharacterRemoved), OnCharacterRemoved);
        connection.On<SharedMail>(nameof(IMailClient.OnMailUpdated), OnMailUpdated);
        connection.On<SharedPlayer>(nameof(IMailClient.OnPlayerUpdated), OnPlayerUpdated);
        connection.On<SharedCharacter>(nameof(IMailClient.OnCharacterUpdated), OnCharacterUpdated);
        connection.On<int>(nameof(IMailClient.OnMailRemoved), OnMailRemoved);
        connection.On<int>(nameof(IMailClient.OnForkRemoved), OnForkRemoved);
    }

    public ForktierMailClient(HubConnection existingConnection, string apiKey = "")
    {
        ApiKey = apiKey;
        connection = existingConnection;
        dataManger = new ClientDataManger(this);

        connection.On<SharedMail, bool>(nameof(IMailClient.OnMailRecieved), OnMailRecieved);
        connection.On<ServerHandshakeData, bool>(nameof(IMailClient.OnServerHandshake), OnServerHandshake);

        connection.On<SharedPlayer>(nameof(IMailClient.OnPlayerAdded), OnPlayerAdded);
        connection.On<SharedCharacter>(nameof(IMailClient.OnCharacterAdded), OnCharacterAdded);
        connection.On<Guid>(nameof(IMailClient.OnPlayerRemoved), OnPlayerRemoved);
        connection.On<int, int>(nameof(IMailClient.OnCharacterRemoved), OnCharacterRemoved);
        connection.On<SharedMail>(nameof(IMailClient.OnMailUpdated), OnMailUpdated);
        connection.On<SharedPlayer>(nameof(IMailClient.OnPlayerUpdated), OnPlayerUpdated);
        connection.On<SharedCharacter>(nameof(IMailClient.OnCharacterUpdated), OnCharacterUpdated);
        connection.On<int>(nameof(IMailClient.OnMailRemoved), OnMailRemoved);
        connection.On<int>(nameof(IMailClient.OnForkRemoved), OnForkRemoved);
    }

    public int ForkId { get; private set; }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
            await connection.DisposeAsync();
    }

    // Connection
    ///////////////////////////////////////////////////////////

    public async Task Start()
    {
        if (connection.State == HubConnectionState.Disconnected)
            await connection.StartAsync();
        await OnServerHandshake(await connection.InvokeAsync<ServerHandshakeData>(nameof(IMailHub.GetHandshakeData)));
    }

    public virtual Task<SharedFork> GetIdentity()
    {
        return connection.InvokeAsync<SharedFork>(nameof(IMailHub.GetIdentity));
    }

    ///////

    // DON'T FORGET TO IMPLEMENT THIS! Make it so it list all of the data
    // This code is intended for SS14 to hook into (aka implement) and returns the data as ForktierMail's shared models
    public virtual Task<List<SharedPlayer>> ListAllPlayer()
    {
        throw new NotImplementedException();
    }

    // DON'T FORGET TO IMPLEMENT THIS! Make it so it list all of the data
    // This code is intended for SS14 to hook into (aka implement) and returns the data as ForktierMail's shared models
    public virtual Task<List<SharedCharacter>> ListAllCharacter()
    {
        throw new NotImplementedException();
    }

    ////

    public async void AddPlayer(SharedPlayer player)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.AddPlayer), player);

        if (!isAccepted)
            return;

        dataManger.TrackPlayer(player);
    }

    public async Task<bool> AddPlayerAsync(SharedPlayer player)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.AddPlayer), player);

        if (!isAccepted)
            return false;

        dataManger.TrackPlayer(player);
        return true;
    }

    public async void AddCharacter(SharedCharacter character)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.AddCharacter), character);

        if (!isAccepted)
            return;

        dataManger.TrackCharacter(character);
    }

    public async Task<bool> AddCharacterAsync(SharedCharacter character)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.AddCharacter), character);

        if (!isAccepted)
            return false;

        dataManger.TrackCharacter(character);
        return true;
    }

    public async void RemovePlayer(Guid playerId)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.RemovePlayer), playerId);

        if (!isAccepted)
            return;

        dataManger.DeletePlayer(playerId);
    }

    public async void RemoveCharacter(int characterId)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.RemoveCharacter), characterId);

        if (!isAccepted)
            return;

        dataManger.DeleteCharacter(ForkId, characterId);
    }

    public async void UpdatePlayer(SharedPlayer player)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.UpdatePlayer), player);

        if (!isAccepted)
            return;

        dataManger.UpdatePlayer(player.Id, player);
    }

    public async void UpdateCharacter(SharedCharacter character)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.UpdateCharacter), character);

        if (!isAccepted)
            return;

        dataManger.UpdateCharacter(character.ForkId, character.CharacterId, character);
    }

    /////////

    public async Task<bool> SendMail(SharedMail mail)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.SendMail), mail);

        if (!isAccepted)
            return false;

        dataManger.TrackMail(mail);
        return true;
    }

    public async void DeleteMail(int mailId)
    {
        var isAccepted = await connection.InvokeAsync<bool>(nameof(IMailHub.DeleteMail), mailId);

        if (!isAccepted)
            return;

        dataManger.DeleteMail(mailId);
    }

    //////////////////

    public async Task<bool> OnServerHandshake(ServerHandshakeData handshake)
    {
        ForkId = handshake.ForkId;

        foreach (var fork in handshake.Forks)
            dataManger.TrackFork(fork);

        foreach (var player in handshake.Players)
            dataManger.TrackPlayer(player);

        foreach (var character in handshake.Characters)
            dataManger.TrackCharacter(character);

        foreach (var mail in handshake.Mails)
            dataManger.TrackMail(mail);

        var clientHandshakeData = new ClientHandshakeData
        {
            Players = await ListAllPlayer(),
            Characters = await ListAllCharacter()
        };

        return await connection.InvokeAsync<bool>(nameof(IMailHub.SendHandshake), clientHandshakeData);
    }

    // DON'T FORGET TO OVERWRITE THIS!
    // Returns false if you want to reject the mail
    public virtual async Task<bool> OnMailRecieved(SharedMail mail)
    {
        dataManger.TrackMail(mail);
        return await Task.FromResult(true);
    }

    public void OnPlayerAdded(SharedPlayer player)
    {
        dataManger.TrackPlayer(player);
    }

    public void OnCharacterAdded(SharedCharacter character)
    {
        dataManger.TrackCharacter(character);
    }

    public void OnPlayerRemoved(Guid playerId)
    {
        dataManger.DeletePlayer(playerId);
    }

    public void OnCharacterRemoved(int forkId, int characterId)
    {
        dataManger.DeleteCharacter(forkId, characterId);
    }

    public void OnMailUpdated(SharedMail mail)
    {
        dataManger.UpdateMail(mail.Id, mail);
    }

    public void OnPlayerUpdated(SharedPlayer player)
    {
        dataManger.UpdatePlayer(player.Id, player);
    }

    public void OnCharacterUpdated(SharedCharacter character)
    {
        dataManger.UpdateCharacter(character.ForkId, character.CharacterId, character);
    }

    public void OnMailRemoved(int mailId)
    {
        dataManger.DeleteMail(mailId);
    }

    public void OnForkRemoved(int forkId)
    {
        dataManger.DeleteFork(forkId);
    }
}