// scripts/endpoints/getOrganisationParties.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{ORG_GUID}/parties
 *
 * @param {object} opts
 *   - token   (string) Bearer token for Authorization header
 *   - domain  (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid (string) optionally override the default config ORG_GUID
 */
export function getOrganisationParties({ token, domain, orgGuid }) {
  // Fallback to config's ORG_GUID if none is passed
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Construct the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/parties`;

  // Set request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check for a 200 status on success
  check(res, {
    'GET organisation parties is 200': (r) => r.status === 200,
  });

  return res;
}
