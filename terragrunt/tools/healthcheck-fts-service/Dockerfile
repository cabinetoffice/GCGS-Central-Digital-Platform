FROM php:5.6-apache

RUN docker-php-ext-install mysqli && docker-php-ext-enable mysqli

# Change Apache to listen on port 8070
RUN sed -i 's/80/8071/g' /etc/apache2/ports.conf /etc/apache2/sites-available/000-default.conf
EXPOSE 8071

COPY php/* /var/www/html/
