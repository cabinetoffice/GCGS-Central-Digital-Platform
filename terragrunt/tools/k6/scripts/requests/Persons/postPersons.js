// scripts/endpoints/postPersons.js
import http from 'k6/http';
import { check } from 'k6';
import { uuidv4 } from '../lib/uuid.js';

/**
 * POST /persons
 *
 * @param {object} opts
 *   - token   (string)  Bearer token for Authorization header (optional)
 *   - domain  (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - payload (object)  optionally override the default POST body
 */
export function postPersons({ token, domain, payload }) {
  // Construct the request URL
  const url = `https://person.${domain}/persons`;

  // Prepare request headers
  const params = {
    headers: {
      'Content-Type': 'application/json',
      accept: 'application/json',
    },
  };

  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Generate a unique UUID to embed in the email
  const personUuid = uuidv4();

  // If no custom payload is provided, default to the sample from your curl
  let body = payload;
  if (!body) {
    body = {
      firstName: `${personUuid}`,
      lastName: `${personUuid}`,
      phone: `${personUuid}`,
      userUrn: `${personUuid}`,
      // Insert the UUID into the email address
      email: `${personUuid}@example.com`,
    };
  }

  // If it's an object, convert to JSON
  if (typeof body === 'object') {
    body = JSON.stringify(body);
  }

  // Make the POST request
  const res = http.post(url, body, params);

  // Typically, creating a resource returns a 201 (Created), 
  // but double-check your API docs. We'll assume 201:
  check(res, {
    'POST person is 201': (r) => r.status === 201,
  });

  return res;
}
