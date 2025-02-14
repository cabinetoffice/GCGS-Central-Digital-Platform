// scripts/endpoints/getMouLatest.js
import http from 'k6/http';
import { check } from 'k6';

/**
 * GET /mou/latest
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getMouLatest({ token, domain }) {
  // 1) Construct the full URL
  const url = `https://organisation.${domain}/mou/latest`;

  // 2) Build headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // 3) Make the GET request
  const res = http.get(url, params);

  // 4) Check for 200 OK
  check(res, {
    'GET /mou/latest status is 200': (r) => r.status === 200,
  });

  return res;
}
