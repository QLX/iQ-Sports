<?php
require_once('Force.com-Toolkit-for-PHP-master/soapclient/SforcePartnerClient.php');
require_once('Force.com-Toolkit-for-PHP-master/soapclient/SforceMetadataClient.php');


    $mySforceConnection = new SforcePartnerClient();
    $mySoapClient = $mySforceConnection->createConnection('Force.com-Toolkit-for-PHP-master/soapclient/partner.wsdl.xml');
    $loginResult = $mySforceConnection->login('nayeemuddin.shaik@qlx.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');
    
    $myMetadataConnection = new SforceMetadataClient('Force.com-Toolkit-for-PHP-master/soapclient./metadata.wsdl.xml', $loginResult, $mySforceConnection);

try { 

//Create object
    $customObject = new SforceCustomObject();
    $customObject->fullName = 'TestObject__c';
    $customObject->deploymentStatus = DEPLOYMENT_STATUS_DEPLOYED;

    $customObject->setDescription("A description");
    $customObject->setEnableActivities(true);
    $customObject->setEnableDivisions(false);
    $customObject->setEnableHistory(true);
    $customObject->setEnableReports(true);
    $customObject->setHousehold(false);
    $customObject->setLabel("My Custom Obj from PHP");
	
    $customField = new SforceCustomField();
    $customField->setFullName('RegdNo__c');
    $customField->setDescription('Registration Id');
    $customField->setLabel('Regd No');
    $customField->setType('Text');
	
	$customField1 = new SforceCustomField();
    $customField1->setFullName('StaffName__c');
    $customField1->setDescription('Name of the Staff');
    $customField1->setLabel('Staff Name');
    $customField1->setType('Text');
        
    $customObject->nameField = $customField;
	$customObject->nameField = $customField1;
    
    $customObject->pluralLabel = 'Test Objects';
    $customObject->sharingModel = SHARING_MODEL_READWRITE;
    print_r($myMetadataConnection->create($customObject));

 

} catch (Exception $e) {
    echo $myMetadataConnection->getLastRequest();
    echo $e->faultstring;
}

?>