using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using ApexBetX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Accounts.AnyAsync(a => a.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.IDNumber == model.IDNumber))
            {
                ModelState.AddModelError("IDNumber", "A user with this ID Number already exists.");
                return View(model);
            }

            var token = Random.Shared.Next(100000, 1000000).ToString();

            var account = new Account
            {
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "User",
                IsEmailVerified = false,
                EmailVerificationToken = token,
                EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(10)
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var user = new User
            {
                IDNumber = model.IDNumber,
                FirstName = model.FirstName,
                Surname = model.Surname,
                Email = model.Email,
                Phone = model.Phone,
                AccountId = account.AccountId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                account.Email,
                "ApexBetX Verification Code",
                $"Your ApexBetX verification code is: {token}");

            TempData["Success"] = "Account created. Please check your email for the verification code.";
            return RedirectToAction("VerifyToken", new { email = account.Email });
        }

        public IActionResult VerifyToken(string email)
        {
            var model = new VerifyTokenViewModel
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyToken(VerifyTokenViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == model.Email);

            if (account == null)
            {
                ModelState.AddModelError("", "Account not found.");
                return View(model);
            }

            if (account.EmailVerificationToken != model.Token ||
                account.EmailVerificationTokenExpiry < DateTime.Now)
            {
                ModelState.AddModelError("Token", "Invalid or expired verification code.");
                return View(model);
            }

            account.IsEmailVerified = true;
            account.EmailVerificationToken = null;
            account.EmailVerificationTokenExpiry = null;

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendEmailAsync(
                    account.Email,
                    "ApexBetX Account Activated",
                    "Your ApexBetX account has been verified successfully." +
                    "\nYou can now log in.");
            }
            catch
            {
                TempData["Error"] = "Account verified, but confirmation email could not be sent.";
            }

            var currentRole = HttpContext.Session.GetString("Role");

            if (currentRole == "Admin")
            {
                TempData["Success"] = "User account verified successfully.";
                return RedirectToAction("Index", "Users");
            }

            TempData["Success"] = "Account verified successfully. You can now login.";
            return RedirectToAction("Login", "Account");

        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Email == model.Email);

            if (account == null || !BCrypt.Net.BCrypt.Verify(model.Password, account.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (!account.IsEmailVerified)
            {
                ModelState.AddModelError("", "Please verify your email before logging in.");
                return View(model);
            }

            HttpContext.Session.SetInt32("AccountId", account.AccountId);
            HttpContext.Session.SetString("Email", account.Email);
            HttpContext.Session.SetString("Role", account.Role);

            if (account.User != null)
            {
                HttpContext.Session.SetInt32("UserId", account.User.UserId);
            }

            TempData["Success"] = "Login successful.";

            if (account.Role == "Admin")
            {
                return RedirectToAction("Index", "Users");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been logged out.";

            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == model.Email);

            if (account == null)
            {
                ModelState.AddModelError("Email", "No account found with this email address.");
                return View(model);
            }

            var token = Random.Shared.Next(100000, 1000000).ToString();

            account.PasswordResetToken = token;
            account.PasswordResetTokenExpiry = DateTime.Now.AddMinutes(10);

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                account.Email,
                "ApexBetX Password Reset Code",
                $"Your ApexBetX password reset code is: {token}");

            TempData["Success"] = "A reset code has been sent to your email.";

            return RedirectToAction("ResetPassword", new { email = account.Email });
        }

        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == model.Email);

            if (account == null)
            {
                ModelState.AddModelError("", "Account not found.");
                return View(model);
            }

            if (account.PasswordResetToken != model.Token ||
                account.PasswordResetTokenExpiry < DateTime.Now)
            {
                ModelState.AddModelError("Token", "Invalid or expired reset code.");
                return View(model);
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            account.PasswordResetToken = null;
            account.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Password reset successfully. Please login.";
            return RedirectToAction("Login");
        }
    }
}
