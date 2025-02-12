// scripts/endpoints/getTenantLookup.js
import http from 'k6/http';
import { check } from 'k6';

/**
 * GET /tenant/lookup
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for the Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getTenantLookup({ token, domain }) {
  // Build the request URL using the domain param
  const url = `https://api.${domain}/tenant/lookup`;

  // Set the request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check the response status (assuming 200 on success)
  check(res, {
    'GET tenant lookup is 200': (r) => r.status === 200,
  });

  return res;
}
