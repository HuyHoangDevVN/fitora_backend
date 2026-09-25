using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using ChatService.Application.Data.Message.Request;
using ChatService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthorizeExtension _authorizeExtension;
        private readonly IPresenceService _presenceService;

        public ChatController(IChatService chatService, IAuthorizeExtension authorizeExtension, IPresenceService presenceService)
        {
            _chatService = chatService;
            _authorizeExtension = authorizeExtension;
            _presenceService = presenceService;
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var conversationId =
                await _chatService.CreateConversationAsync(request.ParticipantIds, request.IsGroup, request.GroupInfo);
            return Ok(new ResponseDto(conversationId));
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ConversationId))
                return BadRequest(new ResponseDto(null, false, "ConversationId là bắt buộc"));
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new ResponseDto(null, false, "Nội dung tin nhắn không được để trống"));
            if (request.Content.Length > 5000)
                return BadRequest(new ResponseDto(null, false, "Nội dung tin nhắn không được vượt quá 5000 ký tự"));

            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            await _chatService.SendMessageAsync(userId.ToString(), request.ConversationId, request.Content,
                request.Type);
            return Ok();
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory([FromQuery] GetHistoryChatRequest request)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var conv = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conv == null || !conv.ParticipantIds.Contains(callerId))
                return Forbid();
            var messages = await _chatService.GetChatHistoryAsync(request);
            return Ok(new ResponseDto(messages));
        }

        [HttpGet("group-conversations")]
        public async Task<IActionResult> GetGroupConversationsByUserId([FromQuery] string userId)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            if (userId != callerId) return Forbid();
            var conversations = await _chatService.GetGroupConversationsByUserIdAsync(userId);
            return Ok(new ResponseDto(conversations));
        }

        [HttpGet("private-conversation")]
        public async Task<IActionResult> GetPrivateConversation([FromQuery] string userId, string otherUserId)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            if (userId != callerId) return Forbid();
            var conversation = await _chatService.GetPrivateConversationAsync(userId, otherUserId);
            return Ok(new ResponseDto(conversation));
        }

        [HttpGet("private-conversations")]
        public async Task<IActionResult> GetPrivateConversationsByUserId([FromQuery] string userId)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            if (userId != callerId) return Forbid();
            var conversations = await _chatService.GetPrivateConversationsByUserIdAsync(userId);
            return Ok(new ResponseDto(conversations));
        }
        
        [HttpPost("delete-message")]
        public async Task<IActionResult> DeleteMessage([FromBody] string messageId)
        {
            var response = await _chatService.DeleteMessageAsync(messageId);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("recall-message")]
        public async Task<IActionResult> RecallMessage([FromBody] GetHistoryChatRequest request)
        {
            var response = await _chatService.RecallMessageAsync(request);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("add-reaction")]
        public async Task<IActionResult> AddReaction([FromBody] AddReactionRequest request)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var response = await _chatService.AddReactionAsync(request.MessageId, callerId, request.Emoji);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            var response = await _chatService.MarkAsReadAsync(request.MessageId, request.IsRead);
            return Ok(new ResponseDto(response));
        }

        [HttpPut("update-group-info")]
        public async Task<IActionResult> UpdateGroupInfo([FromBody] UpdateGroupInfoRequest request)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var conv = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conv == null || !conv.ParticipantIds.Contains(callerId))
                return Forbid();
            var response = await _chatService.UpdateGroupInfoAsync(request.ConversationId, request.GroupInfo);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("add-group-member")]
        public async Task<IActionResult> AddGroupMember([FromBody] GroupMemberRequest request)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var conv = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conv == null || !conv.ParticipantIds.Contains(callerId))
                return Forbid();
            var response = await _chatService.AddGroupMemberAsync(request.ConversationId, request.UserId);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("remove-group-member")]
        public async Task<IActionResult> RemoveGroupMember([FromBody] GroupMemberRequest request)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var conv = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conv == null || (!conv.GroupInfo.AdminIds.Contains(callerId) && request.UserId != callerId))
                return Forbid();
            var response = await _chatService.RemoveGroupMemberAsync(request.ConversationId, request.UserId);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("assign-group-admin")]
        public async Task<IActionResult> AssignGroupAdmin([FromBody] GroupMemberRequest request)
        {
            var callerId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var conv = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conv == null || !conv.GroupInfo.AdminIds.Contains(callerId))
                return Forbid();
            var response = await _chatService.AssignGroupAdminAsync(request.ConversationId, request.UserId);
            return Ok(new ResponseDto(response));
        }

        [HttpGet("presence")]
        public async Task<IActionResult> GetPresence([FromQuery] string userIds)
        {
            if (string.IsNullOrWhiteSpace(userIds))
                return Ok(new ResponseDto(new Dictionary<string, PresenceDto>()));
            var ids = userIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var map = await _presenceService.GetPresenceAsync(ids);
            return Ok(new ResponseDto(map));
        }

        [HttpPost("group-conversations/{groupId}/create-or-get")]
        public async Task<IActionResult> CreateOrGetGroupConversation([FromRoute] string groupId, [FromBody] CreateOrGetGroupConversationRequest? request)
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            var conv = await _chatService.CreateOrGetGroupConversationAsync(groupId, userId, request?.GroupName, request?.MemberIds);
            return Ok(new ResponseDto(conv));
        }

        [HttpPost("group-conversations/{groupId}/sync-members")]
        public async Task<IActionResult> SyncGroupMembers([FromRoute] string groupId, [FromBody] SyncGroupMembersRequest request)
        {
            if (request?.MemberIds == null) return BadRequest(new ResponseDto("MemberIds is required"));
            var conv = await _chatService.SyncGroupMembersAsync(groupId, request.MemberIds);
            return Ok(new ResponseDto(conv));
        }
    }
}