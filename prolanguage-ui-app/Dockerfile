FROM node:22-alpine AS build
WORKDIR /app

# Copy package properties
COPY package.json package-lock.json ./
RUN npm ci

# Copy the rest of the application
COPY . .
RUN npm run build -- --configuration production

# Serve using Nginx
FROM nginx:alpine
COPY --from=build /app/dist/prolanguage-ui-app/browser /usr/share/nginx/html
# You can also copy a custom nginx configuration if needed
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
