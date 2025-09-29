#!/bin/bash

API_URL="http://localhost:5136/api"
ACCOUNT_URL="$API_URL/account"
REPORT_URL="$API_URL/report"
HEALTH_URL="http://localhost:5136/api/health"

echo "=== Checking API Health ==="
HEALTH_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$HEALTH_URL")
HEALTH_BODY=$(echo "$HEALTH_RESPONSE" | head -n 1)
HEALTH_STATUS=$(echo "$HEALTH_RESPONSE" | tail -n 1)

if [[ "$HEALTH_STATUS" != "200" ]]; then
  echo "❌ API health check failed (HTTP $HEALTH_STATUS)"
  echo "Response: $HEALTH_BODY"
  exit 1
else
  echo "✅ API health check passed (HTTP $HEALTH_STATUS)"
fi
echo

declare -a USERS=(
'{"Username":"adminUser4","Password":"AdminPass444!"}'
'{"Username":"executiveUser4","Password":"ExecutivePass444!"}'
'{"Username":"hrUser4","Password":"hrPassword444!"}'
'{"Username":"managerUser4","Password":"ManagerPass444!"}'
'{"Username":"leaderUser4","Password":"LeaderPass444!"}'
'{"Username":"regularUser4","Password":"RegularPass444!"}'
)

# Endpoints to check for each user
declare -a REPORT_ENDPOINTS=("admin" "executives" "manager" "leader" "general")

i=1
for creds in "${USERS[@]}"; do
  echo "=== Test $i: Login ==="
  echo "Credentials: $creds"

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$ACCOUNT_URL/login" \
    -H "Content-Type: application/json" \
    -d "$creds")

  BODY=$(echo "$RESPONSE" | head -n 1)
  STATUS=$(echo "$RESPONSE" | tail -n 1)

  if [[ "$STATUS" == "200" ]]; then
    echo "✅ Login success (HTTP $STATUS)"
    TOKEN=$(echo "$BODY" | jq -r '.token')

    echo "Token: $TOKEN"

    echo "=== Test $i: Me Endpoint ==="
    ME_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$ACCOUNT_URL/me" \
      -H "Authorization: Bearer $TOKEN")

    ME_BODY=$(echo "$ME_RESPONSE" | head -n 1)
    ME_STATUS=$(echo "$ME_RESPONSE" | tail -n 1)

    if [[ "$ME_STATUS" == "200" ]]; then
      echo "✅ Me success (HTTP $ME_STATUS)"
      echo "Response: $ME_BODY"
    else
      echo "❌ Me failed (HTTP $ME_STATUS)"
      echo "Response: $ME_BODY"
    fi

    echo "=== Test $i: Report Endpoints ==="
    for endpoint in "${REPORT_ENDPOINTS[@]}"; do
      REPORT_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$REPORT_URL/$endpoint" \
        -H "Authorization: Bearer $TOKEN")

      REPORT_BODY=$(echo "$REPORT_RESPONSE" | head -n 1)
      REPORT_STATUS=$(echo "$REPORT_RESPONSE" | tail -n 1)

      if [[ "$REPORT_STATUS" == "200" ]]; then
        echo "✅ Access to /report/$endpoint granted"
        echo "Response: $REPORT_BODY"
      else
        echo "❌ Access to /report/$endpoint denied (HTTP $REPORT_STATUS)"
      fi
    done
  else
    echo "❌ Login failed (HTTP $STATUS)"
    echo "Response: $BODY"
  fi

  echo
  ((i++))
done