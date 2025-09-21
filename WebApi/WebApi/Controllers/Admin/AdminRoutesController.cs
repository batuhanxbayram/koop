using Koop.Data.Context; 
using Koop.Entity.DTOs.Vehicle;
using Koop.Entity.Entities; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/admin/routes")]
[ApiController]
[Authorize(Roles = "Admin")] 
public class AdminRoutesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminRoutesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoutes()
    {
        var routes = await _context.Routes
            .OrderBy(r => r.RouteName) 
            .ToListAsync();

        return Ok(routes);
    }



    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto createRouteDto)
    {
        if (await _context.Routes.AnyAsync(r => r.RouteName == createRouteDto.RouteName))
        {
            return BadRequest("Bu rota adı zaten mevcut.");
        }

        var route = new Koop.Entity.Entities.Route
        {
            RouteName = createRouteDto.RouteName,
            IsActive = true
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRouteById), new { id = route.Id }, route);
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteById(long id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound();
        }
        return Ok(route);
    }

    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(long id, [FromBody] UpdateRouteDto updateRouteDto)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound("Güncellenecek rota bulunamadı.");
        }

        route.RouteName = updateRouteDto.RouteName;
        route.IsActive = updateRouteDto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpPatch("{id}/set-active")]
    public async Task<IActionResult> SetRouteActiveStatus(long id, [FromBody] SetActiveDto setActiveDto)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound("Rota bulunamadı.");
        }

        route.IsActive = setActiveDto.IsActive;
        await _context.SaveChangesAsync();

        return NoContent();
    }

   
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(long id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound("Silinecek rota bulunamadı.");
        }
        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}