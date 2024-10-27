using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CreateExcel.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(string email, string password)
        {
            var hasUser = await _userManager.FindByEmailAsync(email);

            if (hasUser is null)
                return View();

            var signInResult = await _signInManager.PasswordSignInAsync(
                    user: hasUser,
                    password,
                    isPersistent: true,
                    lockoutOnFailure: false
                );

            if(!signInResult.Succeeded)
                return View();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
