<?php include("config.php"); ?>
<?php
    $dbname=$_REQUEST["dbname"];
	$tablename=$_REQUEST["tablename"];

	
	$connectionInfo = array( "Database"=>$dbname, "UID"=>$userid, "PWD"=>$password);
	$conn = sqlsrv_connect( $serverName, $connectionInfo);

	if( !$conn ) 
     {
     //echo "Connection could not be established.<br />";
     die( print_r( "[{\"Status\":\"Database is not Connected...\"}]", true));
	}

    $str="SELECT * FROM ".$tablename;
	
		//echo $str;
	//$st="<table border='1' width='90%'>";
	
	$stmt = sqlsrv_query( $conn, $str );
	if( $stmt === false) {
    die( print_r( sqlsrv_errors(), true) );
	}
	
	/*foreach( sqlsrv_field_metadata( $stmt ) as $fieldMetadata ) {
    foreach( $fieldMetadata as $name => $value) {
       echo "$name: $value<br />";
    }
      echo "<br />";
   }*/

	$st="";
	foreach( sqlsrv_field_metadata( $stmt) as $fieldMetadata)  
	 { 
	   $colname=$fieldMetadata["Name"];
       $st=$st."<option value='".$colname."'>".$colname."</option>";	
	    
		//$st=$st."<tr><td>".$fieldMetadata["Name"]."</td><td>".$fieldMetadata["Type"]."</td><td>".$fieldMetadata["Size"]."</td><td>".$fieldMetadata["Precision"]."</td><td>".$fieldMetadata["Scale"]."</td><td>".$fieldMetadata["Nullable"]."</td></tr>";
		
     }
	//$st=$st."</table>"; 
	
sqlsrv_free_stmt( $stmt);  
sqlsrv_close( $conn);  
  echo $st;

?>