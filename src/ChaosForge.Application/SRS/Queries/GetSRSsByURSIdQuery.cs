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

public record GetSRSsByURSIdQuery(Guid URSId) : IRequest<Result<IReadOnlyList<SRSDto>>>;

internal sealed class GetSRSsByURSIdQueryHandler(ISRSRepository srsRepository)
    : IRequestHandler<GetSRSsByURSIdQuery, Result<IReadOnlyList<SRSDto>>>
{
    public async Task<Result<IReadOnlyList<SRSDto>>> Handle(
        GetSRSsByURSIdQuery request,
        CancellationToken cancellationToken)
    {
        var srsList = await srsRepository.GetByURSIdAsync(request.URSId, cancellationToken);

        var dtos = srsList
            .Select(s => new SRSDto(s.Id, s.URSId, s.Title, s.TechnicalDescription, s.HumanEditNote, s.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<SRSDto>>.Success(dtos);
    }
}
