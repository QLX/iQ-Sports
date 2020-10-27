<?php include("connect.php"); ?>
<?php
    $str="select name from sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') order by Name";	
	
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