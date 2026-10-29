using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GeneratedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    SnapshotsAnalyzed = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    CpuPercent = table.Column<double>(type: "REAL", nullable: false),
                    MemoryUsedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    MemoryTotalBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    DiskReadBps = table.Column<long>(type: "INTEGER", nullable: false),
                    DiskWriteBps = table.Column<long>(type: "INTEGER", nullable: false),
                    GpuDeviceName = table.Column<string>(type: "TEXT", nullable: true),
                    GpuUtilizationPercent = table.Column<double>(type: "REAL", nullable: true),
                    GpuTemperatureCelsius = table.Column<double>(type: "REAL", nullable: true),
                    GpuMemoryUsedBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    GpuMemoryTotalBytes = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Anomalies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyReportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OccurredAt = table.Column<long>(type: "INTEGER", nullable: false),
                    Metric = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    ObservedValue = table.Column<double>(type: "REAL", nullable: false),
                    Threshold = table.Column<double>(type: "REAL", nullable: true),
                    CulpritPid = table.Column<int>(type: "INTEGER", nullable: true),
                    CulpritName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anomalies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anomalies_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopConsumers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyReportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessPid = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Metric = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageValue = table.Column<double>(type: "REAL", nullable: false),
                    PeakValue = table.Column<double>(type: "REAL", nullable: false),
                    TotalObservedTime = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopConsumers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopConsumers_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Pid = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CpuPercent = table.Column<double>(type: "REAL", nullable: false),
                    MemoryWorkingSetBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    DiskReadBps = table.Column<long>(type: "INTEGER", nullable: false),
                    DiskWriteBps = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processes_Snapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "Snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_CulpritPid",
                table: "Anomalies",
                column: "CulpritPid");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_DailyReportId",
                table: "Anomalies",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_Date",
                table: "DailyReports",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Pid",
                table: "Processes",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_SnapshotId",
                table: "Processes",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_Timestamp",
                table: "Snapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvents_Timestamp",
                table: "SystemEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TopConsumers_DailyReportId",
                table: "TopConsumers",
                column: "DailyReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anomalies");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "SystemEvents");

            migrationBuilder.DropTable(
                name: "TopConsumers");

            migrationBuilder.DropTable(
                name: "Snapshots");

            migrationBuilder.DropTable(
                name: "DailyReports");
        }
    }
}
