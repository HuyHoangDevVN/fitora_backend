using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Responses;
using BuildingBlocks.Security;
using FluentAssertions;
using Moq;
using UserService.Application.Usecases.Group.Commands.DissolveGroup;
using UserService.Domain.Enums;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.Usecases.Group;

public class DissolveGroupHandlerTests
{
    private readonly Mock<IRepositoryBase<Domain.Models.Group>> _groupRepo = new();
    private readonly Mock<IRepositoryBase<GroupMember>> _memberRepo = new();
    private readonly Mock<IAuthorizeExtension> _auth = new();

    private DissolveGroupHandler CreateHandler() => new(_groupRepo.Object, _memberRepo.Object, _auth.Object);

    [Fact]
    public async Task Handle_CallerIsOwner_DissolvesGroup()
    {
        var callerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(callerId, "owner", "Owner"));
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), default))
            .ReturnsAsync(new GroupMember { GroupId = groupId, UserId = callerId, Role = GroupRole.Owner });
        _groupRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.Group, bool>>>(), default))
            .ReturnsAsync(new Domain.Models.Group { Id = groupId, Name = "Test" });
        _groupRepo.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(new DissolveGroupCommand(groupId), default);

        result.IsSuccess.Should().BeTrue();
        _memberRepo.Verify(r => r.DeleteRangeAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), default), Times.Once);
        _groupRepo.Verify(r => r.DeleteAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.Group, bool>>>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_CallerIsAdmin_ThrowsUnAuthorization()
    {
        var callerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(callerId, "admin", "Admin"));
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), default))
            .ReturnsAsync(new GroupMember { GroupId = groupId, UserId = callerId, Role = GroupRole.Admin });

        var act = async () => await CreateHandler().Handle(new DissolveGroupCommand(groupId), default);

        await act.Should().ThrowAsync<UnAuthorizationException>();
        _groupRepo.Verify(r => r.DeleteAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.Group, bool>>>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_CallerNotMember_ThrowsUnAuthorization()
    {
        var callerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(callerId, "stranger", "Stranger"));
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), default))
            .ReturnsAsync((GroupMember?)null);

        var act = async () => await CreateHandler().Handle(new DissolveGroupCommand(groupId), default);

        await act.Should().ThrowAsync<UnAuthorizationException>();
    }
}
