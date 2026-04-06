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

internal sealed class SRSConfiguration : IEntityTypeConfiguration<SRS>
{
    public void Configure(EntityTypeBuilder<SRS> builder)
    {
        builder.ToTable("SRSs");
        builder.ConfigureEntityBase();

        builder.Property(e => e.URSId).IsRequired();

        builder.HasOne<URS>()
            .WithMany()
            .HasForeignKey(e => e.URSId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TechnicalDescription)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.HumanEditNote)
            .IsRequired(false)
            .HasMaxLength(1000);
    }
}
