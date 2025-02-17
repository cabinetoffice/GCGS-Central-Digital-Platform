// scripts/endpoints/getOrganisationApiKeys.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{ORG_GUID}/api-keys
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getOrganisationApiKeys({ token, domain }) {
  // 1) Build the request URL, referencing ORG_GUID from config
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/api-keys`;

  // 2) Set the request headers
  const params = {
    headers: {
      accept: 'application/json',        // from your curl: -H 'accept: application/json'
      Authorization: `Bearer ${token}`,  // from your curl: -H 'Authorization: Bearer ...'
    },
  };

  // 3) Make the GET request
  const res = http.get(url, params);

  // 4) Check the response status (assuming 200 on success)
  check(res, {
    'GET organisation API keys is 200': (r) => r.status === 200,
  });

  return res;
}
