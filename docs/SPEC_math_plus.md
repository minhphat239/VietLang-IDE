# Math Plus Library — SPEC v1.0

## Tổng quan
Thư viện toán học nâng cao cho VietLang.

## Danh sách functions

### 1. ngau_nhien() → number
- Số ngẫu nhiên từ 0 đến 1
- Ví dụ: `ngau_nhien()` → 0.7234...

### 2. ngau_nhien_range(min, max) → number
- Số ngẫu nhiên trong khoảng
- Ví dụ: `ngau_nhien_range(1, 10)` → 5

### 3. gan_gioi_han(value, min, max) → number
- Giới hạn value trong khoảng
- Ví dụ: `gan_gioi_han(15, 0, 10)` → 10

### 4. interpolation(a, b, t) → number
- Linear interpolation: `a + (b - a) * t`
- Ví dụ: `interpolation(0, 100, 0.5)` → 50

### 5. anh_xa(value, in_min, in_max, out_min, out_max) → number
- Ánh xạ value từ khoảng này sang khoảng khác
- Ví dụ: `anh_xa(5, 0, 10, 0, 100)` → 50

### 6. boi_so_chung(a, b) → number
- Ước số chung lớn nhất
- Ví dụ: `boi_so_chung(12, 8)` → 4

### 7. boi_so_chung_nho_nhat(a, b) → number
- Bội số chung nhỏ nhất
- Ví dụ: `boi_so_chung_nho_nhat(4, 6)` → 12

### 8. nguyen_to(n) → bool
- Kiểm tra nguyên tố
- Ví dụ: `nguyen_to(7)` → đúng

### 9. fibonacci(n) → number
- Số fibonacci thứ n
- Ví dụ: `fibonacci(10)` → 55

### 10. giai_thua(n) → number
- Giai thừa n
- Ví dụ: `giai_thua(5)` → 120

### 11. luy_thua_co_so(a, b) → number
- a mũ b
- Ví dụ: `luy_thua_co_so(2, 10)` → 1024

### 12. tong(mang) → number
- Tổng tất cả phần tử
- Ví dụ: `tong([1, 2, 3, 4])` → 10

### 13. trung_binh(mang) → number
- Trung bình cộng
- Ví dụ: `trung_binh([1, 2, 3, 4])` → 2.5

### 14. lon_nhat(mang) → number
- Giá trị lớn nhất
- Ví dụ: `lon_nhat([1, 5, 3])` → 5

### 15. nho_nhat(mang) → number
- Giá trị nhỏ nhất
- Ví dụ: `nho_nhat([1, 5, 3])` → 1

## Lưu ý
- Viết bằng tiếng Việt
- Dùng markdown format
