// IMPORTANT !!!!!!! <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
// Change the file name to Program.cs if adding to a new project. also Rename the namespace on line 30
// Also reference the https://www.tek.com/en/documents/application-note/c-sharp-getting-started-guide for general guidance on the project
// Make sure to set the VISA Resources Address on line 37 of the code.

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Ivi.Visa;
using System.Runtime.CompilerServices;
// IVI VISA library. Used to send SCPI commands, Receive data (via SCPI Commands), and interpret the SCPI commands in the VISA connection.

// Description
/*
 * This program is an example of C# automation code with Tektronix 5 Series B Mixed-Signal Oscilloscopes (MSO). 
 * This program opens a visa connection with the oscilloscope, 
 * resets the scope, sets up measurements, and configures the arbitrary Fucntion generator (AFG) on channel 1. 
 * To get proper results, a BNC connection must be between channel 1 of the oscilloscope and the 
 * AFG BNC out the rear of the 5 series B MSO. The program then loops over 100kHz, 1MHz, 5MHz, 10MHz, 50MHz
 * sine waves. The loop sets the frequency, autosets the scope at the specified sine wave frequency, captures 
 * a screen of the display and then saves those screen shots to a specified directory or drive.
 * 
*/

namespace automationExample
{
    internal class Program
    {
        static void Main(string[] args) // MAIN Run -----------------------------------------------------------------
        {
            // setup the Visa connection to the instrument, this VISA connection uses USB to the instrument
            string visaRsrcAddr = "USB::0x0699::0x0522::PQ300127::INSTR";        // Create a string of the VISA Resource Address
            var scope = GlobalResourceManager.Open(visaRsrcAddr) as IMessageBasedSession; // Open a VISA connection to the instrument
                                                                                          // Any returned object results are casted as an IMessageBasedSession

            // PATH Definition and user defined parameters ------------------------------------------
            string directory_path = "E:/";

            // TESTS
            List<double> doublelist = new List<double> {1e5, 1e6, 5e6, 1e7, 5e7};

            // --------------------------------------------------------------------------------------

            // USING scope statement - this statement will ensure a proper cleanup and closing of the Scope Object created above. Specifically, if any exceptions
            // are thrown by the SCPI command code below while running then the VISA connection will be properly closed out with the instrument BEFORE the program
            // quits.
            using (scope)
            {
                // ---------------------- SETUP OF SCOPE ----------------------------
                // Get the ID of the scope (a test of the VISA connection)
                Console.Write("ID of the Instrument");              // Write message to console
                scope.FormattedIO.WriteLine("*IDN?");                   // Query the scope for ID information with SCPI Command *IDN?
                Console.WriteLine(scope.FormattedIO.ReadLine());        // Write to console


                // Default set the scope and wait for default operation is complete.
                Console.Write("Resetting the scope ....");         // Write message to console
                scope.FormattedIO.WriteLine("*RST");                   // SCPI write *RST to reset/default the scope
                scope.FormattedIO.WriteLine("*OPC?");                  // Wait (query) for the default operation to complete, Sychronizes oscilloscope operations and automation code.
                scope.RawIO.ReadString();                              // Read the IO buffers confirm operation complete
                Console.WriteLine("Complete");                         // Write message to console


                // Setup the measurements (Vpk2pk, frequency) for source channel 1
                Console.Write("Setting up measurements...");                    // write message to console
                scope.FormattedIO.WriteLine("MEASUREMENT:MEAS1:TYPE FREQUENCY");    // Setup meas1 and the source to channel 1
                scope.FormattedIO.WriteLine("MEAUSREMENT:MEAS1:SOURCE CH1");
                scope.FormattedIO.WriteLine("*OPC?");                               // Wait for operation to complete
                scope.RawIO.ReadString();

                scope.FormattedIO.WriteLine("MEASUREMENT:MEAS2:TYPE PK2PK");        // Setup meas2 and the source to channel 1
                scope.FormattedIO.WriteLine("MEASUREMENT:MEAS2:SOURCE CH1");        
                scope.FormattedIO.WriteLine("*OPC?");                               // Wait for operation to complete
                scope.RawIO.ReadString();
                Console.WriteLine("Complete");


                // Setup and Configuration of the Arbritary Function Generator (AFG) on the scope
                Console.Write("Setting up AFG...");
                scope.FormattedIO.WriteLine("AFG:OUTPut:MODe CONTinuous");  // Turn on the AFG
                scope.FormattedIO.WriteLine("AUTOSET EXECUTE");             // Auto Set the scope
                scope.FormattedIO.WriteLine("*OPC?");                       // Wait (query) for the default operation to complete
                scope.RawIO.ReadString();
                Console.WriteLine("Complete");

                Thread.Sleep(1000);

                // --------------- LOOP -----------------------
                // List loop testing
                foreach (double number in doublelist)
                {
                    //Console.WriteLine(number);                            // debugging code for loop
                    scope.FormattedIO.WriteLine($"AFG:FREQuency {number}"); // Set the specified frequency from doublelist
                    scope.FormattedIO.WriteLine("AUTOSET EXECUTE");         // Autoset the scope
                    scope.FormattedIO.WriteLine("*OPC?");                   // wait until operations are complete on scope
                    scope.RawIO.ReadString();

                    Thread.Sleep(700);  // halt sending commands for 0.7 sec

                    scope.FormattedIO.WriteLine($"SAVe:IMAGe '{directory_path}test_{number}.png'");    // Save screen capture of frequency
                                                                                                       // Note, the save order is NOT equal to the <double> llist order above.
                    scope.FormattedIO.WriteLine("*OPC?");                                 // wait until operations are complete on scope
                    scope.RawIO.ReadString();
                }

                // --------------- END OF LOOP -----------------
                Console.WriteLine("Press the Enter key to continue."); // Write message to console
                Console.ReadLine();                                    // Read the user input ("Enter") 

                // Close out the VISA connection from the VISA Resource Address. Close the VISA session.
            }
        }
    }
}
