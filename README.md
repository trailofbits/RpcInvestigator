# RPC Investigator

Intro - TODO

## Usage

Workflow with screenshots - TODO

### Configuration

The Rpc Investigator has several configuration settings.

| Setting | Description | Default |
|---------|-------------|---------|
| dbghelp.dll | File location of the `dbghelp.dll` module | Find latest version within installed Windows Kits. |
| Symbol Path | Path to Windows symbols, which can be a symbol server or local directory | Default public Windows Server: `srv*c:\symbols*https://msdl.microsoft.com/download/symbols` |
| Trace Level | The logging trace level | Info |

The configuration settings can be modified within the application through the **Edit -> Settings** menu.

## Development Environment

1. Install [Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/), make sure to select the **.NET Desktop Development** workflow.
2. Download and install the latest [Windows 10 SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/). Perform a full installation so that .NET 4.8.1 and Debugging Tools are installed.
3. Open the Solution and verify that the projects loaded correctly. If there is an error about missing .NET 4.8.1 Targeting Pack, download and install [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481) and then restart Visual Studio.

