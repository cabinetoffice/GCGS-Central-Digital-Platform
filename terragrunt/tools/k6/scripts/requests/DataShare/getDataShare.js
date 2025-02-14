// scripts/endpoints/getDataShare.js
import http from 'k6/http';
import { check } from 'k6';

// Import your share code (and anything else) from config
import { SHARE_CODE } from '../config.js';

/**
 * GET /share/data/{SHARE_CODE}
 *
 * @param {object} opts
 *   - token     (string)  Bearer token for Authorization header
 *   - domain    (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - shareCode (string)  If you want to override the default config SHARE_CODE
 */
export function getDataShare({ token, domain, shareCode }) {
  // If shareCode is not passed in, fall back to SHARE_CODE from config
  const finalShareCode = shareCode || SHARE_CODE;

  // Build the request URL — referencing `data-sharing` subdomain
  const url = `https://data-sharing.${domain}/share/data/${finalShareCode}`;

  // Set request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check response status (assuming 200 = success)
  check(res, {
    'GET data share is 200': (r) => r.status === 200,
  });

  return res;
}
