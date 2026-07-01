@echo off
setlocal enabledelayedexpansion

echo ============================================
echo  RelPro MVP Demo - Cascading Endpoint Tests
echo  Using: Hurl (https://hurl.dev)
echo ============================================

:: Check if Hurl is installed; install via winget if not
where hurl >nul 2>&1
if errorlevel 1 (
    echo [INFO] Hurl not found. Installing via winget...
    winget install Orange-OpenSource.Hurl
    if errorlevel 1 (
        echo [ERROR] winget install failed. Install Hurl manually from https://hurl.dev
        exit /b 1
    )
)

:: Set credentials - override here or pass as env vars
if "%TEST_EMAIL%"=="" set TEST_EMAIL=SET_YOUR_EMAIL_HERE
if "%TEST_PASSWORD%"=="" set TEST_PASSWORD=SET_YOUR_PASSWORD_HERE
if "%SEARCH_QUERY%"=="" set SEARCH_QUERY=Elon

set DEMO_DIR=%~dp0
set ENV_FILE=%DEMO_DIR%demo.env

echo.
echo [Step 1/3] POST /v1/auth/login
echo [Step 2/3] GET  /v1/user/me       (token from step 1)
echo [Step 3/3] GET  /v1/search/people (token from step 1)
echo.

:: Run all three hurl files in cascade
:: --variables-file injects env vars; captured values flow between files
hurl ^
  --variables-file "%ENV_FILE%" ^
  --variable test_email=%TEST_EMAIL% ^
  --variable test_password=%TEST_PASSWORD% ^
  --variable search_query=%SEARCH_QUERY% ^
  --test ^
  --color ^
  "%DEMO_DIR%01-login.hurl" ^
  "%DEMO_DIR%02-user-me.hurl" ^
  "%DEMO_DIR%03-search-people.hurl"

if errorlevel 1 (
    echo.
    echo [FAIL] One or more tests failed.
    exit /b 1
)

echo.
echo [PASS] All demo tests passed.
exit /b 0
