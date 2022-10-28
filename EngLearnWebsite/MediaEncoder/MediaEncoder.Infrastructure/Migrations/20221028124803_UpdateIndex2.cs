using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaEncoder.Infrastructure.Migrations
{
    public partial class UpdateIndex2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_ME_EncodingItems_Status_FileSHA256Hash_FileSizeInByte",
                table: "T_ME_EncodingItems");

            migrationBuilder.CreateIndex(
                name: "IX_T_ME_EncodingItems_FileSHA256Hash_FileSizeInByte",
                table: "T_ME_EncodingItems",
                columns: new[] { "FileSHA256Hash", "FileSizeInByte" });

            migrationBuilder.CreateIndex(
                name: "IX_T_ME_EncodingItems_Status",
                table: "T_ME_EncodingItems",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_ME_EncodingItems_FileSHA256Hash_FileSizeInByte",
                table: "T_ME_EncodingItems");

            migrationBuilder.DropIndex(
                name: "IX_T_ME_EncodingItems_Status",
                table: "T_ME_EncodingItems");

            migrationBuilder.CreateIndex(
                name: "IX_T_ME_EncodingItems_Status_FileSHA256Hash_FileSizeInByte",
                table: "T_ME_EncodingItems",
                columns: new[] { "Status", "FileSHA256Hash", "FileSizeInByte" });
        }
    }
}
