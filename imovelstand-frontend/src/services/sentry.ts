import * as Sentry from '@sentry/react';

/**
 * Inicializa Sentry se VITE_SENTRY_DSN estiver definido.
 * Em dev sem DSN, é no-op — código continua funcionando, apenas não reporta.
 */
export function initSentry() {
  const dsn = import.meta.env.VITE_SENTRY_DSN;
  if (!dsn) return;

  Sentry.init({
    dsn,
    environment: import.meta.env.MODE,
    tracesSampleRate: import.meta.env.PROD ? 0.1 : 1.0,
    replaysSessionSampleRate: 0,
    replaysOnErrorSampleRate: 1.0,
    integrations: [Sentry.browserTracingIntegration(), Sentry.replayIntegration({ maskAllText: false, blockAllMedia: false })]
  });
}

export { Sentry };
