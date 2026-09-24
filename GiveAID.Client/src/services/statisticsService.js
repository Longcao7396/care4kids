import api from './api';
import { API_ENDPOINTS } from '../config';

// =====================================================
// STATISTICS SERVICE
// Fetches aggregated statistics from the backend database.
// NOTE: The api.js interceptor already unwraps the v2.0 envelope
// { success, data } and returns body.data directly. Do NOT add
// another .data accessor here.
// =====================================================
export const statisticsService = {
  /**
   * Dashboard overview statistics:
   * - totalRaised: sum of all completed donations
   * - totalDonors: unique donors count
   * - activeCampaigns / totalCampaigns / completedCampaigns
   * - thisMonthRaised / lastMonthRaised / monthlyGrowthPercent
   * - totalCauses
   */
  getDashboardStatistics: async () => {
    // Interceptor returns body.data directly (the stats object).
    return api.get(API_ENDPOINTS.STATISTICS.DASHBOARD);
  },

  /**
   * Top 10 campaigns by raised amount, including donor count and progress %.
   */
  getCampaignsPerformance: async () => {
    // Interceptor returns body.data directly (array of campaign stats).
    return api.get(API_ENDPOINTS.STATISTICS.CAMPAIGNS_PERFORMANCE);
  },

  /**
   * Monthly donation totals for the last 12 months.
   * Returns array of { year, month, monthName, total, count }.
   */
  getMonthlyDonations: async () => {
    // Interceptor returns body.data directly (array of monthly totals).
    return api.get(API_ENDPOINTS.STATISTICS.MONTHLY_DONATIONS);
  },

  /**
   * Top 10 donors by total donated amount.
   */
  getTopDonors: async () => {
    // Interceptor returns body.data directly (array of donor stats).
    return api.get(API_ENDPOINTS.STATISTICS.TOP_DONORS);
  },
};

export default statisticsService;
