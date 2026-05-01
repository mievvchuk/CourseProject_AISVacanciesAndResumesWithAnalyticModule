using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.Services;
using AisVacanciesAndResumes.ViewModels.Messages;
using Microsoft.EntityFrameworkCore;

namespace AisVacanciesAndResumes.Tests;

public class MessageServiceTests
{
    [Fact]
    public async Task GetCreateModel_WithReceiverId_LocksSelectedReceiver()
    {
        await using var context = TestDbContextFactory.Create();
        var employer = new User { Id = "employer-1", UserName = "employer@test.local", Email = "employer@test.local", FullName = "Employer", IsActive = true };
        var candidate = new User { Id = "candidate-1", UserName = "candidate@test.local", Email = "candidate@test.local", FullName = "Candidate", IsActive = true };
        context.Users.AddRange(employer, candidate);
        await context.SaveChangesAsync();

        var service = new MessageService(context);

        var model = await service.GetCreateModelAsync(employer.Id, candidate.Id);

        Assert.Equal(candidate.Id, model.ReceiverId);
        Assert.True(model.IsReceiverLocked);
        Assert.Contains(candidate.Email!, model.ReceiverName);
    }

    [Fact]
    public async Task SendAsync_CreatesMessageForSelectedCandidate()
    {
        await using var context = TestDbContextFactory.Create();
        var employer = new User { Id = "employer-1", UserName = "employer@test.local", Email = "employer@test.local", FullName = "Employer", IsActive = true };
        var candidate = new User { Id = "candidate-1", UserName = "candidate@test.local", Email = "candidate@test.local", FullName = "Candidate", IsActive = true };
        context.Users.AddRange(employer, candidate);
        await context.SaveChangesAsync();

        var service = new MessageService(context);

        await service.SendAsync(employer.Id, new MessageCreateViewModel
        {
            ReceiverId = candidate.Id,
            Subject = "Interview",
            Content = "Please contact us."
        });

        var message = await context.Messages.SingleAsync();
        Assert.Equal(employer.Id, message.SenderId);
        Assert.Equal(candidate.Id, message.ReceiverId);
        Assert.Equal("Interview", message.Subject);
        Assert.False(message.IsRead);
    }
}
