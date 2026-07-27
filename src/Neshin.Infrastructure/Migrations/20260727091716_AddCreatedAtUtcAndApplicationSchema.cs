using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neshin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtUtcAndApplicationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "application");

            migrationBuilder.RenameTable(
                name: "venue_visits",
                schema: "clients",
                newName: "venue_visits",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "venue_events",
                schema: "clients",
                newName: "venue_events",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "identity",
                newName: "users",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "orders",
                schema: "ordering",
                newName: "orders",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "order_items",
                schema: "ordering",
                newName: "order_items",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "menus",
                schema: "catalog",
                newName: "menus",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "menu_items",
                schema: "catalog",
                newName: "menu_items",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "customer_sessions",
                schema: "identity",
                newName: "customer_sessions",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "customer_profiles",
                schema: "identity",
                newName: "customer_profiles",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "clients",
                schema: "clients",
                newName: "clients",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "branches",
                schema: "clients",
                newName: "branches",
                newSchema: "application");

            migrationBuilder.RenameTable(
                name: "branch_customers",
                schema: "clients",
                newName: "branch_customers",
                newSchema: "application");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "venue_visits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "venue_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "menus",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "menu_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "clients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "branches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                schema: "application",
                table: "branch_customers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "venue_visits");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "venue_events");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "menus");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                schema: "application",
                table: "branch_customers");

            migrationBuilder.EnsureSchema(
                name: "clients");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.RenameTable(
                name: "venue_visits",
                schema: "application",
                newName: "venue_visits",
                newSchema: "clients");

            migrationBuilder.RenameTable(
                name: "venue_events",
                schema: "application",
                newName: "venue_events",
                newSchema: "clients");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "application",
                newName: "users",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "orders",
                schema: "application",
                newName: "orders",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "order_items",
                schema: "application",
                newName: "order_items",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "menus",
                schema: "application",
                newName: "menus",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "menu_items",
                schema: "application",
                newName: "menu_items",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "customer_sessions",
                schema: "application",
                newName: "customer_sessions",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "customer_profiles",
                schema: "application",
                newName: "customer_profiles",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "clients",
                schema: "application",
                newName: "clients",
                newSchema: "clients");

            migrationBuilder.RenameTable(
                name: "branches",
                schema: "application",
                newName: "branches",
                newSchema: "clients");

            migrationBuilder.RenameTable(
                name: "branch_customers",
                schema: "application",
                newName: "branch_customers",
                newSchema: "clients");
        }
    }
}
