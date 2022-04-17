for /d /r ..\.. %%d in (.svn) do @if exist "%%d" rd /s/q "%%d"
