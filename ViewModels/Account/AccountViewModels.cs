using System.ComponentModel.DataAnnotations;
using AisVacanciesAndResumes.Enums;

namespace AisVacanciesAndResumes.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть email.")]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Введіть коректний email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль.")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запам'ятати мене")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ПІБ.")]
    [MinLength(2, ErrorMessage = "ПІБ має містити мінімум 2 символи.")]
    [Display(Name = "ПІБ")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть email.")]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Введіть коректний email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль.")]
    [MinLength(6, ErrorMessage = "Пароль має містити мінімум 6 символів.")]
    [RegularExpression(@"^[\x21-\x7E]+$", ErrorMessage = "Пароль має містити лише латинські літери, цифри та символи.")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не збігаються.")]
    [Display(Name = "Підтвердження пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Оберіть роль.")]
    [Display(Name = "Роль")]
    public UserRoleType? Role { get; set; }
}
