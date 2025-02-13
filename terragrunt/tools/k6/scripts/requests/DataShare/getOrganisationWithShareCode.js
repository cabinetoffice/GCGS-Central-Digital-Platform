// scripts/endpoints/getOrganisationShareCode.js
import http from 'k6/http';
import { check } from 'k6';
// Import your defaults from config
import { ORG_GUID, SHARE_CODE } from '../config.js';

/**
 * GET /share/organisations/{ORG_GUID}/codes/{SHARE_CODE}
 *
 * @param {object} opts
 *   - token     (string)  Bearer token for Authorization header
 *   - domain    (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid   (string)  override the default config ORG_GUID if needed
 *   - shareCode (string)  override the default config SHARE_CODE if needed
 */
export function getOrganisationWithShareCode({
  token,
  domain,
  orgGuid,
  shareCode,
}) {
  // Fall back to config values if none are passed
  const finalOrgGuid = orgGuid || ORG_GUID;
  const finalShareCode = shareCode || SHARE_CODE;

  // Build the request URL
  const url = `https://data-sharing.${domain}/share/organisations/${finalOrgGuid}/codes/${finalShareCode}`;

  // Set request headers
  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Check the response status (assuming 200 on success)
  check(res, {
    'GET specific share code is 200': (r) => r.status === 200,
  });

  return res;
}
