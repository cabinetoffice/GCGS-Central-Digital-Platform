// scripts/endpoints/patchOrganisationSupplierInformation.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';

/**
 * PATCH /organisations/{ORG_GUID}/supplier-information
 *
 * @param {object} opts
 *   - token      (string)  Bearer token for Authorization header
 *   - domain     (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 *   - orgGuid    (string)  optionally override the default ORG_GUID
 *   - payload    (object|string) the JSON body for the PATCH
 *       If not provided, we'll default to the sample from your curl:
 *         {
 *           "type": "SupplierType",
 *           "supplierInformation": {
 *             "supplierType": "Organisation",
 *             "legalForm": {
 *               "registeredUnderAct2006": true,
 *               "registeredLegalForm": "string",
 *               "lawRegistered": "string",
 *               "registrationDate": "2025-02-12T00:54:59.475Z"
 *             },
 *             "operationTypes": [
 *               "SmallOrMediumSized"
 *             ]
 *           }
 *         }
 */
export function patchOrganisationSupplierInformation({
  token,
  domain,
  orgGuid,
  payload,
}) {
  // Fallback to config's ORG_GUID if none passed
  const finalOrgGuid = orgGuid || ORG_GUID;

  // Build the request URL
  const url = `https://organisation.${domain}/organisations/${finalOrgGuid}/supplier-information`;

  // Prepare request headers
  const params = {
    headers: {
      accept: '*/*',
      'Content-Type': 'application/json',
    },
  };

  // Add Authorization header if token provided
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  // Default to the sample body if no custom payload is provided
  let body = payload;
  if (!body) {
    body = {
      type: 'SupplierType',
      supplierInformation: {
        supplierType: 'Organisation',
        legalForm: {
          registeredUnderAct2006: true,
          registeredLegalForm: 'string',
          lawRegistered: 'string',
          registrationDate: '2025-02-12T00:54:59.475Z',
        },
        operationTypes: ['SmallOrMediumSized'],
      },
    };
  }

  // If it's an object, serialize to JSON
  if (typeof body === 'object') {
    body = JSON.stringify(body);
  }

  // Make the PATCH request
  const res = http.patch(url, body, params);

  // Typically a 200 or 204 is returned for successful PATCH,
  // but let's assume 200 here:
  check(res, {
    'PATCH supplier-information is 204': (r) => r.status === 204,
  });

  return res;
}
