<?php
//define('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient');
require_once ('Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');


$mySforceConnection = new SforceEnterpriseClient();
$mySoapClient = $mySforceConnection->createConnection('Force.com-Toolkit-for-PHP-master/wsdl.xml');
$mylogin = $mySforceConnection->login('nayeemuddin.shaik-2mru@force.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');
$result = $mySforceConnection->describeGlobal(); // Method to fetch all sobjects from salesforce
$str="<option value='--Select--'>--Select--</option>";
foreach($result->sobjects as $key => $val) {
$oname=$val->name;
     $str=$str."<option value='".$oname."'>".$oname."</option>";
}

echo $str;
?>
