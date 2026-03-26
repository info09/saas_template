# Hướng Dẫn Sử Dụng Template SaaS .NET 8 (Clean Architecture)

Tài liệu này sẽ hướng dẫn bạn cách khởi chạy, cấu hình và phát triển thêm các tính năng mới trên nền tảng Clean Architecture với mô hình Multi-tenant (Mỗi khách hàng một Database) này.

---

## 1. Yêu cầu hệ thống (Prerequisites)
- **.NET 8 SDK** đã được cài đặt.
- **PostgreSQL Server** đang chạy (có thể dùng Docker hoặc cài trực tiếp).
- **IDE**: Visual Studio 2022, Rider, hoặc VS Code.

---

## 2. Cấu hình ban đầu (Configuration)

Mở file `SaaS.API/appsettings.json` và cập nhật chuỗi kết nối cục bộ của PostgreSQL cho dữ liệu Master (Catalog):

```json
{
  "ConnectionStrings": {
    "MasterConnection": "Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "chuoi_ky_tu_bi_mat_rat_dai_va_an_toan_1234567890",
    "Issuer": "http://localhost:5000",
    "Audience": "http://localhost:5000"
  }
}
```

---

## 3. Chạy Migration cho Database (EF Core)

Do sử dụng cấu trúc Database-per-tenant, hệ thống có hai `DbContext`. Bạn cần chạy lệnh để tạo các bảng tương ứng cho PostgreSQL.

Mở terminal ở thư mục gốc `SaaS.Template` và chạy:

**A. Migration cho Master Catalog (Lưu danh sách khách hàng):**
```bash
dotnet ef migrations add InitialCatalog -c MasterDbContext -o Migrations -p SaaS.Infrastructure -s SaaS.API
```

**B. Migration cho Tenant (Lưu Dữ liệu Schema của mỗi khách hàng):**
```bash
dotnet ef migrations add InitialTenant -c TenantDbContext -o Persistence/Migrations/Tenant -p SaaS.Infrastructure -s SaaS.API
```
Sau khi đã có migration, apply schema bằng các lệnh explicit sau:
```bash
dotnet run --project SaaS.API -- --migrate-master
dotnet run --project SaaS.API -- --migrate-tenants
dotnet run --project SaaS.API -- --migrate-all
```
> **Lưu ý**: API không còn tự động chạy migration lúc startup. Điều này tách deployment schema ra khỏi vòng đời boot của ứng dụng.

---

## 4. Cách API phân luồng Tenant

1. Khách hàng B tạo yêu cầu tới API: `GET /api/tenant/current`.
2. Khách hàng B gửi kèm HTTP Header: `X-Tenant-Id: tenant_b`.
3. Middleware `TenantResolutionMiddleware.cs` đọc Header, tra cứu vào `MasterDbContext` để tìm chuỗi Connection String của `tenant_b`.
4. Mọi query thông qua `TenantDbContext` trong request đó sẽ đi vào CSDL của `tenant_b`.

---

## 5. Quy trình thêm một tính năng mới (Ví dụ: Module Sản Phẩm)

Để giữ đúng chuẩn Clean Architecture bạn cần tuân theo thứ tự luồng phụ thuộc sau (Luôn bắt đầu từ Core ra ngoài):

### Bước 1: Domain Layer (`SaaS.Domain`)
Tạo Entity `Product` (Các class thuần túy mang tính chất cốt lõi):
```csharp
namespace SaaS.Domain.Entities;
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Bước 2: Application Layer (`SaaS.Application`)
- Tạo các class DTOs, Commands, Queries (Sử dụng CQRS / MediatR).
- Tạo interface Repository `IProductRepository` (Nếu dùng pattern Repository).
- Viết các Validation (Sử dụng FluentValidation) cho đầu vào (Ví dụ: Giá phải > 0).

### Bước 3: Infrastructure Layer (`SaaS.Infrastructure`)
- Khai báo thêm `DbSet<Product>` vào trong `TenantDbContext.cs`.
- Chạy lệnh `dotnet ef migrations add AddProduct -c TenantDbContext ...`.
- Triển khai logic ghi/đọc Database dựa theo `IProductRepository`. 

### Bước 4: API Presentation (`SaaS.API`)
Tạo `ProductController.cs`:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase {
    private readonly IMediator _mediator;
    
    public ProductController(IMediator mediator) { _mediator = mediator; }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command) {
        // Gửi Command vào Application Layer
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

---

## 6. Gọi lệnh chạy thử API
```bash
dotnet run --project SaaS.API
```
Mở đường dẫn `https://localhost:XXXX/swagger` trên trình duyệt để coi danh sách API.

**Cách test Tenant trên Swagger/Postman**:
- Chèn parameter Key là `X-Tenant-Id` vào Header, Value ví dụ: `khachhang_01`.
- Chèn parameter Key là `Authorization` vào Header, Value `Bearer eyJ...` (nếu route đó yêu cầu đăng nhập).

**Health checks**:
```http
GET /health
GET /health/ready
```
Hai endpoint này dùng để kiểm tra trạng thái PostgreSQL và Redis.

**Cách gọi login hiện tại**:
```http
POST /api/auth/login
X-Tenant-Id: khachhang_01
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "123456"
}
```

**Cách gọi refresh token**:
```http
POST /api/auth/refresh
X-Tenant-Id: khachhang_01
Content-Type: application/json

{
  "refreshToken": "..."
}
```

**Cách gọi logout**:
```http
POST /api/auth/logout
X-Tenant-Id: khachhang_01
Content-Type: application/json

{
  "refreshToken": "..."
}
```

**Quy ước response**:
- Thành công: trả `Result<T>`.
- Lỗi validation: trả `ValidationProblemDetails`.
- Lỗi nghiệp vụ, auth, tenant: trả `ProblemDetails`.

**Lưu ý về bảo mật session**:
- Access token mang claim `sessionId`.
- Redis lưu session state của từng phiên đăng nhập theo tenant.
- Khi logout, logout-session, hoặc logout-all, session tương ứng sẽ bị revoke trong DB và cache, nên access token cũ sẽ bị từ chối ở request tiếp theo.
- Nếu Redis miss hoặc Redis lỗi, hệ thống fallback sang DB để kiểm tra session state.
- Nếu cả Redis và DB đều không cung cấp được auth state, API sẽ trả `503 Service Unavailable` theo cấu hình mặc định.
