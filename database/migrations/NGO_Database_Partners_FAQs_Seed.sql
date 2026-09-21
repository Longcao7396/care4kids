-- =============================================================================
-- GiveAID Database Migration: Additional Partners & FAQs Seed
-- Created: 2026-09-18
-- Purpose: Add realistic data for Organizations and FAQs
-- IDEMPOTENT: Safe to run multiple times
-- =============================================================================

SET NOCOUNT ON;
GO

-- =============================================================================
-- PART 1: ADDITIONAL ORGANIZATIONS (Partners)
-- Only using valid types: Supporter, Partner, NGO
-- =============================================================================

-- 1. VinaCapital Foundation - as Partner
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-014')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('VinaCapital Foundation', 'Partner',
        'The VinaCapital Foundation is a USA-based charitable foundation that supports capacity building, healthcare, and education programs for underprivileged children and families in Vietnam.',
        NULL, 'https://vina-capital.com/foundation/',
        'info@vinacapitalfoundation.org', '+84-28-3821-0900', '17th Floor, Bitexco Financial Tower, 2 Hai Trieu Street, District 1, Ho Chi Minh City',
        'NGO-VN-014',
        'To improve the lives of the underprivileged children and families in Vietnam through support in healthcare, education, and community development.',
        'A Vietnam where every child has access to healthcare and education to reach their full potential.',
        80000.00, 'Financial', 1, 1, 23, GETDATE(), GETDATE());
END
GO

-- 2. Lotus Fund - as Partner
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-015')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('Lotus Fund', 'Partner',
        'Lotus Fund supports education and healthcare for poor children in Vietnam. Named after the lotus flower that grows from mud yet blooms beautifully, we believe every child deserves a chance to succeed.',
        NULL, 'https://lotusfund.vn',
        'info@lotusfund.vn', '+84-24-3936-2700', '38 Giang Vo Street, Ba Dinh District, Hanoi',
        'NGO-VN-015',
        'To support education and healthcare for underprivileged children in Vietnam, helping them break the cycle of poverty.',
        'A Vietnam where every child, regardless of background, has the opportunity to thrive.',
        25000.00, 'Financial', 1, 0, 24, GETDATE(), GETDATE());
END
GO

-- 3. World Bank Vietnam - as Partner
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-023')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('World Bank Vietnam', 'Partner',
        'The World Bank works with the Government of Vietnam to reduce poverty and promote shared prosperity through policy advice, research, and investment financing in education, health, and infrastructure.',
        NULL, 'https://www.worldbank.org/en/country/vietnam',
        'vietnam@worldbank.org', '+84-24-3934-6600', '63 Ly Thai To Street, Hoan Kiem District, Hanoi',
        'NGO-VN-023',
        'To help developing countries reduce poverty and promote shared prosperity by providing knowledge, financing, and technical assistance.',
        'A world free of poverty.',
        500000.00, 'Financial', 1, 1, 25, GETDATE(), GETDATE());
END
GO

-- 4. FPT Corporation - as Partner
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-024')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('FPT Corporation', 'Partner',
        'FPT Corporation is Vietnam''s leading technology company, providing digital transformation services globally. FPT is committed to corporate social responsibility and has supported numerous charitable initiatives.',
        NULL, 'https://fpt.com.vn',
        'csr@fpt.com.vn', '+84-24-7300-1818', 'FPT Building, 17 Duy Tan Street, Cau Giay District, Hanoi',
        'NGO-VN-024',
        'To pioneer digital transformation and create shared values that contribute to the sustainable development of society.',
        'To become a global leading technology and AI services company that drives digital transformation for clients worldwide.',
        40000.00, 'Financial', 1, 0, 26, GETDATE(), GETDATE());
END
GO

-- 5. Vingroup - as Partner
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-025')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('Vingroup', 'Partner',
        'Vingroup is one of Vietnam''s largest conglomerates, with businesses in real estate, retail, healthcare, education, and technology. The group actively supports charitable causes through its Vingroup Foundation.',
        NULL, 'https://vingroup.net',
        'csr@vingroup.com', '+84-24-3974-9999', '7th Floor, River View Building, 11 Nguyen Gia Thieu Street, Ba Dinh District, Hanoi',
        'NGO-VN-025',
        'To contribute to the development of Vietnamese society through pioneering applications of technology in business and social programs.',
        'To build VinUniversity and VinBigdata as world-class institutions that attract global talent.',
        100000.00, 'Financial', 1, 0, 27, GETDATE(), GETDATE());
END
GO

-- 6. MSF Vietnam - as NGO
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-010')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('MSF Vietnam', 'NGO',
        'MSF provides independent, neutral medical humanitarian assistance to people affected by conflict, epidemics, disasters, or exclusion from healthcare. We have been working in Vietnam since 1990.',
        NULL, 'https://www.msf.org/vietnam',
        'hanoi@msf.org', '+84-24-3844-6100', '46 Hoang Sam Street, Nghia Do Ward, Cau Giay District, Hanoi',
        'NGO-VN-010',
        'To provide medical care to people affected by conflict, epidemics, and disasters, regardless of race, religion, or political affiliation.',
        'A world where healthcare is accessible to all, and where medical ethics are upheld.',
        55000.00, 'Medical Services', 1, 1, 28, GETDATE(), GETDATE());
END
GO

-- 7. Additional Supporter - Vietnam Corporate Alliance
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'SUPP-VN-001')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('Vietnam Corporate Alliance', 'Supporter',
        'A coalition of Vietnamese corporations committed to social responsibility and sustainable development. Members pool resources to support education, healthcare, and environmental initiatives nationwide.',
        NULL, 'https://vca.org.vn',
        'info@vca.org.vn', '+84-24-3944-5000', '8th Floor, Hoa Binh Building, 106 Hoang Quoc Viet Street, Cau Giay District, Hanoi',
        'SUPP-VN-001',
        'To unite Vietnamese businesses in creating positive social impact through collaborative philanthropy and sustainable development programs.',
        'A Vietnam where businesses are drivers of social change and environmental stewardship.',
        150000.00, 'Financial', 1, 0, 6, GETDATE(), GETDATE());
END
GO

-- 8. Additional Partner - Viettel Social Policy Bank
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'PART-VN-001')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('Viettel Social Fund', 'Partner',
        'Viettel Social Fund supports education, healthcare, and rural development programs in disadvantaged areas of Vietnam. Established by Viettel Group, Vietnam''s largest telecommunications company.',
        NULL, 'https://viettel.com.vn',
        'csr@viettel.com.vn', '+84-24-3818-1000', '01 Giang Vo Street, Ba Dinh District, Hanoi',
        'PART-VN-001',
        'To contribute to social welfare and sustainable development by supporting disadvantaged communities in education, healthcare, and infrastructure.',
        'A Vietnam where technology bridges the gap between urban and rural communities.',
        120000.00, 'Financial', 1, 1, 29, GETDATE(), GETDATE());
END
GO

-- 9. Additional NGO -春蕾计划中国
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'NGO-VN-026')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('Spring Bud Foundation Vietnam', 'NGO',
        'Spring Bud Foundation supports education for girls and children from ethnic minorities in mountainous regions. We provide scholarships, school supplies, and boarding support to ensure children stay in school.',
        NULL, 'https://springbudvietnam.org',
        'info@springbudvietnam.org', '+84-26-3838-9000', '45 Nguyen Trai Street, Thanh Hoa City, Thanh Hoa Province',
        'NGO-VN-026',
        'To ensure every girl and child from disadvantaged backgrounds has access to quality education and the opportunity to reach their full potential.',
        'A Vietnam where every child, regardless of gender or ethnicity, can dream and achieve.',
        18000.00, 'Educational', 1, 0, 30, GETDATE(), GETDATE());
END
GO

-- 10. Additional Supporter - Petrovietnam Community Fund
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE registration_number = 'SUPP-VN-002')
BEGIN
    INSERT INTO Organizations (organization_name, organization_type, description, logo_url, website_url,
        contact_email, contact_phone, address, registration_number, mission, vision,
        contribution_amount, contribution_type, is_active, is_featured, display_order,
        created_at, updated_at)
    VALUES ('Petrovietnam Community Fund', 'Supporter',
        'Petrovietnam Community Fund supports energy access, education, and healthcare in rural and remote communities. Established by Petrovietnam, Vietnam''s national oil and gas corporation.',
        NULL, 'https://pvn.com.vn',
        'csr@pvn.com.vn', '+84-24-3832-1000', 'Block A, 173 Nguyen Truong To Street, Ba Dinh District, Hanoi',
        'SUPP-VN-002',
        'To support sustainable community development in energy access, education, and healthcare for underserved populations in Vietnam.',
        'A Vietnam where energy and education reach every community.',
        90000.00, 'Financial', 1, 0, 7, GETDATE(), GETDATE());
END
GO

-- =============================================================================
-- PART 2: FAQs - Using existing valid categories
-- Categories: Account, Child Welfare, Donations, General, Partners, Programmes, Volunteering
-- =============================================================================

-- FAQ: How do I make a donation on GiveAID?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How do I make a donation on GiveAID?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How do I make a donation on GiveAID?',
        '<p>Making a donation on GiveAID is quick and secure. Follow these steps:</p><ol><li><strong>Create an Account:</strong> Register for a free GiveAID account to track your donations and receive updates.</li><li><strong>Browse Causes:</strong> Visit our Campaigns or Causes pages to find a cause you care about.</li><li><strong>Select a Campaign:</strong> Click on any campaign to learn more about its goals, progress, and the organization behind it.</li><li><strong>Enter Donation Amount:</strong> Choose how much you would like to donate. Even small amounts make a big difference when combined with other donors.</li><li><strong>Choose Payment Method:</strong> We accept credit/debit cards, bank transfers, and mobile payments (MoMo, ZaloPay, VNPay).</li><li><strong>Confirm Donation:</strong> Review your donation details and confirm. You will receive an email confirmation with your donation receipt.</li></ol><p>All donations are processed securely through our certified payment gateway. Your financial information is never stored on our servers.</p>',
        'Donations', 7, 1, 1, 312, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Is my donation tax-deductible?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Is my donation tax-deductible?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Is my donation tax-deductible?',
        '<p>Yes, donations to GiveAID''s registered non-profit partners may be tax-deductible depending on your country of residence and the tax laws applicable to you.</p><p><strong>For Vietnamese donors:</strong></p><p>GiveAID works with registered NGOs that are recognized by Vietnamese authorities. While Vietnamese tax law does not currently provide a general deduction for charitable donations, many employers offer matching programs or volunteer benefits.</p><p><strong>For international donors:</strong></p><p>Tax deductibility depends on the specific charity and your country of tax residency. Most of our partner organizations are registered as 501(c)(3) equivalents or have equivalent status in their respective countries.</p><p><strong>Receipts:</strong></p><p>Every donation automatically generates a tax receipt that you can download from your My Donations page.</p>',
        'Donations', 8, 1, 1, 198, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: What payment methods do you accept?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'What payment methods do you accept?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('What payment methods do you accept?',
        '<p>We offer a variety of secure payment options to make donating as convenient as possible:</p><h6>Cards</h6><ul><li>Visa</li><li>Mastercard</li><li>JCB</li><li>American Express</li></ul><h6>Bank Transfers</h6><ul><li>Local bank transfers via Vietcombank, VietinBank, BIDV, and others</li><li>International wire transfers (SWIFT)</li></ul><h6>Mobile Wallets</h6><ul><li>MoMo</li><li>ZaloPay</li><li>VNPay</li><li>AirPay</li></ul><h6>Other Methods</h6><ul><li>PayPal (for international transactions)</li></ul><p>All transactions are processed through our PCI-DSS compliant payment gateway.</p>',
        'Donations', 9, 0, 1, 145, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Can I set up a recurring donation?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Can I set up a recurring donation?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Can I set up a recurring donation?',
        '<p>Yes! Recurring donations are one of the most impactful ways to support your chosen cause. By setting up a recurring donation, you provide organizations with predictable funding that allows them to plan and execute longer-term programs.</p><p><strong>How to set up a recurring donation:</strong></p><ol><li>Go to any campaign page and click Donate</li><li>Toggle the Make this recurring option</li><li>Choose your frequency: Weekly, Monthly, or Quarterly</li><li>Enter your donation amount and payment details</li><li>Confirm your recurring donation</li></ol><p><strong>Managing your recurring donation:</strong></p><ul><li>You can modify or cancel your recurring donation at any time from your My Donations page.</li><li>You will receive an email reminder 3 days before each scheduled donation.</li><li>Each recurring donation generates a separate receipt.</li></ul>',
        'Donations', 10, 0, 1, 167, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How is my donation used?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How is my donation used?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How is my donation used?',
        '<p>We believe in complete transparency. Here is how your donation is used:</p><h6>Where your money goes:</h6><p>When you donate to a campaign on GiveAID, your funds go directly to the organization running that campaign. Each organization commits to spending at least 80% of donations on program activities (direct services to beneficiaries).</p><h6>Administrative costs:</h6><p>GiveAID retains a small platform fee (typically 2-5% depending on payment method) to cover payment processing costs, platform maintenance, and security.</p><h6>How we ensure accountability:</h6><ul><li><strong>Regular Reporting:</strong> Partner organizations submit quarterly reports on their activities and spending.</li><li><strong>Impact Updates:</strong> Campaign pages are updated with progress reports, photos, and beneficiary stories.</li><li><strong>Financial Audits:</strong> All partner organizations undergo annual financial audits.</li><li><strong>Real-Time Tracking:</strong> See exactly how much has been raised for each campaign.</li></ul>',
        'Donations', 11, 1, 1, 289, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Can I get a refund for my donation?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Can I get a refund for my donation?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Can I get a refund for my donation?',
        '<p>We understand that circumstances can change. Our refund policy is as follows:</p><h6>Standard Refund Process:</h6><ul><li>Donations may be refunded within 48 hours of the original transaction, provided the funds have not yet been disbursed to the recipient organization.</li><li>To request a refund, please contact us at refunds@giveaid.org with your donation details.</li><li>Refunds are processed within 5-10 business days to the original payment method.</li></ul><h6>Exceptions:</h6><ul><li>Donations made more than 48 hours ago, where funds have already been transferred to the organization, cannot be refunded.</li><li>Donations to campaigns that have already been completed and funds disbursed are non-refundable.</li></ul>',
        'Donations', 12, 0, 1, 124, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Why is my donation showing as Pending?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Why is my donation showing as Pending?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Why is my donation showing as Pending?',
        '<p>If your donation shows as Pending, it means the payment is still being processed. Here is what you need to know:</p><h6>Common Reasons for Pending Status:</h6><ul><li><strong>Bank transfers:</strong> May take 1-3 business days to clear, depending on the bank.</li><li><strong>Card authorization:</strong> Some banks hold the authorization before settling the actual charge.</li><li><strong>3D Secure verification:</strong> If your bank requires additional verification (e.g., OTP), the transaction will be pending until completed.</li></ul><h6>What to do:</h6><ol><li>Wait 1-3 business days for the payment to process.</li><li>Check with your bank to confirm they have authorized the transaction.</li><li>If using a card, ensure you have completed any required 3D Secure verification.</li></ol><p>Contact us at donations@giveaid.org with your transaction ID for assistance.</p>',
        'Donations', 13, 0, 1, 98, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: What are the campaigns on GiveAID?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'What are the campaigns on GiveAID?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('What are the campaigns on GiveAID?',
        '<p>Campaigns on GiveAID are specific fundraising initiatives run by our verified partner organizations. Each campaign has a clear goal, timeline, and impact plan.</p><h6>Campaign Types:</h6><ul><li><strong>Emergency Relief:</strong> Rapid-response campaigns for natural disasters, conflicts, or urgent humanitarian crises.</li><li><strong>Education:</strong> Programs building schools, providing scholarships, or delivering learning materials.</li><li><strong>Healthcare:</strong> Medical camps, hospital equipment, health insurance for children, and disease prevention programs.</li><li><strong>Clean Water and Sanitation:</strong> Building wells, distributing water filters, and constructing latrines.</li><li><strong>Women and Children:</strong> Programs supporting vulnerable women, orphans, and children with disabilities.</li><li><strong>Environment:</strong> Reforestation, marine conservation, and sustainable agriculture projects.</li></ul><h6>How Campaigns Work:</h6><ol><li>Partner organizations create campaigns with specific goals and timelines.</li><li>GiveAID reviews and verifies each campaign before it goes live.</li><li>Donors browse and contribute to campaigns they care about.</li><li>Organizations update donors on progress throughout the campaign.</li><li>Funds are disbursed to the organization upon campaign completion.</li></ol>',
        'Programmes', 4, 1, 1, 234, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How do I start a campaign?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How do I start a campaign?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How do I start a campaign?',
        '<p>GiveAID currently accepts campaigns from verified non-profit organizations. If you represent a registered NGO and would like to start a campaign, follow the steps below:</p><h6>Eligibility Requirements:</h6><ul><li>Your organization must be a registered non-profit in Vietnam or have a registered local partner.</li><li>Your organization must have a valid registration certificate and audited financial statements.</li><li>The campaign must have a clear, measurable goal and realistic budget.</li><li>At least 80% of funds raised must go directly to program activities.</li></ul><h6>Application Process:</h6><ol><li><strong>Register as a Partner:</strong> Submit your organization''s details through our Partner Application Form.</li><li><strong>Verification:</strong> Our team will review your registration documents and conduct a verification call (usually within 5 business days).</li><li><strong>Campaign Proposal:</strong> Once verified, submit your campaign proposal including goals, timeline, budget breakdown, and beneficiary information.</li><li><strong>Campaign Review:</strong> GiveAID reviews the proposal (typically 3-5 business days) and may request clarifications.</li><li><strong>Launch:</strong> Once approved, your campaign goes live!</li></ol><p>Contact our Partner Relations team at partners@giveaid.org.</p>',
        'Partners', 2, 0, 1, 178, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How does GiveAID verify its partner organizations?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How does GiveAID verify its partner organizations?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How does GiveAID verify its partner organizations?',
        '<p>We take due diligence seriously. Every organization on GiveAID undergoes a thorough verification process before being approved as a partner.</p><h6>Verification Criteria:</h6><ul><li><strong>Legal Registration:</strong> Must have a valid business/charity registration certificate.</li><li><strong>Financial Transparency:</strong> Must have audited financial statements for the past two years.</li><li><strong>Track Record:</strong> Must demonstrate at least two years of active operations with measurable impact.</li><li><strong>Governance:</strong> Must have a documented governance structure with clear roles and accountability.</li><li><strong>Anti-Fraud Policies:</strong> Must have policies against fraud, corruption, and misuse of funds.</li></ul><h6>Ongoing Monitoring:</h6><ul><li><strong>Quarterly Reports:</strong> Partners submit financial and program progress reports every quarter.</li><li><strong>Annual Audits:</strong> All partners must submit annual audited financial statements.</li><li><strong>Spot Checks:</strong> We conduct random field visits and beneficiary verification calls.</li></ul>',
        'Partners', 3, 0, 1, 134, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Can I volunteer with GiveAID partner organizations?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Can I volunteer with GiveAID partner organizations?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Can I volunteer with GiveAID partner organizations?',
        '<p>Yes! Volunteering is a wonderful way to make a direct impact. While GiveAID is primarily a donation platform, we connect volunteers with our partner organizations.</p><h6>How to Volunteer:</h6><ol><li><strong>Browse Opportunities:</strong> Check our Volunteer Opportunities page for current openings at partner organizations.</li><li><strong>Register Interest:</strong> Fill out the volunteer registration form with your skills and availability.</li><li><strong>Orientation:</strong> Selected volunteers attend a brief orientation with the partner organization.</li><li><strong>Start Volunteering:</strong> Join on-site or remote volunteering activities.</li></ol><h6>Types of Volunteer Work:</h6><ul><li><strong>On-site:</strong> Teaching, medical assistance, construction, event support, community outreach</li><li><strong>Remote:</strong> Translation, content writing, graphic design, social media, fundraising support</li><li><strong>Professional:</strong> Legal advice, accounting, IT support, marketing strategy</li></ul>',
        'Volunteering', 5, 0, 1, 156, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: What happens if a campaign does not reach its goal?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'What happens if a campaign does not reach its goal?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('What happens if a campaign does not reach its goal?',
        '<p>We believe every contribution matters, even if a campaign does not reach 100% of its goal. Our policy for incomplete campaigns is as follows:</p><h6>Flexible Funding Campaigns:</h6><p>Some campaigns are marked as Flexible Funding, meaning the organization can adjust their approach based on the amount raised:</p><ul><li>If a campaign raises 50% of its goal, the organization may deliver a scaled-down version of the program.</li><li>If a campaign raises 75%, they may reduce the scope while maintaining core activities.</li><li>The organization will communicate any changes in approach to donors via email.</li></ul><h6>Fixed Funding Campaigns:</h6><p>Campaigns marked as Fixed Funding will only receive funds if they reach their goal:</p><ul><li>If the campaign does not reach its goal, all donations are automatically refunded within 5 business days.</li><li>Donors are notified before donating if a campaign requires 100% funding.</li></ul>',
        'Programmes', 5, 0, 1, 112, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How do I update my profile information?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How do I update my profile information?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How do I update my profile information?',
        '<p>Keeping your profile information up to date ensures you receive important updates and receipts. Here is how to update your profile:</p><h6>To update your profile:</h6><ol><li>Log in to your GiveAID account.</li><li>Click on your profile name or avatar in the top-right corner.</li><li>Select My Profile from the dropdown menu.</li><li>Click Edit Profile.</li><li>Update the fields you wish to change: Full name, Email address, Phone number, Address, Notification preferences.</li><li>Click Save Changes.</li></ol><h6>Changing your email address:</h6><p>If you change your email address, you will receive a verification email at the new address. You must click the verification link to confirm the change.</p>',
        'Account', 6, 0, 1, 98, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How do I download my donation receipt?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How do I download my donation receipt?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How do I download my donation receipt?',
        '<p>You can download official donation receipts at any time from your GiveAID account. Here is how:</p><h6>To download a receipt:</h6><ol><li>Log in to your GiveAID account.</li><li>Go to My Donations from your profile menu.</li><li>Find the donation you want a receipt for.</li><li>Click the Download Receipt button next to that donation.</li><li>The receipt will download as a PDF file.</li></ol><h6>What the receipt includes:</h6><ul><li>Your name (or Anonymous Donor if you donated anonymously)</li><li>Donation amount and currency</li><li>Date and time of donation</li><li>Campaign and organization name</li><li>Transaction ID</li><li>GiveAID platform details and registration number</li></ul>',
        'Account', 7, 0, 1, 178, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How do I cancel my account?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How do I cancel my account?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How do I cancel my account?',
        '<p>We are sorry to see you go. If you wish to delete your GiveAID account, please follow these steps:</p><h6>Before You Leave:</h6><ul><li>Download any donation receipts you may need from your My Donations page.</li><li>Cancel any active recurring donations.</li><li>Note that account deletion is permanent and cannot be undone.</li></ul><h6>How to Delete Your Account:</h6><ol><li>Log in to your GiveAID account.</li><li>Go to My Profile > Security Settings.</li><li>Scroll to the bottom and click Delete Account.</li><li>Confirm your decision by entering your password.</li><li>Click Permanently Delete Account.</li></ol>',
        'Account', 8, 0, 1, 67, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Is my personal information secure?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Is my personal information secure?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Is my personal information secure?',
        '<p>Yes, protecting your personal information is our top priority. We use industry-leading security measures to keep your data safe.</p><h6>Security Measures:</h6><ul><li><strong>Encryption:</strong> All data is encrypted in transit using TLS 1.3 and at rest using AES-256.</li><li><strong>PCI-DSS Compliance:</strong> Our payment processing is fully PCI-DSS compliant. We never store your full credit card details.</li><li><strong>Two-Factor Authentication:</strong> We offer 2FA via email or authenticator app for an extra layer of security.</li><li><strong>Regular Security Audits:</strong> Our systems are audited by independent security firms quarterly.</li></ul><h6>Your rights:</h6><p>You have the right to access, correct, or delete your personal data. Contact privacy@giveaid.org for any data-related requests.</p>',
        'Account', 9, 0, 1, 198, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How does GiveAID protect against fraud?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How does GiveAID protect against fraud?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How does GiveAID protect against fraud?',
        '<p>Fraud prevention is fundamental to our operations. We employ multiple layers of security to protect donors and beneficiaries.</p><h6>For Donors:</h6><ul><li><strong>Secure Payments:</strong> All transactions go through PCI-DSS compliant payment gateways.</li><li><strong>Email Verification:</strong> New accounts must verify their email address.</li><li><strong>Suspicious Activity Monitoring:</strong> Our system flags unusual donation patterns for review.</li><li><strong>Refund Protection:</strong> 48-hour refund window for eligible donations.</li></ul><h6>For Organizations:</h6><ul><li><strong>Rigorous Vetting:</strong> All partners undergo thorough verification before listing.</li><li><strong>Regular Audits:</strong> Financial statements and program reports are reviewed quarterly.</li><li><strong>Disbursement Controls:</strong> Funds are released based on verified milestones, not upfront.</li></ul><p>If you suspect fraudulent activity on GiveAID, please contact us at fraud@giveaid.org.</p>',
        'Child Welfare', 5, 0, 1, 145, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: What is GiveAID mission and vision?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'What is GiveAID mission and vision?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('What is GiveAID mission and vision?',
        '<p>GiveAID was founded with a simple but powerful belief: that everyone deserves the opportunity to make a difference, and that technology can make philanthropy more accessible, transparent, and impactful.</p><h6>Our Mission:</h6><p>To democratize philanthropy in Vietnam and Southeast Asia by providing a transparent, trustworthy, and user-friendly platform that connects donors with verified charitable organizations, enabling individuals and businesses to create meaningful social impact.</p><h6>Our Vision:</h6><p>A world where generosity is accessible to all, where every donation - no matter how small - creates real change, and where complete transparency builds trust between donors and the organizations they support.</p><h6>Our Core Values:</h6><ul><li><strong>Transparency:</strong> Every donation is tracked, every organization is verified.</li><li><strong>Trust:</strong> We build relationships based on integrity and accountability.</li><li><strong>Impact:</strong> We measure success not by funds raised, but by lives changed.</li><li><strong>Inclusivity:</strong> We believe everyone has something to give.</li></ul>',
        'General', 4, 0, 1, 156, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How can my company partner with GiveAID?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How can my company partner with GiveAID?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How can my company partner with GiveAID?',
        '<p>We welcome corporate partnerships! GiveAID offers several ways for businesses to get involved and demonstrate social responsibility.</p><h6>Partnership Options:</h6><ul><li><strong>Matching Gifts:</strong> Your company matches employee donations dollar-for-dollar, doubling the impact.</li><li><strong>Cause Marketing:</strong> Launch a co-branded campaign tied to your products or services.</li><li><strong>Employee Giving Programs:</strong> Set up workplace giving campaigns with easy payroll deduction.</li><li><strong>Sponsorship:</strong> Sponsor GiveAID platform features, events, or reports.</li><li><strong>Pro-Bono Services:</strong> Offer professional skills (legal, accounting, IT, marketing) to our partner NGOs.</li></ul><h6>Benefits of Partnering:</h6><ul><li>Demonstrate CSR commitments with measurable impact.</li><li>Engage employees through meaningful volunteer and giving opportunities.</li><li>Gain exposure to our growing donor community.</li><li>Receive detailed impact reports and recognition on our platform.</li></ul><p>Contact our Corporate Partnerships team at corporate@giveaid.org.</p>',
        'Partners', 4, 0, 1, 134, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Are there any fees for using GiveAID?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Are there any fees for using GiveAID?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Are there any fees for using GiveAID?',
        '<p>GiveAID is free for donors. We believe that nothing should stand between you and your generosity.</p><h6>For Donors:</h6><ul><li><strong>No registration fees</strong></li><li><strong>No platform fees</strong></li><li><strong>No monthly charges</strong></li></ul><p>The only cost is your actual donation. Payment processing fees (typically 2-3% for card payments) are built into our platform operations.</p><h6>For Partner Organizations:</h6><ul><li><strong>Platform fee:</strong> 5% of funds raised (covers payment processing, platform maintenance, and donor support).</li><li><strong>Verification fee:</strong> One-time $100 fee for new partner organizations (waived for organizations that complete our partnership application).</li></ul><p>All fees are clearly disclosed before organizations sign up. No hidden charges, ever.</p>',
        'General', 5, 0, 1, 189, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Can international donors use GiveAID?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Can international donors use GiveAID?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Can international donors use GiveAID?',
        '<p>Absolutely! GiveAID welcomes donors from around the world. We have made it easy for international donors to support Vietnamese and global causes.</p><h6>How International Donations Work:</h6><ul><li><strong>Currency:</strong> Donations can be made in multiple currencies (USD, EUR, GBP, VND, and more). Currency conversion is handled automatically at competitive exchange rates.</li><li><strong>Payment Methods:</strong> International donors can use credit/debit cards (Visa, Mastercard, Amex), PayPal, or international bank transfers.</li><li><strong>Tax Receipts:</strong> International donors receive receipts that include our platform registration details, which may be useful for tax purposes in their country of residence.</li></ul><h6>For US-Based Donors:</h6><p>Many of our partner organizations are registered 501(c)(3) entities, making donations potentially tax-deductible.</p><h6>For European Donors:</h6><p>Our platform complies with GDPR requirements. Donors from EU countries have full data protection rights.</p>',
        'General', 6, 0, 1, 167, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: Does GiveAID offer corporate volunteering programs?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'Does GiveAID offer corporate volunteering programs?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('Does GiveAID offer corporate volunteering programs?',
        '<p>Yes! GiveAID connects businesses with meaningful volunteer opportunities that align with their corporate social responsibility goals. Our corporate volunteering programs are designed to create real impact while building team cohesion.</p><h6>Types of Corporate Volunteering:</h6><ul><li><strong>Skilled Volunteering:</strong> Employees share professional expertise (IT, legal, marketing, finance) with our partner NGOs.</li><li><strong>Direct Service:</strong> Teams participate in hands-on activities like teaching, construction, or environmental projects.</li><li><strong>Mentorship Programs:</strong> Employees mentor students or young entrepreneurs from disadvantaged backgrounds.</li><li><strong>Disaster Response:</strong> Rapid-response volunteering during natural disasters or emergencies.</li></ul><h6>Benefits:</h6><ul><li>Boost employee engagement and satisfaction</li><li>Develop leadership and teamwork skills</li><li>Meet CSR and ESG reporting requirements</li><li>Build brand reputation and community ties</li></ul><p>Contact corporate@giveaid.org to schedule a consultation.</p>',
        'Volunteering', 6, 0, 1, 98, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: How do I contact GiveAID support?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'How do I contact GiveAID support?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('How do I contact GiveAID support?',
        '<p>We are here to help! There are several ways to reach our support team:</p><h6>Email Support:</h6><ul><li><strong>General Inquiries:</strong> hello@giveaid.org</li><li><strong>Donation Issues:</strong> donations@giveaid.org</li><li><strong>Technical Support:</strong> support@giveaid.org</li><li><strong>Partnership:</strong> partners@giveaid.org</li><li><strong>Privacy Concerns:</strong> privacy@giveaid.org</li></ul><p>We typically respond within 24 hours on business days.</p><h6>Contact Form:</h6><p>Use our online contact form for general questions and feedback. Select the relevant department to route your inquiry.</p><h6>Social Media:</h6><ul><li>Facebook: @GiveAIDVietnam</li><li>Twitter/X: @GiveAID</li><li>LinkedIn: GiveAID Vietnam</li></ul><h6>Office Hours:</h6><p>Monday to Friday, 9:00 AM - 6:00 PM (Vietnam Time, GMT+7). Our support team is available in Vietnamese and English.</p>',
        'General', 7, 1, 1, 201, GETDATE(), GETDATE(), 1);
END
GO

-- FAQ: What fees do partner organizations pay?
IF NOT EXISTS (SELECT 1 FROM Faqs WHERE question = 'What fees do partner organizations pay?')
BEGIN
    INSERT INTO Faqs (question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by)
    VALUES ('What fees do partner organizations pay?',
        '<p>We believe in transparency about platform costs. Here is a breakdown of fees for partner organizations:</p><h6>Platform Fee:</h6><p>GiveAID charges a 5% platform fee on all funds raised through campaigns. This fee covers:</p><ul><li>Payment processing costs (credit cards, bank transfers, mobile payments)</li><li>Platform maintenance and security infrastructure</li><li>Donor support services</li><li>Campaign management tools</li></ul><h6>Verification Fee:</h6><p>A one-time verification fee of $100 applies to new partner organizations. This covers the cost of due diligence, document review, and onboarding. This fee is waived for organizations referred by an existing GiveAID partner.</p><h6>Premium Features (Optional):</h6><ul><li><strong>Custom Campaign Pages:</strong> $50/campaign</li><li><strong>Advanced Analytics:</strong> $25/month</li><li><strong>Priority Support:</strong> $50/month</li><li><strong>Featured Placement:</strong> Starting at $200/campaign</li></ul>',
        'Partners', 5, 0, 1, 145, GETDATE(), GETDATE(), 1);
END
GO

-- =============================================================================
-- SUMMARY
-- =============================================================================
PRINT '';
PRINT '============================================';
PRINT 'Migration Complete: Additional Partners and FAQs';
PRINT '============================================';
PRINT 'Additional Organizations: 10 partners/supporters/NGOs';
PRINT 'Additional FAQs: 20 FAQs';
PRINT '============================================';
GO
