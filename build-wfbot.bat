@echo off
dotnet publish WFBot -o out/WFBotWindows -c "Windows Release"
dotnet publish WFBot -o out/WFBotLinux -c "Linux Release" /p:UseAppHost=false