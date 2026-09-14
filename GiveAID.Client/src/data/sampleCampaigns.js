// Sample campaigns for demo / fallback when API is empty or unavailable.
// All images are sourced from Unsplash and represent Vietnamese children
// in real contexts: classroom, meals, healthcare, shelter, disaster relief.
// When a specific scene is hard to source, we use a simple representative
// image (book, food bowl, water pump, stethoscope) that still reads clearly.

export const SAMPLE_CAMPAIGNS = [
  {
    campaignId: 'sample-001',
    campaignName: 'Bữa Cơm Có Thịt — Meals for Saigon Children',
    description:
      'Cung cấp hơn 5.000 bữa cơm có thịt cho trẻ em nghèo tại các mái ấm và trung tâm bảo trợ ở TP. Hồ Chí Minh. Mỗi bữa ăn đảm bảo đủ dinh dưỡng với cơm, thịt, rau xanh và sữa. Chương trình hợp tác cùng Hội Từ thiện Thành phố và các mái ấm nuôi dạy trẻ mồ côi.',
    imageUrl:
      'https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=900&q=80',
    goalAmount: 120000000,
    raisedAmount: 87450000,
    percentageReached: 73,
    donorCount: 1245,
    beneficiariesCount: 850,
    daysRemaining: 21,
    location: 'TP. Hồ Chí Minh',
    status: 'Active',
    isFeatured: true,
    cause: { causeId: 'c1', causeName: 'Bữa Cơm' },
  },
  {
    campaignId: 'sample-002',
    campaignName: 'Sách Vở Cho Em Đến Trường — Back-to-School Kits',
    description:
      'Trao tặng 1.500 bộ balo, sách vở, vở ô ly, bút màu và đồ dùng học tập cho trẻ em vùng cao Tây Bắc trước thềm năm học mới. Mỗi bộ kit trị giá 600.000đ và đi kèm bảo hiểm học sinh một năm. Hợp tác cùng Hội Chữ thập đỏ Việt Nam và Sở Giáo dục các tỉnh.',
    imageUrl:
      'https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=900&q=80',
    goalAmount: 900000000,
    raisedAmount: 612000000,
    percentageReached: 68,
    donorCount: 3120,
    beneficiariesCount: 1500,
    daysRemaining: 45,
    location: 'Lào Cai, Sơn La',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c2', causeName: 'Giáo Dục' },
  },
  {
    campaignId: 'sample-003',
    campaignName: 'Khám Sức Khỏe Miễn Phí Cho Trẻ Em Vùng Sâu',
    description:
      'Tổ chức 30 chuyến khám sức khỏe lưu động cho 3.000 trẻ em tại các huyện nghèo của tỉnh Nghệ An, Quảng Nam và Bến Tre. Mỗi chuyến khám bao gồm khám tổng quát, cấp thuốc miễn phí, tẩy giun và tư vấn dinh dưỡng. Đội ngũ y bác sĩ tình nguyện từ Bệnh viện Nhi Trung ương.',
    imageUrl:
      'https://images.unsplash.com/photo-1576091160550-2173dba999ef?auto=format&fit=crop&w=900&q=80',
    goalAmount: 450000000,
    raisedAmount: 387500000,
    percentageReached: 86,
    donorCount: 890,
    beneficiariesCount: 3000,
    daysRemaining: 12,
    location: 'Nghệ An, Quảng Nam',
    status: 'Active',
    isFeatured: true,
    cause: { causeId: 'c3', causeName: 'Y Tế' },
  },
  {
    campaignId: 'sample-004',
    campaignName: 'Mái Ấm Tình Thương — Shelter for Orphans',
    description:
      'Xây dựng và vận hành 3 mái ấm mới cho trẻ mồ côi tại Hà Nội, Đà Nẵng và Cần Thơ. Mỗi mái ấm có sức chứa 50 em với phòng ngủ, phòng học, sân chơi và vườn rau sạch. Hoạt động duy trì bền vững nhờ hỗ trợ nhà hảo tâm hàng tháng.',
    imageUrl:
      'https://images.unsplash.com/photo-1542810634-71277d95dcbb?auto=format&fit=crop&w=900&q=80',
    goalAmount: 2500000000,
    raisedAmount: 1750000000,
    percentageReached: 70,
    donorCount: 4521,
    beneficiariesCount: 150,
    daysRemaining: 60,
    location: 'Hà Nội, Đà Nẵng, Cần Thơ',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c4', causeName: 'Mái Ấm' },
  },
  {
    campaignId: 'sample-005',
    campaignName: 'Cứu Trợ Lũ Lụt Miền Trung — Flood Relief 2026',
    description:
      'Hỗ trợ khẩn cấp 10.000 hộ gia đình bị ảnh hưởng bởi bão lũ tại miền Trung với nhu yếu phẩm, nước sạch, thuốc men và tiền mặt để tái thiết cuộc sống. Hợp tác chặt chẽ với Mặt trận Tổ quốc Việt Nam và chính quyền địa phương để vận chuyển cứu trợ nhanh chóng.',
    imageUrl:
      'https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=900&q=80',
    goalAmount: 3000000000,
    raisedAmount: 2380000000,
    percentageReached: 79,
    donorCount: 6723,
    beneficiariesCount: 10000,
    daysRemaining: 8,
    location: 'Quảng Bình, Hà Tĩnh',
    status: 'Active',
    isFeatured: true,
    cause: { causeId: 'c5', causeName: 'Cứu Trợ' },
  },
  {
    campaignId: 'sample-006',
    campaignName: 'Lớp Học Hy Vọng — Free English Classes',
    description:
      'Mở 20 lớp học tiếng Anh miễn phí cho 600 trẻ em tại các khu công nghiệp và vùng nông thôn nghèo. Giáo trình Cambridge Young Learners, giáo viên tình nguyện quốc tế và Việt Nam. Mỗi khóa học 6 tháng kèm cấp chứng chỉ và học bổng cho học sinh xuất sắc.',
    imageUrl:
      'https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=900&q=80',
    goalAmount: 600000000,
    raisedAmount: 312000000,
    percentageReached: 52,
    donorCount: 1980,
    beneficiariesCount: 600,
    daysRemaining: 90,
    location: 'Bình Dương, Long An',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c2', causeName: 'Giáo Dục' },
  },
  {
    campaignId: 'sample-007',
    campaignName: 'Mổ Tim Miễn Phí Cho Trẻ Em — Heart Surgery Fund',
    description:
      'Tài trợ 50 ca phẫu thuật tim miễn phí cho trẻ em mắc bệnh tim bẩm sinh từ các gia đình khó khăn. Mỗi ca phẫu thuật trị giá từ 80-150 triệu đồng, thực hiện tại Bệnh viện E và Viện Tim mạch Quốc gia. Chương trình hợp tác với Quỹ Vì Trái Tim.',
    imageUrl:
      'https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=900&q=80',
    goalAmount: 5000000000,
    raisedAmount: 4150000000,
    percentageReached: 83,
    donorCount: 5230,
    beneficiariesCount: 50,
    daysRemaining: 30,
    location: 'Hà Nội',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c3', causeName: 'Y Tế' },
  },
  {
    campaignId: 'sample-008',
    campaignName: 'Sân Chơi Cho Trẻ Em Nông Thôn — Rural Playgrounds',
    description:
      'Xây dựng 25 sân chơi cộng đồng an toàn cho trẻ em tại các xã vùng sâu vùng xa. Mỗi sân chơi bao gồm cầu trượt, xích đu, bập bênh, sân bóng và khu vực đọc sách ngoài trời. Giúp trẻ em có không gian vui chơi, rèn luyện sức khỏe và phát triển kỹ năng xã hội.',
    imageUrl:
      'https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?auto=format&fit=crop&w=900&q=80',
    goalAmount: 850000000,
    raisedAmount: 510000000,
    percentageReached: 60,
    donorCount: 2340,
    beneficiariesCount: 5000,
    daysRemaining: 50,
    location: 'Tuyên Quang, Bắc Kạn',
    status: 'Active',
    isFeatured: false,
    cause: { causeId: 'c6', causeName: 'Trẻ Em' },
  },
];

export const SAMPLE_CAUSES = [
  { causeId: 'c1', causeName: 'Bữa Cơm' },
  { causeId: 'c2', causeName: 'Giáo Dục' },
  { causeId: 'c3', causeName: 'Y Tế' },
  { causeId: 'c4', causeName: 'Mái Ấm' },
  { causeId: 'c5', causeName: 'Cứu Trợ' },
  { causeId: 'c6', causeName: 'Trẻ Em' },
];
