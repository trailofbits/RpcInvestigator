# RPC Investigator

Please read our [blog post announcement](https://blog.trailofbits.com/2023/01/17/rpc-investigator-microsoft-windows-remote-procedure-call/).

RPC Investigator (RPCI) is a .NET/C# Windows Forms UI application that provides an advanced discovery and analysis interface to Windows RPC endpoints. The tool provides a visual interface around the existing core RPC capabilities of the [NtApiDotNet](https://github.com/googleprojectzero/sandbox-attacksurface-analysis-tools/tree/main/NtApiDotNet) platform, including:

* Enumerating all active ALPC RPC servers
* Parsing RPC servers from any PE file
* Parsing RPC servers from processes and their loaded modules, including services
* Pulling symbol information from a Symbol Server
* Exporting RPC server definitions as serialized .NET objects for your own scripting

Beyond these core features, RPCI provides additional capabilities:

* The Client Workbench allows you to create and execute an RPC client binary on-the-fly by right-clicking on an RPC server of interest. The workbench has a C# code editor pane that allows you to edit the client in real time and observe results from RPC procedures executed in your code.
* Discovered RPC servers are organized into a searchable library, allowing you to pivot RPC server data in useful ways, such as searching all RPC procedures for all servers for interesting routines through a customizable search interface.
* The RPC Sniffer tool adds visibility into RPC-related ETW data to provide a near real-time view of active RPC calls. By combining ETW data with RPC server data from NtApiDotNet, we can build a more complete picture of ongoing RPC activity.

## Common Workflows

There are several workflows that the RPC Investigator supports:

- **Auditing**
  - Enumerating all active ALPC RPC servers across all processes that are communicating with an ALPC endpoint
  - Enumerating all RPC servers running in a Windows service
  - Loading offline RPC servers defined in a PE file (such as an EXE or DLL)
- **Interactive**
  - Client Workbench: Automatically generate RPC client code that can be customized and used to call into any RPC service.
  - RPC Sniffer: Realtime monitor of RPC-related Event Tracing for Windows (ETW) data.

## Example Workflow: Analyzing the Task Scheduler RPC

In this example, we'll be inspecting the Windows Task Scheduler RPC service, which is used to manage and execute scheduled tasks. We'll find the service, generate client code, and then customize the client to interact with one of the exposed procedures.

First, load the Windows services list by clicking **File -> Load From Service**. This opens a new service list window:

![](docs/img/ServiceListWindow.png)

Find the **Schedule** service, which is the Windows Task Scheduler, select the service and click **Go**.

![](docs/img/ScheduleService.png)

You will be prompted prior to RPCI loading all associated RPC DLLs. Click **Yes** to continue. Once loaded, you will see a list of all RPC servers discovered across all modules loaded in the service process. The Windows Task Scheduler RPC server has an Interface ID of [`86D35949-83C9-4044-B424-DB363231FD0C`](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-tsch/fbab083e-f79f-4216-af4c-d5104a913d40). Find the row within the list that has this Interface ID, which should have a running service named **Task Scheduler**, right-click on the row and select **New Client**.

![](docs/img/TaskSchedulerClient.png)

The left portion of the client window shows RPC server metadata and command line output from the client code. The right side shows two tabs:

- **Client Code** - Auto generated C# client code that can be customized to interact with one or more procedures. 
- **Procedures** - List of exposed RPC procedures.

In this example we'll be calling the [`SchRpcHighestVersion`](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-tsch/b266c231-52db-4244-88da-725cf2a9557a) procedure. This method accepts a single argument, `out int version`, which, after calling the procedure, will contain the highest Task Scheduler protocol version supported by the RPC interface. The high 16-bits are the major version and the low 16-bits are the minor version.

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
           Console.WriteLine("call to SchRpcHighestVersion failed with error: {0:X}", status);
       }
       return true;
   }
   ```

3. After adding this code, run the client by clicking the **Run** button. This will compile the C# code and then execute the **`Run`** method.
   - You will see a popup box with any compilation errors if the client code could not be compiled.

If compilation is successful, you will see something similar to the following in the **Output** box:

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

## Troubleshooting

After its initial release, RPC Investigator was converted from a .NET Framework application to a .NET 7 application. If you run into build issues, make sure Visual Studio is up-to-date. Wiping all build output folders prior to building from the .NET Framework version is a good idea.

Also, due to the move from the insecure BinaryFormatter class to protobuf-net, RPC libraries generated with the .NET framework version are incompatible with the .NET version.

If you're experiencing random crashes in RPC Investigator, you might find a solution in asking your administrator to tweak your EDR. We have found that some EDRs do not behave sanely with JIT'ed languages.

## Development Environment

1. Install [Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/), make sure to select the **.NET Desktop Development** workflow.
2. Download and install the latest [Windows 10 SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/). Perform a full installation so that .NET 4.8.1 and Debugging Tools are installed.
3. Open the Solution and verify that the projects loaded correctly. If there is an error about missing .NET 4.8.1 Targeting Pack, download and install [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481) and then restart Visual Studio.

## Further Reading

Because Windows RPC has been a popular research topic for well over a decade, there are too many related resources and research efforts to name here. We've listed a few below that we encountered while building this tool:

* https://clearbluejar.github.io/posts/surveying-windows-rpc-discovery-tools/
* https://www.powerofcommunity.net/poc2019/James.pdf
* https://www.tiraniddo.dev/2022/06/finding-running-rpc-server-information.html 
* https://clearbluejar.github.io/posts/from-ntobjectmanager-to-petitpotam/ 
* https://itm4n.github.io/from-rpcview-to-petitpotam/ 
* https://learn.microsoft.com/en-us/windows/win32/rpc/rpc-security-essentials 
* https://www.cyberark.com/resources/threat-research-blog/understanding-windows-containers-communication 
* https://github.com/silverf0x/RpcView
* https://github.com/xpn/RpcEnum
* https://github.com/cyberark/RPCMon 
* https://github.com/tyranid/WindowsRpcClients 

If you're unfamiliar with RPC internals or need a technical refresher, we would recommend one of the authoritative sources on the topic - Alex Ionescu's 2014 SyScan talk in Singapore, [All about the RPC, LRPC, ALPC, and LPC in your PC](https://www.youtube.com/watch?v=UNpL5csYC1E).
