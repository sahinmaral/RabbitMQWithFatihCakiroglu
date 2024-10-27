using CreateExcel.Shared;
using CreateExcel.Web.Models;
using CreateExcel.Web.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreateExcel.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(AppDbContext dbContext, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new UserFile()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Created
            };

            await _dbContext.UserFiles.AddAsync(userFile);

            await _dbContext.SaveChangesAsync();

            _rabbitMQPublisher.Publish(new CreateExcelMessage()
            {
                UserId = user.Id,
                FileId = userFile.Id
            });

            TempData["StartCreatingExcel"] = true;

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userFilesOfUser = await _dbContext.UserFiles.Where(uf => uf.UserId == user.Id).OrderByDescending(uf => uf.CreatedAt).ToListAsync();

            return View(userFilesOfUser);
        }
    }
}
