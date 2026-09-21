# DateTime Plus Library — SPEC v1.0

## Danh sách functions

### 1. `phan_tach(date_string)` → dict
- Phân tách chuỗi ngày thành dict
- Ví dụ: `phan_tach("2026-09-21")` → `{"nam": 2026, "thang": 9, "ngay": 21}`

### 2. `dinh_dang(date, format)` → string
- Định dạng ngày theo format
- Ví dụ: `dinh_dang(bay_gio(), "dd/MM/yyyy")` → `"21/09/2026"`

### 3. `them_ngay(date, days)` → string
- Thêm số ngày
- Ví dụ: `them_ngay("2026-09-21", 7)` → `"2026-09-28"`

### 4. `them_gio(date, hours)` → string
- Thêm số giờ
- Ví dụ: `them_gio("2026-09-21T10:00:00", 2)` → `"2026-09-21T12:00:00"`

### 5. `hieu_ngay(date1, date2)` → number
- Hiệu giữa 2 ngày (ngày)
- Ví dụ: `hieu_ngay("2026-09-21", "2026-09-01")` → `20`

### 6. `hieu_gio(date1, date2)` → number
- Hiệu giữa 2 ngày (giờ)
- Ví dụ: `hieu_gio("2026-09-21T12:00:00", "2026-09-21T10:00:00")` → `2`

### 7. `truoc(date1, date2)` → bool
- Kiểm tra date1 trước date2
- Ví dụ: `truoc("2026-09-01", "2026-09-21")` → `đúng`

### 8. `sau(date1, date2)` → bool
- Kiểm tra date1 sau date2
- Ví dụ: `sau("2026-09-21", "2026-09-01")` → `đúng`

### 9. `ngay_trong_tuan(date)` → string
- Lấy ngày trong tuần
- Ví dụ: `ngay_trong_tuan("2026-09-21")` → `"Thứ Hai"`

### 10. `ngay_trong_nam(date)` → number
- Lấy ngày trong năm
- Ví dụ: `ngay_trong_nam("2026-09-21")` → `264`

### 11. `tuan(date)` → number
- Lấy số tuần trong năm
- Ví dụ: `tuan("2026-09-21")` → `39`

### 12. `la_ngay_nghi(date)` → bool
- Kiểm tra có phải ngày nghỉ (Thứ 7, CN)
- Ví dụ: `la_ngay_nghi("2026-09-21")` → `sai` (Thứ Hai)

## Lưu ý
- Viết bằng tiếng Việt
- Dùng markdown format