cd..
rem ���ù���Ŀ¼
set WorkDir=%cd%\Production\

rem ����վ�����Ŀ¼
set CleanDir=%WorkDir%Website\
rem ���վ���еĸ���Ŀ¼
for /f "tokens=1,2 delims= " %%i in (%WorkDir%publishwebsites.txt) do cd %CleanDir%%%j\wwwroot\js & RD /q /s plugin & RD /q /s winner & cd %CleanDir%%%j && RD /q /s de & RD /q /s es & RD /q /s fr & RD /q /s it & RD /q /s ja & RD /q /s ko & RD /q /s ru & RD /q /s runtimes & RD /q /s cs & RD /q /s pt-BR & RD /q /s refs & RD /q /s tr & RD /q /s pl  & RD /q /s zh-Hans & RD /q /s zh-Hant & RD /q /s cs & RD /q /s pl & RD /q /s pt-BR & RD /q /s refs & RD /q /s tr
rem ���վ���е��ļ�
cd %CleanDir% && del /a /f /s *.pdb
cd %CleanDir% && del /a /f /s *.exe
for /f "tokens=1 delims= " %%i in (%WorkDir%cleandll.txt) do cd %CleanDir% && del /a /f /s %%i 

cd %CleanDir%Beeant.Presentation.Service.Shared && RD /q /s wwwroot
pause