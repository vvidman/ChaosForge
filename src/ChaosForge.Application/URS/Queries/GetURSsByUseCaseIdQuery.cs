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

namespace ChaosForge.Application.URS.Queries;

public record GetURSsByUseCaseIdQuery(Guid UseCaseId) : IRequest<Result<IReadOnlyList<URSDto>>>;

internal sealed class GetURSsByUseCaseIdQueryHandler(IURSRepository ursRepository)
    : IRequestHandler<GetURSsByUseCaseIdQuery, Result<IReadOnlyList<URSDto>>>
{
    public async Task<Result<IReadOnlyList<URSDto>>> Handle(
        GetURSsByUseCaseIdQuery request,
        CancellationToken cancellationToken)
    {
        var ursList = await ursRepository.GetByUseCaseIdAsync(request.UseCaseId, cancellationToken);

        var dtos = ursList
            .Select(u => new URSDto(u.Id, u.UseCaseId, u.Title, u.Description, u.HumanEditNote, u.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<URSDto>>.Success(dtos);
    }
}
