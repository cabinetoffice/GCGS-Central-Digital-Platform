// scripts/endpoints/getOrganisationInvites.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{ORG_GUID}/invites
 *
 * @param {object} opts
 *   - token   (string) Bearer token for the Authorization header
 *   - domain  (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid (string) optionally override the default ORG_GUID
 */
export function getOrganisationInvites({ token, domain, orgGuid }) {
  // Fallback to config’s ORG_GUID if none passed
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Build the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/invites`;

  // Set the request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check the status code is 200
  check(res, {
    'GET organisation invites is 200': (r) => r.status === 200,
  });

  return res;
}
