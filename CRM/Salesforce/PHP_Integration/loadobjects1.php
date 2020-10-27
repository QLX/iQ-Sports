<?php
//define('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient');
require_once ('Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');


$mySforceConnection = new SforceEnterpriseClient();
$mySoapClient = $mySforceConnection->createConnection('Force.com-Toolkit-for-PHP-master/wsdl.xml');
$mylogin = $mySforceConnection->login('nayeemuddin.shaik-2mru@force.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');
$result = $mySforceConnection->describeGlobal(); // Method to fetch all sobjects from salesforce

$SFobj = "Contact";
$result = $mySforceConnection->describeSObject($SFobj);
$str="<table border='1' width='90%'>";
foreach($result->fields as $key => $val) {
     $str=$str."<tr><td>".$val->name."</td><td>".$val->type."</td><td>".$val->length."</td><td>".$val->scale."</td><tr>";
}
  $str=$str."</table>";
echo $str;
?>
