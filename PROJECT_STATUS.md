# PROJECT STATUS — VietLang IDE

## Ngày cập nhật: 20/09/2026

## 1. Tổng quan dự án
- **Tên:** VietLang IDE (Python IDLE replica)
- **Mục tiêu:** Tạo IDE chạy trên terminal cho ngôn ngữ VietLang
- **Ngôn ngữ:** Viết bằng .vl (VietLang), chạy trên engine C#

## 2. Hành động đã làm

### Builtins mới (149/149 PASS)
- `thực_thi(code)` — eval code VietLang từ chuỗi
- `đọc_file(path)`, `ghi_file(path, nội_dung)`, `tồn_tại(path)`
- `json_phân_tách(string)`, `json_gộp(object)`
- `thoát()` — thoát chương trình
- `chạy_lệnh(cmd)` — chạy lệnh system
- `lấy_tham_số()` — lấy tham số dòng lệnh
- `in_mau(text, color)` — in màu (red/green/yellow/blue/magenta/cyan/white)
- `xóa_màn_hình()` — clear screen
- `nhập(prompt)` — đọc input với prompt
- `in_ra(x, y, ..., sep, end)` — in ra với sep/end
- `lấy_tất_cả_biến()`, `gán_biến(tên, giá_trị)` — session persistence
- 6 NLP builtins + 3 regex builtins

### REPL hoàn chỉnh (repl.vl — 196 dòng)
- Prompt `>>> ` màu xanh lá
- Xử lý EOF (Ctrl+D)
- Lệnh đặc biệt: `thoát`, `lịch_sử`, `lưu_session`, `xóa_session`, `xóa_history`, `xóa_state`, `trợ_giúp`
- Session persistence (.vlsession)
- History persistence (.vlhistory, giới hạn 1000 lệnh)
- Hiển thị lỗi màu đỏ

### Test suite
- 149/149 test PASS
- Bao gồm: lexer, parser, interpreter, dict, exception, module, methods, golden tests

## 3. Dự định tiếp theo

### Milestone 2: Script Runner
- [ ] IDLE.4: script_runner.vl — chạy file .vl với error display đẹp
- [ ] IDLE.5: watch_mode.vl — auto-run khi file thay đổi

### Milestone 3: Smart Error Display
- [ ] IDLE.6: error_formatter.vl — format lỗi ANSI color
- [ ] IDLE.7: source_highlight.vl — hiển thị dòng bị lỗi

### Milestone 4: Integration
- [ ] IDLE.8: Tích hợp tất cả vào main
- [ ] IDLE.9: Test toàn diện + documentation

## 4. Mục tiêu dài hạn
- IDE hoàn chỉnh như Python IDLE
- Chạy cross-platform (Windows/Linux/macOS)
- Hỗ trợ REPL, script runner, error display
- Tích hợp NLP builtins cho xử lý tiếng Việt

## 5. Files quan trọng
```
VietLang/
├── repl.vl                    # REPL hoàn chỉnh
├── demo/bai_01.vl             # Demo program
├── src/                       # Engine C#
│   ├── Builtins.cs            # Builtins (149/149 PASS)
│   ├── Interpreter.cs         # Interpreter
│   ├── Parser.cs              # Parser
│   └── Lexer.cs               # Lexer
├── tests/                     # Test suite
├── Feedback/                  # Feedback files
└── PROJECT_STATUS.md          # File này
```

## 6. Cách chạy
```bash
# Chạy REPL
vietlang repl.vl

# Chạy file .vl
vietlang <file.vl>

# Chạy test
vietlang test
```

## 7. Notes
- Engine C# (.NET 8) — không sửa khi làm IDE
- Code .vl viết bằng VietLang, không dùng C#
- Feedback gửi vào folder Feedback/
