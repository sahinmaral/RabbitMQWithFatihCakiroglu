using CreateExcel.Web.Hubs;
using CreateExcel.Web.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CreateExcel.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext dbContext, IHubContext<MyHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadAsync(IFormFile file, int fileId)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            var userFile = await _dbContext.UserFiles.FirstOrDefaultAsync(x => x.Id == fileId);
            if (userFile == null)
                return BadRequest();

            var filePath = userFile.FileName + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            userFile.CreatedAt = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

            return Ok();
        }
    }
}
