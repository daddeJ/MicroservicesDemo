#!/bin/bash

API_URL="http://localhost:5136/api/account/register"
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

# Array of JSON payloads
declare -a USERS=(
'{"UserName":"adminUser4","Email":"admin4@example.com","Password":"AdminPass444!","ConfirmPassword":"AdminPass444!","Role":"Admin","Tier":"0"}'
'{"UserName":"executiveUser4","Email":"executive4@example.com","Password":"ExecutivePass444!","ConfirmPassword":"ExecutivePass444!","Role":"Executive","Tier":"1"}'
'{"UserName":"hrUser4","Email":"hr4@example.com","Password":"hrPassword444!","ConfirmPassword":"hrPassword444!","Role":"HR","Tier":"2"}'
'{"UserName":"managerUser4","Email":"manager4@example.com","Password":"ManagerPass444!","ConfirmPassword":"ManagerPass444!","Role":"Manager","Tier":"3"}'
'{"UserName":"leaderUser4","Email":"leader4@example.com","Password":"LeaderPass444!","ConfirmPassword":"LeaderPass444!","Role":"Leader","Tier":"4"}'
'{"UserName":"regularUser4","Email":"regular4@example.com","Password":"RegularPass444!","ConfirmPassword":"RegularPass444!","Role":"User","Tier":"5"}'
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
