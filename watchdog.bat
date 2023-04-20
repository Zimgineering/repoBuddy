@echo off
set RB_PID=%1
set FFXIV_PID=%2

title [repoBuddy] WatchDog
echo [repoBuddy] waiting for RB (PID: %RB_PID%, attached to FFXIV PID: %FFXIV_PID%) to close before restarting...

:whileRB
timeout /t 5 /nobreak
taskkill /PID %RB_PID% /F /FI "status eq not responding" >nul
TASKLIST /FI "PID eq %RB_PID%" | FINDSTR %RB_PID% >nul && goto :whileRB

start rebornbuddy.exe --processid=%FFXIV_PID% -a

exit
