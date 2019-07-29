using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DBModels.Migrations
{
    public partial class DBHrLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_hr_authority",
                columns: table => new
                {
                    InsertTime = table.Column<DateTime>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    Mark = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AbilityName = table.Column<string>(maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_hr_authority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_hr_menu",
                columns: table => new
                {
                    InsertTime = table.Column<DateTime>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    Mark = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MenuName = table.Column<string>(maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_hr_menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_hr_userrole",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Mark = table.Column<int>(maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_hr_userrole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_hr_role",
                columns: table => new
                {
                    InsertTime = table.Column<DateTime>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    Mark = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 30, nullable: false),
                    AuthorityId = table.Column<long>(nullable: true),
                    UserRoleId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_hr_role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_hr_role_t_hr_authority_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "t_hr_authority",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_hr_role_t_hr_userrole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "t_hr_userrole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_hr_user",
                columns: table => new
                {
                    InsertTime = table.Column<DateTime>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    Mark = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(maxLength: 30, nullable: false),
                    Password = table.Column<string>(maxLength: 30, nullable: false),
                    Email = table.Column<string>(maxLength: 30, nullable: true),
                    Phone = table.Column<string>(maxLength: 30, nullable: true),
                    Image = table.Column<byte[]>(maxLength: 2000, nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    UserRoleId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_hr_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_hr_user_t_hr_userrole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "t_hr_userrole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_hr_role_AuthorityId",
                table: "t_hr_role",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_t_hr_role_UserRoleId",
                table: "t_hr_role",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_t_hr_user_UserRoleId",
                table: "t_hr_user",
                column: "UserRoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_hr_menu");

            migrationBuilder.DropTable(
                name: "t_hr_role");

            migrationBuilder.DropTable(
                name: "t_hr_user");

            migrationBuilder.DropTable(
                name: "t_hr_authority");

            migrationBuilder.DropTable(
                name: "t_hr_userrole");
        }
    }
}
