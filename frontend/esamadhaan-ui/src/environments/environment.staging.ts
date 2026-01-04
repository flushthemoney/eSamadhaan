import { Environment } from './environment.interface';

export const environment: Environment = {
  production: false,
  environmentName: "Staging",
  apiUrl: "https://staging-api.esamadhaan.gov.in/api",
  apiTimeout: 30000,

  // Feature Flags
  features: {
    enableAnalytics: true,
    enableLogging: true,
    enableDebugMode: false,
    enableServiceWorker: true,
    enableNotifications: true,
    enableFileUpload: true,
    enableReports: true,
  },

  // Authentication
  auth: {
    tokenKey: "auth_token",
    tokenExpiryMinutes: 120,
    refreshTokenEnabled: false,
  },

  // Pagination
  pagination: {
    defaultPageSize: 25,
    pageSizeOptions: [10, 25, 50, 100],
  },

  // File Upload
  fileUpload: {
    maxSizeInMB: 5,
    allowedTypes: ["application/pdf", "image/jpeg", "image/jpg", "image/png"],
    allowedExtensions: [".pdf", ".jpg", ".jpeg", ".png"],
  },

  // Notification
  notification: {
    successDuration: 3000,
    errorDuration: 5000,
    warningDuration: 4000,
    infoDuration: 3000,
    position: {
      horizontal: "end",
      vertical: "top",
    },
  },

  // Logging
  logging: {
    level: "info", // Less verbose than dev
    enableConsoleLog: true,
    enableRemoteLog: true,
  },

  // External Services
  externalServices: {
    analyticsId: "UA-XXXXX-Y-STAGING",
    sentryDsn: "https://xxx@sentry.io/staging",
    googleMapsApiKey: "AIzaSy-STAGING-KEY",
  },
};

