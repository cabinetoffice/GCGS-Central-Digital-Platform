import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js'; // (If using config)

// If you don't want a separate config, you can just hardcode the ID here
// const ORG_GUID = 'b1d62e28-52de-4769-92fb-5db76ee7993b';

/**
 * GET /organisations/{orgGuid}/buyer-information
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for the Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getBuyerInformation({ token, domain }) {
  // Build the request URL with the org ID from config (or local constant).
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/buyer-information`;

  // Headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check the status code (assuming 200 on success)
  check(res, {
    'GET buyer-information status is 200': (r) => r.status === 200,
  });

  return res;
}
