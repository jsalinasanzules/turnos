using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnos.Api.Services;

namespace Turnos.Api.Controllers;

[ApiController, Authorize, Route("api/clients")]
public sealed class ClientsController(ClientService clients) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await clients.ListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(ClientRequest request)
    {
        try { return Created("/api/clients", new { id = await clients.CreateAsync(request, RequestUser.Id(User)) }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") { return Conflict(new { message = "La identificación o correo ya está registrado." }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ClientRequest request)
    {
        try { await clients.UpdateAsync(id, request, RequestUser.Id(User)); return NoContent(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") { return Conflict(new { message = "La identificación o correo ya está registrado." }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { await clients.SoftDeleteAsync(id, RequestUser.Id(User)); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(BulkClientsRequest request)
    {
        try { return Ok(await clients.ImportAsync(request.Clients, RequestUser.Id(User))); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
public sealed record BulkClientsRequest(List<ClientRequest> Clients);
