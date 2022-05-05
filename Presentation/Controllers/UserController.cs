using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Services.Abstractions;

namespace Presentation.Controllers
{
    [Authorize("Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> AddUser([FromForm] RegisterUserDto userDto)
        {
            var user = await _userService.AddUserAsync(userDto);
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Users()
        {
            var user = HttpContext.User.Claims;
            var users = await _userService.GetUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUserAsync(HttpContext.User, id);
            return RedirectToAction("Users");
        }

        public IActionResult CreateUser()
        {
            return View();
        }
    }
}
