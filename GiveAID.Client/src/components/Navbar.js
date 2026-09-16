import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Navbar as BsNavbar, Nav, Container, NavDropdown } from 'react-bootstrap';
import { useAuth } from '../contexts/AuthContext';
import './Navbar.css';

const Navbar = () => {
  const { user, isAuthenticated, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/');
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  return (
    <BsNavbar expand="lg" className="c4k-navbar" fixed="top">
      <Container className="c4k-navbar-container">
        <BsNavbar.Brand as={Link} to="/" className="c4k-brand">
          <span className="c4k-brand-icon" aria-hidden="true">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
            </svg>
          </span>
          <span className="c4k-brand-text">
            Care<span className="c4k-brand-accent">4</span>Kids
          </span>
        </BsNavbar.Brand>

        <BsNavbar.Toggle aria-controls="c4k-nav" className="c4k-toggler" />

        <BsNavbar.Collapse id="c4k-nav">
          <Nav className="ms-auto c4k-nav-list">
            <Nav.Link as={Link} to="/" className="c4k-nav-link">Home</Nav.Link>
            <Nav.Link as={Link} to="/campaigns" className="c4k-nav-link">Campaigns</Nav.Link>
            <Nav.Link as={Link} to="/gallery" className="c4k-nav-link">Gallery</Nav.Link>
            <Nav.Link as={Link} to="/about" className="c4k-nav-link">About</Nav.Link>
            <Nav.Link as={Link} to="/about/partners" className="c4k-nav-link">Our Partners</Nav.Link>
            <Nav.Link as={Link} to="/help-centre" className="c4k-nav-link">Help Centre</Nav.Link>
            <Nav.Link as={Link} to="/contact" className="c4k-nav-link">Contact</Nav.Link>

            {isAuthenticated ? (
              <>
                <NavDropdown
                  title={user?.fullName || 'Account'}
                  id="c4k-user-dropdown"
                  className="c4k-nav-dropdown"
                  align="end"
                >
                  <NavDropdown.Item as={Link} to="/dashboard">Dashboard</NavDropdown.Item>
                  <NavDropdown.Item as={Link} to="/my-donations">My Donations</NavDropdown.Item>
                  <NavDropdown.Item as={Link} to="/my-registrations">My Registrations</NavDropdown.Item>
                  <NavDropdown.Item as={Link} to="/raise-query">Raise a Query</NavDropdown.Item>
                  {isAdmin() && (
                    <>
                      <NavDropdown.Divider />
                      <NavDropdown.Item as={Link} to="/admin">Admin Dashboard</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/campaigns">Campaigns Mgmt</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/campaign-reports">Campaign Reports</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/partners">Partners Mgmt</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/gallery">Gallery Mgmt</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/about">About Us Mgmt</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/contacts">Contact Mgmt</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/cms">Content Mgmt</NavDropdown.Item>
                      <NavDropdown.Item as={Link} to="/admin/queries">Queries Mgmt</NavDropdown.Item>
                    </>
                  )}
                  <NavDropdown.Divider />
                  <NavDropdown.Item onClick={handleLogout}>Logout</NavDropdown.Item>
                </NavDropdown>

                <Link to="/donate" className="btn-coral c4k-nav-cta">
                  Donate
                </Link>
              </>
            ) : (
              <>
                <Nav.Link as={Link} to="/login" className="c4k-nav-link">Login</Nav.Link>
                <Link to="/register" className="c4k-nav-cta c4k-nav-cta-secondary">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                    <circle cx="8.5" cy="7" r="4"/>
                    <line x1="20" y1="8" x2="20" y2="14"/>
                    <line x1="23" y1="11" x2="17" y2="11"/>
                  </svg>
                  <span>Register</span>
                </Link>
                <Link to="/donate" className="c4k-nav-cta c4k-nav-cta-primary">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
                  </svg>
                  <span>Donate</span>
                </Link>
              </>
            )}
          </Nav>
        </BsNavbar.Collapse>
      </Container>
    </BsNavbar>
  );
};

export default Navbar;
