// scripts/endpoints/postConnectedEntities.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js'; // If you're using config.js

/**
 * POST /organisations/{orgGuid}/connected-entities
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function postConnectedEntities({ token, domain }) {
  // 1) Construct the URL
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/connected-entities`;

  // 2) Build the JSON payload exactly as in your curl -d
  const payload = JSON.stringify({
    entityType: "Organisation",
    hasCompnayHouseNumber: true,
    companyHouseNumber: "string",
    overseasCompanyNumber: "string",
    organisation: {
      category: "RegisteredCompany",
      name: "string",
      insolvencyDate: "2025-02-11T20:45:17.453Z",
      registeredLegalForm: "string",
      lawRegistered: "string",
      controlCondition: ["None"],
      organisationId: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    individualOrTrust: {
      category: "PersonWithSignificantControlForIndividual",
      firstName: "string",
      lastName: "string",
      dateOfBirth: "2025-02-11T20:45:17.453Z",
      nationality: "string",
      controlCondition: ["None"],
      connectedType: "Individual",
      personId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      residentCountry: "string"
    },
    addresses: [
      {
        streetAddress: "82 St. John’s Road",
        locality: "CHESTER",
        region: "Lancashire",
        postalCode: "CH43 7UR",
        countryName: "United Kingdom",
        country: "GB",
        type: "Registered"
      }
    ],
    registeredDate: "2025-02-11T20:45:17.453Z",
    registerName: "string",
    startDate: "2025-02-11T20:45:17.453Z",
    endDate: "2025-02-11T20:45:17.453Z"
  });

  // 3) Headers
  const headers = {
    accept: '*/*',                  // matches curl's accept: */*
    'Content-Type': 'application/json',
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  // 4) Make the POST request
  const res = http.post(url, payload, { headers });

  // 5) Check the response (assuming 200 or 201)
  check(res, {
    'POST /connected-entities is 204': (r) => r.status === 204,
    // or if your API returns 201:
    // 'POST /connected-entities is 201': (r) => r.status === 201,
  });

  return res;
}
