@echo off
title repoBuddy WatchDog
echo [repoBuddy] waiting for RB to close before restarting...

:whileRB
TASKLIST | FINDSTR /I "RebornBuddy.exe" >nul && goto :whileRB

start rebornbuddy.exe -a

exit