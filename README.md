# SD Cafe - Restaurant Management System

Modern bir restoran yönetim sistemi. .NET 9.0, Entity Framework Core ve ASP.NET Core MVC kullanılarak geliştirilmiştir.

## 🚀 Özellikler

### 🔐 Güvenlik
- **BCrypt** ile güvenli şifre hash'leme
- **JWT Token** tabanlı API authentication
- **Role-based authorization** (Admin, Manager, Waiter, Cashier, Kitchen, Accounting)
- **CORS** politikası ile güvenli API erişimi

### 📊 Yönetim Modülleri
- **Kullanıcı Yönetimi**: Personel kayıt ve yetkilendirme
- **Ürün Yönetimi**: Menü ürünleri ve kategorileri
- **Masa Yönetimi**: Masa durumu ve kapasite takibi
- **Sipariş Yönetimi**: Sipariş alma, takip ve durum güncelleme
- **Ödeme Yönetimi**: Ödeme alma ve raporlama
- **Muhasebe**: Günlük raporlar ve finansal takip

### 🛠️ Teknik Özellikler
- **Clean Architecture** (Entities, DataAccess, Business, Web, API)
- **Entity Framework Core** ile SQLite veritabanı
- **FluentValidation** ile input validation
- **Serilog** ile structured logging
- **Global Exception Handling** middleware
- **Unit Tests** (xUnit, Moq, FluentAssertions)
- **Swagger** API documentation

## 🏗️ Proje Yapısı

```
SD_Cafe/
├── SDCafe.Entities/          # Domain entities
├── SDCafe.DataAccess/        # Data access layer
├── SDCafe.Business/          # Business logic layer
├── SDCafe.Web/              # ASP.NET Core MVC Web App
├── SDCafe.API/              # REST API
└── SDCafe.Tests/            # Unit tests
```

## 🚀 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Repository'yi klonlayın**
   ```bash
   git clone https://github.com/your-username/SD_Cafe.git
   cd SD_Cafe
   ```

2. **Bağımlılıkları yükleyin**
   ```bash
   dotnet restore
   ```

3. **Veritabanını oluşturun**
   ```bash
   dotnet ef database update --project SDCafe.DataAccess --startup-project SDCafe.Web
   ```

4. **Uygulamayı çalıştırın**
   ```bash
   # Web uygulaması
   dotnet run --project SDCafe.Web
   
   # API
   dotnet run --project SDCafe.API
   ```

## 🔑 Varsayılan Kullanıcılar

Uygulama ilk çalıştırıldığında aşağıdaki kullanıcılar otomatik olarak oluşturulur:

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Admin | admin@sdcafe.com | admin123 |
| Manager | manager@sdcafe.com | admin123 |
| Waiter | waiter@sdcafe.com | admin123 |
| Cashier | cashier@sdcafe.com | admin123 |
| Kitchen | kitchen@sdcafe.com | admin123 |
| Accounting | accounting@sdcafe.com | admin123 |

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - Kullanıcı girişi

### Products
- `GET /api/products` - Tüm ürünleri listele
- `POST /api/products` - Yeni ürün ekle
- `PUT /api/products/{id}` - Ürün güncelle
- `DELETE /api/products/{id}` - Ürün sil

### Orders
- `GET /api/orders` - Tüm siparişleri listele
- `POST /api/orders` - Yeni sipariş oluştur
- `PUT /api/orders/{id}/status` - Sipariş durumu güncelle

## 🧪 Testler

```bash
# Tüm testleri çalıştır
dotnet test

# Coverage ile test çalıştır
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Logging

Uygulama Serilog kullanarak aşağıdaki lokasyonlara log yazar:
- **Console**: Geliştirme sırasında
- **File**: `logs/sdcafe-YYYY-MM-DD.log`

## 🔧 Konfigürasyon

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SDCafe.db"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key",
    "Issuer": "SDCafe",
    "Audience": "SDCafeUsers",
    "ExpirationHours": 8
  }
}
```

## 🚨 Güvenlik Notları

1. **Production'da** JWT secret key'i değiştirin
2. **HTTPS** kullanın
3. **Environment variables** ile hassas bilgileri saklayın
4. **Regular security updates** yapın

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 👥 Geliştirici

**Samet** - [GitHub](https://github.com/your-username)

## 📞 İletişim

- **E-posta**: your-email@example.com
- **GitHub**: [@your-username](https://github.com/your-username)

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!

