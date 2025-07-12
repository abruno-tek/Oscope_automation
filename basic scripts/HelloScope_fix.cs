/*
 * IMPORTANT ------------------------------------------------------------------------------------------------
 * The following program fixes an issue with the example code in the Tektronix 
 * C# automation document: https://www.tek.com/en/documents/application-note/c-sharp-getting-started-guide 
 * This program addresses the issue of the scope not triggering during an acquisition when there
 * is no signal present on channel 1 in the example.
 * In the C# automation document the issue is on pages 13 and 14.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa;

/* 
 * In order to make an acquisition, the scope must be able to trigger. Therefore, line 62
 * was added to force the scope to trigger during the acquisition. Line 45 was also added
 * in case the user would like to connect a BNC cable between the internal scope AFG
 * and channel 1, providing the user an actual signal to measure.
 */

namespace exScopeProgram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // VISA resource Address
            string visaRsrcAddr = "USB::0x0699::0x0522::PQ300127::INSTR";

            // open a connection to the instrument located at the visaRsrcAddr string
            // Cast the returned object as an IMessagedBasedSession. This is the interface of the
            // IVI VISA library used to send and receive commands and data
            var scope = GlobalResourceManager.Open(visaRsrcAddr) as IMessageBasedSession;

            // using statement ensures that the connection will be closed even if an exception is thrown.
            // Closes the connection properly with the instrument.
            using (scope)
            {
                // Query the instrument ID and print Response to console
                scope.FormattedIO.WriteLine("*IDN?"); // Query SCPI Command written to instrument
                Console.WriteLine(scope.FormattedIO.ReadLine()); // Write IDN returned details to console

                scope.TerminationCharacterEnabled = true;

                // Reset the instrument to default state and wait for it to complete.
                Console.Write("Resetting Instrument..."); // writing to console, Reset
                scope.FormattedIO.WriteLine("*RST");      // SCPI command write for instrument reset
                scope.FormattedIO.WriteLine("*OPC?");     // SCPI command Query that reset operation is complete.
                scope.RawIO.ReadString();   // Read characters into the return string
                Console.WriteLine("Reset Complete!");    // write reset complete message to console

                // Perform an Autoset and wait for operation to complete.
                // scope.FormattedIO.WriteLine("AFG:OUTPUT:STATE 1"); // Connect a BNC from AFG output on back to CH1 on front

                Console.Write("Autoset Instrument..."); // writing to console, Autoset
                scope.FormattedIO.WriteLine("AUTOSET EXECUTE"); // SCPI command write for instrument autoset operation
                scope.FormattedIO.WriteLine("*OPC?");   // SCPI command Query that autoset operation is complete.
                scope.RawIO.ReadString();
                Console.WriteLine("Autoset Complete!");

                // Add an Amplitude measurment and stop acquisition/acquiring
                scope.FormattedIO.WriteLine("MEASU:ADDMEAS AMPLITUDE"); // SCPI command to add amplitude measurement
                scope.FormattedIO.WriteLine("ACQ:STATE STOP");  // SCPI command to stop instrument acquisition
                scope.FormattedIO.WriteLine("*OPC?");   // SCPI command to Query the operations above are completed.
                scope.RawIO.ReadString(); // Read characters into return string, reading the operations above are completed.

                // Initiate a single acquisition and wait for it to complete.
                Console.Write("Performing Single Sequence ...");
                scope.FormattedIO.WriteLine("ACQ:STOPAFTER SEQUENCE"); // Set the acquisition to single sequence
                scope.FormattedIO.WriteLine("ACQ:STATE RUN"); // Start an acquisition
                scope.FormattedIO.WriteLine("TRIGGER FORCE"); // Forcing a trigger on the scope to complete acquisition
                scope.FormattedIO.WriteLine("*OPC?"); // Query for operation completed.
                scope.RawIO.ReadString();   // read chars into return string, reading the operations above are completed.
                Console.WriteLine("Done! \r\n"); // write message to console

                // Fetch the measurement result and print the result to console
                scope.FormattedIO.WriteLine("MEASU:MEAS1:RESULTS:CURRENTACQ:MEAN?"); // fetch the mean amplitude measurement (from measurement 1) from the acquisiton taken.
                float ampl = float.Parse(scope.FormattedIO.ReadLine()); // Reading buffer and converting string results to float using parse.
                Console.WriteLine($"Signal Amplitude: {ampl} Volts\r\n"); //Writing results to console

                Console.Write("Pess the Enter key to continue"); // Write to console to end program
                Console.ReadLine();
            }

        }
    }
}
