# About Us Page - Low-Fidelity Wireframe Documentation

**File**: `about-us-wireframe.html`  
**Created**: September 4, 2026  
**Page Type**: Informational / About Us  
**Design System**: Care4Kids Low-Fidelity Wireframe System

---

## Overview

The About Us page provides information about the Care4Kids organization, its mission, impact, services, and partners. This is a simple informational page designed to build trust and transparency with donors, volunteers, and beneficiaries.

---

## Page Structure

### 1. **Header** (Consistent across all pages)
- **Logo**: Care4Kids logo placeholder (140px × 50px)
- **Navigation Menu**: Home | Campaigns | Donate | Programmes | Gallery | **About Us** (active/underlined) | Help Centre
- **Auth Buttons**: Login | Register
- **Styling**: 2px solid black border, Courier Prime font

### 2. **Breadcrumb Navigation**
```
Home > About Us
```
- Font size: 12px
- Helps users understand their location in the site hierarchy

### 3. **Page Title**
```
About Care4Kids
```
- Font: Architects Daughter (hand-drawn style)
- Font size: 32px
- No description lines (title only)

---

## Main Content Sections

### **SECTION 1: About Care4Kids** (Two-Column Layout)

**Left Column: Hero Image**
- Large image placeholder with X pattern
- Dimensions: Full width × 400px height
- Border: 2px solid black
- Label: `[IMAGE]` in top-left corner

**Right Column: Organization Overview**
- **Heading**: "About Care4Kids" (24px, Architects Daughter)
- **Description**: Short paragraph explaining Care4Kids as a children's welfare and donation management platform
- **Mission**: Heading + 3 text lines
- **Vision**: Heading + 3 text lines  
- **Core Values**: Heading + 4 text lines

**Purpose**: Provides core identity and values of the organization

---

### **SECTION 2: Our Impact** (Statistics Grid)

**Layout**: 4 statistic boxes in a single row

**Stat Boxes**:
1. **Children Supported**: 5,000+
2. **Campaigns Completed**: 250+
3. **Total Donations**: $500K+
4. **Volunteers**: 1,200+

**Styling**:
- Border: 3px solid black
- Text alignment: Center
- Number font: Architects Daughter, 36px, bold
- Label font: Courier Prime, 13px, bold
- Padding: 30px vertical, 20px horizontal

**Purpose**: Showcases organizational impact through key metrics

---

### **SECTION 3: What We Do** (Services Grid)

**Layout**: 4 service cards in a single row

**Service Cards**:
1. **Education** (icon placeholder + description)
2. **Food** (icon placeholder + description)
3. **Healthcare** (icon placeholder + description)
4. **School Supplies** (icon placeholder + description)

**Card Structure**:
- Icon placeholder: 60px × 60px, centered, `[ICON]` label
- Title: 16px, Architects Daughter, centered
- Description: 3 horizontal text lines

**Styling**:
- Border: 2px solid black
- Padding: 20px
- Background: White

**Purpose**: Explains the four main program areas Care4Kids supports

---

### **SECTION 4: Our Partners & NGOs** (Partner Grid)

**Layout**: 4 partner cards in a single row

**Partner Card Structure**:
- Logo placeholder: Full width × 80px height, X pattern, `[LOGO]` label
- Partner name: 14px, Architects Daughter, centered
- Description: 2 text lines

**Styling**:
- Border: 2px solid black
- Padding: 20px
- Text alignment: Center

**Purpose**: Highlights organizational partnerships and NGO collaborations to build credibility

---

### **SECTION 5: Get Involved** (Call-to-Action)

**Layout**: Centered content with dashed border

**Content**:
- Text: "Help support children in difficult circumstances."
- Two buttons side-by-side:
  - `[DONATE]` (primary button - black background, white text)
  - `[CONTACT US]` (secondary button - white background, black border)

**Styling**:
- Border: 2px dashed black
- Padding: 40px
- Button size: Large (12px vertical, 24px horizontal padding)
- Button font: 14px, bold

**Purpose**: Encourages visitor engagement through donation or contact

---

### 6. **Footer** (Consistent across all pages)

**Layout**: 4-column grid

**Column 1: About Care4Kids**
- Heading + 3 text lines (organization description)

**Column 2: Quick Links**
- Home, Campaigns, Donate, Programmes, Gallery, About Us, Partners
- Arrow prefix (→) for each link

**Column 3: Help & Support**
- Help Centre, FAQ, Contact Us, Raise a Query
- Arrow prefix (→) for each link

**Column 4: Connect**
- Social media icons: f (Facebook), t (Twitter), in (LinkedIn), ig (Instagram)
- Each icon: 35px × 35px, 2px border
- Additional text lines below for contact info

**Footer Bottom**:
- Centered horizontal text line (copyright/legal placeholder)
- Width: 400px, centered

---

## Design System Specifications

### Typography
- **Headings**: Architects Daughter (hand-drawn, cursive)
- **Body text**: Courier Prime (monospace)
- **Size hierarchy**:
  - Page title: 32px
  - Section titles: 24px
  - Subsection titles: 16px
  - Stat numbers: 36px
  - Body text: 13px
  - Footer links: 12px
  - Breadcrumb: 12px

### Layout & Spacing
- **Max width**: 1440px (centered)
- **Page padding**: 40px
- **Section spacing**: 50px bottom margin
- **Grid gaps**: 20-40px depending on section
- **Card padding**: 20px (standard), 30px vertical (stat boxes)

### Visual Elements
- **Borders**: 2-3px solid #222 (black)
- **Dashed borders**: 2px dashed #222 (for CTA section)
- **Image placeholders**: X pattern using CSS pseudo-elements (::before, ::after)
- **Text placeholders**: Horizontal lines (2px height, #222 background)
- **Background**: White (#fff) on light gray page (#fafafa)

### Buttons
- **Border**: 2px solid black
- **Padding**: 8px × 16px (standard), 12px × 24px (large)
- **Primary button**: Black background, white text
- **Secondary button**: White background, black border
- **Font size**: 12px (standard), 14px (large)

---

## Wireframe Characteristics

### Low-Fidelity Features
✓ Black and white only (no colors)  
✓ Hand-drawn typography (Architects Daughter)  
✓ Simple geometric shapes  
✓ X pattern for image placeholders  
✓ Horizontal lines for text content  
✓ No photography or illustrations  
✓ No gradients or shadows  
✓ Minimal decoration  

### NOT Included (High-Fidelity Only)
✗ Brand colors  
✗ Actual images/photos  
✗ Real text content  
✗ Hover states  
✗ Animations  
✗ Glassmorphism  
✗ 3D effects  
✗ Heavy shadows  

---

## Page Purpose

The About Us page serves to:

1. **Build Trust**: Provide transparency about the organization's mission and values
2. **Demonstrate Impact**: Showcase tangible results through statistics
3. **Explain Services**: Clarify what programs Care4Kids supports
4. **Highlight Partnerships**: Build credibility through partner relationships
5. **Drive Action**: Encourage donations and contact through clear CTAs

---

## User Journey

**Typical visitor flow**:
1. User clicks "About Us" in main navigation
2. Reads organizational overview and values
3. Views impact statistics to understand scale
4. Reviews service areas to understand focus
5. Sees partner organizations for credibility
6. Takes action via Donate or Contact Us buttons

---

## Content Placeholders

### Text Line Representations
- **Horizontal lines (text-line)**: Represent paragraph text or descriptions
- **Short lines (70% width)**: Represent shorter text or last line of paragraph
- **Medium lines (85% width)**: Represent medium-length text

### Image/Icon Placeholders
- **[IMAGE]**: Large content images (hero, feature images)
- **[LOGO]**: Partner/organization logos
- **[ICON]**: Service/feature icons (60px × 60px)

### Numeric Placeholders
- Statistics use realistic example numbers: 5,000+, 250+, $500K+, 1,200+
- Partner names use generic "Partner Name" label

---

## Consistency with Other Pages

### Shared Elements (Identical across all wireframes)
- Header structure (logo, navigation, auth buttons)
- Footer structure (4-column layout)
- Typography system (Architects Daughter + Courier Prime)
- Border weights (2-3px solid black)
- Button styles
- Breadcrumb format
- Image placeholder X pattern

### Page-Specific Elements (Unique to About Us)
- Two-column About section with large hero image
- 4-column statistics grid
- Service cards with icon placeholders
- Partner cards with logo placeholders
- Centered CTA section with dashed border

---

## Technical Implementation

### HTML Structure
```
<!DOCTYPE html>
<html>
  <head>
    - Google Fonts (Architects Daughter, Courier Prime)
    - Embedded CSS styles
  </head>
  <body>
    - Wireframe page container
    - Header
    - Breadcrumb
    - Page header
    - About section (2-column grid)
    - Impact section (4-column stats)
    - Services section (4-column grid)
    - Partners section (4-column grid)
    - CTA section (centered)
    - Footer (4-column grid + bottom)
  </body>
</html>
```

### CSS Grid Usage
- **About section**: `grid-template-columns: 1fr 1fr` (2 equal columns)
- **Stats, services, partners**: `grid-template-columns: repeat(4, 1fr)` (4 equal columns)
- **Footer**: `grid-template-columns: repeat(4, 1fr)` (4 equal columns)
- **Grid gaps**: 20px (cards), 30px (footer), 40px (about section)

### Responsive Considerations (Not implemented in wireframe)
This is a desktop-only wireframe. Responsive behavior would need:
- Mobile: Stack columns vertically
- Tablet: 2-column grids for stats/services/partners
- Breakpoints: 768px (tablet), 1024px (desktop)

---

## File Information

**Location**: `C:\Users\admin\Desktop\project NGO\wireframes\about-us-wireframe.html`  
**Size**: ~625 lines of HTML + CSS  
**Dependencies**: Google Fonts (Architects Daughter, Courier Prime)  
**Browser Compatibility**: All modern browsers (Chrome, Firefox, Safari, Edge)

---

## Related Wireframes

- `campaign-list-final.html` - Campaign listing page
- `donate-page-wireframe.html` - Donation flow page
- `programmes-list-wireframe.html` - Programmes listing page
- `gallery-wireframe.html` - Gallery/media page

All wireframes share the same design system and visual language.

---

## Notes for Development Team

1. **Content Population**: Replace all text lines with actual mission/vision/values content
2. **Images**: Replace X placeholders with real organizational photos
3. **Statistics**: Connect to database for real-time impact metrics
4. **Partners**: Fetch partner data from database, including logos and descriptions
5. **CTAs**: Link DONATE button to donation page, CONTACT US to contact form
6. **Icons**: Replace [ICON] placeholders with appropriate service icons (education, food, healthcare, school supplies)
7. **Responsive Design**: Implement mobile and tablet layouts with appropriate breakpoints
8. **Accessibility**: Add proper alt text, ARIA labels, semantic HTML when converting to high-fidelity

---

**End of Documentation**
