using Api.DTOs;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = "ManagerOrAdmin")]
public sealed class AuditLogsController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] long? userId = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var result = await auditLogService.GetPagedAsync(
            page, pageSize, search, entityType, action, userId, fromUtc, toUtc, cancellationToken);

        return Ok(ApiResponse<PagedResultDto<AuditLogDto>>.Ok(result));
    }
}