using AisVacanciesAndResumes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AisVacanciesAndResumes.Controllers.Api;

[ApiController]
[Authorize(Roles = "Candidate")]
[Route("api/recommendations")]
public class RecommendationsApiController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsApiController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentCandidateRecommendations()
    {
        var model = await _recommendationService.GetRecommendationsAsync(GetCurrentUserId());
        return Ok(model.Items);
    }

    [HttpGet("{candidateId}")]
    public async Task<IActionResult> GetByCandidateId(string candidateId)
    {
        var currentUserId = GetCurrentUserId();
        if (!string.Equals(candidateId, currentUserId, StringComparison.Ordinal))
        {
            return Forbid();
        }

        var model = await _recommendationService.GetRecommendationsAsync(currentUserId);
        return Ok(model.Items);
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Ідентифікатор поточного користувача не знайдено.");
    }
}
