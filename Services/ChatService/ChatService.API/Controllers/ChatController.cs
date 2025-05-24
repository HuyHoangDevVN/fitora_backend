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
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthorizeExtension _authorizeExtension;

        public ChatController(IChatService chatService, IAuthorizeExtension authorizeExtension)
        {
            _chatService = chatService;
            _authorizeExtension = authorizeExtension;
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
            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            await _chatService.SendMessageAsync(userId.ToString(), request.ConversationId, request.Content,
                request.Type);
            return Ok();
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory([FromQuery] GetHistoryChatRequest request)
        {
            var messages = await _chatService.GetChatHistoryAsync(request);
            return Ok(new ResponseDto(messages));
        }
        
        [HttpGet("group-conversations")]
        public async Task<IActionResult> GetGroupConversationsByUserId([FromQuery] string userId)
        {
            var conversations = await _chatService.GetGroupConversationsByUserIdAsync(userId);
            return Ok(new ResponseDto(conversations));
        }

        [HttpGet("private-conversation")]
        public async Task<IActionResult> GetPrivateConversation([FromQuery] string userId, string otherUserId)
        {
            var conversation = await _chatService.GetPrivateConversationAsync(userId, otherUserId);
            return Ok(new ResponseDto(conversation));
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
            var response = await _chatService.AddReactionAsync(request.MessageId, request.UserId, request.Emoji);
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
            var response = await _chatService.UpdateGroupInfoAsync(request.ConversationId, request.GroupInfo);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("add-group-member")]
        public async Task<IActionResult> AddGroupMember([FromBody] GroupMemberRequest request)
        {
            var response = await _chatService.AddGroupMemberAsync(request.ConversationId, request.UserId);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("remove-group-member")]
        public async Task<IActionResult> RemoveGroupMember([FromBody] GroupMemberRequest request)
        {
            var response = await _chatService.RemoveGroupMemberAsync(request.ConversationId, request.UserId);
            return Ok(new ResponseDto(response));
        }

        [HttpPost("assign-group-admin")]
        public async Task<IActionResult> AssignGroupAdmin([FromBody] GroupMemberRequest request)
        {
            var response = await _chatService.AssignGroupAdminAsync(request.ConversationId, request.UserId);
            return Ok(new ResponseDto(response));
        }
    }
}