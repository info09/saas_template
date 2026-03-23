using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Common.Models;
using SaaS.Application.TodoItems.Commands.AssignTodoItem;
using SaaS.Application.TodoItems.Commands.CreateTodoItem;
using SaaS.Application.TodoItems.Commands.DeleteTodoItem;
using SaaS.Application.TodoItems.Commands.UpdateTodoItem;
using SaaS.Application.TodoItems.Commands.UpdateTodoItemStatus;
using SaaS.Application.TodoItems.Queries.GetTodosWithPagination;
using System.Security.Claims;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown_user";
    }

    [HttpGet]
    public async Task<ActionResult<Result<PaginatedList<TodoItemDto>>>> GetTodosWithPagination([FromQuery] GetTodosWithPaginationQuery query)
    {
        query.UserId = GetUserId();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTodoItemCommand command)
    {
        command.UserId = GetUserId();
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTodosWithPagination), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTodoItemCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch.");
        }

        command.UserId = GetUserId();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteTodoItemCommand(id, GetUserId());
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{id}/assign")]
    public async Task<IActionResult> Assign(int id, AssignTodoItemCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch.");
        command.UserId = GetUserId();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateTodoItemStatusCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch.");
        command.UserId = GetUserId();
        await _mediator.Send(command);
        return NoContent();
    }
}
