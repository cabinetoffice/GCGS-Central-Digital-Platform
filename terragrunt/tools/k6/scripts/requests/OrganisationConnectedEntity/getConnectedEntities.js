// scripts/endpoints/getConnectedEntities.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js'; 
// If you prefer to hardcode the GUID, you can skip importing config and define a const here.

/**
 * GET /organisations/{orgGuid}/connected-entities
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for the Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getConnectedEntities({ token, domain }) {
  // Build the request URL with the org ID from config
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/connected-entities`;

  // Headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check the response (assuming 200 is success)
  check(res, {
    'GET connected-entities is 200': (r) => r.status === 200,
  });

  return res;
}
