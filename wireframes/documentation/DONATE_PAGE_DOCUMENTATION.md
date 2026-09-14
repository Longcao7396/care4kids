# Care4Kids - Donate Page Wireframe

**Created:** September 4, 2026  
**File:** `donate-page-wireframe.html`  
**Design System:** Matches Campaign List wireframe exactly

---

## 📋 Page Overview

The Donate Page wireframe is designed to match the Campaign List page's visual style perfectly, maintaining consistency across the Care4Kids platform.

---

## 🎨 Visual Style (Matched from Campaign List)

### ✅ Style Elements Preserved:
- **Low-fidelity wireframe aesthetic**
- **Black and white / grayscale only**
- **Hand-drawn sketch-like appearance**
- **Simple rectangular borders (2-3px)**
- **Courier Prime monospace typography**
- **Architects Daughter for headings**
- **X-shaped image placeholders**
- **Horizontal lines for text placeholders**
- **Simple button styles**
- **Academic UX/UI wireframe conventions**

### ❌ NOT Used:
- Gradients
- Real photographs
- Colors or colorful UI
- Glassmorphism
- 3D effects
- Heavy shadows
- Decorative illustrations
- High-fidelity UI elements

---

## 📐 Page Structure

### **HEADER** (Identical to Campaign List)
```
[Care4Kids Logo]  Home | Campaigns | Donate | Programmes | Gallery | About Us | Help Centre    [Login] [Register]
                                      ~~~~~~
                                    (underlined)
```

- Logo placeholder: 140px × 50px with border
- Navigation items with 15px gap
- "Donate" is the active page (underlined)
- Auth buttons on the right

---

### **BREADCRUMB**
```
Home > Donate
```

---

### **PAGE HEADER**
```
Donate
_______________________________________
_______________________________
```

- 32px Architects Daughter font
- Text line placeholders below

---

### **MAIN CONTENT LAYOUT**

**Two-column grid:**
- **Left Column (1/3 width):** Campaign Summary
- **Right Column (2/3 width):** Donation Form

---

## 🎯 Left Column: Campaign Summary

### Selected Campaign Card:
```
┌─────────────────────────────┐
│ SELECTED CAMPAIGN           │
├─────────────────────────────┤
│  ┌─────────────────────┐   │
│  │    IMAGE            │   │
│  │      X              │   │
│  │    X   X            │   │
│  └─────────────────────┘   │
│                             │
│  Cặp sách đến trường       │
│                             │
│  Cause:                     │
│  Education                  │
│                             │
│  Goal:                      │
│  30,000,000 VNĐ            │
│                             │
│  Raised:                    │
│  18,500,000 VNĐ            │
│                             │
│  Progress:                  │
│  ████████░░░░░ 62%         │
└─────────────────────────────┘
```

**Components:**
- Image placeholder with X pattern (180px height)
- Campaign title (16px bold, Architects Daughter)
- Cause, Goal, Raised (12px)
- Progress bar with hatched fill (62%)

---

## 📝 Right Column: Donation Form

### 1. **Donation Amount Section**
```
Donation Amount

┌─────────┐ ┌─────────┐ ┌─────────┐
│ 50,000  │ │ 100,000 │ │ 200,000 │
└─────────┘ └─────────┘ └─────────┘

┌─────────┐ ┌─────────┐ ┌─────────┐
│ 500,000 │ │1,000,000│ │  Other  │
└─────────┘ └─────────┘ └─────────┘

Custom Amount:
┌──────────────────────────────────┐
│ Enter amount in VNĐ              │
└──────────────────────────────────┘
```

**Features:**
- 6 preset amount buttons (3×2 grid)
- Custom amount input field
- All amounts in VNĐ (Vietnamese Dong)

---

### 2. **Donation Type Section**
```
Donation Type

( ) Public Donation
( ) Anonymous Donation
```

**Features:**
- Radio button group
- Two options: Public or Anonymous
- 20px circular radio indicators

---

### 3. **Optional Message Section**
```
Optional Message

┌──────────────────────────────────┐
│                                  │
│ Leave a message of support       │
│ (optional)                       │
│                                  │
└──────────────────────────────────┘
```

**Features:**
- Textarea field (100px height)
- Placeholder text
- Vertically resizable

---

### 4. **Payment Method Section**
```
Payment Method

( ) Credit / Debit Card
( ) Bank Transfer
( ) QR Payment

┌─────────────────────────────────────┐
│ ⚠ THIS IS A DUMMY PAYMENT FOR AN   │
│    ACADEMIC PROJECT                 │
└─────────────────────────────────────┘
```

**Features:**
- Radio button group
- Three payment options
- Academic disclaimer (dashed border)

---

### 5. **Form Actions**
```
                    ┌──────┐  ┌──────────┐
                    │ BACK │  │ CONTINUE │
                    └──────┘  └──────────┘
```

**Features:**
- Two buttons aligned right
- BACK: white background, black border
- CONTINUE: black background, white text (primary)
- 14px padding, 40px horizontal

---

## 📌 UX Notes Section

```
UX NOTE: User must select a campaign before proceeding. Donation 
amount must be greater than 0. Anonymous donation is optional. 
Payment is a dummy academic payment for demonstration purposes only.
```

**Style:**
- Gray left border (3px)
- Light gray background
- 11px italic text
- Distinguished from actual UI

---

## 🦶 Footer (Identical to Campaign List)

**Four-column layout:**

### Column 1: About Care4Kids
- Text line placeholders

### Column 2: Quick Links
- Home
- Campaigns
- Donate
- Programmes
- Gallery
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
- Centered text line placeholder
- Top border separation

---

## 🔄 User Flow

```
Campaign List Page
        ↓
   Click "Donate" on Campaign Card
        ↓
   Donate Page (current)
        ↓
   Select Amount + Type + Payment
        ↓
   Click "CONTINUE"
        ↓
   Payment Confirmation Page
```

**Alternative Entry:**
```
Navigation Menu → Donate
        ↓
   Select Campaign First
        ↓
   Continue with Donation Flow
```

---

## ✅ Design Consistency Checklist

- [x] Same header structure and styling
- [x] Same navigation menu (with Donate active)
- [x] Same typography (Courier Prime + Architects Daughter)
- [x] Same button styles (2px borders)
- [x] Same input field styles (2px borders, 42px height)
- [x] Same border thickness (2px solid, 2px dashed)
- [x] Same spacing system (consistent padding/margins)
- [x] Same footer structure (4 columns)
- [x] Same low-fidelity aesthetic
- [x] Same monochrome color scheme
- [x] Same image placeholder style (X pattern)
- [x] Same text placeholder style (horizontal lines)
- [x] Same UX notes styling (gray border, italic)

---

## 🎓 Academic Disclaimer

This is a **dummy payment wireframe** for an academic project. It demonstrates:
- User interface design principles
- Form layout best practices
- NGO donation flow patterns
- Wireframing techniques

**Not included:**
- Real payment gateway integration
- Actual transaction processing
- Live backend connectivity
- Production security measures

---

## 💡 Implementation Notes

### When building the actual page:

1. **Campaign Selection:**
   - User should select a campaign from the Campaign List page
   - Campaign data should be passed via URL parameters or session
   - Display selected campaign summary in the left column

2. **Amount Validation:**
   - Minimum donation amount: 10,000 VNĐ (suggested)
   - Maximum validation for dummy payment
   - Highlight selected preset button
   - Validate custom amount input

3. **Form Validation:**
   - Donation amount required (> 0)
   - Payment method required
   - Message optional
   - Donation type defaults to "Public"

4. **Payment Flow:**
   - For academic demo: show success message
   - For production: integrate real payment gateway
   - Store donation record in database
   - Send confirmation email

5. **Responsive Design:**
   - Stack columns vertically on mobile
   - Campaign summary becomes top section
   - Form becomes bottom section
   - Maintain button accessibility

---

## 📂 Related Files

- `campaign-list-final.html` - Master design reference
- `donate-page-wireframe.html` - Current file
- `WIREFRAME_UPDATES.md` - Campaign list documentation

---

## 🔍 Visual Elements Breakdown

### Form Elements Used:
- **Input fields:** Text input with 2px border
- **Textarea:** Message box with 2px border
- **Radio buttons:** 20px circles with 2px border
- **Buttons:** Preset amounts (grid), Primary/Secondary actions
- **Progress bar:** Hatched pattern fill (matching campaign cards)

### Layout Components:
- **Grid system:** 1/3 (summary) + 2/3 (form)
- **Section dividers:** 2px dashed lines
- **Spacing:** 15px, 20px, 25px, 30px intervals
- **Container padding:** 20-25px

### Typography Scale:
- **Page title:** 32px (Architects Daughter)
- **Section titles:** 16px (Architects Daughter, bold)
- **Labels:** 13-14px (Courier Prime, bold)
- **Body text:** 12-13px (Courier Prime)
- **Small text:** 11px (disclaimers, notes)

---

## 🎯 Success Criteria

✅ Matches Campaign List visual style exactly  
✅ Low-fidelity wireframe maintained throughout  
✅ All required form sections included  
✅ Clear user flow and hierarchy  
✅ Academic disclaimer prominent  
✅ Consistent header and footer  
✅ Same design system applied  
✅ Professional NGO appearance  
✅ Clean, uncluttered layout  
✅ Accessibility-friendly structure  

---

**Wireframe Status:** ✅ Complete and ready for review  
**Next Steps:** Review with team, gather feedback, proceed to high-fidelity mockup or development
