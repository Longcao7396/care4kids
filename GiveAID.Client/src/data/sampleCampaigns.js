// Sample campaigns for demo / fallback when API is empty or unavailable.
// All images are sourced from Cloudinary (giveaid/replacement/...).
// Campaign names and descriptions are written in English to match the
// public-facing site copy.

export const SAMPLE_CAMPAIGNS = [
  {
    campaignId: 'sample-001',
    campaignName: 'Clean Water for Every Child',
    description:
      'Reliable access to clean drinking water for 2,500 children in drought-prone districts. Each project installs boreholes, rainwater tanks and household filters, with training on safe water practices for parents and teachers.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337418/giveaid/replacement/thieunhi9-2869-1401513005_chlpkd.webp',
    goalAmount: 800000000,
    raisedAmount: 564000000,
    percentageReached: 71,
    donorCount: 2145,
    beneficiariesCount: 2500,
    daysRemaining: 58,
    location: 'Quang Tri, Binh Thuan',
    status: 'Active',
    isFeatured: true,
    cause: { causeId: 'c1', causeName: 'Clean Water' },
  },
  {
    campaignId: 'sample-002',
    campaignName: '1,500 Back-to-School Kits for Highland Children',
    description:
      'School bags, books, notebooks, stationery and one year of student insurance for 1,500 children in remote northern provinces. Delivered with the Vietnam Red Cross and provincial Departments of Education before the new school year.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337381/giveaid/replacement/thang3-4930-1396341890_cyjvnr.webp',
    goalAmount: 900000000,
    raisedAmount: 612000000,
    percentageReached: 68,
    donorCount: 3120,
    beneficiariesCount: 1500,
    daysRemaining: 45,
    location: 'Lao Cai, Son La',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c2', causeName: 'Education' },
  },
  {
    campaignId: 'sample-003',
    campaignName: '30 Mobile Health Clinics for Children',
    description:
      'Thirty mobile health clinics serve 3,000 children in remote districts of Nghe An, Quang Nam and Ben Tre. Each visit includes a general check-up, free medicine, deworming and nutrition counselling, delivered by volunteer paediatricians from the National Children’s Hospital.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337383/giveaid/replacement/7-2_sblz8z.jpg',
    goalAmount: 450000000,
    raisedAmount: 387500000,
    percentageReached: 86,
    donorCount: 890,
    beneficiariesCount: 3000,
    daysRemaining: 12,
    location: 'Nghe An, Quang Nam',
    status: 'Active',
    isFeatured: true,
    cause: { causeId: 'c3', causeName: 'Healthcare' },
  },
  {
    campaignId: 'sample-004',
    campaignName: 'Three New Care Homes for Orphaned Children',
    description:
      'Build and operate three new long-term care homes in Hanoi, Da Nang and Can Tho. Each home houses fifty orphaned children with bedrooms, classrooms, playgrounds and a vegetable garden. Operations are sustained through monthly sponsor commitments.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337386/giveaid/replacement/10-2_compw6.jpg',
    goalAmount: 2500000000,
    raisedAmount: 1750000000,
    percentageReached: 70,
    donorCount: 4521,
    beneficiariesCount: 150,
    daysRemaining: 60,
    location: 'Hanoi, Da Nang, Can Tho',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c4', causeName: 'Care Homes' },
  },
  {
    campaignId: 'sample-005',
    campaignName: 'Central Vietnam Flood Relief 2026',
    description:
      'Emergency aid for 10,000 households affected by central-Vietnam floods: food, clean water, medicine and cash grants to rebuild livelihoods. Coordinated with the Vietnam Fatherland Front and local authorities for last-mile delivery.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337388/giveaid/replacement/11_itbi3m.jpg',
    goalAmount: 3000000000,
    raisedAmount: 2380000000,
    percentageReached: 79,
    donorCount: 6723,
    beneficiariesCount: 10000,
    daysRemaining: 8,
    location: 'Quang Binh, Ha Tinh',
    status: 'Active',
    isFeatured: true,
    cause: { causeId: 'c5', causeName: 'Relief' },
  },
  {
    campaignId: 'sample-006',
    campaignName: 'Free English Classes for Children near Industrial Zones',
    description:
      'Twenty free weekend English classes for 600 children near industrial zones, taught by international and Vietnamese volunteers using the Cambridge Young Learners curriculum. Each six-month course ends with a certificate and scholarship for outstanding students.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337381/giveaid/replacement/thang3-4930-1396341890_cyjvnr.webp',
    goalAmount: 600000000,
    raisedAmount: 312000000,
    percentageReached: 52,
    donorCount: 1980,
    beneficiariesCount: 600,
    daysRemaining: 90,
    location: 'Binh Duong, Long An',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c2', causeName: 'Education' },
  },
  {
    campaignId: 'sample-007',
    campaignName: '50 Free Heart Surgeries for Children',
    description:
      'Fund fifty free paediatric heart surgeries for underprivileged children born with congenital heart disease. Each surgery costs 80–150 million VND and is performed at Hospital E and the National Heart Institute, in partnership with the Heart for Children Fund.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337396/giveaid/replacement/13-1_gz91bw.jpg',
    goalAmount: 5000000000,
    raisedAmount: 4150000000,
    percentageReached: 83,
    donorCount: 5230,
    beneficiariesCount: 50,
    daysRemaining: 30,
    location: 'Hanoi',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c3', causeName: 'Healthcare' },
  },
  {
    campaignId: 'sample-008',
    campaignName: '25 Safe Playgrounds for Rural Children',
    description:
      'Build twenty-five safe community playgrounds in remote communes, including slides, swings, seesaws, a small soccer area and an outdoor reading space. Each playground gives children a safe place to play, stay active and develop social skills.',
    imageUrl:
      'https://res.cloudinary.com/mczqcagv/image/upload/v1790337398/giveaid/replacement/14-1_fwygad.jpg',
    goalAmount: 850000000,
    raisedAmount: 510000000,
    percentageReached: 60,
    donorCount: 2340,
    beneficiariesCount: 5000,
    daysRemaining: 50,
    location: 'Tuyen Quang, Bac Kan',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c6', causeName: 'Children' },
  },
];

export const SAMPLE_CAUSES = [
  { causeId: 'c1', causeName: 'Daily Meals' },
  { causeId: 'c2', causeName: 'Education' },
  { causeId: 'c3', causeName: 'Healthcare' },
  { causeId: 'c4', causeName: 'Care Homes' },
  { causeId: 'c5', causeName: 'Relief' },
  { causeId: 'c6', causeName: 'Children' },
];
