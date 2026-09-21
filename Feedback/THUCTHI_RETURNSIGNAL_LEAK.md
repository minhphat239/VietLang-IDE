# FEEDBACK — thực_thi() ReturnSignal leak

## TÌNH HUỐNG
Investigate cho thấy `thực_thi()` KHÔNG có infinite recursion. Bug thật sự: `ReturnSignal` leak từ `ThucThiChuoi`.

## FILE C#
VietLang/src/Interpreter.cs — dòng 278-293

## LỖI
`ThucThiChuoi` bắt LexError, ParseError, RuntimeError nhưng KHÔNG bắt:
- `ReturnSignal`
- `BreakSignal`
- `ContinueSignal`

Ví dụ:
```
thực_thi("trả_về 42")
→ Lỗi: trả_về nằm ngoài hàm (crash thay vì trả 42)
```

## KỲ VỌNG
`ThucThiChuoi` nên catch:
```csharp
catch (ReturnSignal r) { return r.Value; }
catch (BreakSignal) { /* có thể trả null hoặc RuntimeError */ }
catch (ContinueSignal) { /* tương tự */ }
```

## ƯU TIẾN: CAO
Vì khi fix xong, script_runner.vl có thể dùng `thực_thi()` thay vì `chạy_lệnh("dotnet run ...")`.

---
*Feedback tạo: 21/09/2026*
