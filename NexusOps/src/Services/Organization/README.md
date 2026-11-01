# Organization Service

The Organization service owns department, team, and employee-profile data. It stores `IdentityUserId` as an external reference to the Identity service; it deliberately does not duplicate passwords or Identity tables.

## Local configuration

`Organization.Api/appsettings.Development.json` contains the local PostgreSQL development connection string. Replace the development password for your environment and use environment variables/user-secrets outside local development.

## HTTP endpoints

### Departments

- `GET /api/departments?includeInactive=false`
- `GET /api/departments/{id}`
- `POST /api/departments` — `{ "name": "Engineering", "description": "..." }`
- `PUT /api/departments/{id}` — `{ "name": "Engineering", "description": "..." }`
- `PATCH /api/departments/{id}/deactivate`

### Teams

- `GET /api/teams?departmentId={guid}&includeInactive=false`
- `GET /api/teams/{id}`
- `POST /api/teams` — `{ "name": "Platform", "description": "...", "departmentId": "{guid}" }`
- `PUT /api/teams/{id}` — `{ "name": "Platform", "description": "...", "departmentId": "{guid}", "isActive": true }`
- `PUT /api/teams/{id}/team-lead` — `{ "employeeId": "{guid}" }` (use `null` to clear)

### Employees

- `GET /api/employees?departmentId={guid}&teamId={guid}&status=Active`
- `GET /api/employees/{id}`
- `GET /api/employees/by-identity/{identityUserId}`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `PATCH /api/employees/{id}/status` — `{ "status": "OnLeave" }`

Employee create payload fields: `identityUserId`, `employeeNumber`, `firstName`, `lastName`, `jobTitle`, `phoneNumber`, `workEmail`, `departmentId`, `teamId`, `managerId`, and `dateJoined`. The status defaults to `Active`.

## Business rules currently enforced

- Identity user IDs and employee numbers are unique.
- Work email is unique when supplied.
- Employees require an active department; an assigned team must be active and belong to that department.
- Reporting managers must exist, be in the same department, and cannot create a self-reference or management cycle.
- Team leads must belong to the team's department.
- A team with assigned employees cannot be moved to another department until those employees are reassigned.
- Department deactivation is soft deactivation; existing employee/team records are retained.

## Run

From the `NexusOps` solution directory:

```powershell
dotnet run --project src/Services/Organization/Organization.Api
```

The development launch profile uses `http://localhost:5030`. Health check: `GET /health`. OpenAPI document is available in Development at `/openapi/v1.json`.
