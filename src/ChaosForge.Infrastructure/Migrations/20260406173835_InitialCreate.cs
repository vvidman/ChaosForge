using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChaosForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonaName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentTaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentInstances_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentSlots_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionGates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AgentOutput = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    HumanEditedOutput = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Action = table.Column<int>(type: "INTEGER", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionGates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisionGates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UseCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UseCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UseCases_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "URSs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UseCaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    HumanEditNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_URSs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_URSs_UseCases_UseCaseId",
                        column: x => x.UseCaseId,
                        principalTable: "UseCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SRSs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    URSId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TechnicalDescription = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    HumanEditNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SRSs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SRSs_URSs_URSId",
                        column: x => x.URSId,
                        principalTable: "URSs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SRSId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SprintId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StoryPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTasks_SRSs_SRSId",
                        column: x => x.SRSId,
                        principalTable: "SRSs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgentInstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Output = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ReviewNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TestNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Result = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAttempts_WorkTasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_ProjectId",
                table: "AgentInstances",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSlots_ProjectId",
                table: "AgentSlots",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionGates_ProjectId",
                table: "RevisionGates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SRSs_URSId",
                table: "SRSs",
                column: "URSId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttempts_WorkTaskId",
                table: "TaskAttempts",
                column: "WorkTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_URSs_UseCaseId",
                table: "URSs",
                column: "UseCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UseCases_ProjectId",
                table: "UseCases",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_SRSId",
                table: "WorkTasks",
                column: "SRSId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentInstances");

            migrationBuilder.DropTable(
                name: "AgentSlots");

            migrationBuilder.DropTable(
                name: "RevisionGates");

            migrationBuilder.DropTable(
                name: "TaskAttempts");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropTable(
                name: "SRSs");

            migrationBuilder.DropTable(
                name: "URSs");

            migrationBuilder.DropTable(
                name: "UseCases");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
