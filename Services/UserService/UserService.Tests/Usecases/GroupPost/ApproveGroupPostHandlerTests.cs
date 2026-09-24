using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Responses;
using BuildingBlocks.Security;
using FluentAssertions;
using Moq;
using UserService.Application.Usecases.GroupPost.Commands.ApproveGroupPost;
using UserService.Domain.Enums;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.Usecases.GroupPost;

public class ApproveGroupPostHandlerTests
{
    private readonly Mock<IRepositoryBase<Domain.Models.GroupPost>> _postRepo = new();
    private readonly Mock<IRepositoryBase<GroupMember>> _memberRepo = new();
    private readonly Mock<IAuthorizeExtension> _auth = new();
    private readonly Guid _groupId = Guid.NewGuid();
    private readonly Guid _postId = Guid.NewGuid();
    private readonly Guid _callerId = Guid.NewGuid();

    private ApproveGroupPostHandler CreateHandler() => new(_postRepo.Object, _memberRepo.Object, _auth.Object);

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ThrowsNotFoundException()
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_callerId, "u", "U"));
        _postRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.GroupPost, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Models.GroupPost?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ApproveGroupPostCommand(_postId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotMember_ThrowsUnAuthorizationException()
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_callerId, "u", "U"));
        _postRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.GroupPost, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Models.GroupPost { GroupId = _groupId, PostId = _postId });
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ApproveGroupPostCommand(_postId), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizationException>();
    }

    [Theory]
    [InlineData(GroupRole.Member)]
    public async Task Handle_WhenCallerIsPlainMember_ThrowsUnAuthorizationException(GroupRole role)
    {
        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_callerId, "u", "U"));
        _postRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.GroupPost, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Models.GroupPost { GroupId = _groupId, PostId = _postId });
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupMember { GroupId = _groupId, UserId = _callerId, Role = role });

        var handler = CreateHandler();
        var act = () => handler.Handle(new ApproveGroupPostCommand(_postId), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizationException>();
    }

    [Theory]
    [InlineData(GroupRole.Owner)]
    [InlineData(GroupRole.Admin)]
    [InlineData(GroupRole.Moderator)]
    public async Task Handle_WhenCallerHasModerationRole_ApprovesPost(GroupRole role)
    {
        var post = new Domain.Models.GroupPost
        {
            GroupId = _groupId,
            PostId = _postId,
            ApprovalStatus = ApprovalStatus.Pending
        };

        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_callerId, "u", "U"));
        _postRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.GroupPost, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupMember { GroupId = _groupId, UserId = _callerId, Role = role });
        _postRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(new ApproveGroupPostCommand(_postId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        post.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAlreadyApproved_ReturnsFailureWithoutSaving()
    {
        var post = new Domain.Models.GroupPost
        {
            GroupId = _groupId,
            PostId = _postId,
            ApprovalStatus = ApprovalStatus.Approved
        };

        _auth.Setup(a => a.GetUserFromClaimToken()).Returns(new UserLoginResponseBase(_callerId, "u", "U"));
        _postRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Models.GroupPost, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _memberRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GroupMember, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupMember { GroupId = _groupId, UserId = _callerId, Role = GroupRole.Owner });

        var handler = CreateHandler();
        var result = await handler.Handle(new ApproveGroupPostCommand(_postId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _postRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
