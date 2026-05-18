using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyCollectionThroughTest
{
    [Fact]
    public void CollectionThroughWithICollection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            [MapProperty("UserRoles.Role", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.ICollection<UserRole> UserRoles { get; set; } = new System.Collections.Generic.List<UserRole>(); }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToUserDto",
                """
                var target = new global::UserDto();
                target.Roles = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserRoles, x1 => x1.Role),
                        x => MapToRoleDto(x)
                    )
                );
                return target;
                """
            );
    }

    [Fact]
    public void CollectionThroughWithList()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            [MapProperty("UserRoles.Role", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.List<UserRole> UserRoles { get; set; } = []; }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToUserDto",
                """
                var target = new global::UserDto();
                target.Roles = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserRoles, x1 => x1.Role),
                        x => MapToRoleDto(x)
                    )
                );
                return target;
                """
            );
    }

    [Fact]
    public void CollectionThroughWithIEnumerable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            [MapProperty("UserRoles.Role", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.IEnumerable<UserRole> UserRoles { get; set; } = System.Linq.Enumerable.Empty<UserRole>(); }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToUserDto",
                """
                var target = new global::UserDto();
                target.Roles = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserRoles, x1 => x1.Role),
                        x => MapToRoleDto(x)
                    )
                );
                return target;
                """
            );
    }

    [Fact]
    public void CollectionThroughWithArray()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            [MapProperty("UserRoles.Role", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public UserRole[] UserRoles { get; set; } = []; }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToUserDto",
                """
                var target = new global::UserDto();
                target.Roles = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserRoles, x1 => x1.Role),
                        x => MapToRoleDto(x)
                    )
                );
                return target;
                """
            );
    }

    [Fact]
    public void CollectionThroughWithInterpolatedNameOf()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            [MapProperty($"{nameof(User.UserRoles)}.{nameof(UserRole.Role)}", nameof(UserDto.Roles))]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.ICollection<UserRole> UserRoles { get; set; } = new System.Collections.Generic.List<UserRole>(); }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToUserDto",
                """
                var target = new global::UserDto();
                target.Roles = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserRoles, x1 => x1.Role),
                        x => MapToRoleDto(x)
                    )
                );
                return target;
                """
            );
    }

    [Fact]
    public void CollectionThroughWithNullableCollection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            [MapProperty("UserRoles.Role", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.ICollection<UserRole>? UserRoles { get; set; } }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property UserRoles.[].Role of User to the target property Roles of UserDto which is not nullable"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type Role? to target of type RoleDto which is not nullable"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void CollectionThroughMultiplePropertiesSameMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial RoleDto MapToRoleDto(Role src);
            public static partial TagDto MapToTagDto(Tag src);
            [MapProperty("UserRoles.Role", "Roles")]
            [MapProperty("UserTags.Tag", "Tags")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.ICollection<UserRole> UserRoles { get; set; } = new System.Collections.Generic.List<UserRole>(); public System.Collections.Generic.ICollection<UserTag> UserTags { get; set; } = new System.Collections.Generic.List<UserTag>(); }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class UserTag { public Tag Tag { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class Tag { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; public System.Collections.Generic.List<TagDto> Tags { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }",
            "class TagDto { public string Name { get; set; } = \"\"; }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToUserDto",
                """
                var target = new global::UserDto();
                target.Roles = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserRoles, x1 => x1.Role),
                        x => MapToRoleDto(x)
                    )
                );
                target.Tags = global::System.Linq.Enumerable.ToList(
                    global::System.Linq.Enumerable.Select(
                        global::System.Linq.Enumerable.Select(src.UserTags, x1 => x1.Tag),
                        x => MapToTagDto(x)
                    )
                );
                return target;
                """
            );
    }

    [Fact]
    public void CollectionThroughNonExistentMemberOnElementShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("UserRoles.NonExistent", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.ICollection<UserRole> UserRoles { get; set; } = new System.Collections.Generic.List<UserRole>();}",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingSourceMemberNotFound,
                "Specified member UserRoles.NonExistent on source type User was not found"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member UserRoles on the mapping source type User is not mapped to any member on the mapping target type UserDto"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member Roles on the mapping target type UserDto was not found on the mapping source type User"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void CollectionThroughNonCollectionMemberShouldNotApply()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"Address.City\", \"City\")] public static partial BDto Map(A source);",
            "class A { public Address Address { get; set; } = null!; }",
            "class Address { public string City { get; set; } = \"\"; }",
            "class BDto { public string City { get; set; } = \"\"; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::BDto();
                target.City = source.Address.City;
                return target;
                """
            );
    }

    [Fact]
    public Task CollectionThroughQueryableProjection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial System.Linq.IQueryable<UserDto> ProjectToUserDto(System.Linq.IQueryable<User> src);
            [MapProperty("UserRoles.Role", "Roles")]
            public static partial UserDto MapToUserDto(User src);
            """,
            "class User { public System.Collections.Generic.ICollection<UserRole> UserRoles { get; set; } = new System.Collections.Generic.List<UserRole>(); }",
            "class UserRole { public Role Role { get; set; } = null!; }",
            "class Role { public string Name { get; set; } = \"\"; }",
            "class UserDto { public System.Collections.Generic.List<RoleDto> Roles { get; set; } = []; }",
            "class RoleDto { public string Name { get; set; } = \"\"; }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
