// scripts/endpoints/patchOrganisationReview.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID, REVIEWED_BY_ID } from '../config.js';

/**
 * PATCH /support/organisation/{ORG_GUID}
 *
 * @param {object} opts
 *   - token           (string) Bearer token for Authorization header
 *   - domain          (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid         (string) optionally override the default ORG_GUID
 *   - reviewedById    (string) optionally override the default REVIEWED_BY_ID
 *   - approved        (boolean) or anything you like to pass in
 *   - comment         (string)  e.g. "this is my comment"
 *   - type            (string)  defaults to "Review"
 */
export function patchOrganisationReview({
  token,
  domain,
  orgGuid,
  reviewedById,
  approved = true,
  comment = 'string',
  type = 'Review',
}) {
  // Fallback to config’s ORG_GUID and REVIEWED_BY_ID if none provided
  const finalOrgGuid = orgGuid || ORG_GUID;
  const finalReviewedById = reviewedById || REVIEWED_BY_ID;

  // Build the request URL
  const url = `https://organisation.${domain}/support/organisation/${finalOrgGuid}`;

  // Prepare request headers
  const params = {
    headers: {
      'Content-Type': 'application/json',
      accept: 'application/json',
    },
  };

  // Include Authorization header if token is provided
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Construct the JSON body (default from your curl)
  const payload = JSON.stringify({
    type,
    organisation: {
      reviewedById: finalReviewedById,
      approved,
      comment,
    },
  });

  // Make the PATCH request
  const res = http.patch(url, payload, params);

  // Check for 200 (or the appropriate success code your endpoint returns)
  check(res, {
    'PATCH organisation review is 200': (r) => r.status === 200,
  });

  return res;
}
