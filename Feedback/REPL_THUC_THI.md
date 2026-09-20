# FEEDBACK — Thiếu builtin thực_thi() cho REPL

## TÌNH HUỐNG
REPL dùng `chạy_lệnh(lệnh)` để thực thi lệnh, nhưng `chạy_lệnh` chỉ chạy shell commands (dir, echo...), KHÔNG eval code VietLang.

## FILE .VL
repl.vl dòng 77: `chạy_lệnh(lệnh)`

## LỖI
- Thiếu tính năng: `thực_thi(code)` — eval code VietLang từ chuỗi
- REPL hiện tại chỉ chạy được shell commands, không chạy được code .vl

## KỲ VỌNG
Built-in `thực_thi(code_string)`:
- Parse + eval code VietLang từ chuỗi
- Trả về kết quả (nếu có)
- Hiển thị lỗi nếu code sai
- Giữ state giữa các lệnh (biến, hàm đã định nghĩa)

Ví dụ:
```
>>> x = 5
>>> in_ra(x)
5
>>> hàm cộng(a, b) { trả_về a + b }
>>> in_ra(cộng(3, 4))
7
```

## ƯU TIẾN: CAO
Không có `thực_thi()` → REPL chỉ chạy shell commands, không phải REPL thực sự.

---
*Feedback tạo: 20/09/2026*
