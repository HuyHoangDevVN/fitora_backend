using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Responses;
using BuildingBlocks.Security;
using FluentAssertions;
using Moq;
using UserService.Application.Usecases.Group.Commands.TransferOwner;
using UserService.Domain.Enums;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.Usecases.Group;

public class TransferOwnerHandlerTests
{
    private readonly Mock<IRepositoryBase<GroupMember>> _memberRepo = new();
    private readonly Mock<IAuthorizeExtension> _auth = new();
    private readonly Guid _groupId = Guid.NewGuid();
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _newOwnerId = Guid.NewGuid();

    private TransferOwnerHandler CreateHandler() => new(_memberRepo.Object, _auth.Object);

    [Fact]
    public async Task Handle_WhenCallerIsNotOwner_ThrowsUnAuthorizationException()
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_ownerId, "owner", "Owner"));
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupMember { GroupId = _groupId, UserId = _ownerId, Role = GroupRole.Admin });

        var handler = CreateHandler();
        var act = () => handler.Handle(new TransferOwnerCommand(_groupId, _newOwnerId), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizationException>();
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotMember_ThrowsUnAuthorizationException()
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_ownerId, "owner", "Owner"));
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new TransferOwnerCommand(_groupId, _newOwnerId), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizationException>();
    }

    [Fact]
    public async Task Handle_WhenNewOwnerIsSameAsCaller_ReturnsFailureWithoutChanges()
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_ownerId, "owner", "Owner"));
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupMember { GroupId = _groupId, UserId = _ownerId, Role = GroupRole.Owner });

        var handler = CreateHandler();
        var result = await handler.Handle(new TransferOwnerCommand(_groupId, _ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _memberRepo.Verify(r => r.UpdateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNewOwnerIsNotMember_ThrowsNotFoundException()
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_ownerId, "owner", "Owner"));
        _memberRepo.SetupSequence(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupMember { GroupId = _groupId, UserId = _ownerId, Role = GroupRole.Owner })
            .ReturnsAsync((GroupMember?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new TransferOwnerCommand(_groupId, _newOwnerId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_TransfersOwnershipAndDowngradesCallerToAdmin()
    {
        var callerMembership = new GroupMember { GroupId = _groupId, UserId = _ownerId, Role = GroupRole.Owner };
        var newOwnerMembership = new GroupMember { GroupId = _groupId, UserId = _newOwnerId, Role = GroupRole.Member };

        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_ownerId, "owner", "Owner"));
        _memberRepo.SetupSequence(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(callerMembership)
            .ReturnsAsync(newOwnerMembership);
        _memberRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(new TransferOwnerCommand(_groupId, _newOwnerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        callerMembership.Role.Should().Be(GroupRole.Admin);
        newOwnerMembership.Role.Should().Be(GroupRole.Owner);
    }
}
