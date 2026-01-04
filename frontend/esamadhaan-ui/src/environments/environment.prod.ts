import { Environment } from './environment.interface';

export const environment: Environment = {
  production: true,
  environmentName: "Production",
  apiUrl: "https://api.esamadhaan.gov.in/api",
  apiTimeout: 30000,

  // Feature Flags
  features: {
    enableAnalytics: true,
    enableLogging: false, // Disable console logging in prod
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
    level: "error", // Only log errors in production
    enableConsoleLog: false,
    enableRemoteLog: true,
  },

  // External Services
  externalServices: {
    analyticsId: "UA-XXXXX-Y",
    sentryDsn: "https://xxx@sentry.io/production",
    googleMapsApiKey: "AIzaSy-PRODUCTION-KEY",
  },
};

