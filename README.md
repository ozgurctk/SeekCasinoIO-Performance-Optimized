# SeekCasinoIO - Clean Architecture Casino API (Performans İyileştirilmiş Versiyon)

Bu proje, .NET 9 kullanılarak Clean Architecture ile geliştirilmiş bir casino bilgi sistemi API'sidir. Bu versiyon, performans iyileştirmeleri içermektedir.

## Performans İyileştirmeleri

Bu versiyonda aşağıdaki performans iyileştirmeleri yapılmıştır:

1. N+1 Sorgu Probleminin Çözümü 
2. Sorgu Optimizasyonları ve Veritabanı İndeksleri
3. Seçici Veri Yükleme (Selective Loading)
4. Geliştirilmiş Önbellek Stratejisi
5. Daha Verimli Transaction Yönetimi
6. Asenkron I/O Optimizasyonları
7. LINQ Sorgu İyileştirmeleri
8. Toplu İşlem Desteği

## Proje Yapısı

Proje Clean Architecture prensiplerine uygun olarak katmanlı bir yapıda tasarlanmıştır:

- **Core**: Entity'ler ve ortak domain mantığı burada tanımlanır
- **Application**: İş kuralları, CQRS pattern, MediatR ve validation burada yer alır
- **Infrastructure**: Veritabanı işlemleri, repository implementasyonları ve harici servisler
- **API**: HTTP Controllers ve middlewares

## Kullanılan Teknolojiler ve Pattern'lar

- **.NET 9**: En yeni .NET sürümü
- **EF Core**: Veritabanı erişimi için ORM
- **CQRS Pattern**: Command ve Query sorumlulukları ayrıştırması
- **MediatR**: In-Process mesajlaşma için mediator pattern implementasyonu
- **MediatR Pipeline Behaviors**: Validation, logging, caching ve eventual consistency için
- **Repository Pattern & Unit of Work**: Veritabanı erişiminin soyutlanması
- **FluentValidation**: Validasyon kuralları için
- **ErrorOr**: Sonuç/Hata işlemleri için
- **Identity & JWT**: Kullanıcı yönetimi ve güvenlik

## Mimari Özellikleri

- **CQRS ve MediatR**: Command ve Query sorumlulukları ayrılmış, her biri için ayrı modeller kullanılmıştır
- **Pipeline Behaviors**: Tüm istekler için validation, logging, caching, ve eventual consistency davranışları
- **Validation Pipeline**: FluentValidation ile giriş doğrulama
- **Error Handling**: ErrorOr kütüphanesi ile tutarlı hata yönetimi
- **Eventual Consistency**: Transactional tutarlılık için middleware desteği
- **Repository Pattern**: Entity-specific ve generic repository implementasyonları
- **Authentication & Authorization**: JWT tabanlı kimlik doğrulama ve rol bazlı yetkilendirme

## Kimlik Doğrulama ve Yetkilendirme

Sistem, JWT tabanlı kimlik doğrulama kullanır ve aşağıdaki rolleri destekler:

- **Admin**: Tam yönetim yetkileri (Casino ekleme/düzenleme vb.)
- **Support**: Müşteri desteği işlemleri için yetkiler
- **Customer**: Standart kullanıcı yetkileri (Casino listeleme, görüntüleme vb.)

Varsayılan kullanıcılar:
- Admin: admin@seekcasino.io / Admin123!
- Support: support@seekcasino.io / Support123!

## Kurulum ve Çalıştırma

1. Projeyi klonlayın
2. SQL Server'ın yüklü olduğundan emin olun
3. `appsettings.json` dosyasında veritabanı bağlantı ayarlarını yapın
4. Migration'ları uygulayın:
   ```
   cd src/SeekCasinoIO.API
   dotnet ef database update
   ```
5. Projeyi çalıştırın:
   ```
   dotnet run
   ```
6. API'ye http://localhost:5000/swagger adresinden erişebilirsiniz

## API Endpoints

API aşağıdaki temel endpoint'leri sağlar:

### Authentication
- `POST /api/auth/register`: Yeni kullanıcı kaydı
- `POST /api/auth/login`: Kullanıcı girişi

### Casinos
- `GET /api/casinos`: Tüm casinoları listeler (Customer, Support, Admin rolleri için)
- `GET /api/casinos/{id}`: ID'ye göre casino detaylarını getirir (Customer, Support, Admin rolleri için)
- `POST /api/casinos`: Yeni bir casino ekler (Sadece Admin rolü için)

## Lisans

MIT