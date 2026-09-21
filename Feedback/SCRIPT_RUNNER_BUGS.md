# FEEDBACK — Script runner + thực_thi() bug

## TÌNH HUỐNG
Script runner dùng `chạy_lệnh("dotnet run ...")` thay vì `thực_thi()` vì `thực_thi()` có bug infinite recursion.

## FILE .VL
script_runner.vl dòng 42: `chạy_lệnh("dotnet run --project D:\\Documents\\VietLang\\VietLang -- " + file_path)`

## LỖI

### L1: thực_thi() infinite recursion (ƯU TIẾN: CAO)
- **Mô tả:** Gọi `thực_thi(code)` trong file .vl gây infinite recursion
- **Nguyên nhân:** Interpreter gọi lại chính nó qua builtin index
- **Kỳ vọng:** `thực_thi("in_ra(1+1)")` → in `2`, không crash
- **Ghi chú:** Builtins test (`thuc_thi_bieu_thuc`, etc.) PASS, nhưng dùng từ trong C# test khác với gọi từ .vl

### L2: Đường dẫn hardcoded (ƯU TIẾN: TRUNG BÌNH)
- **Mô tả:** `D:\\Documents\\VietLang\\VietLang` hardcoded trong script_runner.vl
- **Kỳ vọng:** Tự tìm đường dẫn engine, hoặc dùng `vietlang.cmd` trực tiếp
- **Gợi ý:** Dùng `chạy_lệnh("vietlang.cmd " + file_path)` thay vì `dotnet run`

### L3: --time dùng PowerShell (ƯU TIẾN: THẤP)
- **Mô tả:** `chạy_lệnh("powershell -Command ...")` không cross-platform
- **Kỳ vọng:** Không cần --time phức tạp, hoặc dùng cách khác

## KỲ VỌNG
1. Fix `thực_thi()` infinite recursion
2. Sửa script_runner.vl dùng `vietlang.cmd` thay vì `dotnet run`
3. Hoặc thêm builtin `thời_gian()` để đo thời gian

## ƯU TIẾN
- CAO: L1 (thực_thi() bug)
- TRUNG BÌNH: L2 (hardcoded path)
- THẤP: L3 (PowerShell timing)

---
*Feedback tạo: 20/09/2026*
