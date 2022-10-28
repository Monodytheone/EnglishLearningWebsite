using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaEncoder.Infrastructure.Migrations
{
    public partial class UpdateIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_ME_EncodingItems_Status_DestFormat_SourceSystem",
                table: "T_ME_EncodingItems");

            migrationBuilder.AlterColumn<string>(
                name: "SourceSystem",
                table: "T_ME_EncodingItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_T_ME_EncodingItems_Status_FileSHA256Hash_FileSizeInByte",
                table: "T_ME_EncodingItems",
                columns: new[] { "Status", "FileSHA256Hash", "FileSizeInByte" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_ME_EncodingItems_Status_FileSHA256Hash_FileSizeInByte",
                table: "T_ME_EncodingItems");

            migrationBuilder.AlterColumn<string>(
                name: "SourceSystem",
                table: "T_ME_EncodingItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_T_ME_EncodingItems_Status_DestFormat_SourceSystem",
                table: "T_ME_EncodingItems",
                columns: new[] { "Status", "DestFormat", "SourceSystem" });
        }
    }
}
