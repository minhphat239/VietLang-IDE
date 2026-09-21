# String Plus Library — SPEC v1.0

## Tổng quan
Thư viện nâng cao xử lý chuỗi cho VietLang.

## Danh sách functions

### 1. `chuyển_thường(text)` → `string`
- **Mô tả:** Chuyển chuỗi thành chữ thường.
- **Tham số:** `text` (string) — chuỗi đầu vào.
- **Return:** `string` — chuỗi đã chuyển thành chữ thường.
- **Ví dụ:** `chuyển_thường("HELLO")` → `"hello"`
- **Lỗi hay gặp:** Không có (chuỗi rỗng trả về rỗng).

### 2. `chuyển_hoa(text)` → `string`
- **Mô tả:** Chuyển chuỗi thành chữ hoa.
- **Tham số:** `text` (string) — chuỗi đầu vào.
- **Return:** `string` — chuỗi đã chuyển thành chữ hoa.
- **Ví dụ:** `chuyển_hoa("hello")` → `"HELLO"`
- **Lỗi hay gặp:** Không có.

### 3. `cắt(text)` → `string`
- **Mô tả:** Loại bỏ khoảng trắng đầu và cuối chuỗi.
- **Tham số:** `text` (string) — chuỗi đầu vào.
- **Return:** `string` — chuỗi đã loại bỏ khoảng trắng.
- **Ví dụ:** `cắt("  hello  ")` → `"hello"`
- **Lỗi hay gặp:** Không có.

### 4. `thay_thế(text, cũ, mới)` → `string`
- **Mô tả:** Thay thế tất cả occurrences của `cũ` bằng `mới`.
- **Tham số:** `text` (string), `cũ` (string), `mới` (string).
- **Return:** `string` — chuỗi sau khi thay thế.
- **Ví dụ:** `thay_thế("hello world", "world", "VietLang")` → `"hello VietLang"`
- **Lỗi hay gặp:** Nếu `cũ` không tìm thấy → trả về chuỗi gốc.

### 5. `tách(text, phân_cách)` → `array`
- **Mô tả:** Tách chuỗi thành mảng dựa trên delimiter.
- **Tham số:** `text` (string), `phân_cách` (string).
- **Return:** `array` — mảng các chuỗi con.
- **Ví dụ:** `tách("a,b,c", ",")` → `["a", "b", "c"]`
- **Lỗi hay gặp:** Nếu delimiter không có trong chuỗi → mảng 1 phần tử.

### 6. `gộp(mảng, phân_cách)` → `string`
- **Mô tả:** Gộp các phần tử mảng thành chuỗi, phân cách bởi `phân_cách`.
- **Tham số:** `mảng` (array), `phân_cách` (string).
- **Return:** `string` — chuỗi đã gộp.
- **Ví dụ:** `gộp(["a", "b", "c"], ",")` → `"a,b,c"`
- **Lỗi hay gặp:** Nếu mảng rỗng → trả về chuỗi rỗng.

### 7. `chứa(text, chuỗi_con)` → `bool`
- **Mô tả:** Kiểm tra chuỗi có chứa `chuỗi_con` hay không.
- **Tham số:** `text` (string), `chuỗi_con` (string).
- **Return:** `bool` — `đúng` nếu chứa, `sai` nếu không.
- **Ví dụ:** `chứa("hello world", "world")` → `đúng`
- **Lỗi hay gặp:** Phân biệt hoa/thường.

### 8. `bắt_đầu(text, tiền_tố)` → `bool`
- **Mô tả:** Kiểm tra chuỗi có bắt đầu bởi `tiền_tố` hay không.
- **Tham số:** `text` (string), `tiền_tố` (string).
- **Return:** `bool` — `đúng` nếu bắt đầu bởi tiền tố, `sai` nếu không.
- **Ví dụ:** `bắt_đầu("hello", "hel")` → `đúng`
- **Lỗi hay gặp:** Phân biệt hoa/thường.

### 9. `kết_thúc(text, hậu_tố)` → `bool`
- **Mô tả:** Kiểm tra chuỗi có kết thúc bởi `hậu_tố` hay không.
- **Tham số:** `text` (string), `hậu_tố` (string).
- **Return:** `bool` — `đúng` nếu kết thúc bởi hậu tố, `sai` nếu không.
- **Ví dụ:** `kết_thúc("hello", "llo")` → `đúng`
- **Lỗi hay gặp:** Phân biệt hoa/thường.

### 10. `lặp(text, số_lần)` → `string`
- **Mô tả:** Lặp chuỗi `số_lần` lần.
- **Tham số:** `text` (string), `số_lần` (number).
- **Return:** `string` — chuỗi đã lặp.
- **Ví dụ:** `lặp("ab", 3)` → `"ababab"`
- **Lỗi hay gặp:** Nếu `số_lần` = 0 → trả về chuỗi rỗng.

### 11. `đảo(text)` → `string`
- **Mô tả:** Đảo ngược chuỗi.
- **Tham số:** `text` (string) — chuỗi đầu vào.
- **Return:** `string` — chuỗi đã đảo ngược.
- **Ví dụ:** `đảo("hello")` → `"olleh"`
- **Lỗi hay gặp:** Không có.

### 12. `lấy(text, bắt_đầu, độ_dài)` → `string`
- **Mô tả:** Lấy substring từ vị trí `bắt_đầu` với độ dài `độ_dài`.
- **Tham số:** `text` (string), `bắt_đầu` (number), `độ_dài` (number).
- **Return:** `string` — chuỗi con.
- **Ví dụ:** `lấy("hello", 1, 3)` → `"ell"`
- **Lỗi hay gặp:** Nếu `bắt_đầu` ngoài phạm vi → trả về chuỗi rỗng.

### 13. `tìm_tất_cả(text, mẫu)` → `array`
- **Mô tả:** Tìm tất cả vị trí (index) của `mẫu` trong chuỗi.
- **Tham số:** `text` (string), `mẫu` (string).
- **Return:** `array` — mảng các vị trí index.
- **Ví dụ:** `tìm_tất_cả("ababab", "ab")` → `[0, 2, 4]`
- **Lỗi hay gặp:** Nếu không tìm thấy → mảng rỗng.

### 14. `đếm(text, chuỗi_con)` → `number`
- **Mô tả:** Đếm số lần xuất hiện của `chuỗi_con` trong chuỗi.
- **Tham số:** `text` (string), `chuỗi_con` (string).
- **Return:** `number` — số lần xuất hiện.
- **Ví dụ:** `đếm("ababab", "ab")` → `3`
- **Lỗi hay gặp:** Nếu không tìm thấy → trả về 0.

### 15. `hợp_lệ(text, mẫu)` → `bool`
- **Mô tả:** Kiểm tra chuỗi có khớp với mẫu regex `mẫu` hay không.
- **Tham số:** `text` (string), `mẫu` (string) — regex pattern.
- **Return:** `bool` — `đúng` nếu khớp, `sai` nếu không.
- **Ví dụ:** `hợp_lệ("123", "\\d+")` → `đúng`
- **Lỗi hay gặp:** Mẫu regex không hợp lệ → lỗi runtime.
