#if NET7_0_OR_GREATER
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    // Regression test for https://github.com/riok/mapperly/issues/2340: a queryable projection that
    // inlines a Use= helper whose parameter is a supertype (interface or base class) of the source
    // element type must emit a parenthesized upcast (`((I)x).Value`). Failing to compile or to
    // translate both reproduce the bug.
    public class SupertypeProjectionMapperTest
    {
        [Fact]
        public async Task ProjectionWithSupertypeUseMappingTranslatesToRenamedColumns()
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder().UseSqlite(connection).Options;
            await using var ctx = new SupertypeProjectionDbContext(options);
            await ctx.Database.EnsureCreatedAsync();
            ctx.Sources.Add(
                new SupertypeProjectionSource
                {
                    Id = 1,
                    Value = 42,
                    Name = "foo",
                }
            );
            await ctx.SaveChangesAsync();

            var query = ctx.Sources.ProjectToDto();

            // The inserted upcast must resolve each member to its renamed backing column, proving the cast
            // is bound to the mapped property and not to a coincidentally matching name.
            var sql = query.ToQueryString();
            sql.ShouldContain("raw_value");
            sql.ShouldContain("entity_name");

            var dto = (await query.ToListAsync()).ShouldHaveSingleItem();
            dto.Id.ShouldBe(1);
            dto.MappedValue.ShouldBe(42);
            dto.MappedName.ShouldBe("foo");
        }

        sealed class SupertypeProjectionDbContext : DbContext
        {
            public SupertypeProjectionDbContext(DbContextOptions options)
                : base(options) { }

            public DbSet<SupertypeProjectionSource> Sources { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var entity = modelBuilder.Entity<SupertypeProjectionSource>();
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Value).HasColumnName("raw_value");
                entity.Property(p => p.Name).HasColumnName("entity_name");
            }
        }
    }
}
#endif
