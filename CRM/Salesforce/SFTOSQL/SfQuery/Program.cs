using System;
using System.Configuration;
using System.Data.SqlClient;
using SfQuery.ServiceReference1;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;




namespace SfQuery
{
    public class Program
    {
        //static XDocument configXml;
        static int num_days;
        private static SalesforceClient CreateClient()
        {
            //configXml = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "/appCfg.xml");
            return new SalesforceClient
            {
                Username = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"],
                Token = ConfigurationManager.AppSettings["token"],
                ClientId = ConfigurationManager.AppSettings["clientId"],
                ClientSecret = ConfigurationManager.AppSettings["clientSecret"]
            };
        }

        static void Main(string[] args)
        {
            var client = CreateClient();
            num_days = int.Parse(ConfigurationManager.AppSettings["num_days"]);

            if (args.Length > 0)
            {
                client.Login();
                Console.WriteLine(client.Query(args[0]));
            }
            else
            {
                client.Login();

                //Console.WriteLine(client.Describe("Contact"));
                // Console.WriteLine(client.QueryEndpoints());
                // Console.WriteLine(client.Query("Select ID,Firstname,Phone,MobilePhone,email,ssbcrm__str_dwId__c,createddate,lastmodifieddate,lastactivitydate from Contact"));

                int fullpull = int.Parse(ConfigurationManager.AppSettings["fullpull"]);

                String qry = "";
                if (fullpull == 1)
                {
                    qry = "Select ID,Firstname,Lastname,MailingStreet,MailingCity,MailingState,MailingPostalCode,MailingCountry,Phone,MobilePhone,email,ssbcrm__str_dwId__c,createddate,lastmodifieddate,lastactivitydate from Contact";
                }
               else
                {
                    qry = "Select ID,Firstname,Lastname,MailingStreet,MailingCity,MailingState,MailingPostalCode,MailingCountry,Phone,MobilePhone,email,ssbcrm__str_dwId__c,createddate,lastmodifieddate,lastactivitydate from Contact where SystemModStamp >= LAST_N_DAYS:" + num_days;
                }
                Console.WriteLine(qry);
                QueryResult accresult = new QueryResult();
                QueryOptions qx = new QueryOptions();
                qx.batchSize = 100000;
                //client.Query(qry,out accresult);
                String data = client.Query(qry);                

                
                //QueryResult data = client.Query(qry);
                //Console.WriteLine("Data : " + data);
              /* System.IO.StreamWriter sw = new System.IO.StreamWriter("E://Contacts.json");
                sw.Write(data);
                sw.Close();*/
                                
                JObject obj = JObject.Parse(data);
                JArray contacts = JArray.Parse(obj.SelectToken("records").ToString());
                Console.WriteLine("Contacts Count : " + contacts.Count);

                String connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                String tablename = ConfigurationManager.AppSettings["tablename"];
                // String connectionString = configXml.Descendants(XName.Get("connection")).First().Value.ToString();
                SqlConnection cn = new SqlConnection(connectionString);
                cn.Open();
                String nexturl = obj.SelectToken("nextRecordsUrl").ToString();
                while (nexturl != "" || nexturl != null)
                {
                    foreach (JObject contact in contacts)
                    {
                        Console.WriteLine(contact.ToString());
                        //Console.Read();
                        String Id = contact["Id"].ToString();
                        String FirstName = contact["FirstName"].ToString().Replace("\'","\'\'").Replace("\"","\"\"");
                        String LastName = contact["LastName"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String MailingStreet = contact["MailingStreet"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String MailingCity = contact["MailingCity"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String MailingState = contact["MailingState"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String MailingPostalCode = contact["MailingPostalCode"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String MailingCountry = contact["MailingCountry"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String Phone = contact["Phone"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        String Mobile_Phone = contact["MobilePhone"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"");
                        //String Mobile_Phone = "";
                        String email = contact["Email"].ToString();
                        String ssb_str_dwid = contact["ssbcrm__str_dwId__c"].ToString();
                        //String ssb_str_dwid = "";
                        String createddate = DateTime.Parse(contact["CreatedDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        String lastmodifieddate = DateTime.Parse(contact["LastModifiedDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        String lastactivitydate = null;
                        try
                        {
                            lastactivitydate = DateTime.Parse(contact["LastActivityDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch (Exception e1)
                        { }
                        Console.WriteLine(Id + " ==> " + FirstName + " ==> " + MailingStreet + " ==> " + MailingCity);

                        String str = "insert into " + tablename + "(contactID,firstname,lastname,Address1,City,State,ZipCode,Country,Phone_Number,Mobile_Phone,email,IQ_Customer_Dim_ID,createddate,lastmodifieddate,lastactivityDate,LoadDttm) values ('" + Id + "','" + FirstName + "','" + LastName + "','" + MailingStreet + "','" + MailingCity + "','" + MailingState + "','" + MailingPostalCode + "','" + MailingCountry + "','" + Phone + "','" + Mobile_Phone + "','" + email + "','" + ssb_str_dwid + "','" + createddate + "','" + lastmodifieddate + "','" + lastactivitydate + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                        try
                        {
                            SqlCommand cmd = new SqlCommand(str, cn);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e1) {
                            String strx = "update " + tablename + " set firstname='" + FirstName + "',LastName='" + LastName + "',Address1='" + MailingStreet + "',City='" + MailingCity + "',State='" + MailingState + "',ZipCode='" + MailingPostalCode + "',Country='" + MailingCountry + "',Phone_Number='" + Phone + "',Mobile_Phone='" + Mobile_Phone + "',Email='" + email + "',IQ_Customer_Dim_ID='" + ssb_str_dwid + "',createddate='" + createddate + "',lastmodifieddate='" + lastmodifieddate + "',lastactivitydate='" + lastactivitydate + "' where contactID='" + Id + "'";
                            try
                            {
                                SqlCommand cmd = new SqlCommand(strx, cn);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ee) { }
                        }
                    }
                    nexturl = obj.SelectToken("nextRecordsUrl").ToString();

                    //nexturl="https://login.salesforce.com/" + nexturl;
                   // Console.WriteLine("Next Url : " + nexturl);
                    //client.InstanceUrl = nexturl;
                    nexturl = "https://chicago-fire.my.salesforce.com" + nexturl;
                    Console.WriteLine("Next Url : " + nexturl);
                    data = client.QueryNext(nexturl);
                    Console.WriteLine(data);
                    obj = JObject.Parse(data);                    
                    contacts = JArray.Parse(obj.SelectToken("records").ToString());
                }
                cn.Close();
               
                Console.WriteLine("Data display completed Successfully...");
                //Console.Read();
            }
        }
    }
}