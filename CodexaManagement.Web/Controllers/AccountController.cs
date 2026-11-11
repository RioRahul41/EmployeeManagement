using CodexaManagement.Core.DTO;
using CodexaManagement.Core.DTO.Account;
using CodexaManagement.Core.EmailTemplates;
using CodexaManagement.Core.IServices;
using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Web;

namespace CodexaManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityApplication> _userManager;
        private readonly SignInManager<IdentityApplication> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeService _employeeService;
        private readonly IEmailService _emailService;


        public AccountController(
     UserManager<IdentityApplication> userManager,
     SignInManager<IdentityApplication> signInManager,
     RoleManager<IdentityRole> roleManager,
     IEmployeService employeeService,
     IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _employeeService = employeeService;
            _emailService = emailService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FirstName = user.FirstName;
            ViewBag.FullName = $"{user.FirstName} {user.LastName}";

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? "User";

            return View();
        }

        [Authorize(Roles = "HR,Admin,Manager")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles
                .OrderBy(n => n.Name)
                .ToListAsync();
            return View(roles);
        }

        [Authorize(Roles = "HR,Admin,Manager")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userManager.Users
                .Where(r => !r.UserIsActive && !r.UserIsDeleted)
                .OrderBy(r => r.CreatedTime)
                .ToListAsync();
            return View(users);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", dto);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View("Login", dto);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                dto.Password,
                dto.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Force newly created users to change their temporary password.
                if (user.MustChangePassword)
                {
                    return RedirectToAction("ChangePassword", "Account");
                }

                // Normal login after the password has been changed.
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    "",
                    "Your account is locked. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");
            }

            return View("Login", dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }


        //forgotpassword 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var token =
     await _userManager.GeneratePasswordResetTokenAsync(user);

                var resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new
                    {
                        email = model.Email,
                        token = token
                    }, protocol:
                    Request.Scheme);

                var body = $"""
            <h2>Reset Your Password</h2>
            <p>We received a request to reset your CodexaManagement password.</p>
            <p><a href="{resetLink}">Click here to reset your password</a></p>
            <p>If you did not request this, you can ignore this email.</p>
            """;

                await _emailService.SendEmailAsync(
                    model.Email,
                    "CodexaManagement - Password Reset",
                    body,
                    "PasswordReset");
            }

            // Same response whether the account exists or not.
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        //reset password 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            Console.WriteLine("ResetPassword GET started");

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            Console.WriteLine("Returning ResetPassword view");

            return View(model);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid password reset request.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        //reset confrimation page
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [Authorize(Roles = "HR,Admin,Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Roles = new SelectList(
                    new[] { "HR", "Manager", "User" });
            }
            else if (User.IsInRole("HR"))
            {
                ViewBag.Roles = new SelectList(
                    new[] { "User" });
            }

            return View(new RoleCreateview());
        }




        //[Authorize(Roles = "Admin,HR")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(RoleCreateview model)
        //{
        //    // Step 1: Validate the selected role.
        //    // HR can create only User accounts.
        //    if (User.IsInRole("HR") && !User.IsInRole("Admin"))
        //    {
        //        if (model.Role != "User")
        //        {
        //            ModelState.AddModelError(
        //                nameof(model.Role),
        //                "HR can create only User accounts.");
        //        }
        //    }
        //    else if (model.Role != "User" && model.Role != "HR")
        //    {
        //        ModelState.AddModelError(
        //            nameof(model.Role),
        //            "Please select a valid role.");
        //    }

        //    // Step 2: Define valid designations for each department.
        //    var designationsByDepartment =
        //        new Dictionary<DepartmentEnum, string[]>
        //        {
        //            [DepartmentEnum.IT] = new[]
        //            {
        //        ".NET Developer",
        //        "Python Developer",
        //        "Java Developer",
        //        "Frontend Developer",
        //        "QA Tester",
        //        "DevOps Engineer"
        //            },

        //            [DepartmentEnum.HR] = new[]
        //            {
        //        "HR Executive",
        //        "HR Manager",
        //        "Recruiter",
        //        "Talent Acquisition Specialist"
        //            },

        //            [DepartmentEnum.Finance] = new[]
        //            {
        //        "Accountant",
        //        "Financial Analyst",
        //        "Finance Manager",
        //        "Auditor"
        //            },

        //            [DepartmentEnum.Sales] = new[]
        //            {
        //        "Sales Executive",
        //        "Sales Manager",
        //        "Business Development Executive"
        //            }
        //        };

        //    // Step 3: Validate department.
        //    if (!model.Department.HasValue ||
        //        !Enum.IsDefined(
        //            typeof(DepartmentEnum),
        //            model.Department.Value))
        //    {
        //        ModelState.AddModelError(
        //            nameof(model.Department),
        //            "Please select a valid department.");
        //    }
        //    else
        //    {
        //        // Step 4: Validate designation against department.
        //        if (string.IsNullOrWhiteSpace(model.Designation))
        //        {
        //            ModelState.AddModelError(
        //                nameof(model.Designation),
        //                "Please select a designation.");
        //        }
        //        else if (
        //            !designationsByDepartment.TryGetValue(
        //                model.Department.Value,
        //                out var allowedDesignations) ||
        //            !allowedDesignations.Contains(
        //                model.Designation.Trim(),
        //                StringComparer.OrdinalIgnoreCase))
        //        {
        //            ModelState.AddModelError(
        //                nameof(model.Designation),
        //                "Please select a valid designation for the selected department.");
        //        }
        //    }

        //    // Stop if validation fails.
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    // Step 5: Check whether email already exists.
        //    var existingUser =
        //        await _userManager.FindByEmailAsync(model.Email);

        //    if (existingUser != null)
        //    {
        //        ModelState.AddModelError(
        //            nameof(model.Email),
        //            "This email is already in use.");

        //        return View(model);
        //    }

        //    // Step 6: Generate a temporary password.
        //    var temporaryPassword = GeneratePassword();

        //    // Step 7: Create the Identity user.
        //    var newUser = new IdentityApplication
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        FirstName = model.FirstName.Trim(),
        //        LastName = model.LastName.Trim(),
        //        Location = model.Location?.Trim(),

        //        // Store department enum integer in AspNetUsers.
        //        DepartmentId = (int)model.Department!.Value,

        //        CreatedTime = DateTime.Now,
        //        MustChangePassword = true
        //    };

        //    var result = await _userManager.CreateAsync(
        //        newUser,
        //        temporaryPassword);

        //    if (!result.Succeeded)
        //    {
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }

        //        return View(model);
        //    }

        //    // Step 8: Assign the selected role.
        //    var roleResult = await _userManager.AddToRoleAsync(
        //        newUser,
        //        model.Role);

        //    if (!roleResult.Succeeded)
        //    {
        //        foreach (var error in roleResult.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }

        //        await _userManager.DeleteAsync(newUser);

        //        return View(model);
        //    }

        //    // Step 9: Create the linked Employee record.
        //    try
        //    {
        //        var employee = new Employee
        //        {
        //            FirstName = newUser.FirstName,
        //            LastName = newUser.LastName,
        //            Email = newUser.Email!,
        //            Location = newUser.Location,

        //            IdentityUserId = newUser.Id,

        //            // Store department name as a string.
        //            Department = model.Department.Value.ToString(),

        //            Designation = model.Designation.Trim(),

        //            // Details will be completed later.
        //            Phone = null,
        //            DOB = null,
        //            Gender = null,
        //            Salary = null,

        //            CreatedTime = DateTime.Now,
        //            EmpIsActive = true,
        //            EmpIsDeleted = false
        //        };

        //        await _employeeService.CreateEmployeeAsync(employee);
        //    }
        //    catch (Exception)
        //    {
        //        // Remove the Identity account if Employee creation fails.
        //        await _userManager.DeleteAsync(newUser);

        //        ModelState.AddModelError(
        //            "",
        //            "Account creation failed while creating the employee record. Please try again.");

        //        return View(model);
        //    }

        //    // Step 10: Build the account-created email.
        //    var loginUrl = Url.Action(
        //        "Login",
        //        "Account",
        //        values: null,
        //        protocol: Request.Scheme
        //    )!;

        //    var subject = "Your CodexaManagement Account";

        //    var body = AccountCreatedTemplate.GetHtml(
        //        newUser.FirstName,
        //        newUser.Email!,
        //        temporaryPassword,
        //        loginUrl
        //    );

        //    // Step 11: Send the email and record the result.
        //    try
        //    {
        //        await _emailService.SendEmailAsync(
        //            newUser.Email!,
        //            subject,
        //            body,
        //            "AccountCreated"
        //        );

        //        TempData["Success"] =
        //            "Account and employee record created successfully. The account email was sent.";
        //    }
        //    catch (Exception)
        //    {
        //        // Do not delete a successfully created account because
        //        // email delivery failed.
        //        TempData["Success"] =
        //            "Account and employee record created successfully, but the email could not be sent. Please check EmailLogs.";
        //    }

        //    // Step 12: Keep credentials available for local testing only.
        //    TempData["LoginUserName"] = newUser.Email;
        //    TempData["TemporaryPassword"] = temporaryPassword;

        //    return RedirectToAction("UserAccoutcreatedPage");
        //}




        //user creaetd acocount



        [Authorize(Roles = "HR,Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateview model)
        {
            // Step 1: Validate selected role based on logged-in user's role.

            string[] allowedRoles;

            if (User.IsInRole("Admin"))
            {
                // Admin can create HR, Manager and User.
                allowedRoles = new[] { "HR", "Manager", "User" };
            }
            else if (User.IsInRole("HR"))
            {
                // HR can create only User.
                allowedRoles = new[] { "User" };
            }
            else
            {
                allowedRoles = Array.Empty<string>();
            }

            if (string.IsNullOrWhiteSpace(model.Role) ||
                !allowedRoles.Contains(
                    model.Role.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "You are not authorized to assign the selected role.");
            }

            // Step 2: Department and designation validation.

            var departmentDesignations =
                new Dictionary<DepartmentEnum, List<string>>
                {
            {
                DepartmentEnum.IT,
                new List<string>
                {
                    ".NET Developer",
                    "Python Developer",
                    "Java Developer",
                    "Frontend Developer",
                    "QA Tester",
                    "DevOps Engineer"
                }
            },
            {
                DepartmentEnum.HR,
                new List<string>
                {
                    "HR Executive",
                    "HR Manager",
                    "Recruiter",
                    "Talent Acquisition Specialist"
                }
            },
            {
                DepartmentEnum.Finance,
                new List<string>
                {
                    "Accountant",
                    "Financial Analyst",
                    "Finance Manager",
                    "Auditor"
                }
            },
            {
                DepartmentEnum.Sales,
                new List<string>
                {
                    "Sales Executive",
                    "Sales Manager",
                    "Business Development Executive"
                }
            }
                };

            // Step 3: Validate department.

            if (!model.Department.HasValue ||
                !Enum.IsDefined(typeof(DepartmentEnum), model.Department.Value))
            {
                ModelState.AddModelError(
                    nameof(model.Department),
                    "Please select a valid department.");
            }

            // Step 4: Validate designation.

            if (model.Department.HasValue &&
                departmentDesignations.TryGetValue(
                    model.Department.Value, out var allowedDesignations))
            {
                if (string.IsNullOrWhiteSpace(model.Designation) ||
                    !allowedDesignations.Contains(
                        model.Designation.Trim(),
                        StringComparer.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        nameof(model.Designation),
                        "Please select a valid designation.");
                }
            }

            // Step 5: Check model validation.

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step 6: Check duplicate email.

            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email already exists.");

                return View(model);
            }

            // Step 7: Generate temporary password.

            var temporaryPassword = GeneratePassword();

            // Step 8: Create Identity user.

            var newUser = new IdentityApplication
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Location = model.Location,
                DepartmentId = (int)model.Department.Value,
                CreatedTime = DateTime.Now,
                MustChangePassword = true
            };

            var createResult = await _userManager.CreateAsync(
                newUser, temporaryPassword);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Step 9: Assign selected role to Identity user.

            var roleResult = await _userManager.AddToRoleAsync(
                newUser, model.Role.Trim());

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Step 10: Create linked Employee record.

            var employee = new Employee
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email!,
                Location = newUser.Location,

                IdentityUserId = newUser.Id,

                Department = model.Department.Value.ToString(),
                Designation = model.Designation.Trim(),

                Phone = null,
                DOB = null,
                Gender = null,
                Salary = null,

                CreatedTime = DateTime.Now,
                EmpIsActive = true,
                EmpIsDeleted = false
            };

            try
            {
                await _employeeService.CreateEmployeeAsync(employee);
            }
            catch
            {
                // Remove Identity user if Employee creation fails.
                await _userManager.DeleteAsync(newUser);

                ModelState.AddModelError(
                    "",
                    "Unable to create the employee record. Please try again.");

                return View(model);
            }

            // Step 11: Send account-created email.

            try
            {
                string subject = "Your CodexaManagement Account";

                // Generate the login URL.
                string loginUrl = Url.Action(
                    "Login",
                    "Account",
                    null,
                    Request.Scheme
                )!;

                // Generate account-created email HTML.
                string emailBody = AccountCreatedTemplate.GetHtml(
                    model.FirstName,
                    model.Email,
                    temporaryPassword,
                    loginUrl
                );

                // Send email.
                await _emailService.SendEmailAsync(
                    newUser.Email!,
                    subject,
                    emailBody,
                    "AccountCreated"
                );
            }
            catch
            {
                // Account is already created.
                // Email failure should not delete the account.
                TempData["Warning"] =
                    "Account created successfully, but the email could not be sent.";
            }

            // Step 12: Set success messages.

            TempData["Success"] = "Account created successfully.";

            TempData["LoginEmail"] = newUser.Email;
            TempData["TemporaryPassword"] = temporaryPassword;

            // Step 13: Redirect to account-created page.

            return RedirectToAction("UserAccoutcreatedPage");
        }


        [HttpGet]
        public IActionResult UserAccoutcreatedPage()
        {
            return View();
        }
        public async Task<IActionResult> Profile()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Role = "Admin";
            }
            else if (User.IsInRole("HR"))
            {
                ViewBag.Role = "HR";
            }
            else if (User.IsInRole("Manager"))
            {
                ViewBag.Role = "Manager";
            }
            else if (User.IsInRole("User"))
            {
                ViewBag.Role = "User";
            }
            else
            {
                ViewBag.Role = "No Role Assigned";
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignHRRole()
        {
            var user = await _userManager.FindByEmailAsync("hr@codexa.com");

            if (user == null)
            {
                return NotFound("HR account not found.");
            }

            if (!await _roleManager.RoleExistsAsync("HR"))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole("HR"));

                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }
            }

            if (!await _userManager.IsInRoleAsync(user, "HR"))
            {
                var result = await _userManager.AddToRoleAsync(user, "HR");

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }

            return Content("HR role assigned successfully to hr@codexa.com.");
        }

        private static string GeneratePassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%*?";
            const string all = upper + lower + digits + symbols;

            var chars = new List<char>
{
    upper[RandomNumberGenerator.GetInt32(upper.Length)],
    lower[RandomNumberGenerator.GetInt32(lower.Length)],
    digits[RandomNumberGenerator.GetInt32(digits.Length)],
    symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
};
            while (chars.Count < 12)
            {
                chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
            }
            for (int i = chars.Count - 1; i >= 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars.ToArray());
        }


        //change passowrd method 
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            ViewBag.MustChangePassword = user.MustChangePassword;

            return View(new ChangePasswordViewModel());
        }

        // POST: Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);

                ViewBag.MustChangePassword =
                    currentUser?.MustChangePassword ?? true;

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            // Change the password.
            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                ViewBag.MustChangePassword = user.MustChangePassword;
                return View(model);
            }

            // Remove the forced password-change requirement.
            user.MustChangePassword = false;
            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                ViewBag.MustChangePassword = user.MustChangePassword;
                return View(model);
            }

            // Refresh the signed-in user's authentication cookie.
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Password changed successfully.Your email is now marked as confirmed";

            // Go directly to the Dashboard.
            return RedirectToAction("Login", "Account");
        }


        //test email for testing purpose

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            var currentUserEmail = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(currentUserEmail))
            {
                return Unauthorized();
            }

            await _emailService.SendEmailAsync(
                currentUserEmail,
                "CodexaManagement SMTP Test",
                "<h2>SMTP Test Successful</h2><p>This email was captured by smtp4dev.</p>",
                "Test");

            return Content("Email test completed. Check smtp4dev and EmailLogs.");
        }

    }
}