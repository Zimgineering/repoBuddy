@echo off
set RB_PID=%1
set FFIV_PID=%2

title [repoBuddy] WatchDog
echo [repoBuddy] waiting for RB (PID: %1, attached to FFXIV PID: %2) to close before restarting...

:whileRB
TASKLIST /FI "PID eq %1" | FINDSTR /I "RebornBuddy.exe" >nul && goto :whileRB

start rebornbuddy.exe --processid=%2 -a

exit