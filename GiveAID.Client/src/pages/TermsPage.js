import React from 'react';
import { Container } from 'react-bootstrap';
import '../styles/AboutPages.css';

function TermsPage() {
  return (
    <div className="about-subpage">
      <section className="page-header">
        <Container>
          <h1>
            Terms of <span className="text-accent">Service</span>
          </h1>
          <p className="lead">
            Please read these terms carefully before using our website and services.
          </p>
        </Container>
      </section>

      <Container className="pb-5">
        <div className="about-content">
          <div className="about-admin-card">
            <h2>Agreement to Terms</h2>
            <p>
              By accessing or using the Care4Kids website and services, you agree to be bound 
              by these Terms of Service. If you do not agree to these terms, please do not use 
              our services.
            </p>

            <h2>Our Mission</h2>
            <p>
              Care4Kids is a non-profit organization dedicated to supporting vulnerable children 
              through food, education, healthcare, and safe housing programmes. We facilitate 
              donations and volunteer participation to further our charitable mission.
            </p>

            <h2>Donations</h2>
            <p>By making a donation through our platform, you agree that:</p>
            <ul>
              <li>All donations are voluntary and non-refundable once processed</li>
              <li>Donations are used for charitable purposes as described in our campaigns</li>
              <li>You will receive a tax receipt for eligible donations</li>
              <li>You are authorized to use the payment method provided</li>
            </ul>

            <h2>User Accounts</h2>
            <p>
              When you create an account, you are responsible for maintaining the confidentiality 
              of your login credentials. You agree to notify us immediately of any unauthorized 
              use of your account.
            </p>

            <h2>User Conduct</h2>
            <p>You agree not to:</p>
            <ul>
              <li>Use our services for any unlawful purpose</li>
              <li>Submit false or misleading information</li>
              <li>Attempt to gain unauthorized access to our systems</li>
              <li>Interfere with the proper operation of our website</li>
            </ul>

            <h2>Intellectual Property</h2>
            <p>
              All content on this website, including text, images, logos, and design, is the 
              property of Care4Kids or its content creators and is protected by copyright laws.
            </p>

            <h2>Limitation of Liability</h2>
            <p>
              Care4Kids shall not be liable for any indirect, incidental, special, or consequential 
              damages arising from your use of our services. Our total liability shall not exceed 
              the amount of your most recent donation.
            </p>

            <h2>Changes to Terms</h2>
            <p>
              We reserve the right to modify these terms at any time. Continued use of our 
              services after changes constitutes acceptance of the updated terms.
            </p>

            <h2>Contact Us</h2>
            <p>
              For questions about these Terms of Service, please contact us at{' '}
              <a href="mailto:legal@care4kids.org">legal@care4kids.org</a> or through our{' '}
              <a href="/contact">contact page</a>.
            </p>

            <p className="text-muted mt-4">
              <em>Last updated: September 2026</em>
            </p>
          </div>
        </div>
      </Container>
    </div>
  );
}

export default TermsPage;
