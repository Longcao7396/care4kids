import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';

// Layout Components
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import AdminLayout from './layouts/AdminLayout';
import ProtectedRoute from './components/ProtectedRoute';
import AuthBootstrap from './components/AuthBootstrap';

// Pages
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ResetPasswordPage from './pages/ResetPasswordPage';
import CausesPage from './pages/CausesPage';
import CampaignsPage from './pages/CampaignsPage';
import CampaignDetailPage from './pages/CampaignDetailPage';
import DonatePage from './pages/DonatePage';
import DonationReceiptPage from './pages/DonationReceiptPage';
import DonationHistoryDetailPage from './pages/DonationHistoryDetailPage';
import DashboardPage from './pages/DashboardPage';
import MyDonationsPage from './pages/MyDonationsPage';
import MyRegistrationsPage from './pages/MyRegistrationsPage';
import ProfilePage from './pages/ProfilePage';
import AboutPage from './pages/AboutPage';
import OurTeamPage from './pages/OurTeamPage';
import CareerPage from './pages/CareerPage';
import AchievementsPage from './pages/AchievementsPage';
import SupportersPage from './pages/SupportersPage';
import OurPartnersPage from './pages/OurPartnersPage';
import ContactPage from './pages/ContactPage';
import HelpCentrePage from './pages/HelpCentrePage';
import GalleryPage from './pages/GalleryPage';
import RaiseQueryPage from './pages/RaiseQueryPage';
import PrivacyPage from './pages/PrivacyPage';
import TermsPage from './pages/TermsPage';

// Admin Pages
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminCmsPage from './pages/admin/AdminCmsPage';
import AdminAboutPage from './pages/admin/AdminAboutPage';
import AdminContactPage from './pages/admin/AdminContactPage';
import AdminPartnersPage from './pages/admin/AdminPartnersPage';
import AdminGalleryPage from './pages/admin/AdminGalleryPage';
import AdminQueriesPage from './pages/admin/AdminQueriesPage';
import AdminNgoPage from './pages/admin/AdminNgoPage';
import AdminCampaignPage from './pages/admin/AdminCampaignPage';
import AdminCampaignReportsPage from './pages/admin/AdminCampaignReportsPage';
import AdminDonationsPage from './pages/admin/AdminDonationsPage';
import AdminUsersPage from './pages/admin/AdminUsersPage';
import AdminAchievementsPage from './pages/admin/AdminAchievementsPage';
import AdminInvitationsPage from './pages/admin/AdminInvitationsPage';
import AdminEmailLogsPage from './pages/admin/AdminEmailLogsPage';

// Styles
import './styles/App.css';
import 'bootstrap/dist/css/bootstrap.min.css';

const ADMIN_ROLES = ['Admin', 'SuperAdmin'];

function App() {
  return (
    <AuthProvider>
      <Router>
        <AuthBootstrap />
        <div className="App">
          <Routes>
            {/* ═══════════════════════════════════════════════════
                ADMIN ROUTES — own shell, require Admin/SuperAdmin
               ═══════════════════════════════════════════════════ */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute roles={ADMIN_ROLES}>
                  <AdminLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<AdminDashboard />} />
              <Route path="campaigns" element={<AdminCampaignPage />} />
              <Route path="campaign-reports" element={<AdminCampaignReportsPage />} />
              <Route path="donations" element={<AdminDonationsPage />} />
              <Route path="users" element={<AdminUsersPage />} />
              <Route path="ngos" element={<AdminNgoPage />} />
              <Route path="partners" element={<AdminPartnersPage />} />
              <Route path="gallery" element={<AdminGalleryPage />} />
              <Route path="achievements" element={<AdminAchievementsPage />} />
              <Route path="cms" element={<AdminCmsPage />} />
              <Route path="about" element={<AdminAboutPage />} />
              <Route path="queries" element={<AdminQueriesPage />} />
              <Route path="contacts" element={<AdminContactPage />} />
              <Route path="invitations" element={<AdminInvitationsPage />} />
              <Route path="emails" element={<AdminEmailLogsPage />} />
              {/* Admin fallback */}
              <Route path="*" element={<Navigate to="/admin" replace />} />
            </Route>

            {/* ═══════════════════════════════════════════════════
                PUBLIC + USER ROUTES — share Navbar + Footer
               ═══════════════════════════════════════════════════ */}
            <Route
              path="*"
              element={
                <>
                  <Navbar />
                  <main className="main-content">
                    <Routes>
                      {/* Public Routes */}
                      <Route path="/" element={<HomePage />} />
                      <Route path="/login" element={<LoginPage />} />
                      <Route path="/register" element={<RegisterPage />} />
                      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
                      <Route path="/reset-password" element={<ResetPasswordPage />} />
                      <Route path="/causes" element={<CausesPage />} />
                      <Route path="/campaigns" element={<CampaignsPage />} />
                      <Route path="/campaigns/:id" element={<CampaignDetailPage />} />
                      <Route path="/about" element={<AboutPage />} />
                      <Route path="/about/team" element={<OurTeamPage />} />
                      <Route path="/about/careers" element={<CareerPage />} />
                      <Route path="/about/achievements" element={<AchievementsPage />} />
                      <Route path="/about/supporters" element={<SupportersPage />} />
                      <Route path="/about/partners" element={<OurPartnersPage />} />
                      <Route path="/contact" element={<ContactPage />} />
                      <Route path="/help-centre" element={<HelpCentrePage />} />
                      <Route path="/gallery" element={<GalleryPage />} />
                      <Route path="/privacy" element={<PrivacyPage />} />
                      <Route path="/terms" element={<TermsPage />} />

                      {/* Protected (authenticated) routes */}
                      <Route
                        path="/raise-query"
                        element={
                          <ProtectedRoute>
                            <RaiseQueryPage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/donate"
                        element={
                          <ProtectedRoute>
                            <DonatePage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/donation-receipt/:id"
                        element={
                          <ProtectedRoute>
                            <DonationReceiptPage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/my-donations/:id"
                        element={
                          <ProtectedRoute>
                            <DonationHistoryDetailPage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/dashboard"
                        element={
                          <ProtectedRoute>
                            <DashboardPage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/my-donations"
                        element={
                          <ProtectedRoute>
                            <MyDonationsPage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/my-registrations"
                        element={
                          <ProtectedRoute>
                            <MyRegistrationsPage />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/profile"
                        element={
                          <ProtectedRoute>
                            <ProfilePage />
                          </ProtectedRoute>
                        }
                      />

                      {/* 404 */}
                      <Route path="*" element={<Navigate to="/" replace />} />
                    </Routes>
                  </main>
                  <Footer />
                </>
              }
            />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
