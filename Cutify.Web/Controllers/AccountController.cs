using Cutify.Domain.Entities;
using Cutify.Services.Helper;
using Cutify.Services.Interfaces;
using Cutify.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cutify.Web.Controllers;

public class AccountController : BaseController
{
    private readonly IBarberService _barberService;

    public AccountController(IBarberService barberService, ILogService logService)
        : base(logService)
    {
        _barberService = barberService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var barber = await _barberService.GetByEmailAsync(model.Email);
            if (barber == null || !await _barberService.ValidatePasswordAsync(model.Email, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, barber.Email),
            new Claim(ClaimTypes.NameIdentifier, barber.Id.ToString()),
            new Claim("FullName", $"{barber.FirstName} {barber.LastName}"),
            new Claim("BarberId", barber.Id.ToString()), // BarberId'yi Claims'e ekledik
            new Claim(ClaimTypes.Role, Cutify.Domain.Entities.Barber.Role)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties   
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            await LogInfoAsync($"User {barber.Email} logged in successfully.");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            await LogErrorAsync("Error during login", ex);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return View(model);
        }
    }



    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        string? imagePath = null;

        // ??kil yükl?nm?si
        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(fileStream);
            }

            imagePath = $"/uploads/profiles/{uniqueFileName}";
        }

        // Yeni istifad?çi yarad?l?r
        var user = new Barber
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = "000 000 00 00",
            WorkplaceAddress = model.WorkplaceAddress,
            ProfileImagePath = imagePath
        };

        // Database-? ?lav? olunur
        _barberService.CreateAsync(user, model.Password);

        return RedirectToAction("Login", "Account");
    }


    [HttpGet]
    public IActionResult PasswordReset(string email)
    {
        return View(new VerificationCodeViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasswordReset(VerificationCodeViewModel model)
    {

        ModelState.Remove("Code");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _barberService.GetByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "E-poçt ünvanı ilə əlaqəli istifadəçi tapılmadı.");
            return View(model);
        }

        // Generate a verification code
        var verificationCode = new Random().Next(1000, 9999).ToString();

        // Save the verification code (You can use session, database, etc.)
        HttpContext.Session.SetString($"VerificationCode:{model.Email}", verificationCode);

        var subject = "CUTIFY - Şifrə bərpa kodu";
        var body = $"<p>Sizin doğrulama kodunuz: <strong>{verificationCode}</strong></p>";

        await EmailHelper.SendEmailAsync(model.Email, subject, body);

        // Send the verification code to the user's email
        // Implement email sending logic here

        return RedirectToAction("VerificationCode", new { email = model.Email });

    }


    [HttpPost]
    [IgnoreAntiforgeryToken] // Əgər AJAX'dan AntiForgeryToken göndərmirsənsə
    public JsonResult ResendVerificationCode([FromBody] ResendCodeRequest model)
    {
        if (string.IsNullOrEmpty(model.Email))
        {
            return Json(new { success = false, message = "Email tapılmadı." });
        }

        // Yeni təsdiq kodu yarat
        var newCode = new Random().Next(1000, 9999).ToString();

        // Kodun sessiyada saxlanması (və ya DB)
        HttpContext.Session.SetString($"VerificationCode:{model.Email}", newCode);

        // TODO: Email göndərmə funksiyanı buraya əlavə et
        // await _emailService.SendCodeAsync(model.Email, newCode);

        return Json(new { success = true, message = "Yeni kod göndərildi." });
    }

    public class ResendCodeRequest
    {
        public string Email { get; set; }
    }




    [HttpGet]
    public IActionResult VerificationCode(string email, string code)
    {
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(code))
        {
            return RedirectToAction("Login"); // ?g?r email yoxdursa, login? yönl?ndir
        }

        var model = new VerificationCodeViewModel
        {
            Email = email,
            Code = code
        };

        return View( model);
    }

    // POST: /Account/VerifyCode
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerificationCode(VerificationCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Verification", model);
        }

        // BURADA kodu yoxlama loqikas?n? yaz?rsan
        // m?s?l?n, session, redis, db v? ya email gönd?ril?rk?n saxlan?lan kod il? müqayis?
        string correctCode = HttpContext.Session.GetString($"VerificationCode:{model.Email}");

        if (correctCode == null || correctCode != model.Code)
        {
            ModelState.AddModelError(string.Empty, "Təsdiq kodu yanlışdır.");
            return View("VerificationCode", model);
        }

        // Kod do?rudursa, istifad?çini t?sdiql? (m?s?l?n DB-d? statusu d?yi?)
        // Daha sonra ist?diyin s?hif?y? yönl?ndir
        return RedirectToAction("SetNewPassword", new { email = model.Email });
    }



    [HttpGet]
    public IActionResult SetNewPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login");


        // Burada doğrulama kodunun geçerliliğini kontrol et
        var correctCode = HttpContext.Session.GetString($"VerificationCode:{email}");

        if (string.IsNullOrEmpty(correctCode))
        {
            // Eğer doğrulama kodu yoksa, kullanıcıyı tekrar doğrulama sayfasına yönlendir
            return RedirectToAction("PasswordReset", new { email = email });
        }


        var model = new SetNewPasswordModel
        {
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetNewPassword(SetNewPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // ?stifad?çini email? gör? tap
        var user = await _barberService.GetByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "İstifadəçi tapılmadı.");
            return View(model);
        }

        // ?ifr?ni d?yi?
        var result = await _barberService.ResetPasswordAsync(user.Email, model.Input.Password);
        if (!result)
        {
            ModelState.AddModelError(string.Empty, "Şifrə dəyişdirilə bilmədi.");
            return View(model);
        }

        return RedirectToAction("Login", "Account");
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
} 