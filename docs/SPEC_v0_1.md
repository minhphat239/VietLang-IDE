# VietLang — Đặc tả ngôn ngữ v0.2 (CONTRACT cho T2/T3)

> Tài liệu này là CONTRACT của milestone M1 (MVP interpreter). T2 (Parser) và T3 (Interpreter + Builtins) BẮT BUỘC bám theo. Không tự thêm/cắt tính năng. Mọi khác biệt với tài liệu này phải được chốt lại trong task.

## 1. Tổng quan

- Ngôn ngữ lập trình tiếng Việt, dynamic typing, cú pháp giống Python nhưng khối lệnh dùng `{}` thay vì thụt lề.
- Từ khóa BẮT BUỘC có dấu tiếng Việt.
- Lệnh kết thúc bằng `newline` hoặc `;` (tùy chọn).
- Comment: `# ...` đến hết dòng (khoảng trắng/wrapping, không sinh token).
- Chuỗi: `"..."` hoặc `'...'`; escape `\n \t \\ \" \'`.
- Số: số nguyên + số thập phân (`4.5`). Dấu trừ LUÔN là toán tử `TRU`; KHÔNG có literal âm ở tầng lexer — số âm là `UnaryExpr("TRU", NumLit)` do parser dựng khi gặp `-` ở vị trí biểu thức (VD `x = -3` → token `=`, `TRU`, `3`; `5-3` → `5`, `TRU`, `3`).

## 2. Từ khóa & nghĩa

| Từ khóa | TokenKind | Nghĩa |
|---|---|---|
| `hàm` | HAM | định nghĩa hàm (func/def) |
| `lớp` | LOP | định nghĩa lớp (class) |
| `trả_về` | TRA_VE | trả về (return) |
| `nếu` | NEU | điều kiện (if) |
| `còn_nếu` | CON_NEU | nhánh "else if" |
| `không_thì` | KHONG_THI | nhánh ngược lại (else) |
| `trong_lúc` | TRONG_LUC | vòng lặp (while) |
| `với` | VOI | vòng lặp (for) |
| `trong` | TRONG | thành phần của (in) |
| `dừng` | DUNG_BREAK | thoát vòng lặp (break) |
| `tiếp` | TIEP | sang lượt tiếp (continue) |
| `và` | VA | và (and) |
| `hoặc` | HOAC | hoặc (or) |
| `không` | KHONG | phủ định (not) |
| `đúng` | DUNG_TRUE | true |
| `sai` | SAI | false |
| `rỗng` | RONG | null |
| `this` | THIS | đối tượng hiện tại (this) |
| `chuẩn_hóa` | CHUAN_HOA | chuẩn_hóa chuỗi (normalize) |
| `tìm_từ` | TIM_TU | tìm vị trí từ trong chuỗi (find_word) |
| `tách_từ` | TACH_TU | tách chuỗi thành mảng từ (word_split) |
| `tách_câu` | TACH_CAU | tách chuỗi thành mảng câu (sentence_split) |
| `đếm_từ` | DEM_TU | đếm số từ trong chuỗi (count_words) |
| `chuẩn_hóa_tìm_kiếm` | CHUAN_HOA_TIM_KIEM | chuẩn_hóa cho tìm kiếm (normalize_for_search) |

Lưu ý: `ham`, `lop`, `va`, `neu`... (thiếu dấu) KHÔNG phải từ khóa — chúng là identifier hợp lệ. Gợi ý thiếu dấu chỉ xuất hiện trong THÔNG BÁO LỖI (tầng parse/runtime), ví dụ khi gặp `ham` ở vị trí mong đợi từ khóa: `Bạn có định viết 'hàm'` (tham chiếu bảng `Lexer.SuggestionTable` / `Lexer.GoiYThieuDau`).

## 3. Token

Mỗi token: `Kind`, `Lexeme` (chuỗi gốc), `Value` (chỉ literal: số=double, chuỗi=string), `Line`, `Col` (1-based).

- **SO**: số (giá trị lưu `double`, luôn parse với `InvariantCulture`).
- **CHUOI**: chuỗi có escape; giá trị sau khi giải escape.
- **TEN**: identifier — bắt đầu bằng chữ cái (không dấu hoặc có dấu tiếng Việt) hoặc `_`; tiếp theo chữ/số/`_`.
- **Từ khóa**: 24 loại riêng (bảng mục 2), khớp CHÍNH XÁC có dấu.
- **TOANTU**: `= == != < > <= >= + - * / %` (mỗi loại một TokenKind).
- **DAU**: `{ } ( ) [ ] , .` và `;` (TokenKind.SOL_SEMI — dấu kết thúc lệnh tùy chọn; parser quyết định cách dùng).
- **NEWLINE**: phát cho `\n` (mỗi `\r?\n` cho ĐÚNG 1 NEWLINE; `\r` của CRLF là khoảng trắng).
  - **KHÔNG phát NEWLINE khi độ sâu `(`/`[` > 0** (cho phép biểu thức nhiều dòng trong ngoặc — giống Python).
  - **Trong khối `{}` vẫn phát NEWLINE bình thường** (đây là dấu kết thúc câu lệnh trong khối). *(Quyết định triển khai: contract test yêu cầu count NEWLINE trong `{}` — chi tiết memory/T1.)*
  - Nhiều newline liên tiếp → phát đủ (parser tự bỏ qua newline thừa).
- **EOF**: 1 token cuối cùng (parse dừng khi gặp).

## 4. Cú pháp lệnh

```
<lệnh>         := <lệnh khai báo biến> | <gán> | <biểu thức> | <lệnh điều khiển>
Lệnh kết thúc    bằng NEWLINE hoặc `;` (tùy chọn). Khối lệnh nằm trong `{}`.
```

- Khai báo biến: `<tên> = <biểu thức>` (dynamic typing, không từ khóa khai báo).
- Gán: `<biểu thức trái> = <giá trị>` (target có thể là biến, `a.b`, `a[i]`).
- Biểu thức: số, chuỗi, mảng `[1, 2, 3]`, `đúng`/`sai`/`rỗng`, biến, `this`,
  toán tử `+ - * / %` và so sánh `== != < > <= >=`, logic `và hoặc không`,
  gọi hàm `f(a, b)`, truy cập thành viên `a.b`, truy cập chỉ mục `a[i]`.
  Số âm (`-3`) = `UnaryExpr("TRU", <toán hạng>)` do parser dựng (tầng lexer chỉ phát token `TRU`).
- Điều khiển:
  - `nếu <đk> { ... } còn_nếu <đk> { ... } không_thì { ... }`
  - `trong_lúc <đk> { ... }`
  - `với <biến> trong <danh sách> { ... }`
  - `dừng` / `tiếp` trong vòng lặp; `trả_về <giá trị?>` trong hàm.
- Hàm: `hàm <tên>(<tham số...>) { <lệnh> }`
- Lớp: `lớp <tên> { hàm <tên_khác_this>(...) { ... } }` (method đầu tiên...— cú pháp chi tiết do T2 chốt theo các token có sẵn).
- NEWLINE thừa giữa các lệnh → parser bỏ qua. NEWLINE thiếu/`;` thiếu giữa 2 lệnh → lỗi cú pháp.

## 5. Kiểu dữ liệu runtime (dynamic)

`Số (double) · Chuỗi (string) · Mảng (list) · Đúng/Sai (bool) · Rỗng (null) · Hàm · Lớp · Đối tượng (instance của lớp)`

- `+`: số+cộng, chuỗi=nối. Các phép khác: số. So sánh/logic: như thường; `đúng`/`sai` là truthy phủ định.
- Mảng: chỉ mục từ 0. `độ_dài` trên mảng/chuỗi.
- Phạm vi v0.1: KHÔNG có dict/object ghép cặp key, KHÔNG có exception (`thử`/`ngoại_lệ`), KHÔNG có module (`khai_báo`) — thuộc M2.

## 6. Builtins (v0.1)

| Tên | Nghĩa |
|---|---|
| `in_ra(...)` | in ra màn hình |
| `độ_dài(x)` | độ dài chuỗi/mảng |
| `chuyển_chuỗi(x)` | ép về chuỗi |
| `chuyển_số(x)` | ép về số |
| `nhập()` | đọc dòng từ bàn phím |
| `thêm(mảng, phần_tử)` | thêm phần tử vào mảng |
| `chuẩn_hóa(text)` | bỏ dấu tiếng Việt, giữ nguyên chữ |
| `tìm_từ(text, từ)` | mảng vị trí index của `trong` |
| `tách_từ(text)` | mảng từ bằng whitespace |
| `tách_câu(text)` | mảng câu (giữ delimiter `.?!`) |
| `đếm_từ(text)` | số từ |
| `chuẩn_hóa_tìm_kiếm(text)` | bỏ dấu + lowercase + trim |

## 7. Quy tắc lỗi

Mọi thông báo lỗi bằng tiếng Việt, kèm số dòng (và cột nếu có).

- **Lỗi từ vựng** (lexer): `Lỗi từ vựng dòng <N>: ký tự '<c>' không hợp lệ`. Chuỗi chưa kết thúc / escape lạ cũng là lỗi từ vựng.
- **Lỗi cú pháp** (parser): chỉ rõ token gây lỗi + dòng/cột + cụm từ khóa/lệnh mong đợi.
- **Gợi ý thiếu dấu**: khi gặp identifier trùng (không dấu) với từ khóa tại vị trí mong đợi từ khóa → thêm gợi ý, VD `ham` → `Bạn có định viết 'hàm'?`; `dung` → `'đúng' hoặc 'dừng'?` (dùng `Lexer.GoiYThieuDau`).
- **Lỗi runtime** (interpreter): tên lỗi + dòng + gợi ý nếu biết (VD gọi `ham()` khi `ham` chưa được định nghĩa → nhắc `hàm`).