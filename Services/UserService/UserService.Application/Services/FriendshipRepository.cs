using System.Linq.Expressions;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.DTOs.Friendship.Responses;
using UserService.Domain.Enums;

namespace UserService.Application.Services
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly IRepositoryBase<FriendShip> _friendshipRepo;
        private readonly IRepositoryBase<FriendRequest> _friendRequestRepo;
        private readonly IMapper _mapper;

        public FriendshipRepository(
            IRepositoryBase<FriendShip> friendshipRepo,
            IRepositoryBase<FriendRequest> friendRequestRepo, 
            IMapper mapper)
        {
            _friendshipRepo = friendshipRepo;
            _friendRequestRepo = friendRequestRepo;
            _mapper = mapper;
        }

        private static FriendRequestDto MapToFriendRequestDto(FriendRequest fr) => new()
        {
            Id = fr.Id,
            SenderId = fr.SenderId,
            ReceiverId = fr.ReceiverId,
            SenderName = fr.Sender?.Username ?? "Unknown Sender",
            SenderImageUrl = fr.Sender?.UserInfo?.ProfilePictureUrl ?? string.Empty,
            ReceiverName = fr.Receiver?.Username ?? "Unknown Receiver",
            ReceiverImageUrl = fr.Receiver?.UserInfo?.ProfilePictureUrl ?? string.Empty,
            Status = fr.Status,
            CreateDate = fr.CreatedAt
        };

        private static FriendDto MapToFriendDto(FriendShip fs, Guid userId)
        {
            var friend = fs.User1Id == userId ? fs.User2 : fs.User1;
            return new FriendDto
            {
                Id = friend?.Id ?? Guid.Empty,
                Username = friend?.Username ?? "Unknown",
                Email = friend?.Email ?? "Unknown",
                FirstName = friend?.UserInfo?.FirstName ?? "Unknown",
                LastName = friend?.UserInfo?.LastName ?? "Unknown",
                Address = friend?.UserInfo?.Address ?? "Unknown",
                PhoneNumber = friend?.UserInfo?.PhoneNumber ?? "Unknown",
                ProfilePictureUrl = friend?.UserInfo?.ProfilePictureUrl ?? string.Empty,
                Bio = friend?.UserInfo?.Bio ?? "No bio available",
                Gender = friend?.UserInfo?.Gender ?? Gender.Unknown,
                BirthDate = friend?.UserInfo?.BirthDate ?? DateTime.Now,
            };
        }

        public async Task<ResponseDto> CreateFriendRequestAsync(CreateFriendRequest request)
        {
            var isRequestSent = (await _friendRequestRepo
                .FindAsync(fr => fr.SenderId == request.senderId && fr.ReceiverId == request.receiverId))
                .Any();

            if (isRequestSent)
            {
                return new ResponseDto(null, false, "The request is already sended.");
            }

            var isFriend = (await _friendshipRepo.FindAsync(fs =>
                (fs.User1Id == request.senderId && fs.User2Id == request.receiverId) ||
                (fs.User2Id == request.senderId && fs.User1Id == request.receiverId)))
                .Any();

            if (isFriend)
            {
                return new ResponseDto(null, false, "The user is already a friend.");
            }

            var friendRequest = new FriendRequest
            {
                SenderId = request.senderId,
                ReceiverId = request.receiverId,
                Status = StatusFriendRequest.Pending,
                CreatedAt = DateTime.Now,
            };

            await _friendRequestRepo.AddAsync(friendRequest);
            var isSuccess = await _friendRequestRepo.SaveChangesAsync() > 0;
            return new ResponseDto(null, isSuccess, isSuccess ? "Success" : "Failed");
        }

        public async Task<PaginatedResult<FriendRequestDto>> GetSentFriendRequestAsync(GetSentFriendRequest request)
        {
            var includes = new List<Expression<Func<FriendRequest, object>>>
            {
                fr => fr.Sender,
                fr => fr.Sender.UserInfo,
                fr => fr.Receiver,
                fr => fr.Receiver.UserInfo
            };

            return await _friendRequestRepo.GetPageWithIncludesAsync(
                paginationRequest: new PaginationRequest(request.PageIndex, request.PageSize),
                selector: fr => MapToFriendRequestDto(fr),
                conditions: fr => fr.SenderId == request.Id && fr.Status == StatusFriendRequest.Pending,
                includes: includes,
                cancellationToken: CancellationToken.None
            );
        }

        public async Task<PaginatedResult<FriendRequestDto>> GetReceivedFriendRequestAsync(GetReceivedFriendRequest request)
        {
            var includes = new List<Expression<Func<FriendRequest, object>>>
            {
                fr => fr.Sender,
                fr => fr.Sender.UserInfo,
                fr => fr.Receiver,
                fr => fr.Receiver.UserInfo
            };

            return await _friendRequestRepo.GetPageWithIncludesAsync(
                paginationRequest: new PaginationRequest(request.PageIndex, request.PageSize),
                selector: fr => MapToFriendRequestDto(fr),
                conditions: fr => fr.ReceiverId == request.Id && fr.Status == StatusFriendRequest.Pending,
                includes: includes,
                cancellationToken: CancellationToken.None
            );
        }

        public async Task<PaginatedResult<FriendDto>> GetFriends(GetFriendsRequest request)
        {
            var includes = new List<Expression<Func<FriendShip, object>>>
            {
                fs => fs.User2,
                fs => fs.User2.UserInfo,
                fs => fs.User1,
                fs => fs.User1.UserInfo
            };

            return await _friendshipRepo.GetPageWithIncludesAsync(
                paginationRequest: new PaginationRequest(request.PageIndex, request.PageSize),
                selector: fs => MapToFriendDto(fs, request.Id),
                conditions: fs => fs.User1Id == request.Id || fs.User2Id == request.Id,
                includes: includes,
                cancellationToken: CancellationToken.None
            );
        }

        public async Task<bool> AcceptFriendRequestAsync(CreateFriendRequest request)
        {
            var friendRequest = await _friendRequestRepo.GetAsync(fr =>
                fr.SenderId == request.senderId && fr.ReceiverId == request.receiverId);
            if (friendRequest == null)
                return false;

            friendRequest.Status = StatusFriendRequest.Accepted;
            await _friendRequestRepo.UpdateAsync(fr =>
                fr.SenderId == request.senderId && fr.ReceiverId == request.receiverId, friendRequest);

            if (await _friendRequestRepo.SaveChangesAsync() <= 0)
                return false;

            var friendship = new FriendShip
            {
                User1Id = friendRequest.SenderId,
                User2Id = friendRequest.ReceiverId,
                CreatedAt = DateTime.Now
            };

            await _friendshipRepo.AddAsync(friendship);
            return await _friendshipRepo.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteFriendRequestAsync(CreateFriendRequest request)
        {
            await _friendRequestRepo.DeleteAsync(fr =>
                fr.SenderId == request.senderId && fr.ReceiverId == request.receiverId);
            return await _friendRequestRepo.SaveChangesAsync() > 0;
        }

        public async Task<bool> UnfriendAsync(Guid id)
        {
            await _friendshipRepo.DeleteAsync(fs =>
                (fs.User1Id == id || fs.User2Id == id));

            await _friendRequestRepo.DeleteAsync(fr =>
                (fr.SenderId == id || fr.ReceiverId == id));

            var friendshipDeleted = await _friendshipRepo.SaveChangesAsync() > 0;
            var friendRequestDeleted = await _friendRequestRepo.SaveChangesAsync() > 0;

            return friendshipDeleted || friendRequestDeleted;
        }
    }
}
