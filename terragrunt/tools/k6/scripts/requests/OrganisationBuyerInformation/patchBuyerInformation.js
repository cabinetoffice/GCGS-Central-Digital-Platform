// scripts/endpoints/patchBuyerInformation.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js'; 
/**
 * PATCH /organisations/{guid}/buyer-information
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for the Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function patchBuyerInformation({ token, domain }) {
  // 1) Instead of hardcoding, we import `ORG_GUID` from config
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/buyer-information`;

  // 2) JSON payload
  const payload = JSON.stringify({
    type: "BuyerOrganisationType",
    buyerInformation: {
      buyerType: "string",
      devolvedRegulations: [
        "NorthernIreland"
      ],
    },
  });

  // 3) Headers
  const headers = {
    accept: '*/*',
    'Content-Type': 'application/json',
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  // 4) Make the PATCH request
  const res = http.request('PATCH', url, payload, { headers });

  // 5) Check the status
  check(res, {
    'PATCH buyer-information is 204': (r) => r.status === 204,
  });

  return res;
}
