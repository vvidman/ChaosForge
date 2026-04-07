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

namespace ChaosForge.Application.SRS.Queries;

public record SRSDto(
    Guid Id,
    Guid URSId,
    string Title,
    string TechnicalDescription,
    string? HumanEditNote,
    DateTime CreatedAt);

public record GetSRSByIdQuery(Guid SRSId) : IRequest<Result<SRSDto>>;

internal sealed class GetSRSByIdQueryHandler(ISRSRepository srsRepository)
    : IRequestHandler<GetSRSByIdQuery, Result<SRSDto>>
{
    public async Task<Result<SRSDto>> Handle(
        GetSRSByIdQuery request,
        CancellationToken cancellationToken)
    {
        var srs = await srsRepository.GetByIdAsync(request.SRSId, cancellationToken);

        if (srs is null)
        {
            return Result<SRSDto>.Failure("SRS not found.");
        }

        var dto = new SRSDto(
            srs.Id,
            srs.URSId,
            srs.Title,
            srs.TechnicalDescription,
            srs.HumanEditNote,
            srs.CreatedAt);

        return Result<SRSDto>.Success(dto);
    }
}
