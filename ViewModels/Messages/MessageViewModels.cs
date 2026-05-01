using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AisVacanciesAndResumes.ViewModels.Messages;

public class MessageCreateViewModel
{
    [Required(ErrorMessage = "Оберіть отримувача")]
    [Display(Name = "Отримувач")]
    public string ReceiverId { get; set; } = string.Empty;

    public string ReceiverName { get; set; } = string.Empty;

    public bool IsReceiverLocked { get; set; }

    [Required(ErrorMessage = "Вкажіть тему повідомлення")]
    [Display(Name = "Тема")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть текст повідомлення")]
    [Display(Name = "Повідомлення")]
    public string Content { get; set; } = string.Empty;

    public List<SelectListItem> ReceiverOptions { get; set; } = new();
}

public class MessageDetailsViewModel
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsInboxMessage { get; set; }
    public string ReplyUserId { get; set; } = string.Empty;
}

public class MessageListItemViewModel
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}
