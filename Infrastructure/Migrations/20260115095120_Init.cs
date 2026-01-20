using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractionMethod = table.Column<string>(type: "text", nullable: false),
                    MerchantName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "VND"),
                    BillDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bills_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bill_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 1m),
                    UnitPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ItemConfidence = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bill_items_bills_BillId",
                        column: x => x.BillId,
                        principalTable: "bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: false),
                    MethodType = table.Column<string>(type: "text", nullable: false),
                    BankCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankAccount = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    AccountHolder = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TransactionAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "VND"),
                    PaymentContent = table.Column<string>(type: "character varying(19)", maxLength: 19, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_methods_bills_BillId",
                        column: x => x.BillId,
                        principalTable: "bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bill_items_BillId",
                table: "bill_items",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_bills_UserId",
                table: "bills",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_BillId",
                table: "payment_methods",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_items");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "bills");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
