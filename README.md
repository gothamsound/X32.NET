# X32.NET
A .NET library for OSC communications with X32 and X-Air

This project is a C# implementation of the OSC protocol specifically designed to interact with Behringer X32 and X-Air devices. It is NOT an official product, nor associated with Behringer in any way, so use it at your own risk.

Much of the work on this has been possible thanks to Patrick-Gilles Maillot's research on the X32 OSC protocol. His website, with a link to the latest unofficial documentation, is located at https://sites.google.com/site/patrickmaillot/x32.

The project is in-progress and contains basic functionality to send OSC commands given an X32's IP address. Subscriptions are being worked on but are not complete. However, the current state of the x32server class allows for some powerful control surface possibilities. The Main() method in Program.cs shows some usage examples.

Eventually, this code will all be streamlined into an actual class script and compiled to a .DLL for use with other .NET applications (such as ASP.NET web pages). The code is being written in a platform-agnostic manner to allow for use on OSX/Linux/etc via the Mono framework without having to port any Windows libraries.

-Will C.
