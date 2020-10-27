<?php include("config.php"); ?>
<?php
session_start();
//$serverName = "vdev003.qlx.com"; //serverName\instanceName
$connectionInfo = array( "Database"=>$database, "UID"=>$userid, "PWD"=>$password);
$conn = sqlsrv_connect( $serverName, $connectionInfo);

if( !$conn ) 
     {
     //echo "Connection could not be established.<br />";
     die( print_r( "[{\"Status\":\"Database is not Connected...\"}]", true));
}
?>