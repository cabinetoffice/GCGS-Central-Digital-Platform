<?php
require __DIR__ . '/vendor/autoload.php';

use Aws\SesV2\SesV2Client;
use Aws\Exception\AwsException;

$region          = getenv('AWS_REGION') ?: 'eu-west-2';
$mailDomain      = getenv('MAIL_DOMAIN') ?: 'example.com';
$envConfigSet    = trim((string) getenv('SES_CONFIGURATION_SET_NAME'));
$defaultFrom     = 'no-reply@' . $mailDomain;
$defaultTo       = 'success@simulator.amazonses.com'; // good for delivery tests

$method = $_SERVER['REQUEST_METHOD'] ?? 'GET';
$debug  = [];
$result = null;
$error  = null;

if ($method === 'POST') {
    $from        = trim($_POST['from'] ?? $defaultFrom);
    $to          = trim($_POST['to'] ?? $defaultTo);
    $subject     = (string)($_POST['subject'] ?? 'FTS SES test');
    $body        = (string)($_POST['body'] ?? 'Hello from the FTS healthcheck service.');
    $configSet   = trim((string)($_POST['config_set'] ?? $envConfigSet));

    $client = new SesV2Client([
        'version' => 'latest',
        'region'  => $region,
    ]);

    $send = [
        'FromEmailAddress' => $from,
        'Destination'      => ['ToAddresses' => [$to]],
        'Content'          => [
            'Simple' => [
                'Subject' => ['Data' => $subject],
                'Body'    => ['Text' => ['Data' => $body]],
            ],
        ],
    ];
    if ($configSet !== '') {
        $send['ConfigurationSetName'] = $configSet;
    }

    $debug = [
        'region'                 => $region,
        'from'                   => $from,
        'to'                     => $to,
        'config_set_used'        => $configSet !== '' ? $configSet : '(none)',
        'env_SES_CONFIGURATION_SET_NAME' => $envConfigSet !== '' ? $envConfigSet : '(unset)',
        'env_AWS_REGION'         => getenv('AWS_REGION') ?: '(unset)',
        'env_MAIL_DOMAIN'        => $mailDomain,
    ];

    try {
        $result = $client->sendEmail($send);
    } catch (AwsException $e) {
        $error = [
            'aws_error_code'    => $e->getAwsErrorCode(),
            'aws_error_message' => $e->getAwsErrorMessage(),
            'message'           => $e->getMessage(),
        ];
    }
}
?>
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <title>SES Test</title>
    <style>
        body { font-family: system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif; margin: 2rem; background:#f7f7f7; color:#333; }
        h1 { color:#0078d7; }
        form { background:#fff; padding:1rem; border-radius:8px; box-shadow: 0 2px 6px rgba(0,0,0,.06); max-width: 800px; }
        label { display:block; font-weight:600; margin-top:.75rem; }
        input[type=text], textarea { width:100%; padding:.6rem; border:1px solid #ccc; border-radius:6px; margin-top:.25rem; }
        textarea { min-height: 120px; }
        .row { display:grid; grid-template-columns: 1fr 1fr; gap:1rem; }
        .actions { margin-top:1rem; }
        button { padding:.6rem 1rem; border:0; border-radius:6px; background:#0078d7; color:#fff; cursor:pointer; }
        pre { background:#111; color:#eee; padding:1rem; border-radius:8px; overflow:auto; max-width: 980px; }
        a { color:#0078d7; }
    </style>
</head>
<body>
<h1>SES Test</h1>
<p>This page sends a test email via <strong>SESv2</strong> and shows the inputs and the raw AWS response. Use the simulator address
    (<code>success@simulator.amazonses.com</code>) to verify delivery events without sending a real email.</p>

<form method="post">
    <div class="row">
        <div>
            <label>From</label>
            <input type="text" name="from" value="<?php echo htmlspecialchars($defaultFrom, ENT_QUOTES); ?>">
        </div>
        <div>
            <label>To</label>
            <input type="text" name="to" value="<?php echo htmlspecialchars($defaultTo, ENT_QUOTES); ?>">
        </div>
    </div>
    <div class="row">
        <div>
            <label>Subject</label>
            <input type="text" name="subject" value="FTS SES test">
        </div>
        <div>
            <label>Configuration set (optional)</label>
            <input type="text" name="config_set" placeholder="cdp-sirsi-ses-logging-config-set" value="<?php echo htmlspecialchars($envConfigSet, ENT_QUOTES); ?>">
        </div>
    </div>
    <label>Body</label>
    <textarea name="body">Hello from the FTS healthcheck service.</textarea>
    <div class="actions">
        <button type="submit">Send test email</button>
        <a href="index.php" style="margin-left:.75rem;">Back</a>
    </div>
</form>

<?php if ($method === 'POST'): ?>
    <h2>Debug</h2>
    <pre><?php echo htmlspecialchars(json_encode($debug, JSON_PRETTY_PRINT), ENT_QUOTES); ?></pre>

    <?php if ($result): ?>
        <h2>SES Response</h2>
        <pre><?php echo htmlspecialchars(json_encode($result->toArray(), JSON_PRETTY_PRINT), ENT_QUOTES); ?></pre>
    <?php endif; ?>

    <?php if ($error): ?>
        <h2>Error</h2>
        <pre><?php echo htmlspecialchars(json_encode($error, JSON_PRETTY_PRINT), ENT_QUOTES); ?></pre>
    <?php endif; ?>
<?php endif; ?>

<h3>Notes</h3>
<ul>
    <li>Region used: <strong><?php echo htmlspecialchars($region, ENT_QUOTES); ?></strong></li>
    <li>Set <code>SES_CONFIGURATION_SET_NAME</code> in the service/container to attach your logging config set without editing this page.</li>
    <li>To test end‑to‑end logging, send to the simulator and then check SQS and CloudWatch Logs.</li>
</ul>
</body>
</html>
