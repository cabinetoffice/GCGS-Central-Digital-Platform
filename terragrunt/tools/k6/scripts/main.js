// scripts/main.js
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';
import { endpointRegistry } from './requests/index.js';

// Default values
const DEFAULT_DOMAIN = 'dev.supplier.information.findatender.codatt.net';

// Load environment variables & ensure defaults work properly
const AUTH_TOKEN = __ENV.AUTH_TOKEN || '';
const DURATION = __ENV.DURATION && __ENV.DURATION.trim() !== '' ? __ENV.DURATION : '30s';
const ENDPOINTS_STR = __ENV.ENDPOINTS || 'getOrgs'; // e.g. "getOrgs,postOrgs"
const MAX_VUS = parseInt(__ENV.MAX_VUS || 50, 10);
const RPS = parseInt(__ENV.RPS || 10, 10);
const TARGET_DOMAIN = (__ENV.TARGET_DOMAIN || DEFAULT_DOMAIN).trim();
const TRACE_ID = __ENV.TRACE_ID || `k6-${new Date().toISOString().replace(/[-:.TZ]/g, '')}-${Math.floor(Math.random() * 10000)}`;
const VUS = parseInt(__ENV.VUS || 10, 10);

// ===== SCENARIO FUNCTION =====
export function scenarioRouter() {
  const endpoints = ENDPOINTS_STR
      .split(',')
      .map((x) => x.trim())
      .filter(Boolean);

  for (const ep of endpoints) {
    const fn = endpointRegistry[ep];
    if (!fn) {
      console.error(`No endpoint logic found for key: "${ep}". Skipping.`);
      continue;
    }

    // Each endpoint function *should* return the HTTP response object.
    const res = fn({
      domain: TARGET_DOMAIN,
      token: AUTH_TOKEN,
    });

    // Now we can log or store the status code
    console.log(`Endpoint "${ep}" responded with status: ${res && res.status}`);
  }
}

// ===== K6 OPTIONS =====
export const options = {
  scenarios: {
    single_scenario: {
      executor: 'constant-arrival-rate',
      rate: RPS,
      timeUnit: '1s',
      duration: DURATION,
      preAllocatedVUs: VUS,
      maxVUs: MAX_VUS,
      exec: 'scenarioRouter', // string reference
    },
  },
};

// Pretty print test result and export as JSON
export function handleSummary(data) {
  console.log(`===== K6 Test Summary. Trace ID : ${TRACE_ID} =====`);
  console.log(`${TRACE_ID}: Using Environment Variables:`);
  console.log(`${TRACE_ID}:   - TARGET_DOMAIN: ${TARGET_DOMAIN}`);
  console.log(`${TRACE_ID}:   - AUTH_TOKEN: ${AUTH_TOKEN ? 'Provided' : 'Missing'}`);
  console.log(`${TRACE_ID}:   - DURATION: ${DURATION}`);
  console.log(`${TRACE_ID}:   - MAX_VUS: ${MAX_VUS}`);
  console.log(`${TRACE_ID}:   - RPS: ${RPS}`);
  console.log(`${TRACE_ID}:   - VUS: ${VUS}`);

  const summaryWithTraceId = {
    trace_id: TRACE_ID,
    timestamp: new Date().toISOString(),
    environment: {
      target_domain: TARGET_DOMAIN,
      duration: DURATION,
      max_vus: MAX_VUS,
      rps: RPS,
      vus: VUS,
    },
    test_results: data // ✅ Includes original test results
  };

  console.log(summaryWithTraceId);
  return {
    "stdout": textSummary(data, { indent: " ", enableColors: true }),
    //   "k6-results.json": JSON.stringify(summaryWithTraceId, null, 2),
  };
}
