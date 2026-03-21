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
    "CatalogConnection": "Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password"
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
dotnet ef migrations add InitialCatalog -c CatalogDbContext -o Persistence/Migrations/Catalog -p SaaS.Infrastructure -s SaaS.API
dotnet ef database update -c CatalogDbContext -p SaaS.Infrastructure -s SaaS.API
```

**B. Migration cho Tenant (Lưu Dữ liệu Schema của mỗi khách hàng):**
```bash
dotnet ef migrations add InitialTenant -c TenantDbContext -o Persistence/Migrations/Tenant -p SaaS.Infrastructure -s SaaS.API
```
> **Lưu ý**: Bạn không cần chạy `database update` cho Tenant ở bước này. Việc tạo ra DB và apply schema sẽ được thực thi khi bạn tạo mới một `Tenant` thông qua API hoặc script riêng.

---

## 4. Cách API phân luồng Tenant

1. Khách hàng B tạo yêu cầu tới API: `GET /api/tenant/current`.
2. Khách hàng B gửi kèm HTTP Header: `X-Tenant-Id: tenant_b`.
3. Middleware `TenantResolutionMiddleware.cs` đọc Header, tra cứu vào `CatalogDbContext` cấu hình chuỗi Connection String của `tenant_b`.
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
cd SaaS.API
dotnet run
```
Mở đường dẫn `https://localhost:XXXX/swagger` trên trình duyệt để coi danh sách API.

**Cách test Tenant trên Swagger/Postman**:
- Chèn parameter Key là `X-Tenant-Id` vào Header, Value ví dụ: `khachhang_01`.
- Chèn parameter Key là `Authorization` vào Header, Value `Bearer eyJ...` (nếu route đó yêu cầu đăng nhập).
