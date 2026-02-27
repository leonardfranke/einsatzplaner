set SCRIPT_DIR=%~dp0
set SUBFOLDER=seeds
if not exist "%SCRIPT_DIR%%SUBFOLDER%" mkdir "%SCRIPT_DIR%%SUBFOLDER%"
npx supabase db dump -f "%SCRIPT_DIR%%SUBFOLDER%\data.sql" --data-only