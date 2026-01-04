import { Environment } from './environment.interface';

export const environment: Environment = {
  production: false,
  environmentName: "Development",
  apiUrl: "https://localhost:5124/api",
  apiTimeout: 30000, // 30 seconds

  // Feature Flags
  features: {
    enableAnalytics: false,
    enableLogging: true,
    enableDebugMode: true,
    enableServiceWorker: false,
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
    level: "debug", // 'debug' | 'info' | 'warn' | 'error'
    enableConsoleLog: true,
    enableRemoteLog: false,
  },

  // External Services (if any)
  externalServices: {
    analyticsId: "",
    sentryDsn: "",
    googleMapsApiKey: "",
  },
};

