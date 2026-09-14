import api from './api';
import { API_ENDPOINTS, STORAGE_KEYS } from '../config';

// =====================================================
// AUTH SERVICE
// =====================================================
export const authService = {
  register: async (userData) => {
    const response = await api.post(API_ENDPOINTS.AUTH.REGISTER, userData);
    return response.data;
  },
  login: async (credentials) => {
    const response = await api.post(API_ENDPOINTS.AUTH.LOGIN, credentials);
    if (response.data.success && response.data.data) {
      localStorage.setItem(STORAGE_KEYS.TOKEN, response.data.data.token);
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(response.data.data.user));
    }
    return response.data;
  },
  logout: async () => {
    try {
      await api.post(API_ENDPOINTS.AUTH.LOGOUT);
    } finally {
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
    }
  },
  getCurrentUser: async () => {
    const response = await api.get(API_ENDPOINTS.AUTH.ME);
    return response.data;
  },
  isAuthenticated: () => !!localStorage.getItem(STORAGE_KEYS.TOKEN),
  getStoredUser: () => {
    const userStr = localStorage.getItem(STORAGE_KEYS.USER);
    return userStr ? JSON.parse(userStr) : null;
  },
};

// =====================================================
// CAUSES SERVICE
// =====================================================
export const causesService = {
  getAll: async (activeOnly = true) => {
    const response = await api.get(API_ENDPOINTS.CAUSES.LIST, { params: { activeOnly } });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.CAUSES.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.CAUSES.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.CAUSES.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.CAUSES.DELETE(id));
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.CAUSES.STATS);
    return response.data;
  },
};

// =====================================================
// DONATIONS SERVICE
// =====================================================
export const donationsService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.DONATIONS.LIST, { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.DONATIONS.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.DONATIONS.CREATE, data);
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.DONATIONS.STATS);
    return response.data;
  },
};

// =====================================================
// PROGRAMMES / CAMPAIGNS SERVICE
// =====================================================
// Programmes were merged into Campaigns in the backend.
// All campaign-level reads (including the registration endpoints that
// used to live under /api/programmes) now go through campaignsService.
// programmesService is kept as a thin alias for backward compatibility
// with any code that hasn't been migrated yet.
export const campaignsService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.LIST, { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.CAMPAIGNS.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.CAMPAIGNS.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.CAMPAIGNS.DELETE(id));
    return response.data;
  },
  getFeatured: async (count = 3) => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.FEATURED, { params: { count } });
    return response.data;
  },
  // Unified registration endpoint (replaces /api/programmes/{id}/register)
  register: async (id, data) => {
    const response = await api.post(API_ENDPOINTS.CAMPAIGNS.REGISTER(id), data);
    return response.data;
  },
  getRegistrations: async (id) => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.REGISTRATIONS(id));
    return response.data;
  },
  getMyRegistrations: async () => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.MY_REGISTRATIONS);
    return response.data;
  },
};

// Thin alias — Programmes were merged into Campaigns. We default the
// `eventsOnly=true` filter so callers still get the same UX (events only).
export const programmesService = {
  getAll: async (params = {}) => {
    const merged = { eventsOnly: true, ...params };
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.LIST, { params: merged });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.DETAIL(id));
    return response.data;
  },
  register: async (id, data) => {
    const response = await api.post(API_ENDPOINTS.CAMPAIGNS.REGISTER(id), data);
    return response.data;
  },
  getMyRegistrations: async () => {
    const response = await api.get(API_ENDPOINTS.CAMPAIGNS.MY_REGISTRATIONS);
    return response.data;
  },
};

// =====================================================
// ABOUT US MODULE SERVICES
// =====================================================

export const teamService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.TEAM.LIST, { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.TEAM.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.TEAM.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.TEAM.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.TEAM.DELETE(id));
    return response.data;
  },
};

export const achievementsService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.ACHIEVEMENTS.LIST, { params });
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.ACHIEVEMENTS.STATS);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.ACHIEVEMENTS.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.ACHIEVEMENTS.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.ACHIEVEMENTS.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.ACHIEVEMENTS.DELETE(id));
    return response.data;
  },
};

export const careersService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.CAREERS.LIST, { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.CAREERS.DETAIL(id));
    return response.data;
  },
  apply: async (id, data) => {
    const response = await api.post(API_ENDPOINTS.CAREERS.APPLY(id), data);
    return response.data;
  },
  getApplications: async (id) => {
    const response = await api.get(API_ENDPOINTS.CAREERS.APPLICATIONS(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.CAREERS.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.CAREERS.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.CAREERS.DELETE(id));
    return response.data;
  },
};

export const supportersService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.SUPPORTERS.LIST, { params });
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.SUPPORTERS.STATS);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.SUPPORTERS.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.SUPPORTERS.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.SUPPORTERS.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.SUPPORTERS.DELETE(id));
    return response.data;
  },
};

export const cmsService = {
  getPages: async (keys) => {
    const params = keys ? { keys: Array.isArray(keys) ? keys.join(',') : keys } : {};
    const response = await api.get(API_ENDPOINTS.CMS.PAGES, { params });
    return response.data;
  },
  getByKey: async (key) => {
    const response = await api.get(API_ENDPOINTS.CMS.PAGE_BY_KEY(key));
    return response.data;
  },
  updatePage: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.CMS.UPDATE_PAGE(id), data);
    return response.data;
  },
};

// =====================================================
// HELP CENTRE + CONTACT SERVICES
// =====================================================

export const faqService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.FAQS.LIST, { params });
    return response.data;
  },
  getCategories: async () => {
    const response = await api.get(API_ENDPOINTS.FAQS.CATEGORIES);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.FAQS.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.FAQS.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.FAQS.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.FAQS.DELETE(id));
    return response.data;
  },
};

export const contactService = {
  submit: async (data) => {
    const response = await api.post(API_ENDPOINTS.CONTACTS.SUBMIT, data);
    return response.data;
  },
  // Admin-only
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.CONTACTS.LIST, { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.CONTACTS.DETAIL(id));
    return response.data;
  },
  reply: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.CONTACTS.REPLY(id), data);
    return response.data;
  },
  toggleRead: async (id) => {
    const response = await api.put(API_ENDPOINTS.CONTACTS.READ(id));
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.CONTACTS.DELETE(id));
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.CONTACTS.STATS);
    return response.data;
  },
};

export const galleryService = {
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.GALLERY.LIST, { params });
    return response.data;
  },
  getCategories: async () => {
    const response = await api.get(API_ENDPOINTS.GALLERY.CATEGORIES);
    return response.data;
  },
  getProgrammes: async () => {
    const response = await api.get(API_ENDPOINTS.GALLERY.PROGRAMMES);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.GALLERY.DETAIL(id));
    return response.data;
  },
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.GALLERY.CREATE, data);
    return response.data;
  },
  update: async (id, data) => {
    const response = await api.put(API_ENDPOINTS.GALLERY.UPDATE(id), data);
    return response.data;
  },
  remove: async (id) => {
    const response = await api.delete(API_ENDPOINTS.GALLERY.DELETE(id));
    return response.data;
  },
};

// =====================================================
// INVITATIONS SERVICE  (Invite Friends)
// =====================================================
export const invitationsService = {
  send: async (data) => {
    const response = await api.post(API_ENDPOINTS.INVITATIONS.SEND, data);
    return response.data;
  },
  getMine: async () => {
    const response = await api.get(API_ENDPOINTS.INVITATIONS.MINE);
    return response.data;
  },
  // Admin
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.INVITATIONS.LIST, { params });
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.INVITATIONS.STATS);
    return response.data;
  },
  cancel: async (id) => {
    const response = await api.post(API_ENDPOINTS.INVITATIONS.CANCEL(id));
    return response.data;
  },
};

// =====================================================
// CONVERSATIONS SERVICE  (Raise Query / user-admin chat)
// =====================================================
export const conversationsService = {
  // User
  create: async (data) => {
    const response = await api.post(API_ENDPOINTS.CONVERSATIONS.CREATE, data);
    return response.data;
  },
  getMine: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.CONVERSATIONS.MINE, { params });
    return response.data;
  },
  addMessage: async (id, data) => {
    const response = await api.post(API_ENDPOINTS.CONVERSATIONS.MESSAGES(id), data);
    return response.data;
  },
  close: async (id) => {
    const response = await api.post(API_ENDPOINTS.CONVERSATIONS.CLOSE(id));
    return response.data;
  },
  // Admin
  getAll: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.CONVERSATIONS.LIST, { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(API_ENDPOINTS.CONVERSATIONS.DETAIL(id));
    return response.data;
  },
  getStats: async () => {
    const response = await api.get(API_ENDPOINTS.CONVERSATIONS.STATS);
    return response.data;
  },
  assign: async (id, data) => {
    const response = await api.post(API_ENDPOINTS.CONVERSATIONS.ASSIGN(id), data);
    return response.data;
  },
};

// =====================================================
// DEFAULT EXPORT
// =====================================================
const services = {
  auth: authService,
  causes: causesService,
  donations: donationsService,
  campaigns: campaignsService,
  programmes: programmesService,
  team: teamService,
  achievements: achievementsService,
  careers: careersService,
  supporters: supportersService,
  cms: cmsService,
  faq: faqService,
  contact: contactService,
  gallery: galleryService,
  invitations: invitationsService,
  conversations: conversationsService,
};

export default services;
