# Help Centre Page - Wireframe Documentation

## Overview
This document outlines the low-fidelity wireframe design for the **Care4Kids Help Centre** page. The Help Centre provides users with self-service support through search functionality, frequently asked questions, quick links, and access to create support conversations.

---

## Page Information

**Page Name:** Help Centre  
**Wireframe File:** `help-centre-wireframe.html`  
**Design Type:** Low-fidelity, black and white wireframe  
**Active Navigation:** Help Centre (underlined)

---

## Design System Consistency

### Typography
- **Headings:** Architects Daughter (hand-drawn appearance)
- **Body Text:** Courier Prime (monospace)
- **Font Sizes:** 
  - Page Title: 32px
  - Section Titles: 24px
  - Body Text: 13-14px
  - Small Text: 11-12px

### Visual Style
- **Borders:** 2-3px solid #222
- **Background:** White (#fff) on light gray (#fafafa)
- **Buttons:** 2px solid borders, primary buttons filled with #222
- **Text Placeholders:** Horizontal lines (2px height)
- **Low-fidelity aesthetic:** Hand-drawn feel, no colors/gradients/photos

### Layout Components
- Same header structure as all other pages
- Same footer with 4-column layout
- Same breadcrumb format
- Consistent spacing and padding

---

## Page Structure

### 1. Header (Consistent across all pages)
**Components:**
- Care4Kids logo placeholder (140px × 50px, bordered box)
- Navigation menu: Home | Campaigns | Donate | Programmes | Gallery | About Us | **Help Centre** (active)
- Auth buttons: [Login] [Register]

**Specifications:**
- Border: 2px solid #222
- Padding: 20px
- "Help Centre" has 2px bottom border to indicate active state

---

### 2. Breadcrumb
**Content:** Home > Help Centre

**Specifications:**
- Font size: 12px
- Margin bottom: 15px

---

### 3. Page Header
**Content:** 
- H1: "Help Centre"

**Specifications:**
- Font: Architects Daughter, 32px
- Margin bottom: 40px

---

### 4. Search Section
**Purpose:** Allow users to search for help articles and FAQs

**Components:**
- Large search input field
  - Placeholder text: "Search for help................................."
  - Full width with flex: 1
  - Height: 45px
  - Border: 2px solid #222
- [SEARCH] button
  - Background: #222 (black)
  - Color: white
  - Padding: 12px 30px

**Specifications:**
- Container border: 2px solid #222
- Container padding: 30px
- Display: flex with 15px gap
- Margin bottom: 40px

**UX Notes:**
- Search should query both FAQ content and help articles
- Real-time suggestions could be added in high-fidelity version
- Search results would appear below this section

---

### 5. Frequently Asked Questions Section
**Purpose:** Provide quick answers to common questions

**Section Title:** "Frequently Asked Questions"
- Font: Architects Daughter, 24px

**FAQ Items (6 total):**
Each FAQ displayed as a collapsible item (wireframe representation only):

1. **How do I donate?**
2. **How can I view my donation history?**
3. **How do I register for a programme?**
4. **Can I donate anonymously?**
5. **How can I contact Care4Kids?**
6. **What happens when a campaign is completed?**

**FAQ Item Structure:**
- Question text on left (14px, bold)
- [+] icon on right (24px × 24px box with 2px border)
- Border: 2px solid #222
- Padding: 18px 20px
- Margin: 12px bottom between items

**Specifications:**
- All items shown in collapsed state (indicated by [+] icon)
- Border: 2px solid #222 around each item
- Margin bottom: 40px for section

**UX Notes:**
- In functional version, clicking [+] expands to show answer and changes to [-]
- Expanded state would show answer text below question
- Only one FAQ should be open at a time (accordion behavior)
- This wireframe shows visual representation only, not interactive functionality

---

### 6. Quick Links Section
**Purpose:** Provide fast access to common help resources

**Section Title:** "Quick Links"
- Font: Architects Daughter, 24px

**Quick Link Cards (4 cards in a row):**
1. **[Donation Guide]**
2. **[Programme Registration]**
3. **[Donation History]**
4. **[Contact Support]**

**Card Specifications:**
- Grid layout: 4 columns, equal width
- Gap: 20px between cards
- Border: 2px solid #222
- Padding: 30px 20px
- Text: Centered, 14px, bold
- Each card is clickable and navigates to respective resource

**Specifications:**
- Display: CSS Grid with `repeat(4, 1fr)`
- Margin bottom: 40px for section

**UX Notes:**
- Cards act as navigation buttons
- Hover state would add visual feedback in functional version
- Could be adapted to 2×2 grid on smaller screens

---

### 7. Need More Help? Section
**Purpose:** Provide escalation path when users can't find answers

**Content:**
- Heading: "Need More Help?" (24px, Architects Daughter)
- Description text (3 placeholder lines):
  - "Can't find what you're looking for?"
  - "Our support team can help answer your question."
- Call-to-action button: [CREATE CONVERSATION]

**Specifications:**
- Container: 2px dashed border (#555) - visual distinction from other sections
- Padding: 40px
- Text alignment: Center
- Text lines: 2px height, max-width 600px (short lines: 400px)
- Button: Black background (#222), white text, 12px 35px padding
- Margin top on button: 25px

**UX Notes:**
- "Create Conversation" opens a support ticket system
- Users can describe their issue in detail
- Redirects to conversation/chat interface or opens modal
- Dashed border indicates this is a secondary/alternative action path

---

### 8. Footer (Consistent across all pages)
**Four-Column Layout:**

**Column 1: About Care4Kids**
- Heading: "About Care4Kids"
- 3 text placeholder lines

**Column 2: Quick Links**
- Heading: "Quick Links"
- Links: Home, Campaigns, Donate, Programmes, Gallery, About Us, Partners
- Arrow prefix: "→"

**Column 3: Help & Support**
- Heading: "Help & Support"
- Links: Help Centre, FAQ, Contact Us, Raise a Query
- Arrow prefix: "→"

**Column 4: Connect**
- Heading: "Connect"
- Social media icons: f, t, in, ig (Facebook, Twitter, LinkedIn, Instagram)
- Newsletter signup placeholder (2 text lines)

**Footer Bottom:**
- Copyright/legal text placeholder (centered, 400px wide line)

**Specifications:**
- Border: 2px solid #222
- Padding: 30px
- Grid: 4 equal columns, 30px gap
- Social icons: 32px circles with 2px borders
- Border-top for footer bottom: 2px solid #222

---

## Content Requirements

### FAQ Answer Content (for high-fidelity version)

1. **How do I donate?**
   - Click on "Donate" in navigation or any campaign's "DONATE" button
   - Select donation amount or enter custom amount
   - Choose payment method (Bank Transfer, Card, or E-Wallet)
   - Complete payment process
   - Receive confirmation email

2. **How can I view my donation history?**
   - Log in to your account
   - Go to "My Account" > "Donation History"
   - View all past donations with dates, amounts, and campaign details
   - Download receipts for tax purposes

3. **How do I register for a programme?**
   - Navigate to "Programmes" page
   - Browse available programmes
   - Click "REGISTER" on desired programme
   - Fill out registration form
   - Submit and receive confirmation

4. **Can I donate anonymously?**
   - Yes, select "Donate Anonymously" checkbox during donation
   - Your name will not appear in public donor lists
   - You'll still receive confirmation and receipt

5. **How can I contact Care4Kids?**
   - Email: support@care4kids.org
   - Phone: +84 123 456 789
   - Use "Create Conversation" for online support
   - Visit "Contact Us" page for office address

6. **What happens when a campaign is completed?**
   - Campaign is marked as [COMPLETED]
   - Funds are distributed to beneficiaries
   - Update report is published in campaign details
   - Donors receive thank you notification

### Search Functionality
- Search should index: FAQ questions/answers, help articles, common terms
- Suggested queries: donation, payment, refund, registration, receipt, support
- Search results page would show matched FAQs and articles with snippets

---

## User Flows

### Flow 1: Finding Answer via FAQ
1. User lands on Help Centre
2. Scans FAQ section for relevant question
3. Clicks on FAQ item to expand
4. Reads answer
5. Problem resolved

### Flow 2: Searching for Help
1. User lands on Help Centre
2. Types query in search box
3. Clicks [SEARCH]
4. Reviews search results
5. Clicks on relevant result to read full article

### Flow 3: Using Quick Links
1. User lands on Help Centre
2. Recognizes topic in Quick Links section
3. Clicks appropriate quick link card
4. Redirected to detailed guide

### Flow 4: Escalating to Support
1. User can't find answer in FAQ or search
2. Scrolls to "Need More Help?" section
3. Clicks [CREATE CONVERSATION]
4. Fills out support form or starts chat
5. Receives personalized assistance

---

## Responsive Considerations (for high-fidelity)

### Mobile Layout:
- Stack header logo and navigation
- Full-width search input
- FAQ items remain stacked (natural)
- Quick Links: 2×2 grid or stacked vertically
- Footer: Stack columns vertically

### Tablet Layout:
- Maintain horizontal navigation if space permits
- Quick Links: 2×2 grid
- Footer: 2×2 grid

---

## Accessibility Considerations

### Keyboard Navigation:
- Search input accessible via Tab
- FAQ items keyboard-expandable (Enter/Space)
- Quick link cards keyboard-focusable
- Skip to main content link

### Screen Reader:
- Proper heading hierarchy (H1 > H2)
- FAQ items as buttons with aria-expanded states
- Alt text for all icons
- Clear link text (avoid "click here")

### ARIA Labels:
- `aria-label="Search help articles"` on search input
- `aria-expanded="false"` on collapsed FAQ items
- `role="button"` on expandable elements
- `aria-live="polite"` for search results region

---

## Technical Implementation Notes

### HTML Structure:
- Semantic HTML5: `<header>`, `<nav>`, `<main>`, `<section>`, `<footer>`
- FAQ items: `<details>` and `<summary>` elements OR custom accordion
- Form for search with proper labels

### CSS Classes:
- `.header`, `.nav-menu`, `.nav-item.active`
- `.search-section`, `.search-input`, `.search-button`
- `.faq-section`, `.faq-item`, `.faq-question`, `.faq-icon`
- `.quick-links-section`, `.quick-links-grid`, `.quick-link-card`
- `.need-help-section`, `.help-button`
- `.footer`, `.footer-columns`, `.footer-column`

### JavaScript Functionality:
- FAQ accordion toggle
- Search form submission and results display
- Quick link card click tracking
- "Create Conversation" modal/redirect

---

## Design Principles Applied

### 1. Self-Service First
- Prominent search functionality
- Comprehensive FAQ section
- Quick access to common resources

### 2. Clear Escalation Path
- "Need More Help?" section for when self-service isn't enough
- Clear call-to-action to contact support

### 3. Scannability
- Large, readable text
- Clear section headings
- Visual hierarchy with spacing

### 4. Low-Fidelity Aesthetic
- Simple wireframe representation
- Focus on structure and content, not visual polish
- Hand-drawn typography feel

### 5. Consistency
- Matches existing wireframe pages (Campaigns, Donate, Programmes, Gallery, About Us)
- Same header and footer structure
- Same typographic and spacing system

---

## Future Enhancements (post-MVP)

1. **Live Chat Widget**
   - Real-time chat with support agents
   - Chatbot for initial triage

2. **Video Tutorials**
   - Embedded videos for complex processes
   - Step-by-step visual guides

3. **Community Forum**
   - User-to-user help
   - Community-sourced answers

4. **Contextual Help**
   - Help tooltips throughout the site
   - In-page help overlays

5. **Multi-language Support**
   - Vietnamese and English content
   - Language switcher in header

6. **Search Analytics**
   - Track common searches
   - Identify content gaps

---

## Related Pages
- Campaign List (search and filter patterns)
- Donate Page (payment-related FAQs)
- Programmes Page (registration-related FAQs)
- Contact Us Page (alternative support path)

---

## Files
- **Wireframe:** `help-centre-wireframe.html`
- **Documentation:** `HELP_CENTRE_PAGE_DOCUMENTATION.md`

---

**Version:** 1.0  
**Date:** September 4, 2026  
**Status:** Wireframe Complete
