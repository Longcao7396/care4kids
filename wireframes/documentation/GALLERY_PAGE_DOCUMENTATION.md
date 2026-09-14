# Care4Kids - Gallery Page Wireframe

**Created:** September 4, 2026  
**File:** `gallery-wireframe.html`  
**Design System:** Matches Campaign List, Donate, and Programmes wireframes

---

## 📋 Page Overview

The Gallery page displays photos and images from Care4Kids programmes and events. Users can view visual documentation of the organization's activities and impact.

---

## 🎨 Visual Style (Consistent Across All Pages)

### ✅ Preserved Design Elements:
- **Low-fidelity wireframe aesthetic**
- **Black and white / grayscale only**
- **Hand-drawn sketch appearance**
- **Simple rectangular borders (2px)**
- **Courier Prime monospace typography**
- **Architects Daughter for headings**
- **X-shaped image placeholders**
- **Horizontal line text placeholders**
- **Simple button styles**
- **Academic UX/UI wireframe style**

### ❌ NOT Used:
- Real photographs
- Colors or gradients
- High-fidelity UI
- 3D effects
- Glassmorphism
- Heavy shadows
- Decorative graphics

---

## 📐 Page Structure

### **HEADER** (Identical to All Other Pages)
```
[Care4Kids]  Home | Campaigns | Donate | Programmes | Gallery | About Us | Help Centre    [Login] [Register]
                                                      ~~~~~~~
                                                    (underlined)
```

- Logo: 140px × 50px with 2px border
- Navigation: 15px gap
- "Gallery" is active (underlined)
- Auth buttons aligned right

---

### **BREADCRUMB**
```
Home > Gallery
```

---

### **PAGE HEADER**
```
Gallery
_______________________________________
_______________________________
```

- Title: 32px Architects Daughter font
- Text line placeholders

---

## 🔍 Filter Section

```
┌──────────────────────────────────┐
│ Programme: [All Programmes ▼]   │
│                                  │
│ Date:      [All Dates ▼]        │
│                                  │
│ [APPLY FILTER]                   │
└──────────────────────────────────┘
```

### Components:

1. **Programme Filter**
   - Select dropdown: 200px width
   - Options: All Programmes, Education, Food, Healthcare, Community Events
   - Dropdown indicator: ▼

2. **Date Filter**
   - Select dropdown: 200px width
   - Options: All Dates, This Month, Last Month, This Year
   - Dropdown indicator: ▼

3. **Apply Button**
   - Primary style (black background)
   - 2px border

**Container Style:**
- 2px dashed border (#555)
- 20px padding
- Consistent with other pages

---

## 🖼️ Gallery Grid Layout

### **Grid Configuration:**
- **Columns:** 4 columns
- **Gap:** 20px between items
- **Total Items:** 8 gallery items displayed

---

### **Gallery Item Structure:**

```
┌─────────────────────┐
│  ┌───────────────┐  │
│  │    IMAGE      │  │
│  │      X        │  │
│  │    X   X      │  │
│  └───────────────┘  │
├─────────────────────┤
│ Trao tặng dụng cụ  │
│ học tập             │
│                     │
│ 15/09/2026         │
│                     │
│ ___________________│
│ ___________________│
│ ______________     │
└─────────────────────┘
```

### **Item Components:**

1. **Image Placeholder**
   - Height: 200px
   - X pattern (rotated 45° diagonal lines)
   - "IMAGE" label top-left corner
   - 2px border bottom separator

2. **Caption Section** (15px padding)
   - **Title:** 13px bold, Architects Daughter
   - **Date:** 11px, gray color (#555)
   - **Description:** 3 horizontal lines (1px height)
     - 2 full-width lines
     - 1 short line (75% width)

---

## 📚 Gallery Content

### **8 Programme Gallery Items:**

1. **Trao tặng dụng cụ học tập**
   - Date: 15/09/2026
   - Description lines (placeholder)

2. **Ngày hội trẻ em**
   - Date: 22/09/2026
   - Description lines (placeholder)

3. **Hỗ trợ trẻ em vùng khó khăn**
   - Date: 30/09/2026
   - Description lines (placeholder)

4. **Tặng quà Trung Thu**
   - Date: 10/09/2026
   - Description lines (placeholder)

5. **Hoạt động cộng đồng**
   - Date: 18/08/2026
   - Description lines (placeholder)

6. **Đồng hành cùng trẻ đến trường**
   - Date: 12/10/2026
   - Description lines (placeholder)

7. **Chương trình bữa ăn cho em**
   - Date: 05/10/2026
   - Description lines (placeholder)

8. **Hỗ trợ trẻ em thiếu may mắn**
   - Date: 25/08/2026
   - Description lines (placeholder)

---

## 📄 Pagination

```
[ < PREV ]  [ 1 ]  [ 2 ]  [ 3 ]  [ NEXT > ]
              ═══
           (active)
```

- Centered at page bottom
- Page 1 active (black background, white text)
- 10px gap between buttons
- 2px borders on all buttons

---

## 📝 UX Notes

```
UX NOTE: Gallery displays photos from various Care4Kids programmes 
and events. Users can filter by programme type and date. Each image 
shows programme name, date, and brief description. Clicking an image 
opens a lightbox view with full details.
```

**Style:**
- 3px left border (#666)
- Light gray background (#f9f9f9)
- 11px italic font
- "UX NOTE:" prefix in bold

---

## 🦶 Footer (Identical to All Pages)

**Four-column layout:**

### Column 1: About Care4Kids
- Text line placeholders

### Column 2: Quick Links
- Home
- Campaigns
- Donate
- Programmes
- Gallery ← Current page
- About Us
- Partners

### Column 3: Help & Support
- Help Centre
- FAQ
- Contact Us
- Raise a Query

### Column 4: Connect
- Social media icons (f, t, in, ig)
- Text placeholders

**Footer Bottom:**
- 2px top border
- Centered text line (400px width)

---

## 🔄 User Flow

### **Primary Path:**
```
Navigation → Gallery
        ↓
Browse Gallery Grid (8 items)
        ↓
Filter by Programme/Date (optional)
        ↓
Click Image
        ↓
View Lightbox with Full Image & Details
```

### **Filter Path:**
```
Gallery Page
        ↓
Select Programme Filter
        ↓
Select Date Range
        ↓
Click "APPLY FILTER"
        ↓
View Filtered Results
```

---

## ✅ Design Consistency Checklist

- [x] Same header (logo, nav, auth buttons)
- [x] Same navigation menu format
- [x] "Gallery" properly underlined as active
- [x] Same typography (Courier Prime + Architects Daughter)
- [x] Same button styles (2px borders)
- [x] Same filter section style (2px dashed border)
- [x] Same image placeholder (X pattern)
- [x] Same text placeholder (horizontal lines)
- [x] Same pagination style
- [x] Same footer (4 columns)
- [x] Same spacing system
- [x] Same low-fidelity aesthetic
- [x] Same monochrome scheme

---

## 📊 Comparison with Other Pages

### **All 4 Pages Share:**

| Element | Campaign | Donate | Programmes | Gallery |
|---------|----------|--------|------------|---------|
| Header | ✓ Same | ✓ Same | ✓ Same | ✓ Same |
| Navigation | ✓ Same | ✓ Same | ✓ Same | ✓ Same |
| Footer | ✓ Same | ✓ Same | ✓ Same | ✓ Same |
| Typography | ✓ Same | ✓ Same | ✓ Same | ✓ Same |
| Borders | ✓ Same | ✓ Same | ✓ Same | ✓ Same |
| Buttons | ✓ Same | ✓ Same | ✓ Same | ✓ Same |
| Image X | ✓ Yes | ✓ Yes | ✓ Yes | ✓ Yes |
| Low-fi | ✓ Yes | ✓ Yes | ✓ Yes | ✓ Yes |

---

## 🎯 Gallery-Specific Features

### **What Makes Gallery Different:**

**Campaign List:**
- 3-column grid
- Shows progress bars
- Donation buttons

**Donate:**
- 2-column form layout
- Payment options
- Donation amounts

**Programmes:**
- 3-column grid
- Volunteer slots
- Registration buttons

**Gallery:**
- **4-column grid** (more compact)
- **Pure visual display** (no actions)
- **Date-based organization**
- **No buttons on items** (click entire card)
- **Lighter caption section** (no heavy info)

---

## 💡 Implementation Notes

### **When developing the actual system:**

1. **Image Management:**
   - Store images in CDN or cloud storage
   - Implement lazy loading for performance
   - Support multiple image formats (JPEG, PNG, WebP)
   - Generate thumbnails automatically

2. **Lightbox Functionality:**
   - Click image to open full-screen view
   - Navigate between images with arrows
   - Show full programme details
   - ESC key to close
   - Support touch gestures on mobile

3. **Filter Logic:**
   - Filter by programme category
   - Filter by date range
   - Combine filters (AND logic)
   - Update URL with filter parameters
   - Preserve filters on pagination

4. **Gallery Organization:**
   - Sort by date (newest first)
   - Group by programme
   - Tag system for better discovery
   - Search functionality

5. **Responsive Design:**
   - Desktop: 4 columns
   - Tablet: 3 columns
   - Mobile: 2 columns or 1 column
   - Maintain aspect ratios

6. **Accessibility:**
   - Alt text for all images
   - Keyboard navigation in lightbox
   - Focus management
   - Screen reader announcements

---

## 🎨 Gallery Item Dimensions

### **Image Area:**
- Width: 100% of container
- Height: 200px (fixed)
- X pattern: 2px lines, 45° rotation
- Border bottom: 2px solid #222

### **Caption Area:**
- Padding: 15px all sides
- Title: 13px bold
- Date: 11px gray
- Description lines: 1px height, 3px spacing

### **Total Item:**
- Border: 2px solid #222
- Background: white
- Hover: None (keep simple for wireframe)

---

## 📐 Grid Calculations

### **4-Column Layout:**
- Container width: ~1360px (max-width minus padding)
- Column width: ~320px per column
- Gap: 20px × 3 = 60px total gap
- Math: (1360 - 60) / 4 = 325px per column

### **Responsive Breakpoints (for implementation):**
- Desktop (>1200px): 4 columns
- Tablet (768-1199px): 3 columns
- Mobile (480-767px): 2 columns
- Small mobile (<480px): 1 column

---

## 🔍 Filter Options (for implementation)

### **Programme Filter:**
- All Programmes
- Education Support
- Community Events
- Food & Nutrition
- Healthcare
- Seasonal Celebrations
- Outreach Activities

### **Date Filter:**
- All Dates
- This Month
- Last Month
- Last 3 Months
- This Year
- Custom Range

---

## 📂 Related Files

```
C:\Users\admin\Desktop\project NGO\wireframes\
├── campaign-list-final.html           (Master reference)
├── donate-page-wireframe.html         (Donation flow)
├── programmes-list-wireframe.html     (Volunteer programmes)
├── gallery-wireframe.html             (NEW - Photo gallery)
├── WIREFRAME_UPDATES.md               (Campaign docs)
├── DONATE_PAGE_DOCUMENTATION.md       (Donate docs)
└── PROGRAMMES_PAGE_DOCUMENTATION.md   (Programmes docs)
```

---

## 🎨 Care4Kids Design System Summary

### **Established Patterns (All 4 Pages):**

1. **Header:** Logo + 7 nav items + 2 auth buttons
2. **Breadcrumb:** Simple text navigation
3. **Page Title:** 32px Architects Daughter + text lines
4. **Filters:** 2px dashed border container
5. **Content Grid:** 3 or 4 columns, 20-25px gap
6. **Image Placeholder:** X pattern, "IMAGE" label
7. **Buttons:** 2px border, primary = black bg
8. **Pagination:** Centered, page 1 active
9. **Footer:** 4 columns, social icons, links
10. **UX Notes:** Left border, gray background, italic

---

## 🎯 Gallery vs. Other Content Types

### **Content Density:**

**Campaign Cards:** Heavy information
- Goal, raised, progress, donors, time, buttons
- Requires more vertical space

**Programme Cards:** Moderate information
- Date, location, slots, status, buttons
- Balanced vertical space

**Gallery Items:** Light information
- Title, date, brief description only
- Minimal vertical space
- Focus on visual content

### **Grid Choice:**

- **Campaigns:** 3 columns (need space for data)
- **Programmes:** 3 columns (need space for info)
- **Gallery:** 4 columns (compact, visual-first)

---

## ✅ Quality Assurance

**Visual Consistency:**
- [x] Matches all other wireframe pages
- [x] Low-fidelity maintained
- [x] Black and white only
- [x] X pattern for all images
- [x] Same borders (2px)
- [x] Same fonts
- [x] Same spacing

**Functional Completeness:**
- [x] Filter by programme
- [x] Filter by date
- [x] 8 gallery items displayed
- [x] Pagination implemented
- [x] UX notes included
- [x] All programme names realistic

**User Experience:**
- [x] Clear visual hierarchy
- [x] Scannable grid layout
- [x] Minimal cognitive load
- [x] Filter accessibility
- [x] Consistent navigation

---

## 🎓 Gallery Purpose

The Gallery page serves to:

1. **Document Impact:** Show visual evidence of programmes
2. **Build Trust:** Transparency through photos
3. **Engage Donors:** Emotional connection through visuals
4. **Celebrate Success:** Highlight completed activities
5. **Recruit Volunteers:** Show the work environment
6. **Share Stories:** Visual storytelling of children's welfare work

---

**Wireframe Status:** ✅ Complete and consistent with Care4Kids design system  
**Page Count:** 4 pages completed (Campaign List, Donate, Programmes, Gallery)  
**Next Steps:** Review all 4 pages together, ensure consistency, proceed to additional pages if needed
