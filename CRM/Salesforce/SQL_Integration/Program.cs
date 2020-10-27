using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using SQLtoSF.ServiceReference1;
using System.Xml;
using System.Net;
using System.Xml.Linq;
using System.Net.Mail;


namespace SQLtoSF
{
    class Program
    {
        static int batchCount;
        static XDocument configXml;
        static int newrecord = 0, updated = 0, failed = 0, duplicate_count = 0, total_records = 0, duplicates_detected = 0, missing_argument = 0, required_field_missing = 0, invalid_emails_count = 0;
        static String duplicate_ids = "", invalid_emails = "";
        static void Main(string[] args)
        {
            configXml = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "/appCfg.xml");
            String sfdcUserName = configXml.Descendants(XName.Get("salesforceusername")).First().Value.ToString();
            String sfdcPassword = configXml.Descendants(XName.Get("salesforcepassword")).First().Value.ToString();
            String sfdcToken = configXml.Descendants(XName.Get("salesforcetoken")).First().Value.ToString();            

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                        
            ServiceReference1.SoapClient loginClient = new ServiceReference1.SoapClient("Soap");
            string loginPassword = sfdcPassword + sfdcToken;
                
            ServiceReference1.LoginResult result = loginClient.login(null, null, sfdcUserName, loginPassword);
            
            String serverUrl = result.serverUrl;            
            SessionHeader header = new SessionHeader();
            header.sessionId = result.sessionId;

            
            SoapClient sf = new SoapClient();
            sf.Endpoint.Address = new System.ServiceModel.EndpointAddress(serverUrl);

            GetUserInfoResult userdata;
            sf.getUserInfo(header, null,out userdata);
                        
            Console.WriteLine("Working with Accounts....");
            
            String qry = "Select Id,Name from Account";            
            QueryResult accresult = new QueryResult();
            QueryOptions qx = new QueryOptions();
            qx.batchSize = 100000;
            sf.query(header, null, qx, null, null, qry, out accresult);
            //sf.queryAll(header, null, qx, qry, out accresult);
            //sf.queryMore(header, null, null, qry, out accresult);

            List<Account> accountdata = new List<Account>();
            List<String> accountids = new List<String>();
            List<String> accountnames = new List<String>();
            int i = 0;
            //if (accresult.size != 0)
            while (accresult.size != 0)
            {                
                foreach (sObject account in accresult.records)
                {
                    String acctId = account.Any[0].InnerText.Trim();
                    String acctName = account.Any[1].InnerText.Trim();

                  //  Console.WriteLine(acctId + " ==> " + acctName);
                    //accountdata.Add(new Account(account.Any[0].InnerText, account.Any[1].InnerText));
                    accountids.Add(account.Any[0].InnerText.Trim());
                    accountnames.Add(account.Any[1].InnerText.Trim());
                    i++;
                }
                //Console.WriteLine(i);
                String querylocator = accresult.queryLocator;
                try
                {
                    sf.queryMore(header, null, null, querylocator, out accresult);
                }
                catch(Exception ex)
                {
                    break;
                }
            }
            Console.WriteLine("\nInitial Accounts Count : " + i);
            
            string connectionString = configXml.Descendants(XName.Get("connection")).First().Value.ToString();
            String sourcetable = configXml.Descendants(XName.Get("sourcetable")).First().Value.ToString();
            String companyname = configXml.Descendants(XName.Get("AccountName")).First().Value.ToString();
            String str = "Select distinct(" + companyname + ") from " + sourcetable + " where " + companyname + " is not NULL";
            SqlConnection cn = new SqlConnection(connectionString);
            cn.Open();

            SqlCommand cmd = new SqlCommand(str, cn);
            cmd.CommandTimeout = 1000 * 360;
            SqlDataReader dr = cmd.ExecuteReader();

            //String new_accounts = "";
            StringBuilder new_accounts = new StringBuilder();
            String accheaderLine = "Name";
            new_accounts.AppendLine(accheaderLine);
            while (dr.Read())
            {
                String company_name = dr.GetValue(0).ToString().Trim().Replace(",","");
                if (!accountnames.Contains(company_name))
                {
                    company_name = company_name.Trim().Replace(",", "");
                    //new_accounts = new_accounts + "\"" + company_name + "\"\n";
                    new_accounts.AppendLine(company_name.Trim());
                }
            }
            dr.Close();


            //Linq Expression to get column names, which need to be appended to every batch

            Console.WriteLine("New Accounts : " + new_accounts.ToString());
            if (new_accounts.Length > 6)
            {
                // Console.WriteLine(new_accounts);
                String jobId1 = createAccountJob("Account", header.sessionId);
                //Console.WriteLine("Account Job ID : " + jobId1);
                byte[] acc_pushedBytes = Encoding.ASCII.GetBytes(new_accounts.ToString());
                addAccountBatch(acc_pushedBytes, header.sessionId, jobId1);
                //Console.WriteLine("Account batch Completed succesfully");                


                Console.WriteLine("Waiting for Accounts to complete.."+new_accounts.Length);
                String accres = getAccountBatchResult(jobId1, header.sessionId, serverUrl, header);
                while (accres!="Completed")
                 {
                    System.Threading.Thread.Sleep(1000 * 15);
                    accres=getAccountBatchResult(jobId1, header.sessionId, serverUrl, header);
                    Console.WriteLine("Accounts Process Status : " + accres);
                }
            }
            //System.Threading.Thread.Sleep(1000 * 30);

            //  -- FETCHING THE ALL THE ACCOUNT ID AND NAMES FROM SALESFORCE
            qry = "Select Id,Name from Account";            
            QueryResult accresult1 = new QueryResult();
            sf.query(header, null, qx, null, null, qry, out accresult1);
            //sf.queryAll(header, null, qx, qry, out accresult1);
            //sf.queryMore(header, null, null, qry, out accresult1);
            accountids.Clear();            
            accountnames.Clear();
            i = 0;
            while (accresult1.size != 0)
            {
                foreach (sObject account in accresult1.records)
                {
                    String acctId = account.Any[0].InnerText.Trim();
                    String acctName = account.Any[1].InnerText.Trim();

                    //Console.WriteLine(acctId + " ==> " + acctName);
                    //accountdata.Add(new Account(account.Any[0].InnerText, account.Any[1].InnerText));
                    accountids.Add(account.Any[0].InnerText.Trim());
                    accountnames.Add(account.Any[1].InnerText.Trim());
                    i++;
                }

                String querylocator = accresult1.queryLocator;
                try
                {
                    sf.queryMore(header, null, null, querylocator, out accresult1);
                }
                catch (Exception ex)
                {
                    break;
                }
            }

            Console.WriteLine("Current Account Count : " + i);
            Console.WriteLine("Counts of Accounts : " + accountids.Count + " ==> " + accountnames.Count);
            //Console.Read();
            //String mappingtable= configXml.Descendants(XName.Get("mappingtable")).First().Value.ToString();

            // FETCHING THE CONTACTS WITH CATEGORY CONTAINING "FULL"
            String referringfield = configXml.Descendants(XName.Get("referringfield")).First().Value.ToString();
            qry = "Select Email from Contact where "+ referringfield + " like '%Full%'";
            List<String> emails = new List<String>();
            QueryResult accresult2 = new QueryResult();
            sf.query(header, null, qx, null, null, qry, out accresult2);
            //sf.queryAll(header, null, qx, qry, out accresult1);
            //sf.queryMore(header, null, null, qry, out accresult1);
            emails.Clear();
            i = 0;
            while (accresult2.size != 0)
            {
                foreach (sObject account in accresult2.records)
                {
                    String email = account.Any[0].InnerText.Trim();                    
                    emails.Add(email);                                        
                }

                String querylocator = accresult2.queryLocator;
                try
                {
                    sf.queryMore(header, null, null, querylocator, out accresult2);
                }
                catch (Exception ex)
                {
                    break;
                }
            }

            // ADDING THE CONTACTS START HERE
            String jobId = "";
            try
            {
                String mappingtable = configXml.Descendants(XName.Get("mappingtable")).First().Value.ToString();
                String mapqry = "Select count(*) from " + mappingtable + " where isActive=1";
                cmd = new SqlCommand(mapqry, cn);
                dr = cmd.ExecuteReader();
                int rcnt = 0;
                while (dr.Read())
                {
                    rcnt = dr.GetInt32(0);
                }
                dr.Close();
                mappingtable = configXml.Descendants(XName.Get("mappingtable")).First().Value.ToString();
                mapqry = "Select DatabaseColumn,SalesForceColumn,DataType,BindedObject,isUpdatable from " + mappingtable + " where isActive=1";
                cmd = new SqlCommand(mapqry, cn);
                dr = cmd.ExecuteReader();
                String databasecolumns = "";
                String salesforcecolumns = "";
                String databaseupdatablecolumns = "";
                String salesforceupdatablecolumns = "";
                String[] dbcolumns = new String[rcnt];
                String[] sfcolumns = new String[rcnt];
                String[] bindedtable = new String[rcnt];
                String[] datatypes = new String[rcnt];
                String[] isupdatable = new String[rcnt];
                int j = 0;
                while (dr.Read())
                {
                    String dbcolumn = dr.GetValue(0).ToString().Trim();
                    String sfcolumn = dr.GetValue(1).ToString().Trim();
                    databasecolumns = databasecolumns + dbcolumn + ",";
                    salesforcecolumns = salesforcecolumns + sfcolumn + ",";
                    dbcolumns[j] = dbcolumn;
                    sfcolumns[j] = sfcolumn;
                    datatypes[j] = dr.GetValue(2).ToString().Trim();
                    bindedtable[j] = dr.GetValue(3).ToString().Trim();
                    isupdatable[j] = dr.GetValue(4).ToString().Trim();

                    Console.WriteLine(dr.GetValue(4).ToString().Trim());
                    if (int.Parse(dr.GetValue(4).ToString().Trim()) == 1)
                    {
                        databaseupdatablecolumns = databaseupdatablecolumns + dbcolumn + ",";
                        salesforceupdatablecolumns = salesforceupdatablecolumns + sfcolumn + ",";
                    }
                    j++;
                }
                dr.Close();
                databasecolumns = databasecolumns.Substring(0, databasecolumns.Length - 1);
                salesforcecolumns = salesforcecolumns.Substring(0, salesforcecolumns.Length - 1);
                databaseupdatablecolumns = databaseupdatablecolumns.Substring(0, databaseupdatablecolumns.Length - 1);
                salesforceupdatablecolumns = salesforceupdatablecolumns.Substring(0, salesforceupdatablecolumns.Length - 1);


                //String queryString = "SELECT top 5000 iQ_Customer_Key,Acct_ID,Salutation,First_Name,Last_Name,Company_Name From CFS_Customer_Contact";
                sourcetable = configXml.Descendants(XName.Get("sourcetable")).First().Value.ToString();
                int num_rows = int.Parse(configXml.Descendants(XName.Get("num_rows")).First().Value.ToString());
                String queryString = "";
                if (num_rows > 0)
                    queryString = "SELECT top " + num_rows + " " + databasecolumns + " From " + sourcetable + " where Category is NULL or Category not like '%Full%'";
                else
                    queryString = "SELECT " + databasecolumns + " From " + sourcetable + " where Category is NULL or Category not like '%Full%'";

                cmd = new SqlCommand(queryString, cn);
                dr = cmd.ExecuteReader();
                StringBuilder data = new StringBuilder();
                StringBuilder data1 = new StringBuilder();
                data.AppendLine(salesforcecolumns);
                data1.AppendLine(salesforceupdatablecolumns);
                int rec_count = 0;
                String exter_id = configXml.Descendants(XName.Get("externalid")).First().Value.ToString();

                jobId = createSFJob("Contact", exter_id, header.sessionId);
                int recordsperbatch = int.Parse(configXml.Descendants(XName.Get("recordsperbatch")).First().Value.ToString());
                while (dr.Read())
                {
                    int cnt = dbcolumns.Length;
                    //Console.WriteLine("Total Column Count : " + cnt);
                    String recorddata = "";
                    String replacevalue = "";
                    for (int k = 0; k < cnt; k++)
                    {
                        if (datatypes[k] != "date")
                        {
                            if (bindedtable[k] == "Account")
                            {
                                String acc = dr.GetValue(k).ToString().Trim();
                                if (acc.Length != 0 && acc != null)
                                {
                                    //Console.WriteLine(acc+" ==> "+acc.Length);
                                    int key = accountnames.IndexOf(dr.GetValue(k).ToString().Trim().Replace(",", ""));
                                    Console.WriteLine("key : " + key);
                                    Console.WriteLine(key + " ==> " + dr.GetValue(k).ToString() + " ==> " + k + " ==> " + dr.FieldCount);
                                    String acc_key = accountids[key];
                                    recorddata = recorddata + "\"" + acc_key + "\",";
                                }
                                else
                                {
                                    recorddata = recorddata + "\"" + acc + "\",";
                                }
                            }
                            else if (bindedtable[k] == "Email")
                            {
                                String mailid = dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "").Replace("..", ".").Trim();
                                if (mailid.Contains(";"))
                                    mailid = mailid.Substring(0, mailid.IndexOf(";"));

                                mailid = mailid.Replace(";", "");

                                if (mailid.Contains(" "))
                                    mailid = mailid.Replace(" ", "");

                                recorddata = recorddata + "\"" + mailid + "\",";
                            }
                            else
                            {
                                recorddata = recorddata + "\"" + dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "") + "\",";
                            }
                        }
                        else
                        {
                            //Console.WriteLine("Date : " + dr.GetValue(k).ToString());
                            recorddata = recorddata + "\"" + ((dr.GetValue(k).ToString().Trim().Length == 0) ? null : DateTime.Parse(dr.GetValue(k).ToString().Trim().Replace("'", "")).ToString("yyyy-MM-dd")) + "\",";
                        }
                    }

                    recorddata = recorddata.Substring(0, recorddata.Length - 1);
                    data.AppendLine(recorddata);
                    rec_count++;

                    if (rec_count % recordsperbatch == 0)
                    {
                        byte[] pushedBytes = Encoding.ASCII.GetBytes(data.ToString());
                        addBatch(pushedBytes, header.sessionId, jobId);
                        Console.WriteLine("Added batch succesfully");
                        data.Clear();
                        data.AppendLine(salesforcecolumns);
                        rec_count = 0;
                    }
                }
                dr.Close();


                if (rec_count > 0)
                {
                    byte[] pushedBytes = Encoding.ASCII.GetBytes(data.ToString());
                    addBatch(pushedBytes, header.sessionId, jobId);
                    Console.WriteLine("Added batch succesfully");
                    data.Clear();
                    data.AppendLine(salesforcecolumns);
                    rec_count = 0;
                }
            
                // UPLOADS DATA WITH CATEGORY null TO SF COMPLETES HERE


                // ADDED NEW FOR LOADING CATEGORY NOT NULL RECORDS
                queryString = "SELECT " + databasecolumns + " From " + sourcetable + " where Category like '%Full%'";
                Console.WriteLine(queryString);
                cmd = new SqlCommand(queryString, cn);
                dr = cmd.ExecuteReader();
                data.Clear();
                data.AppendLine(salesforcecolumns);
                rec_count = 0;
                while (dr.Read())
                {
                    String recorddata = "";
                    String recorddata1 = "";
                    if (!emails.Contains(dr.GetValue(0).ToString().Trim()))
                    {
                        int cnt = dbcolumns.Length;
                        //Console.WriteLine("Total Column Count : " + cnt);

                        //String replacevalue = "";
                        for (int k = 0; k < cnt; k++)
                        {
                            if (datatypes[k] != "date")
                            {
                                if (bindedtable[k] == "Account")
                                {
                                    String acc = dr.GetValue(k).ToString().Trim();
                                    if (acc.Length != 0 && acc != null)
                                    {
                                        //Console.WriteLine(acc+" ==> "+acc.Length);
                                        int key = accountnames.IndexOf(dr.GetValue(k).ToString().Trim().Replace(",", ""));
                                        Console.WriteLine("key : " + key);
                                        Console.WriteLine(key + " ==> " + dr.GetValue(k).ToString() + " ==> " + k + " ==> " + dr.FieldCount);
                                        String acc_key = accountids[key];
                                        recorddata = recorddata + "\"" + acc_key + "\",";
                                    }
                                    else
                                    {
                                        recorddata = recorddata + "\"" + acc + "\",";
                                    }
                                }
                                else if (bindedtable[k] == "Email")
                                {
                                    String mailid = dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "").Replace("..", ".").Trim();
                                    if (mailid.Contains(";"))
                                        mailid = mailid.Substring(0, mailid.IndexOf(";"));

                                    mailid = mailid.Replace(";", "");

                                    if (mailid.Contains(" "))
                                        mailid = mailid.Replace(" ", "");

                                    recorddata = recorddata + "\"" + mailid + "\",";
                                }
                                else
                                {
                                    recorddata = recorddata + "\"" + dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "") + "\",";
                                }
                            }
                            else
                            {
                                //Console.WriteLine("Date : " + dr.GetValue(k).ToString());
                                recorddata = recorddata + "\"" + ((dr.GetValue(k).ToString().Trim().Length == 0) ? null : DateTime.Parse(dr.GetValue(k).ToString().Trim().Replace("'", "")).ToString("yyyy-MM-dd")) + "\",";
                            }
                        }
                    }
                    else
                    {
                        int cnt1 = dbcolumns.Length;
                        //Console.WriteLine("Total Column Count : " + cnt);

                        //String replacevalue = "";
                        for (int k = 0; k < cnt1; k++)
                        {
                            Console.WriteLine("IsUpdateable : " + isupdatable[k].Trim() + " ==> " + dr.GetValue(k).ToString());
                            if (isupdatable[k].Trim() == "1")
                            {
                                if (datatypes[k] != "date")
                                {
                                    recorddata1 = recorddata1 + "\"" + dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "") + "\",";
                                }
                                else
                                {
                                    //Console.WriteLine("Date : " + dr.GetValue(k).ToString());
                                    recorddata1 = recorddata1 + "\"" + ((dr.GetValue(k).ToString().Trim().Length == 0) ? null : DateTime.Parse(dr.GetValue(k).ToString().Trim().Replace("'", "")).ToString("yyyy-MM-dd")) + "\",";
                                }
                            }
                        }
                    }

                    if (recorddata.Length != 0)
                    {
                        recorddata = recorddata.Substring(0, recorddata.Length - 1);
                        data.AppendLine(recorddata);
                    }
                    if (recorddata1.Length != 0)
                    {
                        recorddata1 = recorddata1.Substring(0, recorddata1.Length - 1);
                        data1.AppendLine(recorddata1);
                    }
                    rec_count++;

                    if (rec_count % recordsperbatch == 0)
                    {
                        if (recorddata.Length != 0)
                        {
                            byte[] pushedBytes = Encoding.ASCII.GetBytes(data.ToString());
                            addBatch(pushedBytes, header.sessionId, jobId);
                            data.Clear();
                            data.AppendLine(salesforcecolumns);
                        }

                        if (recorddata1.Length != 0)
                        {
                            byte[] pushedBytes1 = Encoding.ASCII.GetBytes(data1.ToString());
                            addBatch(pushedBytes1, header.sessionId, jobId);
                            data1.Clear();
                            data1.AppendLine(salesforceupdatablecolumns);
                        }
                        Console.WriteLine("Added batch succesfully");

                        rec_count = 0;
                    }
                }
                dr.Close();

                if (rec_count > 0)
                {
                    byte[] pushedBytes = Encoding.ASCII.GetBytes(data.ToString());
                    addBatch(pushedBytes, header.sessionId, jobId);
                    data.Clear();
                    data.AppendLine(salesforcecolumns);

                    if (data1.Length != 0)
                    {
                        byte[] pushedBytes1 = Encoding.ASCII.GetBytes(data1.ToString());
                        addBatch(pushedBytes1, header.sessionId, jobId);
                        data1.Clear();
                        data1.AppendLine(salesforceupdatablecolumns);
                    }

                    Console.WriteLine("Added batch succesfully");
                    data.Clear();
                    data.AppendLine(salesforcecolumns);
                    rec_count = 0;
                }
                cn.Close();
                System.Threading.Thread.Sleep(1000 * 15);                
            }
           catch (Exception e1)
           {
               Console.WriteLine("Error : " + e1.ToString());
           }

            getBatchResult(jobId, header.sessionId,serverUrl,header);
            //}
            Console.WriteLine("Data Loading Completed Successsfully...");
            //Console.Read();
        }

        class Account
        {
            public const String SObjectTypeName = "Account";

            public String Id { get; set; }
            public String Name { get; set; }

            public Account(String id,String name)
            {
                Id = id;
                Name = name;
            }

        }

        // METHOD FOR LOADING ONE OF THE DUPLICATE RECORDS STARTS HERE
        public static void re_runBatch_ForDuplicateRecords(String serverUrl,SessionHeader header,String externalids,String jobId)
        {
            SoapClient sf = new SoapClient();
            sf.Endpoint.Address = new System.ServiceModel.EndpointAddress(serverUrl);

            List<Account> accountdata = new List<Account>();
            List<String> accountids = new List<String>();
            List<String> accountnames = new List<String>();
            
            string connectionString = configXml.Descendants(XName.Get("connection")).First().Value.ToString();            
            SqlConnection cn = new SqlConnection(connectionString);
            cn.Open();

            //  -- FETCHING THE ALL THE ACCOUNT ID AND NAMES FROM SALESFORCE
            String qry = "Select Id,Name from Account";
            QueryResult accresult = new QueryResult();
            sf.query(header, null, null, null, null, qry, out accresult);

            accountids.Clear();
            accountnames.Clear();
            int i = 0;
            if (accresult.size != 0)
            {
                foreach (sObject account in accresult.records)
                {
                    String acctId = account.Any[0].InnerText;
                    String acctName = account.Any[1].InnerText;

                    //Console.WriteLine(acctId + " ==> " + acctName);
                    //accountdata.Add(new Account(account.Any[0].InnerText, account.Any[1].InnerText));
                    accountids.Add(account.Any[0].InnerText);
                    accountnames.Add(account.Any[1].InnerText);
                    i++;
                }
            }
            //String mappingtable= configXml.Descendants(XName.Get("mappingtable")).First().Value.ToString();


            // ADDING THE CONTACTS START HERE
            //String jobId = "";
            try
            {
                String mappingtable = configXml.Descendants(XName.Get("mappingtable")).First().Value.ToString();
                String mapqry = "Select count(*) from " + mappingtable + " where isActive=1";
                SqlCommand cmd = new SqlCommand(mapqry, cn);
                SqlDataReader dr = cmd.ExecuteReader();
                int rcnt = 0;
                while (dr.Read())
                {
                    rcnt = dr.GetInt32(0);
                }
                dr.Close();
                mapqry = "Select DatabaseColumn,SalesForceColumn,DataType,BindedObject from " + mappingtable + " where isActive=1";
                cmd = new SqlCommand(mapqry, cn);
                dr = cmd.ExecuteReader();
                String databasecolumns = "";
                String salesforcecolumns = "";
                String[] dbcolumns = new String[rcnt];
                String[] sfcolumns = new String[rcnt];
                String[] bindedtable = new String[rcnt];
                String[] datatypes = new String[rcnt];
                int j = 0;
                while (dr.Read())
                {
                    String dbcolumn = dr.GetValue(0).ToString().Trim();
                    String sfcolumn = dr.GetValue(1).ToString().Trim();
                    databasecolumns = databasecolumns + dbcolumn + ",";
                    salesforcecolumns = salesforcecolumns + sfcolumn + ",";
                    dbcolumns[j] = dbcolumn;
                    sfcolumns[j] = sfcolumn;
                    datatypes[j] = dr.GetValue(2).ToString().Trim();
                    bindedtable[j] = dr.GetValue(3).ToString().Trim();
                    j++;
                }
                dr.Close();
                databasecolumns = databasecolumns.Substring(0, databasecolumns.Length - 1);
                salesforcecolumns = salesforcecolumns.Substring(0, salesforcecolumns.Length - 1);


                //String queryString = "SELECT top 5000 iQ_Customer_Key,Acct_ID,Salutation,First_Name,Last_Name,Company_Name From CFS_Customer_Contact";
                String sourcetable = configXml.Descendants(XName.Get("sourcetable")).First().Value.ToString();               

                    String extid = configXml.Descendants(XName.Get("externaldbid")).First().Value.ToString();
                //String duplicatefilter = configXml.Descendants(XName.Get("duplicatefilter")).First().Value.ToString();
                String[] extid_list = externalids.Split(',');
                extid_list = extid_list.Distinct().ToArray();
                //for (int ii = 0; ii < extid_list.Length; ii++)
                //{
                       String queryString = "SELECT distinct " + databasecolumns + " From " + sourcetable + " where " + extid + " in (" + externalids + ")";
                    //String queryString = "SELECT distinct " + databasecolumns + " From " + sourcetable + " where " + extid + "=" + extid_list[ii] + " and " + duplicatefilter + "=(select max(" + duplicatefilter + ") from " + sourcetable + " where " + extid + "=" + extid_list[ii] + ")";
                    Console.WriteLine(queryString);
                    Console.WriteLine("Query under rerun batch...press any key to continue...");
                    //Console.Read();
                    cmd = new SqlCommand(queryString, cn);
                    cmd.CommandTimeout = 1000 * 360;
                    dr = cmd.ExecuteReader();
                    StringBuilder data = new StringBuilder();
                    data.AppendLine(salesforcecolumns);
                    int rec_count = 0;

                    //jobId = createSFJob("Contact", "Ssbcrm_Str_DwId__c", header.sessionId);
                    //int recordsperbatch = int.Parse(configXml.Descendants(XName.Get("recordsperbatch")).First().Value.ToString());
                    int recordsperbatch = 1;
                    while (dr.Read())
                    {
                        int cnt = dbcolumns.Length;
                        //Console.WriteLine("Total Column Count : " + cnt);
                        String recorddata = "";
                    string replacevalue = "";
                        for (int k = 0; k < cnt; k++)
                        {
                            if (datatypes[k] != "date")
                            {
                                if (bindedtable[k] == "Account")
                                {
                                    String acc = dr.GetValue(k).ToString().Trim();
                                    if (acc.Length != 0 && acc != null)
                                    {
                                        //Console.WriteLine(acc+" ==> "+acc.Length);
                                        int key = accountnames.IndexOf(dr.GetValue(k).ToString().Trim());
                                        //Console.WriteLine(key + " ==> " + dr.GetValue(k).ToString() + " ==> " + k + " ==> " + dr.FieldCount);
                                        String acc_key = accountids[key];
                                        recorddata = recorddata + "\"" + acc_key + "\",";
                                    }
                                    else
                                    {
                                        recorddata = recorddata + "\"" + acc + "\",";
                                    }
                                }
                            /*else if (bindedtable[k] == "replace_empty")
                            {
                                //Console.WriteLine("Current Index : " + k);
                                recorddata = recorddata + "\"" + dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"","") + "\",";
                                replacevalue = dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "");
                            }
                            else if (bindedtable[k].Contains("required:"))
                            {
                                if (dr.GetValue(k).ToString().Trim().Length == 0)
                                {
                                    recorddata = recorddata + "\"" + replacevalue + "\",";
                                    replacevalue = "";
                                }
                                else
                                    recorddata = recorddata + "\"" + dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "") + "\",";
                            }*/
                            else
                            {
                                recorddata = recorddata + "\"" + dr.GetValue(k).ToString().Trim().Replace("'", "").Replace("\"", "") + "\",";
                            }
                        }
                            else
                            {
                                //Console.WriteLine("Date : " + dr.GetValue(k).ToString());
                                recorddata = recorddata + "\"" + ((dr.GetValue(k).ToString().Trim().Length == 0) ? null : DateTime.Parse(dr.GetValue(k).ToString().Trim().Replace("'", "")).ToString("yyyy-MM-dd")) + "\",";
                            }
                        }

                        recorddata = recorddata.Substring(0, recorddata.Length - 1);
                        data.AppendLine(recorddata);
                        rec_count++;

                        if (rec_count % recordsperbatch == 0)
                        {
                            Console.WriteLine("Data in Re-Run : " + data);
                            //Console.Read();
                            byte[] pushedBytes = Encoding.ASCII.GetBytes(data.ToString());
                            addBatch(pushedBytes, header.sessionId, jobId);

                            Console.WriteLine("Added batch succesfully");
                            data.Clear();
                            data.AppendLine(salesforcecolumns);
                            rec_count = 0;
                        }
                    }
                    dr.Close();
                    

                    if (rec_count > 0)
                    {
                        byte[] pushedBytes = Encoding.ASCII.GetBytes(data.ToString());
                        addBatch(pushedBytes, header.sessionId, jobId);
                        Console.WriteLine("Added batch succesfully");
                        data.Clear();
                        data.AppendLine(salesforcecolumns);
                        rec_count = 0;
                    }
                    System.Threading.Thread.Sleep(1000 * 10);
                //}                
            }
            catch (Exception e1)
            {
                Console.WriteLine("Error : " + e1.ToString());
            }
            cn.Close();
        }
        // METHOD FOR LOADING DUPLICATE RECORDS END HERE

       public static String getAccountBatchResult(String jobid, String sessionId, String serverurl, SessionHeader header)
        {
            String returnvalue = "Processing";
            String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
            String reqURL = salesforcepath + "services/async/45.0/job/" + jobid + "/batch/";
            XmlDocument responseXmlDocument = myHttpPost(null, reqURL, sessionId, "GET", "text/csv; charset=UTF-8;X-SFDC-Session: " + sessionId);
            XmlNodeList batchStateList = responseXmlDocument.GetElementsByTagName("state");            
            
            //            int newrecord = 0, updated = 0, failed = 0, duplicate = 0, total_records = 0;
            String external_ids = "";

            if (batchStateList[0].InnerText.Equals("Completed"))
                returnvalue = "Completed";
            
            return returnvalue;
        }
        public static void getBatchResult(String jobid,String sessionId,String serverurl,SessionHeader header)
        {
            String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
            String reqURL = salesforcepath + "services/async/45.0/job/" + jobid + "/batch/";
            XmlDocument responseXmlDocument = myHttpPost(null, reqURL, sessionId, "GET", "text/csv; charset=UTF-8;X-SFDC-Session: " + sessionId);
            XmlNodeList batchStateList = responseXmlDocument.GetElementsByTagName("state");
            XmlNodeList batchNodeList = responseXmlDocument.GetElementsByTagName("id");
            int batch_count = batchNodeList.Count;
//            int newrecord = 0, updated = 0, failed = 0, duplicate = 0, total_records = 0;
            String external_ids = "";
            int duplicate = 0;
            int batch_complete_count = 0;
            for (int jc = 0; jc < batch_count; jc++)
            {
                Console.WriteLine("Batch Status : " + batchStateList[0].InnerText);
                if (batchStateList[jc].InnerText.Equals("Completed"))
                {
                    batch_complete_count++;
                    closeJob(jobid, sessionId);
                }
            }

            if (batch_complete_count != batch_count)
            {
                System.Threading.Thread.Sleep(1000 * 15);
                getBatchResult(jobid, sessionId, serverurl, header);
            }
            else
            {
                for (int jc = 0; jc < batch_count; jc++)
                {
                    String batchId = batchNodeList[jc].InnerText;
                    Console.WriteLine("Batch Id: " + batchId + " ==> " + jobid);
                    //Console.Write("Session Id : " + sessionId);

                    reqURL = salesforcepath + "/services/async/45.0/job/" + jobid + "/batch/" + batchId + "/result";
                    //Console.WriteLine(reqURL);            
                    String output = myHttpPost_BatchResult(null, reqURL, sessionId, "GET", "text/json; charset=UTF-8");

                    //responseXmlDocument.Save("F:/Output.xml");
                    //Console.WriteLine(output);
                    String[] data = output.Split('\n');
                    Console.WriteLine("Batch Result : \n: " + (data.Length - 2));

                    for (int i = 1; i < data.Length - 1; i++)
                    {
                        String record = data[i];
                        String[] dataitems = record.Split(',');
                        total_records++;
                        //Console.WriteLine(record + " ==> " + dataitems.Length);

                        if (dataitems[1] == "\"true\"" && dataitems[2] == "\"true\"")
                            newrecord++;

                        if (dataitems[1] == "\"true\"" && dataitems[2] == "\"false\"")
                            updated++;

                        if (dataitems[1] == "\"false\"" && dataitems[2] == "\"false\"")
                        {
                            //total_records--;
                            duplicate_count++;
                            String response = dataitems[3];
                            if (response.Contains("DUPLICATE_VALUE"))
                            {                                
                                duplicate++;
                                String[] res_values = response.Split(':');
                                String[] values = res_values[2].Split(',');
                                String[] num = values[0].Split('.');
                                //external_ids = external_ids + "'" + num[0].Trim() + "',";
                                external_ids = external_ids + "'" + values[0].Trim() + "',";
                                Console.WriteLine("Duplicate ID : " + values[0]);
                                duplicate_ids = duplicate_ids + values[0] + ",";                                           
                            }

                            if (response.Contains("MISSING_ARGUMENT"))
                            {
                                String[] res_values = response.Split(':');
                                //String[] values = res_values[1].Split(',');
                                Console.WriteLine("Missing Argument : " + res_values[1]);
                                missing_argument++;
                            }
                        }

                        if (dataitems[1] == "\"false\"" && dataitems[2] == "\"true\"")
                        {
                            failed++;
                            String response = dataitems[3];
                            if (response.Contains("INVALID_EMAIL_ADDRESS"))
                            {
                                invalid_emails_count++;
                                String[] res_values = response.Split(':');
                                //String[] values = res_values[2].Split(',');
                                Console.WriteLine("Failed ID : " + res_values[3]+" ==> "+response);
                                invalid_emails = invalid_emails + "<tr><td>"+res_values[3] + "</td></tr>";
                                //Console.Read();
                            }

                            if (response.Contains("DUPLICATES_DETECTED"))
                            {
                                String[] res_values = response.Split(':');
                                String[] values = res_values[2].Split(',');
                                Console.WriteLine("Failed ID : " + values[0]);
                                duplicates_detected++;
                            }

                            if (response.Contains("REQUIRED_FIELD_MISSING"))
                            {
                                String[] res_values = response.Split(':');
                                //String[] values = res_values[2];//.Split(',');
                                Console.WriteLine("Required Field Missing : " + res_values[2]);
                                required_field_missing++;
                            }
                        }
                    }
                }


                /*Console.WriteLine("Total records   : " + total_records);
                Console.WriteLine("New Records     : " + newrecord);
                Console.WriteLine("Updated Records : " + updated);
                Console.WriteLine("Duplicated Records  : " + duplicate);
                Console.WriteLine("Failed Records  : " + failed);*/
                //return responseXmlDocument;

                Console.WriteLine("Duplicates Count : " + duplicate);
                if (duplicate > 0)
                {
                    external_ids = external_ids.Substring(0, external_ids.Length - 1);
                    total_records = total_records - (duplicate / 2);
                    duplicate = 0;
                    Console.WriteLine("External IDS : " + external_ids);
					String exter_idx = configXml.Descendants(XName.Get("externalid")).First().Value.ToString();
                    String jobId = createSFJob("Contact", exter_idx, header.sessionId);
                    re_runBatch_ForDuplicateRecords(serverurl, header, external_ids, jobId);
                    getBatchResult(jobId, sessionId, serverurl, header);
                }
                else
                {
                    Console.WriteLine("Total records   : " + total_records);
                    Console.WriteLine("New Records     : " + newrecord);
                    Console.WriteLine("Updated Records : " + updated);
                    Console.WriteLine("Duplicate Ids  : " + duplicate_count);
                    Console.WriteLine("Duplicate Records  : " + duplicates_detected);
                    Console.WriteLine("Failed Records  : " + (failed-duplicates_detected));

                    String msg = "Summary of Data Loaded to CF Salesforce for " + DateTime.Now.ToString("dd-MMM-yyyy") + ": <br>";
                    msg = msg + "<table border='1'><tr><th align='left'>Total Records : </th><th align='left'>" + total_records + "</th></tr>";
                    msg = msg + "<tr><th align='left'>Newly added Records : </th><th align='left'>" + newrecord + "</th></tr>";
                    msg = msg + "<tr><th align='left'>Updated Records : </th><th align='left'>" + updated + "</th></tr>";
                    /*msg = msg + "<tr><th align='left'>Duplicate Ids : </th><th align='left'>" + duplicate_count + "</th></tr>";                    
                    msg = msg + "<tr><td></td><td><table><tr><td>Duplicate Value : </td><td>" + duplicate + "</td></tr></table></td></tr>";*/
                    msg = msg + "<tr><th align='left'>Failed Records : </th><th align='left'>" + (invalid_emails_count + missing_argument + duplicates_detected + required_field_missing) + "</th></tr>";
                    msg = msg + "<tr><td></td><table><tr><td>Invalid Email Address : </td><td>" + invalid_emails_count + "</td></tr>";
                    msg = msg + "<tr><td>Missing Argument<br><small>(Email not Specified)</small> : </td><td>" + missing_argument + "</td></tr>";
                    msg = msg + "<tr><td>Duplicate Detected<br><small>(Duplicate Email)</small> : </td><td>" + duplicates_detected + "</td></tr>";
                    msg = msg + "<tr><td>Required Field Missing <br><small>(LastName)</small> : </td><td>" + required_field_missing + "</td></tr></table></td></tr></table>";
                    //msg = msg + "<tr><th align='left'>Failed Records : </th><th align='left'>" + (failed - duplicates_detected) + "</th></tr></table>";

                    if(duplicate_ids.Length>0)
                    {
                        duplicate_ids = duplicate_ids.Substring(0, duplicate_ids.Length - 1);
                        String[] dupids = duplicate_ids.Split(',');
                        String[] dupids_unique = dupids.Distinct().ToArray();
                        duplicate_ids = "";
                        for (int ii = 0; ii < dupids_unique.Length; ii++)
                            duplicate_ids = duplicate_ids + dupids_unique[ii] + ",";

                        duplicate_ids = duplicate_ids.Substring(0, duplicate_ids.Length - 1);
                        msg = msg + "<br><br><b>Duplicate Ids (" + dupids_unique.Length + "):</b> " + duplicate_ids;
                    }

                    if (invalid_emails.Length != 0)
                        msg = msg + "<br><br><b>Failed Emails : </b><table border='1'>" + invalid_emails;

                    msg = msg + "</tr>";
                    sendEmail(msg);
                }
            }
        }

        public static void sendEmail(String msg)
        {
            string fromAddress = configXml.Descendants(XName.Get("fromadress")).First().Value.ToString();
            string toAddress = configXml.Descendants(XName.Get("toaddress")).First().Value.ToString();
            string fromPassword = configXml.Descendants(XName.Get("frompassword")).First().Value.ToString();
            string smtpServer = configXml.Descendants(XName.Get("smtpserver")).First().Value.ToString();
            int smtpPort = int.Parse(configXml.Descendants(XName.Get("smtpport")).First().Value.ToString());
            string email_subject = configXml.Descendants(XName.Get("emailsubject")).First().Value.ToString() + DateTime.Now.ToString("dd-MMM-yyyy");

            var smtp = new SmtpClient
            {
                Host = smtpServer,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = email_subject,
                Body = msg,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }

        public static String createAccountJob(string sfObject, String sessionId)
        {
            string jobId = null;
            Console.WriteLine("creating Salesforce Job...");
            String str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            str = str + "<jobInfo xmlns=\"http://www.force.com/2009/06/asyncapi/dataload\">";
            str = str + "<operation>insert</operation>";
            str = str + "<object>" + sfObject + "</object>";            
            str = str + "<contentType>CSV</contentType>";
            str = str + "</jobInfo>";
            try
            {
                String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
                String reqURL = salesforcepath + "services/async/45.0/job";
                XmlDocument reqDoc = new XmlDocument();
                reqDoc.LoadXml(str);
                Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(reqDoc.InnerXml);
                //myHttpPost is 
                XmlDocument responseXmlDocument = myHttpPost(bytes, reqURL, sessionId, "POST", "text/csv; charset=UTF-8");

                jobId = responseXmlDocument.GetElementsByTagName("id").Item(0).InnerText;
              //  Console.WriteLine("job id = " + jobId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in createSFJob : " + ex);
            }
            // responseHttpRequest.Dispose();
            return jobId;
        }

        public static String createSFJob(string sfObject, string extId,String sessionId)
        {
            string jobId = null;
            Console.WriteLine("creating Salesforce Job...");
            String str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            str = str + "<jobInfo xmlns=\"http://www.force.com/2009/06/asyncapi/dataload\">";
            str = str + "<operation>upsert</operation>";
            str = str + "<object>" + sfObject + "</object>";
            str = str + "<externalIdFieldName>" + extId + "</externalIdFieldName>";
            str = str + "<contentType>CSV</contentType>";
            str = str + "</jobInfo>";
            try
            {
                String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
                String reqURL = salesforcepath + "services/async/45.0/job";
                XmlDocument reqDoc = new XmlDocument();
                reqDoc.LoadXml(str);
                Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(reqDoc.InnerXml);
                //myHttpPost is 
                XmlDocument responseXmlDocument = myHttpPost(bytes, reqURL, sessionId, "POST", "text/csv; charset=UTF-8");

                jobId = responseXmlDocument.GetElementsByTagName("id").Item(0).InnerText;
                //Console.WriteLine("job id = " + jobId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in createSFJob : " + ex);
            }
            // responseHttpRequest.Dispose();
            return jobId;
        }

        public static String myHttpPost_BatchResult(Byte[] bytes, String reqURL, string sessionId, string method, string contentType)
        {
            XmlDocument responseXmlDocument = new XmlDocument();
            String data = "";
            try
            {
                WebRequest requestHttp = WebRequest.Create(reqURL);
                requestHttp.Method = method;
                requestHttp.Timeout = 1000000000;
                requestHttp.ContentType = contentType;
                requestHttp.Headers.Add(("X-SFDC-Session: " + sessionId));              

                using (WebResponse responseHttpRequest = requestHttp.GetResponse())
                {
                    System.IO.Stream responseStream = responseHttpRequest.GetResponseStream();
                    int ch;                   

                     while ((ch=responseStream.ReadByte())!=-1)
                     {
                         data = data + (char)ch;                         
                     }                    
                }
            }
            catch (WebException ex)
            {
                data = "";
            }            
            return data;
        }

        public static XmlDocument myHttpPost(Byte[] bytes, String reqURL, string sessionId, string method, string contentType)
        {
            XmlDocument responseXmlDocument = new XmlDocument();
            try
            {
                WebRequest requestHttp = WebRequest.Create(reqURL);
                requestHttp.Method = method;
                //requestHttp.Timeout = Timeout.Infinite;
                requestHttp.ContentType = contentType;
                requestHttp.Headers.Add(("X-SFDC-Session: " + sessionId));
                if (bytes != null)
                {
                    requestHttp.ContentLength = bytes.Length;
                    System.IO.Stream strmHttpContent = requestHttp.GetRequestStream();
                    strmHttpContent.Write(bytes, 0, bytes.Length);
                    strmHttpContent.Close();

                }
                using (WebResponse responseHttpRequest = requestHttp.GetResponse())
                {
                    System.IO.Stream responseStream = responseHttpRequest.GetResponseStream();
                    responseXmlDocument.Load(responseStream);

                }

            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse wr = ex.Response;
                    string msg = new System.IO.StreamReader(wr.GetResponseStream()).ReadToEnd().Trim();
                    Console.WriteLine(msg);
                }
                Console.WriteLine("Exception in POST method with URL :: " + reqURL + "\n And Exception is :" + ex);
            }
            return responseXmlDocument;
        }

        public static void addAccountBatch(Byte[] bytes, string sessionId, string jobId)
        {
            //batchCount++;
            Console.WriteLine("Creating Account batch-------");
            String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
            String requestURI = salesforcepath + "services/async/45.0/job/" + jobId + "/batch";
            XmlDocument response= myHttpPost(bytes, requestURI, sessionId, "POST", "text/csv; charset=UTF-8");
            //Console.WriteLine(response.OuterXml);
        }
        public static XmlDocument addBatch(Byte[] bytes, string sessionId, string jobId)
        {
            batchCount++;
            Console.WriteLine("Adding batch : " + batchCount);
            String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
            String requestURI = salesforcepath + "services/async/45.0/job/" + jobId + "/batch";
            XmlDocument response= myHttpPost(bytes, requestURI, sessionId, "POST", "text/csv; charset=UTF-8");
            return response;
        }

        public static void closeJob(string jobId,String sessionID)
        {
            Console.WriteLine("closing job :" + jobId + "....");
            String str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            str = str + "<jobInfo xmlns=\"http://www.force.com/2009/06/asyncapi/dataload\">";
            str = str + "<state>Closed</state>";
            str = str + "</jobInfo>";
            String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
            String reqURL = salesforcepath + "services/async/45.0/job/" + jobId;
            XmlDocument reqDoc = new XmlDocument();
            try
            {
                reqDoc.LoadXml(str);
                Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(reqDoc.InnerXml);
                XmlDocument responseXmlDocument = myHttpPost(bytes, reqURL, sessionID, "POST", "text/csv; charset=UTF-8");
                String state = responseXmlDocument.GetElementsByTagName("state").Item(0).InnerText;
                if (state == "Closed")
                {
                    Console.WriteLine("Successfully closed job..\n Press any key to continue...");
                }
                else
                {
                    Console.WriteLine("failed to close job");
                }
                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in closeJob :" + ex);
            }
        }

        public static void getBatchStatus(string sessionId, string jobId)
        {
            Console.WriteLine("getting batch status...");
            string batchId = null;
            XmlDocument responseXmlDocument = new XmlDocument();
            String salesforcepath = configXml.Descendants(XName.Get("salesforcepath")).First().Value.ToString();
            String requestUrl = salesforcepath + "services/async/45.0/job/" + jobId + "/batch";
            bool isCompleted = false;
            try
            {
                while (!isCompleted)
                {
                    responseXmlDocument = myHttpPost(null, requestUrl, sessionId, "GET", "text/csv; charset=UTF-8");
                    Console.WriteLine("batch id xml " + responseXmlDocument.ToString());
                    XmlNodeList batchNodeList = responseXmlDocument.GetElementsByTagName("id");
                    XmlNodeList stateNodeList = responseXmlDocument.GetElementsByTagName("state");
                    //foreach (XmlNode xn in stateNodeList)
                    for (int i = 0; i < batchNodeList.Count; i++)
                    {

                        batchId = batchNodeList[i].InnerText;
                        Console.WriteLine("\n-----" + DateTime.Now + "---- JobId = " + jobId + "----batchId : " + batchId + "----- status : " + stateNodeList[i].InnerText);
                        if (stateNodeList[i].InnerText.CompareTo("Completed") == 0)
                        {

                            isCompleted = true;
                            if (batchId != null)
                            {
                                //retrieveResults(sessionId, jobId, batchId);
                            }
                        }
                        else if (stateNodeList[i].InnerText.CompareTo("Failed") == 0)
                        {
                            isCompleted = true;
                            if (batchId != null)
                            {
                                //retrieveResults(sessionId, jobId, batchId);
                            }
                        }
                        else
                        {
                            //  Thread.Sleep(sleepTime_ms);
                            isCompleted = false;
                        }
                    }

                    //responseHttpRequest.Dispose();
                }
                //closeJob(jobId);
            }
            catch (Exception e1)
            {
                Console.Write("Error in get batch status...");
            }
        }
    }    
}