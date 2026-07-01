# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM node:20-alpine AS builder

WORKDIR /app

# Install dependencies first (layer cache)
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
RUN npm run build
# Output is in /app/dist

# ── Stage 2: Serve with nginx ──────────────────────────────────────────────
FROM nginx:alpine AS runner

# Copy built assets from builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# nginx config: handle Vue Router history mode (fallback to index.html)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
