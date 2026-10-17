# Pagination standardı

Liste endpointleri `Result<PagedResult<T>>` döner. `PageIndex` **1 tabanlıdır**.
Parametre verilmezse ilk sayfa ve 20 kayıt döner. `PageSize` 1–100 aralığındadır.

```http
GET /api/reports?pageIndex=2&pageSize=20&status=PendingReview
```

Başarılı yanıtın alanları (örnekte öğeler kısaltılmıştır):

```json
{
  "isSuccess": true,
  "data": {
    "items": [],
    "pageIndex": 2,
    "pageSize": 20,
    "totalCount": 125,
    "totalPages": 7,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "errors": []
}
```

`totalCount`, kullanıcının erişebildiği ve tüm filtreleri karşılayan kayıtların toplamıdır;
yalnızca mevcut sayfanın uzunluğu değildir. `totalPages = ceil(totalCount / pageSize)`.
`hasPreviousPage = pageIndex > 1`, `hasNextPage = pageIndex < totalPages`.

## Parametre ve sınır davranışı

- `pageIndex < 1`, `pageSize < 1`, `pageSize > 100` veya `(pageIndex - 1) * pageSize`
  değerinin `Int32.MaxValue` sınırını aşması HTTP 400 döner. Değerler otomatik düzeltilmez.
- Sayısal olmayan veya `int` aralığı dışındaki parametreleri ASP.NET model binding HTTP 400 ile reddeder.
- Ortak doğrulama, MediatR'ın mevcut `ValidationBehavior` akışında endpoint kurallarıyla birlikte çalışır.
  FluentValidation hataları mevcut `{ isSuccess: false, errors: [...], traceId: ... }` biçimini kullanır.
  Model binding hataları mevcut ASP.NET validation problem yanıtını kullanmaya devam eder.
- Hiç kayıt yoksa `items=[]`, `totalCount=0`, `totalPages=0` döner.
- Son sayfanın ötesindeki geçerli indeks HTTP 200 ve boş `items` döner; istenen indeks ve gerçek toplam korunur.
  Bu durumda `hasPreviousPage`, kayıt olmasa bile `pageIndex > 1` formülünü izler.
- Mevcut yetki kontrolleri ve bulunamayan üst kaynaklar için 403/404 davranışları korunur.
- Count ve sayfa iki ardışık SQL sorgusudur; aralarında veri değişirse küçük tutarsızlıklar olabilir.
  Snapshot transaction kullanılmaz. Sayfalar arasında ekleme/silme, offset tabanlı pagination'da kaymaya yol açabilir.

## Geçiş yapılan rotalar

Tüm rotalar aşağıda `GET` olarak listelenmiştir. Mevcut filtreler geçerlidir.

| Grup | Rotalar |
| --- | --- |
| Raporlar | `/api/reports`, `/api/reports/mine`, `/api/reports/manageable` |
| Öneriler | `/api/suggestions` |
| Topluluk gönderileri | `/api/community-posts` |
| Duyurular | `/api/announcements`, `/api/announcements/manageable` |
| Bildirimler | `/api/notifications` |
| Kullanıcılar | `/api/users`, `/api/users/by-province/{provinceId}`, `/api/users/by-district/{districtId}`, `/api/users/by-neighborhood/{neighborhoodId}` |
| Konum kullanıcıları | `/api/provinces/{provinceId}/users`, `/api/districts/{districtId}/users`, `/api/neighborhoods/{neighborhoodId}/users` |
| Yorumlar | `/api/reports/{reportId}/comments`, `/api/suggestions/{suggestionId}/comments`, `/api/community-posts/{communityPostId}/comments` |
| Oy verenler | `/api/suggestions/{suggestionId}/votes/voters` |

İl/ilçe/mahalle referans listeleri, kategori/departman listeleri ve istatistikleri, kurum/birim listeleri,
atama seçenekleri, izin verilen durum geçişleri ve detay içindeki koleksiyonlar bu geçişe dahil değildir.
Mevcut referans cache anahtarları korunur; bu geçişte sayfalı listelere cache eklenmez.

### Oy verenler

`upvoters` ve `downvoters` kaldırıldı. Artık `data.items` tek bir liste içerir:

```ts
type VoterItem = {
  userId: string;
  fullName: string;
  votedAt: string;
  isUpvote: boolean;
};
```

`?isUpvote=true` yalnızca olumlu, `?isUpvote=false` yalnızca olumsuz oyları getirir;
parametre yoksa ikisi de gelir. `totalCount` bu filtreyi izler. Genel oy sayıları için
`GET /api/suggestions/{suggestionId}/votes/summary` kullanılır.

## Yeni bir liste endpointinde kullanım

Application `Common/Models` altındaki `PaginationRequest`, mevcut query record'larıyla
uyumlu bir temel record'dur. Filtreler query'de, pagination özellikleri temel record'da tutulur:

```csharp
public sealed record UserListQuery(string? Search = null)
    : PaginationRequest, IRequest<Result<PagedResult<UserListDto>>>;
```

Controller'da `[FromQuery] PaginationRequest pagination` kullanılır. Böylece URL'de
`pageIndex` ve `pageSize` düz parametrelerdir; `pagination.pageIndex` gerekli değildir.
Kimlik/rol alanlarını içeren MediatR query'si doğrudan HTTP'den bind edilmez.

```csharp
var result = await mediator.Send(
    new UserListQuery(search)
    {
        PageIndex = pagination.PageIndex,
        PageSize = pagination.PageSize
    }, cancellationToken);
return Ok(result);
```

Handler'da yetki/kapsam ve filtreler, sıralama ve projection'dan önce uygulanır:

```csharp
var query = context.Users.AsNoTracking();
if (!string.IsNullOrWhiteSpace(request.Search))
    query = query.Where(user => user.FirstName.Contains(request.Search));

var page = await query
    .OrderBy(user => user.FirstName)
    .ThenBy(user => user.Id)
    .Select(user => new UserListDto(user.Id, user.FirstName))
    .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

return Result<PagedResult<UserListDto>>.Success(page);
```

Extension `Common/Extensions` altındadır. Önce `CountAsync`, sonra SQL seviyesinde
`Skip/Take/ToListAsync` çalışır. Metadata `PagedResult<T>` içinde hesaplanır.
`ToList`/`AsEnumerable` çağırıp tüm sonuçları bellekte sayfalamayın.
Sıralamanın son anahtarı benzersiz olmalıdır; extension sorgunun iş sıralamasını seçmez.
DTO içindeki hesaplanan entity metotları yerine SQL'e çevrilebilir ifadeler kullanın.
Kullanıcı adı için `User.FullName` yerine `UserProjections.FullName.Invoke(x.User)` yazın ve
sorguya `.Expand()` ekleyin; `Expand` çağrılmazsa sorgu çalışırken hata verir.
Controller'a `[ProducesResponseType(typeof(Result<PagedResult<UserListDto>>), 200)]` ekleyin.

## UI geçişi ve yayınlama

Bu **kırıcı bir API değişikliğidir**: `data: T[]` artık `data: PagedResult<T>` olmuştur.
Parametre göndermeyen çağrılar da artık yalnızca ilk 20 kaydı alır. Eski rotalar veya
sayfalanmamış alternatif sözleşmeler eklenmemiştir.

```ts
type PagedResult<T> = {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};
```

- Web ve mobil API tiplerini `ApiResult<PagedResult<T>>` / `ApiResponse<PagedResult<T>>` olarak değiştirin.
- Dizi işlemlerinde `response.data` yerine API wrapper'ının `data.items` alanını kullanın.
  Axios'un HTTP response `data` alanı ile uygulama wrapper'ının `data` alanını ayırt edin.
- Sayfa numaralarını 1 tabanlı gönderin; sıfır tabanlı UI bileşenlerinde dönüşümü istemcide yapın.
- Filtre veya sayfa boyutu değişince ilk sayfaya dönün. Numbered pagination için `totalCount`/`totalPages` kullanın.
- İstemci query/cache anahtarlarına filtreler, kullanıcı kapsamı, `pageIndex` ve `pageSize` ekleyin.
- Oy verenlerin iki sekmesi için ayrı `isUpvote` filtreleri ve sayfa durumları tutun; toplamları summary'den okuyun.
- Silme sonrası sayfa boş kalırsa `totalPages > 0` için son geçerli sayfayı tekrar isteyin.
- Backend'i web/mobil uyarlamalarıyla birlikte yayınlayın. Bu değişiklik yalnızca backend ve bu geçiş dokümanını içerir.

Veritabanı migration'ı gerekmez. Mevcut `Skip/Take` kullanan seed ve AI aday sınırlandırma
işlemleri endpoint pagination'ı değildir ve değiştirilmemiştir.

## Doğrulama

```sh
dotnet build src/UrbanCommunity.Api/UrbanCommunity.Api.csproj
dotnet test tests/UrbanCommunity.Infrastructure.Tests/UrbanCommunity.Infrastructure.Tests.csproj
```

Pagination testleri metadata/sınırları, gerçek SQLite SQL sayfalamasını ve HTTP üzerinden
model binding, yetki, filtreler, 19 rota ve OpenAPI sözleşmesini kapsar.
HTTP testleri izole EF InMemory veritabanı ve test kimliği kullanır; üretim PostgreSQL,
JWT doğrulaması veya harici servisler üzerinde uçtan uca test değildir.
