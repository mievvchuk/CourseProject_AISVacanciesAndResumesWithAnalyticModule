using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUserNotifications()
    {
        var userId = GetUserId();
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationApiDto
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                Type = x.Type,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var notification = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new NotificationApiDto
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                Type = x.Type,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        return notification is null ? NotFound() : Ok(notification);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
