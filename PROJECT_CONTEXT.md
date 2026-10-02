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
- Chưa dùng Redis/RabbitMQ/EF Core trong code (sẽ thêm ở các phase sau).

## 4. Kiến trúc — Clean Architecture 4 tầng

```
TicketBooking.Api            →  Application + Infrastructure   (Minimal API, HTTP)
TicketBooking.Infrastructure →  Application                    (repo in-memory)
TicketBooking.Application     →  Domain                         (use case, interface)
TicketBooking.Domain         →  (không phụ thuộc ai)            (entity + luật nghiệp vụ)
TicketBooking.Tests          →  Domain                         (unit test xUnit)
```
Quy tắc vàng: **mọi phụ thuộc hướng vào trong. Domain độc lập.**

## 5. Trạng thái hiện tại — PHASE 1 ĐÃ XONG

### Domain (hoàn chỉnh, có test)
- `ESeatStatus` enum: `Available`, `Held`, `Booked`.
- `Seat`: `Id` (Guid, tự sinh), `Name` (read-only, vd "A5"), `Status` (private set).
  Method đổi trạng thái `Hold()/Book()/Release()` là **`internal`** (chỉ Event gọi,
  chặn thế giới ngoài lách cửa). Constructor tự đặt `Available`.
- `Event` (**aggregate root**): `Id`, `Name`, `_seats` (private List) + `Seats`
  (IReadOnlyCollection, chỉ đọc). Method **`HoldSeat(seatName)`** = cửa duy nhất giữ ghế:
  tìm ghế → không có thì `throw KeyNotFoundException` → không `Available` thì
  `throw InvalidOperationException` (INVARIANT double-book) → `seat.Hold()`.
- `EBookingStatus` enum: `SeatHeld`, `Confirmed`, `Expired` (và `Cancelled`).
- `Booking` (aggregate): `Id`, `EventId`, `SeatName`, `Status` (private set).
  Constructor đặt `SeatHeld`. Method `Confirm()` và `Expire()` có **guard**
  (`if Status != SeatHeld throw InvalidOperationException`).

### Application (hoàn chỉnh)
- `IEventRepository`: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`.
- `IBookingRepository`: `GetByIdAsync`, `AddAsync`.
- `BookingService.Create(eventId, seatName)` — use case điều phối 2 aggregate:
  lấy Event → `HoldSeat` → `UpdateAsync` → tạo `Booking` → `AddAsync` → trả về.
  Chỉ điều phối, KHÔNG lặp lại logic nghiệp vụ (nằm trong Domain).

### Infrastructure (hoàn chỉnh)
- `InMemoryEventRepository`, `InMemoryBookingRepository`: Dictionary theo Id, dùng
  `Task.FromResult` (không async-no-await). EventRepo **seed sẵn** 1 event
  "Concert-01" với ghế A1–A5.

### Api (hoàn chỉnh)
- DI: repository `AddSingleton` (giữ state in-memory), `BookingService` `AddScoped`.
- `GET /events` — liệt kê event + ghế (để lấy eventId).
- `POST /bookings` — body `{ EventId, SeatName }`; try/catch map
  `KeyNotFoundException → 404`, `InvalidOperationException → 409`, thành công → 201.
- `GET /bookings/{id:guid}` — trả booking hoặc 404.
- Record `CreateBookingRequest(Guid EventId, string SeatName)`.

### Test (7 test, tất cả pass)
Event: double-book throw, hold ghế trống → Held, ghế không tồn tại → throw.
Booking: new = SeatHeld, Confirm → Confirmed, Confirm 2 lần → throw,
Expire khi Confirmed → throw.

## 6. Lộ trình các phase còn lại (bài tập, chưa làm)

- **Phase 2 — Redis giữ ghế bằng TTL:** giữ ghế tạm trong Redis (TTL ~5 phút), hết
  giờ tự nhả; chống double-book ở tầng phân tán. (Đã học qua "Demo 5" cache-aside.)
- **Phase 3 — Messaging:** phát event `BookingCreated`, thêm `Payment.Worker` consume
  (MassTransit + RabbitMQ). (Đã học "Demo 6" + MassTransit.)
- **Phase 4 — Saga:** state machine vòng đời booking `SeatHeld → Confirmed / Expired`,
  timeout nhả ghế (MassTransit State Machine). (Đã học Saga.)
- **Phase 5 — OpenTelemetry:** distributed tracing xuyên service, export OTLP. (Đã
  học "Demo 8".)
- **Phase 6 (tùy chọn) — Docker Compose:** gói các service + Redis + RabbitMQ.

Mỗi phase phải **chạy được + có test** trước khi sang phase sau.

## 7. Quy ước / bài học đã rút ra (để không lặp lỗi)

- `Guid.NewGuid()` (sinh mới) KHÁC `new Guid()` (Guid rỗng) — luôn dùng NewGuid.
- Guard trong aggregate phải **đúng chiều** (`if Status != Hợp lệ throw`) VÀ phải
  **ném lỗi** khi vi phạm (không "nuốt im lặng").
- Chọn đúng loại exception theo ngữ nghĩa: `KeyNotFoundException` (không tồn tại) vs
  `InvalidOperationException` (vi phạm trạng thái) → map sang 404 vs 409 ở API.
- Test phải có `Assert` kiểm đúng điều tên nó tuyên bố ("ShouldBe..." dùng
  `Assert.Equal`, "ShouldThrow" dùng `Assert.Throws`).
- Repo in-memory: dùng `Task.FromResult`, không `async` thừa.
- Entity: đóng gói (`private set`), đổi trạng thái qua method có kiểm soát.
- Luôn `dotnet build` để tự bắt lỗi biên dịch trước khi nhờ review.

## 8. Lệnh hay dùng

```powershell
cd E:\Dev\TicketBooking
dotnet build          # bắt lỗi biên dịch
dotnet test           # chạy 7 unit test
dotnet run --project TicketBooking.Api   # chạy API
```
Test API: `GET /events` lấy eventId → `POST /bookings {EventId, SeatName:"A1"}` →
đặt lại "A1" phải ra **409** (invariant chặn double-book).

Repo GitHub: https://github.com/tinhbui008/TicketBooking
