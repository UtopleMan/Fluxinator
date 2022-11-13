# Fluxinator
UI for Flux CD
![Main UI](https://github.com/UtopleMan/Fluxinator/blob/main/images/mainscreen.png)

Right click on the maui solution
Open in terminal
Run publish command: msbuild /restore /t:Publish /p:TargetFramework=net6.0-windows10.0.19041 /p:configuration=release /p:WindowsAppSDKSelfContained=true /p:Platform=x64 /p:PublishSingleFile=true /p:WindowsPackageType=None /p:RuntimeIdentifier=win10-x64
For x86 run: msbuild /restore /t:Publish /p:TargetFramework=net6.0-windows10.0.19041 /p:configuration=release /p:WindowsAppSDKSelfContained=true /p:Platform=x86 /p:PublishSingleFile=true /p:WindowsPackageType=None /p:RuntimeIdentifier=win10-x86 
Using 'dotnet publish' is only working sometimes so stay away from it
.exe can be found under \bin\x64\release\net6.0-windows10.0.19041\win10-x64\publish\ or \bin\x86\release\net6.0-windows10.0.19041\win10-x86\publish\





dotnet -f net6.0-windows10.0.19041 -c release --sc -r win10-x64 