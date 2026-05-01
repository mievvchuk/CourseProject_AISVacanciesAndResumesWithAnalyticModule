using AisVacanciesAndResumes.Data;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MessageListItemViewModel>> GetInboxAsync(string userId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(x => x.Sender)
            .Where(x => x.ReceiverId == userId)
            .OrderByDescending(x => x.SentAt)
            .Select(x => new MessageListItemViewModel
            {
                Id = x.Id,
                Subject = x.Subject,
                OtherUserName = x.Sender != null ? x.Sender.FullName : string.Empty,
                IsRead = x.IsRead,
                SentAt = x.SentAt
            })
            .ToListAsync();
    }

    public async Task<List<MessageListItemViewModel>> GetSentAsync(string userId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(x => x.Receiver)
            .Where(x => x.SenderId == userId)
            .OrderByDescending(x => x.SentAt)
            .Select(x => new MessageListItemViewModel
            {
                Id = x.Id,
                Subject = x.Subject,
                OtherUserName = x.Receiver != null ? x.Receiver.FullName : string.Empty,
                IsRead = x.IsRead,
                SentAt = x.SentAt
            })
            .ToListAsync();
    }

    public async Task<MessageCreateViewModel> GetCreateModelAsync(string senderUserId, string? receiverId = null)
    {
        var options = await GetReceiverOptionsAsync(senderUserId);
        var selectedReceiver = !string.IsNullOrWhiteSpace(receiverId)
            ? options.FirstOrDefault(x => x.Value == receiverId)
            : null;

        return new MessageCreateViewModel
        {
            ReceiverId = selectedReceiver?.Value ?? string.Empty,
            ReceiverName = selectedReceiver?.Text ?? string.Empty,
            IsReceiverLocked = selectedReceiver is not null,
            ReceiverOptions = options
        };
    }

    public async Task SendAsync(string senderUserId, MessageCreateViewModel model)
    {
        var receiverExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == model.ReceiverId && x.Id != senderUserId);

        if (!receiverExists)
        {
            throw new InvalidOperationException("Отримувача не знайдено.");
        }

        var message = new Message
        {
            SenderId = senderUserId,
            ReceiverId = model.ReceiverId,
            Subject = model.Subject,
            Content = model.Content,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<MessageDetailsViewModel?> GetDetailsAsync(string userId, int id)
    {
        var message = await _context.Messages
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .FirstOrDefaultAsync(x => x.Id == id && (x.SenderId == userId || x.ReceiverId == userId));

        if (message is null)
        {
            return null;
        }

        if (message.ReceiverId == userId && !message.IsRead)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return new MessageDetailsViewModel
        {
            Id = message.Id,
            Subject = message.Subject,
            Content = message.Content,
            SenderName = message.Sender?.FullName ?? string.Empty,
            SenderEmail = message.Sender?.Email ?? string.Empty,
            ReceiverName = message.Receiver?.FullName ?? string.Empty,
            ReceiverEmail = message.Receiver?.Email ?? string.Empty,
            IsRead = message.IsRead,
            SentAt = message.SentAt,
            IsInboxMessage = message.ReceiverId == userId,
            ReplyUserId = message.ReceiverId == userId ? message.SenderId : message.ReceiverId
        };
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Messages
            .AsNoTracking()
            .CountAsync(x => x.ReceiverId == userId && !x.IsRead);
    }

    private async Task<List<SelectListItem>> GetReceiverOptionsAsync(string senderUserId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id != senderUserId && x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new SelectListItem($"{x.FullName} ({x.Email})", x.Id))
            .ToListAsync();
    }
}
