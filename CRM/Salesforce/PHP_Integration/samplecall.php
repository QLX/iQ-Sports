<?php
// SOAP_CLIENT_BASEDIR - folder that contains the PHP Toolkit and your WSDL
// $USERNAME - variable that contains your Salesforce.com username (must be in the form of an email)
// $PASSWORD - variable that contains your Salesforce.ocm password

define('E:\Force.com-Toolkit-for-PHP-master/soapclient');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient/SforceHeaderOptions.php');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/samples/userAuth.php');

try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/wsdl.xml');
  $mylogin = $mySforceConnection->login('nayeemuddin.shaik-2mru@force.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');

  $sObject = new stdclass();
  $sObject->FirstName = 'gowthami';
  $sObject->LastName = 'Lnkjn';
  $sObject->Phone = '8901234567';
  $sObject->Company	 = 'Softsol';
	 $sObject->Status  = 'Open - Not Contacted';
  
  
 // $sObject2->customfield__c = 'ABC Company';


  $sObject2 = new stdclass();
  $sObject2->FirstName = 'Leena';
  $sObject2->LastName = 'cnj';
  $sObject2->Phone = '7890123567';
 $sObject2->Company = 'Qualcomm';
	 $sObject2->Status= 'Working - Contacted';
  
 // $sObject2->customfield__c = 'XYZ Company';

  echo "**** Creating the following:\r\n";
  $createResponse = $mySforceConnection->create(array($sObject, $sObject2), 'Lead');

  $ids = array();
  foreach ($createResponse as $createResult) {
    print_r($createResult);
    array_push($ids, $createResult->id);
  }
  //echo "**** Now for Delete:\r\n";
//  $deleteResult = $mySforceConnection->delete($ids);
 // print_r($deleteResult);

 // echo "**** Now for UnDelete:\r\n";
 // $deleteResult = $mySforceConnection->undelete($ids);
 // print_r($deleteResult);

} catch (Exception $e) {
  echo $mySforceConnection->getLastRequest();
  echo $e->faultstring;
}
?>