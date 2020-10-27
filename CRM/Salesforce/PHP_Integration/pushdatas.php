<?php
// SOAP_CLIENT_BASEDIR - folder that contains the PHP Toolkit and your WSDL
// $USERNAME - variable that contains your Salesforce.com username (must be in the form of an email)
// $PASSWORD - variable that contains your Salesforce.ocm password
//define('D:\PHP Tool Kit\PHP Tool Kit/soapclient');
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');
require_once ('E:\Force.com-Toolkit-for-PHP-master/soapclient/SforceHeaderOptions.php');
require_once ('E:\Force.com-Toolkit-for-PHP-master/samples/userAuth.php');
set_time_limit(3600*24);
try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('E:\Force.com-Toolkit-for-PHP-master/wsdl.xml');
  $mylogin = $mySforceConnection->login('nayeemuddin.shaik-2mru@force.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');

 /* $serverName = "vdev003.qlx.com";
$connectionInfo = array( "Database"=>"iQSports_V4", "UID"=>"sa", "PWD"=>"Carito33156");
$conn = sqlsrv_connect( $serverName, $connectionInfo );
if( $conn === false ) {
    die( print_r( sqlsrv_errors(), true));
}*/

mysql_connect("localhost", "root", "") or
    die("Could not connect: " . mysql_error());
mysql_select_db("users1");

    $sql=mysql_query("Select accountname,aphone,rating,employeesno from accounts");
	echo $sql;

	$row = array();	
			//   echo $rows;
	//$ids = array();
	//echo $rows;
	//while( $row = mysql_fetch_array( $stmt, mysql_FETCH_ASSOC) ) {    
while ($row = mysql_fetch_array($sql, mysql_BOTH))	{
	  $sObject =new stdclass();
		  $sObject->Name=$row['accountname'];
		   $sObject->Phone=$row['aphone']; //.','.$row['city'].','.$row['state'].','.$row['zipcode'];
		   $sObject->Rating=$row['rating'];
		   $sObject->NumberOfEmployees=$row['employeesno'];	

		   //$sObject->LeadSource=$row['leadsource'];
		   //$sObject->Street=$row['street'];
		   //$sObject->City=$row['city'];
		    // $sObject->State=$row['state'];
		    //echo "**** Creating the following:\r\n";
			$createResponse = $mySforceConnection->create(array($sObject), 'account');
			
			foreach ($createResponse as $createResult) {
				print_r($createResult);
					echo "records sent successfully";
				array_push($ids, $createResult->id);
			}
	}
	
	//echo "**** Now for Delete:\r\n";
		//	$deleteResult = $mySforceConnection->delete($ids);
			//print_r($deleteResult);

			//echo "**** Now for UnDelete:\r\n";
	//		$deleteResult = $mySforceConnection->undelete($ids);
		//	print_r($deleteResult);
	
	mysql_free_result( $sql);

} catch (Exception $e) {
  echo $mySforceConnection->getLastRequest();
  echo $e->faultstring;  
}
echo "Data is uploaded successfully...";
?>
