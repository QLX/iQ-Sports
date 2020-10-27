<?php include("config.php"); ?>
<?php
    $dbname=$_REQUEST["dbname"];
	
	$connectionInfo = array( "Database"=>$dbname, "UID"=>$userid, "PWD"=>$password);
	$conn = sqlsrv_connect( $serverName, $connectionInfo);

	if( !$conn ) 
     {
     //echo "Connection could not be established.<br />";
     die( print_r( "[{\"Status\":\"Database is not Connected...\"}]", true));
	}

    $str="SELECT TABLE_NAME 'name' FROM information_schema.tables WHERE TABLE_CATALOG='".$dbname."' order by TABLE_NAME";	
	//$str="SELECT * FROM sys.views";
		//echo $str;
	$st="<option>--Select--</option>";
	
	$stmt = sqlsrv_query( $conn, $str );
	if( $stmt === false) {
    die( print_r( sqlsrv_errors(), true) );
	}
	while( $row = sqlsrv_fetch_array( $stmt, SQLSRV_FETCH_ASSOC) )
	{       
	   $dbname=$row["name"];
	   $st=$st."<option value='".$dbname."'>".$dbname."</option>";
	}
	sqlsrv_free_stmt( $stmt);	
    echo $st;
?>