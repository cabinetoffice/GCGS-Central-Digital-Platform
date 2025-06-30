<?php
require __DIR__ . '/vendor/autoload.php';

use Aws\Ses\SesClient;
use Aws\Exception\AwsException;

header('Content-Type: application/json');

try {
    $client = new SesClient([
        'version' => '2010-12-01',
        'region'  => getenv('AWS_REGION') ?: 'eu-west-1',
    ]);

    $result = $client->sendEmail([
        'Source' => 'no-reply@' . getenv('MAIL_DOMAIN'),  // Pass MAIL_DOMAIN as env var in ECS
        'Destination' => [
            'ToAddresses' => ['ali.bahman@goaco.com'],
        ],
        'Message' => [
            'Subject' => ['Data' => 'Test SES Email'],
            'Body' => ['Text' => ['Data' => 'Hello! This is a SES test from the healthcheck service.']],
        ],
    ]);

    echo json_encode(['status' => 'success', 'messageId' => $result['MessageId']]);

} catch (AwsException $e) {
    echo json_encode(['status' => 'error', 'error' => $e->getAwsErrorMessage()]);
}
