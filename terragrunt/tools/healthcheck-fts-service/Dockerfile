FROM php:8.1-apache

RUN docker-php-ext-install mysqli && docker-php-ext-enable mysqli

RUN sed -i "s/Listen 80/Listen ${APP_PORT:-8071}/" /etc/apache2/ports.conf
EXPOSE ${APP_PORT}

RUN apt-get update && apt-get install -y curl unzip

COPY php/* /var/www/html/

WORKDIR /var/www/html

RUN curl -sS https://getcomposer.org/installer | php -- --install-dir=/usr/local/bin --filename=composer && \
    composer require aws/aws-sdk-php
