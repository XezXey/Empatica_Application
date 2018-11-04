using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Collections;

namespace EmpaticaBLEClient
{
    class CsvWriterFile
    {
        public static void StartWriter(string[] args)
        {
            using (var sr = new StreamReader(@"empatica_read.csv"))
            {
                using (var sw = new StreamWriter(@"empatica_written.csv"))
                {
                    var reader = new CsvReader(sr);
                    var writer = new CsvWriter(sw);

                    //CSVReader will now read the whole file into an enumerable
                    IEnumerable records = reader.GetRecords<DataRecord>().ToList();

                    //Write the entire contents of the CSV file into another
                    writer.WriteRecords(records);

                    //Now we will write the data into the same output file but will do it 
                    //Using two methods.  The first is writing the entire record.  The second
                    //method writes individual fields.  Note you must call NextRecord method after 
                    //using Writefield to terminate the record.

                    //Note that WriteRecords will write a header record for you automatically.  If you 
                    //are not using the WriteRecords method and you want to a header, you must call the 
                    //Writeheader method like the following:
                    //
                    //writer.WriteHeader<DataRecord>();
                    //
                    //Do not use WriteHeader as WriteRecords will have done that already.

                    foreach (DataRecord record in records)
                    {
                        //Write entire current record
                        writer.WriteRecord(record);

                        //write record field by field
                        writer.WriteField(record.Time);
                        writer.WriteField(record.Acceleration);
                        writer.WriteField(record.Galvanic_Skin_Response);
                        writer.WriteField(record.Blood_Volume_Pulse);
                        writer.WriteField(record.Heartbeat);
                        writer.WriteField(record.Interbeat_Interval);
                        writer.WriteField(record.Skin_Temperature);
                        writer.WriteField(record.Device_Battery);
                        writer.WriteField(record.Tag);
                        //ensure you write end of record when you are using WriteField method
                        writer.NextRecord();
                    }
                }
            }
        }

        private void WriteRecords(IEnumerable records)
        {
            throw new NotImplementedException();
        }
    }
}
