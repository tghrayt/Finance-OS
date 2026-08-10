FROM node:20.19-alpine AS build
WORKDIR /src
COPY package*.json ./
COPY apps/web/package*.json apps/web/
RUN npm ci
COPY apps/web apps/web
RUN npm --workspace apps/web run build

FROM nginx:1.29-alpine AS runtime
COPY apps/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/apps/web/dist/web/browser /usr/share/nginx/html
EXPOSE 80
