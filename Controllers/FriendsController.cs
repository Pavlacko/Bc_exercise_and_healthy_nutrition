using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.Status == "Accepted")
            .ToList();

        var requests = _db.FriendRequests
            .Include(fr => fr.Sender)
            .Where(fr => fr.ReceiverId == userId && fr.Status == "Pending")
            .ToList();

        ViewBag.Friends = friends;
        ViewBag.Requests = requests;

        return View();
    }

    [HttpGet]
    public IActionResult Search(string query)
    {
        var users = _db.Users
            .Where(u => u.Meno.Contains(query))
            .Select(u => new { u.Id, u.Meno })
            .Take(10)
            .ToList();

        return Json(users);
    }

    [HttpPost]
    public IActionResult SendRequest([FromBody] SendRequestDto dto)
    {
        int senderId = HttpContext.Session.GetInt32("UserId") ?? 0;

        if (senderId == dto.ReceiverId)
            return BadRequest();

        bool exists = _db.FriendRequests.Any(fr =>
            (fr.SenderId == senderId && fr.ReceiverId == dto.ReceiverId) ||
            (fr.SenderId == dto.ReceiverId && fr.ReceiverId == senderId));

        if (exists)
            return BadRequest("Request already exists");

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId
        };

        _db.FriendRequests.Add(request);
        _db.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public IActionResult Accept(int id)
    {
        var req = _db.FriendRequests.Find(id);
        if (req == null) return NotFound();

        req.Status = "Accepted";
        _db.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public IActionResult Reject(int id)
    {
        var req = _db.FriendRequests.Find(id);
        if (req == null) return NotFound();

        req.Status = "Rejected";
        _db.SaveChanges();

        return Ok();
    }
}