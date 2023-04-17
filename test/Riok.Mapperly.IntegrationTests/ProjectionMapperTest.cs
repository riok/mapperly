using System;
using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using VerifyXunit;
using Xunit;
#if NET7_0_OR_GREATER
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
#endif

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    public class ProjectionMapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(ProjectionMapper));
            return Verifier.VerifyFile(path);
        }

#if NET7_0_OR_GREATER
        [Fact]
        public async Task ProjectionShouldTranslateToQuery()
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;

            await using var ctx = new ProjectionDbContext(options);
            await ctx.Database.EnsureCreatedAsync();
            ctx.Objects.Add(CreateObject());
            await ctx.SaveChangesAsync();

            var query = ctx.Objects.ProjectToDto();
            await Verifier
                .Verify(query.ToQueryString(), "sql")
                .UseTextForParameters("query");

            var objects = await query.ToListAsync();
            await Verifier
                .Verify(objects)
                .UseTextForParameters("result");
        }

        private TestObjectProjection CreateObject()
        {
            return new TestObjectProjection
            {
                RequiredValue = 10,
                EnumName = TestEnum.Value10,
                EnumReverseStringValue = nameof(TestEnum.Value10),
                EnumValue = TestEnum.Value20,
                IntValue = 100,
                EnumRawValue = TestEnum.Value30,
                EnumStringValue = TestEnum.Value10,
                Flattening = new IdObject { IdValue = 10 },
                CtorValue = 2,
                IgnoredIntValue = 3,
                IntInitOnlyValue = 4,
                StringNullableTargetNotNullable = "fooBar",
                DateTimeValueTargetTimeOnly = new DateTime(2018, 11, 29, 10, 11, 12),
                DateTimeValueTargetDateOnly = new DateTime(2018, 11, 29, 10, 11, 12),
                RenamedStringValue = "fooBar2",
                UnflatteningIdValue = 7,
                StringValue = "fooBar3",
                IgnoredStringValue = "fooBar4",
                NullableFlattening = new IdObject { IdValue = 20 },
                SubObject = new InheritanceSubObject
                {
                    BaseIntValue = 10,
                    SubIntValue = 20,
                },
                RecursiveObject = new TestObjectProjection
                {
                    RequiredValue = -1,
                    EnumName = TestEnum.Value10,
                    EnumReverseStringValue = nameof(TestEnum.Value10),
                    EnumValue = TestEnum.Value20,
                },
            };
        }

        class ProjectionDbContext : DbContext
        {
            public ProjectionDbContext(DbContextOptions options) : base(options)
            {
            }

            public DbSet<TestObjectProjection> Objects { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TestObjectProjection>().HasKey(p => p.IntValue);
                modelBuilder.Entity<TestObjectProjection>().HasOne(p => p.RecursiveObject);
                modelBuilder.Entity<TestObjectProjection>().HasOne(p => p.SubObject);

                modelBuilder.Entity<IdObject>().HasKey(p => p.IdValue);
                modelBuilder.Entity<InheritanceSubObject>().HasKey(p => p.SubIntValue);
                modelBuilder.Entity<TestObjectNested>().HasKey(p => p.IntValue);
            }
        }
#endif
    }
}
