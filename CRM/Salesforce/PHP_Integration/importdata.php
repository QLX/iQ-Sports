<?php include("connect.php"); ?>
<?php
// SOAP_CLIENT_BASEDIR - folder that contains the PHP Toolkit and your WSDL
// $USERNAME - variable that contains your Salesforce.com username (must be in the form of an email)
// $PASSWORD - variable that contains your Salesforce.ocm password
//define('D:\PHP Tool Kit\PHP Tool Kit/soapclient');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient/SforceHeaderOptions.php');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/samples/userAuth.php');
set_time_limit(3600*24);
try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/wsdl.xml');
  $mylogin = $mySforceConnection->login('nayeemuddin.shaik-2mru@force.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');

  $database=$_REQUEST["database"];
  $tablename=$_REQUEST["tablename"];
  $mappedcolumns=$_REQUEST["mappedcolumns"];
  $sfobject=$_REQUEST["sfobject"];
  
  $serverName = "localhost";
  $connectionInfo = array( "Database"=>$database, "UID"=>"sa", "PWD"=>"Carito33156");
  $conn = sqlsrv_connect( $serverName, $connectionInfo );
  if( $conn === false ) {
    die( print_r( sqlsrv_errors(), true)); 
  }

  $columns=explode(",",$mappedcolumns);
  $sffields=array();
  $tfields=array();
  $tablefields="";
  //echo count($columns);
  for($i=0;$i<count($columns);$i=$i+1)
   {
       echo $columns[$i];   
       $cols=explode(":",$columns[$i]);
	   $tablefields=$tablefields.$cols[0].",";
	   $tfields[$i]=$cols[0];
	   $sffields[$i]=$cols[1];
   }
   
  $tablefields=substr($tablefields,0,strlen($tablefields)-1);
  $sql="Select ".$tablefields." from ".$tablename;
  //echo $sql;
  $stmt = sqlsrv_query( $conn, $sql );

  //$row = array();	
  while( $row = sqlsrv_fetch_array( $stmt, SQLSRV_FETCH_ASSOC))	{
	  $sObject =new stdclass();
	  /*		  $sObject->FirstName=$row['First_Name'];
		  $sObject->LastName=$row['Last_Name'];
		   $sObject->Phone=$row['Mobile_Number']; 
		   $sObject->Email=$row['Email_Address'];
		   $sObject->Company="Qualex";
		   $sObject->Status="New";*/
		   
		   
        for($j=0;$j<count($tfields);$j=$j+1)
		 {
		     //echo $sffields[$j]." ==> ".$tfields[$j]." ==> ".$row[trim($tfields[$j])];
			 //$field=$tfields[$j];		
			 $sffield=trim($sffields[$j]);
			 echo $sffield;
		     $sObject->$sffield=$row[trim($tfields[$j])];
		 }
		  
		  //echo "**** Creating the following:\r\n";
			$createResponse = $mySforceConnection->create(array($sObject), $sfobject);						
	}
	echo "Data is uploaded successfully...";
	//echo "**** Now for Delete:\r\n";
		//	$deleteResult = $mySforceConnection->delete($ids);
			//print_r($deleteResult);

			//echo "**** Now for UnDelete:\r\n";
	//		$deleteResult = $mySforceConnection->undelete($ids);
		//	print_r($deleteResult);
	
	//mssql_free_result( $sql);

} catch (Exception $e) {
  echo $mySforceConnection->getLastRequest();
  echo $e->faultstring;  
  //echo "Data is uploaded successfully...";
}

?>
