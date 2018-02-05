echo off
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll new customer tellick
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll new project tellickadmin tellick
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll active tellickadmin
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll log 4 "First"
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll log 4 "Second" 2018-2-1
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll show
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll show 2018-2
dotnet cli\bin\Debug\netcoreapp2.0\cli.dll show 2018