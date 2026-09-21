import api from './api';
import { API_ENDPOINTS } from '../config';

// =====================================================
// STATISTICS SERVICE
// Fetches aggregated statistics from the backend database.
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
    const response = await api.get(API_ENDPOINTS.STATISTICS.DASHBOARD);
    return response.data;
  },

  /**
   * Top 10 campaigns by raised amount, including donor count and progress %.
   */
  getCampaignsPerformance: async () => {
    const response = await api.get(API_ENDPOINTS.STATISTICS.CAMPAIGNS_PERFORMANCE);
    return response.data;
  },

  /**
   * Monthly donation totals for the last 12 months.
   * Returns array of { year, month, monthName, total, count }.
   */
  getMonthlyDonations: async () => {
    const response = await api.get(API_ENDPOINTS.STATISTICS.MONTHLY_DONATIONS);
    return response.data;
  },

  /**
   * Top 10 donors by total donated amount.
   */
  getTopDonors: async () => {
    const response = await api.get(API_ENDPOINTS.STATISTICS.TOP_DONORS);
    return response.data;
  },
};

export default statisticsService;
