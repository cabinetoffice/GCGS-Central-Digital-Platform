// scripts/endpoints/patchPersonScopes.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID, PERSON_ID } from '../config.js';

/**
 * PATCH /organisations/{ORG_GUID}/persons/{PERSON_ID}
 *
 * @param {object} opts
 *   - token     (string)  Bearer token for Authorization header
 *   - domain    (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid   (string)  if you want to override the default from config
 *   - personId  (string)  if you want to override the default from config
 */
export function patchPerson({
  token,
  domain,
  orgGuid,
  personId,
}) {
  // Fall back to config values if none are passed
  const finalOrgGuid = orgGuid || ORG_GUID;
  const finalPersonId = personId || PERSON_ID;

  // Build the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/persons/${finalPersonId}`;

  // Sample JSON body from your curl -d
  const payload = JSON.stringify({
    scopes: ['string'],
  });

  // Set request headers
  const params = {
    headers: {
      accept: '*/*',                // same as '-H "accept: */*"'
      'Content-Type': 'application/json',
    },
  };

  // Add bearer token if provided
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Make the PATCH request
  const res = http.patch(url, payload, params);

  // Check the response status (assuming 200 = success)
  check(res, {
    'PATCH person is 204': (r) => r.status === 204,
  });

  return res;
}
