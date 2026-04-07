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

public record GetUseCasesByProjectIdQuery(Guid ProjectId) : IRequest<Result<IReadOnlyList<UseCaseDto>>>;

internal sealed class GetUseCasesByProjectIdQueryHandler(IUseCaseRepository useCaseRepository)
    : IRequestHandler<GetUseCasesByProjectIdQuery, Result<IReadOnlyList<UseCaseDto>>>
{
    public async Task<Result<IReadOnlyList<UseCaseDto>>> Handle(
        GetUseCasesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var useCases = await useCaseRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = useCases
            .Select(uc => new UseCaseDto(uc.Id, uc.ProjectId, uc.Title, uc.Description, uc.Priority, uc.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<UseCaseDto>>.Success(dtos);
    }
}
