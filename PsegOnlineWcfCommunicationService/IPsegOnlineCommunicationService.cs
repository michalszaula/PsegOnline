using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using PsegOnlineWcfCommunicationService.Models;

namespace PsegOnlineWcfCommunicationService
{

    [ServiceContract(Namespace = "http://localhost:10000/PsegServices/CommunicationService")]
    public interface IPsegOnlineCommunicationService
    {
        
        [OperationContract]
        string GetUser(int UserId);

        [OperationContract]
        List<UserProfile> GetAllUsers();


        [OperationContract]
        List<UserData> GetAllUsersData();




        [OperationContract]
        void WriteRfidRequest();

        [OperationContract]
        string ReadRfidCard(byte COMPort);

        [OperationContract]
        string ReadRfidInformation();

        [OperationContract]
        string ReadSmartCard();

        [OperationContract]
        string GetFileFromFTP(string host, int port, string remoteFolderPath, string localFolderPath, string username, string password, List<string> userCodesList, string fileName);

        [OperationContract]
        List<string> GetFilesNamesFromFTP(string host, int port, string remoteFolderPath, string localFolderPath, string username, string password, List<string> userCodesList);
    }

    
}
