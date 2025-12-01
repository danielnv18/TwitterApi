using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitterCloneApi.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddUserProfileFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FollowerCount",
            table: "Users",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "FollowingCount",
            table: "Users",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "PostCount",
            table: "Users",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FollowerCount",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "FollowingCount",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PostCount",
            table: "Users");
    }
}
