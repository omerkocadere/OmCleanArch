using CleanArch.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Web.Api.Controllers;

public class GetDbErrorController(ApplicationDbContext db) : BaseApiController
{
    [HttpGet()]
    public IActionResult ForceDbError()
    {
        // deliberately query a non-existent table or column to trigger a database error
        var _ = db.Database.ExecuteSqlRaw("SELECT * FROM NonExistentTable");
        return Ok();
    }
}
