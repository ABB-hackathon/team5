FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY src/ .
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80

