set /p target=dll full path
.\NStrip.exe -p -n -o -cg -cg-exclude-events -remove-readonly "%target%"
pause
exit