import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Card, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { causesService } from '../services';

const CausesPage = () => {
  const [causes, setCauses] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadCauses();
  }, []);

  const loadCauses = async () => {
    try {
      const response = await causesService.getAll(true);
      if (response.success) {
        setCauses(response.data);
      }
    } catch (error) {
      console.error('Error loading causes:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(amount);
  };

  return (
    <div className="causes-page">
      <section className="page-header">
        <Container>
          <h1 className="page-title">Our Causes</h1>
          <p className="page-subtitle">
            Support child welfare causes that make a real difference
          </p>
        </Container>
      </section>

      <Container className="py-5">
        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
          </div>
        ) : causes.length > 0 ? (
          <Row className="g-4">
            {causes.map((cause) => (
              <Col key={cause.causeId} md={6} lg={4}>
                <Card className="h-100 cause-card">
                  <div className="cause-image">
                    <img
                      src={cause.imageUrl || '/images/placeholder-cause.jpg'}
                      alt={cause.causeName}
                      style={{ width: '100%', height: '200px', objectFit: 'cover' }}
                    />
                    <div className="cause-icon-overlay">
                      <i className={`bi bi-${cause.icon || 'heart'}`}></i>
                    </div>
                  </div>
                  <Card.Body>
                    <Card.Title>{cause.causeName}</Card.Title>
                    <Card.Text className="text-muted">
                      {cause.description}
                    </Card.Text>

                    {cause.targetAmount && (
                      <div className="mt-3">
                        <div className="d-flex justify-content-between mb-2">
                          <span className="fw-bold text-accent">
                            {formatCurrency(cause.raisedAmount || 0)}
                          </span>
                          <span className="text-muted">
                            Goal: {formatCurrency(cause.targetAmount)}
                          </span>
                        </div>
                        <div className="progress" style={{ height: '8px' }}>
                          <div
                            className="progress-bar"
                            style={{ width: `${Math.min(cause.percentageReached || 0, 100)}%` }}
                          ></div>
                        </div>
                        <small className="text-muted">
                          {Math.round(cause.percentageReached || 0)}% Funded
                        </small>
                      </div>
                    )}

                    <Button
                      as={Link}
                      to="/donate"
                      variant="primary"
                      className="w-100 mt-3"
                    >
                      <i className="bi bi-heart-fill me-2"></i>
                      Donate to this Cause
                    </Button>
                  </Card.Body>
                </Card>
              </Col>
            ))}
          </Row>
        ) : (
          <div className="empty-state text-center py-5">
            <i className="bi bi-heart" style={{ fontSize: '3rem', color: 'var(--text-gray)' }}></i>
            <h5 className="mt-3">No causes found</h5>
            <p className="text-muted">There are currently no causes available. Please check back soon.</p>
          </div>
        )}
      </Container>
    </div>
  );
};

export default CausesPage;
