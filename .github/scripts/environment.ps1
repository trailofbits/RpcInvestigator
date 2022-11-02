
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))  
{  
  $arguments = "& '" +$myinvocation.mycommand.definition + "'"
  Start-Process powershell -Verb runAs -ArgumentList $arguments
  Break
}

$url = "https://aaron.nyc3.digitaloceanspaces.com/X64%20Debuggers%20And%20Tools-x64_en-us.msi"
$file = "$env:temp\debuggingtools.msi"
Invoke-WebRequest $url -OutFile $file
$log = "install.log" 
$procMain = Start-Process "msiexec" "/i `"$file`" /qn /l*! `"$log`"" -NoNewWindow -PassThru
$procLog = Start-Process "powershell" "Get-Content -Path `"$log`" -Wait" -NoNewWindow -PassThru 
$procMain.WaitForExit()
$procLog.Kill()