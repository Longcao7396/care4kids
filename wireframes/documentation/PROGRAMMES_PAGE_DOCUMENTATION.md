# Care4Kids - Programmes List Page Wireframe

**Created:** September 4, 2026  
**File:** `programmes-list-wireframe.html`  
**Design System:** Matches Campaign List and Donate Page wireframes

---

## 📋 Page Overview

The Programmes List page displays volunteer programmes for children's welfare activities. Users can browse, search, filter, and register as volunteers for upcoming programmes.

---

## 🎨 Visual Style (Consistent with Campaign List)

### ✅ Preserved Design Elements:
- **Low-fidelity wireframe aesthetic**
- **Black and white / grayscale only**
- **Hand-drawn sketch appearance**
- **Thin borders (2-3px solid)**
- **Courier Prime monospace typography**
- **Architects Daughter for headings**
- **X-shaped image placeholders**
- **Horizontal text line placeholders**
- **Simple button styles**
- **Academic UX/UI wireframe style**

### ❌ NOT Used:
- Gradients
- Real photographs
- Colors
- Glassmorphism
- 3D effects
- Heavy shadows
- Decorative illustrations
- High-fidelity UI

---

## 📐 Page Structure

### **HEADER** (Identical to Campaign List & Donate Page)
```
[Care4Kids]  Home | Campaigns | Donate | Programmes | Gallery | About Us | Help Centre    [Login] [Register]
                                         ~~~~~~~~~~
                                        (underlined)
```

- Logo: 140px × 50px with 2px border
- Navigation: 15px gap between items
- "Programmes" is active (underlined with 2px border)
- Auth buttons aligned right

---

### **BREADCRUMB**
```
Home > Programmes
```
- 12px font size
- Simple text navigation

---

### **PAGE HEADER**
```
Programmes
_______________________________________
_______________________________
```
- Title: 32px Architects Daughter font
- Text line placeholders below

---

## 🔍 Search & Filter Section

```
┌─────────────────────────────────────────────────┐
│ Search:  [Search programmes...               ] │
│                                                 │
│ Date:  [All Dates ▼]    Status: [All Status ▼]│
│                                                 │
│ [APPLY FILTER]  [RESET]                        │
└─────────────────────────────────────────────────┘
```

### Components:
1. **Search Bar**
   - Input field: 400px max-width
   - Placeholder: "Search programmes..."
   - 2px solid border

2. **Date Filter**
   - Select dropdown: 180px width
   - Options: All Dates, Upcoming, This Month, This Quarter
   - Dropdown indicator: ▼

3. **Status Filter**
   - Select dropdown: 180px width
   - Options: All Status, Open, Full, Closed, Completed
   - Dropdown indicator: ▼

4. **Action Buttons**
   - APPLY FILTER (primary - black background)
   - RESET (secondary - white background)

**Container Style:**
- 2px dashed border (#555)
- 20px padding
- Matches Campaign List filter style

---

## 🎯 Programme Cards Grid

### **Layout:**
- **Grid:** 3 columns × 2 rows
- **Gap:** 25px between cards
- **Card border:** 3px solid #222

---

### **Programme Card Structure:**

```
┌─────────────────────────┐
│  ┌─────────────────┐   │
│  │    IMAGE        │   │
│  │      X          │   │
│  │    X   X        │   │
│  └─────────────────┘   │
│                         │
│ Trao tặng dụng cụ      │
│ học tập                 │
│                         │
│ Date: 15/09/2026       │
│ Location: Hanoi        │
│ Volunteer Slots: 50    │
│ Registered: 37         │
│ Remaining: 13          │
│                         │
│ [OPEN]                 │
│                         │
│ [VIEW DETAILS][REGISTER]│
└─────────────────────────┘
```

### **Card Components:**

1. **Image Placeholder**
   - Height: 180px
   - X pattern (rotated 45° lines)
   - "IMAGE" label top-left
   - 2px border bottom

2. **Programme Title**
   - 16px bold, Architects Daughter
   - 18px padding around content
   - Vietnamese programme names

3. **Programme Information**
   - **Date:** Programme date (DD/MM/YYYY)
   - **Location:** City/province in Vietnam
   - **Volunteer Slots:** Total available slots
   - **Registered:** Currently registered volunteers
   - **Remaining:** Available slots left
   - All info rows: 12px font, label bold

4. **Status Badge**
   - Display format: `[OPEN]`, `[FULL]`
   - 2px border, 11px font, bold
   - Inline-block display

5. **Action Buttons**
   - Two buttons side-by-side (flex layout)
   - VIEW DETAILS (secondary)
   - REGISTER (primary - black background)
   - 10px padding, 12px font

---

## 📚 Programme Examples

### **6 Realistic Children's Welfare Programmes:**

1. **Trao tặng dụng cụ học tập**
   - Date: 15/09/2026
   - Location: Hanoi
   - Slots: 50 | Registered: 37 | Remaining: 13
   - Status: [OPEN]

2. **Ngày hội trẻ em**
   - Date: 22/09/2026
   - Location: Ho Chi Minh City
   - Slots: 100 | Registered: 100 | Remaining: 0
   - Status: [FULL]

3. **Hỗ trợ trẻ em vùng khó khăn**
   - Date: 30/09/2026
   - Location: Da Nang
   - Slots: 60 | Registered: 28 | Remaining: 32
   - Status: [OPEN]

4. **Tặng quà Trung Thu**
   - Date: 10/09/2026
   - Location: Can Tho
   - Slots: 40 | Registered: 40 | Remaining: 0
   - Status: [FULL]

5. **Chương trình bữa ăn cho em**
   - Date: 05/10/2026
   - Location: Hue
   - Slots: 30 | Registered: 15 | Remaining: 15
   - Status: [OPEN]

6. **Đồng hành cùng trẻ đến trường**
   - Date: 12/10/2026
   - Location: Nha Trang
   - Slots: 45 | Registered: 22 | Remaining: 23
   - Status: [OPEN]

---

## 📄 Pagination

```
[ < PREV ]  [ 1 ]  [ 2 ]  [ 3 ]  [ NEXT > ]
              ~~~
            (active)
```

**Style:**
- Centered alignment
- 10px gap between buttons
- Page 1 active (black background, white text)
- All buttons: 2px border, 12px font
- Min-width: 40px for page numbers

---

## 📝 UX Notes

```
UX NOTE: Users can search and filter programmes by date and status. 
Each programme shows volunteer availability. Status options: [OPEN], 
[FULL], [CLOSED], [COMPLETED]. Users must be logged in to register 
as volunteers.
```

**Style:**
- 3px left border (#666)
- Light gray background (#f9f9f9)
- 11px italic font
- Prefixed with "UX NOTE:" in bold

---

## 🦶 Footer (Identical to Campaign List & Donate)

**Four-column layout:**

### Column 1: About Care4Kids
- Text line placeholders

### Column 2: Quick Links
- Home
- Campaigns
- Donate
- Programmes ← Current page
- Gallery
- About Us
- Partners

### Column 3: Help & Support
- Help Centre
- FAQ
- Contact Us
- Raise a Query

### Column 4: Connect
- Social media icons: f, t, in, ig (circular 32px)
- Text placeholders below

**Footer Bottom:**
- 2px top border
- Centered text line (400px width)

---

## 🔄 User Flow

### **Primary Flow:**
```
Navigation → Programmes
        ↓
Browse Programme Cards
        ↓
Filter by Date/Status (optional)
        ↓
View Programme Details
        ↓
Register as Volunteer
```

### **Alternative Flow:**
```
Home → Programmes
     ↓
Search for Specific Programme
     ↓
View Details
     ↓
Register
```

---

## ✅ Design Consistency Checklist

- [x] Same header structure (logo, nav, auth buttons)
- [x] Same navigation menu format
- [x] "Programmes" properly underlined as active
- [x] Same typography system (Courier Prime + Architects Daughter)
- [x] Same button styles (2px borders, same padding)
- [x] Same card structure (3px borders)
- [x] Same image placeholder style (X pattern)
- [x] Same text placeholder style (horizontal lines)
- [x] Same filter section style (2px dashed border)
- [x] Same footer structure (4 columns)
- [x] Same spacing system (consistent margins/padding)
- [x] Same low-fidelity aesthetic
- [x] Same monochrome color scheme
- [x] Same UX notes styling

---

## 📊 Comparison with Other Pages

### **Campaign List Page:**
- Both use 3-column card grid
- Both have search/filter sections
- Both show status badges
- Both use same card image placeholders
- **Difference:** Campaigns show progress bars; Programmes show volunteer slots

### **Donate Page:**
- Both use same header/footer
- Both use same typography
- Both use same button styles
- **Difference:** Donate uses 2-column form layout; Programmes uses 3-column grid

### **Visual Unity:**
All three pages share:
- Identical header
- Identical footer
- Same wireframe style
- Same UI components
- Consistent spacing

---

## 🎓 Programme Types Reference

All programmes are **children's welfare activities:**

1. **Education Support** - School supplies, books, learning materials
2. **Community Events** - Children's festivals, celebration days
3. **Outreach Programs** - Supporting children in remote/difficult areas
4. **Seasonal Gifts** - Mid-Autumn Festival, Tet, holidays
5. **Nutrition Programs** - Meal support, food assistance
6. **Educational Access** - Transportation, school enrollment support

---

## 💡 Implementation Notes

### **When developing the actual system:**

1. **Programme Status Logic:**
   - **[OPEN]** - Remaining slots > 0 and date is future
   - **[FULL]** - Remaining slots = 0
   - **[CLOSED]** - Registration closed but not yet completed
   - **[COMPLETED]** - Programme date has passed

2. **Registration Requirements:**
   - User must be logged in
   - Check available slots before registration
   - Validate age requirements (18+ for volunteers)
   - Send confirmation email after registration

3. **Search & Filter:**
   - Search by programme name, location, or description
   - Filter by date range (upcoming, this month, this quarter)
   - Filter by status (open, full, closed, completed)
   - Combined filters should work together

4. **Volunteer Management:**
   - Track registered volunteers per programme
   - Update remaining slots in real-time
   - Waitlist functionality for full programmes
   - Volunteer check-in system on event day

5. **Responsive Design:**
   - Desktop: 3 columns
   - Tablet: 2 columns
   - Mobile: 1 column
   - Filters stack vertically on mobile

---

## 🎯 Programme Card States

### **Normal State (OPEN):**
- Status badge: `[OPEN]`
- Remaining slots visible and > 0
- REGISTER button active (black background)

### **Full State:**
- Status badge: `[FULL]`
- Remaining slots: 0
- REGISTER button may be disabled or show "WAITLIST"

### **Closed State:**
- Status badge: `[CLOSED]`
- Registration period ended
- REGISTER button disabled or hidden

### **Completed State:**
- Status badge: `[COMPLETED]`
- Programme date passed
- Only VIEW DETAILS available
- Show programme outcomes/photos

---

## 📂 Related Files

- `campaign-list-final.html` - Master design reference
- `donate-page-wireframe.html` - Donate page
- `programmes-list-wireframe.html` - Current file
- `WIREFRAME_UPDATES.md` - Campaign list documentation
- `DONATE_PAGE_DOCUMENTATION.md` - Donate page documentation

---

## 🔍 Visual Elements Breakdown

### **Component Inventory:**

1. **Cards:** 3px border, white background
2. **Image placeholders:** X pattern, 180px height
3. **Text lines:** 2px height, black (#222)
4. **Buttons:** 2px border, various sizes
5. **Input fields:** 2px border, 38px height
6. **Select dropdowns:** 2px border, 38px height, ▼ indicator
7. **Status badges:** 2px border, inline-block
8. **Pagination:** 2px border, 40px min-width
9. **Footer:** 2px border, 4-column grid
10. **UX notes:** 3px left border, gray background

### **Typography Scale:**
- **Page title:** 32px (Architects Daughter)
- **Card titles:** 16px (Architects Daughter, bold)
- **Section titles:** 14px (Architects Daughter)
- **Body text:** 12-13px (Courier Prime)
- **Labels:** 13px (Courier Prime, bold)
- **Small text:** 11px (badges, notes)
- **Tiny text:** 10px (image labels)

### **Spacing System:**
- **Card gap:** 25px
- **Section margins:** 30px
- **Container padding:** 20px
- **Button gaps:** 10px
- **Info row margins:** 8px
- **Filter row margins:** 15px

---

## ✅ Quality Assurance

**Visual Consistency:**
- [x] Matches Campaign List wireframe style
- [x] Matches Donate page wireframe style
- [x] Low-fidelity maintained throughout
- [x] No colors used (black/white/gray only)
- [x] All borders consistent (2-3px)
- [x] All fonts consistent (Courier Prime + Architects Daughter)

**Functional Completeness:**
- [x] Search functionality represented
- [x] Filter functionality represented
- [x] Programme cards show all required info
- [x] Status badges clearly visible
- [x] Volunteer slot tracking shown
- [x] Registration action available
- [x] Pagination implemented

**UX Best Practices:**
- [x] Clear visual hierarchy
- [x] Scannable card layout
- [x] Important info emphasized (remaining slots)
- [x] Clear call-to-action buttons
- [x] Consistent navigation
- [x] Accessible button sizing

---

## 🎨 Design System Summary

### **Care4Kids Wireframe System:**

**Typography:**
- Headings: Architects Daughter
- Body: Courier Prime

**Colors:**
- Primary: #222 (black)
- Secondary: #555 (dark gray)
- Background: #fff (white)
- Accent: #f9f9f9 (light gray)

**Borders:**
- Solid: 2-3px #222
- Dashed: 2px #555

**Spacing:**
- Small: 8-10px
- Medium: 15-20px
- Large: 25-30px

**Components:**
- Cards: 3px border
- Buttons: 2px border
- Inputs: 2px border
- Images: X placeholder pattern

---

**Wireframe Status:** ✅ Complete and consistent with Care4Kids design system  
**Next Steps:** Review, gather feedback, proceed to additional page states (empty, loading, error) if needed
