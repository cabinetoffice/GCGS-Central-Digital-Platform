import http from 'k6/http';
import { check, sleep } from 'k6';

// Default values
const DEFAULT_DOMAIN = 'staging.supplier.information.findatender.codatt.net';

// Load environment variables & ensure defaults work properly
const AUTH_TOKEN = __ENV.AUTH_TOKEN || '';  // Must be explicitly provided
const DURATION = __ENV.DURATION && __ENV.DURATION.trim() !== '' ? __ENV.DURATION : '30s';
const MAX_VUS = parseInt(__ENV.MAX_VUS, 10) || 50;
const RPS = parseInt(__ENV.RPS, 10) || 10;
const TARGET_DOMAIN = __ENV.TARGET_DOMAIN && __ENV.TARGET_DOMAIN.trim() !== '' ? __ENV.TARGET_DOMAIN : DEFAULT_DOMAIN;
const VUS = parseInt(__ENV.VUS, 10) || 10;

// Log environment variables for debugging
console.debug(`Using Environment Variables:`);
console.debug(`  - TARGET_DOMAIN: ${TARGET_DOMAIN}`);
console.debug(`  - AUTH_TOKEN: ${AUTH_TOKEN ? 'Provided' : 'Missing'}`);
console.debug(`  - DURATION: ${DURATION}`);
console.debug(`  - MAX_VUS: ${MAX_VUS}`);
console.debug(`  - RPS: ${RPS}`);
console.debug(`  - VUS: ${VUS}`);

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
