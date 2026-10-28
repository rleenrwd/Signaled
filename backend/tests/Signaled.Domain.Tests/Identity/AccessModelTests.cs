using Signaled.Domain.Identity;
using Xunit;

namespace Signaled.Domain.Tests.Identity;

public class AccessModelTests
{
    [Fact]
    public void Role_Create_PreservesIdAndTrimsText()
    {
        // ARRANGE
        var id = Guid.NewGuid();

        // ACT
        var role = Role.Create(
            id,
            "  Contributor  ",
            "  Contributor  ");

        // ASSERT
        Assert.Equal(id, role.Id);
        Assert.Equal("Contributor", role.Code);
        Assert.Equal("Contributor", role.Name);
        Assert.True(role.IsActive);
    }

    [Theory]
    [InlineData(PermissionIdentityType.Human)]
    [InlineData(PermissionIdentityType.Service)]
    public void Permission_Create_PreservesIdentityType(
        PermissionIdentityType identityType)
    {
        // ARRANGE
        var id = Guid.NewGuid();

        var code = identityType == PermissionIdentityType.Human
            ? "event.create"
            : "worker.ingestionProcess";

        // ACT
        var permission = Permission.Create(
            id,
            $"  {code}  ",
            "  Test capability  ",
            identityType);

        // ASSERT
        Assert.Equal(id, permission.Id);
        Assert.Equal(code, permission.Code);
        Assert.Equal("Test capability", permission.Description);
        Assert.Equal(identityType, permission.AppliesToIdentityType);
    }

    [Fact]
    public void RolePermission_Create_PreservesRelationship()
    {
        // ARRANGE
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // ACT
        var relationship = RolePermission.Create(roleId, permissionId);

        // ASSERT
        Assert.Equal(roleId, relationship.RoleId);
        Assert.Equal(permissionId, relationship.PermissionId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UserRole_Create_PreservesAssignmentAndNormalizesTime(
        bool hasAssigningUser)
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        Guid? assigningUserId = hasAssigningUser
            ? Guid.NewGuid()
            : null;

        var assignedAt = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-7));

        // ACT
        var assignment = UserRole.Create(
            userId,
            roleId,
            assignedAt,
            assigningUserId);

        // ASSERT
        Assert.Equal(userId, assignment.UserId);
        Assert.Equal(roleId, assignment.RoleId);
        Assert.Equal(assigningUserId, assignment.AssignedByUserId);
        Assert.Equal(assignedAt.ToUniversalTime(), assignment.AssignedAt);
        Assert.Equal(TimeSpan.Zero, assignment.AssignedAt.Offset);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UserPermission_Create_PreservesGrantAndNormalizesTime(
        bool hasGrantingUser)
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        Guid? grantingUserId = hasGrantingUser
            ? Guid.NewGuid()
            : null;

        var grantedAt = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-7));

        // ACT
        var grant = UserPermission.Create(
            userId,
            permissionId,
            grantedAt,
            grantingUserId);

        // ASSERT
        Assert.Equal(userId, grant.UserId);
        Assert.Equal(permissionId, grant.PermissionId);
        Assert.Equal(grantingUserId, grant.GrantedByUserId);
        Assert.Equal(grantedAt.ToUniversalTime(), grant.GrantedAt);
        Assert.Equal(TimeSpan.Zero, grant.GrantedAt.Offset);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Role_Create_WithMissingCode_Throws(string? code)
    {
        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentException>(() =>
            Role.Create(Guid.NewGuid(), code!, "Contributor"));

        Assert.Equal("code", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Role_Create_WithMissingName_Throws(string? name)
    {
        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentException>(() =>
            Role.Create(Guid.NewGuid(), "Contributor", name!));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Permission_Create_WithMissingCode_Throws(string? code)
    {
        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentException>(() =>
            Permission.Create(
                Guid.NewGuid(),
                code!,
                "Create an event.",
                PermissionIdentityType.Human));

        Assert.Equal("code", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Permission_Create_WithMissingDescription_Throws(
        string? description)
    {
        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentException>(() =>
            Permission.Create(
                Guid.NewGuid(),
                "event.create",
                description!,
                PermissionIdentityType.Human));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void Permission_Create_WithUndefinedIdentityType_Throws()
    {
        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Permission.Create(
                Guid.NewGuid(),
                "event.create",
                "Create an event.",
                (PermissionIdentityType)99));

        Assert.Equal("appliesToIdentityType", exception.ParamName);
    }

    [Fact]
    public void Factories_WithEmptyRequiredIds_Throw()
    {
        // ARRANGE
        var validId = Guid.NewGuid();
        var timestamp = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

        // ACT AND ASSERT
        Assert.Throws<ArgumentException>(() =>
            Role.Create(Guid.Empty, "Contributor", "Contributor"));

        Assert.Throws<ArgumentException>(() =>
            Permission.Create(
                Guid.Empty,
                "event.create",
                "Create an event.",
                PermissionIdentityType.Human));

        Assert.Throws<ArgumentException>(() =>
            RolePermission.Create(Guid.Empty, validId));

        Assert.Throws<ArgumentException>(() =>
            RolePermission.Create(validId, Guid.Empty));

        Assert.Throws<ArgumentException>(() =>
            UserRole.Create(Guid.Empty, validId, timestamp, null));

        Assert.Throws<ArgumentException>(() =>
            UserRole.Create(validId, Guid.Empty, timestamp, null));

        Assert.Throws<ArgumentException>(() =>
            UserPermission.Create(Guid.Empty, validId, timestamp, null));

        Assert.Throws<ArgumentException>(() =>
            UserPermission.Create(validId, Guid.Empty, timestamp, null));
    }

    [Fact]
    public void Assignments_WithEmptyActorIds_Throw()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var timestamp = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

        // ACT AND ASSERT
        var roleException = Assert.Throws<ArgumentException>(() =>
            UserRole.Create(userId, targetId, timestamp, Guid.Empty));

        var permissionException = Assert.Throws<ArgumentException>(() =>
            UserPermission.Create(userId, targetId, timestamp, Guid.Empty));

        Assert.Equal("assignedByUserId", roleException.ParamName);
        Assert.Equal("grantedByUserId", permissionException.ParamName);
    }
}