// scripts/endpoints/getOrganisationPersons.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{ORG_GUID}/persons
 *
 * @param {object} opts
 *   - token   (string) Bearer token for the Authorization header
 *   - domain  (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid (string) optionally override the default ORG_GUID
 */
export function getOrganisationPersons({ token, domain, orgGuid }) {
  // Fallback to config's ORG_GUID if none passed
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Build the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/persons`;

  // Set the request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check for 200 status on success
  check(res, {
    'GET organisation persons is 200': (r) => r.status === 200,
  });

  return res;
}
