# FEEDBACK — nhập() trả rỗng khi EOF

## TÌNH HUỐNG
REPL dùng `nhập()` để đọc input. Khi stdin EOF (Ctrl+D / pipe), `nhập()` trả `""` thay vì `rỗng`.

## FILE .VL
repl.vl dòng 55: `lệnh = nhập()`

## LỖI
- `nhập()` trả `""` khi EOF → REPL lặp vô hạn
- Cần `nhập()` trả `rỗng` khi EOF để REPL thoát được

## KỲ VỌNG
```
>>> ^D  (EOF)
Tạm biệt!
```
Thay vì lặp vô hạn.

## ƯU TIẾN: CAO
REPL không thoát được khi stdin EOF.

---
*Feedback tạo: 20/09/2026*
