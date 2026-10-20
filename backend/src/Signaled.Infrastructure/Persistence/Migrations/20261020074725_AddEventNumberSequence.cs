using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Signaled.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "EventNumberSequence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "EventNumberSequence");
        }
    }
}
