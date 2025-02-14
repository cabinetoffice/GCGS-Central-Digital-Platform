// scripts/endpoints/getPersonById.js
import http from 'k6/http';
import { check } from 'k6';
import { PERSON_ID } from '../config.js';

/**
 * GET /persons/{PERSON_ID}
 *
 * @param {object} opts
 *   - token    (string) Bearer token for the Authorization header
 *   - domain   (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - personId (string) optionally override the default PERSON_ID
 */
export function getPersonById({ token, domain, personId }) {
  // Fallback to config’s PERSON_ID if none is provided
  const finalPersonId = personId || PERSON_ID;

  // Build the request URL
  const url = `https://person.${domain}/persons/${finalPersonId}`;

  // Prepare headers
  const params = {
    headers: {
      accept: 'application/json',
    },
  };

  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Make the GET request
  const res = http.get(url, params);

  // Check for a 200 success
  check(res, {
    'GET person is 200': (r) => r.status === 200,
  });

  return res;
}
