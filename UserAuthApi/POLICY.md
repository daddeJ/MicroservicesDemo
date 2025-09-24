# API Access Control Policies

## Role & Tier Definitions

### 1. SuperAdminOnly
```
Role: Admin
Claims: Tier 0
```

**Permissions:**
- Has access to all API endpoints
- Can update all tiers and roles
- Can delete users of all tiers and roles
- **Constraint:** Update user detail only allows changes to tier and roles

**API Endpoints:**

**Read all users (any tier/role)**
```
# HTTP Request
GET <IP>/api/admin/users
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/admin/users" \
  -H "Authorization: Bearer <token>"
```

**Read single user (any tier/role)**
```
# HTTP Request
GET <IP>/api/admin/users/{id}
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/admin/users/{id}" \
  -H "Authorization: Bearer <token>"
```

**Update user tier/role (any tier/role)**
```
# HTTP Request
PATCH <IP>/api/admin/users/{id}
Content-Type: application/json
Authorization: Bearer <token>

{
  "role": "Role1",
  "tier": "1"
}

# cURL Command
curl -X PATCH "<IP>/api/admin/users/{id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "role": "Role1",
    "tier": "1"
  }'
```

**Delete user (any tier/role)**
```
# HTTP Request
DELETE <IP>/api/admin/users/{id}
Authorization: Bearer <token>

# cURL Command
curl -X DELETE "<IP>/api/admin/users/{id}" \
  -H "Authorization: Bearer <token>"
```

---

### 2. ExecutivesOnly
```
Role: Executive
Claims: Tier 1, 0
```

**Permissions:**
- Can update users with tier 2-5 and roles: HR, Manager, Leader, Regular
- Can delete users with tier 2-5 and roles: HR, Manager, Leader, Regular
- Has read access to users with tier 2-5 and roles: HR, Manager, Leader, Regular
- **Constraint:** Update user detail only allows changes to tier and roles

**API Endpoints:**

**Read users list (tier 2-5, roles: HR, Manager, Leader, Regular)**
```
# HTTP Request
GET <IP>/api/executive/users?tier=2,3,4,5&role=HR,Manager,Leader,Regular
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/executive/users?tier=2,3,4,5&role=HR,Manager,Leader,Regular" \
  -H "Authorization: Bearer <token>"
```

**Read single user (tier 2-5, roles: HR, Manager, Leader, Regular)**
```
# HTTP Request
GET <IP>/api/executive/users/{id}
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/executive/users/{id}" \
  -H "Authorization: Bearer <token>"
```

**Update user tier/role (tier 2-5, roles: HR, Manager, Leader, Regular)**
```
# HTTP Request
PATCH <IP>/api/executive/users/{id}
Content-Type: application/json
Authorization: Bearer <token>

{
  "role": "Manager",
  "tier": "3"
}

# cURL Command
curl -X PATCH "<IP>/api/executive/users/{id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "role": "Manager",
    "tier": "3"
  }'
```

**Delete user (tier 2-5, roles: HR, Manager, Leader, Regular)**
```
# HTTP Request
DELETE <IP>/api/executive/users/{id}
Authorization: Bearer <token>

# cURL Command
curl -X DELETE "<IP>/api/executive/users/{id}" \
  -H "Authorization: Bearer <token>"
```

---

### 3. ManagerAndAbove
```
Role: Admin, Executive, HR, Manager, Leader
Claims: Tier 0, 1, 2, 3, 4
```

**Permissions:**
- Has read access to users with tier 4-5 and roles: Leader, Regular
- Can update users with tier 4-5 and roles: Leader, Regular
- Can delete users with tier 4-5 and roles: Leader, Regular

**API Endpoints:**

**Read users list (tier 4-5, roles: Leader, Regular)**
```
# HTTP Request
GET <IP>/api/manager/users?tier=4,5&role=Leader,Regular
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/manager/users?tier=4,5&role=Leader,Regular" \
  -H "Authorization: Bearer <token>"
```

**Read single user (tier 4-5, roles: Leader, Regular)**
```
# HTTP Request
GET <IP>/api/manager/users/{id}
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/manager/users/{id}" \
  -H "Authorization: Bearer <token>"
```

**Update user tier/role (tier 4-5, roles: Leader, Regular)**
```
# HTTP Request
PATCH <IP>/api/manager/users/{id}
Content-Type: application/json
Authorization: Bearer <token>

{
  "role": "Regular",
  "tier": "5"
}

# cURL Command
curl -X PATCH "<IP>/api/manager/users/{id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "role": "Regular",
    "tier": "5"
  }'
```

**Delete user (tier 4-5, roles: Leader, Regular)**
```
# HTTP Request
DELETE <IP>/api/manager/users/{id}
Authorization: Bearer <token>

# cURL Command
curl -X DELETE "<IP>/api/manager/users/{id}" \
  -H "Authorization: Bearer <token>"
```

---

### 4. RegularAndAbove
```
Role: Admin, Executive, HR, Manager, Leader, Regular
Claims: Tier 0, 1, 2, 3, 4, 5
```

**Permissions:**
- Has access to own user details
- Can update own user details
- **Constraint:** Cannot update own tier or role

**API Endpoints:**

**Read own user details**
```
# HTTP Request
GET <IP>/api/user/profile
Authorization: Bearer <token>

# cURL Command
curl -X GET "<IP>/api/user/profile" \
  -H "Authorization: Bearer <token>"
```

**Update own user details**
```
# HTTP Request
PATCH <IP>/api/user/profile
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "Updated Name",
  "email": "new@email.com",
  "phone": "+1234567890"
}

# cURL Command
curl -X PATCH "<IP>/api/user/profile" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Updated Name",
    "email": "new@email.com",
    "phone": "+1234567890"
  }'
```

---

## Access Control Summary

| Role Level | Roles | Tiers | Can Manage | Read Access | Update/Delete |
|------------|-------|-------|------------|-------------|---------------|
| **SuperAdminOnly** | Admin | 0 | All users | All users | All users |
| **ExecutivesOnly** | Executive | 1, 0 | Tier 2-5 (HR, Manager, Leader, Regular) | Tier 2-5 users | Tier 2-5 users |
| **ManagerAndAbove** | Admin, Executive, HR, Manager, Leader | 0, 1, 2, 3, 4 | Tier 4-5 (Leader, Regular) | Tier 4-5 users | Tier 4-5 users |
| **RegularAndAbove** | All roles | 0, 1, 2, 3, 4, 5 | Own profile only | Own profile only | Own profile only |

---

## API Design Principles

1. **Hierarchical Path Structure**: Different access levels use different base paths (`/admin/`, `/executive/`, `/manager/`, `/user/`)
2. **Consistent Authentication**: All endpoints require Bearer token authentication
3. **Query Parameters**: Used for filtering by tier and role where applicable
4. **Constraint Enforcement**: Administrative roles can only update tier/role fields, regular users cannot update their own tier/role
5. **Self-Service Endpoints**: Regular users access their own data through `/profile` endpoints