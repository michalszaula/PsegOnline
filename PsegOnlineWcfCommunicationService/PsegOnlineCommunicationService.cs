using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.SqlClient;
using J_RFID;
using PCSC;
using PCSC.Iso7816;
using Limilabs.FTP.Client;
using PsegOnlineWcfCommunicationService.Models;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace PsegOnlineWcfCommunicationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class PsegOnlineCommunicationService : IPsegOnlineCommunicationService
    {
        string connectionString = "Data source=MSSQL1.kkibci.pl;Initial catalog=PsegDB;User Id=administrator;Password=p$egp$eg1234567890;";
        //string connectionString = "Data source=78.8.35.134\\firma,56247;Initial catalog=PsegDB;User Id=Alfred;Password=Alf22r@d0;";

        public string GetUser(int UserId)
        {
            return string.Format("You entered: {0}", UserId);
        }

        public List<UserProfile> GetAllUsers()
        {
            List<UserProfile> usersList = new List<UserProfile>();
    
            UserProfile user;
        
            try
            {
                using (SqlConnection SqlConn = new SqlConnection(connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand("Select * From UserProfile", SqlConn);
                    SqlConn.Open();
                    SqlDataReader reader = sqlCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        user = new UserProfile();

                        user.UserId = int.Parse(reader[0].ToString());
                        user.UserEmail = reader[1].ToString();
                       
                        
                        usersList.Add(user);
                    }
                    SqlConn.Close();

                    
                }
            }
            catch (Exception ex)
            {
                
                return null;
            }

            return usersList;
        }

        public List<UserData> GetAllUsersData()
        {

            List<UserData> dataList = new List<UserData>();

            UserData data;
            try
            {
                using (SqlConnection SqlConn = new SqlConnection(connectionString))
                {


                    SqlCommand sqlCmd = new SqlCommand("Select * From UserData", SqlConn);
                    SqlConn.Open();
                    SqlDataReader reader = sqlCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        data = new UserData();
                        data.DataId = int.Parse(reader[0].ToString());
                        string userId = reader[1].ToString();
                        if (!String.IsNullOrWhiteSpace(userId)) data.DataUserId = int.Parse(reader[1].ToString());
                        
                        data.FirstName = reader[2].ToString();
                        data.LastName = reader[3].ToString();
                        data.AdressStreetName = reader[4].ToString();
                        data.AdressCity = reader[5].ToString();
                        data.AdressHomeNumber = reader[6].ToString();
                        data.AdressLocalNumber = reader[7].ToString();
                        data.PostCode = reader[8].ToString();
                        data.NewDataDateTime = DateTime.Parse(reader[9].ToString());
                      
                        
                        data.RFIDCardNumber = reader[14].ToString();
                        data.PsegOnlineAccessPassword = reader[15].ToString();
                        dataList.Add(data);
                    }
                    SqlConn.Close();
                }
            }
            catch (Exception ex)
            {

                return null;
            }

            return dataList;
        }

        

        public void WriteRfidRequest()
        {

            
        }
        public string ReadRfidCard(byte COMPort=0)
        {
            if (COMPort == 0) COMPort = 0x05;

            string Uid="";
            string CType="";

            try
            { 
                RFIDAPI rfidApi = new RFIDAPI();
                rfidApi.RFID_OpenReader(COMPort);
                rfidApi.RFID_OpenCard(out Uid, out CType);
                rfidApi.RFID_CloseCard();
                rfidApi.RFID_CloseReader(0x05);
            }
            catch(Exception ex)
            {
                Uid = "error";
            }
            if (String.IsNullOrWhiteSpace(Uid)) Uid = "none";
            if (String.IsNullOrWhiteSpace(CType)) CType = "none";

            return Uid;
        }
        public string ReadRfidInformation()
        {
            string apiVersion="";
            string firmwareVersion="";
            string cType="";
            try
            { 
                
                RFIDAPI rfidApi = new RFIDAPI();
                //rfidApi.RFID_OpenReader(0x05);
                //rfidApi.RFID_OpenCard(out readedData, out cType);

                rfidApi.RFID_GetAPIVersionString(out apiVersion);
                rfidApi.RFID_FWVersion(out firmwareVersion);
                

                //rfidApi.RFID_CloseCard();
                //rfidApi.RFID_CloseReader(0x05);

                return apiVersion+" ; "+firmwareVersion;

            }catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public string ReadSmartCard()
        {
            using (var context = new SCardContext())
            {
                context.Establish(SCardScope.System);
                string readerName = null;
                try
                {
                    string[] readerNames = context.GetReaders();
                    readerName = readerNames[0];
                }
                catch(Exception ex)
                {
                    return "error";
                }

                if (readerName == null)
                {
                    return "error";
                }

                using (var rfidReader = new SCardReader(context))
                {

                    var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        return "error";//"Could not connect to reader {0}:\n{1}";
                            
                       
                    }

                    var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.ActiveProtocol)
                    {
                        CLA = 0xFF,
                        Instruction = InstructionCode.GetData,
                        P1 = 0x00,
                        P2 = 0x00,
                        Le = 0  // We don't know the ID tag size
                    };

                    sc = rfidReader.BeginTransaction();
                    if (sc != SCardError.Success)
                    {
                        return "none";// "Could not begin transaction.";
                        
                        
                    }

                    

                    var receivePci = new SCardPCI(); // IO returned protocol control information.
                    var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                    var receiveBuffer = new byte[256];
                    var command = apdu.ToArray();

                    sc = rfidReader.Transmit(
                        sendPci,            // Protocol Control Information (T0, T1 or Raw)
                        command,            // command APDU
                        receivePci,         // returning Protocol Control Information
                        ref receiveBuffer); // data buffer

                    if (sc != SCardError.Success)
                    {
                        return "none";//SCardHelper.StringifyError(sc);
                    }

                    var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);

                    rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                    rfidReader.Disconnect(SCardReaderDisposition.Reset);

                    int id = responseApdu.HasData ? BitConverter.ToInt32(responseApdu.GetData(),0) : -1;
                    //int id = responseApdu.HasData ? System.Text.Encoding.UTF8.GetString(responseApdu.GetData()) : "none";

                    if (id < 0) id = id * (-1);
                    return id.ToString();
                        
                }
            }
            return "none";
        }

        public string GetFileFromFTP(string host, int port, string remoteFolderPath, string localFolderPath, string username, string password, List<string> userCodesList, string fileName)
        {
            
            //host="83.12.64.6";
            //port = 12024;
            //username = "alfredftp";
            //password="Alfr@d22FTP";
            //remoteFolderPath = "Test2";
            //localFolderPath = @"C:\";
            //remoteFolderPath = "";
            //userCodesList = new List<string>();
            //userCodesList.Add("PL0091");
       
          
            try
            {
                using (Ftp client = new Ftp())
                {

                    client.Connect(host, port);    // or ConnectSSL for SSL

                    client.Login(username, password);

                    
                    client.ChangeFolder(remoteFolderPath);
                    string filePath = localFolderPath + "\\" + fileName;
                            
                    if (!File.Exists(filePath))
                    {
                        byte[] bytes = client.Download(fileName);
                        

                        MemoryStream stream = new MemoryStream(bytes);

                        XElement xelement = XElement.Load(stream);//XElement.Parse(xelementString);
                                
                        xelement.Save(filePath);
                       
                    }
                            
                          
                    
                    
                    client.Close();

                }
            }
            catch(Exception ex)
            {
                return "error-"+ex.Message;
            }
            return "ok";//+ " plików dla kodów "+codesString;
        }

        public List<string> GetFilesNamesFromFTP(string host, int port, string remoteFolderPath, string localFolderPath, string username, string password, List<string> userCodesList)
        {

            //host="83.12.64.6";
            //port = 12024;
            //username = "alfredftp";
            //password="Alfr@d22FTP";
            //remoteFolderPath = "Test2";
            //localFolderPath = @"C:\";
            //remoteFolderPath = "";
            //userCodesList = new List<string>();
            //userCodesList.Add("PL0091");
            List<string> filesList = new List<string>();
 
            try
            {
                using (Ftp client = new Ftp())
                {

                    client.Connect(host, port);    // or ConnectSSL for SSL

                    client.Login(username, password);

                    //client.Download(@"reports\report.txt", @"c:\report.txt");
                    //client.DownloadFiles(remoteFolderPath, localFolderPath);
                    //RemoteSearchOptions option = new RemoteSearchOptions("*.xml", true);

                    //client.DeleteFiles(remoteFolderPath, option);
                    //client.DeleteFolder(remoteFolderPath);
                    //client.CreateFolder(remoteFolderPath);
                    //byte[] bytes = client.Download(@"reports/report.txt");
                    //string report = Encoding.UTF8.GetString(bytes,0,bytes.Length);

                    client.ChangeFolder(remoteFolderPath);
                    List<FtpItem> items = client.GetList();
                    
                    foreach (FtpItem item in items)
                    {
                        if (item.IsFile)
                        {
                            filesList.Add(item.Name);
                            /*
                            string filePath = localFolderPath + "\\" + item.Name;
                            currentFileName = item.Name;
                            if (!File.Exists(filePath))
                            {
                                byte[] bytes = client.Download(item.Name);
                                //string xelementString = Encoding.UTF8.GetString(bytes,0,bytes.Length);
                                //xelementString = getRidOfUnprintablesAndUnicode(xelementString);

                                MemoryStream stream = new MemoryStream(bytes);

                                XElement xelement = XElement.Load(stream);//XElement.Parse(xelementString);

                                xelement.Save(filePath);
                                downloadedFilesCount++;
                            }

                            
                            var sender = (from nm in xelement.Elements("Sender") select nm).FirstOrDefault();
                            string code = sender.Element("Code").Value;
                            code = userCodesList.Where(c=>c==code).FirstOrDefault();
                            if (code != null)
                            {
                                xelement.Save(localFolderPath + "\\" + item.Name,);
                                
                                client.DeleteFile(item.Name);
                                downloadedFilesCount++;
                            }
                            */

                        }
                    }

                    client.Close();

                }
            }
            catch (Exception ex)
            {
                return new List<string> { "error-"+ex.Message};
            }
            return filesList;
        }

        public string getRidOfUnprintablesAndUnicode(string inpString)
        {
            string outputs = String.Empty;
            for (int jj = 0; jj < inpString.Length; jj++)
            {
                char ch = inpString[jj];
                if (((int)(byte)ch) >= 32 & ((int)(byte)ch) <= 128)
                {
                    outputs += ch;
                }
            }
            return outputs;
        } 
    }
}
