// perf-test/endpoints/getOrganisations.js
import http from 'k6/http';
import { check } from 'k6';

export function getOrganisations({ token, domain }) {
  const url = `https://organisation.${domain}/organisation/lookup?name=test123`;

  const params = {
    headers: {
      accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  const res = http.get(url, params);

  check(res, {
    'GET status is 200': (r) => r.status === 200,
  });

  return res;
}
