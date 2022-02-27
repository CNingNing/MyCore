cd..
set SolutionDir=%cd%\Codes\
set PublishDir=%cd%\Production\



for /f "tokens=1,2 delims= " %%i in (%cd%\Production\publishwebsites.txt) do  cd %SolutionDir%%%i\%%j && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false -c Release -o %PublishDir%beeant\www\websites\%%j

pause