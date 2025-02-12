// scripts/endpoints/getOrganisationJoinRequests.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{guid}/join-requests
 *
 * @param {object} opts
 *   - token   (string)  Bearer token for Authorization header
 *   - domain  (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function getOrganisationJoinRequests({ token, domain }) {
  // Use the ORG_GUID from config
  const existingOrganisationGuid = ORG_GUID;

  // Construct the full URL including /join-requests
  const url = `https://organisation.${domain}/organisations/${existingOrganisationGuid}/join-requests`;

  // Add name tag: getOrganisationJoinRequests
  const params = {
    tags: { name: 'getOrganisationJoinRequests' },
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
    { 'GET join-requests status is 200': (r) => r.status === 200 },
    { name: 'getOrganisationJoinRequests' }
  );

  return res;
}
