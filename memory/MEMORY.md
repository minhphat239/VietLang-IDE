# MEMORY — VietLang (manager)

## TIEN DO
- 2026-09-20: Khởi tạo dự án tại D:\Documents\VietLang (thư mục rỗng, greenfield). Chốt kiến trúc: C#/.NET 8 · MVP interpreter phiên này · từ khóa bắt buộc có dấu · NLP ưu tiên = thư viện chuẩn tiếng Việt. Bắt đầu M1, giao T1.
- 2026-09-20: T1 + T1.1 xong (verify lại: build exit 0, 14/14 PASS). Đang giao T2 (Parser). T2/T3 phải đọc src/Token.cs + src/AST.cs + docs/SPEC_v0_1.md trước khi code (contract đã có trong code + spec).
- 2026-09-20: T2 + 2.1 + 2.2 xong (30/30 PASS). Đang giao T3 (Interpreter). Quyết định runtime đã chốt trong brief T3 (xem tracker). Sau T3 còn T4 (demo+README) rồi T5 verify tổng.
- 2026-09-20: T3 + T2.3 xong — 45/45 + chạy .vl thật (if-chain nhiều dòng, lớp, mảng) OK. MVP đã chạy được. Giao T4 (demo + README).
- 2026-09-20: **M1 DONE.** T4 + T4.1 + T5 xong: 45/45 test, demo exit 0, README 9 mục, SPEC v0.1, 0 sentinel. VietLang v0.1 chạy được: hàm/lớp/this/mảng/if-chain nhiều dòng + 6 builtin + gợi ý chính tả từ khóa. Đề mục tiếp theo: M2 → M3 (thư viện chuẩn tiếng Việt — ưu tiên NLP).
- 2026-09-20: **M2 DONE.** T6-T10 (M2.1-M2.5 + rename): 96/96 test. Dict (CRUD + 5 method), exception (try/catch/finally/ném), module (khai_báo), +=, method chuỗi/tìm/thay/cắt/chứa/phân_tách + mảng/lọc/map/gộp, golden tests (4 file .vl), CHANGELOG, version v0.2. Đã đổi `nhập`→`khai_báo`. Đã thêm `+=` operator. Đã thêm ColonToken `:` cho dict. Nền tảng đủ để mở project NLP.

## RUT KINH NGHIEM (quy trình)
- Lỗi: worker T1 lex literal âm (`-3` thành 1 token) → vỡ phép trừ nhị phân `5-3`. Bài học: với thiết kế ngôn ngữ, `-` phải LUÔN là toán tử; số âm để parser dựng UnaryExpr — lexer đừng gộp dấu vào số. Áp dụng: đã fix T1.1 + test regression `tru_nhi_phan`; nhắc T2/T3 không đưa literal âm trở lại.
- Bài học: relai theo dõi worker tự khai "lệch contract" — tôi đã đọc Lexer.cs + chạy lại build/test thay vì tin report → phát hiện đúng bug nguồn trước khi nó lan sang parser. Áp dụng: luôn verify phần worker tự khai là vùng rủi ro.
- Bài học thứ 3 (T3→T2.3): worker T3 tự khai "if-chain chỉ cùng dòng" — tôi quyết định sửa parser (T2.3) thay vì chấp nhận constraint, vì UX viết if/else xuống dòng là chuẩn người Việt. Áp dụng: với vấn đề "cảm giác ngôn ngữ", ưu tiên sửa nguồn, đừng để constraint rẻ hơn chất lượng.
- Bài học verify tổng: kiểm memory worker bằng regex QUÁ CHẶT suýt báo thiếu mục (Memory_T4 tiêu đề mục 3 viết dài hơn chuẩn) → đọc file thật trước khi kết luận; regex chỉ để lọc, file thật mới là chân lý. Áp dụng: khi 1/4 thiếu → đọc nguyên file, không chốt vội.

## RUT KINH NGHIEM (UI/UX)
- MVP không có giao diện đồ họa (backend thuần). "UI/UX" của ngôn ngữ = thông báo lỗi tiếng Việt rõ ràng + kèm dòng + gợi ý sửa khi gõ từ khóa thiếu dấu (dân gõ telex quen tay). Luôn nhắc T1+.
- Tính năng "cảm giác ngôn ngữ" (if-chain nhiều dòng) quan trọng hơn chi li kỹ thuật — user viết code tiếng Việt sẽ viết kiểu đọc tự nhiên. Cần giữ tư duy này ở M2/M3.

## TU CHAM DIEM
- M1 (quản lý 9 task + 5 fix nhỏ, 45/45 test, demo chạy chuẩn):
  - DIEM TOT: quyết định kiến trúc hỏi đúng 1 lượt trước khi làm; brief 10 khối + spec chốt trong tracker khiến worker tự chủ; verify lại TỪNG task bằng lệnh thật (build/test/file) trước khi đánh DONE; phát hiện + sửa nguồn 2 lỗi thiết kế trước khi lan; tài liệu đối chiếu hành vi thật (T4.1).
  - DIEM XAU: tracker bị dòng trùng lặp 1 lần (self-fix); memory worker kiểm bằng regex quá chặt suýt kết luận sai (đã sửa bằng đọc file thật); còn CHƯA VERIFY: hiển thị UTF-8 trên console cp437, `nhập()` end-to-end, hành vi `;` rỗng.
  - Scorecard: 50/55 (trừ 5: tracker dính duplicate 1 lần, regex memory suýt sai 1 lần, chưa verify console/cp437, `nhập()` chưa end-to-end, chưa chạy `nhập()` demo).
  - GREEN: 12/12 (file-first, verify máy móc, checkpoint mỗi task, PARTIAL chưa dùng nhưng brief có sẵn, memory đủ 4 mục, báo cáo 7 dòng, retry theo bậc, token tiết kiệm vì brief gọn + resume session cho fix nhỏ, không tự viết code hộ, không nhồi RULES vào brief, không bịa số liệu, escalate chưa cần).
  - RED dính: không (chưa lặp lỗi lần 2).
  - CHUA VERIFY: UTF-8 console ngoài cp1252/cp65001; `nhập()`; `;` rỗng; build trên Linux/macOS.
  - SOI LAI: lần sau khi giao chuỗi task phụ thuộc, viết đủ 1 dòng spec vào tracker NGAY từ đầu (đã làm) — và khi sửa tracker phải đọc lại cả section trước khi edit (tránh duplicate). Regex verify = công cụ lọc, không phải kết luận.