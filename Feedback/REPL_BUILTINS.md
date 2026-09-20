# FEEDBACK — Thiếu sót builtins cho REPL/IDE

## TÌNH HUỐNG
Viết REPL bằng VietLang (repl.vl) nhưng gặp nhiều thiếu sót builtins.

## FILE .VL
repl.vl — REPL đơn giản

## THIẾU TÍNH NĂNG

### 1. Không có cách thoát vòng lặp (ƯU TIẾN: CAO)
- **Vấn đề:** `trong_lúc đúng { ... }` không có cách thoát
- **Code:**
  ```
  trong_lúc đúng {
    lệnh = nhập()
    # Không có cách thoát khỏi vòng lặp này
  }
  ```
- **Kỳ vọng:** Built-in `thoát()` để thoát khỏi vòng lặp/chương trình
- **Ghi chú:** Python có `sys.exit()`, JavaScript có `process.exit()`

### 2. Không có file I/O (ƯU TIẾN: CAO)
- **Vấn đề:** Không đọc/ghi file từ code .vl
- **Kỳ vọng:**
  - `đọc_file("path")` — đọc nội dung file
  - `ghi_file("path", "nội dung")` — ghi nội dung ra file
  - `tồn_tại("path")` — kiểm tra file tồn tại
- **Ghi chú:** Cần cho: lưu state/history ra file

### 3. Không có JSON (ƯU TIẾN: CAO)
- **Vấn đề:** Không serialize/deserialize dict
- **Kỳ vọng:**
  - `json_phân_tách("string")` — parse JSON string thành dict
  - `json_gộp(object)` — convert dict thành JSON string
- **Ghi chú:** Cần cho: lưu state/history ra file JSON

### 4. Không có system process (ƯU TIẾN: TRUNG BÌNH)
- **Vấn đề:** Không chạy lệnh system từ code .vl
- **Kỳ vọng:**
  - `chạy_lệnh("cmd")` — chạy lệnh system
  - `lấy_tham_số()` — lấy tham số dòng lệnh
- **Ghi chú:** Cần cho: chạy engine từ REPL

### 5. Không có ANSI color (ƯU TIẾN: TRUNG BÌNH)
- **Vấn đề:** Không in color từ code .vl
- **Kỳ vọng:**
  - `in_mau("text", "red")` — in color
  - `xóa_màn_hình()` — clear screen
- **Ghi chú:** Cần cho: REPL đẹp

### 6. `nhập()` không có prompt (ƯU TIẾN: THẤP)
- **Vấn đề:** `nhập()` chỉ đọc input, không hiển thị prompt
- **Code:**
  ```
  x = nhập()  # Không hiển thị gì, chờ user nhập
  ```
- **Kỳ vọng:** `nhập(">>> ")` hiển thị prompt trước khi đọc
- **Ghi chú:** Python: `input(">>> ")`

### 7. `in_ra()` không có sep/end (ƯU TIẾN: THẤP)
- **Vấn đề:** `in_ra(a, b)` in `a b` rồi xuống dòng
- **Kỳ vọng:** `in_ra(a, b, sep=", ", end="")` tùy chỉnh được
- **Ghi chú:** Python: `print(a, b, sep=", ", end="")`

## REPL ĐÃ VIẾT

Đã tạo `repl.vl` với các tính năng:
- Hiển thị prompt `>>> `
- Đọc input bằng `nhập()`
- Xử lý lệnh đặc biệt: `thoát`, `lịch_sử`, `xóa`, `state`
- Lưu history trong memory

**Vấn đề:** Không thoát được khỏi vòng lặp (cần Ctrl+C)

## KỲ VỌNG
1. Thêm built-in `thoát()` để thoát chương trình
2. Thêm builtins file I/O: `đọc_file`, `ghi_file`, `tồn_tại`
3. Thêm JSON: `json_phân_tách`, `json_gộp`
4. Sửa `nhập()` có prompt
5. Thêm `xóa_màn_hình()` cho REPL

## ƯU TIẾN
- CAO: `thoát()`, file I/O, JSON
- TRUNG BÌNH: System process, ANSI color
- THẤP: Sửa `nhập()`, `in_ra()`

---
*Feedback tạo: 20/09/2026*
*REPL đã viết: repl.vl (chưa hoạt động hoàn chỉnh)*
