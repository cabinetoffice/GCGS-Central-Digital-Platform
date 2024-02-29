FROM node:20-alpine as base
WORKDIR /app
RUN apk add --no-cache curl

FROM base as dependencies
COPY package.json package-lock.json /app/
RUN npm ci

FROM base as source

COPY --from=dependencies /app/package.json /app/package-lock.json /app/
COPY --from=dependencies /app/node_modules /app/node_modules
COPY tsconfig.json /app/
COPY public/ /app/public
COPY src/ /app/src

FROM source as build
RUN npm run build

FROM base as serve
RUN npm install -g serve
COPY --from=build /app/build /app/build
CMD ["serve", "-s", "/app/build"]

FROM source as dev
HEALTHCHECK CMD curl --fail --silent --write-out '{\"status\":%{http_code}}' --output /dev/null http://127.0.0.1:3000/
CMD ["npm", "start"]
