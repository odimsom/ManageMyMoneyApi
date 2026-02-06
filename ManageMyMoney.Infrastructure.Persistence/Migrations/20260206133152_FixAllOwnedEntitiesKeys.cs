using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageMyMoney.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAllOwnedEntitiesKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    related_entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budgets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    limit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    limit_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    spent_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    spent_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    alerts_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    category_ids = table.Column<List<Guid>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budgets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category_budgets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    limit_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    spent_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    spent_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alert_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    alert_threshold = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_budgets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    decimal_places = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "email_verification_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verification_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    to_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expense_splits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_splits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "income_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incomes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    income_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recurring_income_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incomes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_four_digits = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recurring_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recurrence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_generated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_expenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recurring_incomes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recurrence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: false),
                    income_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_generated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_incomes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    related_entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recurrence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_sent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "savings_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    target_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    target_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    current_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    target_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_savings_goals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    category_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    phone_country_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    preferred_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subcategories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subcategories", x => x.id);
                    table.ForeignKey(
                        name: "f_k_subcategories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "goal_contributions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    savings_goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_contributions", x => x.id);
                    table.ForeignKey(
                        name: "f_k_goal_contributions_savings_goals_savings_goal_id",
                        column: x => x.savings_goal_id,
                        principalTable: "savings_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    include_in_total = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    device_info = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "credit_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_limit_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_balance_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    statement_closing_day = table.Column<int>(type: "integer", nullable: false),
                    payment_due_day = table.Column<int>(type: "integer", nullable: false),
                    interest_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_cards", x => x.id);
                    table.ForeignKey(
                        name: "f_k_credit_cards_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recurring_expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subcategory_id = table.Column<Guid>(type: "uuid", nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.ForeignKey(
                        name: "f_k_expenses_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_expenses_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_expenses_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_expenses_recurring_expenses_recurring_expense_id",
                        column: x => x.recurring_expense_id,
                        principalTable: "recurring_expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_expenses_subcategories_subcategory_id",
                        column: x => x.subcategory_id,
                        principalTable: "subcategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_expenses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expense_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_attachments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_expense_attachments_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expense_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_tags", x => x.id);
                    table.ForeignKey(
                        name: "f_k_expense_tags_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_transactions_date",
                table: "account_transactions",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_account_transactions_from_account",
                table: "account_transactions",
                column: "from_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_account_transactions_to_account",
                table: "account_transactions",
                column: "to_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_account_transactions_user_id",
                table: "account_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_user_active",
                table: "accounts",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_user_name",
                table: "accounts",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_type",
                table: "alerts",
                column: "alert_type");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_user_acknowledged",
                table: "alerts",
                columns: new[] { "user_id", "is_acknowledged" });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_user_id",
                table: "alerts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_budgets_user_active",
                table: "budgets",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_budgets_user_id",
                table: "budgets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_user_name",
                table: "categories",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_user_type",
                table: "categories",
                columns: new[] { "user_id", "transaction_type" });

            migrationBuilder.CreateIndex(
                name: "ix_category_budgets_user_category",
                table: "category_budgets",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_credit_cards_account_id",
                table: "credit_cards",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currencies_active",
                table: "currencies",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_token",
                table: "email_verification_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_user_id",
                table: "email_verification_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_currencies_date",
                table: "exchange_rates",
                columns: new[] { "from_currency", "to_currency", "effective_date" });

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_effective_date",
                table: "exchange_rates",
                column: "effective_date");

            migrationBuilder.CreateIndex(
                name: "ix_expense_attachments_expense_id",
                table: "expense_attachments",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "ix_expense_splits_expense_id",
                table: "expense_splits",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "ix_expense_splits_participant_id",
                table: "expense_splits",
                column: "participant_user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_expense_tags_expense_id",
                table: "expense_tags",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "ix_expense_tags_user_name",
                table: "expense_tags",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_expenses_payment_method_id",
                table: "expenses",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "i_x_expenses_recurring_expense_id",
                table: "expenses",
                column: "recurring_expense_id");

            migrationBuilder.CreateIndex(
                name: "i_x_expenses_subcategory_id",
                table: "expenses",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_account_id",
                table: "expenses",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_category_id",
                table: "expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_date",
                table: "expenses",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_user_date",
                table: "expenses",
                columns: new[] { "user_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_expenses_user_id",
                table: "expenses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_goal_contributions_date",
                table: "goal_contributions",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_goal_contributions_goal_id",
                table: "goal_contributions",
                column: "savings_goal_id");

            migrationBuilder.CreateIndex(
                name: "ix_income_sources_user_name",
                table: "income_sources",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_incomes_date",
                table: "incomes",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_incomes_user_date",
                table: "incomes",
                columns: new[] { "user_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_incomes_user_id",
                table: "incomes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_token",
                table: "password_reset_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_user_default",
                table: "payment_methods",
                columns: new[] { "user_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_user_id",
                table: "payment_methods",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_next_due",
                table: "recurring_expenses",
                column: "next_due_date");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_user_active",
                table: "recurring_expenses",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_user_id",
                table: "recurring_expenses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_incomes_user_active",
                table: "recurring_incomes",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_recurring_incomes_user_id",
                table: "recurring_incomes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reminders_due_date",
                table: "reminders",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_reminders_user_completed",
                table: "reminders",
                columns: new[] { "user_id", "is_completed" });

            migrationBuilder.CreateIndex(
                name: "ix_reminders_user_id",
                table: "reminders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_savings_goals_user_id",
                table: "savings_goals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_savings_goals_user_status",
                table: "savings_goals",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_subcategories_category_name",
                table: "subcategories",
                columns: new[] { "category_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_country",
                table: "tax_rates",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_country_active",
                table: "tax_rates",
                columns: new[] { "country_code", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_active",
                table: "user_sessions",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_is_active",
                table: "users",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_transactions");

            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "budgets");

            migrationBuilder.DropTable(
                name: "category_budgets");

            migrationBuilder.DropTable(
                name: "credit_cards");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "email_verification_tokens");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "expense_attachments");

            migrationBuilder.DropTable(
                name: "expense_splits");

            migrationBuilder.DropTable(
                name: "expense_tags");

            migrationBuilder.DropTable(
                name: "goal_contributions");

            migrationBuilder.DropTable(
                name: "income_sources");

            migrationBuilder.DropTable(
                name: "incomes");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "recurring_incomes");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "tax_rates");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "savings_goals");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "recurring_expenses");

            migrationBuilder.DropTable(
                name: "subcategories");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
