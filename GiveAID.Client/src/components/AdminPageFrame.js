import React from 'react';
import { Spinner } from 'react-bootstrap';
import '../pages/admin/AdminForm.css';

/* ─────────────────────────────────────────────────
 * AdminPageFrame
 *   Standard wrapper for /admin/* CRUD pages.
 *   Provides page header (eyebrow + title + sub + actions),
 *   optional tab strip, and content area.
 *
 *   props:
 *     eyebrow   (string)
 *     title     (string, required)
 *     sub       (string, optional description)
 *     actions   (node, optional — usually a button on the right)
 *     tabs      (array: [{ id, label, count, icon }])
 *     activeTab (string)
 *     onTabChange (fn)
 *     loading   (bool)
 *     error     (node/string)
 *     success   (node/string)
 *     children  (main content)
 * ───────────────────────────────────────────────── */

function AdminPageFrame({
  eyebrow,
  title,
  sub,
  actions,
  tabs,
  activeTab,
  onTabChange,
  loading,
  error,
  success,
  children,
}) {
  return (
    <div className="af-page">
      {/* Header */}
      <header className="af-header">
        <div className="af-header-text">
          {eyebrow && <p className="af-eyebrow">{eyebrow}</p>}
          <h1 className="af-title">{title}</h1>
          {sub && <p className="af-sub">{sub}</p>}
        </div>
        {actions && <div className="af-actions">{actions}</div>}
      </header>

      {/* Banners */}
      {error && (
        <div className="af-banner af-banner-error" role="alert">
          <span>{error}</span>
        </div>
      )}
      {success && (
        <div className="af-banner af-banner-success" role="status">
          <span>{success}</span>
        </div>
      )}

      {/* Tabs */}
      {tabs && tabs.length > 0 && (
        <div className="af-tabs" role="tablist">
          {tabs.map((t) => (
            <button
              key={t.id}
              type="button"
              role="tab"
              aria-selected={activeTab === t.id}
              className={`af-tab ${activeTab === t.id ? 'is-active' : ''}`}
              onClick={() => onTabChange?.(t.id)}
            >
              {t.icon && <i className={`bi ${t.icon}`} aria-hidden="true" />}
              <span>{t.label}</span>
              {t.count != null && <span className="af-tab-count">{t.count}</span>}
            </button>
          ))}
        </div>
      )}

      {/* Content */}
      {loading ? (
        <div className="af-loading">
          <Spinner animation="border" />
        </div>
      ) : (
        children
      )}
    </div>
  );
}

export default AdminPageFrame;
