// scripts/endpoints/putConnectedEntity.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID, CONNECT_ENTITY_ID } from '../config.js';

/**
 * PUT /organisations/{ORG_GUID}/connected-entities/{CONNECT_ENTITY_ID}
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function putConnectedEntity({ token, domain }) {
  // 1) Construct the full URL
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/connected-entities/${CONNECT_ENTITY_ID}`;

  // 2) JSON payload (direct from your curl -d)
  const payload = JSON.stringify({
    "id": "string",
    "entityType": "Organisation",
    "hasCompnayHouseNumber": true,
    "companyHouseNumber": "string",
    "overseasCompanyNumber": "string",
    "organisation": {
      "category": "RegisteredCompany",
      "name": "string",
      "insolvencyDate": "2025-02-11T21:18:28.410Z",
      "registeredLegalForm": "string",
      "lawRegistered": "string",
      "controlCondition": ["None"],
      "organisationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    "individualOrTrust": {
      "category": "PersonWithSignificantControlForIndividual",
      "firstName": "string",
      "lastName": "string",
      "dateOfBirth": "2025-02-11T21:18:28.410Z",
      "nationality": "string",
      "controlCondition": ["None"],
      "connectedType": "Individual",
      "personId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "residentCountry": "string"
    },
    "addresses": [
      {
        "streetAddress": "82 St. John’s Road",
        "locality": "CHESTER",
        "region": "Lancashire",
        "postalCode": "CH43 7UR",
        "countryName": "United Kingdom",
        "country": "GB",
        "type": "Registered"
      }
    ],
    "registeredDate": "2025-02-11T21:18:28.410Z",
    "registerName": "string",
    "startDate": "2025-02-11T21:18:28.410Z",
    "endDate": "2025-02-11T21:18:28.410Z"
  });

  // 3) Headers
  const headers = {
    accept: '*/*',  // from curl: -H 'accept: */*'
    'Content-Type': 'application/json'
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  // 4) Make the PUT request
  const res = http.put(url, payload, { headers });

  // 5) Check response (assuming 200 on success)
  check(res, {
    'PUT connected entity is 204': (r) => r.status === 204,
  });

  return res;
}
