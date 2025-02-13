// scripts/endpoints/verifySharedData.js
import http from 'k6/http';
import { check } from 'k6';
import { SHARE_CODE } from '../config.js';

/**
 * POST /share/data/verify
 *
 * @param {object} opts
 *   - domain         (string) e.g. "staging.supplier.information.findatender.codatt.net"
 *   - apiKey         (string) for 'CDP-Api-Key' header
 *   - shareCode      (string) fallback to SHARE_CODE from config if not given
 *   - formVersionId  (string) defaults to "1.0"
 */
export function verifySharedData({
  domain,
  apiKey = 'aa44bbfc-b5a3-47f5-8cc4-7343edaf8664', // default if not passed in
  shareCode,
  formVersionId = '1.0',
}) {
  // If shareCode not provided, use config's SHARE_CODE
  const finalShareCode = shareCode || SHARE_CODE;

  // Build the request URL
  const url = `https://data-sharing.${domain}/share/data/verify`;

  // JSON body
  const payload = JSON.stringify({
    shareCode: finalShareCode,
    formVersionId,
  });

  // Prepare request headers
  const params = {
    headers: {
      accept: 'application/json',
      'Content-Type': 'application/json',
      'CDP-Api-Key': apiKey,
    },
  };

  // Make the POST request
  const res = http.post(url, payload, params);

  // Check success (200 or 201)
  check(res, {
    'POST /share/data/verify is 200 or 201': (r) => r.status === 200 || r.status === 201,
  });

  return res;
}
