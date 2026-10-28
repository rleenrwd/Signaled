using Microsoft.EntityFrameworkCore;
using Signaled.Domain.Identity;
using Signaled.Infrastructure.Persistence;
using Xunit;

namespace Signaled.Infrastructure.Tests.Identity;

public class AccessPersistenceTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public AccessPersistenceTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AccessRelationships_CanBeSavedAndLoaded()
    {
        // ARRANGE
        var timestamp = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

        var suffix = Guid.NewGuid().ToString("N");

        var administrator = UserAccount.Create(
            $"admin-{suffix}",
            "Test Administrator",
            timestamp);

        var user = UserAccount.Create(
            $"user-{suffix}",
            "Test Investigator",
            timestamp);

        var role = Role.Create(
            Guid.NewGuid(),
            $"Investigator-{suffix}",
            "Investigator");

        var roleCapability = Permission.Create(
            Guid.NewGuid(),
            $"case.readAll-{suffix}",
            "Read cases.",
            PermissionIdentityType.Human);

        var directCapability = Permission.Create(
            Guid.NewGuid(),
            $"case.noteCorrect-{suffix}",
            "Correct investigation notes.",
            PermissionIdentityType.Human);

        var userRole = UserRole.Create(
            user.Id,
            role.Id,
            timestamp,
            administrator.Id);

        var rolePermission = RolePermission.Create(
            role.Id,
            roleCapability.Id);

        var userPermission = UserPermission.Create(
            user.Id,
            directCapability.Id,
            timestamp,
            administrator.Id);

        // ACT: SAVE PRINCIPALS AND RELATIONSHIPS
        await using (var writeContext = _fixture.CreateContext())
        {
            writeContext.UserAccounts.AddRange(administrator, user);
            writeContext.Roles.Add(role);
            writeContext.Permissions.AddRange(
                roleCapability,
                directCapability);

            writeContext.UserRoles.Add(userRole);
            writeContext.RolePermissions.Add(rolePermission);
            writeContext.UserPermissions.Add(userPermission);

            await writeContext.SaveChangesAsync();
        }

        // READ USING A SEPARATE CONTEXT
        await using var readContext = _fixture.CreateContext();

        var savedAssignment = await readContext.UserRoles.SingleAsync(
            assignment =>
                assignment.UserId == user.Id &&
                assignment.RoleId == role.Id);

        var savedRolePermission = await readContext.RolePermissions.SingleAsync(
            link =>
                link.RoleId == role.Id &&
                link.PermissionId == roleCapability.Id);

        var savedGrant = await readContext.UserPermissions.SingleAsync(
            grant =>
                grant.UserId == user.Id &&
                grant.PermissionId == directCapability.Id);

        // ASSERT
        Assert.Equal(administrator.Id, savedAssignment.AssignedByUserId);
        Assert.Equal(timestamp, savedAssignment.AssignedAt);

        Assert.Equal(role.Id, savedRolePermission.RoleId);
        Assert.Equal(roleCapability.Id, savedRolePermission.PermissionId);

        Assert.Equal(administrator.Id, savedGrant.GrantedByUserId);
        Assert.Equal(timestamp, savedGrant.GrantedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveChanges_WithDuplicateCode_Throws(bool testRole)
    {
        // ARRANGE
        var code = $"duplicate-{Guid.NewGuid():N}";

        await using (var firstContext = _fixture.CreateContext())
        {
            if (testRole)
            {
                firstContext.Roles.Add(
                    Role.Create(Guid.NewGuid(), code, "Test role"));
            }
            else
            {
                firstContext.Permissions.Add(
                    Permission.Create(
                        Guid.NewGuid(),
                        code,
                        "Test capability.",
                        PermissionIdentityType.Human));
            }

            await firstContext.SaveChangesAsync();
        }

        await using var secondContext = _fixture.CreateContext();

        if (testRole)
        {
            secondContext.Roles.Add(
                Role.Create(Guid.NewGuid(), code, "Another role"));
        }
        else
        {
            secondContext.Permissions.Add(
                Permission.Create(
                    Guid.NewGuid(),
                    code,
                    "Another capability.",
                    PermissionIdentityType.Human));
        }

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            secondContext.SaveChangesAsync());
    }

    [Theory]
    [InlineData("RolePermission")]
    [InlineData("UserRole")]
    [InlineData("UserPermission")]
    public async Task SaveChanges_WithMissingReferencedRecords_Throws(
        string relationshipType)
    {
        // ARRANGE
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        var timestamp = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

        await using var context = _fixture.CreateContext();

        switch (relationshipType)
        {
            case "RolePermission":
                context.RolePermissions.Add(
                    RolePermission.Create(firstId, secondId));
                break;

            case "UserRole":
                context.UserRoles.Add(
                    UserRole.Create(firstId, secondId, timestamp, null));
                break;

            case "UserPermission":
                context.UserPermissions.Add(
                    UserPermission.Create(firstId, secondId, timestamp, null));
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(relationshipType));
        }

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());
    }

    [Theory]
    [InlineData("RolePermission")]
    [InlineData("UserRole")]
    [InlineData("UserPermission")]
    public async Task SaveChanges_WithDuplicateRelationship_Throws(
        string relationshipType)
    {
        // ARRANGE
        var timestamp = new DateTimeOffset(
            2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

        var suffix = Guid.NewGuid().ToString("N");

        var user = UserAccount.Create(
            $"user-{suffix}",
            "Test Investigator",
            timestamp);

        var role = Role.Create(
            Guid.NewGuid(),
            $"Investigator-{suffix}",
            "Investigator");

        var permission = Permission.Create(
            Guid.NewGuid(),
            $"case.noteCorrect-{suffix}",
            "Correct investigation notes.",
            PermissionIdentityType.Human);

        void AddRelationship(SignaledDbContext context)
        {
            switch (relationshipType)
            {
                case "RolePermission":
                    context.RolePermissions.Add(
                        RolePermission.Create(role.Id, permission.Id));
                    break;

                case "UserRole":
                    context.UserRoles.Add(
                        UserRole.Create(user.Id, role.Id, timestamp, null));
                    break;

                case "UserPermission":
                    context.UserPermissions.Add(
                        UserPermission.Create(
                            user.Id,
                            permission.Id,
                            timestamp,
                            null));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(relationshipType));
            }
        }

        // SAVE VALID PRINCIPALS AND THE FIRST RELATIONSHIP
        await using (var firstContext = _fixture.CreateContext())
        {
            firstContext.UserAccounts.Add(user);
            firstContext.Roles.Add(role);
            firstContext.Permissions.Add(permission);

            AddRelationship(firstContext);

            await firstContext.SaveChangesAsync();
        }

        // ADD THE SAME PAIR THROUGH A DIFFERENT CONTEXT
        await using var secondContext = _fixture.CreateContext();

        AddRelationship(secondContext);

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            secondContext.SaveChangesAsync());
    }
}