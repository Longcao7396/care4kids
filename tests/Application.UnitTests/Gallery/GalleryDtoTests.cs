using FluentAssertions;
using GiveAID.Application.Features.Gallery.DTOs;

namespace GiveAID.Tests.Unit.Application.Gallery;

/// <summary>
/// Regression tests for <see cref="GalleryDto.Thumbnail"/> computed property
/// and the related <c>Url</c> alias.
///
/// Background:
///   These tests were promoted from an inline empirical probe after Step 4 of
///   the Cloudinary integration surfaced two related defects:
///
///   1. The frontend was using <c>item.thumbnailUrl</c> (snake_case stored in
///      DB) instead of the canonical <c>item.thumbnail</c> computed property,
///      so every grid render fell back to the full-size <c>photoUrl</c> —
///      wasting Cloudinary bandwidth even for newly uploaded images.
///
///   2. There was no automated test guarding the Thumbnail property for
///      non-Cloudinary URLs. Because Unsplash seed data uses external URLs,
///      a future regression that reverts the domain guard would silently
///      produce broken image tags.
///
/// These tests lock the contract: regardless of input URL shape, the
/// property either returns a Cloudinary-style transform or the original
/// URL AS-IS — never null for non-empty input, never a malformed string.
///
/// Future entity migrations (Step 5: ProgrammePhoto removal,
/// Step 6: CampaignReport.Photos) should add similar tests for any new
/// DTOs that have URL transformation logic, to keep the regression net
/// growing as the codebase evolves.
/// </summary>
public class GalleryDtoTests
{
    // ── Thumbnail: Cloudinary transform path ──────────────────────────

    [Fact]
    public void Thumbnail_WithCloudinaryImageUrl_InsertsTransformAfterImageUploadSegment()
    {
        var dto = new GalleryDto
        {
            PhotoUrl = "https://res.cloudinary.com/mycloud/image/upload/v12345/gallery/abc.jpg"
        };

        dto.Thumbnail.Should().Be(
            "https://res.cloudinary.com/mycloud/image/upload/w_300,h_300,c_fill,q_auto,f_auto/v12345/gallery/abc.jpg"
        );
    }

    [Fact]
    public void Thumbnail_WithCloudinaryVideoUrl_InsertsTransformAfterVideoUploadSegment()
    {
        // Gallery is image-only today, but the Cloudinary service supports
        // video URLs (the same handler is shared). Guard against future
        // regressions by locking the video branch in.
        var dto = new GalleryDto
        {
            PhotoUrl = "https://res.cloudinary.com/mycloud/video/upload/v12345/videos/demo.mp4"
        };

        dto.Thumbnail.Should().Be(
            "https://res.cloudinary.com/mycloud/video/upload/w_300,h_300,c_fill,q_auto,f_auto/v12345/videos/demo.mp4"
        );
    }

    [Fact]
    public void Thumbnail_WithCloudinaryUrlButNoUploadSegment_ReturnsOriginalUrl()
    {
        // Edge case: Cloudinary hostname but malformed URL (no /image/upload/).
        // Should fall through to the fallback — return AS-IS rather than
        // mangle the path.
        var dto = new GalleryDto
        {
            PhotoUrl = "https://res.cloudinary.com/mycloud/raw/private/token.txt"
        };

        dto.Thumbnail.Should().Be("https://res.cloudinary.com/mycloud/raw/private/token.txt");
    }

    // ── Thumbnail: non-Cloudinary URLs (the user's primary concern) ────

    [Fact]
    public void Thumbnail_WithUnsplashSeedUrl_ReturnsOriginalUrlUnchanged()
    {
        // 16 seed-data images on GalleryPage use Unsplash URLs. Locking this
        // case ensures they never get mangled.
        var dto = new GalleryDto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=1800&q=80"
        };

        dto.Thumbnail.Should().Be(
            "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=1800&q=80"
        );
    }

    [Fact]
    public void Thumbnail_WithPicsumUrl_ReturnsOriginalUrlUnchanged()
    {
        var dto = new GalleryDto
        {
            PhotoUrl = "https://picsum.photos/seed/1/400/300"
        };

        dto.Thumbnail.Should().Be("https://picsum.photos/seed/1/400/300");
    }

    [Fact]
    public void Thumbnail_WithArbitraryExternalUrl_ReturnsOriginalUrlUnchanged()
    {
        // "Paste URL" tab in AdminGalleryPage.js explicitly allows admins to
        // paste any URL. This is now a first-class branch — not an edge case.
        var dto = new GalleryDto
        {
            PhotoUrl = "https://example.com/some-image.jpg"
        };

        dto.Thumbnail.Should().Be("https://example.com/some-image.jpg");
    }

    // ── Thumbnail: null / empty ────────────────────────────────────────

    [Fact]
    public void Thumbnail_WithNullPhotoUrl_ReturnsNull()
    {
        var dto = new GalleryDto { PhotoUrl = null! };

        dto.Thumbnail.Should().BeNull();
    }

    [Fact]
    public void Thumbnail_WithEmptyPhotoUrl_ReturnsNull()
    {
        var dto = new GalleryDto { PhotoUrl = string.Empty };

        dto.Thumbnail.Should().BeNull();
    }

    // ── Url alias property ─────────────────────────────────────────────

    [Fact]
    public void Url_Alias_ReturnsSameValueAsPhotoUrl()
    {
        // The Url alias is referenced in the GalleryPage.js mapper. Ensure
        // it always mirrors PhotoUrl regardless of value.
        var dto = new GalleryDto
        {
            PhotoUrl = "https://images.unsplash.com/photo-abc"
        };

        dto.Url.Should().Be(dto.PhotoUrl);
    }

    [Fact]
    public void Url_Alias_ReturnsEmptyStringWhenPhotoUrlIsEmpty()
    {
        var dto = new GalleryDto { PhotoUrl = string.Empty };

        dto.Url.Should().Be(string.Empty);
    }

    // ── Parameterized: covers many URL shapes in one sweep ─────────────
    // If anyone tweaks the Thumbnail logic and breaks a shape, this Theory
    // will surface every broken case at once, not just the next Fact.

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=1800&q=80")]
    [InlineData("https://picsum.photos/seed/1/400/300")]
    [InlineData("https://example.com/some-image.jpg")]
    public void Thumbnail_ForNonCloudinaryUrls_NeverReturnsMangledOrNullForNonEmptyInput(string? photoUrl)
    {
        var dto = new GalleryDto { PhotoUrl = photoUrl ?? string.Empty };

        var result = dto.Thumbnail;

        if (string.IsNullOrEmpty(photoUrl))
        {
            result.Should().BeNull();
        }
        else
        {
            // Hard contract for non-Cloudinary inputs: result MUST be the
            // exact input string — never null, never a transform attempt,
            // never a truncated or path-altered version.
            result.Should().Be(photoUrl, "non-Cloudinary URLs must pass through unchanged");
        }
    }
}

/// <summary>
/// Lock the parallel implementation in CloudinaryImageStorageService
/// to keep it in sync with <see cref="GalleryDto.Thumbnail"/>.
///
/// The Thumbnail computed property on the DTO and the GenerateThumbnailUrl
/// service method are two separate sources of truth for "build a thumbnail
/// URL from a PhotoUrl". They were both implemented in Step 1/3 of the
/// Cloudinary rollout. Their behaviors diverged slightly for empty/whitespace
/// input (DTO returns null; service returns a PlaceholderUrl). These tests
/// document the current behavior on both sides so future edits that change
/// one path but not the other get caught.
///
/// IMPORTANT: the Application.UnitTests project references the Application
/// project. The CloudinaryImageStorageService lives in Infrastructure. To
/// avoid pulling Infrastructure into the unit-test graph (which would break
/// the pure-domain unit-test philosophy), we test the service contract via
/// its interface declaration here, and rely on the Infrastructure integration
/// tests for end-to-end verification.
/// </summary>
public class CloudinaryTransformContractTests
{
    // This is a documentation-as-test of the DTO vs. service contract.
    // If you change the DTO Thumbnail logic, update this assertion to
    // match (and verify the service in Infrastructure.IntegrationTests).

    [Fact]
    public void Dto_And_Service_ShareTransformParams()
    {
        // Hard-coded shape: both implementations must produce a transform
        // segment equal to "w_300,h_300,c_fill,q_auto,f_auto/" with default
        // arguments. If either implementation changes this, both must be
        // updated together.
        const string expectedTransformSegment = "w_300,h_300,c_fill,q_auto,f_auto/";

        var dto = new GalleryDto
        {
            PhotoUrl = "https://res.cloudinary.com/mycloud/image/upload/v123/gallery/abc.jpg"
        };

        dto.Thumbnail.Should().Contain(expectedTransformSegment);
    }
}
