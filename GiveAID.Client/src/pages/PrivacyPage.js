import React from 'react';
import { Container } from 'react-bootstrap';
import '../styles/AboutPages.css';

function PrivacyPage() {
  return (
    <div className="about-subpage">
      <section className="page-header">
        <Container>
          <h1>
            Privacy <span className="text-accent">Policy</span>
          </h1>
          <p className="lead">
            Your privacy matters to us. Learn how we collect, use, and protect your personal information.
          </p>
        </Container>
      </section>

      <Container className="pb-5">
        <div className="about-content">
          <div className="about-admin-card">
            <h2>Introduction</h2>
            <p>
              Care4Kids ("we," "our," or "us") is committed to protecting your privacy. 
              This Privacy Policy explains how we collect, use, disclose, and safeguard your 
              information when you visit our website or use our services.
            </p>

            <h2>Information We Collect</h2>
            <p>We may collect the following types of information:</p>
            <ul>
              <li><strong>Personal Information:</strong> Name, email address, phone number, postal address, and payment information when you make a donation or register for a programme.</li>
              <li><strong>Usage Data:</strong> Information about how you access and use our website, including your IP address, browser type, and pages visited.</li>
              <li><strong>Cookies:</strong> Small data files stored on your device to enhance your browsing experience.</li>
            </ul>

            <h2>How We Use Your Information</h2>
            <p>We use your information to:</p>
            <ul>
              <li>Process donations and send receipts</li>
              <li>Communicate about campaigns and programmes</li>
              <li>Provide customer support</li>
              <li>Improve our website and services</li>
              <li>Comply with legal obligations</li>
            </ul>

            <h2>Data Protection</h2>
            <p>
              We implement appropriate technical and organizational measures to protect your personal 
              data against unauthorized access, alteration, disclosure, or destruction. All payment 
              information is processed securely through encrypted connections.
            </p>

            <h2>Your Rights</h2>
            <p>Under applicable data protection laws, you have the right to:</p>
            <ul>
              <li>Access your personal data</li>
              <li>Correct inaccurate data</li>
              <li>Request deletion of your data</li>
              <li>Withdraw consent at any time</li>
              <li>Lodge a complaint with a supervisory authority</li>
            </ul>

            <h2>Contact Us</h2>
            <p>
              If you have questions about this Privacy Policy or wish to exercise your rights, 
              please contact us at <a href="mailto:privacy@care4kids.org">privacy@care4kids.org</a> 
              or through our <a href="/contact">contact page</a>.
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

export default PrivacyPage;
