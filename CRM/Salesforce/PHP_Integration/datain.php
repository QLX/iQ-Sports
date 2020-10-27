<?php
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceHeaderOptions.php');
require_once ('E:\Force.com-Toolkit-for-PHP-master/samples/userAuth.php');
set_time_limit(3600*24);
try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('E:\Force.com-Toolkit-for-PHP-master/wsdl.xml');
  $mylogin = $mySforceConnection->login('mounikamg11@salesforce.com', 'blacky99gm5i3hIGIz7jWPTNTh46Ua96p');

  
$mysqli = new mysqli("localhost", "root", "", "users1");

/* check connection */
if ($mysqli->connect_errno) {
    printf("Connect failed: %s\n", $mysqli->connect_error);
    exit();
}
$query = "Select accountname,aphone,email,rating,employeesno from accounts LIMIT 5";
$result = $mysqli->query($query);
$row = array();
//$ids = array();	
while($row = mysqli_fetch_array($result,MYSQLI_ASSOC)){
	//echo $row;
$sObject =new stdclass();
		  $sObject->Name=$row['accountname'];
		   $sObject->Phone=$row['aphone']; //.','.$row['city'].','.$row['state'].','.$row['zipcode'];
		   $sObject->Email__c=$row['email'];
		   $sObject->Rating=$row['rating'];
		   $sObject->NumberOfEmployees=$row['employeesno'];	
		  
//printf ("%s (%s)\n", $row["accountname"], $row["aphone"]);
$createResponse = $mySforceConnection->create(array($sObject), 'account');
			
			foreach ($createResponse as $createResult) {
				print_r($createResult);
					//echo "records sent successfully";
			}
}
/* free result set */
$result->free();
} catch (Exception $e) {
  echo $mySforceConnection->getLastRequest();
  echo $e->faultstring;  
}
echo "Data is uploaded successfully...";
/* close connection */
$mysqli->close();

?>