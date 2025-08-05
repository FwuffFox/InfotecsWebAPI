using Microsoft.EntityFrameworkCore;
using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Data;

/// <summary>
/// Database context for TimescaleDB operations.
/// </summary>
public class TimescaleDbContext : DbContext
{
    public TimescaleDbContext(DbContextOptions<TimescaleDbContext> options) : base(options)
    {
    }

    public DbSet<ValueEntity> Values { get; set; }
    public DbSet<ResultEntity> Results { get; set; }
}
