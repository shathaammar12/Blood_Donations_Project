using Blood_Donations_Project.Models;
using Blood_Donations_Project.Services;
using Blood_Donations_Project.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blood_Donations_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly BloodDonationContext _context;
        private readonly IConfiguration _config;


        public AccountController(BloodDonationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "")
    };

            var keyString = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT Key is missing in appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: model.RememberMe
                    ? DateTime.UtcNow.AddDays(7)
                    : DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax, 
                Secure = Request.IsHttps,                
                Expires = model.RememberMe
                    ? DateTimeOffset.Now.AddDays(7)
                    : DateTimeOffset.Now.AddHours(2)
            };

            Response.Cookies.Append("jwt", tokenString, cookieOptions);

            HttpContext.Session.SetString("jwt", tokenString);
            HttpContext.Session.SetString("UserRole", user.Role?.RoleName ?? "");
            HttpContext.Session.SetString("UserId", user.UserId.ToString());

            return RedirectToAction("Dashboard", "Admin");
 
        }

        public async Task<IActionResult> Register()
        {
            var roles = await _context.Roles
                .Where(r => r.RoleName != "Admin")
                .Select(r => new { r.RoleId, r.RoleName })
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");

            var bloodTypes = await _context.BloodTypes
            .Select(bt => new {bt.BloodTypeId, bt.TypeName})
                .ToListAsync();
            ViewBag.BloodTypes = new SelectList(bloodTypes, "BloodTypeId", "TypeName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var selectedRole = await _context.Roles.FindAsync(model.RoleId);
            if (selectedRole?.RoleName != "Donor")
            {
                ModelState.Remove("BloodTypeId");
            }

            if(!ModelState.IsValid)
            {
                await LoadRegisterDropdowns();
                return View(model);
            }

            try
            {
                if(await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists!");
                    await LoadRegisterDropdowns();
                    return View(model);
                }

                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username already exists");
                    await LoadRegisterDropdowns();
                    return View(model);
                }

                var hasher = new PasswordHasher<User>();
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    MobileNo = model.MobileNo,
                    Address = model.Address,
                    RoleId = model.RoleId
                };

                user.Password = hasher.HashPassword(user, model.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if(selectedRole?.RoleName == "Donor")
                {
                    if(!model.BloodTypeId.HasValue)
                    {
                        ModelState.AddModelError("BloodTypeId", "Blood type is required!");
                        await LoadRegisterDropdowns();
                        return View(model);
                    }

                    var donor = new Donor
                    {
                        UserId = user.UserId,
                        BloodTypeId = model.BloodTypeId.Value,
                        IsAvailable = true,
                        LastDonationDate = null
                    };

                    _context.Donors.Add(donor);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                await LoadRegisterDropdowns();
                return View(model);
            }
        }

        private async Task LoadRegisterDropdowns()
        {
            var roles = await _context.Roles
                .Where(r => r.RoleName != "Admin")
                .Select(r => new { r.RoleId, r.RoleName })
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");

            var bloodTypes = await _context.BloodTypes
                .Select(bt => new { bt.BloodTypeId, bt.TypeName })
                .ToListAsync();

            ViewBag.BloodTypes = new SelectList(bloodTypes, "BloodTypeId", "TypeName");
        }
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrWhiteSpace(role))
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            TempData["SuccessMessage"] = "If the email exists, a reset link has been sent.";

            if (user == null)
                return RedirectToAction("Login");

            var oldTokens = await _context.PasswordReset
                .Where(t => t.UserId == user.UserId && !t.Used)
                .ToListAsync();

            if (oldTokens.Any())
                _context.PasswordReset.RemoveRange(oldTokens);

            var token = Guid.NewGuid().ToString("N");

            _context.PasswordReset.Add(new PasswordReset
            {
                UserId = user.UserId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Used = false
            });

            await _context.SaveChangesAsync();

            var resetUrl = Url.Action("ResetPassword", "Account",
                new { email = user.Email, token = token }, Request.Scheme);

            Console.WriteLine("RESET LINK => " + resetUrl);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPassword
            {
                Email = email ?? "",
                Token = token ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
            {
                ModelState.AddModelError("", "Invalid reset link.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError("", "New password is required.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid reset link.");
                return View(model);
            }

            var tokenRow = await _context.PasswordReset
                .FirstOrDefaultAsync(t =>
                    t.UserId == user.UserId &&
                    t.Token == model.Token &&
                    !t.Used);

            if (tokenRow == null || tokenRow.ExpiresAt < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Reset link expired or invalid.");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.NewPassword);

            tokenRow.Used = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password reset successfully. Please login.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login");

            var vm = new Profile
            {
                UserId = user.UserId,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                MobileNo = user.MobileNo,
                Address = user.Address,
                RoleName = user.Role?.RoleName
            };

            if (string.Equals(vm.RoleName, "Donor", StringComparison.OrdinalIgnoreCase))
            {
                var donor = await _context.Donors
                    .Include(d => d.BloodType)
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                vm.BloodTypeId = donor?.BloodTypeId;
                vm.BloodTypeName = donor?.BloodType?.TypeName;

                ViewBag.BloodTypes = await _context.BloodTypes
                    .Select(bt => new { bt.BloodTypeId, bt.TypeName })
                    .ToListAsync();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Profile model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login");

            if (model.UserId != userId) return Forbid();

            var roleName = HttpContext.Session.GetString("UserRole") ?? "";

            if (string.Equals(roleName, "Donor", StringComparison.OrdinalIgnoreCase))
            {
                if (!model.BloodTypeId.HasValue)
                    ModelState.AddModelError(nameof(model.BloodTypeId), "Blood type is required.");
            }

            if (!ModelState.IsValid)
            {
                if (string.Equals(roleName, "Donor", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.BloodTypes = await _context.BloodTypes
                        .Select(bt => new { bt.BloodTypeId, bt.TypeName })
                        .ToListAsync();
                }
                model.RoleName = roleName;
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login");

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.MobileNo = model.MobileNo;
            user.Address = model.Address;

            if (string.Equals(roleName, "Donor", StringComparison.OrdinalIgnoreCase))
            {
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (donor != null)
                {
                    donor.BloodTypeId = model.BloodTypeId!.Value;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

    }
}
