# Allocation Planning API — ASP.NET Core 8 + SQL Server

Original sample API for supply-chain allocation planning.

## Business logic
- Eligible stock per location = On-hand - Reserved - Safety stock - previously allocated stock.
- Allocate across locations in location-code order until the requested quantity is fulfilled or eligible stock is exhausted.
- Request status is `Allocated`, `PartiallyAllocated`, or `Unallocated`.
- Uses a serializable database transaction during allocation to reduce the chance of concurrent requests consuming the same stock.

This is a demo. A production solution should define explicit allocation priority/ordering policies, order eligibility, inventory reservations as atomic stock updates, cancellation/release flows, audit history, and robust concurrency controls.

## Run
1. Open `AllocationPlanningApi.csproj` in Visual Studio 2022.
2. Run `Database/AllocationPlanningDb.sql` in SQL Server Management Studio.
3. Update `ConnectionStrings:DefaultConnection` in `appsettings.json` if using another SQL Server instance.
4. Restore NuGet packages and press F5.
5. Test endpoints in Swagger.

## Endpoints
- `GET /api/allocation/requests`
- `GET /api/allocation/requests?status=Pending`
- `POST /api/allocation/requests`
- `POST /api/allocation/requests/1/allocate`
- `GET /api/allocation/stock`
- `POST /api/allocation/stock`
- `GET /api/allocation/allocations`

### Create request JSON
```json
{
  "requestNumber": "ALC-1002",
  "productCode": "SKU-1001",
  "requestedQuantity": 90,
  "requiredDate": "2026-10-30T00:00:00",
  "priority": "High"
}
```

### Example flow
1. `GET /api/allocation/stock` to review available stock.
2. `GET /api/allocation/requests` to find request IDs.
3. `POST /api/allocation/requests/1/allocate` to run allocation.
4. `GET /api/allocation/allocations` to inspect the result rows.
