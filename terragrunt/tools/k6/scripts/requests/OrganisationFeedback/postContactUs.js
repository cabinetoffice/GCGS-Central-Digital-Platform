// scripts/endpoints/postContactUs.js
import http from 'k6/http';
import { check } from 'k6';

/**
 * POST /feeback/contact-us
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for the Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function postContactUs({ token, domain }) {
  // 1) Construct the URL
  const url = `https://organisation.${domain}/feeback/contact-us`;

  // 2) Build the JSON body
  const payload = JSON.stringify({
    name: "string",
    emailAddress: "string",
    organisationName: "string",
    message: "string",
  });

  // 3) Prepare headers
  const headers = {
    accept: 'application/json',
    'Content-Type': 'application/json',
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  const params = { headers };

  // 4) Make the POST request
  const res = http.post(url, payload, params);

  // 5) Check for 200 OK
  check(res, {
    'POST contact-us status is 200': (r) => r.status === 200,
  });

  return res;
}
