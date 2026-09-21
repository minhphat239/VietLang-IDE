# BUILTIN RENAME MAP — Diacritics → No Diacritics

Quy tắc: Bỏ toàn bộ dấu tiếng Việt (àáảãạ, ăắằẳẵặ, âấầẩẫậ, èéẻẽẹ, êếềểễệ, ìíỉĩị, òóỏõọ, ôốồổỗộ, ơớờởỡợ, ùúủũụ,ưứừửữự, ỳýỷỹỵ, đ) rồi chuyển về chữ thường.

## Builtin Builtins (Core 6)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 1 | `độ_dài` | `do_dai` |
| 2 | `chuyển_chuỗi` | `chuyen_chuoi` |
| 3 | `chuyển_số` | `chuyen_so` |
| 4 | `nhập` | `nhap` |
| 5 | `thêm` | `them` |
| 6 | `in_ra` | `in_ra` *(giữ nguyên — đã không dấu)* |

## NLP Builtins (6)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 7 | `chuẩn_hóa` | `chuan_hoa` |
| 8 | `tìm_từ` | `tim_tu` |
| 9 | `tách_từ` | `tach_tu` |
| 10 | `tách_câu` | `tach_cau` |
| 11 | `đếm_từ` | `dem_tu` |
| 12 | `chuẩn_hóa_tìm_kiếm` | `chuan_hoa_tim_kiem` |

## Regex Builtins (3)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 13 | `tìm_kiếm` | `tim_kiem` |
| 14 | `khớp_pattern` | `khop_pattern` |
| 15 | `thay_thế` | `thay_the` |

## REPL Builtins (7)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 16 | `thoát` | `thoat` |
| 17 | `đọc_file` | `doc_file` |
| 18 | `ghi_file` | `ghi_file` *(giữ nguyên)* |
| 19 | `tồn_tại` | `ton_tai` |
| 20 | `đọc_thu_muc` | `doc_thu_muc` |
| 21 | `tạo_thu_muc` | `tao_thu_muc` |
| 22 | `xóa_file` | `xoa_file` |
| 23 | `sao_chép` | `sao_chep` |
| 24 | `đi_tường` | `di_tuong` |
| 25 | `kích_thước_file` | `kich_thuoc_file` |
| 26 | `là_thu_muc` | `la_thu_muc` |
| 27 | `là_file` | `la_file` |
| 28 | `json_phân_tách` | `json_phan_tach` |
| 29 | `json_gộp` | `json_gop` |
| 30 | `chạy_lệnh` | `chay_lenh` |
| 31 | `lấy_tham_số` | `lay_tham_so` |
| 32 | `in_màu` | `in_mau` |
| 33 | `xóa_màn_hình` | `xoa_man_hinh` |

## Eval Builtins (2)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 34 | `thực_thi` | `thuc_thi` |
| 35 | `lấy_tất_cả_biến` | `lay_tat_ca_bien` |
| 36 | `gán_biến` | `gan_bien` |

## Math Builtins (11)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 37 | `căn_bac_hai` | `can_bac_hai` |
| 38 | `tuyệt_đối` | `tuyet_doi` |
| 39 | `tối_đa` | `toi_da` |
| 40 | `tối_thiểu` | `toi_thieu` |
| 41 | `làm_tròn` | `lam_tron` |
| 42 | `làm_nguyên` | `lam_nguyen` |
| 43 | `làm_tròn_lên` | `lam_tron_len` |
| 44 | `số_nguyên` | `so_nguyen` |
| 45 | `sin` | `sin` *(giữ nguyên)* |
| 46 | `cos` | `cos` *(giữ nguyên)* |
| 47 | `tan` | `tan` *(giữ nguyên)* |
| 48 | `log` | `log` *(giữ nguyên)* |
| 49 | `log2` | `log2` *(giữ nguyên)* |
| 50 | `log10` | `log10` *(giữ nguyên)* |
| 51 | `PI` | `PI` *(giữ nguyên)* |
| 52 | `E` | `E` *(giữ nguyên)* |

## DateTime Builtins (5)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 53 | `bay_giờ` | `bay_gio` |
| 54 | `ngày` | `ngay` |
| 55 | `giờ` | `gio` |
| 56 | `thời_gian` | `thoi_gian` |
| 57 | `cho` | `cho` *(giữ nguyên)* |
| 58 | `đếm_ngược` | `dem_nguoc` |

## HTTP Builtins (3)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 59 | `lấy` | `lay` |
| 60 | `gửi` | `gui` |
| 61 | `gửi_chuỗi` | `gui_chuoi` |

## GUI Builtins (20)

| # | Old (dấu) | New (không dấu) |
|---|-----------|-----------------|
| 62 | `tạo_cửa_sổ` | `tao_cua_so` |
| 63 | `tạo_nút` | `tao_nut` |
| 64 | `tạo_ô_văn_bản` | `tao_o_van_ban` |
| 65 | `tạo_nhãn` | `tao_nhan` |
| 66 | `tạo_dòng_chữ` | `tao_dong_chu` |
| 67 | `đặt_title` | `dat_title` |
| 68 | `đặt_kích_thước` | `dat_kich_thuoc` |
| 69 | `lấy_văn_bản` | `lay_van_ban` |
| 70 | `đặt_văn_bản` | `dat_van_ban` |
| 71 | `thêm_dòng` | `them_dong` |
| 72 | `xóa_trống` | `xoa_trong` |
| 73 | `đóng` | `dong` |
| 74 | `đặt_kích_thước_widget` | `dat_kich_thuoc_widget` |
| 75 | `đặt_vị_trí` | `dat_vi_tri` |
| 76 | `đặt_font` | `dat_font` |
| 77 | `đặt_màu_nền` | `dat_mau_nen` |
| 78 | `đặt_màu_chữ` | `dat_mau_chu` |
| 79 | `tạo_text_editor` | `tao_text_editor` *(giữ nguyên)* |
| 80 | `tạo_output` | `tao_output` *(giữ nguyên)* |
| 81 | `tạo_panel` | `tao_panel` *(giữ nguyên)* |
| 82 | `thêm_vào_panel` | `them_vao_panel` |
| 83 | `hộp_thoại_mở_file` | `hop_thoai_mo_file` |
| 84 | `hộp_thoại_lưu_file` | `hop_thoai_luu_file` |
| 85 | `lấy_dòng` | `lay_dong` |
| 86 | `đặt_dòng` | `dat_dong` |
| 87 | `đếm_dòng` | `dem_dong` |
| 88 | `chạy` | `chạy` *(giữ nguyên — keyword)* |
| 89 | `thêm_widget` | `them_widget` |

## Tổng cộng: 89 builtin names đã audit
- Đổi: ~65 builtin có dấu → không dấu
- Giữ nguyên: ~24 builtin đã không dấu (in_ra, ghi_file, sin, cos, tan, log, log2, log10, PI, E, cho, tao_text_editor, tao_output, tao_panel, chạy, etc.)
