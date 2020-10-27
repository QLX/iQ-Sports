<?php  
/* Connect to the local server using Windows Authentication and  
specify the AdventureWorks database as the database in use. */  
	$serverName = "localhost"; //serverName\instanceName
	$connectionInfo = array( "Database"=>"CasinoApp", "UID"=>"sa", "PWD"=>"Carito33156"); 
$conn = sqlsrv_connect( $serverName, $connectionInfo);  
if( $conn === false )  
{  
     echo "Could not connect.\n";  
     die( print_r( sqlsrv_errors(), true));  
}  
  
/* Prepare the statement. */  
    $str="Select * from CasinoList";
	$stmt = sqlsrv_query( $conn, $str );
	if( $stmt === false) {
    die( print_r( sqlsrv_errors(), true) );
	}
	
	foreach( sqlsrv_field_metadata( $stmt) as $fieldMetadata)  
	{ 
       echo $fieldMetadata["Name"]." ==> ";	
      
      echo "<br>";  
}
	
	
	sqlsrv_free_stmt( $stmt);
/* Note: sqlsrv_field_metadata can be called on any statement  
resource, pre- or post-execution. */  
  
/* Free statement and connection resources. */  

sqlsrv_close( $conn);  
?>  