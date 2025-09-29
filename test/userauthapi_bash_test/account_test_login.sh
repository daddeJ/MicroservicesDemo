#!/bin/bash

API_URL="http://localhost:5136/api/account"

declare -a USERS=(
'{"Username":"adminUser3","Password":"AdminPass333!"}'
'{"Username":"executiveUser3","Password":"ExecutivePass333!"}'
'{"Username":"hrUser3","Password":"hrPassword333!"}'
'{"Username":"managerUser3","Password":"ManagerPass333!"}'
'{"Username":"leaderUser3","Password":"LeaderPass333!"}'
'{"Username":"regularUser3","Password":"RegularPass333!"}'
)

i=1
for creds in "${USERS[@]}"; do
  echo "=== Test $i: Login ==="
  echo "Credentials: $creds"

  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/login" \
    -H "Content-Type: application/json" \
    -d "$creds")

  BODY=$(echo "$RESPONSE" | head -n 1)
  STATUS=$(echo "$RESPONSE" | tail -n 1)

  if [[ "$STATUS" == "200" ]]; then
    echo "✅ Login success (HTTP $STATUS)"
    TOKEN=$(echo "$BODY" | jq -r '.token')

    echo "Token: $TOKEN"

    echo "=== Test $i: Me Endpoint ==="
    ME_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_URL/me" \
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
  else
    echo "❌ Login failed (HTTP $STATUS)"
    echo "Response: $BODY"
  fi

  echo
  ((i++))
done