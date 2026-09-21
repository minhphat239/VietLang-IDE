# FEEDBACK — script_runner.vl no-args timeout

## TÌNH HUỐNG
Chạy `vietlang script_runner.vl` (không có args) → timeout, không thoát được.

## FILE .VL
script_runner.vl dòng 4-10

## LỖI
- `lấy_tham_số()` trả về mảng rỗng khi chạy `vietlang script_runner.vl`
- `độ_dài(tham_số) == 0` kiểm tra nhưng script không thoát
- Script treo vô hạn (timeout 120s)

## KỲ VỌNG
```
> vietlang script_runner.vl
Cách dùng: vietlang script_runner.vl <file.vl>
Tùy chọn: --time  (hiển thị thời gian chạy)
```
Rồi thoát.

## GỢI Ý
Có thể `lấy_tham_số()` trả về `[]` nhưng `độ_dài([])` trả về giá trị khác 0?
Hoặc có bug parser/interpreter với no-args case.

## ƯU TIẾN: TRUNG BÌNH

---
*Feedback tạo: 20/09/2026*
