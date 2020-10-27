<?php
// SOAP_CLIENT_BASEDIR - folder that contains the PHP Toolkit and your WSDL
// $USERNAME - variable that contains your Salesforce.com username (must be in the form of an email)
// $PASSWORD - variable that contains your Salesforce.ocm password
//define('E:\Force.com-Toolkit-for-PHP-master/soapclient');
//define('E:\Force.com-Toolkit-for-PHP-master/soapclient');
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceHeaderOptions.php');
require_once ('E:\Force.com-Toolkit-for-PHP-master/samples/userAuth.php');
set_time_limit(3600*24);
try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('E:\Force.com-Toolkit-for-PHP-master/wsdl.xml');
  $mylogin = $mySforceConnection->login('mounikamg11@salesforce.com','blacky99gm5i3hIGIz7jWPTNTh46Ua96p');

 $servername = "localhost";
$username = "root";
$password = "";
$db="users1";

// Create connection
$conn = new mysqli($servername, $username, $password);

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
} 
    $sql="SELECT firstname, lastname, phone, email, company, leadsource,street,city,state FROM leads LIMIT 0, 25";
	$stmt = mysqli_query( $conn, $sql );
	
	
	$rows = array();	
	$ids = array();
	while( $rows = mysqli_fetch_array($stmt) ) {     
	  $sObject =new stdclass();
		   $sObject->FirstName=$rows['firstname'];
		   $sObject->LastName=$rows['lastname'];
		   $sObject->Phone=$rows['phone']; //.','.$row['city'].','.$row['state'].','.$row['zipcode'];
		   $sObject->Email=$rows['email'];
		   $sObject->Company=$rows['company'];	
		   $sObject->LeadSource=$rows['leadsource'];
		   $sObject->Street=$rows['street'];
		   $sObject->City=$rows['city'];
		   $sObject->State=$rows['state'];
		   
		
		    //echo "**** Creating the following:\r\n";
			$createResponse = $mySforceConnection->create(array($sObject), 'Lead');
			
			foreach ($createResponse as $createResult) {
				print_r($createResult);
				array_push($ids, $createResult->id);
			}
	}
	
	//echo "**** Now for Delete:\r\n";
			//$deleteResult = $mySforceConnection->delete($ids);
			//print_r($deleteResult);

			//echo "**** Now for UnDelete:\r\n";
			//$deleteResult = $mySforceConnection->undelete($ids);
			//print_r($deleteResult);
	
	//mysql_free_stmt( $stmt);

} catch (Exception $e) {
  //echo $mySforceConnection->getLastRequest();
  echo $e->faultstring;  
}
echo "Data is uploaded successfully...";
?>