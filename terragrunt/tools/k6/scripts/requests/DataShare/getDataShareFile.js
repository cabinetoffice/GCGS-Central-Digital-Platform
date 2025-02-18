// scripts/endpoints/getDataShareFile.js
import http from 'k6/http';
import { check } from 'k6';
import { SHARE_CODE } from '../config.js';  // <-- import your default share code

/**
 * GET /share/data/{SHARE_CODE}/file
 *
 * @param {object} opts
 *   - token     (string)  Bearer token for Authorization header
 *   - domain    (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - shareCode (string)  share code to override default config SHARE_CODE
 */
export function getDataShareFile({ token, domain, shareCode }) {
  // If no shareCode is provided in the call, fall back to config
  const finalShareCode = shareCode || SHARE_CODE;

  // Build the request URL
  const url = `https://data-sharing.${domain}/share/data/${finalShareCode}/file`;

  // Set request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check response status (assuming 200 on success)
  check(res, {
    'GET data share file is 200': (r) => r.status === 200,
  });

  return res;
}
