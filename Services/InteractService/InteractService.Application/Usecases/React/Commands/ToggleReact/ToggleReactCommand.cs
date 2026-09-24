using BuildingBlocks.DTOs;
using InteractService.Application.Data;
using InteractService.Application.DTOs.React.Requests;
using InteractService.Application.DTOs.React.Responses;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Application.Usecases.React.Commands.ToggleReact;

public record ToggleReactCommand(ToggleReactRequest Request) : ICommand<ResponseDto>;
