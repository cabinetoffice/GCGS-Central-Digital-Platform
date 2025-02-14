// scripts/endpoints/postOrganisationApiKeys.js
import http from 'k6/http';
import { check } from 'k6';
import { ORG_GUID } from '../config.js';
import { uuidv4 } from '../lib/uuid.js';



export function postOrganisationApiKeys({ token, domain }) {

  const guid = uuidv4();

  // 1) Build the request URL
  const url = `https://organisation.${domain}/organisations/${ORG_GUID}/api-keys`;

  // 2) JSON payload (from your curl -d)
  const payload = JSON.stringify({
    name: `Test_${guid}`,
    key: "string",
    organisationId: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  });

  // 3) Headers (mirroring your curl)
  const headers = {
    accept: '*/*',                  // from curl: -H 'accept: */*'
    'Content-Type': 'application/json'
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  // 4) Make the POST request
  const res = http.post(url, payload, { headers });

  // 5) Check response status. Often a 201 means "Created," but
  //    some APIs might respond with 200. Adjust if needed.
  check(res, {
    'POST create API key status is 201': (r) => r.status === 201,
  });

  return res;
}
