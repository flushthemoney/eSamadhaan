# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files (including package-lock.json)
COPY package.json package-lock.json ./

# Install dependencies (including dev dependencies for build)
RUN npm ci

# Copy source files
COPY . .

# Allow API_URL to be passed as build argument (defaults to production URL)
ARG API_URL=https://api.esamadhaan.gov.in/api

# Replace API URL in environment.prod.ts if provided
RUN if [ -n "$API_URL" ] && [ "$API_URL" != "https://api.esamadhaan.gov.in/api" ]; then \
    sed -i.bak "s|apiUrl: \".*\",|apiUrl: \"$API_URL\",|g" src/environments/environment.prod.ts && \
    rm -f src/environments/environment.prod.ts.bak; \
    fi

# Build the application
ENV NODE_OPTIONS="--max-old-space-size=512"
RUN npm run build -- --configuration production --max-workers=1

# Stage 2: Runtime with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built application from build stage
COPY --from=build /app/dist/esamadhaan-ui/browser .

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Use custom entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]

