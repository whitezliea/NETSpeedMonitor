# NETSpeedMonitor
V0.0.2 跨平台控制台流量监控器
## Windows 平台 --> 由Visual Studio2022 / Rider编译
项目基于 npcap-1.78/dotnet8/sharppacp    
运行本项目前请安装npcap https://npcap.com/#download   
打包命令`dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false --configuration Release`
## Linux 平台 --> 由Rider编译
项目基于 libpcap/dotnet8/sharppacp   
运行本项目前请安装tcpdump  
打包命令`dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained false --configuration Release`
