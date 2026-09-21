# VietLang

Ngôn ngữ lập trình tiếng Việt, phong cách Python nhưng khối lệnh dùng `{}` thay vì thụt lề.

## 1. Giới thiệu

VietLang là ngôn ngữ lập trình có cú pháp và từ khóa **bằng tiếng Việt** (`hàm`, `nếu`, `trong_lúc`, `lớp`, `đúng`, `rỗng`...). Người học và người làm việc xử lý văn bản tiếng Việt có thể đọc code tự nhiên như đọc diễn đạt.

- Cú pháp giống Python: dynamic typing, không cần khai báo biến, vòng lặp/điều kiện dùng khối `{}`.
- Mục tiêu chính: **xử lý NLP tiếng Việt** — từ khóa và thư viện chuẩn hướng tới các thao tác như `chuẩn_hóa`, `tách_từ`, `tách_câu` (kế hoạch M3).
- Engine viết bằng **C# / .NET** chạy trên interpreter riêng, tối ưu cho tốc độ xử lý văn bản lớn.

Trạng thái hiện tại: giai đoạn **M1 — MVP interpreter** (lexer + parser + interpreter đã chạy được, 45 bài tự kiểm tra).

## 2. Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (đã kiểm thử với 8.0.x trên Windows).

## 3. Cách chạy

```sh
# Cách 1: dùng batch script (đơn giản nhất)
vietlang demo\bai_01.vl    # chạy file .vl
vietlang test              # chạy 45 test

# Cách 2: dùng dotnet trực tiếp
dotnet run --project VietLang -- demo\bai_01.vl
dotnet run --project VietLang -- test
```

Chương trình đọc file UTF-8 và in kết quả ra màn hình. Khi gặp lỗi, chương trình dừng và in thông báo tiếng Việt kèm vị trí, rồi thoát với mã lỗi 1. Ví dụ:

```
Lỗi thực thi dòng 3: biến 'x' chưa được gán
```

Có 3 loại lỗi. Lỗi từ vựng (`Lỗi từ vựng dòng N: ...`), lỗi cú pháp (`Lỗi cú pháp dòng N: ...`), lỗi runtime (`Lỗi thực thi dòng N: ...`). Với từ khóa gõ thiếu dấu (như `ham` thay vì `hàm`), chương trình còn gợi ý sửa chính tả.

## 4. Ví dụ nhanh

Trích từ `demo/bai_01.vl` (quản lý học sinh):

```
lớp HọcSinh {
  hàm khởi_tạo(tên, điểm) {
    this.tên = tên
    this.điểm = điểm
  }
  hàm xếp_loại() {
    nếu this.điểm >= 8 { trả_về "Giỏi" }
    còn_nếu this.điểm >= 6 { trả_về "Khá" }
    còn_nếu this.điểm >= 4 { trả_về "Trung bình" }
    không_thì { trả_về "Yếu" }
  }
}

ds = [HọcSinh("An", 9), HọcSinh("Bình", 5)]
với hs trong ds {
  in_ra(hs.tên, "→", hs.xếp_loại())
}
```

Output:

```
An → Giỏi
Bình → Trung bình
```

Chạy bản đầy đủ: `dotnet run --project VietLang -- demo/bai_01.vl`

Lưu ý: phép `+` chỉ cộng số hoặc nối chuỗi với chuỗi — muốn nối số vào chuỗi phải dùng `chuyển_chuỗi()`.

## 5. Từ khóa

| Từ khóa | Nghĩa / giống Python |
|---|---|
| `hàm` | định nghĩa hàm (`def`) |
| `lớp` | định nghĩa lớp (`class`) |
| `trả_về` | trả về giá trị (`return`) |
| `nếu` / `còn_nếu` / `không_thì` | `if` / `elif` / `else` |
| `trong_lúc` | vòng lặp điều kiện (`while`) |
| `với ... trong` | vòng lặp qua danh sách (`for ... in`) |
| `trong` | thành phần của |
| `dừng` / `tiếp` | `break` / `continue` |
| `và` / `hoặc` / `không` | `and` / `or` / `not` |
| `đúng` / `sai` | `True` / `False` |
| `rỗng` | `None` |
| `this` | đối tượng hiện tại (`self`) |
| `khởi_tạo` | tên chuẩn của constructor |

Từ khóa **bắt buộc có dấu**: `ham` không phải `hàm`, `lop` không phải `lớp` (engine chỉ gợi ý chính tả, không tự sửa).

## 6. Builtins

| Tên | Nghĩa |
|---|---|
| `in_ra(x, y, ...)` | in ra màn hình, các giá trị cách nhau dấu cách, tự xuống dòng |
| `độ_dài(x)` | độ dài chuỗi hoặc mảng |
| `chuyển_chuỗi(x)` | ép về chuỗi (bắt buộc khi nối số với chuỗi) |
| `chuyển_số(x)` | ép chuỗi về số (lỗi nếu không chuyển được) |
| `nhập()` | đọc một dòng từ bàn phím |
| `thêm(mảng, x)` | thêm phần tử vào cuối mảng |

## 7. Cấu trúc thư mục

```
VietLang/
├── Program.cs          # điểm vào CLI: chạy file .vl hoặc chạy test
├── VietLang.csproj
├── src/                # engine
│   ├── Token.cs        # định nghĩa token + loại token
│   ├── Lexer.cs        # đọc chuỗi → token + gợi ý thiếu dấu
│   ├── AST.cs          # cây cú pháp
│   ├── Parser.cs       # token → AST
│   ├── Interpreter.cs  # chạy AST + chuẩn hóa giá trị in ra
│   └── Builtins.cs     # hàm dựng sẵn
├── tests/              # bộ tự kiểm tra (lexer, parser, interpreter)
├── demo/
│   └── bai_01.vl       # chương trình demo đầu tiên
└── docs/
    └── SPEC_v0_1.md    # đặc tả ngôn ngữ v0.1 (contract)
```

## 8. Standard Libraries

VietLang cung cấp các thư viện chuẩn sau:

### Math Library
`can_bac_hai`, `tuyet_doi`, `toi_da`, `toi_thieu`, `sin`, `cos`, `tan`, `log`, `log2`, `log10`, `lam_tron`, `lam_nguyen`, `lam_tron_len`, `so_nguyen`, `PI`, `E`

### File System Library
`doc_thu_muc`, `tao_thu_muc`, `xoa_file`, `sao_copy`, `di_tuong`, `kich_thuoc_file`, `la_thu_muc`, `la_file`

### DateTime Library
`bay_gio`, `ngay`, `gio`, `thoi_gian`, `cho`, `dem_nguoc`

### HTTP Library
`lay`, `gui`, `gui_chuoi`

## 9. Lộ trình

| Giai đoạn | Nội dung |
|---|---|
| **M1** | MVP interpreter: lexer + parser + interpreter, 45/45 test ✓ (hiện tại) |
| **M2** | dict/object, exception (`thử`/`ngoại_lệ`), module (`nhập`), toán tử `+=` |
| **M3** | thư viện chuẩn tiếng Việt: `chuẩn_hóa`, `tách_từ`, `tách_câu`, regex tiếng Việt — **ưu tiên NLP** |
| **M4** | REPL, CLI hoàn chỉnh, packaging, extension cho editor |
| **M5+** | mở rộng lên 100+ thư viện |

## 10. Đóng góp

- Bài tập gọn gàng, có thử nghiệm: mỗi tính năng mới đều có test đi kèm (`tests/`).
- Ngôn ngữ v0.1 là **contract** trong `docs/SPEC_v0_1.md` — đổi hành vi ngôn ngữ phải sửa SPEC trước.
- Chuẩn commit theo từng task (T1, T2, T3...) kèm ghi chú verify (số test PASS).
- Yêu cầu tính năng hoặc báo lỗi: ghi rõ đoạn mã `.vl` gây lỗi + output kỳ vọng.