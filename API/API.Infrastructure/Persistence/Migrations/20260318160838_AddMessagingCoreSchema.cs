using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingCoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chats",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chat_type = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    last_message_id = table.Column<long>(type: "bigint", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.id);
                    table.CheckConstraint("CK_chats_chat_type", "chat_type IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_chats_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chat_members",
                columns: table => new
                {
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    mute_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_read_message_id = table.Column<long>(type: "bigint", nullable: true),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_members", x => new { x.chat_id, x.user_id });
                    table.CheckConstraint("CK_chat_members_role", "role IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_chat_members_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_user_id = table.Column<int>(type: "integer", nullable: false),
                    message_type = table.Column<short>(type: "smallint", nullable: false),
                    reply_to_message_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    edited_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.CheckConstraint("CK_messages_message_type", "message_type IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_messages_reply_to_message_id",
                        column: x => x.reply_to_message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_messages_users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "message_content",
                columns: table => new
                {
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    entities = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    emoji_payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_content", x => x.message_id);
                    table.CheckConstraint("CK_message_content_not_empty", "COALESCE(LENGTH(TRIM(text)), 0) > 0 OR JSONB_ARRAY_LENGTH(emoji_payload) > 0");
                    table.ForeignKey(
                        name: "FK_message_content_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_reads",
                columns: table => new
                {
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_reads", x => new { x.message_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_message_reads_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_message_reads_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_chat_id_user_id",
                table: "chat_members",
                columns: new[] { "chat_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_user_id_active",
                table: "chat_members",
                column: "user_id",
                filter: "left_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_user_id_chat_id",
                table: "chat_members",
                columns: new[] { "user_id", "chat_id" });

            migrationBuilder.CreateIndex(
                name: "IX_chats_chat_type",
                table: "chats",
                column: "chat_type");

            migrationBuilder.CreateIndex(
                name: "IX_chats_created_by",
                table: "chats",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_chats_updated_at",
                table: "chats",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_message_reads_message_id",
                table: "message_reads",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_reads_user_id_read_at",
                table: "message_reads",
                columns: new[] { "user_id", "read_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id_created_at_id",
                table: "messages",
                columns: new[] { "chat_id", "created_at", "id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id_created_at_not_deleted",
                table: "messages",
                columns: new[] { "chat_id", "created_at" },
                descending: new[] { false, true },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_messages_reply_to_message_id",
                table: "messages",
                column: "reply_to_message_id",
                filter: "reply_to_message_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_user_id",
                table: "messages",
                column: "sender_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_members");

            migrationBuilder.DropTable(
                name: "message_content");

            migrationBuilder.DropTable(
                name: "message_reads");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "chats");
        }
    }
}
