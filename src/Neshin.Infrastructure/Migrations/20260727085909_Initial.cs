using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neshin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clients");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    is_phone_number_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    phone_number_verified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                schema: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    accepts_app_orders = table.Column<bool>(type: "boolean", nullable: false),
                    allows_pay_at_venue = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    public_phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.id);
                    table.ForeignKey(
                        name: "FK_branches_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "clients",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_profiles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    is_phone_number_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_profiles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menus", x => x.id);
                    table.ForeignKey(
                        name: "FK_menus_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "clients",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venue_events",
                schema: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venue_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_venue_events_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "clients",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branch_customers",
                schema: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    contact_phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    added_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    archived_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_customers_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "clients",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_branch_customers_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "identity",
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_sessions",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_sessions_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "identity",
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    allows_phone_contact = table.Column<bool>(type: "boolean", nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accepted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ready_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejected_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "clients",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "identity",
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venue_visits",
                schema: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    distance_meters = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    accuracy_meters = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venue_visits", x => x.id);
                    table.ForeignKey(
                        name: "FK_venue_visits_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "clients",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venue_visits_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "identity",
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_menu_items_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "catalog",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_branch_customers_branch_id_customer_id",
                schema: "clients",
                table: "branch_customers",
                columns: new[] { "branch_id", "customer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_branch_customers_customer_id",
                schema: "clients",
                table: "branch_customers",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_branches_client_id",
                schema: "clients",
                table: "branches",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_branches_latitude_longitude",
                schema: "clients",
                table: "branches",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_customer_profiles_user_id",
                schema: "identity",
                table: "customer_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_sessions_customer_id_expires_at_utc",
                schema: "identity",
                table: "customer_sessions",
                columns: new[] { "customer_id", "expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_customer_sessions_token_hash",
                schema: "identity",
                table: "customer_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_menu_id_is_available_display_order",
                schema: "catalog",
                table: "menu_items",
                columns: new[] { "menu_id", "is_available", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_menus_branch_id",
                schema: "catalog",
                table: "menus",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                schema: "ordering",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_branch_id_created_at_utc",
                schema: "ordering",
                table: "orders",
                columns: new[] { "branch_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_branch_id_status_created_at_utc",
                schema: "ordering",
                table: "orders",
                columns: new[] { "branch_id", "status", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_id_idempotency_key",
                schema: "ordering",
                table: "orders",
                columns: new[] { "customer_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                schema: "ordering",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number",
                schema: "identity",
                table: "users",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venue_events_branch_id_is_published_starts_at_utc_ends_at_u~",
                schema: "clients",
                table: "venue_events",
                columns: new[] { "branch_id", "is_published", "starts_at_utc", "ends_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_venue_visits_branch_id_last_seen_at_utc",
                schema: "clients",
                table: "venue_visits",
                columns: new[] { "branch_id", "last_seen_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_venue_visits_customer_id_branch_id_ended_at_utc",
                schema: "clients",
                table: "venue_visits",
                columns: new[] { "customer_id", "branch_id", "ended_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_customers",
                schema: "clients");

            migrationBuilder.DropTable(
                name: "customer_sessions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "menu_items",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "order_items",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "venue_events",
                schema: "clients");

            migrationBuilder.DropTable(
                name: "venue_visits",
                schema: "clients");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "branches",
                schema: "clients");

            migrationBuilder.DropTable(
                name: "customer_profiles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "clients");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");
        }
    }
}
