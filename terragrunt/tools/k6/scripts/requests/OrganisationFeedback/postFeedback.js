// scripts/endpoints/postFeedback.js
import http from 'k6/http';
import { check } from 'k6';

/**
 * POST /feeback/feedback
 *
 * @param {object} opts
 *   - token  (string)  Bearer token for the Authorization header
 *   - domain (string)  e.g. "staging.supplier.information.findatender.codatt.net"
 */
export function postFeedback({ token, domain }) {
  // 1) Construct the URL
  const url = `https://organisation.${domain}/feeback/feedback`;

  // 2) Build the payload exactly as in the curl -d
  const payload = JSON.stringify({
    feedbackAbout: "string",
    specificPage: "string",
    feedback: "string",
    name: "string",
    email: "string",
    subject: "string",
  });

  // 3) Headers
  const headers = {
    accept: 'application/json',
    'Content-Type': 'application/json',
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  const params = { headers };

  // 4) Make the POST request
  const res = http.post(url, payload, params);

  // 5) Check the response status (assuming 200 OK)
  check(res, {
    'POST feedback status is 200': (r) => r.status === 200,
  });

  return res;
}
