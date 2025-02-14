// scripts/endpoints/getSingleConnectedEntity.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID, CONNECT_ENTITY_ID } from '../config.js';

/**
 * GET /organisations/{ORG_GUID}/connected-entities/{CONNECT_ENTITY_ID}
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for Authorization
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getSingleConnectedEntity({ token, domain }) {
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/connected-entities/${CONNECT_ENTITY_ID}`;

  // Build request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Perform the GET request
  const res = http.get(url, params);

  // Check the status (assuming 200 is success)
  check(res, {
    'GET single connected entity is 200': (r) => r.status === 200,
  });

  return res;
}
