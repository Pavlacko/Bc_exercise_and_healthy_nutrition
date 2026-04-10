using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireLogin]
    public class FriendsController : Controller
    {
        private readonly AppDbContext _db;

        public FriendsController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var friends = _db.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Where(fr =>
                    (fr.SenderId == userId || fr.ReceiverId == userId) &&
                    fr.Status == "Accepted")
                .OrderByDescending(fr => fr.CreatedAt)
                .ToList();

            var requests = _db.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == userId && fr.Status == "Pending")
                .OrderByDescending(fr => fr.CreatedAt)
                .ToList();

            var sentRequests = _db.FriendRequests
                .Include(fr => fr.Receiver)
                .Where(fr => fr.SenderId == userId && fr.Status == "Pending")
                .OrderByDescending(fr => fr.CreatedAt)
                .ToList();

            ViewBag.Friends = friends;
            ViewBag.Requests = requests;
            ViewBag.SentRequests = sentRequests;

            return View();
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var users = _db.Users
                .Where(u => u.Id != currentUserId && u.Meno.Contains(query))
                .OrderBy(u => u.Meno)
                .Take(10)
                .Select(u => new
                {
                    id = u.Id,
                    meno = u.Meno,
                    email = u.Email,
                    visibility = u.ProfileVisibility.ToString()
                })
                .ToList();

            return Json(users);
        }

        [HttpPost]
        public IActionResult SendRequest([FromBody] SendRequestDto dto)
        {
            int senderId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (senderId == 0)
                return Unauthorized();

            if (dto == null || dto.ReceiverId <= 0)
                return BadRequest("Neplatný používateľ.");

            if (senderId == dto.ReceiverId)
                return BadRequest("Nemôžeš poslať žiadosť sám sebe.");

            bool exists = _db.FriendRequests.Any(fr =>
                (fr.SenderId == senderId && fr.ReceiverId == dto.ReceiverId) ||
                (fr.SenderId == dto.ReceiverId && fr.ReceiverId == senderId));

            if (exists)
                return BadRequest("Žiadosť už existuje alebo ste už priatelia.");

            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.FriendRequests.Add(request);
            _db.SaveChanges();

            return Ok(new { message = "Žiadosť odoslaná." });
        }

        [HttpPost]
        public IActionResult Accept(int id)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var req = _db.FriendRequests.FirstOrDefault(fr => fr.Id == id && fr.ReceiverId == currentUserId);
            if (req == null)
                return NotFound();

            req.Status = "Accepted";
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var req = _db.FriendRequests.FirstOrDefault(fr => fr.Id == id && fr.ReceiverId == currentUserId);
            if (req == null)
                return NotFound();

            req.Status = "Rejected";
            _db.SaveChanges();

            return Ok();
        }
    }
}