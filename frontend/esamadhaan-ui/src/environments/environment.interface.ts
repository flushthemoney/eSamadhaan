export interface Environment {
  production: boolean;
  environmentName: string;
  apiUrl: string;
  apiTimeout: number;

  features: {
    enableAnalytics: boolean;
    enableLogging: boolean;
    enableDebugMode: boolean;
    enableServiceWorker: boolean;
    enableNotifications: boolean;
    enableFileUpload: boolean;
    enableReports: boolean;
  };

  auth: {
    tokenKey: string;
    tokenExpiryMinutes: number;
    refreshTokenEnabled: boolean;
  };

  pagination: {
    defaultPageSize: number;
    pageSizeOptions: number[];
  };

  fileUpload: {
    maxSizeInMB: number;
    allowedTypes: string[];
    allowedExtensions: string[];
  };

  notification: {
    successDuration: number;
    errorDuration: number;
    warningDuration: number;
    infoDuration: number;
    position: {
      horizontal: "start" | "center" | "end" | "left" | "right";
      vertical: "top" | "bottom";
    };
  };

  logging: {
    level: "debug" | "info" | "warn" | "error";
    enableConsoleLog: boolean;
    enableRemoteLog: boolean;
  };

  externalServices: {
    analyticsId: string;
    sentryDsn: string;
    googleMapsApiKey: string;
  };
}

