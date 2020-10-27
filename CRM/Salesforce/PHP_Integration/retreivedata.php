<!--<html>
<body>
<table style="border:2px solid black;">
        <thead>
            <th>Name</th>
            <th>Phone</th>
            <th>Email</th>
            <th>Company</th>
			 <th>LeadSource</th>
            <th>Street</th>
            <th>City</th>
            <th>State</th>
         
        </thead>
        <tbody>-->
		<?php
// SOAP_CLIENT_BASEDIR - folder that contains the PHP Toolkit and your WSDL
// $USERNAME - variable that contains your Salesforce.com username (must be in the form of an email)
// $PASSWORD - variable that contains your Salesforce.ocm password

define('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient');
require_once ('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/soapclient/SforceEnterpriseClient.php');

try {
  $mySforceConnection = new SforceEnterpriseClient();
  $mySoapClient = $mySforceConnection->createConnection('C:\wamp64\www\Force.com-Toolkit-for-PHP-master/wsdl.xml');
  $mylogin = $mySforceConnection->login('nayeemuddin.shaik-2mru@force.com','Qualex1995!wNZKalEdp45wk2z9474br1nP');
  
  $query = 'SELECT Id,Name,Phone,Email,Company,LeadSource,Street,City,State from Lead';
  $response = $mySforceConnection->query(($query));
 //$index = 0;
  foreach ($response->records as $record) {
    /*echo '<tr>
	<td>'.$record->Id.'</td>
	<td>'.$record->fields->Name.'</td>
	<td>'.$record->fields->Phone.'</td>
	<td>'.$record->fields->Email.'</td>
	<td>'.$record->fields->Company.'</td>
	<td>'.$record->fields->LeadSource.'</td>
	<td>'.$record->fields->Street.'</td>
	<td>'.$record->fields->City.'</td>
	<td>'.$record->fields->State.'</td>
	
	 </tr>';
	 $index++; */
	 // print_r($record);
header('Content-Type: application/json');
echo json_encode($response);
  }
} catch (Exception $e) {
  echo $e->faultstring;
}

?>
</tbody>
    </table>
</body>
</html>
