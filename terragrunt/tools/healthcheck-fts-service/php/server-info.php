<?php
header("Content-Type: text/plain");

echo "==========================\n";
echo "   FTS Health Check\n";
echo "==========================\n\n";

// Database check
echo "[ Database Status ]\n";

$host = getenv('DB_HOST');
$user = getenv('DB_USER');
$pass = getenv('DB_PASS');
$dbname = getenv('DB_NAME');

echo "DB host:          " . ($host ?: "<unset>") . "\n";
echo "DB name:          " . ($dbname ?: "<unset>") . "\n\n";

$conn = new mysqli($host, $user, $pass, $dbname);

if ($conn->connect_error) {
    http_response_code(500);
    echo "Connection failed: " . $conn->connect_error . "\n";
} else {
    $result = $conn->query("SELECT VERSION() as version");
    $row = $result->fetch_assoc();
    echo "Connection: OK\n";
    echo "MySQL Version: " . $row['version'] . "\n";

    $result2 = $conn->query("SELECT @@hostname AS server_hostname, @@port AS server_port");
    if ($result2) {
        $row2 = $result2->fetch_assoc();
        echo "Server hostname: " . $row2["server_hostname"] . "\n";
        echo "Server port:     " . $row2["server_port"] . "\n";
    }
    $conn->close();
}

echo "\n[ PHP Environment ]\n";
echo "PHP Version: " . phpversion() . "\n";
echo "Loaded Extensions:\n";

$extensions = get_loaded_extensions();
sort($extensions);
foreach ($extensions as $ext) {
    echo "- $ext\n";
}
