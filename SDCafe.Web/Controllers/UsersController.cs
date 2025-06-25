using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(r => new { Value = (int)r, Text = r.ToString() }), 
                "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                if (!await _userService.IsEmailUniqueAsync(user.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                        Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new { Value = (int)r, Text = r.ToString() }), 
                        "Value", "Text");
                    return View(user);
                }

                await _userService.AddAsync(user);
                TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(r => new { Value = (int)r, Text = r.ToString() }), 
                "Value", "Text");
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(r => new { Value = (int)r, Text = r.ToString() }), 
                "Value", "Text");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!await _userService.IsEmailUniqueAsync(user.Email, id))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                        Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new { Value = (int)r, Text = r.ToString() }), 
                        "Value", "Text");
                    return View(user);
                }

                user.UpdatedDate = DateTime.Now;
                await _userService.UpdateAsync(user);
                TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(r => new { Value = (int)r, Text = r.ToString() }), 
                "Value", "Text");
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.UpdatedDate = DateTime.Now;
                await _userService.UpdateAsync(user);
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("", "Yeni parola boş olamaz.");
                return View(user);
            }

            var success = await _userService.UpdatePasswordAsync(id, newPassword);
            if (success)
            {
                TempData["Success"] = "Parola başarıyla değiştirildi.";
            }
            else
            {
                TempData["Error"] = "Parola değiştirilirken hata oluştu.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
} 