/*
MIT License

Copyright (c) 2017-2026 Space Wizards Federation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// ForktierMail: This broke without this

#define TOOLS

#if TOOLS

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SQLitePCL;

// ReSharper disable UnusedType.Global

namespace ForktierMail.Database;

public sealed class PostgresServerDbContext : ServerDbContext
{
    public PostgresServerDbContext(DbContextOptions options) : base(options)
    {
    }
}


public sealed class SqliteServerDbContext : ServerDbContext
{
    public SqliteServerDbContext(DbContextOptions options) : base(options)
    {
    }
}

public sealed class DesignTimeContextFactoryPostgres : IDesignTimeDbContextFactory<PostgresServerDbContext>
{
    public PostgresServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgresServerDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost");
        return new PostgresServerDbContext(optionsBuilder.Options);
    }
}

public sealed class DesignTimeContextFactorySqlite : IDesignTimeDbContextFactory<SqliteServerDbContext>
{
    public SqliteServerDbContext CreateDbContext(string[] args)
    {
#if !USE_SYSTEM_SQLITE
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
#endif

        var optionsBuilder = new DbContextOptionsBuilder<SqliteServerDbContext>();
        optionsBuilder.UseSqlite("Data Source=:memory:");
        return new SqliteServerDbContext(optionsBuilder.Options);
    }
}

#endif