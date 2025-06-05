FROM php:5.6-apache

ARG APP_PORT=8071
ENV APP_PORT=${APP_PORT}

RUN docker-php-ext-install mysqli && docker-php-ext-enable mysqli

# Change Apache to listen on port 8070
RUN sed -i "s/Listen 80/Listen ${APP_PORT}/" /etc/apache2/ports.conf
EXPOSE ${APP_PORT}

COPY php/* /var/www/html/
