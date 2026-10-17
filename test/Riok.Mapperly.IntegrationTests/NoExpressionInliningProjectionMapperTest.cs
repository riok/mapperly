#if NET7_0_OR_GREATER
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class NoExpressionInliningProjectionMapperTest
    {
        [Fact]
        public async Task ProjectionWithNoExpressionInliningEvaluatesEnumMappingOnClient()
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder().UseSqlite(connection).Options;
            await using var ctx = new NoExpressionInliningProjectionDbContext(options);
            await ctx.Database.EnsureCreatedAsync();
            ctx.Sources.Add(new NoExpressionInliningProjectionSource { Id = 1, Status = NoExpressionInliningProjectionStatus.Active });
            await ctx.SaveChangesAsync();

            var query = ctx.Sources.ProjectToDto();

            // the enum mapping method is invoked on the client, only the raw column is selected
            var sql = query.ToQueryString();
            sql.ShouldContain("\"Status\"");
            sql.ShouldNotContain("CAST");

            // mapped by name, the member values differ between the enums
            var dto = (await query.ToListAsync()).ShouldHaveSingleItem();
            dto.Id.ShouldBe(1);
            dto.Status.ShouldBe(NoExpressionInliningProjectionStatusDto.Active);
        }

        sealed class NoExpressionInliningProjectionDbContext : DbContext
        {
            public NoExpressionInliningProjectionDbContext(DbContextOptions options)
                : base(options) { }

            public DbSet<NoExpressionInliningProjectionSource> Sources { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<NoExpressionInliningProjectionSource>().HasKey(p => p.Id);
            }
        }
    }
}
#endif
