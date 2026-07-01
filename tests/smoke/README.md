# Smoke Tests (Hurl)

End-to-end HTTP tests that run against a real running service.

## Install Hurl

```bash
# Ubuntu/Debian
curl -LO https://github.com/Orange-OpenSource/hurl/releases/latest/download/hurl_amd64.deb
sudo dpkg -i hurl_amd64.deb

# Verify
hurl --version
```

## Run

```bash
# Against local dev
bash tests/smoke/run-all.sh dev

# Against staging
bash tests/smoke/run-all.sh staging

# Single file with verbose output
hurl --variables-file tests/smoke/dev.env --verbose tests/smoke/auth/login.hurl
```

## Adding a new smoke test

1. Create a `.hurl` file in the folder matching the service domain (`auth/`, `search/`, `user/`, etc.)
2. Every new endpoint needs minimum three test files:
   - `{endpoint}-happy-path.hurl` - valid request, assert 200 + response shape
   - `{endpoint}-no-token.hurl` - no userToken/Bearer, assert 401
   - `{endpoint}-no-entitlement.hurl` - valid token, missing feature, assert 403
3. Use `[Captures]` to chain requests (login → token → call endpoint)
4. See `auth/login.hurl` as a reference template

## File naming

`{domain}/{action}.hurl`

Examples:
- `auth/login.hurl`
- `search/search-people.hurl`
- `search/search-people-no-token.hurl`
- `search/export-no-entitlement.hurl`
