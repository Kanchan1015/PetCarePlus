using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoUrlColumnFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous migration 'AddPhotoUrlToInventoryItem' did not add the column.
            // Add the missing nullable PhotoUrl column to the InventoryItems table.
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "InventoryItems",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove PhotoUrl column on rollback
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "InventoryItems");
        }
    }
}
