using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAID.V2.Infrastructure.Migrations
{
    /// <summary>
    /// Adds Cloudinary image-upload metadata fields to the gallery table.
    ///
    /// Why this is hand-written:
    /// EF scaffolding detects a large snapshot drift (model + many rename operations)
    /// because some migrations were applied manually outside EF, so __EFMigrationsHistory
    /// is missing. The previous EF-generated migration was 4000+ lines of unrelated
    /// DROP FOREIGN KEY + RENAME COLUMN statements.
    ///
    /// This migration is targeted at the gallery table only and uses raw SQL to be
    /// predictable and safe to audit:
    ///   - 4 nullable ADD COLUMN (non-destructive)
    ///   - 2 ALTER COLUMN expand length nvarchar(255) -> nvarchar(500) (non-destructive)
    ///
    /// DB naming convention was verified via INFORMATION_SCHEMA on the actual DB:
    ///   Table: gallery; Columns: photo_url, thumbnail_url, content_type,
    ///          file_size_bytes, original_file_name, public_id (all snake_case)
    /// See section 11 of docs/MIGRATION_GUIDE.md for the verification query.
    /// </summary>
    public partial class AddImageUploadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Add 4 Cloudinary metadata columns (all nullable — safe on existing rows)
                IF COL_LENGTH('gallery', 'public_id') IS NULL
                    ALTER TABLE gallery ADD [public_id] nvarchar(255) NULL;
                IF COL_LENGTH('gallery', 'original_file_name') IS NULL
                    ALTER TABLE gallery ADD [original_file_name] nvarchar(255) NULL;
                IF COL_LENGTH('gallery', 'file_size_bytes') IS NULL
                    ALTER TABLE gallery ADD [file_size_bytes] bigint NULL;
                IF COL_LENGTH('gallery', 'content_type') IS NULL
                    ALTER TABLE gallery ADD [content_type] nvarchar(50) NULL;

                -- Expand existing URL columns to accommodate Cloudinary's longer URLs
                -- (with version + folder + hash these routinely exceed 255 chars)
                ALTER TABLE gallery ALTER COLUMN [photo_url] nvarchar(500) NOT NULL;
                ALTER TABLE gallery ALTER COLUMN [thumbnail_url] nvarchar(500) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Reverse: shrink URL columns back to 255 (truncates any data > 255 chars)
                ALTER TABLE gallery ALTER COLUMN [photo_url] nvarchar(255) NOT NULL;
                ALTER TABLE gallery ALTER COLUMN [thumbnail_url] nvarchar(255) NULL;

                -- Drop the metadata columns (only safe if no row has public_id set yet)
                IF COL_LENGTH('gallery', 'content_type') IS NOT NULL
                    ALTER TABLE gallery DROP COLUMN [content_type];
                IF COL_LENGTH('gallery', 'file_size_bytes') IS NOT NULL
                    ALTER TABLE gallery DROP COLUMN [file_size_bytes];
                IF COL_LENGTH('gallery', 'original_file_name') IS NOT NULL
                    ALTER TABLE gallery DROP COLUMN [original_file_name];
                IF COL_LENGTH('gallery', 'public_id') IS NOT NULL
                    ALTER TABLE gallery DROP COLUMN [public_id];
            ");
        }
    }
}
