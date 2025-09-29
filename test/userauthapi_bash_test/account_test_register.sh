#!/bin/bash

API_URL="http://localhost:5136/api/account/register"

# Array of JSON payloads
declare -a USERS=(
'{"UserName":"adminUser3","Email":"admin3@example.com","Password":"AdminPass333!","ConfirmPassword":"AdminPass333!","Role":"Admin","Tier":"0"}'
'{"UserName":"executiveUser3","Email":"executive3@example.com","Password":"ExecutivePass333!","ConfirmPassword":"ExecutivePass333!","Role":"Executive","Tier":"1"}'
'{"UserName":"hrUser3","Email":"hr3@example.com","Password":"hrPassword333!","ConfirmPassword":"hrPassword333!","Role":"HR","Tier":"2"}'
'{"UserName":"managerUser3","Email":"manager3@example.com","Password":"ManagerPass333!","ConfirmPassword":"ManagerPass333!","Role":"Manager","Tier":"3"}'
'{"UserName":"leaderUser3","Email":"leader3@example.com","Password":"LeaderPass333!","ConfirmPassword":"LeaderPass333!","Role":"Leader","Tier":"4"}'
'{"UserName":"regularUser3","Email":"regular3@example.com","Password":"RegularPass333!","ConfirmPassword":"RegularPass333!","Role":"User","Tier":"5"}'
)

i=1
for user in "${USERS[@]}"; do
  echo "=== Test $i: Registering user ==="
  echo "$user"

  # Send POST request and capture HTTP status
  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL" \
    -H "Content-Type: application/json" \
    -d "$user")

  BODY=$(echo "$RESPONSE" | head -n 1)
  STATUS=$(echo "$RESPONSE" | tail -n 1)

  if [[ "$STATUS" == "200" || "$STATUS" == "201" ]]; then
    echo "✅ Success (HTTP $STATUS)"
  else
    echo "❌ Failed (HTTP $STATUS)"
  fi

  echo "Response: $BODY"
  echo
  ((i++))
done
