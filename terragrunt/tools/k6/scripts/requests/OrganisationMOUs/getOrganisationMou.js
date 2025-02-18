// scripts/endpoints/getOrganisationMou.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{ORG_GUID}/mou
 *
 * @param {object} opts
 *   - token  (string) Bearer token for Authorization header
 *   - domain (string) e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getOrganisationMou({ token, domain }) {
  // Build the request URL, referencing ORG_GUID from config
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/mou`;

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
    'GET MOU status is 200': (r) => r.status === 200,
  });

  return res;
}
