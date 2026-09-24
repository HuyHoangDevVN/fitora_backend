using BuildingBlocks.RepositoryBase.EntityFramework;
using FluentAssertions;
using Moq;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Domain.Enums;
using UserService.Domain.Models;
using UserService.Infrastructure.Repositories;
using Xunit;

namespace UserService.Tests.Repositories;

public class GroupMemberRepositoryTests
{
    private readonly Mock<IRepositoryBase<GroupMember>> _memberRepoMock = new();
    private readonly Mock<IRepositoryBase<Group>> _groupRepoMock = new();
    private readonly GroupMemberRepository _sut;
    private readonly List<GroupMember> _members = new();

    public GroupMemberRepositoryTests()
    {
        _memberRepoMock
            .Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((System.Linq.Expressions.Expression<Func<GroupMember, bool>> expr, CancellationToken _) =>
                Task.FromResult(_members.AsQueryable().FirstOrDefault(expr)));

        _memberRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new GroupMemberRepository(_memberRepoMock.Object, _groupRepoMock.Object);
    }

    private GroupMember AddMember(Guid groupId, Guid userId, GroupRole role)
    {
        var member = new GroupMember { Id = Guid.NewGuid(), GroupId = groupId, UserId = userId, Role = role };
        _members.Add(member);
        return member;
    }

    [Fact]
    public async Task AssignRoleAsync_SelfAssignment_ReturnsFailure()
    {
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var member = AddMember(groupId, userId, GroupRole.Member);

        var result = await _sut.AssignRoleAsync(
            new AssignRoleGroupMemberRequest(userId, groupId, member.Id, GroupRole.Admin));

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("chính mình");
    }

    [Fact]
    public async Task AssignRoleAsync_AssignerNotOwnerOrAdmin_ReturnsFailure()
    {
        var groupId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        AddMember(groupId, moderatorId, GroupRole.Moderator);
        var target = AddMember(groupId, targetId, GroupRole.Member);

        var result = await _sut.AssignRoleAsync(
            new AssignRoleGroupMemberRequest(moderatorId, groupId, target.Id, GroupRole.Moderator));

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("không có quyền");
    }

    [Fact]
    public async Task AssignRoleAsync_AdminAssignsAdminRole_ReturnsFailure()
    {
        var groupId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        AddMember(groupId, adminId, GroupRole.Admin);
        var target = AddMember(groupId, targetId, GroupRole.Member);

        var result = await _sut.AssignRoleAsync(
            new AssignRoleGroupMemberRequest(adminId, groupId, target.Id, GroupRole.Admin));

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Moderator hoặc Member");
    }

    [Fact]
    public async Task AssignRoleAsync_AdminAssignsModeratorRole_Succeeds()
    {
        var groupId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        AddMember(groupId, adminId, GroupRole.Admin);
        var target = AddMember(groupId, targetId, GroupRole.Member);

        var result = await _sut.AssignRoleAsync(
            new AssignRoleGroupMemberRequest(adminId, groupId, target.Id, GroupRole.Moderator));

        result.IsSuccess.Should().BeTrue();
        target.Role.Should().Be(GroupRole.Moderator);
    }

    [Fact]
    public async Task AssignRoleAsync_OwnerAssignsAdminRole_Succeeds()
    {
        var groupId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        AddMember(groupId, ownerId, GroupRole.Owner);
        var target = AddMember(groupId, targetId, GroupRole.Member);

        var result = await _sut.AssignRoleAsync(
            new AssignRoleGroupMemberRequest(ownerId, groupId, target.Id, GroupRole.Admin));

        result.IsSuccess.Should().BeTrue();
        target.Role.Should().Be(GroupRole.Admin);
    }

    [Fact]
    public async Task DeleteAsync_SelfDeletion_Throws()
    {
        var id = Guid.NewGuid();

        var act = () => _sut.DeleteAsync(id, id);

        await act.Should().ThrowAsync<Exception>().WithMessage("*tự xóa*");
    }

    [Fact]
    public async Task DeleteAsync_ModeratorDeletesMember_Throws()
    {
        var groupId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        AddMember(groupId, moderatorId, GroupRole.Moderator);
        AddMember(groupId, targetId, GroupRole.Member);

        var act = () => _sut.DeleteAsync(targetId, moderatorId);

        await act.Should().ThrowAsync<Exception>().WithMessage("*không có quyền*");
    }

    [Fact]
    public async Task DeleteAsync_AdminDeletesAnotherAdmin_Throws()
    {
        var groupId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var targetAdminId = Guid.NewGuid();
        AddMember(groupId, adminId, GroupRole.Admin);
        AddMember(groupId, targetAdminId, GroupRole.Admin);

        var act = () => _sut.DeleteAsync(targetAdminId, adminId);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Owner/Admin khác*");
    }

    [Fact]
    public async Task DeleteAsync_AdminDeletesMember_Succeeds()
    {
        var groupId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        AddMember(groupId, adminId, GroupRole.Admin);
        AddMember(groupId, targetId, GroupRole.Member);

        var result = await _sut.DeleteAsync(targetId, adminId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_OwnerDeletesAdmin_Succeeds()
    {
        var groupId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var targetAdminId = Guid.NewGuid();
        AddMember(groupId, ownerId, GroupRole.Owner);
        AddMember(groupId, targetAdminId, GroupRole.Admin);

        var result = await _sut.DeleteAsync(targetAdminId, ownerId);

        result.Should().BeTrue();
    }
}
