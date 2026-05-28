FROM ruby:3.3-alpine

RUN apk add --no-cache --virtual .build-deps build-base \
    && gem install asciidoctor asciidoctor-pdf rouge \
    && apk del .build-deps
