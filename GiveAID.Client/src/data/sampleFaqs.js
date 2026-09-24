// Sample FAQs used as a fallback for the Help Centre page when the
// /api/v1/faqs endpoint returns an empty list or is unreachable. Mirrors
// the backend FAQ DTO shape (PascalCase fields are normalized to camelCase
// by the axios interceptor in services/api.js, so consumers can read either).
//
// We deliberately over-provision (8 FAQs across 4 categories vs 6 in the
// backend seed) so the sample surface is richer than the live data — that
// way if the backend is unreachable users still see a useful catalogue.

export const SAMPLE_FAQS = [
  {
    faqId: 1,
    question: 'How do I make a donation?',
    answer:
      '<p>You can donate in three ways:</p><ol><li>Browse our <strong>campaigns</strong> page and pick a cause you care about.</li><li>Click <em>Donate</em> to enter the secure checkout form.</li><li>Pay via credit card, debit card, or bank transfer.</em></li></ol><p>A receipt is sent to your email within a few minutes.</p>',
    category: 'Donations',
    displayOrder: 1,
    isActive: true,
    isFeatured: true,
    viewCount: 0
  },
  {
    faqId: 2,
    question: 'Is my donation tax-deductible?',
    answer:
      '<p>Yes. Donations to registered charitable organisations in Vietnam may qualify for tax relief. Please consult your tax advisor for personal advice — we will provide a full donation certificate on request.</p>',
    category: 'Donations',
    displayOrder: 2,
    isActive: true,
    isFeatured: true,
    viewCount: 0
  },
  {
    faqId: 3,
    question: 'How is my donation used?',
    answer:
      '<p>100% of your donation is allocated to the programme you chose. Our operations and overhead are funded separately by partner grants, so every dollar you give goes directly to children, families and communities in need. We publish a detailed financial report every quarter.</p>',
    category: 'Donations',
    displayOrder: 3,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 4,
    question: 'Can I cancel or refund a recurring donation?',
    answer:
      '<p>Yes, you can cancel a recurring donation at any time from your profile under <em>My Donations</em>. For a one-time donation that has already been processed, please contact us within 14 days and we will evaluate the request case-by-case.</p>',
    category: 'Donations',
    displayOrder: 4,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 5,
    question: 'How do I volunteer with you?',
    answer:
      '<p>Register on our platform and complete the volunteer profile. Browse the <strong>programmes</strong> page for volunteer opportunities — sign up for the ones that match your skills and schedule. Our team will reach out within 48 hours to confirm next steps.</p>',
    category: 'Volunteering',
    displayOrder: 5,
    isActive: true,
    isFeatured: true,
    viewCount: 0
  },
  {
    faqId: 6,
    question: 'Are there any volunteer requirements?',
    answer:
      '<p>Most programmes accept volunteers from age 16 and up. Younger volunteers must be accompanied by a parent or guardian. Some specialised roles (medical, legal) require proof of qualification. A basic Vietnamese conversation level is helpful but not mandatory for every role.</p>',
    category: 'Volunteering',
    displayOrder: 6,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 7,
    question: 'How do I register for a programme event?',
    answer:
      '<p>Open the campaign you are interested in and click <em>Register to Participate</em>. You may need to log in first. Free events confirm instantly; paid events redirect to the donation checkout. You can manage all your registrations under <em>My Registrations</em>.</p>',
    category: 'Programmes',
    displayOrder: 7,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 8,
    question: 'How can my company partner with you?',
    answer:
      '<p>We welcome corporate partnerships — whether through financial sponsorship, in-kind donation, employee volunteering, or cause-related marketing. Please email <strong>partnerships@care4kids.org</strong> with a brief about your company and we will set up an introductory call.</p>',
    category: 'Partnerships',
    displayOrder: 8,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 9,
    question: 'How do I contact support if I have an issue with my donation?',
    answer:
      '<p>Our support team is ready to help with any donation-related questions. You can reach us in three ways:</p><ul><li>Use the <em>Contact Us</em> form on our website and select <strong>Donations</strong> as the subject category.</li><li>Email <strong>support@care4kids.org</strong> with your donation reference number (found in your confirmation email).</li><li>Send us a direct message from your dashboard under <em>My Donations → Need help?</em>.</li></ul><p>We aim to respond within 1 business day. For urgent issues involving an in-progress payment, please include the transaction timestamp so we can trace it quickly.</p>',
    category: 'Donations',
    displayOrder: 9,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 10,
    question: 'Can I donate anonymously?',
    answer:
      '<p>Yes. When you reach the checkout form, toggle <strong>Display my name publicly on the campaign page</strong> off. Your contribution will still appear in our internal records and financial reports as required by Vietnamese law for registered non-profits, but your name will be shown as <em>Anonymous</em> on the public donor wall.</p>',
    category: 'Donations',
    displayOrder: 10,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 11,
    question: 'What payment methods do you accept?',
    answer:
      '<p>We support the following payment methods on the donation checkout:</p><ul><li>Major <strong>credit cards</strong> (Visa, Mastercard, JCB, Amex).</li><li><strong>Debit cards</strong> issued by Vietnamese banks that support online payments.</li><li>Domestic <strong>bank transfer</strong> (QR code provided at checkout).</li><li>Selected <strong>e-wallets</strong> such as MoMo, ZaloPay and VNPay.</li></ul><p>All transactions are processed through a PCI-DSS compliant payment gateway; we never store your card details on our servers.</p>',
    category: 'Donations',
    displayOrder: 11,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 12,
    question: 'Do you provide a donation receipt?',
    answer:
      '<p>Yes. An automatic <strong>donation receipt</strong> is emailed to you within a few minutes after a successful payment. The receipt includes the donation amount, date, campaign, and your donation reference number.</p><p>If you need an official donation certificate for tax or employer matching purposes, please reply to the receipt email or contact <strong>support@care4kids.org</strong> with your reference number and we will issue one within 2 business days.</p>',
    category: 'Donations',
    displayOrder: 12,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  },
  {
    faqId: 13,
    question: 'How do I check the status of my volunteer application?',
    answer:
      '<p>You can track your volunteer application from your dashboard:</p><ol><li>Log in and open <strong>My Registrations</strong> from the side menu.</li><li>Look for entries under the <em>Volunteer</em> tab — each row shows the current status: <strong>Pending review</strong>, <strong>Approved</strong>, <strong>Waitlisted</strong>, or <strong>Declined</strong>.</li><li>Click any row for full details, including the programme coordinator&rsquo;s contact and any next-step actions.</li></ol><p>If you applied before creating an account, or you cannot find your application, email <strong>volunteer@care4kids.org</strong> with the email address used on the application form and we will help you locate it.</p>',
    category: 'Volunteering',
    displayOrder: 13,
    isActive: true,
    isFeatured: false,
    viewCount: 0
  }
];
