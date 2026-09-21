import api from './api';
import { API_ENDPOINTS } from '../config';
import { authService } from './authService';

// =====================================================
// AUTH SERVICE — re-export from authService.js
// =====================================================
export { authService, AuthService } from './authService';

// =====================================================
// CAUSES SERVICE
// =====================================================
export const causesService = {
  getAll: async (activeOnly = true, params = {}) => {
    const data = await api.get(API_ENDPOINTS.CAUSES.LIST, {
      params: { activeOnly, ...params }
    });
    return data;
  },
  getTree: async (activeOnly = true) => {
    const data = await api.get(API_ENDPOINTS.CAUSES.TREE, {
      params: { activeOnly }
    });
    return data;
  },
  getSubCauses: async (parentId, activeOnly = true) => {
    const data = await api.get(`/causes/${parentId}/sub-causes`, {
      params: { activeOnly }
    });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.CAUSES.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.CAUSES.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.CAUSES.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.CAUSES.DELETE(id));
    return result;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.CAUSES.STATS);
    return data;
  },
};

// =====================================================
// DONATIONS SERVICE
// =====================================================
export const donationsService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.DONATIONS.LIST, { params });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.DONATIONS.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.DONATIONS.CREATE, data);
    return result;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.DONATIONS.STATS);
    return data;
  },
};

// =====================================================
// CAMPAIGNS SERVICE
// =====================================================
export const campaignsService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGNS.LIST, { params });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGNS.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.CAMPAIGNS.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.CAMPAIGNS.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.CAMPAIGNS.DELETE(id));
    return result;
  },
  getFeatured: async (count = 3) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGNS.FEATURED, { params: { count } });
    return data;
  },
  register: async (id, data) => {
    const result = await api.post(API_ENDPOINTS.CAMPAIGNS.REGISTER(id), data);
    return result;
  },
  getRegistrations: async (id) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGNS.REGISTRATIONS(id));
    return data;
  },
  getMyRegistrations: async () => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGNS.MY_REGISTRATIONS);
    return data;
  },
};

// =====================================================
// ABOUT US MODULE SERVICES
// =====================================================

export const teamService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.TEAM.LIST, { params });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.TEAM.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.TEAM.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.TEAM.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.TEAM.DELETE(id));
    return result;
  },
};

export const achievementsService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.ACHIEVEMENTS.LIST, { params });
    return data;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.ACHIEVEMENTS.STATS);
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.ACHIEVEMENTS.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.ACHIEVEMENTS.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.ACHIEVEMENTS.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.ACHIEVEMENTS.DELETE(id));
    return result;
  },
};

export const careersService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.CAREERS.LIST, { params });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.CAREERS.DETAIL(id));
    return data;
  },
  apply: async (id, data) => {
    const result = await api.post(API_ENDPOINTS.CAREERS.APPLY(id), data);
    return result;
  },
  getApplications: async (id) => {
    const data = await api.get(API_ENDPOINTS.CAREERS.APPLICATIONS(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.CAREERS.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.CAREERS.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.CAREERS.DELETE(id));
    return result;
  },
};

export const supportersService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.SUPPORTERS.LIST, { params });
    return data;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.SUPPORTERS.STATS);
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.SUPPORTERS.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.SUPPORTERS.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.SUPPORTERS.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.SUPPORTERS.DELETE(id));
    return result;
  },
};

export const cmsService = {
  getPages: async (keys) => {
    const params = keys ? { keys: Array.isArray(keys) ? keys.join(',') : keys } : {};
    const data = await api.get(API_ENDPOINTS.CMS.PAGES, { params });
    return data;
  },
  getByKey: async (key) => {
    const data = await api.get(API_ENDPOINTS.CMS.PAGE_BY_KEY(key));
    return data;
  },
  updatePage: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.CMS.UPDATE_PAGE(id), data);
    return result;
  },
};

// =====================================================
// HELP CENTRE + CONTACT SERVICES
// =====================================================

export const faqService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.FAQS.LIST, { params });
    return data;
  },
  getCategories: async () => {
    const data = await api.get(API_ENDPOINTS.FAQS.CATEGORIES);
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.FAQS.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.FAQS.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.FAQS.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.FAQS.DELETE(id));
    return result;
  },
};

export const contactService = {
  submit: async (data) => {
    const result = await api.post(API_ENDPOINTS.CONTACTS.SUBMIT, data);
    return result;
  },
  // Admin-only
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.CONTACTS.LIST, { params });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.CONTACTS.DETAIL(id));
    return data;
  },
  reply: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.CONTACTS.REPLY(id), data);
    return result;
  },
  toggleRead: async (id) => {
    const result = await api.put(API_ENDPOINTS.CONTACTS.READ(id));
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.CONTACTS.DELETE(id));
    return result;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.CONTACTS.STATS);
    return data;
  },
};

export const galleryService = {
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.GALLERY.LIST, { params });
    return data;
  },
  getCategories: async () => {
    const data = await api.get(API_ENDPOINTS.GALLERY.CATEGORIES);
    return data;
  },
  getProgrammes: async () => {
    const data = await api.get(API_ENDPOINTS.GALLERY.PROGRAMMES);
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.GALLERY.DETAIL(id));
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.GALLERY.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.GALLERY.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.GALLERY.DELETE(id));
    return result;
  },
};

// =====================================================
// CAMPAIGN REPORTS SERVICE
// =====================================================
export const campaignReportsService = {
  // Public: only published reports by default
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGN_REPORTS.LIST, { params });
    return data;
  },
  // All reports for a given campaign
  getByCampaign: async (campaignId, publishedOnly = true) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGN_REPORTS.BY_CAMPAIGN(campaignId), {
      params: { publishedOnly }
    });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGN_REPORTS.DETAIL(id));
    return data;
  },
  // Admin-only
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.CAMPAIGN_REPORTS.STATS);
    return data;
  },
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.CAMPAIGN_REPORTS.CREATE, data);
    return result;
  },
  update: async (id, data) => {
    const result = await api.put(API_ENDPOINTS.CAMPAIGN_REPORTS.UPDATE(id), data);
    return result;
  },
  remove: async (id) => {
    const result = await api.delete(API_ENDPOINTS.CAMPAIGN_REPORTS.DELETE(id));
    return result;
  },
};

// =====================================================
// INVITATIONS SERVICE (Invite Friends)
// =====================================================
export const invitationsService = {
  send: async (data) => {
    const result = await api.post(API_ENDPOINTS.INVITATIONS.SEND, data);
    return result;
  },
  getMine: async () => {
    const data = await api.get(API_ENDPOINTS.INVITATIONS.MINE);
    return data;
  },
  // Admin
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.INVITATIONS.LIST, { params });
    return data;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.INVITATIONS.STATS);
    return data;
  },
  cancel: async (id) => {
    const result = await api.post(API_ENDPOINTS.INVITATIONS.CANCEL(id));
    return result;
  },
};

// =====================================================
// CONVERSATIONS SERVICE (Raise Query / user-admin chat)
// =====================================================
export const conversationsService = {
  // User
  create: async (data) => {
    const result = await api.post(API_ENDPOINTS.CONVERSATIONS.CREATE, data);
    return result;
  },
  getMine: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.CONVERSATIONS.MINE, { params });
    return data;
  },
  addMessage: async (id, data) => {
    const result = await api.post(API_ENDPOINTS.CONVERSATIONS.MESSAGES(id), data);
    return result;
  },
  close: async (id) => {
    const result = await api.post(API_ENDPOINTS.CONVERSATIONS.CLOSE(id));
    return result;
  },
  // Admin
  getAll: async (params = {}) => {
    const data = await api.get(API_ENDPOINTS.CONVERSATIONS.LIST, { params });
    return data;
  },
  getById: async (id) => {
    const data = await api.get(API_ENDPOINTS.CONVERSATIONS.DETAIL(id));
    return data;
  },
  getStats: async () => {
    const data = await api.get(API_ENDPOINTS.CONVERSATIONS.STATS);
    return data;
  },
  assign: async (id, data) => {
    const result = await api.post(API_ENDPOINTS.CONVERSATIONS.ASSIGN(id), data);
    return result;
  },
};

// =====================================================
// STATISTICS SERVICE (Homepage & Dashboard)
// =====================================================
export * from './statisticsService';

// =====================================================
// DEFAULT EXPORT
// =====================================================
const services = {
  auth: authService,
  causes: causesService,
  donations: donationsService,
  campaigns: campaignsService,
  team: teamService,
  achievements: achievementsService,
  careers: careersService,
  supporters: supportersService,
  cms: cmsService,
  faq: faqService,
  contact: contactService,
  gallery: galleryService,
  campaignReports: campaignReportsService,
  invitations: invitationsService,
  conversations: conversationsService,
};

export default services;
