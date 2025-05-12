FROM node:20-alpine AS builder

# Create app directory
WORKDIR /app

# Copy package.json and package-lock.json first for better caching
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build if your project requires a build step (uncomment if needed)
RUN npm run build

# Remove development dependencies
# RUN npm prune --production

# Production stage
FROM node:18-alpine

# Create app directory
WORKDIR /app

# Set NODE_ENV
ENV NODE_ENV=production

# Copy from builder stage
COPY --from=builder /app .

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001 && \
    chown -R nodejs:nodejs /app

# Switch to non-root user
USER nodejs

# Expose the API port
EXPOSE 8000

# Health check
# HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
#   CMD wget --no-verbose --tries=1 --spider http://localhost:8000/ || exit 1

# Run the application
CMD ["npm", "run", "start"]

# !TODO: Seeding to add initial data

