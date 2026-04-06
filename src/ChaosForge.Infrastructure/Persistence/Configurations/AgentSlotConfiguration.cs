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

using ChaosForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaosForge.Infrastructure.Persistence.Configurations;

internal sealed class AgentSlotConfiguration : IEntityTypeConfiguration<AgentSlot>
{
    public void Configure(EntityTypeBuilder<AgentSlot> builder)
    {
        builder.ToTable("AgentSlots");
        builder.ConfigureEntityBase();

        builder.Property(e => e.ProjectId).IsRequired();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Role).IsRequired();

        builder.Property(e => e.Count).IsRequired();
    }
}
