import http from 'k6/http';
import { check, sleep } from 'k6';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// Default values
const DEFAULT_DOMAIN = 'staging.supplier.information.findatender.codatt.net';

// Load environment variables & ensure defaults work properly
const TRACE_ID = __ENV.TRACE_ID || `k6-${new Date().toISOString().replace(/[-:.TZ]/g, '')}-${Math.floor(Math.random() * 10000)}`;
const AUTH_TOKEN = __ENV.AUTH_TOKEN || '';
const DURATION = __ENV.DURATION && __ENV.DURATION.trim() !== '' ? __ENV.DURATION : '30s';
const MAX_VUS = parseInt(__ENV.MAX_VUS, 10) || 50;
const RPS = parseInt(__ENV.RPS, 10) || 10;
const TARGET_DOMAIN = __ENV.TARGET_DOMAIN && __ENV.TARGET_DOMAIN.trim() !== '' ? __ENV.TARGET_DOMAIN : DEFAULT_DOMAIN;
const VUS = parseInt(__ENV.VUS, 10) || 10;

export const options = {
    scenarios: {
        example_scenario: {
            executor: 'constant-arrival-rate',
            rate: RPS,               // RPS
            timeUnit: '1s',          // RPS is per 1 second
            duration: DURATION,      // How long the test should run
            preAllocatedVUs: VUS,    // Number of VUs to pre-allocate
            maxVUs: MAX_VUS,         // Maximum VUs up to which k6 can scale
        },
    },
};

// K6 Test Logic
export default function () {
    // Construct the URL safely
    const url = `https://organisation.${TARGET_DOMAIN}/organisation/lookup?name=test123`;

    // Ensure token exists before sending
    if (!AUTH_TOKEN) {
        console.error("AUTH_TOKEN is missing! Test will fail.");
        return;
    }

    // Request Headers
    const params = {
        headers: {
            accept: 'application/json',
            Authorization: `Bearer ${AUTH_TOKEN}`,
        },
    };

    // Make GET request
    const res = http.get(url, params);

    // Basic check
    check(res, {
        'status is 200': (r) => r.status === 200,
    });

    // Small sleep to help visualize iteration pacing (optional)
    sleep(1);
}

// Export test results as JSON
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

    console.log(JSON.stringify(summaryWithTraceId, null, 2));
    return {
        "stdout": textSummary(data, { indent: " ", enableColors: true }),
        "k6-results.json": JSON.stringify(summaryWithTraceId, null, 2),
    };
}
