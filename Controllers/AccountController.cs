using AisVacanciesAndResumes.Enums;
using AisVacanciesAndResumes.Models;
using AisVacanciesAndResumes.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AisVacanciesAndResumes.Services;
namespace AisVacanciesAndResumes.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ICandidateProfileService _candidateProfileService;
    private readonly IEmployerProfileService _employerProfileService;

    public AccountController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ICandidateProfileService candidateProfileService,
    IEmployerProfileService employerProfileService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _candidateProfileService = candidateProfileService;
        _employerProfileService = employerProfileService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Role is not (UserRoleType.Candidate or UserRoleType.Employer))
        {
            ModelState.AddModelError(nameof(model.Role), "Для реєстрації доступні лише ролі кандидата або роботодавця.");
            return View(model);
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role.Value.ToString());
        await _signInManager.SignInAsync(user, isPersistent: false);

        return await RedirectAfterSignInAsync(user);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is not null)
            {
                return await RedirectAfterSignInAsync(currentUser);
            }
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null && !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Ваш акаунт заблоковано. Зверніться до адміністратора.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Невірний email або пароль.");
            return View(model);
        }

        if (user is not null)
        {
            return await RedirectAfterSignInAsync(user);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<IActionResult> RedirectAfterSignInAsync(User user)
    {
        if (await _userManager.IsInRoleAsync(user, UserRoleType.Admin.ToString()))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        if (await _userManager.IsInRoleAsync(user, UserRoleType.Employer.ToString()))
        {
            var hasProfile = await _employerProfileService.ExistsAsync(user.Id);

            return hasProfile
                ? RedirectToAction("Index", "Home")
                : RedirectToAction("Edit", "EmployerProfiles");
        }

        if (await _userManager.IsInRoleAsync(user, UserRoleType.Candidate.ToString()))
        {
            var hasProfile = await _candidateProfileService.ExistsAsync(user.Id);

            return hasProfile
                ? RedirectToAction("Index", "Home")
                : RedirectToAction("Edit", "CandidateProfiles");
        }

        return RedirectToAction("Index", "Home");
    }
}
