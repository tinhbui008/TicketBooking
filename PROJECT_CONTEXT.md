# TicketBooking — Project Context (hand-off cho Claude Code)

> Đặt file này ở gốc repo (`E:\Dev\TicketBooking\PROJECT_CONTEXT.md`). Khi mở project
> trong Claude Code desktop, bảo nó đọc file này trước để nắm bối cảnh.

## 1. Đây là gì

Dự án **học tập** (không phải production) do Tính tự dựng để "nén chặt" kiến thức
microservices .NET sau khi đã làm một loạt demo rời (Clean Architecture, DDD, TDD,
Redis, RabbitMQ, MassTransit/Saga, OpenTelemetry) và phân tích repo `dotnet/eShop`.

**Domain:** hệ **đặt vé sự kiện** (ticket booking).

**Luật nghiệp vụ cốt lõi (invariant):** *Một ghế không thể được giữ/đặt bởi hai người
cùng lúc.* Toàn bộ thiết kế xoay quanh việc bảo vệ luật này.

## 2. Cách làm việc mong muốn (QUAN TRỌNG — đọc kỹ)

Đây là bài tập "tự viết từ trí nhớ". Vai trò của trợ lý là **coach**, KHÔNG phải
người viết code hộ:

- **KHÔNG** tự ý viết/sửa code khi chưa được yêu cầu. Người dùng tự code.
- Khi được nhờ review: chỉ ra *cái gì cần làm* + *vì sao* + *chỗ sai*, để người dùng
  tự sửa. Chỉ đưa code hoàn chỉnh khi người dùng xin rõ ràng.
- Ưu tiên review **thiết kế / logic / DDD** (thứ compiler không bắt được). Lỗi biên
  dịch/cú pháp thì nhắc người dùng tự chạy `dotnet build` để bắt.
- Giải thích chi tiết, dễ hiểu cho người đang học (không giả định quá nhiều).
- Ngôn ngữ: **tiếng Việt**.

## 3. Tech stack

- **.NET 10** (SDK 10.0.x), C#.
- Test: **xUnit**.
- IDE người dùng: JetBrains Rider / Antigravity (có lúc dùng Claude Code).
- Chưa dùng Redis/RabbitMQ/EF Core trong code (Redis thêm ở Phase 2, còn lại ở các
  phase sau).

## 4. Kiến trúc — Clean Architecture 4 tầng

```
TicketBooking.Api            →  Application + Infrastructure   (Minimal API, HTTP)
TicketBooking.Infrastructure →  Application                    (repo in-memory)
TicketBooking.Application     →  Domain                         (use case, interface)
TicketBooking.Domain         →  (không phụ thuộc ai)            (entity + luật nghiệp vụ)
TicketBooking.Tests          →  Domain + Application + Infrastructure (unit test xUnit)
```
Quy tắc vàng: **mọi phụ thuộc hướng vào trong. Domain độc lập.**

## 5. Trạng thái hiện tại — PHASE 1 ĐÃ XONG

### Domain (hoàn chỉnh, có test)
- `ESeatStatus` enum: `Available`, `Held`, `Booked`.
- `Seat`: `Id` (Guid, tự sinh), `Name` (get-only, vd "A5"), `Status` (private set).
  Method đổi trạng thái `Hold()/Book()/Release()` là **`internal`** (chỉ Event gọi,
  chặn thế giới ngoài lách cửa). Constructor tự đặt `Available`.
- `Event` (**aggregate root**): `Id`, `Name`, `_seats` (private List) + `Seats`
  (IReadOnlyCollection, chỉ đọc). Constructor có guard (`ArgumentException`): tên
  event rỗng, không có ghế, tên ghế rỗng/khoảng trắng, tên ghế trùng.
  Method **`HoldSeat(seatName)`** = cửa duy nhất giữ ghế: tìm ghế → không có thì
  `throw SeatNotFoundException` → không `Available` thì
  `throw SeatNotAvailableException` (INVARIANT double-book) → `seat.Hold()`.
- `Exceptions/DomainException.cs`: lớp cha abstract `DomainException` + 3 exception
  nghiệp vụ `EventNotFoundException`, `SeatNotFoundException`,
  `SeatNotAvailableException`.
- `EBookingStatus` enum: `SeatHeld`, `Confirmed`, `Expired`, `Cancelled`
  (`Cancelled` chưa có method nào dùng).
- `Booking` (aggregate): `Id`, `EventId`, `SeatName`, `Status` (private set).
  Constructor đặt `SeatHeld`, guard `eventId` khác `Guid.Empty` và `seatName` không
  rỗng. Method `Confirm()` và `Expire()` có **guard**
  (`if Status != SeatHeld throw InvalidOperationException`).

### Application (hoàn chỉnh)
- `IEventRepository`: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`.
- `IBookingRepository`: `GetByIdAsync`, `AddAsync`.
- `BookingService.CreateAsync(eventId, seatName)` — use case điều phối 2 aggregate:
  lấy Event (không có → `EventNotFoundException`) → `HoldSeat` → `UpdateAsync` →
  tạo `Booking` → `AddAsync` → trả về.
  Chỉ điều phối, KHÔNG lặp lại logic nghiệp vụ (nằm trong Domain).

### Infrastructure (hoàn chỉnh)
- `InMemoryEventRepository`, `InMemoryBookingRepository`: `ConcurrentDictionary`
  theo Id, dùng `Task.FromResult` (không async-no-await). `AddAsync` trùng Id →
  `InvalidOperationException`. EventRepo **seed sẵn** 1 event "Concert-01" với ghế
  A1–A5.

### Api (hoàn chỉnh)
- DI: repository `AddSingleton` (giữ state in-memory), `BookingService` `AddScoped`.
- Trả về DTO (`EventResponse`, `SeatResponse`, `BookingResponse`), không trả thẳng
  entity. Enum serialize dạng chữ (`JsonStringEnumConverter`).
- `GET /events` — liệt kê event + ghế (để lấy eventId).
- `POST /bookings` — body `{ EventId, SeatName }`; `EventId`/`SeatName` rỗng → 400;
  try/catch map `EventNotFoundException`/`SeatNotFoundException → 404`,
  `SeatNotAvailableException → 409`, thành công → 201.
- `GET /bookings/{id:guid}` — trả booking hoặc 404.
- Record `CreateBookingRequest(Guid EventId, string SeatName)`.

### Test (23 test: 22 pass, 1 skip có chủ đích)
- `EventTests`: double-book throw, hold ghế trống → Held, không ảnh hưởng ghế khác,
  ghế không tồn tại → throw, tên ghế rỗng → throw, constructor (tên trùng, tên
  khoảng trắng, không có ghế, tên event rỗng) → throw. Test race
  `HoldSeat_Concurrent_OnlyOneShouldSucceed` đang **Skip** (chờ Phase 2).
- `BookingTests`: new = SeatHeld, guard constructor, Confirm/Expire happy path,
  Confirm/Expire sai trạng thái → throw.
- `BookingServiceTests`: event không tồn tại → throw, tạo thành công (ghế Held +
  booking được lưu), đặt cùng ghế 2 lần → throw.

### Nợ đã biết (để lại cho phase sau)
- **Race ở `Event.HoldSeat`** (check-then-act không nguyên tử) + `UpdateAsync`
  in-memory là no-op nên che lỗi lost update → Phase 2.
- **Vòng đời Seat ↔ Booking chưa nối:** `Confirm()` không làm ghế `Booked`,
  `Expire()` không nhả ghế; `Seat.Book()/Release()` chưa ai gọi, chưa có guard →
  Phase 2 / Phase 4.
- **`BookingService.CreateAsync` ghi 2 aggregate không nguyên tử** → Phase 4 (Saga).

## 6. Lộ trình các phase còn lại (bài tập, chưa làm)

- **Phase 2 — Redis giữ ghế bằng TTL (ĐANG LÀM, thiết kế đã chốt — xem 6.1):** giữ
  ghế tạm trong Redis (TTL ~5 phút), hết giờ tự nhả; chống double-book ở tầng phân
  tán. (Đã học qua "Demo 5" cache-aside.)
- **Phase 3 — Messaging:** phát event `BookingCreated`, thêm `Payment.Worker` consume
  (MassTransit + RabbitMQ). (Đã học "Demo 6" + MassTransit.)
- **Phase 4 — Saga:** state machine vòng đời booking `SeatHeld → Confirmed / Expired`,
  timeout nhả ghế (MassTransit State Machine). (Đã học Saga.)
- **Phase 5 — OpenTelemetry:** distributed tracing xuyên service, export OTLP. (Đã
  học "Demo 8".)
- **Phase 6 (tùy chọn) — Docker Compose:** gói các service + Redis + RabbitMQ.

Mỗi phase phải **chạy được + có test** trước khi sang phase sau.

### 6.1. Thiết kế Phase 2 (đã chốt 2026-10-02)

**Phạm vi:** CHỈ giữ ghế. Không làm `Confirm()`/`Expire()`/chuyển ghế sang `Booked`
(để Phase 3 và Phase 4).

**Quyết định cốt lõi — Redis là nguồn duy nhất của trạng thái "đang giữ":**
- `Event` chỉ còn trạng thái lâu dài `Available` / `Booked`. `Held` KHÔNG nằm trong
  aggregate nữa (bỏ khỏi `ESeatStatus`, hoặc chỉ là giá trị tính ra khi dựng response).
- Lý do: nếu cả Redis lẫn `Seat.Status` cùng ghi `Held`, khi TTL hết Redis quên hold
  nhưng aggregate vẫn `Held` → ghế bị khóa vĩnh viễn (hai nguồn lệch nhau).

**Thay đổi so với Phase 1:**
- `Event.HoldSeat` → method chỉ kiểm tra, vd `EnsureSeatCanBeHeld(seatName)`: ghế
  không tồn tại → `SeatNotFoundException`; ghế đã `Booked` →
  `SeatNotAvailableException`. Không đổi trạng thái.
- `BookingService.CreateAsync` bỏ `UpdateAsync` (Event không bị sửa).
- `GET /events`: ghế đang giữ hiện `Available` trừ khi gộp thêm hold từ Redis.
- Test Phase 1 sẽ vỡ, cần viết lại/chuyển chỗ: `HoldSeat_AvailableSeat_ShouldBecomeHeld`,
  `HoldSeat_AlreadyHeld_ShouldThrow`, `HoldSeat_ShouldNotAffectOtherSeats`, test race
  đang Skip (`EventTests`); assert ghế `Held` và "đặt cùng ghế 2 lần"
  (`BookingServiceTests`).

**Redis:**
- Giữ ghế = `SET key value NX EX <giây>` (nguyên tử — đây là thứ sửa race). Trong
  StackExchange.Redis: `StringSetAsync` với `When.NotExists` + expiry, trả `bool`.
- Key: `seat-hold:{eventId}:{seatName}`. Value: `booking.Id` (mã người giữ).
- Nhả ghế phải là **so sánh rồi xóa** (không `DEL` thường, tránh xóa nhầm hold của
  người khác). Cần tra: script Lua, hoặc `LockTakeAsync`/`LockReleaseAsync`.

**Phân tầng:**
- Application: interface `ISeatHoldService` —
  `TryHoldAsync(eventId, seatName, holderId, ttl) : Task<bool>`,
  `ReleaseAsync(eventId, seatName, holderId) : Task<bool>`,
  `GetHolderAsync(eventId, seatName) : Task<Guid?>`.
- Infrastructure: `RedisSeatHoldService` (gói StackExchange.Redis CHỈ tham chiếu ở
  đây) + `InMemorySeatHoldService` (`ConcurrentDictionary.TryAdd` + thời điểm hết
  hạn) để unit test / chạy không cần Redis.
- Api: `IConnectionMultiplexer` singleton; chuỗi kết nối Redis + thời gian giữ ghế
  đọc từ `appsettings.json` (mặc định 5 phút; thử tay ~30 giây).

**Luồng `CreateAsync` mới:**
1. Lấy Event → không có: `EventNotFoundException`.
2. `evt.EnsureSeatCanBeHeld(seatName)`.
3. Tạo `Booking` trong bộ nhớ (để có `booking.Id`).
4. `TryHoldAsync(..., booking.Id, ttl)` → `false`: `SeatNotAvailableException`.
5. Lưu `Booking` → nếu lỗi: `ReleaseAsync` rồi ném lại (TTL là lưới an toàn cuối).
6. Trả `Booking`.

**`Booking`:** thêm `HoldExpiresAt`. Nợ còn lại: TTL hết thì Redis xóa key nhưng
`Booking` vẫn `SeatHeld` (chưa ai gọi `Expire()`) → Phase 4.

**Test:**
- Unit test `BookingService` với `InMemorySeatHoldService`: giữ thành công, ghế đang
  bị giữ → throw, lưu booking lỗi → ghế được nhả.
- Test hợp đồng `ISeatHoldService` chạy trên CẢ HAI implementation: 100 `TryHoldAsync`
  song song → đúng 1 `true` (test race cũ chuyển về đây, bỏ Skip); giữ lại được sau
  khi TTL hết; `ReleaseAsync` sai `holderId` không xóa được hold.
- Test Redis thật: project riêng hoặc trait, để `dotnet test` vẫn xanh khi không có
  Docker (tra `Testcontainers.Redis`; máy dev đã có container `redis` cổng 6379).
- Test TTL: thời gian giữ rất ngắn với Redis thật; bản in-memory dùng
  `TimeProvider`/`FakeTimeProvider`.

**Thứ tự làm:**
1. Domain: `EnsureSeatCanBeHeld`, bỏ `Held` khỏi aggregate, sửa `EventTests`.
2. `ISeatHoldService` + `InMemorySeatHoldService` + test hợp đồng.
3. Sửa `BookingService.CreateAsync` + `BookingServiceTests`.
4. `RedisSeatHoldService` + cấu hình Api + chạy test hợp đồng với Redis thật.
5. Thử tay: đặt A1 → 201; đặt lại → 409; chờ hết TTL; đặt lại → 201.

## 7. Quy ước / bài học đã rút ra (để không lặp lỗi)

- `Guid.NewGuid()` (sinh mới) KHÁC `new Guid()` (Guid rỗng) — luôn dùng NewGuid.
- Guard trong aggregate phải **đúng chiều** (`if Status != Hợp lệ throw`) VÀ phải
  **ném lỗi** khi vi phạm (không "nuốt im lặng").
- Lỗi nghiệp vụ dùng **exception riêng của domain** (`...NotFoundException` → 404,
  `SeatNotAvailableException` → 409). KHÔNG catch exception chung của BCL
  (`KeyNotFoundException`, `InvalidOperationException`) ở API vì framework cũng ném
  chúng → bug kỹ thuật bị dịch nhầm thành lỗi nghiệp vụ. Input sai → `ArgumentException`
  ở Domain, 400 ở API.
- Guard trong constructor đặt **trước khi gán**; quy tắc rỗng/khoảng trắng phải
  nhất quán giữa constructor và method (cùng dùng `IsNullOrWhiteSpace`).
- Test phải có `Assert` kiểm đúng điều tên nó tuyên bố ("ShouldBe..." dùng
  `Assert.Equal`, "ShouldThrow" dùng `Assert.Throws`).
- Repo in-memory: dùng `Task.FromResult`, không `async` thừa.
- Entity: đóng gói (`private set`), đổi trạng thái qua method có kiểm soát.
- Luôn `dotnet build` để tự bắt lỗi biên dịch trước khi nhờ review.

## 8. Lệnh hay dùng

```powershell
cd E:\Dev\TicketBooking
dotnet build          # bắt lỗi biên dịch
dotnet test           # chạy 23 unit test (22 pass, 1 skip)
dotnet run --project TicketBooking.Api   # chạy API
```
Test API: `GET /events` lấy eventId → `POST /bookings {EventId, SeatName:"A1"}` →
đặt lại "A1" phải ra **409** (invariant chặn double-book); ghế không tồn tại → **404**;
`SeatName` rỗng → **400**.

Repo GitHub: https://github.com/tinhbui008/TicketBooking
