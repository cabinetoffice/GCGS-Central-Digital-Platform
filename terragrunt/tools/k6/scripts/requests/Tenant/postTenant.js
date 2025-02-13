// scripts/endpoints/postTenant.js
import http from 'k6/http';
import { check } from 'k6';
import { uuidv4 } from '../lib/uuid.js';

/**
 * POST /tenants
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function postTenant({ token, domain }) {

  const guid = uuidv4();

  // 1) Construct the URL
  const url = `https://api.${domain}/tenants`;

  // 2) Build the JSON payload
  const payload = JSON.stringify({
    name: `Test_${guid}`,
  });

  // 3) Set headers
  const headers = {
    accept: 'application/json',
    'Content-Type': 'application/json'
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  // 4) Make the POST request
  const res = http.post(url, payload, { headers });

  // 5) Check the response status
  //    (If the API responds 201 on successful creation, we check for that)
  check(res, {
    'POST create tenant is 201': (r) => r.status === 201,
  });

  return res;
}