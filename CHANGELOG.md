# CHANGELOG

## [0.3.0] - 2026-09-21
### Added
- Math library: can_bac_hai, tuyet_doi, toi_da, toi_thieu, sin/cos/tan, log/log2/log10, lam_tron/lam_nguyen/lam_tron_len, so_nguyen, PI, E
- File System library: doc_thu_muc, tao_thu_muc, xoa_file, sao_copy, di_tuong, kich_thuoc_file, la_thu_muc, la_file
- DateTime library: bay_gio, ngay, gio, thoi_gian, cho, dem_nguoc
- HTTP library: lay, gui, gui_chuoi
- 35+ new self-tests (167 total)

## v0.2.0 — 20/09/2026

### Tính năng mới
- **Dict**: `{ "key": value }`, truy cập `t["key"]`, gán `t["key"] = val`, `.có()`, `.lấy()`, `.xóa()`, `.tất_cả()`, `.kích_thước()`, vòng lặp `với k trong t`
- **Exception handling**: `thử { ... } ngoại_lệ (e) { ... } cuối_cùng { ... }`, `ném("msg")`
- **Module (khai_báo)**: `khai_báo "file.vl"` — import hàm, biến, lớp từ file khác; circular import safety
- **Augmented assignment**: `+=`, `-=`, `*=`, `/=`, `%=`
- **Method chuỗi**: `.độ_dài()`, `.chứa()`, `.bắt_đầu()`, `.kết_thúc()`, `.thay_thế()`, `.cắt()`, `.viết_hoa()`, `.viết_thường()`, `.viết_hoa_đầu_chữ()`
- **Method mảng**: `.độ_dài()`, `.thêm()`, `.xóa()`, `.chứa()`, `.tìm()`, `.sắp_xếp()`, `.đảo()`, `.lọc()`, `.map()`

### Cải tiến
- Golden integration tests: 4 file `.vl` thật bảo vệ regression (hello, dict, exception, module)
- Phiên bản hiển thị: `dotnet run -- --version` và dòng version trong usage
- Version header `v0.2.0` trong `VietLang.csproj`

## v0.1.0 — M1/M2 cơ bản

### Tính năng cốt lõi
- Lexer + Parser + Interpreter cho tiếng Việt
- Từ khóa bắt buộc có dấu (`hàm`, `lớp`, `nếu`, `trong_lúc`, `với`, `trả_về`, ...)
- Kiểu dữ liệu: số, chuỗi, đúng/sai, rỗng, hàm, lớp
- Toán tử: số học, so sánh, logic
- Vòng lặp: `trong_lúc`, `với ... trong`, `dừng`, `tiếp`
- Điều kiện: `nếu`, `còn_nếu`, `không_thì`
- Hàm: định nghĩa, tham số, `trả_về`, đệ quy
- Lớp: `lớp`, `khoi_tao()`, method, `this`
- Built-in: `in_ra()`, `nhập()`, `độ_dài()`, `kiểu()`, `số()`, `chuỗi()`
- Self-test suite: lexer, parser, interpreter
