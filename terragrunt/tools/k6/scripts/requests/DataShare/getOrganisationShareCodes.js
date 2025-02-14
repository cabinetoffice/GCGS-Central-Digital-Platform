// scripts/endpoints/getOrganisationShareCodes.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js'; // <-- Import your org GUID from config

/**
 * GET /share/organisations/{ORG_GUID}/codes
 *
 * @param {object} opts
 *   - token   (string)  Bearer token for Authorization header
 *   - domain  (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid (string)  Override if you want a different ID than what's in config
 */
export function getOrganisationShareCodes({ token, domain, orgGuid }) {
  // If orgGuid isn't passed, use the default from config
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Build the request URL
  const url = `https://data-sharing.${domain}/share/organisations/${finalOrgGuid}/codes`;

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
    'GET organisation share codes is 200': (r) => r.status === 200,
  });

  return res;
}
