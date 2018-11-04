using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpaticaBLEClient
{
    class DataRecord
    {
        //Should have properties which correspond to the Column Names in the file
        //i.e. CommonName,FormalName,TelephoneCode,CountryCode
        public String Time { get; set; }
        public String Acceleration { get; set; }
        public String Blood_Volume_Pulse { get; set; }
        public String Galvanic_Skin_Response { get; set; }
        public String Skin_Temperature { get; set; }
        public String Interbeat_Interval { get; set; }
        public String Heartbeat { get; set; }
        public String Device_Battery { get; set; }
        public String Tag { get; set; }

    }
}
