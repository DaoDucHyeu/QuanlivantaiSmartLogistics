using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Models;
using SmartLogistics.Models.ViewModels;
using SmartLogistics.Helpers;

namespace SmartLogistics.Controllers
{
    [AuthorizeUserType(UserType.Admin)]
    public class StaffController : Controller
    {
        private readonly UserManager<AppUser> _UserManager;

        public StaffController(UserManager<AppUser> UserManager)
        {
            _UserManager = UserManager;
        }

        // GET: /Staff
        public async Task<IActionResult> Index(string? search, UserType? role, int page = 1)
        {
            var currentAdminId = _UserManager.GetUserId(User);
            // Chỉ Load nhân viên do Admin hiện tại quản lý
            var query = _UserManager.Users.Where(u => u.AdminId == currentAdminId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u => u.FullName!.ToLower().Contains(search)
                    || u.Email!.ToLower().Contains(search)
                    || u.UserName!.ToLower().Contains(search));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.UserType == role.Value);
            }

            var orderedQuery = query.OrderBy(u => u.UserType).ThenBy(u => u.FullName);
            int pageSize = 10;
            var users = await PaginatedList<AppUser>.CreateAsync(orderedQuery, page, pageSize);

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentRole"] = role;
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Admin", ""),
                ("Quản lý Nhân sự", "")
            };

            return View(users);
        }

        // GET: /Staff/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.DispatcherList = await GetDispatcherSelectList();
            return View(new StaffCreateViewModel());
        }

        private async Task<Microsoft.AspNetCore.Mvc.Rendering.SelectList> GetDispatcherSelectList()
        {
            var currentAdminId = _UserManager.GetUserId(User);
            var dispatchers = await _UserManager.Users
                .Where(u => u.UserType == UserType.DieuHanh && u.AdminId == currentAdminId)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            return new Microsoft.AspNetCore.Mvc.Rendering.SelectList(dispatchers, "Id", "FullName");
        }

        // POST: /Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _UserManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            AppUser user = model.UserType switch
            {
                UserType.Admin => new Admin(),
                UserType.DieuHanh => new DieuHanh(),
                UserType.TaiXe => new TaiXe { DieuHanhId = model.DieuHanhId },
                UserType.KeToan => new KeToan(),
                _ => new AppUser()
            };

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.UserType = model.UserType;
            user.AdminId = _UserManager.GetUserId(User); // Gắn nhân viên này cho Admin tạo ra
            user.EmailConfirmed = true;

            var result = await _UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Đã thêm nhân sự \"{model.FullName}\" thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.DispatcherList = await GetDispatcherSelectList();
            return View(model);
        }

        // GET: /Staff/Edit/id
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new StaffEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName ?? "",
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType,
                DieuHanhId = (user is TaiXe driver) ? driver.DieuHanhId : null
            };

            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Admin", ""),
                ("Quản lý Nhân sự", ""),
                ("Chỉnh sửa", "")
            };

            ViewBag.DispatcherList = await GetDispatcherSelectList();
            return View(model);
        }

        // POST: /Staff/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _UserManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (user.UserType != model.UserType)
            {
                ModelState.AddModelError("UserType", "Không thể thay đổi vai trò của tài khoản sau khi tạo (Do khác biệt cấu trúc CSDL). Hãy tạo mới nếu cần.");
                ViewBag.DispatcherList = await GetDispatcherSelectList();
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            if (user is TaiXe taiXe)
            {
                taiXe.DieuHanhId = model.DieuHanhId;
            }

            // Kiểm tra username thay đổi
            if (user.UserName != model.UserName)
            {
                var existingUser = await _UserManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }
                user.UserName = model.UserName;
                user.NormalizedUserName = model.UserName.ToUpper();
            }

            user.NormalizedEmail = model.Email.ToUpper();

            var updateResult = await _UserManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.DispatcherList = await GetDispatcherSelectList();
                return View(model);
            }

            // Đổi mật khẩu nếu có
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _UserManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _UserManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    ViewBag.DispatcherList = await GetDispatcherSelectList();
                    return View(model);
                }
            }

            TempData["Success"] = $"Đã cập nhật nhân sự \"{model.FullName}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Staff/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Không cho xóa chính mình
            var currentUser = await _UserManager.GetUserAsync(User);
            if (currentUser?.Id == user.Id)
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _UserManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Đã xóa nhân sự \"{user.FullName}\" thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi khi xóa nhân sự. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
