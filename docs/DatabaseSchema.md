# Database Schema

Bu dokuman, UrbanCommunity backend projesindeki guncel Entity Framework Core modelini ozetler. Kaynak olarak `ApplicationDbContext`, domain entity siniflari ve `Persistence/Configurations` altindaki EF Core configuration siniflari dikkate alinmistir.

## Genel Notlar

- Veritabani provider'i PostgreSQL'dir.
- Konumsal alanlar icin PostGIS ve NetTopologySuite kullanilir.
- Identity altyapisi `IdentityDbContext<User, IdentityRole<Guid>, Guid>` uzerinden calisir.
- `User` entity'si Identity kullanici tablosunu genisletir ve veritabaninda `AspNetUsers` tablosuna karsilik gelir.
- `BaseEntity` kullanan domain tablolarinda ortak alanlar bulunur: `Id`, `CreatedAt`, `UpdatedAt`.
- Enum alanlari veritabaninda numeric deger olarak saklanir.

## Tablolar

Uygulama DbSet'leri:

| DbSet | Tablo | Aciklama |
| --- | --- | --- |
| `Users` | `AspNetUsers` | Identity kullanicilari ve domain profil alanlari |
| `Provinces` | `Provinces` | Il bilgileri |
| `Districts` | `Districts` | Ilce bilgileri |
| `Neighborhoods` | `Neighborhoods` | Mahalle bilgileri ve opsiyonel polygon verisi |
| `Departments` | `Departments` | Global hizmet turleri |
| `Organizations` | `Organizations` | Il veya ilce seviyesindeki kamu kurumlari |
| `OrganizationDepartments` | `OrganizationDepartments` | Kurumlardaki yerel hizmet birimleri ve iletisim bilgileri |
| `OrganizationDepartmentCategories` | `OrganizationDepartmentCategories` | Yerel birim-kategori yonlendirme eslemeleri |
| `ReportCategories` | `ReportCategories` | Rapor kategori bilgileri |
| `Reports` | `Reports` | Vatandas sorun raporlari |
| `ReportMedias` | `ReportMedias` | Rapor medya dosyalari |
| `ReportHistories` | `ReportHistories` | Rapor durum gecmisleri |
| `ReportSimilarityMatches` | `ReportSimilarityMatches` | Raporlar arasindaki kalici benzerlik eslesmeleri |
| `ReportComments` | `ReportComments` | Rapor yorumlari |
| `Suggestions` | `Suggestions` | Vatandas onerileri |
| `Votes` | `Votes` | Oneri oylari |
| `SuggestionComments` | `SuggestionComments` | Oneri yorumlari |
| `CommunityPosts` | `CommunityPosts` | Topluluk gonderileri |
| `CommunityPostMedias` | `CommunityPostMedias` | Topluluk gonderisi medya dosyalari |
| `CommunityPostComments` | `CommunityPostComments` | Topluluk gonderisi yorumlari |
| `Announcements` | `Announcements` | Admin/official duyurulari |
| `Notifications` | `Notifications` | Kullanici bildirimleri |
| `UserRefreshTokens` | `UserRefreshTokens` | Refresh token kayitlari |
| `AuditLogs` | `AuditLogs` | Kurum, Official erisimi ve rapor atama degisiklikleri |

Identity tarafinda ayrica `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens` tablolari olusur.

## ER Diyagrami

```mermaid
erDiagram
    PROVINCES {
        uuid Id PK
        string Name
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    DISTRICTS {
        uuid Id PK
        string Name
        uuid ProvinceId FK
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    NEIGHBORHOODS {
        uuid Id PK
        string Name
        string District
        string City
        uuid DistrictId FK
        geometry PolygonData
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    ASP_NET_USERS {
        uuid Id PK
        string FirstName
        string LastName
        int Role
        uuid NeighborhoodId FK
        uuid OrganizationDepartmentId FK
        int OfficialJurisdictionLevel
        uuid OfficialProvinceId FK
        uuid OfficialDistrictId FK
        uuid OfficialNeighborhoodId FK
        string ProfileImageUrl
        string Bio
        bool IsActive
        bool MustChangePassword
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    DEPARTMENTS {
        uuid Id PK
        string Name
        string Description
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    ORGANIZATIONS {
        uuid Id PK
        string Name
        int JurisdictionLevel
        uuid ProvinceId FK
        uuid DistrictId FK
        uuid NeighborhoodId FK
        bool IsActive
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    ORGANIZATION_DEPARTMENTS {
        uuid Id PK
        uuid OrganizationId FK
        uuid DepartmentId FK
        string DisplayName
        string ContactEmail
        string ContactPhone
        bool IsActive
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    ORGANIZATION_DEPARTMENT_CATEGORIES {
        uuid OrganizationDepartmentId PK,FK
        uuid ReportCategoryId PK,FK
    }

    REPORT_CATEGORIES {
        uuid Id PK
        string Name
        string Description
        string IconUrl
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    REPORTS {
        uuid Id PK
        string Title
        string Description
        geometry Location
        int Status
        int Priority
        string ContentHash
        timestamp ResolvedAt
        uuid UserId FK
        uuid CategoryId FK
        uuid NeighborhoodId FK
        uuid AssignedOrganizationDepartmentId FK
        bool AssignmentNeedsReview
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    AUDIT_LOGS {
        uuid Id PK
        uuid ActorUserId
        string Action
        string EntityType
        string EntityId
        json OldValues
        json NewValues
        timestamp CreatedAt
    }

    REPORT_MEDIAS {
        uuid Id PK
        uuid ReportId FK
        string Url
        string ThumbnailUrl
        string OriginalFileName
        string ContentType
        bigint FileSizeBytes
        int DisplayOrder
        int MediaType
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    REPORT_HISTORIES {
        uuid Id PK
        uuid ReportId FK
        int FromStatus
        int ToStatus
        string Note
        uuid ChangedByUserId FK
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    REPORT_SIMILARITY_MATCHES {
        uuid Id PK
        uuid ReportId FK
        uuid SimilarReportId FK
        double SimilarityScore
        string Reasoning
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    REPORT_COMMENTS {
        uuid Id PK
        uuid ReportId FK
        uuid UserId FK
        string Content
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    SUGGESTIONS {
        uuid Id PK
        string Title
        string Content
        int Status
        int VoteScore
        uuid UserId FK
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    VOTES {
        uuid Id PK
        uuid SuggestionId FK
        uuid UserId FK
        bool IsUpvote
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    SUGGESTION_COMMENTS {
        uuid Id PK
        uuid SuggestionId FK
        uuid UserId FK
        string Content
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    COMMUNITY_POSTS {
        uuid Id PK
        int Type
        string Title
        string Content
        int Status
        int Scope
        uuid UserId FK
        uuid ProvinceId FK
        uuid DistrictId FK
        uuid NeighborhoodId FK
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    COMMUNITY_POST_MEDIAS {
        uuid Id PK
        uuid CommunityPostId FK
        string Url
        string ThumbnailUrl
        string OriginalFileName
        string ContentType
        bigint FileSizeBytes
        int DisplayOrder
        int MediaType
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    COMMUNITY_POST_COMMENTS {
        uuid Id PK
        uuid CommunityPostId FK
        uuid UserId FK
        string Content
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    ANNOUNCEMENTS {
        uuid Id PK
        string Title
        string Content
        uuid UserId FK
        int Scope
        uuid ProvinceId FK
        uuid DistrictId FK
        uuid NeighborhoodId FK
        bool IsActive
        timestamp ExpiresAt
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    NOTIFICATIONS {
        uuid Id PK
        uuid UserId FK
        string Title
        string Message
        bool IsRead
        uuid RelatedEntityId
        int RelatedEntityType
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    USER_REFRESH_TOKENS {
        uuid Id PK
        uuid UserId FK
        string Token
        timestamp ExpiresAt
        string CreatedByIp
        timestamp RevokedAt
        string RevokedByIp
        string RevokedReason
        string ReplacedByToken
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    PROVINCES ||--o{ DISTRICTS : contains
    DISTRICTS ||--o{ NEIGHBORHOODS : contains
    NEIGHBORHOODS ||--o{ ASP_NET_USERS : residents
    PROVINCES ||--o{ ORGANIZATIONS : contains
    DISTRICTS ||--o{ ORGANIZATIONS : contains
    NEIGHBORHOODS ||--o{ ORGANIZATIONS : contains
    ORGANIZATIONS ||--o{ ORGANIZATION_DEPARTMENTS : owns
    DEPARTMENTS ||--o{ ORGANIZATION_DEPARTMENTS : defines
    ORGANIZATION_DEPARTMENTS ||--o{ ASP_NET_USERS : officials
    PROVINCES ||--o{ ASP_NET_USERS : province_authority
    DISTRICTS ||--o{ ASP_NET_USERS : district_authority
    NEIGHBORHOODS ||--o{ ASP_NET_USERS : neighborhood_authority

    ASP_NET_USERS ||--o{ REPORTS : creates
    REPORT_CATEGORIES ||--o{ REPORTS : categorizes
    NEIGHBORHOODS ||--o{ REPORTS : locates
    ORGANIZATION_DEPARTMENTS ||--o{ REPORTS : handles
    REPORTS ||--o{ REPORT_MEDIAS : contains
    REPORTS ||--o{ REPORT_HISTORIES : tracks
    REPORTS ||--o{ REPORT_SIMILARITY_MATCHES : analyzed_report
    REPORTS ||--o{ REPORT_SIMILARITY_MATCHES : referenced_similar_report
    ASP_NET_USERS ||--o{ REPORT_HISTORIES : changes
    REPORTS ||--o{ REPORT_COMMENTS : has
    ASP_NET_USERS ||--o{ REPORT_COMMENTS : writes

    ASP_NET_USERS ||--o{ SUGGESTIONS : creates
    SUGGESTIONS ||--o{ VOTES : receives
    ASP_NET_USERS ||--o{ VOTES : casts
    SUGGESTIONS ||--o{ SUGGESTION_COMMENTS : has
    ASP_NET_USERS ||--o{ SUGGESTION_COMMENTS : writes

    ASP_NET_USERS ||--o{ COMMUNITY_POSTS : creates
    PROVINCES ||--o{ COMMUNITY_POSTS : scopes
    DISTRICTS ||--o{ COMMUNITY_POSTS : scopes
    NEIGHBORHOODS ||--o{ COMMUNITY_POSTS : scopes
    COMMUNITY_POSTS ||--o{ COMMUNITY_POST_MEDIAS : contains
    COMMUNITY_POSTS ||--o{ COMMUNITY_POST_COMMENTS : has
    ASP_NET_USERS ||--o{ COMMUNITY_POST_COMMENTS : writes

    ASP_NET_USERS ||--o{ ANNOUNCEMENTS : publishes
    PROVINCES ||--o{ ANNOUNCEMENTS : targets
    DISTRICTS ||--o{ ANNOUNCEMENTS : targets
    NEIGHBORHOODS ||--o{ ANNOUNCEMENTS : targets

    ASP_NET_USERS ||--o{ NOTIFICATIONS : receives
    ASP_NET_USERS ||--o{ USER_REFRESH_TOKENS : owns
```

## Ortak Alanlar

`BaseEntity` kullanan tablolarda:

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | Primary key |
| `CreatedAt` | `DateTime` | Evet | Kayit olusturma zamani |
| `UpdatedAt` | `DateTime?` | Hayir | Son guncelleme zamani |

`AspNetUsers` Identity'den geldigi icin `BaseEntity` kullanmaz; fakat domain tarafinda `CreatedAt` ve `UpdatedAt` alanlari ayrica vardir.

## Lokasyon

### Provinces

Il tablosudur.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Name` | `string` | Evet | Il adi |

Iliskiler:
- `Province 1 - N District`
- `Province 1 - N CommunityPost`
- `Province 1 - N Announcement`

### Districts

Ilce tablosudur.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Name` | `string` | Evet | Ilce adi |
| `ProvinceId` | `Guid` | Evet | FK -> `Provinces.Id` |

Iliskiler:
- `District N - 1 Province`
- `District 1 - N Neighborhood`
- `District 1 - N CommunityPost`
- `District 1 - N Announcement`

### Neighborhoods

Mahalle tablosudur. Hem eski metinsel `City/District` alanlarini hem de yeni hiyerarsik `DistrictId` iliskisini tasir.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Name` | `string(100)` | Evet | Mahalle adi |
| `District` | `string(100)` | Evet | Eski/metinsel ilce adi |
| `City` | `string(50)` | Evet | Eski/metinsel il adi |
| `DistrictId` | `Guid` | Evet | FK -> `Districts.Id` |
| `PolygonData` | `geometry?` | Hayir | Opsiyonel PostGIS mahalle siniri; MVP surumunde seed kayitlarinda null olabilir |

Konum karari:
- `Neighborhood.PolygonData` gelecekte koordinattan otomatik mahalle bulma ve sinir dogrulama icin ayrilmistir.
- Mevcut MVP'de mahalle sinirlari seed edilmez; `PolygonData = null` gecerli bir durumdur.
- Raporun zorunlu idari konumu `NeighborhoodId`, opsiyonel kesin sorun noktasi ise WGS84 (`SRID 4326`) `Report.Location` alaniyla tutulur.
- Mobil istemci kesin konum paylasilacaksa `Latitude` ve `Longitude` degerlerini birlikte gonderir; yalnizca mahalle/adres ile bildirim yapilacaksa ikisini de bos birakir.
- Poligon verisi bulunmadigi icin backend bu asamada koordinatin secilen mahalle sinirlari icinde oldugunu kontrol etmez.

Iliskiler:
- `Neighborhood N - 1 District`
- `Neighborhood 1 - N User`
- `Neighborhood 1 - N Report`
- `Neighborhood 1 - N CommunityPost`
- `Neighborhood 1 - N Announcement`

Indexler:
- Unique: `City + District + Name`

## Kullanici ve Yetkilendirme

### AspNetUsers

Identity kullanici tablosudur. Domain `User` sinifi bu tabloyu genisletir.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `FirstName` | `string(50)` | Evet | Ad |
| `LastName` | `string(50)` | Evet | Soyad |
| `Email` | `string?` | Hayir | Identity email |
| `UserName` | `string?` | Hayir | Identity username |
| `PasswordHash` | `string?` | Hayir | Identity password hash |
| `Role` | `UserRole` | Evet | `Citizen`, `Official`, `Admin` |
| `NeighborhoodId` | `Guid?` | Hayir | FK -> `Neighborhoods.Id` |
| `OrganizationDepartmentId` | `Guid?` | Hayir | FK -> `OrganizationDepartments.Id`; Official kullanicinin yerel birimi |
| `OfficialJurisdictionLevel` | `OfficialJurisdictionLevel?` | Hayir | Official yetki alaninin idari seviyesi |
| `OfficialProvinceId` | `Guid?` | Hayir | FK -> `Provinces.Id`; il kapsamli Official |
| `OfficialDistrictId` | `Guid?` | Hayir | FK -> `Districts.Id`; ilce kapsamli Official |
| `OfficialNeighborhoodId` | `Guid?` | Hayir | FK -> `Neighborhoods.Id`; mahalle kapsamli Official |
| `ProfileImageUrl` | `string(500)?` | Hayir | Profil gorseli |
| `Bio` | `string(300)?` | Hayir | Profil aciklamasi |
| `IsActive` | `bool` | Evet | Default `true` |
| `MustChangePassword` | `bool` | Evet | Gecici sifreyle olusturulan Official icin `true` |
| `CreatedAt` | `DateTime` | Evet | Kayit zamani |
| `UpdatedAt` | `DateTime?` | Hayir | Son guncelleme |

Kurallar:
- `NeighborhoodId` kullanicinin ikamet ettigi mahalledir ve resmi yetki alanindan bagimsizdir.
- Yeni Official kullanici icin `OrganizationDepartmentId`, `OfficialJurisdictionLevel` ve seviyeye
  karsilik gelen tek bir konum FK'si zorunludur.
- Province seviyesinde yalniz `OfficialProvinceId`, District seviyesinde yalniz
  `OfficialDistrictId`, Neighborhood seviyesinde yalniz `OfficialNeighborhoodId`
  dolu olabilir.
- Citizen ve Admin icin tum Official yetki alanlari ile departman baglantilari null olmalidir.
- `CK_AspNetUsers_OfficialAccess` check constraint'i bu kurallari veritabani
  seviyesinde korur.

Indexler:
- `Role`
- `IsActive`
- `OrganizationDepartmentId`
- `NeighborhoodId`
- `OfficialJurisdictionLevel + OfficialProvinceId`
- `OfficialJurisdictionLevel + OfficialDistrictId`
- `OfficialJurisdictionLevel + OfficialNeighborhoodId`

### UserRefreshTokens

Refresh token rotasyonu ve iptal bilgilerini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `Token` | `string(512)` | Evet | Refresh token |
| `ExpiresAt` | `DateTime` | Evet | Gecerlilik bitisi |
| `CreatedByIp` | `string(64)?` | Hayir | Olusturan IP |
| `RevokedAt` | `DateTime?` | Hayir | Iptal zamani |
| `RevokedByIp` | `string(64)?` | Hayir | Iptal eden IP |
| `RevokedReason` | `string(512)?` | Hayir | Iptal nedeni |
| `ReplacedByToken` | `string?` | Hayir | Rotasyon sonrasi token referansi |

Iliskiler:
- `UserRefreshToken N - 1 AspNetUser`

## Organizasyon

### Departments

`Fen Isleri` ve `Temizlik Isleri` gibi global hizmet turlerini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Name` | `string(150)` | Evet | Birim adi |
| `Description` | `string(500)?` | Hayir | Aciklama |
Iliskiler:
- `Department 1 - N OrganizationDepartment`

Indexler:
- Unique: `Name`

### Organizations

Il veya ilce seviyesinde hizmet veren belediye/kamu kurumlarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Name` | `string(200)` | Evet | Kurum adi |
| `JurisdictionLevel` | `OrganizationJurisdictionLevel` | Evet | `Province`, `District` veya `Neighborhood` |
| `ProvinceId` | `Guid` | Evet | FK -> `Provinces.Id` |
| `DistrictId` | `Guid?` | Hayir | District ve Neighborhood seviyesinde FK -> `Districts.Id` |
| `NeighborhoodId` | `Guid?` | Hayir | Neighborhood seviyesinde FK -> `Neighborhoods.Id` |
| `IsActive` | `bool` | Evet | Default `true` |

Kurallar:
- Province seviyesinde yalniz `ProvinceId` doludur.
- District seviyesinde `ProvinceId` ve `DistrictId` doludur.
- Neighborhood seviyesinde `ProvinceId`, `DistrictId` ve `NeighborhoodId` doludur.
- Rapor atamasinda en spesifik aktif kurum tercih edilir: Neighborhood, District, Province.

### OrganizationDepartments

Global bir hizmet turunun belirli bir kurumdaki yerel karsiligidir.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `OrganizationId` | `Guid` | Evet | FK -> `Organizations.Id` |
| `DepartmentId` | `Guid` | Evet | FK -> `Departments.Id` |
| `DisplayName` | `string(150)?` | Hayir | Kurumdaki gorunen birim adi |
| `ContactEmail` | `string(100)?` | Hayir | Yerel iletisim e-postasi |
| `ContactPhone` | `string(20)?` | Hayir | Yerel iletisim telefonu |
| `IsActive` | `bool` | Evet | Default `true` |

Unique index: `OrganizationId + DepartmentId`.

### OrganizationDepartmentCategories

Yerel birimlerin sorumlu oldugu rapor kategorilerini tutan bilesik anahtarli
esleme tablosudur.

## Raporlama Modulu

### ReportCategories

Rapor kategorilerini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Name` | `string(50)` | Evet | Kategori adi |
| `Description` | `string(250)?` | Hayir | Aciklama |
| `IconUrl` | `string(500)?` | Hayir | Ikon URL |

Iliskiler:
- `ReportCategory 1 - N Report`

### Reports

Vatandas tarafindan olusturulan sorun raporlarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Title` | `string(150)` | Evet | Baslik |
| `Description` | `string(1000)` | Evet | Aciklama |
| `Location` | `geometry(Point)` | Evet | PostGIS nokta konumu |
| `Status` | `ReportStatus` | Evet | Default `AiReview` |
| `Priority` | `ReportPriority` | Evet | Default `Medium` |
| `ContentHash` | `string(256)?` | Hayir | Normalize baslik ve aciklamanin SHA-256 ozeti; birebir tekrar tespiti |
| `ResolvedAt` | `DateTime?` | Hayir | Cozulme zamani |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `CategoryId` | `Guid` | Evet | FK -> `ReportCategories.Id` |
| `NeighborhoodId` | `Guid` | Evet | FK -> `Neighborhoods.Id` |
| `AssignedOrganizationDepartmentId` | `Guid?` | Hayir | FK -> `OrganizationDepartments.Id` |
| `AssignmentNeedsReview` | `bool` | Evet | Atama konum/kategori degisimi sonrasi yeniden incelenmeli |

Iliskiler:
- `Report N - 1 User`
- `Report N - 1 ReportCategory`
- `Report N - 1 Neighborhood`
- `Report N - 1 OrganizationDepartment`
- `Report 1 - N ReportMedia`
- `Report 1 - N ReportHistory`
- `Report 1 - N ReportSimilarityMatch` (`ReportId`, analiz edilen rapor)
- `Report 1 - N ReportSimilarityMatch` (`SimilarReportId`, benzer bulunan rapor)
- `Report 1 - N ReportComment`

Check constraint:
- `CK_Reports_ResolvedAt_Matches_Status`: status `Resolved` ise `ResolvedAt` dolu, diger statuslerde null olmalidir.

Indexler:
- `Status`
- `Priority`
- `NeighborhoodId + Status`
- `ContentHash`
- `AssignedOrganizationDepartmentId`
- `AssignmentNeedsReview`
- Spatial index: `Location`

### ReportMedias

Raporlara bagli medya dosyalarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `ReportId` | `Guid` | Evet | FK -> `Reports.Id` |
| `Url` | `string(500)` | Evet | Medya URL |
| `ThumbnailUrl` | `string(500)?` | Hayir | Thumbnail URL |
| `OriginalFileName` | `string(255)?` | Hayir | Orijinal dosya adi |
| `ContentType` | `string(100)?` | Hayir | MIME type |
| `FileSizeBytes` | `long?` | Hayir | Dosya boyutu |
| `DisplayOrder` | `int` | Evet | Default `0` |
| `MediaType` | `MediaType` | Evet | `Image`, `Video` |

Check constraint:
- `CK_ReportMedias_FileSize_Positive`: `FileSizeBytes` null veya pozitif olmali.
- `CK_ReportMedias_DisplayOrder_NonNegative`: `DisplayOrder >= 0`.

Indexler:
- `ReportId + DisplayOrder`

### ReportHistories

Rapor status degisikliklerini takip eder.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `ReportId` | `Guid` | Evet | FK -> `Reports.Id` |
| `FromStatus` | `ReportStatus` | Evet | Onceki status |
| `ToStatus` | `ReportStatus` | Evet | Yeni status |
| `Note` | `string(500)?` | Hayir | Degisiklik notu |
| `ChangedByUserId` | `Guid?` | Hayir | FK -> `AspNetUsers.Id` |

Check constraint:
- `CK_ReportHistories_StatusChange_IsRequired`: `FromStatus <> ToStatus`.

Indexler:
- `ReportId + CreatedAt`

### ReportSimilarityMatches

Bir rapor icin bulunan benzer raporlari, benzerlik skorunu ve aciklanabilir
gerekceyi kalici olarak tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `ReportId` | `Guid` | Evet | FK -> `Reports.Id`; analiz edilen/yeni rapor |
| `SimilarReportId` | `Guid` | Evet | FK -> `Reports.Id`; benzer bulunan mevcut rapor |
| `SimilarityScore` | `double` | Evet | `0.0-1.0` arasi benzerlik skoru |
| `Reasoning` | `string(500)` | Evet | Eslesmenin kisa gerekcesi |

Navigation yonleri:

- `Report.SimilarReportMatches`: Bu rapor analiz edilirken bulunan eslesmeler.
- `Report.ReferencedAsSimilarMatches`: Bu raporu benzer rapor olarak referanslayan eslesmeler.

Check constraint'ler:

- `CK_ReportSimilarityMatches_DifferentReports`: Bir rapor kendisiyle eslesemez.
- `CK_ReportSimilarityMatches_Score_Range`: Skor `0.0-1.0` araliginda olmalidir.

Indexler:

- Unique: `ReportId + SimilarReportId`
- `SimilarReportId`

Silme davranisi:

- Eslesmenin herhangi bir tarafindaki rapor silinirse ilgili eslesme kaydi cascade silinir.

### ReportComments

Rapor yorumlarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `ReportId` | `Guid` | Evet | FK -> `Reports.Id` |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `Content` | `string(1000)` | Evet | Yorum metni |

## Oneri Modulu

### Suggestions

Vatandas onerilerini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Title` | `string(200)` | Evet | Baslik |
| `Content` | `string(2000)` | Evet | Icerik |
| `Status` | `SuggestionStatus` | Evet | Default `Active` |
| `VoteScore` | `int` | Evet | Default `0` |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |

Iliskiler:
- `Suggestion N - 1 User`
- `Suggestion 1 - N Vote`
- `Suggestion 1 - N SuggestionComment`

Indexler:
- `Status`

### Votes

Oneri oylarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `SuggestionId` | `Guid` | Evet | FK -> `Suggestions.Id` |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `IsUpvote` | `bool` | Evet | `true`: +1, `false`: -1 |

Indexler:
- Unique: `UserId + SuggestionId`

Kural:
- Bir kullanici ayni oneriyi yalnizca bir kez oylayabilir.

### SuggestionComments

Oneri yorumlarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `SuggestionId` | `Guid` | Evet | FK -> `Suggestions.Id` |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `Content` | `string(1000)` | Evet | Yorum metni |

## Topluluk Gonderileri

### CommunityPosts

Mahalle, ilce veya il kapsaminda gorunebilen topluluk gonderilerini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Type` | `PostType` | Evet | `HelpRequested`, `BorrowTool`, `LostAndFound` |
| `Title` | `string(200)` | Evet | Baslik |
| `Content` | `string(2000)` | Evet | Icerik |
| `Status` | `CommunityPostStatus` | Evet | Default `Active` |
| `Scope` | `CommunityPostScope` | Evet | Default `Neighborhood` |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `ProvinceId` | `Guid?` | Hayir | FK -> `Provinces.Id` |
| `DistrictId` | `Guid?` | Hayir | FK -> `Districts.Id` |
| `NeighborhoodId` | `Guid?` | Hayir | FK -> `Neighborhoods.Id` |

Check constraint:
- `CK_CommunityPosts_Scope_Location`: scope'a gore sadece ilgili lokasyon kolonu dolu olabilir.

Scope kurallari:
- `Scope = Province`: `ProvinceId` dolu, `DistrictId` ve `NeighborhoodId` null.
- `Scope = District`: `DistrictId` dolu, `ProvinceId` ve `NeighborhoodId` null.
- `Scope = Neighborhood`: `NeighborhoodId` dolu, `ProvinceId` ve `DistrictId` null.

Iliskiler:
- `CommunityPost N - 1 User`
- `CommunityPost N - 1 Province`
- `CommunityPost N - 1 District`
- `CommunityPost N - 1 Neighborhood`
- `CommunityPost 1 - N CommunityPostMedia`
- `CommunityPost 1 - N CommunityPostComment`

Indexler:
- `Status`
- `Type`
- `Scope + ProvinceId + Status`
- `Scope + DistrictId + Status`
- `Scope + NeighborhoodId + Status`

### CommunityPostMedias

Topluluk gonderilerine bagli medya dosyalarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `CommunityPostId` | `Guid` | Evet | FK -> `CommunityPosts.Id` |
| `Url` | `string(500)` | Evet | Medya URL |
| `ThumbnailUrl` | `string(500)?` | Hayir | Thumbnail URL |
| `OriginalFileName` | `string(255)?` | Hayir | Orijinal dosya adi |
| `ContentType` | `string(100)?` | Hayir | MIME type |
| `FileSizeBytes` | `long?` | Hayir | Dosya boyutu |
| `DisplayOrder` | `int` | Evet | Default `0` |
| `MediaType` | `MediaType` | Evet | `Image`, `Video` |

Check constraint:
- `CK_CommunityPostMedias_FileSize_Positive`: `FileSizeBytes` null veya pozitif olmali.
- `CK_CommunityPostMedias_DisplayOrder_NonNegative`: `DisplayOrder >= 0`.

Indexler:
- `CommunityPostId + DisplayOrder`

### CommunityPostComments

Topluluk gonderisi yorumlarini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `CommunityPostId` | `Guid` | Evet | FK -> `CommunityPosts.Id` |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `Content` | `string(1000)` | Evet | Yorum metni |

## Duyuru Modulu

### Announcements

Admin veya official kullanicilarin yayinladigi duyurulari tutar. Duyuru tum kullanicilara, belirli bir ile, belirli bir ilceye veya belirli bir mahalleye hedeflenebilir.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `Title` | `string(200)` | Evet | Duyuru basligi |
| `Content` | `string(2000)` | Evet | Duyuru icerigi |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id`, yayinlayan kullanici |
| `Scope` | `AnnouncementScope` | Evet | Default `All` |
| `ProvinceId` | `Guid?` | Hayir | FK -> `Provinces.Id`, il hedefi |
| `DistrictId` | `Guid?` | Hayir | FK -> `Districts.Id`, ilce hedefi |
| `NeighborhoodId` | `Guid?` | Hayir | FK -> `Neighborhoods.Id`, mahalle hedefi |
| `IsActive` | `bool` | Evet | Default `true` |
| `ExpiresAt` | `DateTime?` | Hayir | Duyurunun gecerlilik bitisi |

Iliskiler:
- `Announcement N - 1 User`
- `Announcement N - 1 Province`
- `Announcement N - 1 District`
- `Announcement N - 1 Neighborhood`

Check constraint:
- `CK_Announcements_ExpiresAt_After_CreatedAt`: `ExpiresAt` null olabilir; doluysa `CreatedAt` degerinden sonra olmalidir.
- `CK_Announcements_Scope_Location`: scope degerine gore yalnizca ilgili lokasyon kolonu dolu olabilir.

Scope kurallari:
- `Scope = All`: tum lokasyon kolonlari null.
- `Scope = Province`: yalnizca `ProvinceId` dolu.
- `Scope = District`: yalnizca `DistrictId` dolu.
- `Scope = Neighborhood`: yalnizca `NeighborhoodId` dolu.

Indexler:
- `Scope + ProvinceId + IsActive`
- `Scope + DistrictId + IsActive`
- `Scope + NeighborhoodId + IsActive`
- `ExpiresAt`

Not:
- `User`, `Province`, `District` ve `Neighborhood` iliskileri configuration icinde explicit tanimlanmistir.

## Bildirim Modulu

### Notifications

Kullanici bildirimlerini tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `UserId` | `Guid` | Evet | FK -> `AspNetUsers.Id` |
| `Title` | `string(150)` | Evet | Bildirim basligi |
| `Message` | `string(500)` | Evet | Bildirim mesaji |
| `IsRead` | `bool` | Evet | Okundu bilgisi |
| `RelatedEntityId` | `Guid?` | Hayir | Iliskili entity id |
| `RelatedEntityType` | `NotificationRelatedEntityType?` | Hayir | Iliskili entity tipi |

Check constraint:
- `CK_Notifications_RelatedEntity_Consistency`: `RelatedEntityId` ve `RelatedEntityType` birlikte dolu veya birlikte null olmalidir.

Indexler:
- `UserId + IsRead`

## Audit

### AuditLogs

Kurum, yerel birim, kategori eslemesi, Official erisimi ve rapor atama
degisikliklerinin onceki/yeni degerlerini JSON olarak tutar.

| Alan | Tip | Zorunlu | Aciklama |
| --- | --- | --- | --- |
| `Id` | `Guid` | Evet | PK |
| `ActorUserId` | `Guid?` | Hayir | Islemi yapan oturum kullanicisi |
| `Action` | `string(20)` | Evet | `Added`, `Modified`, `Deleted` |
| `EntityType` | `string(100)` | Evet | Degisen entity tipi |
| `EntityId` | `string(100)` | Evet | Tekil veya bilesik anahtar |
| `OldValues` | `jsonb?` | Hayir | Degisen alanlarin onceki degerleri |
| `NewValues` | `jsonb?` | Hayir | Degisen alanlarin yeni degerleri |

Parola hash'i, security stamp ve token alanlari audit verisine dahil edilmez.

## Enumlar

| Enum | Degerler |
| --- | --- |
| `UserRole` | `Citizen = 1`, `Official = 2`, `Admin = 3` |
| `OfficialJurisdictionLevel` | `Province = 1`, `District = 2`, `Neighborhood = 3` |
| `OrganizationJurisdictionLevel` | `Province = 1`, `District = 2`, `Neighborhood = 3` |
| `ReportStatus` | `PendingReview = 1`, `AiReview = 2`, `Confirmed = 3`, `InProgress = 4`, `Resolved = 5`, `Rejected = 6` |
| `ReportPriority` | `Low = 1`, `Medium = 2`, `High = 3`, `Urgent = 4` |
| `SuggestionStatus` | `Active = 1`, `Reviewing = 2`, `Implementation = 3`, `Closed = 4` |
| `PostType` | `HelpRequested = 1`, `BorrowTool = 2`, `LostAndFound = 3` |
| `CommunityPostStatus` | `Active = 1`, `Reserved = 2`, `Resolved = 3`, `Expired = 4`, `Archived = 5` |
| `CommunityPostScope` | `Province = 1`, `District = 2`, `Neighborhood = 3` |
| `AnnouncementScope` | `All = 0`, `Province = 1`, `District = 2`, `Neighborhood = 3` |
| `MediaType` | `Image = 1`, `Video = 2` |
| `NotificationRelatedEntityType` | `Report = 1`, `Suggestion = 2`, `CommunityPost = 3`, `Announcement = 4` |

Rapor durumlarinin is kurallari ve izin verilen gecisleri icin
[ReportStatusWorkflow.md](ReportStatusWorkflow.md) dokumanina bakiniz.

## Tasarim Kararlari

- Kullanici lokasyonu dogrudan il/ilce kolonlariyla degil, `NeighborhoodId` uzerinden cozulur. Ilce ve il bilgisi mahalle hiyerarsisinden bulunur.
- Raporlar mahalle bazlidir ve konum icin PostGIS `Point` kullanir.
- `ContentHash`, normalize edilmis baslik ve aciklamadan uretilen deterministik SHA-256 degeridir. Yalnizca birebir ayni icerigi hizli tespit eder.
- Anlamsal benzerlik sonuclari `ReportSimilarityMatches` tablosunda skor ve gerekceyle saklanir; `ContentHash` bu tablonun yerine gecmez.
- Topluluk gonderileri scope bazlidir. Bir community post ayni anda yalnizca il, ilce veya mahalle kapsamlarindan birine sahip olabilir.
- Duyurular scope bazlidir ve scope-lokasyon tutarliligi database constraint ile korunur.
- Onerilerde oy toplami `VoteScore` ile tutulur; tekil kullanici oyu unique index ile korunur.
- Bildirimler polymorphic referansi `RelatedEntityId + RelatedEntityType` ikilisiyle temsil eder.
