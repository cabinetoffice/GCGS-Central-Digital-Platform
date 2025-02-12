// scripts/endpoints/postOrganisations.js
import http from 'k6/http';
import { check } from 'k6';
import { uuidv4 } from '../lib/uuid.js';

/**
 * POST /organisations
 *
 * @param {object} opts
 *  - token:  (optional) Bearer token string
 *  - domain: e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function postOrganisations({ token, domain }) {
  // Generate random GUIDs
  const guid = uuidv4();
  const additionalGuid = uuidv4();

  // Build the JSON payload
  const payload = JSON.stringify({
    name: `Test_${guid}`,
    type: 'organisation',
    identifier: {
      scheme: 'GB-PPON',
      id: guid,
      legalName: `Test_${guid}`,
    },
    additionalIdentifiers: [
      {
        scheme: 'GB-PPON',
        id: additionalGuid,
        legalName: `Test_${guid}`,
      },
    ],
    addresses: [
      {
        type: 'Registered',
        streetAddress: 'Buckingham Palace',
        locality: 'London',
        region: null,
        postalCode: 'SW1A 1AA',
        countryName: 'United Kingdom',
        country: 'GB',
      },
    ],
    contactPoint: {
      name: 'Procurement Team',
      email: 'procurement@example.com',
      telephone: '07700000000',
      url: 'https://example.com',
    },
    roles: ['buyer'],
  });

  // Construct the URL
  const url = `https://organisation.${domain}/organisations`;

  // Build headers + tagging
  const params = {
    tags: { name: 'postOrganisations' }, // <-- Tag for request metrics
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  };
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Make the POST request with the name tag
  const res = http.post(url, payload, params);

  // Tag the check with the same name
  check(
    res,
    { 'POST status is 201': (r) => r.status === 201 },
    { name: 'postOrganisations' } // <-- Tag for pass/fail stats
  );

  return res;
}
