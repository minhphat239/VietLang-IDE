# PROJECT STATUS — VietLang IDE

## Ngày cập nhật: 21/09/2026

## 1. Tổng quan dự án
- **Tên:** VietLang IDE (Python IDLE replica)
- **Mục tiêu:** Tạo IDE chạy trên terminal cho ngôn ngữ VietLang
- **Ngôn ngữ:** Viết bằng .vl (VietLang), chạy trên engine C#
- **GitHub:** https://github.com/minhphat239/VietLang-IDE

## 2. Hành động đã làm ✅

### Milestone 1: REPL Core ✅
- `repl.vl` (228 dòng) — REPL hoàn chỉnh
- Prompt `>>> ` màu xanh lá
- Xử lý EOF (Ctrl+D thoát)
- Lệnh đặc biệt: `thoát`, `lịch_sử`, `lưu_session`, `xóa_session`, `xóa_history`, `xóa_state`, `trợ_giúp`
- Session persistence (.vlsession) — lưu/restore biến
- History persistence (.vlhistory) — giới hạn 1000 lệnh
- Smart error display — parse lỗi, hiển thị dòng + source line

### Milestone 2: Script Runner ✅
- `script_runner.vl` — chạy file .vl với error display đẹp
- `--time` flag — hiển thị thời gian chạy
- Kiểm tra file tồn tại, hiển thị lỗi đỏ

### Milestone 3: Watch Mode ✅
- `watch_mode.vl` — theo dõi file .vl, tự chạy lại khi thay đổi
- Hiển thị banner, lần chạy, thông báo theo dõi

### Builtins mới (28 builtins, 149/149 PASS)
- **Cơ bản:** in_ra(sep/end), nhập(prompt), độ_dài, chuyển_chuỗi, chuyển_số, thêm
- **NLP:** chuẩn_hóa, tìm_từ, tách_từ, tách_câu, đếm_từ, chuẩn_hóa_tìm_kiếm
- **Regex:** tìm_kiếm, khớp_pattern, thay_the
- **REPL:** thực_thi, thoát, đọc_file, ghi_file, tồn_tại, json_phân_tách, json_gộp
- **System:** chạy_lệnh, lấy_tham_số, in_mau, xóa_màn_hình
- **Session:** lấy_tất_cả_biến, gán_biến

### Feedback đã gửi (8 files)
- `ENGINE_BUGS.md` — 9 lỗi engine
- `ENGINE_SESSION_CHANGES.md` — Thay đổi engine cho session
- `REPL_BUILTINS.md` — Thiếu builtins (đã fix)
- `REPL_NHAP_EOF.md` — nhập() EOF (đã fix)
- `REPL_THUC_THI.md` — Thiếu thực_thi() (đã fix)
- `SCRIPT_RUNNER_BUGS.md` — Lỗi script runner
- `SCRIPT_RUNNER_NO_ARGS.md` — No-args timeout
- `THUCTHI_RETURNSIGNAL_LEAK.md` — ReturnSignal leak trong ThucThiChuoi

## 3. Đang làm / Dự định

### Milestone 4: Integration ⏳
- [ ] IDLE.8: Tích hợp tất cả vào main entry point
- [ ] IDLE.9: Test toàn diện + documentation

### Milestone 5: Nâng cao ⏳
- [ ] Cross-platform (thêm builtin `xóa_file()`)
- [ ] Autocomplete (gợi ý từ khóa, builtin)
- [ ] Syntax highlighting khi chạy file
- [ ] Multi-line input trong REPL

## 4. Lỗi đã investigate
- `thực_thi()` — KHÔNG có infinite recursion. Bug thật: ReturnSignal leak từ ThucThiChuoi (Interpreter.cs:278-293). Builtins test PASS, .vl test cũng PASS.
- `script_runner.vl` no-args timeout — known parser bug
- `chạy_lệnh("del ...")` — cần builtin `xóa_file()` cho cross-platform

## 5. Cách chạy
```bash
# Chạy REPL
vietlang repl.vl

# Chạy file .vl
vietlang <file.vl>

# Chạy test
vietlang test

# Chạy script runner
vietlang script_runner.vl <file.vl>

# Chạy watch mode
vietlang watch_mode.vl <file.vl>
```

## 6. Cấu trúc repo
```
VietLang-IDE/
├── repl.vl                    # REPL hoàn chỉnh (228 dòng)
├── script_runner.vl           # Script runner
├── watch_mode.vl              # Watch mode
├── PROJECT_STATUS.md          # File này
├── VietLang/                  # Engine C#
│   ├── src/Builtins.cs        # 28 builtins
│   ├── src/Interpreter.cs     # Interpreter
│   ├── src/Parser.cs          # Parser
│   ├── src/Lexer.cs           # Lexer
│   └── tests/                 # 149/149 test
├── demo/                      # Demo programs
├── Feedback/                  # 7 files feedback
└── memory/                    # Memory files
```
