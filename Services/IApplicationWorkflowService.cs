using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.Services;

public interface IApplicationWorkflowService
{
    Task ApplyAsync(int resumeId, int vacancyId, string candidateUserId, string? coverLetter);
    Task MarkAsReviewedAsync(int applicationId, string actorUserId);
    Task UpdateStatusAsync(int applicationId, ApplicationStatus status, string actorUserId);
}
