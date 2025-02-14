// scripts/endpoints/patchOrganisationInvite.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID, INVITE_ID } from '../config.js';

/**
 * PATCH /organisations/{ORG_GUID}/invites/{INVITE_ID}
 *
 * @param {object} opts
 *   - token     (string)  Bearer token for the Authorization header
 *   - domain    (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid   (string)  optionally override the default ORG_GUID
 *   - inviteId  (string)  optionally override the default INVITE_ID
 *   - scopes    (array)   an array of scopes, default ["string"] if not passed
 */
export function patchOrganisationInvite({
  token,
  domain,
  orgGuid,
  inviteId,
  scopes,
}) {
  // Fallback to config’s ORG_GUID and INVITE_ID if none provided
  const finalOrgGuid = orgGuid || ORG_GUID;
  const finalInviteId = inviteId || INVITE_ID;

  // Build the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/invites/${finalInviteId}`;

  // Default to ["string"] if no scopes passed in
  const payload = JSON.stringify({
    scopes: scopes || ['string'],
  });

  // Prepare request headers
  const params = {
    headers: {
      'Content-Type': 'application/json',
      accept: '*/*',
    },
  };

  // Include Authorization header if token is provided
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Make the PATCH request
  const res = http.patch(url, payload, params);

  // Assume success status is 200 (often used for successful PATCH)
  check(res, {
    'PATCH invite is 204': (r) => r.status === 204,
  });

  return res;
}
