using Microsoft.EntityFrameworkCore;

namespace ForktierMail.Database;

public class ServerDbContext : DbContext
{
    public ServerDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Fork> Forks { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Mail> Mails { get; set; }

    public override int SaveChanges()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            if (entry.Entity is Fork or Player or Character or Mail)
            {
                entry.Property("UpdatedAt").CurrentValue = now;

                if (entry.State == EntityState.Added)
                    entry.Property("CreatedAt").CurrentValue = now;
            }

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            if (entry.Entity is Fork or Player or Character or Mail)
            {
                entry.Property("UpdatedAt").CurrentValue = now;

                if (entry.State == EntityState.Added)
                    entry.Property("CreatedAt").CurrentValue = now;
            }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fork>(e =>
        {
            e.HasKey(f => f.Id);

            e.Property(f => f.Name)
                .HasDefaultValue("")
                .IsRequired();

            e.Property(f => f.ApiKey)
                .IsRequired();

            e.HasIndex(f => f.Name).IsUnique();
            e.HasIndex(f => f.ApiKey).IsUnique();
        });

        modelBuilder.Entity<Player>(e => { e.HasKey(p => p.Id); });

        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(c => c.Id);

            e.HasIndex(c => new { c.ForkId, c.CharacterId }).IsUnique();

            e.Property(c => c.Name)
                .HasDefaultValue("")
                .IsRequired();

            e.HasOne(c => c.Player)
                .WithMany(p => p.Characters)
                .HasForeignKey(c => c.PlayerId)
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Fork)
                .WithMany(f => f.Characters)
                .HasForeignKey(c => c.ForkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Mail>(e =>
        {
            e.HasKey(m => m.Id);

            e.Property(m => m.Type)
                .IsRequired()
                .HasDefaultValue(MailType.LETTER)
                .HasSentinel(MailType.UNKNOWN);

            e.Property(m => m.Content)
                .IsRequired()
                .HasDefaultValue("");

            e.HasIndex(m => new { m.SenderForkId, m.SenderId });
            e.HasIndex(m => new { m.RecipientForkId, m.RecipientId });
        });

        var isPostgres = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

        if (isPostgres) OnPostgresModelCreating(modelBuilder);
        else OnSqliteModelCreating(modelBuilder);
    }

    protected void OnPostgresModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(e => { e.HasKey(c => new { c.ForkId, c.CharacterId }); });

        modelBuilder.Entity<Mail>(e =>
        {
            e.HasOne(m => m.Sender)
                .WithMany(c => c.SentMails)
                .HasForeignKey(m => new { m.SenderForkId, m.SenderId })
                .HasPrincipalKey(c => new { c.ForkId, c.CharacterId })
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Recipient)
                .WithMany(c => c.ReceivedMails)
                .HasForeignKey(m => new { m.RecipientForkId, m.RecipientId })
                .HasPrincipalKey(c => new { c.ForkId, c.CharacterId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnSqliteModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => new { c.ForkId, c.CharacterId }).IsUnique();
        });

        modelBuilder.Entity<Mail>(e =>
        {
            e.HasOne(m => m.Sender)
                .WithMany(c => c.SentMails)
                .HasForeignKey(m => new { m.SenderForkId, m.SenderId })
                .HasPrincipalKey(c => new { c.ForkId, c.CharacterId })
                .OnDelete(DeleteBehavior.ClientCascade);

            e.HasOne(m => m.Recipient)
                .WithMany(c => c.ReceivedMails)
                .HasForeignKey(m => new { m.RecipientForkId, m.RecipientId })
                .HasPrincipalKey(c => new { c.ForkId, c.CharacterId })
                .OnDelete(DeleteBehavior.ClientCascade);
        });
    }
}

public enum MailType
{
    UNKNOWN = 0,
    LETTER = 1,
    PACKAGE = 2
}

public class Fork
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required string ApiKey { get; set; }

    public List<Character> Characters { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Player
{
    public Guid Id { get; set; }

    public List<Character> Characters { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Character
{
    /// <summary>
    ///     DO NOT USE THIS!
    ///     THIS IS USED TO BYPASS SQLITE LIMITATION
    /// </summary>
    public int Id { get; set; }

    // Index of <ForkId, CharacterId> is used on Postgres

    public required int ForkId { get; set; }
    public Fork Fork { get; set; } = null!;

    public required Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public required int CharacterId { get; set; }

    public string Name { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Mail> SentMails { get; set; } = [];
    public List<Mail> ReceivedMails { get; set; } = [];
}

public class Mail
{
    public int Id { get; set; }

    public MailType Type { get; set; } = MailType.LETTER;
    public string Content { get; set; } = "";

    public int SenderForkId { get; set; }
    public int SenderId { get; set; }
    public Character Sender { get; set; } = null!;

    public int RecipientForkId { get; set; }
    public int RecipientId { get; set; }
    public Character Recipient { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}