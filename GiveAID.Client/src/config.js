// API Base URL Configuration
// Candidate backend URLs to try, in order of preference.
// Backend IIS Express may listen on 44300 (configured) or 61508 (VS default).
const BACKEND_CANDIDATES = [
  'http://localhost:44300/api',
  'http://localhost:61508/api',
];

// Synchronous initial value (best guess) — actual fallback happens in api.js
export const API_BASE_URL =
  process.env.REACT_APP_API_URL || BACKEND_CANDIDATES[0];

export { BACKEND_CANDIDATES };

// API Endpoints
export const API_ENDPOINTS = {
  // Auth
  AUTH: {
    REGISTER: `${API_BASE_URL}/auth/register`,
    LOGIN: `${API_BASE_URL}/auth/login`,
    LOGOUT: `${API_BASE_URL}/auth/logout`,
    ME: `${API_BASE_URL}/auth/me`,
  },
  
  // Causes
  CAUSES: {
    LIST: `${API_BASE_URL}/causes`,
    DETAIL: (id) => `${API_BASE_URL}/causes/${id}`,
    CREATE: `${API_BASE_URL}/causes`,
    UPDATE: (id) => `${API_BASE_URL}/causes/${id}`,
    DELETE: (id) => `${API_BASE_URL}/causes/${id}`,
    STATS: `${API_BASE_URL}/causes/stats`,
  },
  
  // Donations
  DONATIONS: {
    LIST: `${API_BASE_URL}/donations`,
    DETAIL: (id) => `${API_BASE_URL}/donations/${id}`,
    CREATE: `${API_BASE_URL}/donations`,
    STATS: `${API_BASE_URL}/donations/stats`,
  },
  
  // Programmes / Campaigns — unified.
  // The backend merged Programme concept into Campaign; registration-style
  // events are surfaced via RegistrationRequired = true.
  CAMPAIGNS: {
    LIST: `${API_BASE_URL}/campaigns`,
    DETAIL: (id) => `${API_BASE_URL}/campaigns/${id}`,
    CREATE: `${API_BASE_URL}/campaigns`,
    UPDATE: (id) => `${API_BASE_URL}/campaigns/${id}`,
    DELETE: (id) => `${API_BASE_URL}/campaigns/${id}`,
    FEATURED: `${API_BASE_URL}/campaigns/featured`,
    REGISTER: (id) => `${API_BASE_URL}/campaigns/${id}/register`,
    REGISTRATIONS: (id) => `${API_BASE_URL}/campaigns/${id}/registrations`,
    MY_REGISTRATIONS: `${API_BASE_URL}/campaigns/my-registrations`,
  },

  // Legacy alias kept so older imports don't break.
  PROGRAMMES: {
    LIST: `${API_BASE_URL}/campaigns?eventsOnly=true`,
    DETAIL: (id) => `${API_BASE_URL}/campaigns/${id}`,
    REGISTER: (id) => `${API_BASE_URL}/campaigns/${id}/register`,
    MY_REGISTRATIONS: `${API_BASE_URL}/campaigns/my-registrations`,
  },

  // About Us - Team
  TEAM: {
    LIST: `${API_BASE_URL}/team`,
    DETAIL: (id) => `${API_BASE_URL}/team/${id}`,
    CREATE: `${API_BASE_URL}/team`,
    UPDATE: (id) => `${API_BASE_URL}/team/${id}`,
    DELETE: (id) => `${API_BASE_URL}/team/${id}`,
  },

  // About Us - Achievements
  ACHIEVEMENTS: {
    LIST: `${API_BASE_URL}/achievements`,
    STATS: `${API_BASE_URL}/achievements/stats`,
    DETAIL: (id) => `${API_BASE_URL}/achievements/${id}`,
    CREATE: `${API_BASE_URL}/achievements`,
    UPDATE: (id) => `${API_BASE_URL}/achievements/${id}`,
    DELETE: (id) => `${API_BASE_URL}/achievements/${id}`,
  },

  // About Us - Careers
  CAREERS: {
    LIST: `${API_BASE_URL}/careers`,
    DETAIL: (id) => `${API_BASE_URL}/careers/${id}`,
    APPLY: (id) => `${API_BASE_URL}/careers/${id}/apply`,
    APPLICATIONS: (id) => `${API_BASE_URL}/careers/${id}/applications`,
    CREATE: `${API_BASE_URL}/careers`,
    UPDATE: (id) => `${API_BASE_URL}/careers/${id}`,
    DELETE: (id) => `${API_BASE_URL}/careers/${id}`,
  },

  // About Us - Supporters (uses Organizations table)
  SUPPORTERS: {
    LIST: `${API_BASE_URL}/supporters`,
    STATS: `${API_BASE_URL}/supporters/stats`,
    DETAIL: (id) => `${API_BASE_URL}/supporters/${id}`,
    CREATE: `${API_BASE_URL}/supporters`,
    UPDATE: (id) => `${API_BASE_URL}/supporters/${id}`,
    DELETE: (id) => `${API_BASE_URL}/supporters/${id}`,
  },

  // About Us - editable page text (CMS)
  CMS: {
    PAGES: `${API_BASE_URL}/cms/pages`,
    PAGE_BY_KEY: (key) => `${API_BASE_URL}/cms/pages/${key}`,
    UPDATE_PAGE: (id) => `${API_BASE_URL}/cms/pages/${id}`,
  },

  // Help Centre — FAQs
  FAQS: {
    LIST: `${API_BASE_URL}/faqs`,
    CATEGORIES: `${API_BASE_URL}/faqs/categories`,
    DETAIL: (id) => `${API_BASE_URL}/faqs/${id}`,
    CREATE: `${API_BASE_URL}/faqs`,
    UPDATE: (id) => `${API_BASE_URL}/faqs/${id}`,
    DELETE: (id) => `${API_BASE_URL}/faqs/${id}`,
  },

  // Contact form submissions
  CONTACTS: {
    SUBMIT: `${API_BASE_URL}/contacts`,
    LIST: `${API_BASE_URL}/contacts`,
    DETAIL: (id) => `${API_BASE_URL}/contacts/${id}`,
    REPLY: (id) => `${API_BASE_URL}/contacts/${id}/reply`,
    READ: (id) => `${API_BASE_URL}/contacts/${id}/read`,
    DELETE: (id) => `${API_BASE_URL}/contacts/${id}`,
    STATS: `${API_BASE_URL}/contacts/stats`,
  },

  // Invite Friends (referrals)
  INVITATIONS: {
    SEND: `${API_BASE_URL}/invitations`,
    MINE: `${API_BASE_URL}/invitations/mine`,
    LIST: `${API_BASE_URL}/invitations`,
    STATS: `${API_BASE_URL}/invitations/stats`,
    CANCEL: (id) => `${API_BASE_URL}/invitations/${id}/cancel`,
    ACCEPT: (token) => `${API_BASE_URL}/invitations/accept/${token}`,
  },

  // User-to-admin conversations (Raise Query)
  CONVERSATIONS: {
    CREATE: `${API_BASE_URL}/conversations`,
    MINE: `${API_BASE_URL}/conversations/mine`,
    LIST: `${API_BASE_URL}/conversations`,
    STATS: `${API_BASE_URL}/conversations/stats`,
    DETAIL: (id) => `${API_BASE_URL}/conversations/${id}`,
    MESSAGES: (id) => `${API_BASE_URL}/conversations/${id}/messages`,
    CLOSE: (id) => `${API_BASE_URL}/conversations/${id}/close`,
    ASSIGN: (id) => `${API_BASE_URL}/conversations/${id}/assign`,
  },

  // Gallery
  GALLERY: {
    LIST: `${API_BASE_URL}/gallery`,
    CATEGORIES: `${API_BASE_URL}/gallery/categories`,
    PROGRAMMES: `${API_BASE_URL}/gallery/programmes`,
    DETAIL: (id) => `${API_BASE_URL}/gallery/${id}`,
    CREATE: `${API_BASE_URL}/gallery`,
    UPDATE: (id) => `${API_BASE_URL}/gallery/${id}`,
    DELETE: (id) => `${API_BASE_URL}/gallery/${id}`,
  },
};

// App Settings
export const APP_NAME = 'Care4Kids';
export const APP_DESCRIPTION = 'Children\'s Welfare & Donation Management System';

// Pagination
export const DEFAULT_PAGE_SIZE = 10;

// Local Storage Keys
export const STORAGE_KEYS = {
  TOKEN: 'giveaid_token',
  USER: 'giveaid_user',
};

// User Roles
export const USER_ROLES = {
  SUPER_ADMIN: 'SuperAdmin',
  ADMIN: 'Admin',
  CONTENT_MANAGER: 'ContentManager',
  USER: 'User',
};

// Payment Methods
export const PAYMENT_METHODS = {
  CREDIT_CARD: 'CreditCard',
  DEBIT_CARD: 'DebitCard',
  NET_BANKING: 'NetBanking',
};

// Programme Types
export const PROGRAMME_TYPES = {
  EDUCATION: 'Education',
  HEALTH_CARE: 'HealthCare',
  CHILD_WELFARE: 'ChildWelfare',
  WOMEN_EMPOWERMENT: 'WomenEmpowerment',
};

// Programme Status
export const PROGRAMME_STATUS = {
  UPCOMING: 'Upcoming',
  ONGOING: 'Ongoing',
  COMPLETED: 'Completed',
  CANCELLED: 'Cancelled',
};

// Cause Codes
export const CAUSE_CODES = {
  CHILD: 'CHILD',
  EDU: 'EDU',
  DIS: 'DIS',
  WOMAN: 'WOMAN',
  YOUTH: 'YOUTH',
  ELDER: 'ELDER',
};
