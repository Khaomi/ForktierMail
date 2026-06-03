using ForktierMail.Client;
using ForktierMail.Database;
using ForktierMail.Server;
using ForktierMail.Server.Manager;
using ForktierMail.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MailType = ForktierMail.Shared.Models.MailType;

namespace ForktierMail.Tests;

public class TestServerFactory : WebApplicationFactory<Application>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextFactory<ServerDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddPooledDbContextFactory<ServerDbContext>(opts => { opts.UseSqlite(_connection); });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ServerDbContext>>();
            using var db = dbFactory.CreateDbContext();
            db.Database.EnsureCreated();

            var manager = scope.ServiceProvider.GetRequiredService<ServerDataManager>();
            manager.SeedDevelopment().GetAwaiter().GetResult();
            manager.LoadFromDatabase().GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}

public class TestClient : ForktierMailClient
{
    public TestClient(HubConnection conn) : base(conn)
    {
    }

    public override Task<List<SharedPlayer>> ListAllPlayer()
    {
        return Task.FromResult(new List<SharedPlayer>());
    }

    public override Task<List<SharedCharacter>> ListAllCharacter()
    {
        return Task.FromResult(new List<SharedCharacter>());
    }
}

[TestFixture]
public class ServerIntegrationTests
{
    [SetUp]
    public void Setup()
    {
        _factory = new TestServerFactory();
    }

    [TearDown]
    public void Teardown()
    {
        _factory?.Dispose();
    }

    private TestServerFactory? _factory;

    [Test]
    public async Task Handshake_TracksForks()
    {
        using var handler = _factory!.Server.CreateHandler();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/Mail?apiKey=key1"),
                options => { options.HttpMessageHandlerFactory = _ => handler; })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();

        var client = new TestClient(hubConnection);
        await hubConnection.StartAsync();
        await client.Start();

        Assert.Multiple(() =>
        {
            Assert.That(client.ForkId, Is.EqualTo(1), "Expected ForkId 1");
            Assert.That(client.dataManger.Forks.ContainsKey(1), Is.True, "Fork 1 not tracked by client");
        });
    }

    [Test]
    public async Task AddPlayer_WritesToServerAndClientTracks()
    {
        using var handler = _factory!.Server.CreateHandler();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/Mail?apiKey=key1"),
                options => { options.HttpMessageHandlerFactory = _ => handler; })
            .WithAutomaticReconnect()
            .Build();

        var client = new TestClient(hubConnection);
        await hubConnection.StartAsync();
        await client.Start();

        var player = new SharedPlayer { Id = Guid.NewGuid() };
        var accepted = await client.AddPlayerAsync(player);

        Assert.Multiple(() =>
        {
            Assert.That(accepted, Is.True, "AddPlayer was not accepted by server");
            Assert.That(client.dataManger.Players.ContainsKey(player.Id), Is.True, "Client did not track added player");
        });
    }

    [Test]
    public async Task SendMail_Roundtrip()
    {
        using var handler = _factory!.Server.CreateHandler();
        var hubConnection1 = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/Mail?apiKey=key1"),
                options => { options.HttpMessageHandlerFactory = _ => handler; })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();

        var hubConnection2 = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/Mail?apiKey=key2"),
                options => { options.HttpMessageHandlerFactory = _ => handler; })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();

        var client1 = new TestClient(hubConnection1);
        var client2 = new TestClient(hubConnection2);

        await hubConnection1.StartAsync();
        await hubConnection2.StartAsync();

        await client1.Start();
        await client2.Start();

        var player1 = new SharedPlayer { Id = Guid.NewGuid() };
        var player2 = new SharedPlayer { Id = Guid.NewGuid() };

        await client1.AddPlayerAsync(player1);
        await client2.AddPlayerAsync(player2);

        var char1 = new SharedCharacter { ForkId = 1, Id = 1, CharacterId = 1, PlayerId = player1.Id, Name = "C1" };
        var char2 = new SharedCharacter { ForkId = 2, Id = 1, CharacterId = 1, PlayerId = player2.Id, Name = "C2" };

        var c1ok = await client1.AddCharacterAsync(char1);
        var c2ok = await client2.AddCharacterAsync(char2);

        Assert.That(c1ok && c2ok, Is.True, "Adding characters failed on one or both clients");

        var mail = new SharedMail
        {
            Id = 1,
            Content = "Hello",
            MailType = MailType.LETTER,
            SenderForkId = 1,
            SenderId = 1,
            RecipientForkId = 2,
            RecipientId = 1
        };

        var sent = await client1.SendMail(mail);
        Assert.Multiple(() =>
        {
            Assert.That(sent, Is.True, "SendMail was rejected by server");
            Assert.That(client2.dataManger.Mails.ContainsKey(mail.Id), Is.True, "Recipient did not receive mail");
        });
    }
}