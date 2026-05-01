using AisVacanciesAndResumes.ViewModels.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AisVacanciesAndResumes.Services;

public interface IMessageService
{
    Task<List<MessageListItemViewModel>> GetInboxAsync(string userId);
    Task<List<MessageListItemViewModel>> GetSentAsync(string userId);
    Task<MessageCreateViewModel> GetCreateModelAsync(string senderUserId, string? receiverId = null);
    Task SendAsync(string senderUserId, MessageCreateViewModel model);
    Task<MessageDetailsViewModel?> GetDetailsAsync(string userId, int id);
    Task<int> GetUnreadCountAsync(string userId);
}
