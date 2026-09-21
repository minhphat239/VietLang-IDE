# SPEC v0.1 — Ngôn ngữ VietLang

## §1 Token

### Từ khóa
| Token | Lexeme |
|-------|--------|
| HAM | `hàm` |
| LOP | `lớp` |
| TRA_VE | `trả_về` |
| NEU | `nếu` |
| CON_NEU | `còn_nếu` |
| KHONG_THI | `không_thì` |
| TRONG_LUC | `trong_lúc` |
| VOI | `với` |
| TRONG | `trong` |
| DUNG_BREAK | `dừng` |
| TIEP | `tiếp` |
| VA | `và` |
| HOAC | `hoặc` |
| KHONG | `không` |
| DUNG_TRUE | `đúng` |
| SAI | `sai` |
| RONG | `rỗng` |
| THIS | `this` |
| THU | `thử` |
| NGOAI_LE | `ngoại_lệ` |
| CUOI_CUNG | `cuối_cùng` |
| NEM | `ném` |
| NHAP | `nhập` |

### Toán tử & dấu
| Token | Lexeme |
|-------|--------|
| CONG | `+` |
| TRU | `-` |
| NHAN | `*` |
| CHIA | `/` |
| CHIA_DU | `%` |
| CONG_BANG | `+=` |
| BANG | `=` |
| SO_SANH_BANG | `==` |
| KHAC | `!=` |
| NHO_HON | `<` |
| LON_HON | `>` |
| NHO_HON_HOAC_BANG | `<=` |
| LON_HON_HOAC_BANG | `>=` |
| DAU_MO_NGOAC_TRON | `(` |
| DAU_DONG_NGOAC_TRON | `)` |
| DAU_MO_NGOAC_VUONG | `[` |
| DAU_DONG_NGOAC_VUONG | `]` |
| DAU_MO_NGOAC_NHON | `{` |
| DAU_DONG_NGOAC_NHON | `}` |
| DAU_PHAY | `,` |
| DAU_CHAM | `.` |
| DAU_HAI_CHAM | `:` |
| SOL_SEMI | `;` |

### Literal
| Token | Kiểu |
|-------|------|
| SO | `double` |
| CHUOI | `string` |
| TEN | identifier |

## §2 AST

### Lệnh (Stmt)
- `VarDeclStmt(Name, Init)`
- `AssignStmt(Target, Value)`
- `AugAssignStmt(Target, Value)` — `x += expr` (Target là NameExpr)
- `IndexAssignStmt(Obj, Index, Value)` — gán `obj[index] = value`
- `ExprStmt(Expr)`
- `IfStmt(Cond, Then, Otherwise)`
- `WhileStmt(Cond, Body)`
- `ForInStmt(Var, Iterable, Body)`
- `FuncDeclStmt(Name, Params, Body)`
- `ClassDeclStmt(Name, Methods)`
- `ReturnStmt(Value)`
- `BreakStmt`
- `ContinueStmt`
- `TryStmt(Body, CatchVar, CatchBody, FinallyBody)`
- `RaiseStmt(Message)`
- `ImportStmt(Path)` — nhập file; Path phải là StrLit

### Biểu thức (Expr)
- `NumLit(Value)` — số
- `StrLit(Value)` — chuỗi
- `BoolLit(Value)` — đúng/sai
- `NullLit` — rỗng
- `ArrayLit(Items)` — mảng `[...]`
- `DictLit(Entries)` — dict `{ key: value, ... }`
- `NameExpr(Name)` — biến
- `ThisExpr` — this
- `BinaryExpr(Op, Left, Right)` — nhị phân
- `UnaryExpr(Op, Operand)` — một ngôi
- `CallExpr(Callee, Args)` — gọi hàm
- `GetExpr(Obj, Name)` — truy cập thuộc tính `a.b`
- `IndexExpr(Obj, Index)` — truy cập chỉ số `a[i]`

## §3 Cú pháp dict

### Tạo dict
```vl
t = { "ten": "An", "tuoi": 20 }
t = { }                       # dict rỗng
t = { "a": 1, "b": [1, 2] }  # value là mảng
t = { "a": { "x": 1 } }      # dict lồng
```

Phân biệt `{` là dict hay block:
- Sau `nếu`/`trong_lúc`/`với`/`hàm`/`lớp`/`= ` → **block**
- Trong biểu thức hoặc đầu lệnh → **dict literal** nếu token kế `{` là `CHUOI/TEN DAU_HAI_CHAM`
- Nếu không match → empty dict `{}`

### Truy cập
```vl
t["ten"]           # đọc value
t["ten"] = "Binh"  # gán value mới
```

### Duyệt key
```vl
với k trong t {
    in_ra(k, t[k])
}
```
`với...trong` trên dict → iterate keys (string).

### Method (5 builtin)
```vl
t.có("ten")       # true/false — key tồn tại?
t.lấy("tuoi")     # value — rỗng nếu key không có
t.xóa("ten")      # xóa key, trả value đã xóa
t.tất_cả()        # trả về mảng keys (list strings)
t.kích_thước()    # số key (double)
```

## §4 Built-in functions
| Hàm | Mô tả |
|-----|-------|
| `in_ra(...)` | In ra console, cách nhau bởi khoảng trắng |
| `độ_dài(x)` | Độ dài mảng/chuỗi/dict |
| `chuyển_chuỗi(x)` | Chuyển giá trị thành chuỗi |
| `chuyển_số(x)` | Chuyển chuỗi thành số |
| `nhập()` | Đọc input từ console |
| `thêm(mang, giaTri)` | Thêm phần tử vào mảng |
| `thoát()` | Thoát chương trình (`Environment.Exit(0)`) |
| `đọc_file(path)` | Đọc toàn bộ file UTF-8. File không tồn tại → RuntimeError |
| `ghi_file(path, nội dung)` | Ghi string ra file UTF-8 (ghi đè). Trả nội dung để chain |
| `tồn_tại(path)` | Kiểm tra file/thư mục tồn tại. Trả `đúng`/`sai` |
| `đọc_thu_muc(path)` | Liệt kê các file/thư mục trong thư mục. Trả mảng tên. |
| `tạo_thu_muc(path)` | Tạo thư mục (recursive). Trả `"đã tạo"` hoặc lỗi. |
| `xóa_file(path)` | Xóa file hoặc thư mục trống. Trả `"đã xóa"` hoặc lỗi. |
| `sao_copy(src, dst)` | Sao chép file (ghi đè nếu tồn tại). Trả `"đã sao chép"` hoặc lỗi. |
| `đi_tường(path)` | Resolve đường dẫn tuyệt đối. Trả chuỗi. |
| `kích_thước_file(path)` | Kích thước file (bytes). Trả số. |
| `là_thu_muc(path)` | Kiểm tra có phải thư mục. Trả `đúng`/`sai`. |
| `là_file(path)` | Kiểm tra có phải file. Trả `đúng`/`sai`. |
| `json_phân_tách(text)` | Parse JSON string thành dict/array VietLang |
| `json_gộp(object)` | Serialize dict/array thành JSON string |
| `chạy_lệnh(cmd)` | Chạy lệnh system qua cmd.exe /c, trả stdout (hoặc stderr nếu fail) |
| `thực_thi(code)` | Parse + eval code VietLang từ chuỗi. Trả kết quả expression hoặc rỗng. Lỗi → trả message (không crash). |
| `lấy_tham_số()` | Trả mảng args từ command line (bỏ tên exe) |
| `in_mau(text, mau)` | In text với ANSI color: red, green, yellow, blue, magenta, cyan, white, reset |
| `xóa_màn_hình()` | Xóa màn hình console (ANSI clear screen + cursor home) |
| `căn_bac_hai(x)` | Căn bậc hai (sqrt) |
| `tuyệt_đối(x)` | Giá trị tuyệt đối (abs) |
| `tối_đa(a, b)` | Giá trị lớn nhất (max), 1–2 tham số |
| `tối_thiểu(a, b)` | Giá trị nhỏ nhất (min), 1–2 tham số |
| `sin(x)` | Sinus (radian) |
| `cos(x)` | Cosinus (radian) |
| `tan(x)` | Tangent (radian) |
| `log(x)` | Logarit tự nhiên |
| `log2(x)` | Logarit cơ số 2 |
| `log10(x)` | Logarit cơ số 10 |
| `làm_tròn(x)` | Làm tròn về số nguyên gần nhất |
| `làm_nguyên(x)` | Làm tròn xuống (floor) |
| `làm_tròn_lên(x)` | Làm tròn lên (ceil) |
| `so_nguyen(x)` | Kiểm tra có phải số nguyên (trả `đúng`/`sai`) |
| `bay_gio()` | Trả về timestamp ISO 8601 (ví dụ: "2026-09-21T14:30:00") |
| `ngay()` | Trả về date string (ví dụ: "2026-09-21") |
| `gio()` | Trả về time string (ví dụ: "14:30:00") |
| `thoi_gian()` | Trả về Unix timestamp (seconds since 1970) |
| `cho(ms)` | Thread.Sleep(ms), trả về "đã cho xong" |
| `dem_nguoc(ms)` | Countdown blocking (ms), trả về "hết giờ" |

### Hằng số toán học
| Hằng | Giá trị |
|------|---------|
| `PI` | 3.14159265358979... |
| `E` | 2.71828182845904... |

## §5 Exception handling

### Từ khóa mới
| Token | Lexeme | Ghi chú |
|-------|--------|---------|
| THU | `thử` | bắt đầu khối try |
| NGOAI_LE | `ngoại_lệ` | bắt đầu khối catch |
| CUOI_CUNG | `cuối_cùng` | bắt đầu khối finally |
| NEM | `ném` | ném exception |

### Cú pháp

```vl
# try-catch
thử {
    ném("lỗi xyz")
} ngoại_lệ (e) {
    in_ra(e)  # e = message string
}

# try-catch-finally
thử {
    ném("lỗi")
} ngoại_lệ (e) {
    in_ra(e)
} cuối_cùng {
    in_ra("luôn chạy")
}

# try-finally (không catch)
thử {
    in_ra("hello")
} cuối_cùng {
    in_ra("cleanup")
}

# catch-all (không bind tên)
thử {
    ném("err")
} ngoại_lệ {
    in_ra("caught")
}

# ném ngoài try → runtime error
ném("err")  # lỗi runtime
```

### Đặc tả
- `ném(expr)` → throw `VietLangException` với message = `ChuoiHoa(expr)`.
- `ngoại_lệ (tên)` → bắt `VietLangException`, bind message vào biến `tên` (scope catch).
- `ngoại_lệ` (không tên) → catch-all: bắt mọi `VietLangException`, không bind biến.
- `cuối_cùng` → block luôn chạy: khi không lỗi, khi có lỗi, khi return/break/continue trong try.
- Exception ngoài try → propagate lên caller (runtime error nếu không được catch ở đâu).
- `RuntimeError` KHÔNG bị bắt bởi catch (chỉ bắt `VietLangException`).

## §6 Module (nhập)

### Từ khóa
| Token | Lexeme | Ghi chú |
|-------|--------|---------|
| NHAP | `nhập` | import file |

### Cú pháp
```vl
nhập "math.vl"          # import file, đường dẫn là chuỗi
nhập "thu_vuc/utils.vl" # đường dẫn tương đối so với file đang chạy
```

### Đặc tả
- `nhập "đường_dẫn"` → đọc file UTF-8, lexer → parser → chạy toàn bộ statements trong file đó.
- **Global env share**: file nhập đăng ký thẳng vào global scope — KHÔNG tạo namespace riêng.
- File import có thể định nghĩa hàm, lớp, biến → file chính sử dụng ngay sau lệnh nhập.
- **Circular import**: dùng `HashSet<string>` — nếu đường dẫn đã visit → skip (không loop, không lỗi).
- **File không tồn tại**: ném `RuntimeError` với message `không tìm thấy tệp '<path>'`.
- Đường dẫn phải là **string literal** — nếu không → lỗi parse `nhập cần đường dẫn dạng chuỗi`.
- Đường dẫn tương đối so với file đang chạy (không phải working dir).

## §7 Toán tử +=
```vl
x = 1
x += 5      # x = 6
s = "xin"
s += " chào"  # x = "xin chào"
```
- `x += expr` tương đương `x = x + expr`.
- Hỗ trợ: số (cộng), chuỗi (nối). Kiểu khác → RuntimeError.
- Target chỉ NameExpr (không hỗ trợ `t["k"] += 1` hay `a.b += 1`).

## §8 Method chuỗi
Gọi qua `.` trên string:
| Method | Mô tả |
|--------|-------|
| `s.tìm("sub")` | Vị trí đầu tiên (0-based), hoặc -1 nếu không tìm thấy |
| `s.thay("old", "new")` | Chuỗi mới với "old" được thay bằng "new" |
| `s.cắt()` | Bỏ whitespace 2 đầu |
| `s.chứa("sub")` | bool — có chứa "sub" không |
| `s.phân_tách("dấu")` | Mảng các phần tử tách theo "dấu" |

## §9 Method mảng
Gọi qua `.` trên array:
| Method | Mô tả |
|--------|-------|
| `a.lọc()` | Mảng mới với phần tử truthy (bỏ 0, "", rỗng, sai) |
| `a.map(hàm)` | Mảng mới, áp dụng hàm cho từng phần tử |
| `a.gộp(mảng2)` | Mảng mới (concatenate) |
