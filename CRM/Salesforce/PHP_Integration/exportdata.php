<?php include("config.php"); ?>
<?php
    $dbname=$_REQUEST["database"];
	$tablename=$_REQUEST["tablename"];
	 
	$connectionInfo = array( "Database"=>$dbname, "UID"=>$userid, "PWD"=>$password);
	$conn = sqlsrv_connect( $serverName, $connectionInfo);

	if( !$conn ) 
     {
     //echo "Connection could not be established.<br />";
     die( print_r( "[{\"Status\":\"Database is not Connected...\"}]", true));
	}
	
	$str="SELECT * FROM ".$tablename;
	$stmt = sqlsrv_query( $conn, $str );
	if( $stmt === false) {
    die( print_r( sqlsrv_errors(), true) );
	}
	
	$st="";
	foreach( sqlsrv_field_metadata( $stmt) as $fieldMetadata)  
	 { 
	   $colname=$fieldMetadata["Name"];
	   $type=$fieldMetadata["Type"];
	   $size=$fieldMetadata["Size"];
	   $precision=$fieldMetadata["Precision"];
	   $scale=$fieldMetadata["Scale"];
	   $nullable=$fieldMetadata["Nullable"];
	   
       
	    
		//$st=$st."<tr><td>".$fieldMetadata["Name"]."</td><td>".$fieldMetadata["Type"]."</td><td>".$fieldMetadata["Size"]."</td><td>".$fieldMetadata["Precision"]."</td><td>".$fieldMetadata["Scale"]."</td><td>".$fieldMetadata["Nullable"]."</td></tr>";
		
     }
	//$st=$st."</table>"; 
	
sqlsrv_free_stmt( $stmt);  
sqlsrv_close( $conn);  
  echo $st;
?>