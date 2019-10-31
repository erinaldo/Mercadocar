@echo off
cls
echo “SCRIPT .BAT para apagar arquivos PDF de impressão”

del /q "C:\Users\fmoraes\Documents\Documento Sistema Mercadocar*"


del /q "C:\Users\fmoraes\Documents\Teste\*"
FOR /D %%p IN ("C:\Users\fmoraes\Documents\Teste\*.*") DO rmdir "%%p" /s /q
for /D %%x in ("C:\Users\fmoraes\Documents\Teste\") DO mkdir "%%x\B" "%%x\C"



:INI
@ECHO Off
cls
color 9e
echo.     @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.     @             COPIAR ARQUIVOS AGORA ?                @
echo.     @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.
echo.
color 9C
echo.                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.                @           OPCOES           @
echo.                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.
echo.
color 9A
echo.     @@@@@      @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.     @ C @      @    COPIAR ARQUIVOS AGORA ? @
echo.     @@@@@      @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.
echo.
echo.
color 9E
echo.     @@@@@      @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.     @ S @      @           SAIR ?           @
echo.     @@@@@      @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.
echo.---------------------------------------------------------
echo.
:aff
set /p opcao=****Digite a opcao--
if %opcao% equ S goto ex2
if %opcao% equ s goto ex2
if %opcao% equ C goto vb3
if %opcao% equ c goto vb3
:ex2
echo Saindo.................................
pause
exit
:vb3
echo gerando cópias.........................
@ECHO Off
echo @ECHO OFF >>TESTE.BAT
set hora=%time:~0,2%Hs%time:~3,2%Min
set data=%date:~4,2%-%date:~7,2%-%date:~-4%
set DIR=\\Robson\Videos\%data%
echo color F4>>TESTE.BAT
echo cls>>TESTE.BAT
echo if not exist %dir% goto %data% >>TESTE.BAT
echo if exist %dir% goto %data%PROXIMA >>TESTE.BAT
echo :%data% >>TESTE.BAT
echo md %dir% >>TESTE.BAT
echo :forca>>TESTE.BAT
echo @ECHO OFF >>TESTE.BAT
echo MOVE/-Y  *.txt %dir% >>TESTE.BAT
echo MOVE/-Y  *.log %dir% >>TESTE.BAT
ECHO goto %data%EXIT >>TESTE.BAT
echo :%data%EXIT >>TESTE.BAT
echo goto exit >>TESTE.BAT
echo @ECHO Off >>TESTE.BAT
echo :%data%PROXIMA >>TESTE.BAT
echo @ECHO JA EXISTE UMA PASTA COM ESTA DATA %data% >>TESTE.BAT
echo md %dir%\%hora% >>TESTE.BAT
echo @ECHO OFF >>TESTE.BAT
echo @ECHO OFF >>TESTE.BAT
echo MOVE/-Y  *.txt %dir%\%hora% >>TESTE.BAT
echo MOVE/-Y  *.log %dir%\%hora% >>TESTE.BAT
ECHO goto %data%EXIT >>TESTE.BAT
echo :exit >>TESTE.BAT
echo PAUSE >>TESTE.BAT
CALL TESTE.BAT
rem @ECHO Off
cls
color 9f
echo.              @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.              @             CRIADAS AS PASTAS COM SATISFACAO       @
echo.              @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
echo.
echo.
echo.   OPCOES
echo.
echo.
echo.   (V)    VER ARQUIVOS COPIADOS
echo.   (S)    SAIR SEM APAGAR
echo.---------------------------------------------------------
echo.
:aff
set /p opcao=****Digite a opcao--
if %opcao% equ S goto ex
if %opcao% equ s goto ex
if %opcao% equ V goto vb2
if %opcao% equ v goto vb2
:ex
del teste.bat
echo Saindo.................................
exit
:vb2
del teste.bat
explorer %dir%
goto ex