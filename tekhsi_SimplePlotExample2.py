
'''An example script for connecting to a scope, setting up the trigger and channels, acquiring a waveform,
   retrieving waveform data from a channel, and plotting the waveform data.

IMPORTANT NOTES
- Please change the ADDRESS to your scope's IP address (ADDRESS)
- This program uses an MSO5B, (from tm_device.driver import MSO5B)
- IMPORTANT: MAKE SURE HSI is enable on the scope! On the scope: Utility > I/O > HIGH SPEED INTERFACE > High Speed Interface > (Toggle) "On"
- This example uses matplotlib, tm_devices, tm_data_types, and tekhsi
- Make sure to attach a Passive Probe to channel 1 of the oscilloscope and the probe tip to the Probe Compensation signal (square wave)


PYTHON LIBRARIES NEEDED:
- tm_devices: https://pypi.org/project/tm-devices/
- tekhsi:  https://pypi.org/project/TekHSI/ 
- tm_data_types: https://pypi.org/project/tm-data-types/ 

HARDWARE/SOFTWARE:
- Tektronix 5 Series B MSO (this is what is used for this test): https://www.tek.com/en/products/oscilloscopes/5-series-mso
- Tektronix TPP1000 Passive Probe: https://www.tek.com/en/products/oscilloscopes/oscilloscope-probes/passive-probe 
- Tektronix VISA: https://www.tek.com/en/support/software/driver/tekvisa-connectivity-software-v5111 
- (optionally, if TEK-VISA isn't working) NI-VISA: https://www.ni.com/en/support/downloads/drivers/download.ni-visa.html?srsltid=AfmBOoqugeCeo3YhB3XHr7s8wA2CKJqP4In-K5igqh9XGhW0MUFB1Kcw#565016 
   you can use the conflict manager with TEK-VISA to change the visa used to NI-VISA instead.

'''

# Importing modules, packages, libraries etc.
import matplotlib.pyplot as plt
from tm_devices import DeviceManager                                   # https://pypi.org/project/tm-devices/
from tm_devices.drivers import MSO5                                    # Importing Drivers for Tektronix MSO58B, https://pypi.org/project/tm-devices/
from tm_data_types import AnalogWaveform                               # Importing Waveform Class, https://pypi.org/project/tm-data-types/ 
from tekhsi import TekHSIConnect                                       # Importing TekHSIConnect, https://pypi.org/project/TekHSI/ 

# Scope VISA, IP, Port Settings
ADDRESS = "10.233.65.146"                                              # Instrument IP Address

with DeviceManager() as dm:                                            # Create a DeviceManager Object (dm)

   MSO_scope: MSO5 = dm.add_scope(ADDRESS)                             # adding a scope object
   MSO_scope.visa_timeout = 5000                                       # 5000 ms VISA timeout        

   scope = MSO_scope.commands                                          # Aliasing

   print(scope.idn.query())                                            # printout scope ID

   # User input for connecting channel 1
   input('''
   ACTION
   Connect probe to Oscilloscope Channel 1 and the Probe Compensation Signal.
   Press Enter to continue....
   ''')
   
   print("Scope Setup, Acquire, Waveform transfer, and plotting....")

   scope.rst.write()                                                   # scope reset
   scope.opc.query()                                                   # OPC command

   scope.display.waveview1.ch[1].state.write("ON")                    

   scope.trigger.a.type.write("EDGE")                                  # set edge trigger to ch2
   scope.trigger.a.edge.source.write("CH1")                            
   scope.opc.query()                                                   

   scope.autoset.write("EXECUTE")                                      # scope autoset
   scope.opc.query()                                                   
   scope.acquire.state.write("0")                                      # Setup single acquisition
   scope.acquire.stopafter.write("SEQUENCE")                           
   scope.acquire.state.write("1")                                      # single acquisition
   scope.opc.query()

   scope.acquire.stopafter.write("RUNSTOP")
   scope.opc.query()

   with TekHSIConnect(f"{ADDRESS}:5000") as connection:                # Establish a tekhsi connection to the oscilloscope.
      with connection.access_data():                                   # Begin accessing data from the oscilloscope with connections
         waveform: AnalogWaveform = connection.get_data("ch1")         # Retrieve the waveform data for channel 2 from the scope, waveform object

   hd = waveform.normalized_horizontal_values                          # Extract the normalized horizontal (time) values from waveform.
   vd = waveform.normalized_vertical_values                            # Extract the normalized vertical (voltage) values from waveform.

      # Plotting the waveform data
   _, ax = plt.subplots()                                              # Create a new figure and subplot
   ax.plot(hd, vd)                                                     # Plot the waveform
   ax.set(xlabel=waveform.x_axis_units,                                # plot axes labeling
         ylabel=waveform.y_axis_units, title="Simple Plot")
   plt.show()                                                          # Display plot

print("End of tm_devices simple plot example")