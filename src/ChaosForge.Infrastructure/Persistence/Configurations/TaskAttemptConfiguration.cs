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

internal sealed class TaskAttemptConfiguration : IEntityTypeConfiguration<TaskAttempt>
{
    public void Configure(EntityTypeBuilder<TaskAttempt> builder)
    {
        builder.ToTable("TaskAttempts");
        builder.ConfigureEntityBase();

        builder.Property(e => e.WorkTaskId).IsRequired();

        builder.HasOne<WorkTask>()
            .WithMany()
            .HasForeignKey(e => e.WorkTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // AgentInstanceId is a plain column — no FK constraint, no nav property
        builder.Property(e => e.AgentInstanceId).IsRequired();

        builder.Property(e => e.Type).IsRequired();

        builder.Property(e => e.Output)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.ReviewNote)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(e => e.TestNote)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(e => e.Result).IsRequired();

        builder.Property(e => e.StartedAt).IsRequired();

        builder.Property(e => e.CompletedAt).IsRequired(false);
    }
}
