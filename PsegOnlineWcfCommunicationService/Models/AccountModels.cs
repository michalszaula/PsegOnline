using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Collections.Generic;

namespace PsegOnlineWcfCommunicationService.Models
{

    [DataContract]
    public class UserProfile
    {
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string UserEmail { get; set; }

        [DataMember]
        public bool IsConfirmed { get; set; }

        [DataMember]
        public Guid AthCode { get; set; }

        [DataMember]
        public bool IsFreeUser { get; set; }

        [DataMember]
        public virtual UserProfile UserSupervisor { get; set; }

        
    }

    [DataContract]
    public class UserData
    {
        [DataMember]
        public int DataId { get; set; }

        [DataMember]
        public int DataUserId { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string AdressStreetName { get; set; }

        [DataMember]
        public string AdressCity { get; set; }

        [DataMember]
        public string AdressHomeNumber { get; set; }

        [DataMember]
        public string AdressLocalNumber { get; set; }

        [DataMember]
        public string PostCode { get; set; }

        [DataMember]
        public DateTime NewDataDateTime { get; set; }

    

        [DataMember]
        public string PsegOnlineAccessPassword { get; set; }

        [DataMember]
        public string RFIDCardNumber { get; set; }
    }
    

    
}

