// scripts/endpoints/postOrganisationInvite.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * POST /organisations/{ORG_GUID}/invites
 *
 * @param {object} opts
 *   - token    (string) Bearer token for the Authorization header
 *   - domain   (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid  (string) optionally override the default ORG_GUID
 *   - payload  (object/string) the JSON body for the POST.
 *       If not passed, we'll default to the sample from your curl:
 *         {
 *           firstName: "wdwdwqd",
 *           lastName: "dqwdwqdqwd",
 *           email: "dqwdqw@email.com",
 *           scopes: ["string"]
 *         }
 */
export function postOrganisationInvite({ token, domain, orgGuid, payload }) {
  // Fallback to config’s ORG_GUID if none is passed
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Build the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/invites`;

  // Prepare request headers
  const params = {
    headers: {
      accept: '*/*',                // same as '-H "accept: */*"'
      'Content-Type': 'application/json',
    },
  };

  // Include Authorization header if token is provided
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // If no custom payload is provided, default to the sample from your curl
  let body = payload;
  if (!body) {
    body = JSON.stringify({
      firstName: 'wdwdwqd',
      lastName: 'dqwdwqdqwd',
      email: 'dqwdqw@email.com',
      scopes: ['string'],
    });
  } else if (typeof body === 'object') {
    // If user passed an object, serialize it
    body = JSON.stringify(body);
  }

  // Make the POST request
  const res = http.post(url, body, params);

  // You might get 201 (Created) or 200 depending on your API’s response
  // We'll assume 201 is the expected success code
  check(res, {
    'POST invite is 200': (r) => r.status === 200,
  });

  return res;
}
