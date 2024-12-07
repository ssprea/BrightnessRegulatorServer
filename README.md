Brightness Regulator is a two-part (server and client) application that allows you to adjust the brightness and contrast of a monitor directly from your phone.
The project uses C# and Avalonia UI for the GUI. Hardware settings are managed via Windows API, using DLLs to interact with the monitor's brightness and contrast controls.

Description

The server is responsible for:
    Retrieving the current brightness and contrast settings from the monitor.
    Sending them to the client via a TCP connection.
    Receiving new settings from the client.
    Applying the new settings to the monitor.

Main functions
    TCP connection management: The server listens on a specific port (45743) for incoming connections from the client.
    Hardware interaction: Uses the PhysicalMonitorController class to retrieve and apply hardware settings.
    Data Serialization: Brightness and contrast settings are serialized in JSON format for transmission to the client.
    Asynchronous management: All network operations are implemented asynchronously to ensure smooth execution.

Dependencies
    .NET (>= 6.0)
    A custom library or class called PhysicalMonitorController to interact with the monitor.

Structure
    Main: Starts the server, accepts connections and manages the main loop.
    RetrieveSettings: Retrieves current brightness and contrast.
    SendSettings: Sends serialized data to the client.
    ReceiveSettings: Receives new settings from the client.
    ApplySettings: Applies the received settings to the monitor.

