using Blood_Donations_Project.Models;
using Blood_Donations_Project.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Blood_Donations_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly BloodDonationContext _context;

        public AdminController(BloodDonationContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole") ?? "";
            return role == "Admin";
        }

        public async Task<IActionResult> Dashboard(string table = "BloodRequests", string status = "All")
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!int.TryParse(userIdStr, out var userId) || string.IsNullOrWhiteSpace(role))
                return RedirectToAction("Login", "Account");

            // Cards 
            ViewBag.TotalDonations = await _context.Donations.CountAsync();
            ViewBag.TotalBloodRequests = await _context.BloodRequests.CountAsync();
            ViewBag.TotalDonationRequests = await _context.DonationRequests.CountAsync();
            ViewBag.TotalRequests = (int)ViewBag.TotalBloodRequests + (int)ViewBag.TotalDonationRequests;



            if (role == "Admin")
            {
                table = (table ?? "BloodRequests").Trim();
                status = (status ?? "All").Trim();

                ViewBag.SelectedTable = table;
                ViewBag.SelectedStatus = status;

                var bloodQuery =
                    from br in _context.BloodRequests
                    join u in _context.Users on br.UserId equals u.UserId into users
                    from u in users.DefaultIfEmpty()
                    join bt in _context.BloodTypes on br.BloodTypeId equals bt.BloodTypeId into bts
                    from bt in bts.DefaultIfEmpty()
                    select new BloodRequestRowTable
                    {
                        Id = br.Id,
                        UserId = br.UserId,
                        UserName = u != null ? u.FullName : "-",
                        BloodTypeId = br.BloodTypeId,
                        BloodTypeName = bt != null ? bt.TypeName : "-",
                        RequestDate = br.RequestDate,
                        Quantity = br.Quantity,
                        Status = br.Status
                    };

                if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                    bloodQuery = bloodQuery.Where(x => x.Status != null && x.Status.Trim() == status);

                ViewBag.AdminRequests = await bloodQuery
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                var donorQuery = _context.DonationRequests
                    .Include(r => r.User)
                    .Include(r => r.ApprovedByNavigation)
                    .AsQueryable();

                if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                    donorQuery = donorQuery.Where(r => r.Status != null && r.Status.Trim() == status);

                ViewBag.DonorDonationRequests = await donorQuery
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();
            }


            if (role == "Hospital" || role == "BloodBank")
            {
                ViewBag.BloodTypeMap = await _context.BloodTypes
            .ToDictionaryAsync(bt => bt.BloodTypeId, bt => bt.TypeName);

                ViewBag.HospitalRequests = await _context.BloodRequests
                    .Where(br => br.UserId == userId)
                    .OrderByDescending(br => br.Id)
                    .ToListAsync();
            }

            if (role == "Donor")
            {
                var donor = await _context.Donors
                    .Include(d => d.User)
                    .Include(d => d.BloodType)
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                ViewBag.Donor = donor;

 
                var lastApprovedDonation = await _context.Donations
                    .Where(d => d.UserId == userId && d.Status == "Approved")
                    .OrderByDescending(d => d.DonationDate)
                    .FirstOrDefaultAsync();

                ViewBag.LastDonation = lastApprovedDonation;


                var lastRequestedDate = await _context.DonationRequests
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RequestDate)
                    .Select(r => (DateOnly?)r.RequestDate)
                    .FirstOrDefaultAsync();

                bool canDonate = true;
                if (lastRequestedDate.HasValue)
                {
                    var nextAllowed = lastRequestedDate.Value.ToDateTime(TimeOnly.MinValue).AddMonths(3);
                    canDonate = DateTime.Now >= nextAllowed;
                }

                ViewBag.CanDonate = canDonate;


                ViewBag.DonorRequests = await _context.DonationRequests
                    .Where(r => r.UserId == userId)
                    .Include(r => r.ApprovedByNavigation)
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();
            }


            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Donors).ThenInclude(d => d.BloodType)
                .Where(u => u.Role != null && u.Role.RoleName != "Admin")
                .Select(u => new AdminUserRow
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email,
                    MobileNo = u.MobileNo,
                    Address = u.Address,

                    RoleName = u.Role != null ? u.Role.RoleName : null,

                    BloodTypeName = u.Donors
                        .Select(d => d.BloodType.TypeName)
                        .FirstOrDefault()
                })
                .OrderBy(u => u.UserId)
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var roles = await _context.Roles
                .Select(r => new { r.RoleId, r.RoleName })
                .ToListAsync();
            ViewBag.Roles = roles;

            var model = new EditUser
            {
                UserId = user.UserId,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                MobileNo = user.MobileNo,
                Address = user.Address,
                RoleId = user.RoleId
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUser model)
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            var roles = await _context.Roles
                .Select(r => new { r.RoleId, r.RoleName })
                .ToListAsync();
            ViewBag.Roles = roles;

            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.MobileNo = model.MobileNo;
            user.Address = model.Address;
            user.RoleId = model.RoleId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> ManageDonations()
        {
            var donations = await _context.Donations
                .Include(d => d.User)
                .ThenInclude(u => u.Donors)
                .ThenInclude(d => d.BloodType)
                .Include(d => d.ApprovedByNavigation)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            return View(donations);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDonation(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("UserId");
            if(!int.TryParse(adminIdStr, out var adminId))
                return Json(new {success = false, message = "Not authorized" });

            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
                return Json(new { success = false, message = "Donation not found" });

            donation.Status = "Approved";
            donation.ApprovedBy = adminId;
            donation.DonationDate = DateOnly.FromDateTime(DateTime.Now);

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == donation.UserId);
            if(donor != null)
            {
                donor.LastDonationDate = DateOnly.FromDateTime(DateTime.Now);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Donation approved successfully" });                
        }

        [HttpPost]
        public async Task<IActionResult> RejectDonation(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(adminIdStr, out var adminId))
                return Json(new { success = false, message = "Not authorized" });

            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
                return Json(new { success = false, message = "Donation not found" });

            donation.Status = "Rejected";
            donation.ApprovedBy = adminId;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Donation rejected" });
        }

        public async Task<IActionResult> ManageBloodRequests(string status = "Pending")
        {
            status = (status ?? "Pending").Trim();

            var query =
                from br in _context.BloodRequests
                join u in _context.Users on br.UserId equals u.UserId into users
                from u in users.DefaultIfEmpty()
                join bt in _context.BloodTypes on br.BloodTypeId equals bt.BloodTypeId into bts
                from bt in bts.DefaultIfEmpty()
                select new BloodRequestRowTable
                {
                    Id = br.Id,
                    UserId = br.UserId,
                    UserName = u != null ? u.FullName : "-",
                    BloodTypeId = br.BloodTypeId,
                    BloodTypeName = bt != null ? bt.TypeName : "-",
                    RequestDate = br.RequestDate,
                    Quantity = br.Quantity,
                    Status = br.Status
                };

            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status != null && x.Status.Trim() == status);
            }

            var requests = await query.OrderByDescending(x => x.Id).ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(requests);
        }


        [HttpPost]
        public async Task<IActionResult> ApproveBloodRequest(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(adminIdStr, out var adminId))
                return Json(new { success = false, message = "Not authorized" });

            var req = await _context.BloodRequests.FirstOrDefaultAsync(br => br.Id == id);
            if (req == null)
                return Json(new { success = false, message = "Request not found" });

            if (req.UserId == null)
                return Json(new { success = false, message = "Invalid request data" });

            var donation = new Donation
            {
                UserId = req.UserId.Value,
                ApprovedBy = adminId,
                DonationDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "Approved" 
            };

            _context.Donations.Add(donation);

            req.Status = "Approved";

            var units = req.Quantity ?? 0;

            var inventory = await _context.BloodInventories
                .FirstOrDefaultAsync(i => i.BloodTypeId == req.BloodTypeId);

            if (inventory == null)
                return Json(new { success = false, message = "Inventory not found" });

            if (inventory.UnitsAvailable < units)
                return Json(new { success = false, message = "Not enough blood units available" });

            inventory.UnitsAvailable -= units;


            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Request approved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectBloodRequest(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(adminIdStr, out var adminId))
                return Json(new { success = false, message = "Not authorized" });

            var req = await _context.BloodRequests.FirstOrDefaultAsync(br => br.Id == id);
            if (req == null)
                return Json(new { success = false, message = "Request not found" });

            req.Status = "Rejected";

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Request rejected" });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDonorRequest(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(adminIdStr, out var adminId))
                return Json(new { success = false, message = "Not authorized" });

            var req = await _context.DonationRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return Json(new { success = false, message = "Request not found" });

            if (req.Status != "Pending")
                return Json(new { success = false, message = "Already processed" });

            req.Status = "Approved";
            req.ApprovedBy = adminId;
            req.ApprovedDate = DateOnly.FromDateTime(DateTime.Now);

            _context.Donations.Add(new Donation
            {
                UserId = req.UserId,
                ApprovedBy = adminId,
                DonationDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "Approved"
            });

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == req.UserId);
            if (donor?.BloodTypeId != null)
            {
                var inventory = await _context.BloodInventories
                    .FirstOrDefaultAsync(i => i.BloodTypeId == donor.BloodTypeId);

                if (inventory != null)
                {
                    inventory.UnitsAvailable += 1;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Donor request approved" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectDonorRequest(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(adminIdStr, out var adminId))
                return Json(new { success = false, message = "Not authorized" });

            var req = await _context.DonationRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return Json(new { success = false, message = "Request not found" });

            if (req.Status != "Pending")
                return Json(new { success = false, message = "Already processed" });

            req.Status = "Rejected";
            req.ApprovedBy = adminId;
            req.ApprovedDate = DateOnly.FromDateTime(DateTime.Now);

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Donor request rejected" });
        }



        public async Task<IActionResult> Statistics()
        {
            var bloodTypeStats = await _context.Donors
                .Include(d => d.BloodType)
                .GroupBy(d => d.BloodType.TypeName)
                .Select(g => new
                {
                    BloodType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.BloodTypeStats = bloodTypeStats;

            ViewBag.TotalDonations = await _context.Donations.CountAsync(d => d.Status == "Approved");
            ViewBag.PendingDonations = await _context.Donations.CountAsync(d => d.Status == "Pending");
            ViewBag.RejectedDonations = await _context.Donations.CountAsync(d => d.Status == "Rejected");

            ViewBag.ApprovedRequests = await _context.BloodRequests.CountAsync(br => br.Status == "Approved");
            ViewBag.PendingRequests = await _context.BloodRequests.CountAsync(br => br.Status == "Pending");
            ViewBag.RejectedRequests = await _context.BloodRequests.CountAsync(br => br.Status == "Rejected");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BloodAvailability()
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            var stock = await _context.BloodInventories
                .Include(i => i.BloodType)
                .OrderBy(i => i.BloodType!.TypeName)
                .ToListAsync();

            return View(stock);
        }
    }
}
