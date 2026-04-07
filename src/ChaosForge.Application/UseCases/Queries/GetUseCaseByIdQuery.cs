/*
   Copyright 2026 Viktor Vidman (vvidman)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using ChaosForge.Application.Common;
using ChaosForge.Domain.Repositories;
using MediatR;

namespace ChaosForge.Application.UseCases.Queries;

public record UseCaseDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    int Priority,
    DateTime CreatedAt);

public record GetUseCaseByIdQuery(Guid UseCaseId) : IRequest<Result<UseCaseDto>>;

internal sealed class GetUseCaseByIdQueryHandler(IUseCaseRepository useCaseRepository)
    : IRequestHandler<GetUseCaseByIdQuery, Result<UseCaseDto>>
{
    public async Task<Result<UseCaseDto>> Handle(
        GetUseCaseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var useCase = await useCaseRepository.GetByIdAsync(request.UseCaseId, cancellationToken);

        if (useCase is null)
        {
            return Result<UseCaseDto>.Failure("Use case not found.");
        }

        var dto = new UseCaseDto(
            useCase.Id,
            useCase.ProjectId,
            useCase.Title,
            useCase.Description,
            useCase.Priority,
            useCase.CreatedAt);

        return Result<UseCaseDto>.Success(dto);
    }
}
