// scripts/endpoints/patchOrganisation.js
import http from 'k6/http';
import { check } from 'k6';
import { uuidv4 } from '../lib/uuid.js';
import { ORG_GUID } from '../config.js';

/**
 * PATCH /organisations/{existingOrganisationGuid}
 *
 * @param {object} opts
 *  - token:  (optional) Bearer token string
 *  - domain: e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function patchOrganisation({ token, domain }) {
  // 1) Hardcode the GUID in the path
  // 2) Generate random GUIDs if needed
  const guid = uuidv4();
  const additionalGuid = uuidv4();

  // 3) Build the JSON payload
  const payload = JSON.stringify({
    type: 'AdditionalIdentifiers',
    organisation: {
      organisationName: `Test_${guid}`,
      additionalIdentifiers: [
        {
          scheme: 'GB-PPON',
          id: `${guid}`,
          legalName: 'Acme Corporation Ltd.',
        },
      ],
      contactPoint: {
        name: 'Procurement Team',
        email: 'procurement@example.com',
        telephone: '079256123321',
        url: 'https://example.com',
      },
      addresses: [
        {
          type: 'Registered',
          streetAddress: '82 St. John’s Road',
          locality: 'CHESTER',
          region: 'Lancashire',
          postalCode: 'CH43 7UR',
          countryName: 'United Kingdom',
          country: 'GB',
        },
      ],
      identifierToRemove: {
        scheme: 'GB-PPON',
        id: '5a360be7-e1d3-4214-9f72-0e1d6b57b85d',
        legalName: 'Acme Corporation Ltd.',
      },
      roles: ['buyer'],
      buyerInformation: {
        buyerType: 'string',
        devolvedRegulations: ['NorthernIreland'],
      },
    },
  });

  // 4) Construct the PATCH URL
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}`;

  // 5) Build headers & add a name tag for patchOrganisation
  const params = {
    tags: { name: 'patchOrganisation' },
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  };
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // 6) Make the PATCH request
  const res = http.patch(url, payload, params);

  // 7) Expect 204 or 200 (depending on your API)
  check(
    res,
    { 'PATCH status is 204': (r) => r.status === 204 },
    { name: 'patchOrganisation' }
  );

  return res;
}
