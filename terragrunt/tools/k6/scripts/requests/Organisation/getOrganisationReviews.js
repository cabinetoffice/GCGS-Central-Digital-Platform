// scripts/endpoints/getOrganisationReviews.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{guid}/reviews
 *
 * @param {object} opts
 *   - token   (string)  Bearer token for Authorization header
 *   - domain  (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getOrganisationReviews({ token, domain }) {
  // Use ORG_GUID from config
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/reviews`;

  // Add a name tag: getOrganisationReviews
  const params = {
    tags: { name: 'getOrganisationReviews' },
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Make the GET request
  const res = http.get(url, params);

  // Tag the check with the same name
  check(
    res,
    { 'GET /reviews status is 200': (r) => r.status === 200 },
    { name: 'getOrganisationReviews' }
  );

  return res;
}
