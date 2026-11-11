using CodexaManagement.Data.DbContext;
using CodexaManagement.Model.Entities;
using CodexaManagement.Model.Enum;
using CodexaManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodexaManagement.Web.Controllers;

[Authorize]
public class LeaveController : Controller
{
    // ===== Business rules (keep in sync with the Apply.cshtml / Edit.cshtml scripts) =====
    private const int SickLeavePastDays = 7;        // Sick: today and up to 7 days back
    private const int OtherLeaveWindowDays = 7;     // Maternity/Casual/LOP: 7 days back and 7 days ahead
    private const int ProofRequiredAfterDays = 2;   // Proof needed for MORE than 2 working days
    // =====================================================================================

    private const long MaxProofSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedLeaveTypes = new(StringComparer.OrdinalIgnoreCase)
        { "Maternity", "Sick", "Casual", "LOP" };

    private static readonly HashSet<string> AllowedProofExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".jpg", ".jpeg", ".png" };

    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityApplication> _userManager;
    private readonly IWebHostEnvironment _environment;

    public LeaveController(
        ApplicationDbContext db,
        UserManager<IdentityApplication> userManager,
        IWebHostEnvironment environment)
    {
        _db = db;
        _userManager = userManager;
        _environment = environment;
    }

    // ------------------------------------------------------------------
    // My requests
    // ------------------------------------------------------------------
    [Authorize(Roles = "User,HR,Manager")]
    public async Task<IActionResult> MyRequests()
    {
        var employee = await GetCurrentEmployeeAsync();

        if (employee == null)
        {
            TempData["Error"] = "Your employee profile was not found.";
            return RedirectToAction("Index", "Home");
        }

        var requests = await _db.LeaveRequests
            .Where(x => x.EmployeeId == employee.Id)
            .OrderByDescending(x => x.AppliedDate)
            .ToListAsync();

        return View(requests);
    }

    // ------------------------------------------------------------------
    // Apply
    // ------------------------------------------------------------------
    [Authorize(Roles = "User,HR,Manager")]
    [HttpGet]
    public IActionResult Apply() => View(new ApplyLeaveViewModel());

    [Authorize(Roles = "User,HR,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxProofSize + 256 * 1024)]
    public async Task<IActionResult> Apply(ApplyLeaveViewModel model)
    {
        // 1. Leave type, dates, range rules, working days
        var workingDays = ValidateLeaveDates(
            model.LeaveType, model.StartDate, model.EndDate,
            out var start, out var end);

        // 2. Proof is mandatory for more than 2 working days
        if (workingDays > ProofRequiredAfterDays && model.ProofDocument == null)
        {
            AddErrorOnce(
                nameof(model.ProofDocument),
                "Upload proof for leave requests longer than 2 working days.");
        }

        // 3. Proof file (extension, size, real file content)
        await ValidateProofAsync(model.ProofDocument);

        // 4. Current employee
        var employee = await GetCurrentEmployeeAsync();
        if (employee == null)
            return NotFound("Employee profile was not found.");

        // 5. Overlapping pending/approved requests
        if (workingDays > 0 && await HasOverlapAsync(employee.Id, start, end, null))
        {
            ModelState.AddModelError(
                string.Empty,
                "These dates overlap another pending or approved leave request.");
        }

        if (!ModelState.IsValid)
            return View(model);

        // 6. Save
        var storedFileName = await SaveProofAsync(model.ProofDocument);

        try
        {
            _db.LeaveRequests.Add(new LeaveRequest
            {
                EmployeeId = employee.Id,
                LeaveType = model.LeaveType!.Trim(),
                FromDate = start,
                ToDate = end,
                TotalDays = workingDays,
                Reason = model.Reason?.Trim() ?? string.Empty,
                OriginalFileName = storedFileName,
                Status = LeaveRequestStatus.Pending,
                AppliedDate = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
        catch
        {
            DeleteProof(storedFileName);
            throw;
        }

        TempData["Success"] = "Leave request submitted to HR for review.";
        return RedirectToAction(nameof(MyRequests));
    }

    // ------------------------------------------------------------------
    // HR / Admin list
    // ------------------------------------------------------------------
    [Authorize(Roles = "HR,Admin,  Manager")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var requests = await _db.LeaveRequests
            .Include(request => request.Employee)
            .OrderBy(request => request.Status)
            .ThenByDescending(request => request.AppliedDate)
            .ToListAsync();

        // Reuse the same list instead of a second database query
        var pendingRequests = requests
            .Where(request => request.Status == LeaveRequestStatus.Pending)
            .OrderBy(request => request.AppliedDate)
            .ToList();

        ViewBag.PendingCount = pendingRequests.Count;
        ViewBag.PendingRequests = pendingRequests;

        return View(requests);
    }

    // ------------------------------------------------------------------
    // Edit (HR)
    // ------------------------------------------------------------------
    [Authorize(Roles = "HR,Manager")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var request = await _db.LeaveRequests
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null)
            return NotFound();

        var model = new EditLeaveViewModel
        {
            Id = request.Id,
            LeaveType = request.LeaveType,
            StartDate = request.FromDate,
            EndDate = request.ToDate,
            Reason = request.Reason,
            ExistingProofFileName = request.OriginalFileName
        };

        return View(model);
    }

    [Authorize(Roles = "HR,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxProofSize + 256 * 1024)]
    public async Task<IActionResult> Edit(EditLeaveViewModel model)
    {
        var request = await _db.LeaveRequests
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (request == null)
            return NotFound();

        // 1. Leave type, dates, range rules, working days
        var workingDays = ValidateLeaveDates(
            model.LeaveType, model.StartDate, model.EndDate,
            out var start, out var end);

        // 2. Proof is mandatory for more than 2 working days.
        //    An existing proof is accepted if no replacement is uploaded.
        if (workingDays > ProofRequiredAfterDays &&
            model.ProofDocument == null &&
            string.IsNullOrEmpty(request.OriginalFileName))
        {
            AddErrorOnce(
                nameof(model.ProofDocument),
                "Upload proof for leave requests longer than 2 working days.");
        }

        // 3. Replacement proof (extension, size, real file content)
        await ValidateProofAsync(model.ProofDocument);

        // 4. Overlap (exclude this request)
        if (workingDays > 0 &&
            await HasOverlapAsync(request.EmployeeId, start, end, request.Id))
        {
            ModelState.AddModelError(
                string.Empty,
                "These dates overlap another pending or approved leave request.");
        }

        if (!ModelState.IsValid)
        {
            model.ExistingProofFileName = request.OriginalFileName;
            return View(model);
        }

        // 5. Save replacement proof, if uploaded
        string? newFileName = null;

        if (model.ProofDocument != null)
        {
            newFileName = await SaveProofAsync(model.ProofDocument);
        }

        var oldFileName = request.OriginalFileName;

        try
        {
            request.LeaveType = model.LeaveType!.Trim();
            request.FromDate = start;
            request.ToDate = end;
            request.TotalDays = workingDays;
            request.Reason = model.Reason?.Trim() ?? string.Empty;

            if (newFileName != null)
            {
                request.OriginalFileName = newFileName;
            }

            // Reset previous decision
            request.Status = LeaveRequestStatus.Pending;
            request.HRRemarks = null;
            request.ApprovedBy = null;
            request.ApprovedDate = null;

            await _db.SaveChangesAsync();
        }
        catch
        {
            DeleteProof(newFileName);
            throw;
        }

        // Delete old proof only after a successful save
        if (newFileName != null)
        {
            DeleteProof(oldFileName);
        }

        TempData["Success"] =
            "Leave request updated successfully. It is now pending for review.";

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------
    // Review
    // ------------------------------------------------------------------
    [Authorize(Roles = "HR,Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        var request = await _db.LeaveRequests
            .Include(item => item.Employee)
            .SingleOrDefaultAsync(item => item.Id == id);

        if (request == null)
            return NotFound();

        return View(new ReviewLeaveViewModel
        {
            Id = request.Id,
            EmployeeName = $"{request.Employee.FirstName} {request.Employee.LastName}",
            EmployeeEmail = request.Employee.Email,
            LeaveType = request.LeaveType,
            StartDate = request.FromDate,
            EndDate = request.ToDate,
            WorkingDays = CountWorkingDays(request.FromDate, request.ToDate),
            Reason = request.Reason,
            Status = request.Status.ToString(),
            ReviewNote = request.HRRemarks,
            ProofFileName = request.OriginalFileName
        });
    }

    // ------------------------------------------------------------------
    // Approve / Reject (HR)
    // ------------------------------------------------------------------
    [Authorize(Roles = "HR,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        return await DecideAsync(id, LeaveRequestStatus.Approved, null);
    }

    // The form field is named "reviewComment", so the parameter must use the same name.
    [Authorize(Roles = "HR,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reviewComment)
    {
        return await DecideAsync(id, LeaveRequestStatus.Rejected, reviewComment);
    }

    // ------------------------------------------------------------------
    // Proof download

    [Authorize(Roles = "HR,Admin,User,Manager")]
    [HttpGet]
    public async Task<IActionResult> Proof(int id)
    {
        var request = await _db.LeaveRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null || string.IsNullOrEmpty(request.OriginalFileName))
            return NotFound();

        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "App_Data",
            "leave-proofs",
            request.OriginalFileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        Response.Headers["Content-Disposition"] = "inline";

        return PhysicalFile(filePath, contentType);
    }

    // ------------------------------------------------------------------
    // Pending count (sidebar badge)
    // ------------------------------------------------------------------
    [Authorize(Roles = "HR,Manager")]
    [HttpGet]
    public async Task<IActionResult> GetPendingLeaveCount()
    {
        var count = await _db.LeaveRequests
            .CountAsync(x => x.Status == LeaveRequestStatus.Pending);

        return Json(count);
    }

    // ==================================================================
    // Private helpers
    // ==================================================================

    private async Task<IActionResult> DecideAsync(
        int id,
        LeaveRequestStatus status,
        string? remarks)
    {
        var request = await _db.LeaveRequests
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);

        // HR cannot approve or reject their own leave
        if (request.Employee.IdentityUserId == currentUserId)
        {
            TempData["Error"] = "You cannot approve or reject your own leave request.";
            return RedirectToAction(nameof(Index));
        }

        // Only pending requests can be decided
        if (request.Status != LeaveRequestStatus.Pending)
        {
            TempData["Error"] = "This leave request has already been reviewed.";
            return RedirectToAction(nameof(Index));
        }

        if (status == LeaveRequestStatus.Rejected &&
            string.IsNullOrWhiteSpace(remarks))
        {
            TempData["Error"] = "Please provide a reason for rejection.";
            return RedirectToAction(nameof(Index));
        }

        request.Status = status;
        request.HRRemarks = status == LeaveRequestStatus.Rejected
            ? remarks!.Trim()
            : null;
        request.ApprovedBy = currentUserId;
        request.ApprovedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Success"] = status == LeaveRequestStatus.Approved
            ? "Leave request approved successfully."
            : "Leave request rejected successfully.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// One place for ALL date rules (used by Apply and Edit).
    /// Returns the number of working days (0 when the dates are invalid).
    /// </summary>
    private int ValidateLeaveDates(
        string? leaveType,
        DateTime? startDate,
        DateTime? endDate,
        out DateTime start,
        out DateTime end)
    {
        start = default;
        end = default;

        var type = leaveType?.Trim() ?? string.Empty;
        var typeIsValid = AllowedLeaveTypes.Contains(type);

        if (!typeIsValid)
            AddErrorOnce("LeaveType", "Choose a valid leave type.");

        if (!startDate.HasValue)
            AddErrorOnce("StartDate", "From date is required.");

        if (!endDate.HasValue)
            AddErrorOnce("EndDate", "To date is required.");

        if (!startDate.HasValue || !endDate.HasValue)
            return 0;

        start = startDate.Value.Date;
        end = endDate.Value.Date;

        if (end < start)
        {
            AddErrorOnce("EndDate", "To date must be on or after the From date.");
            return 0;
        }

        if (typeIsValid)
        {
            var today = DateTime.Today;
            DateTime minimumDate;
            DateTime maximumDate;

            if (string.Equals(type, "Sick", StringComparison.OrdinalIgnoreCase))
            {
                // Sick leave: today and up to N days back. No future dates.
                minimumDate = today.AddDays(-SickLeavePastDays);
                maximumDate = today;
            }
            else
            {
                // Maternity / Casual / LOP: N days before to N days after today
                minimumDate = today.AddDays(-OtherLeaveWindowDays);
                maximumDate = today.AddDays(OtherLeaveWindowDays);
            }

            var message =
                $"Select a date between {minimumDate:d MMM yyyy} and {maximumDate:d MMM yyyy}.";

            if (start < minimumDate || start > maximumDate)
                AddErrorOnce("StartDate", message);

            if (end < minimumDate || end > maximumDate)
                AddErrorOnce("EndDate", message);
        }

        var workingDays = CountWorkingDays(start, end);

        if (workingDays == 0)
        {
            AddErrorOnce(
                "StartDate",
                "Choose at least one working day (Monday to Friday).");
        }

        return workingDays;
    }

    /// <summary>Adds a model error only if the field has no error yet.</summary>
    private void AddErrorOnce(string key, string message)
    {
        if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0)
            return;

        ModelState.AddModelError(key, message);
    }

    private async Task<bool> HasOverlapAsync(
        int employeeId,
        DateTime start,
        DateTime end,
        int? excludeRequestId)
    {
        return await _db.LeaveRequests.AnyAsync(x =>
            (excludeRequestId == null || x.Id != excludeRequestId) &&
            x.EmployeeId == employeeId &&
            (x.Status == LeaveRequestStatus.Pending ||
             x.Status == LeaveRequestStatus.Approved) &&
            x.FromDate <= end &&
            x.ToDate >= start);
    }

    /// <summary>Checks extension, size and the real file header (not just the name).</summary>
    private async Task ValidateProofAsync(IFormFile? file)
    {
        if (file == null)
            return;

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedProofExtensions.Contains(extension))
        {
            AddErrorOnce("ProofDocument", "Upload a PDF, JPG, or PNG file.");
            return;
        }

        if (file.Length == 0 || file.Length > MaxProofSize)
        {
            AddErrorOnce("ProofDocument", "The proof document must be under 5 MB.");
            return;
        }

        if (!await HasValidSignatureAsync(file, extension))
        {
            AddErrorOnce(
                "ProofDocument",
                "The file content does not match its type. Upload a real PDF, JPG, or PNG file.");
        }
    }

    private static async Task<bool> HasValidSignatureAsync(IFormFile file, string extension)
    {
        var header = new byte[8];

        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length));

        if (read < 4)
            return false;

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => header[0] == 0x25 && header[1] == 0x50 &&
                      header[2] == 0x44 && header[3] == 0x46,

            ".png" => read >= 8 &&
                      header.AsSpan().SequenceEqual(
                          new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),

            ".jpg" or ".jpeg" => header[0] == 0xFF &&
                                 header[1] == 0xD8 &&
                                 header[2] == 0xFF,

            _ => false
        };
    }

    private async Task<Employee?> GetCurrentEmployeeAsync()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return null;

        return await _db.Employees
            .FirstOrDefaultAsync(x => x.IdentityUserId == userId);
    }

    private async Task<string?> SaveProofAsync(IFormFile? file)
    {
        if (file == null)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";

        Directory.CreateDirectory(GetProofDirectory());

        var path = Path.Combine(GetProofDirectory(), fileName);

        await using var stream = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None);

        await file.CopyToAsync(stream);

        return fileName;
    }

    private string GetProofDirectory() =>
        Path.Combine(_environment.ContentRootPath, "App_Data", "leave-proofs");

    private void DeleteProof(string? fileName)
    {
        if (fileName == null)
            return;

        var path = Path.Combine(GetProofDirectory(), Path.GetFileName(fileName));

        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    private static int CountWorkingDays(DateTime start, DateTime end)
    {
        if (end < start)
            return 0;

        var count = 0;

        for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                count++;
        }

        return count;
    }
}