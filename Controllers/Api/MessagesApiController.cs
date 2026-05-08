using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/messages")]
public class MessagesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessagesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var userId = GetUserId();
        var messages = await ProjectMessages(userId)
            .Where(x => x.ReceiverId == userId)
            .OrderByDescending(x => x.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var userId = GetUserId();
        var messages = await ProjectMessages(userId)
            .Where(x => x.SenderId == userId)
            .OrderByDescending(x => x.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var message = await ProjectMessages(userId)
            .Where(x => x.Id == id && (x.SenderId == userId || x.ReceiverId == userId))
            .FirstOrDefaultAsync();

        return message is null ? NotFound() : Ok(message);
    }

    private IQueryable<MessageApiDto> ProjectMessages(string userId)
    {
        return _context.Messages
            .AsNoTracking()
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x => x.SenderId == userId || x.ReceiverId == userId)
            .Select(x => new MessageApiDto
            {
                Id = x.Id,
                SenderId = x.SenderId,
                SenderName = x.Sender != null ? x.Sender.FullName : string.Empty,
                ReceiverId = x.ReceiverId,
                ReceiverName = x.Receiver != null ? x.Receiver.FullName : string.Empty,
                Subject = x.Subject,
                Content = x.Content,
                IsRead = x.IsRead,
                SentAt = x.SentAt
            });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
