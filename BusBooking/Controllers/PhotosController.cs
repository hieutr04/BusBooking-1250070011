using BusBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BusBooking.Controllers;
[AllowAnonymous]
public sealed class PhotosController : Controller {
    [HttpGet("/nguon-anh")]
    public IActionResult Index() => View(PhotoCatalog.All);
}
