@echo off
setlocal

set DESTINATION=%~dp0Export/

set AUTH_BACKUP=%DESTINATION%auth/
if not exist "%AUTH_BACKUP%" mkdir "%AUTH_BACKUP%"
call firebase auth:export "%AUTH_BACKUP%accounts.json" --project einsatzplaner

set FIRESTORE_BACKUP=%DESTINATION%firestore/
if not exist "%FIRESTORE_BACKUP%" mkdir "%FIRESTORE_BACKUP%"
set BUCKET_PATH=gs://firestore-export-einsatzplaner/Export/*
call gsutil -m cp -r "%BUCKET_PATH%" "%FIRESTORE_BACKUP%"

(
echo {
echo    "version": "14.2.2",
echo    "firestore": {
echo        "version": "1.19.8",
echo        "path": "firestore",
echo        "metadata_file": "firestore/Export.overall_export_metadata"
echo    },
echo    "auth": {
echo        "version": "14.2.2",
echo        "path": "auth"
echo    }
echo }
) > %DESTINATION%firebase-export-metadata.json