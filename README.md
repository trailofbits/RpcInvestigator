# RPC Investigator

Intro - TODO

## Usage

There are several workflows that the RPC Investigator supports:

- **Auditing**: Enumerating all RPC servers from running processes, typically used to survey currently registered and executing RPC handlers.
- **Service Analysis**: Enumerating all services that provide at least one RPC handler, typically used to analyze an existing RPC service.
- **Handler Analysis**: Loading all RPC information from an RPC DLL, typically used to analyze a specific RPC service that may not be installed or running.

Each of these workflows will eventually lead to an RPC endpoint with multiple RPC procedures. The RPC Investigator can generate C# client code to interact with the endpoint.

### Example Workflow: Service Analysis

In this example, we'll be inspecting the Windows Task Scheduler RPC service, which is used to manage and execute scheduled tasks. We'll find the service, generate client code, and then customize the client to interact with one of the exposed procedures. First, load the RPC services list by clicking **File -> Load From Service**. This opens a new service list window:

![](docs/img/ServiceListWindow.png)

Find the **Schedule** service, which is the Windows Task Scheduler, select the service and click **Go**.

![](docs/img/ScheduleService.png)

You will be prompted prior to the investigator loading all associated RPC DLLs. Click **Yes** to continue. Once loaded, you will see a list of RPC handlers. The Windows Task Scheduler RPC service has an Interface ID of [`86D35949-83C9-4044-B424-DB363231FD0C`](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-tsch/fbab083e-f79f-4216-af4c-d5104a913d40). Find the row within the list that has this Interface ID, which should have a running service named **Task Scheduler**, right-click on the row and select **New Client**.

![](docs/img/TaskSchedulerClient.png)

The left portion of the client window shows RPC handler metadata and command line output from the client code. The right side shows two tabs:

- **Client Code** - Auto generated C# client code that can be customized to interact with one or more procedures. 
- **Procedures** - List of exposed RPC procedures.

In this example we'll be calling the [`SchRpcHighestVersion`](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-tsch/b266c231-52db-4244-88da-725cf2a9557a) procedure. This method accepts a single argument, `out int version`, which, after calling the procedure, will contain the highest Task Scheduler protocol version supported by the RPC, the high 16-bits are the major version and the low 16-bits are the minor version.

To call this procedure:

1. In the **Client Code** window, find the **`Run`** method, which is the main entry point for the RPC client.
2. Edit the **`Run`** method body to call the procedure:
   ```cs
   public async Task<bool> Run()
   {
       int version;
       int status = SchRpcHighestVersion(out version);
       if (status == 0) {
           long major = (version & 0xffff0000) >> 16;
           long minor = version & 0x0000ffff;
           Console.WriteLine("highest supported RPC version: {0}.{1}", major, minor);
       } else {
           Console.WriteLine("call to SchRpcHighestVersion failed with error: {0}", status);
       }
       return true;
   }
   ```

3. After adding this code, run the client by clicking the **Run** button. This will compile the C# code and then execute the **`Run`** method.
   - You will see a popup box with any compliation errors if the client code could not be compiled.

If compliation is successful, you will see something similar to the following in the **Output** box:

```
> Run() output:
highest supported RPC version: 1.6
```

![](docs/img/TaskSchedulerClient-Version.png)


### Configuration

The Rpc Investigator has several configuration settings.

| Setting | Description | Default |
|---------|-------------|---------|
| dbghelp.dll | File location of the `dbghelp.dll` module | Find latest version within installed Windows Kits. |
| Symbol Path | Path to Windows symbols, which can be a symbol server or local directory | Default public Windows Server: `srv*c:\symbols*https://msdl.microsoft.com/download/symbols` |
| Trace Level | The logging trace level | `info` |

The configuration settings can be modified within the application through the **Edit -> Settings** menu.

## Development Environment

1. Install [Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/), make sure to select the **.NET Desktop Development** workflow.
2. Download and install the latest [Windows 10 SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/). Perform a full installation so that .NET 4.8.1 and Debugging Tools are installed.
3. Open the Solution and verify that the projects loaded correctly. If there is an error about missing .NET 4.8.1 Targeting Pack, download and install [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481) and then restart Visual Studio.

