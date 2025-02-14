// scripts/endpoints/postAddParty.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * POST /organisations/{ORG_GUID}/add-party
 *
 * @param {object} opts
 *   - token   (string) Bearer token for Authorization header
 *   - domain  (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid (string) optionally override the default ORG_GUID
 *   - organisationPartyId (string) optionally override the default party ID
 *   - organisationRelationship (string) optionally override the relationship (default "Consortium")
 *   - shareCode (string) optionally override the share code (default "6cndtLnK")
 */
export function postAddParty({
  token,
  domain,
  orgGuid,
  organisationPartyId,
  organisationRelationship,
  shareCode,
}) {
  // Fallback to config's ORG_GUID if none passed
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Provide defaults if needed
  const finalOrganisationPartyId = organisationPartyId || '83ccf479-47e9-41c0-983b-4d8527d51732';
  const finalOrganisationRelationship = organisationRelationship || 'Consortium';
  const finalShareCode = shareCode || '6cndtLnK';

  // Construct the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/add-party`;

  // Build the JSON payload
  const payload = JSON.stringify({
    organisationPartyId: finalOrganisationPartyId,
    organisationRelationship: finalOrganisationRelationship,
    shareCode: finalShareCode,
  });

  // Set request headers
  const headers = {
    accept: '*/*',             // from your curl: -H 'accept: */*'
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  };

  // Make the POST request
  const res = http.post(url, payload, { headers });

  // Check for a 200 status on success (adjust if the API returns 201 or something else)
  check(res, {
    'POST add-party is 200': (r) => r.status === 200,
  });

  return res;
}
