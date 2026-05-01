using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet]
    public async Task<IActionResult> Inbox()
    {
        var items = await _messageService.GetInboxAsync(GetUserId());
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Sent()
    {
        var items = await _messageService.GetSentAsync(GetUserId());
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string? receiverId)
    {
        var model = await _messageService.GetCreateModelAsync(GetUserId(), receiverId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MessageCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var createModel = await _messageService.GetCreateModelAsync(GetUserId(), model.ReceiverId);
            model.ReceiverOptions = createModel.ReceiverOptions;
            model.ReceiverName = createModel.ReceiverName;
            model.IsReceiverLocked = createModel.IsReceiverLocked;
            return View(model);
        }

        try
        {
            await _messageService.SendAsync(GetUserId(), model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.ReceiverId), exception.Message);
            var createModel = await _messageService.GetCreateModelAsync(GetUserId(), model.ReceiverId);
            model.ReceiverOptions = createModel.ReceiverOptions;
            model.ReceiverName = createModel.ReceiverName;
            model.IsReceiverLocked = createModel.IsReceiverLocked;
            return View(model);
        }

        TempData["StatusMessage"] = "Повідомлення надіслано.";
        return RedirectToAction(nameof(Sent));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _messageService.GetDetailsAsync(GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Current user identifier was not found.");
    }
}
