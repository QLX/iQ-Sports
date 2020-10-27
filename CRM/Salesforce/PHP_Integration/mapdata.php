<html>
<head>
<script src="js/jquery-1.7.2.js"></script>                                      
<script src="js/jquery-ui.js"></script>
<script>
$(document).ready(function(){ 
	$.ajax({
		url:"loaddatabases.php",
		cache: false
   	    })
		.done(function( data ) {		       
			document.getElementById("dbname").innerHTML=data;
			document.getElementById("tablename").innerHTML="";
			document.getElementById("columnnames").innerHTML="";
			document.getElementById("selectedcolumns").innerHTML="";
		});
		
	$.ajax({
		url:"loadobjects.php",
		cache: false
   	    })
		.done(function( data ) {		       
			document.getElementById("objects").innerHTML=data;			
			document.getElementById("crmcolumns").innerHTML="";
			document.getElementById("selectedcolumns").innerHTML="";
		});
   
    $("#dbname").change(function() {
	    var dbname=$("#dbname").val();
		//alert(dbname);
		$.ajax({
		url:"loadtables.php?dbname="+dbname,
		cache: false
   	    })
		.done(function( data ) {		       
			document.getElementById("tablename").innerHTML=data;
			document.getElementById("columnnames").innerHTML="";
			document.getElementById("selectedcolumns").innerHTML="";
		});
	});
	
    $("#tablename").change(function() {
	    var dbname=$("#dbname").val();
		var tablename=$("#tablename").val();
		//alert(dbname+" ==> "+tablename);
		$.ajax({
		url:"loadcolumns.php?dbname="+dbname+"&tablename="+tablename,
		cache: false
   	    })
		.done(function( data ) {		       
		//alert(data);
			document.getElementById("columnnames").innerHTML=data;
		});
	});
	
   $("#objects").change(function() {
	    var objectname=$("#objects").val();
		
		$.ajax({
		url:"loadcrmcolumns.php?objectname="+objectname,
		cache: false
   	    })
		.done(function( data ) {		       
		//alert(data);
			document.getElementById("crmcolumns").innerHTML=data;
		});
	});
	
	$("#selectcolumn").click(function() {
	    var column1=document.getElementById("columnnames").value;
		//alert(column1);
		if(column1!="")
		 {
			var opt = document.createElement('option');
			opt.appendChild(document.createTextNode(column1) );
			opt.value = column1; 
			selectedcolumns.appendChild(opt); 
			var columns=document.getElementById("columnnames");
			columnnames.remove(columns.selectedIndex); 
		 }
		else
		 {
			alert("Please select the column to add...");
		 }
	});
	
	$("#unselectcolumn").click(function() {
	    var column1=document.getElementById("selectedcolumns").value;
		//alert(column1);
		if(column1!="")
		 {
			var opt = document.createElement('option');
			opt.appendChild(document.createTextNode(column1) );
			opt.value = column1; 
			columnnames.appendChild(opt); 
			var columns=document.getElementById("selectedcolumns");
			selectedcolumns.remove(columns.selectedIndex); 
		 }
		else 
		 {
			alert("Please select the column to unselect...");
		 }
	});
	
	$("#mapcolumn").click(function() {		
	    var selectedcolumn=document.getElementById("selectedcolumns").value;
		var selectedsfcolumn=document.getElementById("crmcolumns").value;
		
		if(selectedcolumn!="" && selectedsfcolumn!="")
		 {
			//alert("Selected Column : "+selectedcolumn+" ==> Mapped column : "+selectedsfcolumn);
			var opt = document.createElement('option');
			var mappedcolumn=selectedcolumn+" : "+selectedsfcolumn;
			opt.appendChild(document.createTextNode(mappedcolumn) );
			opt.value = mappedcolumn; 
			mappedcolumns.appendChild(opt);
		 }
		else
		 {
			alert("Need a selected and mapped column....");
		 }
	});
	
	$("#exportdata").click(function() {
		  var database=$("#dbname").val();
		  var tablename=$("#tablename").val();
		  
	      if(database!="--Select--")
		  {
			  if(tablename!="" || tablename!="--Select--")
			  {
				$.ajax({
				url:"exportdata.php?database="+database+"&tablename="+tablename,
				cache: false
				})
				.done(function( data ) {
				alert(data);
				//document.getElementById("crmcolumns").innerHTML=data;
				});			
			  }
			 else
			 {
					alert("Please select a table to export to Salesforce...");
		     }
		  }
		 else
		 {
				alert("Please select a database...");
		 }
	});
});
</script>
<style>
body{font-family:arial;background-color:white}
select{font-size:14px}
</style>
</head>
<body>
<div >
<!--<h1>Data Integration from SQL Server to SalesForce CRM</h1> -->
<center>
<table border='0'><tr>
<th><img src='images/sqlserver.jpg' style='width:50%;height:50%'></img> </th><th><img src='images/arrow-import.png'  style='width:50%;height:50%'/></th><th><img src='images/salesforce.png' style='width:50%;height:50%'></img></th>
</tr>
</table>
</center>
</div>
<br>
<center>
<div>
<table width='90%' border='0'>
<tr>
<th align='left'>Select Database:</th>
<th align='left'>Select Object:</th>
</tr>
<tr>
<td><select name='dbname' id='dbname' style='width:50%'>
<option>--Select--</option>
</td>
<td><select name='objects' id='objects' style='width:50%'>
<option>--Select--</option>
</td>
</tr>
<tr>
<th align='left'>Select Table:</th></tr>
<tr>
<td><select name='tablename' id='tablename' style='width:50%'>
<option>--Select--</option>
</td
</tr>
<tr>
<th align='left' width='50%'>Columns:</th>
<th align='left'></th>
</tr>
<tr>
<td>
<table width='90%' border='0'>
<tr>
<td width='42%'>
<select name='columnnames' id='columnnames' size='15' style='width:100%'>

</select>
</td>
</tr></table>
<td>

</td>
</tr>
</tr>
</table>
<br><br>
<div style='float:right;margin-right:5%;background-color:blue;color:white;font-weight:bold;padding:10px;border-radius:7px' id='exportdata'>EXPORT DATA</div>
</div>
</body>
</html>