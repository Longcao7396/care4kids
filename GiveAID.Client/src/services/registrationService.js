import api from './api';
import { API_BASE_URL } from '../config';

/* ─────────────────────────────────────────────────
 * registrationService
 *   Admin-side API client for the /api/v1/admin/registrations endpoints.
 *   Both campaign and programme registrations are handled here because
 *   they share the same approval workflow.
 * ───────────────────────────────────────────────── */

const BASE = `${API_BASE_URL}/admin/registrations`;

/**
 * @typedef {Object} AdminRegistration
 * @property {number} registrationId
 * @property {'Campaign'|'Programme'} registrationType
 * @property {number} campaignId
 * @property {string|null} campaignName
 * @property {number} programmeId
 * @property {string|null} programmeName
 * @property {number} userId
 * @property {string|null} userName
 * @property {string|null} userEmail
 * @property {string} status           'Registered' | 'Approved' | 'Rejected' | 'Cancelled'
 * @property {string|null} notes
 * @property {boolean} attendanceConfirmed
 * @property {string}  registrationDate
 * @property {string|null} reviewedAt
 * @property {string|null} reviewedBy
 * @property {string|null} rejectionReason
 */

export const registrationService = {
  /**
   * List every registration (campaign + programme) with filters.
   * @param {{ status?: string, type?: string, search?: string, page?: number, pageSize?: number }} params
   */
  getAll: async (params = {}) => {
    return await api.get(BASE, { params });
  },

  /** Aggregate counts for the dashboard chips. */
  getStats: async () => {
    return await api.get(`${BASE}/stats`);
  },

  /** Approve a single registration. */
  approve: async ({ registrationType, registrationId, reviewedBy }) => {
    return await api.post(`${BASE}/approve`, {
      registrationType,
      registrationId,
      reviewedBy,
    });
  },

  /** Reject a single registration with an optional reason. */
  reject: async ({ registrationType, registrationId, reason, reviewedBy }) => {
    return await api.post(`${BASE}/reject`, {
      registrationType,
      registrationId,
      reason,
      reviewedBy,
    });
  },
};

export default registrationService;