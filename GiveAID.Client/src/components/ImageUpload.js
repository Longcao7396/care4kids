/**
 * ImageUpload.js
 *
 * Multi-purpose image input for the admin gallery form.
 *
 * Supports TWO modes (toggle via the segmented control at the top):
 *   - "Upload"  : drag-and-drop OR click-to-pick a file. Calls the onFileSelected
 *                 callback with the chosen File (caller decides what to do with it —
 *                 typically galleryService.uploadFile(formData)).
 *   - "Paste URL" : plain text input. Calls onUrlSelected with the URL string.
 *                   Useful for reusing an externally-hosted image (Unsplash, etc.)
 *                   without re-uploading.
 *
 * Modes are mutually exclusive. The component manages both pieces of internal
 * state independently; whoever wraps it can read whichever callback they bound.
 *
 * Important constraints honored:
 *   - Client-side validation mirrors the server-side limits in
 *     GalleryController.cs (5 MB, jpg/png/webp). Server is still the source of
 *     truth — never trust client checks.
 *   - The XHR/fetch layer that uses this file MUST let the browser set the
 *     Content-Type header (axios does this automatically when the body is a
 *     FormData instance — do NOT set the header manually on the request).
 *
 * Props:
 *   - initialUrl (string): if editing an existing item whose image was uploaded
 *     via Cloudinary, pass the Cloudinary URL here so the preview shows on mount
 *     and the "Upload" mode can show "replace" wording.
 *   - onFileSelected (file: File|null): fired when the user picks a file (or
 *     null when they clear the selection). The parent will assemble a FormData
 *     and call galleryService.uploadFile / uploadFileReplace.
 *   - onUrlSelected (url: string|null): fired on text change in Paste URL mode.
 *   - maxSizeMB (number): default 5. Matches server's [RequestSizeLimit(5_242_880)].
 *   - acceptedTypes (string[]): default ['image/jpeg','image/jpg','image/png','image/webp'].
 *     Matches server's allowed list.
 *   - disabled (bool): disable the whole control during in-flight upload.
 *   - error (string): server-side error string to surface (e.g., "Cloudinary is
 *     not configured"). Shown below the preview.
 */

import React, { useState, useRef, useEffect } from 'react';
import { Alert, Button, Form } from 'react-bootstrap';

const DEFAULT_MAX_MB = 5;
const DEFAULT_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];

function ImageUpload({
  initialUrl = '',
  onFileSelected,
  onUrlSelected,
  maxSizeMB = DEFAULT_MAX_MB,
  acceptedTypes = DEFAULT_TYPES,
  disabled = false,
  error = '',
}) {
  const [mode, setMode] = useState('upload'); // 'upload' | 'url'
  const [file, setFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(initialUrl || '');
  const [urlInput, setUrlInput] = useState(initialUrl || '');
  const [validationErr, setValidationErr] = useState('');
  const [isDragging, setIsDragging] = useState(false);
  const inputRef = useRef(null);

  // Release object URL when component unmounts or file changes
  useEffect(() => {
    return () => {
      if (previewUrl && previewUrl.startsWith('blob:')) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  // Sync with external initialUrl changes (e.g., when editing a different item)
  useEffect(() => {
    if (initialUrl) {
      setPreviewUrl(initialUrl);
      setUrlInput(initialUrl);
    }
  }, [initialUrl]);

  const maxBytes = maxSizeMB * 1024 * 1024;

  function validateAndAccept(picked) {
    setValidationErr('');
    if (!picked) return;

    if (!acceptedTypes.includes((picked.type || '').toLowerCase())) {
      setValidationErr(
        `Image type "${picked.type || 'unknown'}" is not allowed. Use: ${acceptedTypes
          .map((t) => t.replace('image/', ''))
          .join(', ')}.`
      );
      return;
    }
    if (picked.size > maxBytes) {
      const actualMB = (picked.size / 1024 / 1024).toFixed(2);
      setValidationErr(
        `Image is ${actualMB} MB — max ${maxSizeMB} MB. Resize it before uploading.`
      );
      return;
    }

    // Revoke previous blob URL to avoid memory leaks
    setPreviewUrl((prev) => {
      if (prev && prev.startsWith('blob:')) URL.revokeObjectURL(prev);
      return URL.createObjectURL(picked);
    });
    setFile(picked);
    if (typeof onFileSelected === 'function') onFileSelected(picked);
  }

  function handleInputChange(e) {
    const picked = e.target.files?.[0];
    validateAndAccept(picked);
  }

  function handleDrop(e) {
    e.preventDefault();
    setIsDragging(false);
    if (disabled) return;
    const dropped = e.dataTransfer.files?.[0];
    validateAndAccept(dropped);
  }

  function handleDragEnter(e) {
    e.preventDefault();
    if (!disabled) setIsDragging(true);
  }

  function handleDragOver(e) {
    e.preventDefault(); // required so drop event fires
  }

  function handleDragLeave(e) {
    e.preventDefault();
    setIsDragging(false);
  }

  function clearFile() {
    setFile(null);
    setValidationErr('');
    setPreviewUrl((prev) => {
      if (prev && prev.startsWith('blob:')) URL.revokeObjectURL(prev);
      return initialUrl || '';
    });
    if (inputRef.current) inputRef.current.value = '';
    if (typeof onFileSelected === 'function') onFileSelected(null);
  }

  function handleUrlChange(e) {
    const v = e.target.value;
    setUrlInput(v);
    setPreviewUrl(v); // show preview as you type
    if (typeof onUrlSelected === 'function') onUrlSelected(v);
  }

  function switchMode(next) {
    setMode(next);
    setValidationErr('');
    if (next === 'url') {
      // Clear file state when switching to URL mode
      setFile(null);
      if (typeof onFileSelected === 'function') onFileSelected(null);
      setPreviewUrl(urlInput);
    } else {
      // Going back to upload mode — preserve initialUrl preview
      setPreviewUrl(initialUrl || '');
      setUrlInput('');
      if (typeof onUrlSelected === 'function') onUrlSelected('');
    }
  }

  return (
    <Form.Group className="mb-3">
      <Form.Label>Photo</Form.Label>

      {/* Segmented control for mode */}
      <div
        className="iu-mode-toggle"
        role="tablist"
        aria-label="Image source"
      >
        <button
          type="button"
          role="tab"
          aria-selected={mode === 'upload'}
          className={`iu-mode-btn ${mode === 'upload' ? 'is-active' : ''}`}
          onClick={() => switchMode('upload')}
          disabled={disabled}
        >
          <i className="bi bi-cloud-upload me-1" aria-hidden="true"></i>
          Upload file
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={mode === 'url'}
          className={`iu-mode-btn ${mode === 'url' ? 'is-active' : ''}`}
          onClick={() => switchMode('url')}
          disabled={disabled}
        >
          <i className="bi bi-link-45deg me-1" aria-hidden="true"></i>
          Paste URL
        </button>
      </div>

      {/* Main content area */}
      {mode === 'upload' ? (
        <div
          className={`iu-dropzone ${isDragging ? 'is-dragging' : ''} ${
            disabled ? 'is-disabled' : ''
          }`}
          onDrop={handleDrop}
          onDragEnter={handleDragEnter}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onClick={() => !disabled && inputRef.current?.click()}
          role="button"
          tabIndex={0}
          aria-label="Click or drag to upload an image"
        >
          <input
            ref={inputRef}
            type="file"
            accept={acceptedTypes.join(',')}
            onChange={handleInputChange}
            disabled={disabled}
            className="iu-file-input"
            aria-hidden="true"
          />
          {previewUrl && !validationErr ? (
            <div className="iu-preview-wrap">
              <img src={previewUrl} alt="Selected preview" className="iu-preview-img" />
              <div className="iu-preview-meta">
                {file ? (
                  <>
                    <span className="iu-filename">{file.name}</span>
                    <span className="iu-filesize">
                      {(file.size / 1024 / 1024).toFixed(2)} MB
                    </span>
                  </>
                ) : (
                  <span className="iu-filename">Current image</span>
                )}
              </div>
              {file && (
                <Button
                  variant="outline-light"
                  size="sm"
                  className="iu-clear-btn"
                  onClick={(e) => {
                    e.stopPropagation();
                    clearFile();
                  }}
                  disabled={disabled}
                >
                  <i className="bi bi-x-circle me-1" aria-hidden="true"></i>
                  Remove
                </Button>
              )}
            </div>
          ) : (
            <div className="iu-prompt">
              <i className="bi bi-cloud-arrow-up iu-upload-icon" aria-hidden="true"></i>
              <p className="iu-prompt-text">
                <strong>Click to upload</strong> or drag and drop
              </p>
              <p className="iu-prompt-hint">
                {acceptedTypes.map((t) => t.replace('image/', '')).join(', ')} &middot; max {maxSizeMB} MB
              </p>
            </div>
          )}
        </div>
      ) : (
        <Form.Control
          type="url"
          value={urlInput}
          onChange={handleUrlChange}
          placeholder="https://example.com/photo.jpg"
          isInvalid={!!error}
          disabled={disabled}
        />
      )}

      {/* URL preview pane (only in URL mode, when there is content) */}
      {mode === 'url' && urlInput && !validationErr && (
        <div className="iu-url-preview mt-2">
          <img src={urlInput} alt="URL preview" className="iu-url-preview-img" />
        </div>
      )}

      {/* Validation error (client-side) */}
      {validationErr && (
        <Alert variant="warning" className="mt-2 mb-0 py-2 small">
          <i className="bi bi-exclamation-triangle me-1" aria-hidden="true"></i>
          {validationErr}
        </Alert>
      )}

      {/* Server-side error */}
      {error && !validationErr && (
        <Alert variant="danger" className="mt-2 mb-0 py-2 small">
          <i className="bi bi-exclamation-octagon me-1" aria-hidden="true"></i>
          {error}
        </Alert>
      )}

      <style>{`
        .iu-mode-toggle {
          display: inline-flex;
          background: var(--primary-slate);
          border: 1px solid var(--border-slate);
          border-radius: 6px;
          padding: 2px;
          margin-bottom: 8px;
        }
        .iu-mode-btn {
          background: transparent;
          border: 0;
          color: var(--text-gray);
          padding: 4px 12px;
          font-size: 0.8rem;
          border-radius: 4px;
          cursor: pointer;
          transition: all 0.15s;
        }
        .iu-mode-btn.is-active {
          background: var(--accent-sky);
          color: var(--bg-card);
          font-weight: 600;
        }
        .iu-mode-btn:disabled { opacity: 0.5; cursor: not-allowed; }

        .iu-dropzone {
          position: relative;
          border: 2px dashed var(--border-slate);
          border-radius: 8px;
          padding: 16px;
          text-align: center;
          cursor: pointer;
          background: var(--primary-slate);
          transition: border-color 0.15s, background 0.15s;
          min-height: 140px;
          display: flex;
          align-items: center;
          justify-content: center;
        }
        .iu-dropzone:hover:not(.is-disabled) { border-color: var(--accent-sky); }
        .iu-dropzone.is-dragging {
          border-color: var(--accent-sky);
          background: rgba(56,189,248,0.05);
        }
        .iu-dropzone.is-disabled { opacity: 0.6; cursor: not-allowed; }
        .iu-file-input {
          position: absolute; inset: 0;
          opacity: 0; cursor: pointer;
        }

        .iu-prompt { padding: 8px; }
        .iu-upload-icon { font-size: 2rem; color: var(--accent-sky); display: block; margin-bottom: 6px; }
        .iu-prompt-text { margin: 0; color: var(--text-light); font-size: 0.85rem; }
        .iu-prompt-hint { margin: 4px 0 0 0; color: var(--text-gray); font-size: 0.7rem; }

        .iu-preview-wrap {
          position: relative;
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 8px;
        }
        .iu-preview-img {
          max-width: 100%;
          max-height: 240px;
          border-radius: 6px;
          object-fit: contain;
          background: var(--bg-card);
        }
        .iu-preview-meta {
          display: flex;
          gap: 12px;
          font-size: 0.75rem;
          color: var(--text-gray);
        }
        .iu-filename { font-weight: 600; color: var(--text-light); }
        .iu-clear-btn {
          position: absolute !important;
          top: 4px; right: 4px;
        }

        .iu-url-preview {
          max-height: 240px;
          overflow: hidden;
          border-radius: 6px;
          background: var(--primary-slate);
        }
        .iu-url-preview-img {
          max-width: 100%;
          max-height: 240px;
          object-fit: contain;
          display: block;
          margin: 0 auto;
        }
      `}</style>
    </Form.Group>
  );
}

export default ImageUpload;
