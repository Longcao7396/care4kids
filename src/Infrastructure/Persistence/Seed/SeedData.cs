using Microsoft.EntityFrameworkCore;
using GiveAID.Domain.Entities;
using GiveAID.Infrastructure.Security;

namespace GiveAID.Infrastructure.Persistence.Seed;

public static class SeedData
{
    /// <summary>
    /// Seeds the database with baseline data if tables are empty.
    /// </summary>
    /// <returns>True if seed completed (data was inserted), false if skipped
    /// because data already exists or an exception occurred.</returns>
    public static async Task<bool> SeedAsync(GiveAIDDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // If Users already exist, skip seeding entirely.
        // This makes the method idempotent — running it multiple times is safe.
        if (await context.Users.AnyAsync())
        {
            return false;
        }

        var passwordHasher = new PasswordHasher();

        // SECURITY: Seed passwords MUST come from environment variables.
        // We do NOT hardcode any default — even in Development — to prevent
        // real credentials from being committed to source control.
        // Required env vars:
        //   ADMIN_PASSWORD — min 8 chars
        //   DEMO_PASSWORD  — min 8 chars (optional; random if missing)
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException(
                "ADMIN_PASSWORD environment variable is required to seed the database. " +
                "Set it before running (e.g. `$env:ADMIN_PASSWORD='YourSecurePassword'`).");
        }
        if (adminPassword.Length < 8)
        {
            throw new InvalidOperationException("ADMIN_PASSWORD must be at least 8 characters long.");
        }

        var demoPassword = Environment.GetEnvironmentVariable("DEMO_PASSWORD")
            ?? Guid.NewGuid().ToString("N");  // random if not provided

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@give-aid.org",
            PasswordHash = passwordHasher.Hash(adminPassword),
            FullName = "System Administrator",
            Role = "Admin",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var demoUser = new User
        {
            Username = "demo",
            Email = "demo@give-aid.org",
            PasswordHash = passwordHasher.Hash(demoPassword),
            FullName = "Demo User",
            Role = "User",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, demoUser);
        await context.SaveChangesAsync();

        // Seed Causes if empty
        if (!await context.Causes.AnyAsync())
        {
            var causes = new List<Cause>
            {
                // Parent Causes
                new Cause { CauseCode = "EDU", CauseName = "Education for Children", Description = "Support education initiatives for underprivileged children", Icon = "graduation-cap", TargetAmount = 500000000, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "HEALTH", CauseName = "Healthcare Support", Description = "Provide healthcare access to those in need", Icon = "heartbeat", TargetAmount = 750000000, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "CHILD", CauseName = "Child Welfare", Description = "Protect and support vulnerable children", Icon = "child", TargetAmount = 400000000, IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "WOMEN", CauseName = "Women Empowerment", Description = "Empower women through education and opportunity", Icon = "female", TargetAmount = 350000000, IsActive = true, DisplayOrder = 4, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "ENV", CauseName = "Environment", Description = "Protect our environment for future generations", Icon = "leaf", TargetAmount = 300000000, IsActive = true, DisplayOrder = 5, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "EMERG", CauseName = "Emergency Relief", Description = "Respond to natural disasters and emergencies", Icon = "exclamation-triangle", TargetAmount = 1000000000, IsActive = true, DisplayOrder = 6, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "ELDER", CauseName = "Elderly Care", Description = "Support programs for senior citizens", Icon = "blind", TargetAmount = 250000000, IsActive = true, DisplayOrder = 7, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "DIS", CauseName = "Disability Support", Description = "Assist people with disabilities", Icon = "wheelchair", TargetAmount = 300000000, IsActive = true, DisplayOrder = 8, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "ANIMAL", CauseName = "Animal Welfare", Description = "Protect and care for animals in need", Icon = "paw", TargetAmount = 200000000, IsActive = true, DisplayOrder = 9, CreatedAt = DateTime.UtcNow }
            };

            context.Causes.AddRange(causes);
            await context.SaveChangesAsync();

            // Add sub-causes
            var educationCause = causes.First(c => c.CauseCode == "EDU");
            var healthCause = causes.First(c => c.CauseCode == "HEALTH");
            var childCause = causes.First(c => c.CauseCode == "CHILD");
            var womenCause = causes.First(c => c.CauseCode == "WOMEN");
            var emergencyCause = causes.First(c => c.CauseCode == "EMERG");

            var subCauses = new List<Cause>
            {
                // Education sub-causes
                new Cause { CauseCode = "EDU-BOOK", CauseName = "School Supplies", Description = "Provide books, uniforms, and stationery", ParentCauseId = educationCause.CauseId, TargetAmount = 50000000, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "EDU-SCHOLAR", CauseName = "Scholarships", Description = "Fund educational scholarships", ParentCauseId = educationCause.CauseId, TargetAmount = 100000000, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "EDU-SKILL", CauseName = "Vocational Training", Description = "Skill development programs", ParentCauseId = educationCause.CauseId, TargetAmount = 75000000, IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                // Healthcare sub-causes
                new Cause { CauseCode = "HEALTH-MED", CauseName = "Medical Treatment", Description = "Fund medical treatments and surgeries", ParentCauseId = healthCause.CauseId, TargetAmount = 200000000, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "HEALTH-VAC", CauseName = "Vaccination Programs", Description = "Support vaccination drives", ParentCauseId = healthCause.CauseId, TargetAmount = 100000000, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "HEALTH-MH", CauseName = "Mental Health", Description = "Mental health awareness and support", ParentCauseId = healthCause.CauseId, TargetAmount = 75000000, IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                // Child Welfare sub-causes
                new Cause { CauseCode = "CHILD-SHELTER", CauseName = "Child Shelters", Description = "Safe houses for children", ParentCauseId = childCause.CauseId, TargetAmount = 100000000, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "CHILD-NUTR", CauseName = "Nutrition", Description = "Fight child malnutrition", ParentCauseId = childCause.CauseId, TargetAmount = 75000000, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                // Women Empowerment sub-causes
                new Cause { CauseCode = "WOMEN-ENT", CauseName = "Entrepreneurship", Description = "Support women-owned businesses", ParentCauseId = womenCause.CauseId, TargetAmount = 100000000, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "WOMEN-SHELTER", CauseName = "Safe Housing", Description = "Shelters for women in need", ParentCauseId = womenCause.CauseId, TargetAmount = 75000000, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                // Emergency sub-causes
                new Cause { CauseCode = "EMERG-FLOOD", CauseName = "Flood Relief", Description = "Aid for flood victims", ParentCauseId = emergencyCause.CauseId, TargetAmount = 200000000, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new Cause { CauseCode = "EMERG-QUAKE", CauseName = "Earthquake Relief", Description = "Support earthquake survivors", ParentCauseId = emergencyCause.CauseId, TargetAmount = 250000000, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow }
            };

            context.Causes.AddRange(subCauses);
            await context.SaveChangesAsync();
        }

        // Seed CmsPages if empty
        if (!await context.CmsPages.AnyAsync())
        {
            var cmsPages = new List<CmsPage>
            {
                new CmsPage
                {
                    PageKey = "home",
                    PageSlug = "/",
                    PageTitle = "Home",
                    Content = "<h1>Welcome to GiveAID</h1><p>Making a difference together.</p>",
                    IsActive = true,
                    IsInMenu = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new CmsPage
                {
                    PageKey = "about_us",
                    PageSlug = "about-us",
                    PageTitle = "About Us",
                    Content = "<h1>About GiveAID</h1><p>GiveAID is a platform connecting donors with meaningful causes.</p>",
                    MetaDescription = "Learn about GiveAID's mission and vision",
                    IsActive = true,
                    IsInMenu = true,
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new CmsPage
                {
                    PageKey = "contact",
                    PageSlug = "contact",
                    PageTitle = "Contact Us",
                    Content = "<h1>Contact Us</h1><p>Get in touch with our team.</p>",
                    MetaDescription = "Contact GiveAID team",
                    IsActive = true,
                    IsInMenu = true,
                    DisplayOrder = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new CmsPage
                {
                    PageKey = "privacy_policy",
                    PageSlug = "privacy-policy",
                    PageTitle = "Privacy Policy",
                    Content = "<h1>Privacy Policy</h1><p>Your privacy is important to us.</p>",
                    IsActive = true,
                    IsInMenu = false,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow
                },
                new CmsPage
                {
                    PageKey = "terms_of_service",
                    PageSlug = "terms-of-service",
                    PageTitle = "Terms of Service",
                    Content = "<h1>Terms of Service</h1><p>Please read our terms carefully.</p>",
                    IsActive = true,
                    IsInMenu = false,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.CmsPages.AddRange(cmsPages);
            await context.SaveChangesAsync();
        }

        // Seed Faqs if empty
        if (!await context.Faqs.AnyAsync())
        {
            var faqs = new List<Faq>
            {
                new Faq { Question = "How do I make a donation?", Answer = "Simply browse our causes, select one you care about, and click 'Donate'. You can use credit card, debit card, or bank transfer.", Category = "Donations", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Faq { Question = "Is my donation tax-deductible?", Answer = "Yes, donations to registered charitable organizations may be tax-deductible. Please consult your tax advisor for specific advice.", Category = "Donations", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Faq { Question = "How is my donation used?", Answer = "100% of your donation goes directly to the cause you choose. We maintain transparent reporting on all campaigns.", Category = "Donations", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Faq { Question = "How do I volunteer?", Answer = "Register on our platform, browse volunteer opportunities, and sign up for programs that match your interests.", Category = "Volunteering", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Faq { Question = "Can I cancel my recurring donation?", Answer = "Yes, you can cancel your recurring donation anytime from your account settings.", Category = "Donations", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Faq { Question = "How do I receive a donation receipt?", Answer = "Donation receipts are automatically sent to your registered email address after each donation.", Category = "Donations", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            context.Faqs.AddRange(faqs);
            await context.SaveChangesAsync();
        }

        // Seed Organizations (partners & supporters) if empty.
        // Public /our-partners page reads from this table; without these
        // rows the page renders the empty-state "No partners found".
        if (!await context.Organizations.AnyAsync())
        {
            var organizations = new List<Organization>
            {
                new Organization
                {
                    OrganizationName = "Vietnam Red Cross Society",
                    OrganizationType = "Government",
                    Description = "National humanitarian organization providing emergency relief, healthcare, and disaster response across Vietnam.",
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3c/Red_Cross_logo.svg/240px-Red_Cross_logo.svg.png",
                    WebsiteUrl = "https://redcross.org.vn",
                    ContactEmail = "info@redcross.org.vn",
                    ContactPhone = "+84 24 3822 4030",
                    Address = "82 Nguyễn Du, Hai Bà Trưng, Hà Nội",
                    RegistrationNumber = "RC-001",
                    Mission = "To prevent and alleviate human suffering wherever it may be found.",
                    Vision = "A world where everyone acts with humanity.",
                    ContributionAmount = 2_500_000_000m,
                    ContributionType = "Cash + In-kind",
                    IsActive = true,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "UNICEF Vietnam",
                    OrganizationType = "NGO",
                    Description = "United Nations Children's Fund — protecting children's rights, providing healthcare and education to vulnerable children in Vietnam.",
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/36/UNICEF_Logo.png/240px-UNICEF_Logo.png",
                    WebsiteUrl = "https://www.unicef.org/vietnam",
                    ContactEmail = "hanoi@unicef.org",
                    ContactPhone = "+84 24 3857 3666",
                    Address = "81A Trần Hưng Đạo, Hoàn Kiếm, Hà Nội",
                    Mission = "To advocate for the protection of children's rights, to help meet their basic needs and to expand their opportunities to reach their full potential.",
                    ContributionAmount = 5_000_000_000m,
                    ContributionType = "Cash",
                    IsActive = true,
                    IsFeatured = true,
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "Saigon Children's Charity",
                    OrganizationType = "NGO",
                    Description = "Helping disadvantaged children in southern Vietnam access education, healthcare and social welfare programmes since 1992.",
                    LogoUrl = null,
                    WebsiteUrl = "https://www.saigonchildren.com",
                    ContactEmail = "info@saigonchildren.com",
                    ContactPhone = "+84 28 3827 7305",
                    Address = "Level 3, 38B Phan Đình Phùng, Quận Phú Nhuận, TP.HCM",
                    RegistrationNumber = "SCC-VN-001",
                    Mission = "To enable disadvantaged children to escape poverty through education and healthcare.",
                    ContributionAmount = 850_000_000m,
                    ContributionType = "Cash + In-kind",
                    IsActive = true,
                    IsFeatured = true,
                    DisplayOrder = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "Vingroup Foundation",
                    OrganizationType = "Corporate",
                    Description = "Corporate social responsibility arm of Vingroup — funding scholarships, infrastructure and healthcare programmes nationwide.",
                    LogoUrl = null,
                    WebsiteUrl = "https://vingroup.net/foundation",
                    ContactEmail = "foundation@vingroup.net",
                    ContactPhone = "+84 24 3974 9999",
                    Address = "Số 7, Đại lộ Bằng Lăng 1, Vinhomes Riverside, Long Biên, Hà Nội",
                    RegistrationNumber = "VG-FDN-2017",
                    Mission = "For a better life for Vietnamese people through education, healthcare and sustainable development.",
                    ContributionAmount = 8_700_000_000m,
                    ContributionType = "Cash",
                    IsActive = true,
                    IsFeatured = true,
                    DisplayOrder = 4,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "KOTO Foundation",
                    OrganizationType = "NGO",
                    Description = "Know One Teach One — providing hospitality training and education to at-risk youth across Vietnam.",
                    LogoUrl = null,
                    WebsiteUrl = "https://koto.com.au",
                    ContactEmail = "info@koto.com.au",
                    ContactPhone = "+84 24 3718 4844",
                    Address = "270 Nguyễn Tri Phương, Đống Đa, Hà Nội",
                    Mission = "Empowering at-risk youth through vocational training in hospitality.",
                    ContributionAmount = 420_000_000m,
                    ContributionType = "Cash + In-kind",
                    IsActive = true,
                    IsFeatured = true,
                    DisplayOrder = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "Hội Bảo Trợ Trẻ Em Việt Nam",
                    OrganizationType = "NGO",
                    Description = "Vietnam Children's Protection Association — advocating for child rights and welfare policies.",
                    LogoUrl = null,
                    WebsiteUrl = null,
                    ContactEmail = "contact@hbtt.org.vn",
                    ContactPhone = "+84 24 3934 5678",
                    Address = "35 Hai Bà Trưng, Hoàn Kiếm, Hà Nội",
                    RegistrationNumber = "VPA-2015",
                    Mission = "Bảo vệ và thúc đẩy quyền trẻ em Việt Nam.",
                    ContributionAmount = 650_000_000m,
                    ContributionType = "In-kind",
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 6,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "FPT Software Cares",
                    OrganizationType = "Corporate",
                    Description = "Tech-for-good initiative by FPT — building digital literacy programmes for rural schools.",
                    LogoUrl = null,
                    WebsiteUrl = "https://fptsoftware.com/csr",
                    ContactEmail = "cares@fptsoftware.com",
                    ContactPhone = "+84 24 7300 8866",
                    Address = "FPT Tower, 10 Phố Tố Hữu, Nam Từ Liêm, Hà Nội",
                    ContributionAmount = 1_200_000_000m,
                    ContributionType = "Cash + Technology",
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 7,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "WHO Vietnam",
                    OrganizationType = "Government",
                    Description = "World Health Organization country office — supporting public health programmes in Vietnam.",
                    LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c2/WHO_logo.svg/240px-WHO_logo.svg.png",
                    WebsiteUrl = "https://www.who.int/vietnam",
                    ContactEmail = "who-vietnam@who.int",
                    ContactPhone = "+84 24 3857 3668",
                    Address = "63 Tran Hung Dao, Hoan Kiem, Hanoi",
                    Mission = "Health for all, everywhere.",
                    ContributionAmount = 3_400_000_000m,
                    ContributionType = "Technical assistance",
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 8,
                    CreatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    OrganizationName = "Đoàn Thanh Niên Cộng Sản Hồ Chí Minh",
                    OrganizationType = "Government",
                    Description = "Ho Chi Minh Communist Youth Union — mobilising youth volunteers for community programmes nationwide.",
                    LogoUrl = null,
                    WebsiteUrl = null,
                    ContactEmail = "doanthanhnien@tphcm.gov.vn",
                    ContactPhone = "+84 28 3822 1234",
                    Address = "1 Đồng Khởi, Quận 1, TP.HCM",
                    ContributionAmount = 280_000_000m,
                    ContributionType = "Volunteer hours",
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 9,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Organizations.AddRange(organizations);
            await context.SaveChangesAsync();

            // Now seed Campaigns referencing real Cause IDs.
            var educationCause = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Causes, c => c.CauseCode == "EDU");
            var healthCause = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Causes, c => c.CauseCode == "HEALTH");
            var childCause = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Causes, c => c.CauseCode == "CHILD");
            var emergCause = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Causes, c => c.CauseCode == "EMERG");

            if (educationCause != null && healthCause != null && childCause != null && emergCause != null)
            {
                var redCross = organizations.First(o => o.OrganizationName.Contains("Red Cross"));
                var unicef = organizations.First(o => o.OrganizationName.Contains("UNICEF"));

                var today = DateTime.UtcNow;
                var campaigns = new List<Campaign>
                {
                    new Campaign
                    {
                        CauseId = childCause.CauseId,
                        OrganizationId = redCross.OrganizationId,
                        CampaignName = "Bữa Cơm Có Thịt — 5,000 nutritious meals for Saigon children",
                        CampaignCode = "CMP-001",
                        ProgrammeType = "ChildWelfare",
                        RegistrationRequired = false,
                        Description = "Daily nutritious meals for 850 children at orphanages and care centres in Ho Chi Minh City. Each meal includes rice, meat, vegetables and milk.",
                        GoalAmount = 120_000_000m,
                        RaisedAmount = 87_450_000m,
                        StartDate = today.AddDays(-30),
                        EndDate = today.AddDays(60),
                        ImageUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 850,
                        Location = "TP. Hồ Chí Minh",
                        Status = "Active",
                        IsFeatured = true,
                        DisplayOrder = 1,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = educationCause.CauseId,
                        OrganizationId = unicef.OrganizationId,
                        CampaignName = "Sách Vở Cho Em Đến Trường — 1,500 back-to-school kits",
                        CampaignCode = "CMP-002",
                        ProgrammeType = "Education",
                        RegistrationRequired = false,
                        Description = "School bags, books, notebooks and stationery for 1,500 children in remote northern provinces. Each kit includes a year of student insurance.",
                        GoalAmount = 900_000_000m,
                        RaisedAmount = 612_000_000m,
                        StartDate = today.AddDays(-14),
                        EndDate = today.AddDays(120),
                        ImageUrl = "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 1500,
                        Location = "Lào Cai, Sơn La",
                        Status = "Active",
                        IsFeatured = false,
                        DisplayOrder = 2,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = healthCause.CauseId,
                        CampaignName = "Khám Sức Khỏe Miễn Phí — 30 mobile clinics",
                        CampaignCode = "CMP-003",
                        ProgrammeType = "HealthCare",
                        RegistrationRequired = false,
                        Description = "30 mobile health clinics serving 3,000 children in remote districts of Nghệ An, Quảng Nam and Bến Tre. Each visit includes check-up, free medicine and nutrition counselling.",
                        GoalAmount = 450_000_000m,
                        RaisedAmount = 387_500_000m,
                        StartDate = today.AddDays(-60),
                        EndDate = today.AddDays(30),
                        ImageUrl = "https://images.unsplash.com/photo-1576091160550-2173dba999ef?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 3000,
                        Location = "Nghệ An, Quảng Nam, Bến Tre",
                        Status = "Active",
                        IsFeatured = true,
                        DisplayOrder = 3,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = childCause.CauseId,
                        CampaignName = "Mái Ấm Tình Thương — Three new care homes",
                        CampaignCode = "CMP-004",
                        ProgrammeType = "ChildWelfare",
                        RegistrationRequired = true,
                        MaxParticipants = 50,
                        TargetBeneficiaries = 150,
                        Description = "Build and operate three new long-term care homes in Hanoi, Đà Nẵng and Cần Thơ, each housing 50 orphaned children.",
                        GoalAmount = 2_500_000_000m,
                        RaisedAmount = 1_750_000_000m,
                        StartDate = today.AddDays(-90),
                        EndDate = today.AddDays(180),
                        ImageUrl = "https://images.unsplash.com/photo-1542810634-71277d95dcbb?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 150,
                        Location = "Hà Nội, Đà Nẵng, Cần Thơ",
                        Status = "Active",
                        IsFeatured = false,
                        DisplayOrder = 4,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = emergCause.CauseId,
                        CampaignName = "Cứu Trợ Lũ Lụt Miền Trung — Flood Relief 2026",
                        CampaignCode = "CMP-005",
                        ProgrammeType = "EmergencyRelief",
                        RegistrationRequired = true,
                        MaxParticipants = 200,
                        TargetBeneficiaries = 10000,
                        Description = "Emergency aid for 10,000 households affected by central-Vietnam floods: food, clean water, medicine and cash grants.",
                        GoalAmount = 3_000_000_000m,
                        RaisedAmount = 2_380_000_000m,
                        StartDate = today.AddDays(-7),
                        EndDate = today.AddDays(45),
                        ImageUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 10000,
                        Location = "Quảng Bình, Hà Tĩnh",
                        Status = "Active",
                        IsFeatured = true,
                        DisplayOrder = 5,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = educationCause.CauseId,
                        CampaignName = "Lớp Học Hy Vọng — Free English Classes",
                        CampaignCode = "CMP-006",
                        ProgrammeType = "Education",
                        RegistrationRequired = false,
                        Description = "20 free weekend English classes for 600 children near industrial zones, taught by international and Vietnamese volunteers using Cambridge curriculum.",
                        GoalAmount = 600_000_000m,
                        RaisedAmount = 312_000_000m,
                        StartDate = today.AddDays(-45),
                        EndDate = today.AddDays(150),
                        ImageUrl = "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 600,
                        Location = "Bình Dương, Long An",
                        Status = "Active",
                        IsFeatured = false,
                        DisplayOrder = 6,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = healthCause.CauseId,
                        CampaignName = "Mổ Tim Miễn Phí Cho Trẻ Em — 50 heart surgeries",
                        CampaignCode = "CMP-007",
                        ProgrammeType = "HealthCare",
                        RegistrationRequired = false,
                        Description = "Fund 50 free pediatric heart surgeries for underprivileged children at Bệnh viện E and Viện Tim Mạch Quốc Gia. Each surgery costs 80-150M VND.",
                        GoalAmount = 5_000_000_000m,
                        RaisedAmount = 4_150_000_000m,
                        StartDate = today.AddDays(-180),
                        EndDate = today.AddDays(60),
                        ImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 50,
                        Location = "Hà Nội",
                        Status = "Active",
                        IsFeatured = false,
                        DisplayOrder = 7,
                        CreatedBy = 1,
                        CreatedAt = today
                    },
                    new Campaign
                    {
                        CauseId = childCause.CauseId,
                        CampaignName = "Sân Chơi Cho Trẻ Em Nông Thôn — 25 rural playgrounds",
                        CampaignCode = "CMP-008",
                        ProgrammeType = "ChildWelfare",
                        RegistrationRequired = true,
                        MaxParticipants = 30,
                        TargetBeneficiaries = 5000,
                        Description = "Build 25 safe community playgrounds in remote communes, including slides, swings, seesaws, soccer area and outdoor reading space.",
                        GoalAmount = 850_000_000m,
                        RaisedAmount = 510_000_000m,
                        StartDate = today.AddDays(-30),
                        EndDate = today.AddDays(120),
                        ImageUrl = "https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?auto=format&fit=crop&w=900&q=80",
                        BeneficiariesCount = 5000,
                        Location = "Tuyên Quang, Bắc Kạn",
                        Status = "Active",
                        IsFeatured = false,
                        DisplayOrder = 8,
                        CreatedBy = 1,
                        CreatedAt = today
                    }
                };

                context.Campaigns.AddRange(campaigns);
                await context.SaveChangesAsync();
            }
        }

        // Seed Gallery items if empty. The Gallery page reads from this
        // table; without rows the public page shows the empty state.
        if (!await context.Gallery.AnyAsync())
        {
            var gallery = new List<Gallery>
            {
                new Gallery { Title = "Lớp học vùng cao", PhotoUrl = "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=1600&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=400&q=80", Category = "education", DisplayOrder = 1, IsFeatured = true, UploadedAt = DateTime.UtcNow.AddDays(-30) },
                new Gallery { Title = "Cứu trợ lũ lụt miền Trung", PhotoUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=1600&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=400&q=80", Category = "relief", DisplayOrder = 2, IsFeatured = true, UploadedAt = DateTime.UtcNow.AddDays(-28) },
                new Gallery { Title = "Nụ cười của em", PhotoUrl = "https://images.unsplash.com/photo-1542810634-71277d95dcbb?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1542810634-71277d95dcbb?auto=format&fit=crop&w=400&q=80", Category = "portraits", DisplayOrder = 3, IsFeatured = true, UploadedAt = DateTime.UtcNow.AddDays(-25) },
                new Gallery { Title = "Khám sức khỏe định kỳ", PhotoUrl = "https://images.unsplash.com/photo-1515488042361-ee884cd86774?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1515488042361-ee884cd86774?auto=format&fit=crop&w=400&q=80", Category = "health", DisplayOrder = 4, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-20) },
                new Gallery { Title = "Em đọc sách ở thư viện làng", PhotoUrl = "https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?auto=format&fit=crop&w=400&q=80", Category = "education", DisplayOrder = 5, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-18) },
                new Gallery { Title = "Tiêm chủng mở rộng", PhotoUrl = "https://images.unsplash.com/photo-1509099836639-18ba1795216d?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1509099836639-18ba1795216d?auto=format&fit=crop&w=400&q=80", Category = "health", DisplayOrder = 6, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-15) },
                new Gallery { Title = "Bữa cơm có thịt", PhotoUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=400&q=80", Category = "health", DisplayOrder = 7, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-12) },
                new Gallery { Title = "Giếng nước sạch cho bản làng", PhotoUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?auto=format&fit=crop&w=400&q=80", Category = "water", DisplayOrder = 8, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-10) },
                new Gallery { Title = "Trồng rau cùng các em", PhotoUrl = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb6?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb6?auto=format&fit=crop&w=400&q=80", Category = "environment", DisplayOrder = 9, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-8) },
                new Gallery { Title = "Tình nguyện viên dạy tiếng Anh", PhotoUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=400&q=80", Category = "community", DisplayOrder = 10, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-6) },
                new Gallery { Title = "Mổ tim nhân đạo", PhotoUrl = "https://images.unsplash.com/photo-1576091160550-2173dba999ef?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1576091160550-2173dba999ef?auto=format&fit=crop&w=400&q=80", Category = "health", DisplayOrder = 11, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-5) },
                new Gallery { Title = "Dọn dẹp sau lũ", PhotoUrl = "https://images.unsplash.com/photo-1602052793312-b779c08a90d6?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1602052793312-b779c08a90d6?auto=format&fit=crop&w=400&q=80", Category = "relief", DisplayOrder = 12, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-3) },
                new Gallery { Title = "Chân dung em bé vùng cao", PhotoUrl = "https://images.unsplash.com/photo-1542884748-2b87b36c6b90?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1542884748-2b87b36c6b90?auto=format&fit=crop&w=400&q=80", Category = "portraits", DisplayOrder = 13, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddDays(-2) },
                new Gallery { Title = "Charity Run 2026", PhotoUrl = "https://images.unsplash.com/photo-1559027612-cfa6a79a7c91?auto=format&fit=crop&w=1600&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1559027612-cfa6a79a7c91?auto=format&fit=crop&w=400&q=80", Category = "events", DisplayOrder = 14, IsFeatured = true, UploadedAt = DateTime.UtcNow.AddDays(-1) },
                new Gallery { Title = "Xây nhà tình thương", PhotoUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=400&q=80", Category = "community", DisplayOrder = 15, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddHours(-12) },
                new Gallery { Title = "Phát thuốc miễn phí", PhotoUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=800&q=80", ThumbnailUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=400&q=80", Category = "community", DisplayOrder = 16, IsFeatured = false, UploadedAt = DateTime.UtcNow.AddHours(-6) }
            };

            context.Gallery.AddRange(gallery);
            await context.SaveChangesAsync();
        }

        // Seed Achievements if empty (MAJ-001)
        if (!await context.Achievements.AnyAsync())
        {
            var achievements = new List<Achievement>
            {
                new Achievement
                {
                    Title = "50,000 Children Fed in 2024",
                    Category = "Nutrition",
                    Description = "A landmark milestone — served 50,000 nutritious meals to children across 12 provinces in Vietnam through our Bữa Cơm Có Thịt programme.",
                    MetricValue = 50000,
                    MetricLabel = "Children",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2024, 12, 31),
                    ImageUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=900&q=80",
                    Icon = "utensils",
                    AwardBy = "Ministry of Labour",
                    Location = "Vietnam",
                    Beneficiaries = 50000,
                    DisplayOrder = 1,
                    IsActive = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "1,200 Heart Surgeries Funded",
                    Category = "Healthcare",
                    Description = "Funded over 1,200 life-saving pediatric heart surgeries since 2018, partnering with National Cardiac Hospital.",
                    MetricValue = 1200,
                    MetricLabel = "Surgeries",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2025, 6, 15),
                    ImageUrl = "https://images.unsplash.com/photo-1576091160550-2173dba999ef?auto=format&fit=crop&w=900&q=80",
                    Icon = "heartbeat",
                    AwardBy = "National Cardiac Hospital",
                    Location = "Hanoi, Vietnam",
                    Beneficiaries = 1200,
                    DisplayOrder = 2,
                    IsActive = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "100 Schools Received Library Grants",
                    Category = "Education",
                    Description = "Provided library grants to 100 rural schools across 15 provinces, supplying books, shelving and reading programmes.",
                    MetricValue = 100,
                    MetricLabel = "Schools",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2025, 3, 1),
                    ImageUrl = "https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=900&q=80",
                    Icon = "book",
                    AwardBy = "Ministry of Education",
                    Location = "Northern Vietnam",
                    Beneficiaries = 150000,
                    DisplayOrder = 3,
                    IsActive = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "VND 25 Billion Raised for Flood Relief 2024",
                    Category = "Emergency",
                    Description = "Emergency fundraising campaign for central Vietnam floods — reached VND 25 billion in 30 days from 18,000 donors.",
                    MetricValue = 25_000_000_000,
                    MetricLabel = "VND Raised",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2024, 11, 15),
                    ImageUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=900&q=80",
                    Icon = "water",
                    AwardBy = "Vietnam Red Cross",
                    Location = "Central Vietnam",
                    Beneficiaries = 85000,
                    DisplayOrder = 4,
                    IsActive = true,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "UNICEF Partner of the Year 2024",
                    Category = "Partnership",
                    Description = "Awarded UNICEF Partner of the Year for outstanding contribution to child welfare programmes in Southeast Asia.",
                    MetricValue = 1,
                    MetricLabel = "Award",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2024, 10, 1),
                    ImageUrl = "https://images.unsplash.com/photo-1542810634-71277d95dcbb?auto=format&fit=crop&w=900&q=80",
                    Icon = "award",
                    AwardBy = "UNICEF SEAP",
                    Location = "Bangkok, Thailand",
                    Beneficiaries = 0,
                    DisplayOrder = 5,
                    IsActive = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "10,000 Volunteer Hours Logged",
                    Category = "Community",
                    Description = "Community volunteers contributed over 10,000 hours to teaching, outreach and disaster response activities in 2024.",
                    MetricValue = 10000,
                    MetricLabel = "Volunteer Hours",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2024, 12, 31),
                    ImageUrl = "https://images.unsplash.com/photo-1559027612-cfa6a79a7c91?auto=format&fit=crop&w=900&q=80",
                    Icon = "users",
                    AwardBy = "Internal Recognition",
                    Location = "Nationwide",
                    Beneficiaries = 0,
                    DisplayOrder = 6,
                    IsActive = true,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "3 New Care Homes Opened",
                    Category = "Shelter",
                    Description = "Opened three new long-term residential care homes in Hanoi, Da Nang and Can Tho, providing safe homes for 150 orphaned children.",
                    MetricValue = 3,
                    MetricLabel = "Care Homes",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2025, 1, 15),
                    ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=900&q=80",
                    Icon = "home",
                    AwardBy = "Ministry of Labour",
                    Location = "Hanoi, Da Nang, Can Tho",
                    Beneficiaries = 150,
                    DisplayOrder = 7,
                    IsActive = true,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Achievement
                {
                    Title = "VND 50 Billion in Total Donations Received",
                    Category = "Fundraising",
                    Description = "Cumulative total donations received since platform launch exceeded VND 50 billion — a testament to donor trust.",
                    MetricValue = 50_000_000_000,
                    MetricLabel = "VND Total Raised",
                    MetricSuffix = "",
                    AchievementDate = new DateTime(2025, 4, 1),
                    ImageUrl = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb6?auto=format&fit=crop&w=900&q=80",
                    Icon = "coins",
                    AwardBy = "Internal Milestone",
                    Location = "Platform-wide",
                    Beneficiaries = 0,
                    DisplayOrder = 8,
                    IsActive = true,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Achievements.AddRange(achievements);
            await context.SaveChangesAsync();
        }

        // Seed TeamMembers if empty (MAJ-002)
        if (!await context.TeamMembers.AnyAsync())
        {
            var teamMembers = new List<TeamMember>
            {
                new TeamMember
                {
                    FullName = "Nguyen Van Minh",
                    RoleTitle = "Executive Director",
                    Department = "Leadership",
                    Bio = "Minh has led GiveAID since 2015, growing it from a small local charity to Vietnam's most trusted child welfare platform. With 20 years in NGO management, he oversees all programmes and partnerships.",
                    PhotoUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=400&q=80",
                    Email = "minh.nv@give-aid.org",
                    LinkedInUrl = "https://linkedin.com/in/minh-nguyen",
                    IsActive = true,
                    IsFeatured = true,
                    JoinedDate = new DateTime(2015, 1, 1),
                    DisplayOrder = 1,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "Tran Thi Lan",
                    RoleTitle = "Programmes Director",
                    Department = "Programmes",
                    Bio = "Lan oversees all field programmes including education, healthcare and emergency response. She holds an MBA from Fulbright University Vietnam and previously worked with UNICEF Vietnam.",
                    PhotoUrl = "https://images.unsplash.com/photo-1494790108755-2616b612b193?auto=format&fit=crop&w=400&q=80",
                    Email = "lan.tt@give-aid.org",
                    LinkedInUrl = "https://linkedin.com/in/lan-tran",
                    IsActive = true,
                    IsFeatured = true,
                    JoinedDate = new DateTime(2017, 6, 1),
                    DisplayOrder = 2,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "John Anderson",
                    RoleTitle = "Chief Technology Officer",
                    Department = "Technology",
                    Bio = "John built the GiveAID platform from the ground up, ensuring transparency in donation tracking and real-time impact reporting. He has 15 years of experience in fintech and nonprofit tech.",
                    PhotoUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=400&q=80",
                    Email = "john.a@give-aid.org",
                    LinkedInUrl = "https://linkedin.com/in/johnanderson",
                    TwitterUrl = "https://twitter.com/johnanderson",
                    IsActive = true,
                    IsFeatured = true,
                    JoinedDate = new DateTime(2018, 3, 1),
                    DisplayOrder = 3,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "Pham Thi Mai",
                    RoleTitle = "Head of Fundraising",
                    Department = "Fundraising",
                    Bio = "Mai leads all fundraising initiatives including corporate partnerships, individual donor programmes and grant applications. She has raised over VND 80 billion for charitable causes.",
                    PhotoUrl = "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?auto=format&fit=crop&w=400&q=80",
                    Email = "mai.pt@give-aid.org",
                    LinkedInUrl = "https://linkedin.com/in/maipham",
                    IsActive = true,
                    IsFeatured = false,
                    JoinedDate = new DateTime(2019, 9, 1),
                    DisplayOrder = 4,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "Le Van Hai",
                    RoleTitle = "Field Operations Manager",
                    Department = "Operations",
                    Bio = "Hai coordinates all on-the-ground operations across 12 provinces, managing a team of 45 field staff and 200+ volunteers. He ensures programmes reach beneficiaries efficiently.",
                    PhotoUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=400&q=80",
                    Email = "hai.lv@give-aid.org",
                    IsActive = true,
                    IsFeatured = false,
                    JoinedDate = new DateTime(2016, 4, 1),
                    DisplayOrder = 5,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "Nguyen Thi Thu Ha",
                    RoleTitle = "Communications Manager",
                    Department = "Communications",
                    Bio = "Thu Ha manages all media, storytelling and digital communications. She has grown GiveAID's social following to 500,000+ and leads our donor engagement campaigns.",
                    PhotoUrl = "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=400&q=80",
                    Email = "hantt@give-aid.org",
                    LinkedInUrl = "https://linkedin.com/in/thuha-nguyen",
                    TwitterUrl = "https://twitter.com/thuhan",
                    IsActive = true,
                    IsFeatured = false,
                    JoinedDate = new DateTime(2020, 2, 1),
                    DisplayOrder = 6,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "David Smith",
                    RoleTitle = "Finance & Compliance Director",
                    Department = "Finance",
                    Bio = "David oversees all financial operations, donor fund management and regulatory compliance. He is a CPA with 12 years of experience in nonprofit finance.",
                    PhotoUrl = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=400&q=80",
                    Email = "david.s@give-aid.org",
                    LinkedInUrl = "https://linkedin.com/in/davidsmith",
                    IsActive = true,
                    IsFeatured = false,
                    JoinedDate = new DateTime(2019, 11, 1),
                    DisplayOrder = 7,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamMember
                {
                    FullName = "Vo Thi Kim Lien",
                    RoleTitle = "HR & Volunteer Coordinator",
                    Department = "Human Resources",
                    Bio = "Kim Lien manages staff recruitment, volunteer programmes and capacity building. She has onboarded over 500 volunteers and built GiveAID's strong culture of compassion.",
                    PhotoUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=400&q=80",
                    Email = "lien.vt@give-aid.org",
                    IsActive = true,
                    IsFeatured = false,
                    JoinedDate = new DateTime(2021, 5, 1),
                    DisplayOrder = 8,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.TeamMembers.AddRange(teamMembers);
            await context.SaveChangesAsync();
        }

        // Seed Careers if empty (MAJ-003)
        if (!await context.Careers.AnyAsync())
        {
            var careers = new List<Career>
            {
                new Career
                {
                    PositionTitle = "Senior Programme Officer",
                    Department = "Programmes",
                    Description = "Lead the design, implementation and monitoring of child welfare programmes across Vietnam. You will work with field teams, partners and donors to ensure maximum impact.",
                    Requirements = "• 5+ years in programme management in an NGO setting\n• Bachelor's or Master's degree in Social Work, Development Studies or related field\n• Fluent in Vietnamese and English\n• Experience with M&E frameworks and donor reporting\n• Strong analytical and writing skills",
                    Responsibilities = "• Design and manage child welfare programmes\n• Coordinate with field teams and partner organisations\n• Prepare donor reports and programme documentation\n• Monitor and evaluate programme outcomes\n• Represent GiveAID at cluster meetings and workshops",
                    Location = "Hanoi, Vietnam",
                    EmploymentType = "Full-time",
                    SalaryRange = "VND 25,000,000 – 35,000,000/month",
                    Vacancies = 2,
                    PostedDate = DateTime.UtcNow.AddDays(-14),
                    ClosingDate = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Career
                {
                    PositionTitle = "Frontend Developer (React)",
                    Department = "Technology",
                    Description = "Join our tech team to build beautiful, accessible and performant React interfaces for our donor and admin platforms. You will work closely with designers and the backend team.",
                    Requirements = "• 3+ years of professional React development\n• Strong proficiency in JavaScript/TypeScript, HTML, CSS\n• Experience with React Bootstrap, Redux or Zustand\n• Understanding of accessibility (WCAG 2.1) and responsive design\n• Experience with REST API integration",
                    Responsibilities = "• Build and maintain React components for donor and admin portals\n• Optimise UI performance and responsiveness\n• Collaborate with UX designers on new features\n• Write unit and integration tests\n• Participate in code reviews and technical design",
                    Location = "Remote (Vietnam)",
                    EmploymentType = "Full-time",
                    SalaryRange = "VND 20,000,000 – 30,000,000/month",
                    Vacancies = 1,
                    PostedDate = DateTime.UtcNow.AddDays(-7),
                    ClosingDate = DateTime.UtcNow.AddDays(28),
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Career
                {
                    PositionTitle = "Communications & Storytelling Officer",
                    Department = "Communications",
                    Description = "Help us tell the stories that move donors to act. You will produce written content, manage social media, and coordinate with our photography and video team to showcase real impact.",
                    Requirements = "• 3+ years in communications, journalism or content marketing\n• Excellent writing skills in Vietnamese and English\n• Experience with social media management (Facebook, Instagram, LinkedIn)\n• Basic photo/video editing skills\n• Passion for humanitarian storytelling with ethics",
                    Responsibilities = "• Write impact stories, donor spotlights and newsletter content\n• Manage social media calendars and community engagement\n• Coordinate with photographers and field staff for content\n• Support fundraising campaign communications\n• Track and report on communications KPIs",
                    Location = "Ho Chi Minh City, Vietnam",
                    EmploymentType = "Full-time",
                    SalaryRange = "VND 15,000,000 – 22,000,000/month",
                    Vacancies = 1,
                    PostedDate = DateTime.UtcNow.AddDays(-21),
                    ClosingDate = DateTime.UtcNow.AddDays(14),
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Career
                {
                    PositionTitle = "Volunteer Coordinator (Part-time)",
                    Department = "Human Resources",
                    Description = "Recruit, onboard and manage our volunteer community. You will organise volunteer events, training sessions and recognition programmes.",
                    Requirements = "• 2+ years in volunteer management or HR\n• Strong organisational and interpersonal skills\n• Ability to work evenings and weekends for volunteer events\n• Vietnamese and English proficiency\n• Experience with volunteer management software preferred",
                    Responsibilities = "• Recruit and screen new volunteers\n• Organise volunteer orientation and training\n• Coordinate volunteer schedules for events\n• Maintain volunteer database and records\n• Recognise and retain top volunteers",
                    Location = "Hanoi, Vietnam",
                    EmploymentType = "Part-time",
                    SalaryRange = "VND 10,000,000 – 14,000,000/month",
                    Vacancies = 1,
                    PostedDate = DateTime.UtcNow.AddDays(-5),
                    ClosingDate = DateTime.UtcNow.AddDays(25),
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Careers.AddRange(careers);
            await context.SaveChangesAsync();
        }

        // Seed Donations if empty (MAJ-004)
        if (!await context.Donations.AnyAsync())
        {
            var donationAdminUser = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Users, u => u.Username == "admin");
            var donationDemoUser = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Users, u => u.Username == "demo");

            var causes = await context.Causes.Take(6).ToListAsync();
            var campaigns = await context.Campaigns.Take(4).ToListAsync();

            var today = DateTime.UtcNow;
            var donations = new List<Donation>
            {
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(0)?.CauseId ?? 1, CampaignId = campaigns.ElementAtOrDefault(0)?.CampaignId, Amount = 500000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", TransactionId = "TXN-001-001", Message = "Keep up the great work!", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-60), PaymentConfirmedAt = today.AddDays(-60), CreatedAt = today.AddDays(-60) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(1)?.CauseId ?? 2, CampaignId = campaigns.ElementAtOrDefault(1)?.CampaignId, Amount = 200000, PaymentMethod = "DebitCard", PaymentStatus = "Completed", TransactionId = "TXN-001-002", IsAnonymous = true, ReceiptSent = true, DonationDate = today.AddDays(-58), PaymentConfirmedAt = today.AddDays(-58), CreatedAt = today.AddDays(-58) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(2)?.CauseId ?? 3, CampaignId = campaigns.ElementAtOrDefault(2)?.CampaignId, Amount = 1000000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "4242", CardType = "Visa", TransactionId = "TXN-001-003", Message = "For the children!", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-55), PaymentConfirmedAt = today.AddDays(-55), CreatedAt = today.AddDays(-55) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(0)?.CauseId ?? 1, Amount = 100000, PaymentMethod = "NetBanking", PaymentStatus = "Completed", TransactionId = "TXN-001-004", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-50), PaymentConfirmedAt = today.AddDays(-50), CreatedAt = today.AddDays(-50) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(3)?.CauseId ?? 4, CampaignId = campaigns.ElementAtOrDefault(3)?.CampaignId, Amount = 2000000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "1234", CardType = "Mastercard", TransactionId = "TXN-001-005", Message = "A monthly gift to support your mission", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-45), PaymentConfirmedAt = today.AddDays(-45), CreatedAt = today.AddDays(-45) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(1)?.CauseId ?? 2, Amount = 300000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "5678", CardType = "Visa", TransactionId = "TXN-001-006", IsAnonymous = true, ReceiptSent = true, DonationDate = today.AddDays(-40), PaymentConfirmedAt = today.AddDays(-40), CreatedAt = today.AddDays(-40) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(4)?.CauseId ?? 5, CampaignId = campaigns.ElementAtOrDefault(0)?.CampaignId, Amount = 500000, PaymentMethod = "DebitCard", PaymentStatus = "Completed", TransactionId = "TXN-001-007", Message = "Happy to support!", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-35), PaymentConfirmedAt = today.AddDays(-35), CreatedAt = today.AddDays(-35) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(2)?.CauseId ?? 3, Amount = 750000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "9012", CardType = "Visa", TransactionId = "TXN-001-008", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-30), PaymentConfirmedAt = today.AddDays(-30), CreatedAt = today.AddDays(-30) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(0)?.CauseId ?? 1, CampaignId = campaigns.ElementAtOrDefault(1)?.CampaignId, Amount = 1500000, PaymentMethod = "NetBanking", PaymentStatus = "Completed", TransactionId = "TXN-001-009", Message = "Urgent relief needed!", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-25), PaymentConfirmedAt = today.AddDays(-25), CreatedAt = today.AddDays(-25) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(3)?.CauseId ?? 4, Amount = 400000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "3456", CardType = "Mastercard", TransactionId = "TXN-001-010", IsAnonymous = true, ReceiptSent = true, DonationDate = today.AddDays(-20), PaymentConfirmedAt = today.AddDays(-20), CreatedAt = today.AddDays(-20) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(5)?.CauseId ?? 6, CampaignId = campaigns.ElementAtOrDefault(2)?.CampaignId, Amount = 3000000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "7890", CardType = "Visa", TransactionId = "TXN-001-011", Message = "For the children in flood-affected areas", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-15), PaymentConfirmedAt = today.AddDays(-15), CreatedAt = today.AddDays(-15) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(1)?.CauseId ?? 2, Amount = 200000, PaymentMethod = "DebitCard", PaymentStatus = "Completed", TransactionId = "TXN-001-012", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-12), PaymentConfirmedAt = today.AddDays(-12), CreatedAt = today.AddDays(-12) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(2)?.CauseId ?? 3, CampaignId = campaigns.ElementAtOrDefault(3)?.CampaignId, Amount = 1000000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "2468", CardType = "Visa", TransactionId = "TXN-001-013", Message = "Supporting education for all children", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-10), PaymentConfirmedAt = today.AddDays(-10), CreatedAt = today.AddDays(-10) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(0)?.CauseId ?? 1, Amount = 250000, PaymentMethod = "NetBanking", PaymentStatus = "Completed", TransactionId = "TXN-001-014", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-7), PaymentConfirmedAt = today.AddDays(-7), CreatedAt = today.AddDays(-7) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(4)?.CauseId ?? 5, CampaignId = campaigns.ElementAtOrDefault(0)?.CampaignId, Amount = 2000000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "1357", CardType = "Mastercard", TransactionId = "TXN-001-015", Message = "Monthly donation", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-5), PaymentConfirmedAt = today.AddDays(-5), CreatedAt = today.AddDays(-5) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(3)?.CauseId ?? 4, Amount = 600000, PaymentMethod = "CreditCard", PaymentStatus = "Completed", CardLastFour = "8021", CardType = "Visa", TransactionId = "TXN-001-016", IsAnonymous = true, ReceiptSent = true, DonationDate = today.AddDays(-3), PaymentConfirmedAt = today.AddDays(-3), CreatedAt = today.AddDays(-3) },
                new Donation { UserId = donationAdminUser?.UserId ?? 1, CauseId = causes.ElementAtOrDefault(1)?.CauseId ?? 2, CampaignId = campaigns.ElementAtOrDefault(2)?.CampaignId, Amount = 5000000, PaymentMethod = "NetBanking", PaymentStatus = "Completed", TransactionId = "TXN-001-017", Message = "From our family to yours", IsAnonymous = false, ReceiptSent = true, DonationDate = today.AddDays(-1), PaymentConfirmedAt = today.AddDays(-1), CreatedAt = today.AddDays(-1) },
                new Donation { UserId = donationDemoUser?.UserId ?? 2, CauseId = causes.ElementAtOrDefault(5)?.CauseId ?? 6, Amount = 150000, PaymentMethod = "DebitCard", PaymentStatus = "Completed", TransactionId = "TXN-001-018", IsAnonymous = false, ReceiptSent = true, DonationDate = today, PaymentConfirmedAt = today, CreatedAt = today }
            };
            context.Donations.AddRange(donations);
            await context.SaveChangesAsync();
        }

        // Seed extra CMS pages for keys the public frontend reads. The
        // base seed above already inserts home/about/contact/privacy/terms;
        // the rows below are admin-editable sections rendered by AboutPage
        // and ContactPage when present.
        if (await context.CmsPages.AnyAsync())
        {
            var extras = new List<CmsPage>
            {
                new CmsPage
                {
                    PageKey = "our_mission",
                    PageSlug = "our-mission",
                    PageTitle = "Our Mission, Vision & Promise",
                    Content = "<p><strong>Mission:</strong> We deliver transparent, evidence-based programmes that provide food, education, healthcare and safe shelter to children who need them most.</p><p><strong>Vision:</strong> We envision a future where no child is denied food, schooling, medical care, or love — and where communities sustain that future themselves.</p><p><strong>Promise:</strong> Every donor receives detailed impact reports. Every programme is independently audited. Every story is told with dignity and consent.</p>",
                    IsActive = true,
                    IsInMenu = false,
                    DisplayOrder = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new CmsPage
                {
                    PageKey = "what_we_do",
                    PageSlug = "what-we-do",
                    PageTitle = "What We Do",
                    Content = "<h3>Four pillars of change</h3><p>Every programme we run falls into one of four pillars — designed to give children the foundations for a healthy, hopeful life.</p><ul><li><strong>Nutritious Meals:</strong> Daily meals, food packages and nutrition programmes that combat child hunger across Vietnam.</li><li><strong>Education Access:</strong> School supplies, scholarships, libraries and free English classes for rural and under-served children.</li><li><strong>Healthcare &amp; Wellness:</strong> Mobile clinics, free health checks, heart surgeries and nutrition support.</li><li><strong>Safe Shelter:</strong> Long-term residential care homes and emergency shelter for orphans and at-risk children.</li></ul>",
                    IsActive = true,
                    IsInMenu = false,
                    DisplayOrder = 101,
                    CreatedAt = DateTime.UtcNow
                },
                new CmsPage
                {
                    PageKey = "contact_info",
                    PageSlug = "contact-info",
                    PageTitle = "Additional Information",
                    Content = "<p><strong>Office hours:</strong> Monday to Friday, 9:00 AM – 6:00 PM (GMT+7).</p><p><strong>Out-of-hours emergencies:</strong> For urgent humanitarian matters, email <a href=\"mailto:emergency@care4kids.org\">emergency@care4kids.org</a>.</p><p><strong>Media &amp; press:</strong> All media inquiries should be directed to <a href=\"mailto:press@care4kids.org\">press@care4kids.org</a>. We respond within 1 business day.</p>",
                    IsActive = true,
                    IsInMenu = false,
                    DisplayOrder = 102,
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var extra in extras)
            {
                if (!await context.CmsPages.AnyAsync(p => p.PageKey == extra.PageKey))
                {
                    context.CmsPages.Add(extra);
                }
            }
            await context.SaveChangesAsync();
        }

        return true;
    }
}
