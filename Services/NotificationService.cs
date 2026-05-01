using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationListItemViewModel>> GetUserNotificationsAsync(string userId)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return notifications
            .Select(x => new NotificationListItemViewModel
            {
                Id = x.Id,
                Title = TranslateTitle(x.Title),
                Type = x.Type,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .ToList();
    }

    public async Task<NotificationDetailsViewModel?> GetDetailsAsync(string userId, int id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (notification is null)
        {
            return null;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return new NotificationDetailsViewModel
        {
            Id = notification.Id,
            Title = TranslateTitle(notification.Title),
            Content = TranslateContent(notification.Content),
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    public async Task MarkAsReadAsync(string userId, int id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead);
    }

    private static string TranslateTitle(string title)
    {
        return title switch
        {
            "Application submitted" => "Заявку подано",
            "New candidate application" => "Нова заявка кандидата",
            "Application status updated" => "Статус заявки оновлено",
            _ => title
        };
    }

    private static string TranslateContent(string content)
    {
        if (content == "Your application has been sent to the employer.")
        {
            return "Вашу заявку надіслано роботодавцю.";
        }

        if (content.StartsWith("A candidate applied to vacancy \"") && content.EndsWith("\"."))
        {
            var title = content["A candidate applied to vacancy \"".Length..^2];
            return $"Кандидат подав заявку на вакансію «{title}».";
        }

        if (content.StartsWith("The status of your application was changed to ") && content.EndsWith("."))
        {
            var status = content["The status of your application was changed to ".Length..^1];
            return $"Статус вашої заявки змінено на «{TranslateStatus(status)}».";
        }

        return content;
    }

    private static string TranslateStatus(string status)
    {
        return status switch
        {
            "New" => "Нова",
            "Reviewed" => "Переглянута",
            "Accepted" => "Прийнята",
            "Rejected" => "Відхилена",
            _ => status
        };
    }
}
