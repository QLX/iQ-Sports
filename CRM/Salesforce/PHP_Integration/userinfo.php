<?php
// SOAP_CLIENT_BASEDIR - folder that contains the PHP Toolkit and your WSDL
// $USERNAME - variable that contains your Salesforce.com username (must be in the form of an email)
// $PASSWORD - variable that contains your Salesforce.ocm password

define('E:\Force.com-Toolkit-for-PHP-master/soapclient');
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');

try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');
  $mylogin = $mySforceConnection->login('mounikamg11@salesforce.com', 'blacky99gm5i3hIGIz7jWPTNTh46Ua96p');

  echo "***** Get User Info*****\n";
  $response = $mySforceConnection->getUserInfo();
  print_r($response);
} catch (Exception $e) {
  echo $mySforceConnection->getLastRequest();
  echo $e->faultstring;
}
?>