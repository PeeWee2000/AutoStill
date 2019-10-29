# AutoStill
The purpose of this project is to create a program that can monitor and control the entire process of distillation. The project will be phased, for the first phase water will be distilled and all initial bugs will be fixed. For the second phase functionality will be added to account for the characteristics of the liquid being distilled, ethanol will be the second solution used to determine what is needed to add all required functionality. Thrid, a multi chemical solution tbd (possibly isopropyl/ethanol) will be fractionally distilled to verify that any solution that is input can be effectively separated into the correct fractions.

Prior to commit https://github.com/PeeWee2000/AutoStill/commit/538846325b1abfa24250adbd55d59824950d018d this project was based on just an arduino as the main hardware component. If one wanted to revive the arduino opensourceness of this project it was near functional but I ran into issues with the stability of thermocouple readings and the reliability of the hardware in general. After lots of research I discovered a device much more suited to pumping in safe and easy to read thermocouple data into the arduino if one were to pursue that path: http://thesensorconnection.com/signal-conditioners/signal-conditioners/type-k-thermocouple-amplifier-signal-conditioner-0-5-vdc-out


Subsequent commits have switched over to using a Dataq DI2008 and a FTDI relay board to control data reads and switching due to the long term goals of this project. 

To run this software, plug in both a Dataq DI2008 and a SainSmart 8 channel relayboard and then just run the program. It wont work properly until all hardware is connected to both the relay board and the data logger but it will run so you can see basic usage. When connecting everything it is useful to note that relay outputs are listed in the appconfig and sensor inputs are outlined in the "InitializeDI2008" function in the backgroundworkers class.

Hardware used in this project includes:
   - Dataq DI2008
   - SainSmart 8 Channel Relay Controller
   - 2x DPDT Relays
   - 1x SSR High Voltage/Amperage Relay w/ Heatsink
   - 1x Vacuum/Pressure Transducer (-14.5psi - +30psi)
   - 1x High Accuracy thermocouple
   - Nx Extra thermocouples (optional but can provide some cool data)
   - 2x Motorized Ball Valves (subject to change, phase separation will like require at least one more)
   - 2x Fluid Pumps (subject to change, a vacuum pump in combo with the valves may eliminate their necessity)
   - 4x Float switches - 2 per vessel
 

![DaStill](https://github.com/PeeWee2000/AutoStill/blob/master/Setup.JPG)
