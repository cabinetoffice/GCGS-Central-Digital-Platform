// scripts/endpoints/getOrganisationByGuid.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * GET /organisations/{guid}
 *
 * @param {object} opts
 *   - token   (string) - Bearer token for Authorization header
 *   - domain  (string) - The domain (e.g. "staging.supplier.information.findatender.codatt.net")
 */
export function getOrganisationByGuid({ token, domain }) {
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}`;

  // Add a request tag for "getOrganisationByGuid"
  const params = {
    tags: { name: 'getOrganisationByGuid' },
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Perform the GET request
  const res = http.get(url, params);

  // Tag the check with the same name
  check(
    res,
    { 'GET status is 200': (r) => r.status === 200 },
    { name: 'getOrganisationByGuid' }
  );

  return res;
}
