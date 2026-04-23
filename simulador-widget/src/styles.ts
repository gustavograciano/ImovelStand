export const STYLES = (primary: string) => `
  .is-widget {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
    color: #1f2937;
    max-width: 720px;
    margin: 0 auto;
    padding: 24px;
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.06);
    border: 1px solid #e5e7eb;
  }
  .is-header h2 {
    font-size: 22px;
    font-weight: 700;
    margin: 0 0 6px 0;
    letter-spacing: -0.02em;
  }
  .is-header p {
    margin: 0 0 20px 0;
    color: #6b7280;
    font-size: 14px;
  }
  .is-form {
    display: grid;
    gap: 14px;
  }
  .is-field { display: flex; flex-direction: column; gap: 6px; }
  .is-field label {
    font-size: 12px;
    font-weight: 600;
    color: #374151;
    text-transform: uppercase;
    letter-spacing: 0.03em;
  }
  .is-field input, .is-field select {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid #d1d5db;
    border-radius: 8px;
    font-size: 15px;
    background: #ffffff;
    box-sizing: border-box;
    font-family: inherit;
  }
  .is-field input:focus, .is-field select:focus {
    outline: none;
    border-color: ${primary};
    box-shadow: 0 0 0 3px ${primary}33;
  }
  .is-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 14px;
  }
  @media (max-width: 500px) {
    .is-row { grid-template-columns: 1fr; }
  }
  .is-submit {
    padding: 12px 20px;
    background: ${primary};
    color: #ffffff;
    border: none;
    border-radius: 8px;
    font-size: 15px;
    font-weight: 600;
    cursor: pointer;
    transition: opacity 0.15s;
    margin-top: 6px;
  }
  .is-submit:hover { opacity: 0.9; }
  .is-submit:disabled { opacity: 0.5; cursor: not-allowed; }

  .is-result { margin-top: 28px; }

  .is-alert {
    padding: 14px 16px;
    border-radius: 8px;
    font-size: 14px;
    line-height: 1.5;
    margin-bottom: 16px;
  }
  .is-alert.is-success { background: #ecfdf5; color: #065f46; border: 1px solid #a7f3d0; }
  .is-alert.is-warning { background: #fffbeb; color: #92400e; border: 1px solid #fcd34d; }
  .is-alert.is-info    { background: #eff6ff; color: #1e40af; border: 1px solid #bfdbfe; }
  .is-alert.is-alert-error { background: #fef2f2; color: #991b1b; border: 1px solid #fca5a5; }

  .is-cards {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 12px;
    margin-bottom: 20px;
  }
  .is-card {
    padding: 14px;
    background: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
  }
  .is-card-label {
    font-size: 11px;
    font-weight: 600;
    color: #6b7280;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 6px;
  }
  .is-card-value {
    font-size: 20px;
    font-weight: 700;
    color: #111827;
    letter-spacing: -0.02em;
  }
  .is-card-hint {
    font-size: 11px;
    color: #9ca3af;
    margin-top: 4px;
  }

  .is-comparison {
    padding: 16px;
    background: #fafafa;
    border-radius: 8px;
    margin-bottom: 20px;
    border: 1px solid #e5e7eb;
  }
  .is-comparison h3 {
    font-size: 14px;
    font-weight: 700;
    margin: 0 0 12px 0;
  }
  .is-comparison-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
  }
  .is-comparison-grid div {
    display: flex;
    flex-direction: column;
    padding: 10px;
    background: #ffffff;
    border-radius: 6px;
    border: 1px solid #e5e7eb;
  }
  .is-comparison-grid strong {
    font-size: 12px;
    color: #6b7280;
    text-transform: uppercase;
  }
  .is-comparison-grid span {
    font-size: 18px;
    font-weight: 700;
    color: ${primary};
    margin: 4px 0;
  }
  .is-comparison-grid small {
    font-size: 11px;
    color: #9ca3af;
  }
  .is-reco {
    font-size: 13px;
    color: #4b5563;
    margin: 12px 0 0 0;
    font-style: italic;
  }

  .is-lead-section {
    margin-top: 24px;
    padding: 20px;
    background: linear-gradient(135deg, ${primary}11, ${primary}05);
    border: 1px solid ${primary}33;
    border-radius: 10px;
  }
  .is-lead-section h3 {
    font-size: 15px;
    font-weight: 700;
    margin: 0 0 14px 0;
  }
  .is-lead-form { display: grid; gap: 10px; }
  .is-lead-form input[type="text"],
  .is-lead-form input[type="email"],
  .is-lead-form input[type="tel"] {
    padding: 10px 12px;
    border: 1px solid #d1d5db;
    border-radius: 8px;
    font-size: 14px;
    background: #ffffff;
    font-family: inherit;
  }
  .is-check {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    font-size: 12px;
    color: #4b5563;
    cursor: pointer;
  }
  .is-check input { margin-top: 3px; }
  .is-lead-msg {
    min-height: 18px;
    font-size: 13px;
    margin-top: 6px;
  }
  .is-success-text { color: #059669; font-weight: 600; }
  .is-warning-text { color: #d97706; }
`;
