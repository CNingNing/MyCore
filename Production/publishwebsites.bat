cd..
set SolutionDir=%cd%\Code\
set PublishDir=%cd%\Production\Website



for /f "tokens=1,2 delims= " %%i in (%cd%\Production\publishwebsites.txt) do  cd %SolutionDir%%%i\%%j && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false -c Release -o %PublishDir%\%%j

pause