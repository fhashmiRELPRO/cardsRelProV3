@echo off
setlocal enabledelayedexpansion

echo ============================================
echo  RelPro .NET 8 Demo  -  Smoke Tests
echo ============================================
echo.
echo Tests both the HAPPY PATH and common ERROR SCENARIOS.
echo Each numbered step tests a different status code or validation case.
echo.

:: Hosts
set AUTH_HOST=http://localhost:5010
set USER_HOST=http://localhost:5020
set SEARCH_HOST=http://localhost:5030

:: Params - override via environment if needed
if "%SEARCH_QUERY%"==""   set SEARCH_QUERY=relpro
if "%TEST_USER_ID%"==""   set TEST_USER_ID=0

set PASS_COUNT=0
set FAIL_COUNT=0

:: -------------------------------------------------------
:: TOKEN ACQUISITION
:: Option A: TEST_TOKEN is already set (real CARDS legacy session token).
::           Copy from staging.relpro.com: Dev Tools -> Application -> Cookies -> userToken
:: Option B: TEST_EMAIL + TEST_PASSWORD -> login via Auth.Service (dev only)
:: -------------------------------------------------------

if not "%TEST_TOKEN%"=="" (
    set TOKEN=%TEST_TOKEN%
    echo [INFO] Using pre-set CARDS legacy token: !TOKEN:~0,20!...
    goto :happy_path
)

if "%TEST_EMAIL%"=="" (
    echo [ERROR] Set TEST_TOKEN ^(legacy CARDS token^) or TEST_EMAIL + TEST_PASSWORD before running.
    echo.
    echo   Using a CARDS token ^(recommended for staging^):
    echo     set TEST_TOKEN=your-cards-session-token ^& run-demo-curl.bat
    echo.
    echo   Using new login ^(dev only^):
    echo     set TEST_EMAIL=you@bank.com ^& set TEST_PASSWORD=secret ^& run-demo-curl.bat
    exit /b 1
)
if "%TEST_PASSWORD%"=="" (
    echo [ERROR] TEST_PASSWORD not set.
    exit /b 1
)

echo.
echo === STEP 1/10: POST /v1/auth/login ^(200 expected^)
set LOGIN_BODY={\"email\":\"%TEST_EMAIL%\",\"password\":\"%TEST_PASSWORD%\"}

for /f "delims=" %%T in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' -X POST '%AUTH_HOST%/v1/auth/login' -H 'Content-Type: application/json' -d '%LOGIN_BODY%';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '200' -and $body.success) { $body.data.token } else { 'FAILED' }"') do set TOKEN=%%T

if "!TOKEN!"=="FAILED" (
    echo   [FAIL] Login failed
    set /a FAIL_COUNT+=1
    goto :done
)
echo   [PASS] Login OK - token: !TOKEN:~0,20!...
set /a PASS_COUNT+=1

:happy_path
:: ═══════════════════════════════════════════════════════
echo.
echo ─────── HAPPY PATH ────────────────────────────────
:: ═══════════════════════════════════════════════════════

echo.
echo === STEP 2/10: GET /v1/user/me ^(200 expected^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%USER_HOST%/v1/user/me' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '200' -and $body.success) { 'OK:' + $body.data.email + ' (org=' + $body.data.orgId + ')' } else { 'FAILED:' + $status + ':' + $body.errorCode }"') do set ME_RESULT=%%R

if "!ME_RESULT:~0,2!"=="OK" (
    echo   [PASS] !ME_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !ME_RESULT!
    set /a FAIL_COUNT+=1
)

echo.
echo === STEP 3/10: GET /v1/user/%TEST_USER_ID% ^(200 expected, requires UserManagement entitlement^)

if "%TEST_USER_ID%"=="0" (
    echo   [SKIP] TEST_USER_ID not set - set TEST_USER_ID to a valid user ID to run this step
    goto :step4
)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%USER_HOST%/v1/user/%TEST_USER_ID%' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '200' -and $body.success) { 'OK:' + $body.data.name + ' / ' + $body.data.email } else { 'FAILED:' + $status + ':' + $body.errorCode }"') do set GETUSER_RESULT=%%R

if "!GETUSER_RESULT:~0,2!"=="OK" (
    echo   [PASS] !GETUSER_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !GETUSER_RESULT!
    set /a FAIL_COUNT+=1
)

:step4
echo.
echo === STEP 4/10: GET /v1/search?query=%SEARCH_QUERY% ^(200 expected - CARDS-compatible prospector^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%SEARCH_HOST%/v1/search?query=%SEARCH_QUERY%&pageSize=10&page=1' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '200' -and ($body.totalIndividuals -ne $null)) { 'OK:' + $body.totalIndividuals + ' total individuals, ' + $body.individuals.Count + ' returned' } else { 'FAILED:' + $status }"') do set PROSPECT_RESULT=%%R

if "!PROSPECT_RESULT:~0,2!"=="OK" (
    echo   [PASS] Prospector search: !PROSPECT_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !PROSPECT_RESULT!
    set /a FAIL_COUNT+=1
)

echo.
echo === STEP 5/10: GET /v1/search/people?q=%SEARCH_QUERY% ^(200 expected - MySQL full-text search^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%SEARCH_HOST%/v1/search/people?q=%SEARCH_QUERY%&limit=5' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '200' -and $body.success) { 'OK:' + $body.data.Count + ' results' } else { 'FAILED:' + $status + ':' + $body.errorCode }"') do set SEARCH_RESULT=%%R

if "!SEARCH_RESULT:~0,2!"=="OK" (
    echo   [PASS] !SEARCH_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !SEARCH_RESULT!
    set /a FAIL_COUNT+=1
)

:: ═══════════════════════════════════════════════════════
echo.
echo ─────── ERROR SCENARIOS ───────────────────────────
:: ═══════════════════════════════════════════════════════

echo.
echo === STEP 6/10: No token ^(401 expected - MISSING_TOKEN^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%USER_HOST%/v1/user/me';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '401' -and $body.errorCode -eq 'MISSING_TOKEN') { 'OK:401 MISSING_TOKEN as expected' } else { 'FAILED:expected 401/MISSING_TOKEN got ' + $status + '/' + $body.errorCode }"') do set UNAUTH_RESULT=%%R

if "!UNAUTH_RESULT:~0,2!"=="OK" (
    echo   [PASS] !UNAUTH_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !UNAUTH_RESULT!
    set /a FAIL_COUNT+=1
)

echo.
echo === STEP 7/10: Invalid token ^(401 expected - INVALID_SESSION^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%USER_HOST%/v1/user/me' -H 'userToken: definitely-not-a-real-token-xyz';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '401' -and $body.errorCode -eq 'INVALID_SESSION') { 'OK:401 INVALID_SESSION as expected' } else { 'FAILED:expected 401/INVALID_SESSION got ' + $status + '/' + $body.errorCode }"') do set BADTOKEN_RESULT=%%R

if "!BADTOKEN_RESULT:~0,2!"=="OK" (
    echo   [PASS] !BADTOKEN_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !BADTOKEN_RESULT!
    set /a FAIL_COUNT+=1
)

echo.
echo === STEP 8/10: Non-integer user ID ^(400 or 404 expected - invalid route parameter^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%USER_HOST%/v1/user/not-a-number' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "if ($status -eq '400' -or $status -eq '404') { 'OK:' + $status + ' as expected for non-integer id' } else { 'FAILED:expected 400 or 404 got ' + $status }"') do set BADID_RESULT=%%R

if "!BADID_RESULT:~0,2!"=="OK" (
    echo   [PASS] !BADID_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !BADID_RESULT!
    set /a FAIL_COUNT+=1
)

echo.
echo === STEP 9/10: GET /v1/user/-1 ^(400 expected - INVALID_ID^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%USER_HOST%/v1/user/-1' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '400' -and $body.errorCode -eq 'INVALID_ID') { 'OK:400 INVALID_ID as expected' } else { 'FAILED:expected 400/INVALID_ID got ' + $status + '/' + $body.errorCode }"') do set NEGID_RESULT=%%R

if "!NEGID_RESULT:~0,2!"=="OK" (
    echo   [PASS] !NEGID_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !NEGID_RESULT!
    set /a FAIL_COUNT+=1
)

echo.
echo === STEP 10/10: GET /v1/search/people with no q param ^(400 expected - MISSING_QUERY^)

for /f "delims=" %%R in ('powershell -NoProfile -Command ^
    "$r = curl.exe -s -w '||STATUS=%%{http_code}' '%SEARCH_HOST%/v1/search/people' -H 'userToken: !TOKEN!';" ^
    "$parts = $r -split '\|\|STATUS=';" ^
    "$status = $parts[1];" ^
    "$body = $parts[0] | ConvertFrom-Json;" ^
    "if ($status -eq '400' -and $body.errorCode -eq 'MISSING_QUERY') { 'OK:400 MISSING_QUERY as expected' } else { 'FAILED:expected 400/MISSING_QUERY got ' + $status + '/' + $body.errorCode }"') do set NOQUERY_RESULT=%%R

if "!NOQUERY_RESULT:~0,2!"=="OK" (
    echo   [PASS] !NOQUERY_RESULT:~3!
    set /a PASS_COUNT+=1
) else (
    echo   [FAIL] !NOQUERY_RESULT!
    set /a FAIL_COUNT+=1
)

:done
echo.
echo ============================================
echo  Results: !PASS_COUNT! passed, !FAIL_COUNT! failed
echo ============================================
if !FAIL_COUNT! GTR 0 (
    echo  [ATTENTION] Check failed steps above.
    exit /b 1
)
echo  All checks passed. System is healthy.
exit /b 0
