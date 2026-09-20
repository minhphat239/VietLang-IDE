# FEEDBACK — Lỗi khi chạy các hàm trong engine VietLang

## TÌNH HUỐNG
Test toàn bộ tính năng engine VietLang v0.2, tìm lỗi khi chạy các hàm built-in và từ khóa.

## FILE .VL
Các file test: test_builtin.vl, test_dict.vl, test_exception.vl, test_module.vl, test_nem.vl, test_class.vl, test_string.vl, test_aug.vl, test_chain.vl, test_closure.vl, test_hint_*.vl

## LỖI TÌM THẤY

### LỖI NGHIÊM TRỌNG (ƯU TIẾN: CAO)

#### L1: Try-catch không bắt được RuntimeError
- **File test:** test_exception.vl, test_try_runtime.vl
- **Code:**
  ```
  thử {
    x = 10 / 0
  } ngoại_lệ (e) {
    in_ra("Bắt được:", e)
  }
  ```
- **Output:** `Lỗi thực thi dòng 3: chia cho số 0` (crash, không bắt được)
- **Kỳ vọng:** Try-catch bắt được RuntimeError, in ra "Bắt được: chia cho số 0"
- **Ghi chú:** Try-catch chỉ bắt được VietLangException từ `ném()`, không bắt được RuntimeError từ engine

#### L2: Dict không hỗ trợ multi-line
- **File test:** test_dict.vl, test_dict_multiline.vl
- **Code:**
  ```
  t = {
    "a": 1,
    "b": 2
  }
  ```
- **Output:** `Lỗi cú pháp dòng 2: mong đợi dấu '}' đóng dict rỗng, gặp '\n'`
- **Kỳ vọng:** Dict multi-line hoạt động như Python
- **Ghi chú:** Phải viết dict trên 1 dòng: `t = {"a": 1, "b": 2}`

#### L3: `ném()` không có tham số bị lỗi cú pháp
- **File test:** test_hint_nem.vl, test_nem_params.vl
- **Code:**
  ```
  ném()
  ```
- **Output:** `Lỗi cú pháp dòng 1: không mong đợi ')' ở vị trí biểu thức`
- **Kỳ vọng:** `ném()` không có tham số → ném exception rỗng hoặc lỗi runtime rõ ràng
- **Ghi chú:** Hiện tại `ném` yêu cầu 1 tham số (thông báo lỗi), nhưng lỗi cú pháp không rõ ràng

### LỖI TRUNG BÌNH (ƯU TIẾN: TRUNG BÌNH)

#### L4: Thiếu `rỗng` trong suggestion table
- **File test:** test_hint_rong.vl, test_hint_rong_thuong.vl
- **Code:** `Rong` hoặc `rong`
- **Output:** `Lỗi thực thi dòng 1: biến 'Rong' chưa được gán` (KHÔNG có gợi ý)
- **Kỳ vọng:** Gợi ý `rỗng` khi viết `rong` hoặc `Rong`
- **Ghi chú:** `rỗng` không có trong SuggestionTable

#### L5: `sai` suggestion không hữu ích
- **File test:** test_hint_sai.vl
- **Code:** `Sai`
- **Output:** `Lỗi thực thi dòng 1: biến 'Sai' chưa được gán` (KHÔNG có gợi ý)
- **Kỳ vọng:** Gợi ý `sai` khi viết `Sai`
- **Ghi chú:** `sai` có trong SuggestionTable nhưng map chính nó → không hữu ích khi viết hoa

#### L6: `dung` suggestion mập mờ
- **File test:** test_hint_dung.vl
- **Code:** `Dung`
- **Output:** `Lỗi thực thi dòng 1: biến 'Dung' chưa được gán` (KHÔNG có gợi ý)
- **Kỳ vọng:** Gợi ý `đúng` hoặc `dừng` khi viết `Dung`
- **Ghi chú:** `dung` có trong SuggestionTable nhưng map "đúng hoặc dừng" → mập mờ

### LỖI NHỎ (ƯU TIẾN: THẤP)

#### L7: TestVietNamese.cs có lỗi build
- **File:** tests/TestVietNamese.cs
- **Lỗi:** `KiemTra` trả `void` nhưng test methods dùng như `bool`
- **Kỳ vọng:** `KiemTra` trả `bool` (đã fix)
- **Ghi chú:** File mới thêm (M3.1), chưa được thêm vào Program.cs test suite

#### L8: Test `vn_tim_tu` fail
- **File:** tests/TestVietNamese.cs
- **Code:** `vị trí = tìm_từ("xin chào chào", "chào")`
- **Output:** `Lỗi cú pháp dòng 1: thiếu dấu kết thúc lệnh (newline hoặc ';') trước 'trí'`
- **Kỳ vọng:** `vị trí` là identifier hợp lệ
- **Ghi chú:** Space trong `vị trí` gây lỗi lexer

#### L9: Test `vn_tach_cau` fail
- **File:** tests/TestVietNamese.cs
- **Output:** `tách_câu giữ delimiter` (output không đúng kỳ vọng)
- **Kỳ vọng:** `tách_câu("A. B? C!")` → `["A.", "B?", "C!"]`
- **Ghi chú:** Cần kiểm tra lại logic `tách_câu`

### TỪ KHÓA CÓ GỢI Ý (OK)

| Từ thiếu dấu | Gợi ý | Trạng thái |
|--------------|-------|------------|
| `Ham` | `hàm` | ✅ OK |
| `Lop` | `lớp` | ✅ OK |
| `Neu` | `nếu` | ✅ OK |
| `Con_neu` | `còn_nếu` | ✅ OK |
| `Khong_thi` | `không_thì` | ✅ OK |
| `Trong_luc` | `trong_lúc` | ✅ OK |
| `Tra_ve` | `trả_về` | ✅ OK |
| `Thu` | `thử` | ✅ OK |
| `Ngoai_le` | `ngoại_lệ` | ✅ OK |
| `Cuoi_cung` | `cuối_cùng` | ✅ OK |
| `Nem` | `ném` | ✅ OK |
| `Khai_bao` | `khai_báo` | ✅ OK |

### TỪ KHÓA KHÔNG CÓ GỢI Ý (THIẾU)

| Từ thiếu dấu | Từ đúng | Trạng thái |
|--------------|---------|------------|
| `rong`/`Rong` | `rỗng` | ❌ Thiếu trong table |
| `sai`/`Sai` | `sai` | ⚠️ Map chính nó |
| `dung`/`Dung` | `đúng`/`dừng` | ⚠️ Mập mờ |

### TÍNH NĂNG CHẠY OK

| Tính năng | Trạng thái |
|-----------|------------|
| Builtins (in_ra, độ_dài, chuyển_chuỗi, chuyển_số, thêm) | ✅ OK |
| Dict (CRUD + method) | ✅ OK (trên 1 dòng) |
| Exception (try/catch/ném) | ⚠️ Chỉ bắt VietLangException |
| Module (khai_báo) | ✅ OK |
| Augmented assignment (+=) | ✅ OK |
| String methods (tìm, thay, cắt, chứa, phân_tách) | ✅ OK |
| Array methods (lọc, map, gộp) | ✅ OK |
| Class & object | ✅ OK |
| Closure | ✅ OK |
| Method chaining | ✅ OK |

## KỲ VỌNG
1. Fix try-catch bắt được RuntimeError
2. Hỗ trợ dict multi-line
3. `ném()` không tham số → lỗi runtime rõ ràng
4. Thêm `rỗng` vào suggestion table
5. Fix testVietNamese.cs (đã fix build, cần fix logic)
6. Fix test `vn_tim_tu` và `vn_tach_cau`

## ƯU TIẾN
- CAO: L1 (try-catch RuntimeError), L2 (dict multi-line), L3 (ném() không tham số)
- TRUNG BÌNH: L4-L6 (suggestion table)
- THẤP: L7-L9 (test file fixes)

---
*Feedback tạo: 20/09/2026*
*Số lỗi: 9 (3 nghiêm trọng + 3 trung bình + 3 nhỏ)*
*Test coverage: 105/108 PASS (3 FAIL từ TestVietNamese.cs)*
