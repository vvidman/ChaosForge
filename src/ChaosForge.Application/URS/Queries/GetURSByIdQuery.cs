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

public record URSDto(
    Guid Id,
    Guid UseCaseId,
    string Title,
    string Description,
    string? HumanEditNote,
    DateTime CreatedAt);

public record GetURSByIdQuery(Guid URSId) : IRequest<Result<URSDto>>;

internal sealed class GetURSByIdQueryHandler(IURSRepository ursRepository)
    : IRequestHandler<GetURSByIdQuery, Result<URSDto>>
{
    public async Task<Result<URSDto>> Handle(
        GetURSByIdQuery request,
        CancellationToken cancellationToken)
    {
        var urs = await ursRepository.GetByIdAsync(request.URSId, cancellationToken);

        if (urs is null)
        {
            return Result<URSDto>.Failure("URS not found.");
        }

        var dto = new URSDto(
            urs.Id,
            urs.UseCaseId,
            urs.Title,
            urs.Description,
            urs.HumanEditNote,
            urs.CreatedAt);

        return Result<URSDto>.Success(dto);
    }
}
