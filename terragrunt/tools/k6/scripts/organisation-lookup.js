import http from 'k6/http';
import {check, sleep} from 'k6';

// Load test configuration from environment variables
// If an environment variable isn't set, it will use the fallback default.
const DEFAULT_DOMAIN = 'staging.supplier.information.findatender.codatt.net'
const AUTH_TOKEN = __ENV.AUTH_TOKEN;                            // Auth token (no default, must be provided)
const DURATION = __ENV.DURATION || '30s';                       // Total test duration (default 30s)
const MAX_VUS = parseInt(__ENV.MAX_VUS) || 50;          	    // Max VUs to allow if needed (default 50)
const RPS = parseInt(__ENV.RPS) || 10;                  	    // Requests per second (default 10)
const TARGET_DOMAIN = __ENV.TARGET_DOMAIN || DEFAULT_DOMAIN;    // Target domain
const VUS = parseInt(__ENV.VUS) || 10;                  	    // Pre-allocated VUs for the scenario (default 10)

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
    // Endpoint
    const url = `https://organisation.${TARGET_DOMAIN}/organisation/lookup?name=test123`;

    // Request headers
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
