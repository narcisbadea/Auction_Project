using Auction_Project.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auction_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageTestController : ControllerBase
    {
        private readonly IUserService _userService;

        public ImageTestController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<string>> stImage(IEnumerable<IFormFile> files)
        {
            string uploads = Path.Combine(Directory.GetCurrentDirectory(), @"uploads\", _userService.GetMyName());
            try
            {
                if (Directory.Exists(uploads))
                {
                    return BadRequest("That path exists already.");
                }

                DirectoryInfo dir = Directory.CreateDirectory(uploads);

            }
            catch (Exception e)
            {
                return BadRequest($"The process failed: {e.ToString()}");
            }
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string filePath = Path.Combine(uploads, file.FileName);
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
            return Ok("Uploaded!");
        }

        [HttpGet]
        public IActionResult Get()
        {
            Byte[] b = System.IO.File.ReadAllBytes(@"C:\Users\ionbadea\source\repos\narcisbadea\Auction_Project\Auction_Project\uploads\first.png");   // You can use your own method over here.
            var a = File(b, "image/png");
            return a;
        }
    }
}
