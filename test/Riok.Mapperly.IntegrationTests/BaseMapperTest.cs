using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Models;
using VerifyTests;
using VerifyTests.DiffPlex;
using VerifyXunit;

namespace Riok.Mapperly.IntegrationTests
{
    public abstract class BaseMapperTest
    {
        private static readonly string _solutionDirectory = GetSolutionDirectory();
        private static readonly string _projectDirectory = GetProjectDirectory();

        static BaseMapperTest()
        {
#if !NET6_0_OR_GREATER
            VerifierSettings.AddExtraSettings(settings =>
            {
                settings.Converters.Add(new PortableDateOnlyConverter());
                settings.Converters.Add(new PortableTimeOnlyConverter());
            });
#endif

            VerifierSettings.DontScrubDateTimes();
            VerifyDiffPlex.Initialize(OutputType.Compact);

            Verifier.DerivePathInfo(
                (_, _, type, method) =>
                    new PathInfo(
                        Path.Combine(_projectDirectory, "_snapshots"),
                        type.Name,
                        method.Name + GetSnapshotVersionSuffix(type, method)
                    )
            );
        }

        protected string GetGeneratedMapperFilePath(string name)
        {
#if NET8_0_OR_GREATER || NET48_OR_GREATER
            // artifacts output
            return Path.Combine(
                _solutionDirectory,
                "artifacts",
                "obj",
                "Riok.Mapperly.IntegrationTests",
                "generated",
                "Riok.Mapperly",
                "Riok.Mapperly.MapperGenerator",
                name + ".g.cs"
            );
#else
            return Path.Combine(
                _solutionDirectory,
                "test",
                "Riok.Mapperly.IntegrationTests",
                "obj",
                "generated",
                "Riok.Mapperly",
                "Riok.Mapperly.MapperGenerator",
                name + ".g.cs"
            );
#endif
        }

        public static TestObject NewTestObj()
        {
            return new TestObject(7)
            {
                IntValue = 10,
                EnumName = TestEnum.Value10,
                EnumValue = TestEnum.Value10,
                FlagsEnumValue = TestFlagsEnum.V1 | TestFlagsEnum.V4,
                IntInitOnlyValue = 3,
                RequiredValue = 4,
                NestedNullable = new TestObjectNested { IntValue = 100 },
                StringValue = "fooBar",
                SubObject = new InheritanceSubObject { BaseIntValue = 1, SubIntValue = 2 },
                EnumRawValue = TestEnum.Value20,
                EnumStringValue = TestEnum.Value30,
                DateTimeValue = new DateTime(2020, 1, 3, 15, 10, 5, DateTimeKind.Utc),
                DateTimeValueTargetDateOnly = new DateTime(2020, 1, 3, 15, 10, 5, DateTimeKind.Utc),
                DateTimeValueTargetTimeOnly = new DateTime(2020, 1, 3, 15, 10, 5, DateTimeKind.Utc),
                IgnoredStringValue = "ignored",
                RenamedStringValue = "fooBar2",
                StringNullableTargetNotNullable = "fooBar3",
                EnumReverseStringValue = nameof(TestEnumDtoByValue.DtoValue3),
                NestedNullableTargetNotNullable = new(),
                Flattening = new() { IdValue = 10 },
                NullableFlattening = new() { IdValue = 100 },
                UnflatteningIdValue = 20,
                NullableUnflatteningIdValue = 200,
                NestedMember = new()
                {
                    NestedMemberId = 12,
                    NestedMemberObject = new() { IntValue = 22 },
                },
                TupleValue = ("10", "20"),
                RecursiveObject = new(5)
                {
                    EnumValue = TestEnum.Value10,
                    EnumName = TestEnum.Value30,
                    EnumReverseStringValue = nameof(TestEnumDtoByValue.DtoValue3),
                    RequiredValue = 4,
                },
                NullableReadOnlyObjectCollection = new[]
                {
                    new TestObjectNested { IntValue = 10 },
                    new TestObjectNested { IntValue = 20 },
                },
                SourceTargetSameObjectType = new TestObject(8)
                {
                    IntValue = 99,
                    RequiredValue = 98,
                    NestedMember = new()
                    {
                        NestedMemberId = 123,
                        NestedMemberObject = new() { IntValue = 223 },
                    },
                },
                MemoryValue = new[] { "1", "2", "3" },
                StackValue = new Stack<string>(new[] { "1", "2", "3" }),
                QueueValue = new Queue<string>(new[] { "1", "2", "3" }),
                ImmutableArrayValue = ImmutableArray.Create("1", "2", "3"),
                ImmutableListValue = ImmutableList.Create("1", "2", "3"),
                ImmutableHashSetValue = ImmutableHashSet.Create("1", "2", "3"),
                ImmutableQueueValue = ImmutableQueue.Create("1", "2", "3"),
                ImmutableStackValue = ImmutableStack.Create("1", "2", "3"),
                ImmutableSortedSetValue = ImmutableSortedSet.Create("1", "2", "3"),
                ImmutableDictionaryValue = new Dictionary<string, string>()
                {
                    { "1", "1" },
                    { "2", "2" },
                    { "3", "3" },
                }.ToImmutableDictionary(),
                ImmutableSortedDictionaryValue = new Dictionary<string, string>()
                {
                    { "1", "1" },
                    { "2", "2" },
                    { "3", "3" },
                }.ToImmutableSortedDictionary(),
                ExistingISet = { "1", "2", "3" },
                ExistingHashSet = { "1", "2", "3" },
                ExistingSortedSet = { "1", "2", "3" },
                ExistingList = { "1", "2", "3" },
                ISet = new HashSet<string> { "1", "2", "3" },
#if NET5_0_OR_GREATER
                IReadOnlySet = new HashSet<string> { "1", "2", "3" },
#endif
                HashSet = new HashSet<string> { "1", "2", "3" },
                SortedSet = new SortedSet<string> { "1", "2", "3" },
                SumComponent1 = 32,
                SumComponent2 = 64,
            };
        }

        /// <summary>
        /// Gets the version of the snapshot.
        /// If the test is not a <see cref="VersionedSnapshotAttribute"/>, empty string is returned.
        /// Otherwise either the current version, or the latest version of the <see cref="VersionedSnapshotAttribute"/> prefixed with a _ is returned.
        /// </summary>
        /// <param name="type">The type of the test method.</param>
        /// <param name="method">The test method.</param>
        /// <returns>Either an empty string or the name of the version.</returns>
        private static string GetSnapshotVersionSuffix(Type type, MethodInfo method)
        {
            var versionedSnapshot =
                method.GetCustomAttribute<VersionedSnapshotAttribute>() ?? type.GetCustomAttribute<VersionedSnapshotAttribute>();
            if (versionedSnapshot == null)
                return string.Empty;

            var currentVersion = GetCurrentVersion();
            if (versionedSnapshot.VersionsWithChanges.HasFlag(currentVersion))
                return "_" + currentVersion;

            var supportedVersions = Enum.GetValues(typeof(Versions))
                .Cast<Versions>()
                .Where(x => x < currentVersion)
                .OrderByDescending(x => x);
            foreach (var supportedVersion in supportedVersions)
            {
                if (versionedSnapshot.VersionsWithChanges.HasFlag(supportedVersion))
                    return "_" + supportedVersion;
            }

            return string.Empty;
        }

        private static Versions GetCurrentVersion()
        {
#if NET9_0_OR_GREATER
            return Versions.NET9_0;
#elif NET8_0_OR_GREATER
            return Versions.NET8_0;
#elif NET7_0_OR_GREATER
            return Versions.NET7_0;
#elif NET6_0_OR_GREATER
            return Versions.NET6_0;
#elif NET48_OR_GREATER
            return Versions.NETFRAMEWORK4_8;
#else
            throw new InvalidOperationException("Target framework is not supported");
#endif
        }

        private static string GetProjectDirectory() => FindDirectoryOfFile(".csproj");

        private static string GetSolutionDirectory() => FindDirectoryOfFile(".sln");

        private static string FindDirectoryOfFile(string fileExtension, [CallerFilePath] string baseFilePath = "")
        {
            var dir =
                Path.GetDirectoryName(baseFilePath) ?? throw new InvalidOperationException($"Could not get directory from {baseFilePath}");

            while (Directory.GetFiles(dir, "*" + fileExtension, SearchOption.TopDirectoryOnly).Length == 0)
            {
                dir = Path.GetDirectoryName(dir);
                if (dir == null)
                {
                    throw new InvalidOperationException($"Could not find directory from file {baseFilePath}");
                }
            }

            return dir;
        }
    }
}
