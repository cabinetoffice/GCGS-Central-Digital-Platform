// perf-test/endpoints/getOrganisations.js
import http from 'k6/http';
import { check } from 'k6';

export function getOrganisations({ token, domain }) {
  const url = `https://organisation.${domain}/organisation/lookup?name=test123`;

  const params = {
    // Add a name tag so k6 can group metrics by this endpoint
    tags: { name: 'getOrganisations' },
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Perform the GET request with the name tag
  const res = http.get(url, params);

  // Attach the same "name" tag to the check
  check(
    res,
    {
      'GET status is 200': (r) => r.status === 200,
    },
    { name: 'getOrganisations' }
  );

  return res;
}
