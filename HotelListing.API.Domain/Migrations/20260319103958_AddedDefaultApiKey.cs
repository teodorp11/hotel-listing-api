using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ApiKeys",
                columns: new[] { "Id", "AppName", "CreatedAtUtc", "ExpiresAtUtc", "Key" },
                values: new object[] { 1, "app", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 2, 0, 0, 0)), null, "dXNlcjZAbG9jYWxob3N0LmNvbTpQQHNzd29yZDE=" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9389a22d-1829-4332-9c91-f7ad9bb21bc7",
                column: "ConcurrencyStamp",
                value: "2de9c92e-0c3f-4edc-97b8-26f3fea1423a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c97aba77-7558-4a15-98e3-b56e3da45128",
                column: "ConcurrencyStamp",
                value: "333e5bb6-8d97-4f9e-aa51-32b40c394a1f");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApiKeys",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9389a22d-1829-4332-9c91-f7ad9bb21bc7",
                column: "ConcurrencyStamp",
                value: "3d50969b-356e-4e70-9c13-4beed05dd94a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c97aba77-7558-4a15-98e3-b56e3da45128",
                column: "ConcurrencyStamp",
                value: "0ebea017-f071-4246-b22d-c8517c96f5d2");
        }
    }
}
