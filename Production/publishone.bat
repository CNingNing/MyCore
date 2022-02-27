cd..
set SolutionDir=%cd%\MyCode\
set PublishDir=%cd%\Production\



for /f "tokens=1,2 delims= " %%i in (%cd%\Production\publishone.txt) do  cd %SolutionDir%%%i\%%j && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false -c Release -o %PublishDir%webset\%%j

pause