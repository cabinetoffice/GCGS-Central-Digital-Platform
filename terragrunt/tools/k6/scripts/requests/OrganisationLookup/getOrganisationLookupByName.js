// scripts/endpoints/getOrganisationLookupByName.js
import http from 'k6/http';
import { check } from 'k6';

/**
 * GET /organisation/lookup?name={NAME}
 *
 * @param {object} opts
 *   - token   (string) Bearer token for the Authorization header
 *   - domain  (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - name    (string) the name to look up, e.g. "test123"
 */
export function getOrganisationLookupByName({ token, domain, name = 'test123' }) {
  // Build the request URL with the query param
  const url = `https://organisation.${domain}/organisation/lookup?name=${encodeURIComponent(name)}`;

  // Prepare request headers
  const params = {
    headers: {
      accept: 'application/json',
    },
  };

  // Include Bearer token if provided
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Perform the GET request
  const res = http.get(url, params);

  // Check for a 200 success status
  check(res, {
    'GET org lookup is 200': (r) => r.status === 200,
  });

  return res;
}
