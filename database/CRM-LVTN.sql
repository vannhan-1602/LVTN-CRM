-- --------------------------------------------------------
-- Máy chủ:                      127.0.0.1
-- Phiên bản máy chủ:            8.0.46 - MySQL Community Server - GPL
-- HĐH máy chủ:                  Linux
-- HeidiSQL Phiên bản:           12.15.0.7171
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Đang kết xuất đổ cấu trúc cơ sở dữ liệu cho CRM-LVTN
CREATE DATABASE IF NOT EXISTS `CRM-LVTN` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_bin */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `CRM-LVTN`;

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.BH_CoHoiBanHang
CREATE TABLE IF NOT EXISTS `BH_CoHoiBanHang` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TenThuongVu` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `GiaiDoan` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'KhaoSat',
  `KhachHang_Id` bigint unsigned DEFAULT NULL,
  `Lead_Id` bigint unsigned DEFAULT NULL,
  `TyLeThanhCong` int DEFAULT '0',
  `DoanhThuKyVong` decimal(18,2) DEFAULT NULL,
  `GhiChu` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  `NgayDuKien` date DEFAULT NULL,
  `NhanVienPhuTrach_Id` int unsigned DEFAULT NULL,
  `IsDeleted` tinyint(1) DEFAULT '0',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `fk_ch_kh` (`KhachHang_Id`),
  KEY `fk_ch_nv` (`NhanVienPhuTrach_Id`),
  KEY `fk_ch_lead` (`Lead_Id`),
  CONSTRAINT `fk_ch_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`),
  CONSTRAINT `fk_ch_lead` FOREIGN KEY (`Lead_Id`) REFERENCES `KH_Lead` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ch_nv` FOREIGN KEY (`NhanVienPhuTrach_Id`) REFERENCES `HT_User` (`Id`),
  CONSTRAINT `chk_ty_le` CHECK ((`TyLeThanhCong` between 0 and 100))
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.BH_CoHoiBanHang: ~20 rows (xấp xỉ)
INSERT INTO `BH_CoHoiBanHang` (`Id`, `TenThuongVu`, `GiaiDoan`, `KhachHang_Id`, `Lead_Id`, `TyLeThanhCong`, `DoanhThuKyVong`, `GhiChu`, `NgayDuKien`, `NhanVienPhuTrach_Id`, `IsDeleted`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'Triển khai CRM Pro cho Giải pháp Số Việt', 'ThanhCong', 1, NULL, 100, 45000000.00, 'Đã ký hợp đồng HD2026-001', NULL, 3, 0, '2026-05-02 09:30:00', '2026-05-10 10:00:00'),
	(2, 'Bán CRM Basic cho Hoàng Phát', 'ThuongLuong', 2, NULL, 60, 15000000.00, 'Khách đang chờ duyệt ngân sách quý 3', '2026-07-15', 4, 0, '2026-05-02 09:45:00', '2026-06-20 11:00:00'),
	(3, 'Gói bảo trì hệ thống cho Đông Á', 'DeXuat', 4, NULL, 40, 8000000.00, 'Đã gửi đề xuất qua email, chờ phản hồi', '2026-07-10', 2, 0, '2026-05-10 10:30:00', '2026-06-18 08:30:00'),
	(4, 'Triển khai ERP cho Ánh Dương', 'ThanhCong', 10, 3, 100, 60000000.00, 'Chuyển đổi từ Lead, đã ký hợp đồng HD2026-002', NULL, 5, 0, '2026-06-01 08:30:00', '2026-06-15 10:00:00'),
	(5, 'Nâng cấp hệ thống CRM cho Hương Việt', 'KhaoSat', 5, NULL, 20, 20000000.00, 'Mới khảo sát nhu cầu ban đầu', '2026-08-01', 3, 0, '2026-06-20 09:00:00', '2026-06-20 09:00:00'),
	(6, 'Gói CRM Enterprise cho Logistics Miền Nam', 'ThanhCong', 7, NULL, 100, 50000000.00, 'Đã ký hợp đồng HD2026-006', NULL, 5, 0, '2026-05-15 09:00:00', '2026-06-20 15:00:00'),
	(7, 'Đề xuất phần mềm quản lý cho Phòng khám An Tâm', 'ThatBai', 9, NULL, 0, 12000000.00, 'Khách chọn giải pháp của đối thủ do giá thấp hơn', NULL, 4, 0, '2026-05-14 10:00:00', '2026-06-08 09:00:00'),
	(8, 'Triển khai HRM cho Dệt may Phú Cường', 'ThuongLuong', 11, NULL, 65, 12000000.00, 'Đang thương lượng lại đơn giá', '2026-07-25', 6, 0, '2026-05-20 09:00:00', '2026-06-26 10:00:00'),
	(9, 'Phần mềm quản lý học sinh cho Trường Việt Anh', 'DeXuat', 12, NULL, 35, 9000000.00, 'Đã gửi báo giá sơ bộ', '2026-07-18', 4, 0, '2026-06-02 09:00:00', '2026-06-15 10:00:00'),
	(10, 'Gói CRM chiến lược cho Dược phẩm Minh Khang', 'ThanhCong', 13, NULL, 100, 70000000.00, 'Khách VIP, đã ký hợp đồng dài hạn 24 tháng', NULL, 3, 0, '2026-05-11 09:00:00', '2026-06-01 10:00:00'),
	(11, 'Phần mềm quản lý vận tải cho Vận tải Sài Gòn', 'ThuongLuong', 15, NULL, 55, 18000000.00, 'Khách yêu cầu demo thêm module định vị', '2026-07-22', 5, 0, '2026-05-13 09:00:00', '2026-06-22 10:00:00'),
	(12, 'POS bán lẻ cho Siêu thị Mini Bình Minh', 'ThanhCong', 16, NULL, 100, 16000000.00, 'Đã ký hợp đồng', NULL, 6, 0, '2026-06-01 09:00:00', '2026-06-24 10:00:00'),
	(13, 'Gói CRM bất động sản cho Phú Gia', 'ThuongLuong', 17, NULL, 70, 55000000.00, 'Đang đàm phán điều khoản thanh toán', '2026-07-28', 3, 0, '2026-05-16 09:00:00', '2026-06-28 10:00:00'),
	(14, 'Phần mềm quản lý sản xuất cho Cơ khí Chính Xác', 'DeXuat', 19, NULL, 30, 25000000.00, 'Đã gửi đề xuất kỹ thuật', '2026-08-05', 5, 0, '2026-05-25 09:00:00', '2026-06-10 10:00:00'),
	(15, 'Đề xuất phần mềm bán sách cho Tri Thức Việt', 'ThatBai', 20, NULL, 0, 7000000.00, 'Khách ngừng kinh doanh, hủy dự án', NULL, 6, 0, '2026-05-19 10:00:00', '2026-06-01 10:00:00'),
	(16, 'Gói CRM du lịch cho Việt Xanh', 'KhaoSat', 21, NULL, 25, 22000000.00, 'Mới khảo sát nhu cầu', '2026-08-10', 3, 0, '2026-06-25 09:00:00', '2026-06-25 09:00:00'),
	(17, 'Phần mềm quản lý hội viên cho Gym Sức Sống', 'DeXuat', 22, NULL, 45, 9500000.00, 'Đã gửi báo giá', '2026-07-30', 4, 0, '2026-06-05 09:00:00', '2026-06-20 10:00:00'),
	(18, 'Triển khai CRM Pro quản lý đơn hàng cho In ấn Kim Long', 'ThanhCong', 23, 6, 100, 15000000.00, 'Chuyển đổi từ Lead, đã ký hợp đồng HD2026-008', NULL, 5, 0, '2026-06-18 14:20:00', '2026-06-24 10:00:00'),
	(19, 'Gói ERP cho Thiết bị Y tế Hòa Bình', 'KhaoSat', 25, NULL, 20, 65000000.00, 'Mới khảo sát quy mô dự án', '2026-08-15', 3, 0, '2026-06-27 09:00:00', '2026-06-27 09:00:00'),
	(20, 'Nâng cấp CRM cho Điện máy Thành Công', 'ThatBai', 8, NULL, 0, 9000000.00, 'Khách ngừng giao dịch, không tiếp tục', NULL, 3, 0, '2026-05-05 09:00:00', '2026-06-15 10:00:00'),
	(21, 'Triển khai 1 app', 'ThanhCong', 26, NULL, 100, 1000000.00, 'Lần đầu', '2026-07-16', NULL, 1, '2026-07-08 11:53:37', '2026-07-09 01:32:16');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.BH_LoaiSanPham
CREATE TABLE IF NOT EXISTS `BH_LoaiSanPham` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `TenLoai` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `MoTa` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.BH_LoaiSanPham: ~2 rows (xấp xỉ)
INSERT INTO `BH_LoaiSanPham` (`Id`, `TenLoai`, `MoTa`) VALUES
	(1, 'Phần mềm', 'Bản quyền phần mềm (license)'),
	(2, 'Dịch vụ', 'Triển khai, đào tạo, bảo trì, tùy biến');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.BH_SanPham
CREATE TABLE IF NOT EXISTS `BH_SanPham` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LoaiSanPham_Id` int unsigned DEFAULT NULL,
  `MaSP` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TenSP` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `DonVi` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `GiaBan` decimal(18,2) DEFAULT '0.00',
  `SoLuongTon` int DEFAULT '0',
  `TrangThai` tinyint DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaSP` (`MaSP`),
  KEY `fk_sp_loai` (`LoaiSanPham_Id`),
  CONSTRAINT `fk_sp_loai` FOREIGN KEY (`LoaiSanPham_Id`) REFERENCES `BH_LoaiSanPham` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.BH_SanPham: ~14 rows (xấp xỉ)
INSERT INTO `BH_SanPham` (`Id`, `LoaiSanPham_Id`, `MaSP`, `TenSP`, `DonVi`, `GiaBan`, `SoLuongTon`, `TrangThai`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 1, 'CRM-BASIC', 'Phần mềm CRM Bản Basic', 'License', 5000000.00, 60, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(2, 1, 'CRM-PRO', 'Phần mềm CRM Bản Pro', 'License', 15000000.00, 32, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(3, 1, 'CRM-ENT', 'Phần mềm CRM Bản Enterprise', 'License', 45000000.00, 14, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(4, 1, 'ERP-BASIC', 'Phần mềm ERP Bản Basic', 'License', 30000000.00, 11, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(5, 1, 'ERP-PRO', 'Phần mềm ERP Bản Pro', 'License', 60000000.00, 6, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(6, 1, 'HRM-BASIC', 'Phần mềm quản lý nhân sự HRM', 'License', 12000000.00, 19, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(7, 1, 'POS-RETAIL', 'Phần mềm bán hàng POS cho bán lẻ', 'License', 8000000.00, 23, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(8, 2, 'SRV-SETUP', 'Dịch vụ triển khai hệ thống', 'Gói', 10000000.00, 999, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(9, 2, 'SRV-TRAINING', 'Dịch vụ đào tạo sử dụng phần mềm', 'Buổi', 3000000.00, 999, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(10, 2, 'SRV-MAINTAIN', 'Dịch vụ bảo trì hệ thống hàng năm', 'Gói/năm', 8000000.00, 999, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(11, 2, 'SRV-CUSTOM', 'Dịch vụ tùy biến phần mềm theo yêu cầu', 'Gói', 20000000.00, 999, 0, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(12, 2, 'SRV-MIGRATE', 'Dịch vụ chuyển đổi dữ liệu từ hệ thống cũ', 'Gói', 15000000.00, 999, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(13, 2, 'SRV-CONSULT', 'Dịch vụ tư vấn giải pháp CNTT', 'Giờ', 1500000.00, 999, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00'),
	(14, 2, 'SRV-SUPPORT-VIP', 'Dịch vụ hỗ trợ ưu tiên (gói VIP)', 'Gói/năm', 25000000.00, 999, 1, '2026-05-01 08:00:00', '2026-06-24 10:00:00');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.BH_SanPham_HinhAnh
CREATE TABLE IF NOT EXISTS `BH_SanPham_HinhAnh` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `SanPham_Id` int unsigned DEFAULT NULL,
  `UrlHinhAnh` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsMain` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `fk_sphinhanh_sp` (`SanPham_Id`),
  CONSTRAINT `fk_sphinhanh_sp` FOREIGN KEY (`SanPham_Id`) REFERENCES `BH_SanPham` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.BH_SanPham_HinhAnh: ~0 rows (xấp xỉ)

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_BaoGia
CREATE TABLE IF NOT EXISTS `HD_BaoGia` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaBaoGia` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `TongTien` decimal(18,2) DEFAULT '0.00',
  `TrangThai` enum('Nhap','DaGui','TuChoi','ChapNhan') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'Nhap',
  `NhanVien_Id` int unsigned DEFAULT NULL,
  `LyDoTuChoi` varchar(255) COLLATE utf8mb4_bin DEFAULT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaBaoGia` (`MaBaoGia`),
  KEY `fk_bg_kh` (`KhachHang_Id`),
  CONSTRAINT `fk_bg_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HD_BaoGia: ~17 rows (xấp xỉ)
INSERT INTO `HD_BaoGia` (`Id`, `MaBaoGia`, `KhachHang_Id`, `TongTien`, `TrangThai`, `NhanVien_Id`, `LyDoTuChoi`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'BG2026-001', 1, 45000000.00, 'ChapNhan', 3, NULL, '2026-05-04 09:00:00', '2026-05-09 16:00:00'),
	(2, 'BG2026-002', 2, 15000000.00, 'DaGui', 4, NULL, '2026-05-06 10:00:00', '2026-05-06 15:00:00'),
	(3, 'BG2026-003', 4, 8000000.00, 'Nhap', 2, NULL, '2026-06-18 08:00:00', '2026-06-18 08:00:00'),
	(4, 'BG2026-004', 10, 60000000.00, 'ChapNhan', 5, NULL, '2026-06-05 09:00:00', '2026-06-14 17:00:00'),
	(5, 'BG2026-005', 7, 50000000.00, 'ChapNhan', 5, NULL, '2026-05-16 09:00:00', '2026-05-19 16:00:00'),
	(6, 'BG2026-006', 9, 15000000.00, 'TuChoi', 4, NULL, '2026-05-16 09:00:00', '2026-06-08 09:00:00'),
	(7, 'BG2026-007', 11, 12000000.00, 'DaGui', 6, NULL, '2026-05-20 09:00:00', '2026-05-20 15:00:00'),
	(8, 'BG2026-008', 12, 9000000.00, 'Nhap', 4, NULL, '2026-06-02 09:30:00', '2026-06-02 09:30:00'),
	(9, 'BG2026-009', 13, 70000000.00, 'ChapNhan', 3, NULL, '2026-05-13 09:00:00', '2026-05-24 16:00:00'),
	(10, 'BG2026-010', 15, 16000000.00, 'DaGui', 5, NULL, '2026-05-14 09:00:00', '2026-05-14 15:00:00'),
	(11, 'BG2026-011', 16, 16000000.00, 'ChapNhan', 6, NULL, '2026-05-30 09:00:00', '2026-06-01 16:00:00'),
	(12, 'BG2026-012', 17, 55000000.00, 'DaGui', 3, NULL, '2026-05-17 09:00:00', '2026-05-17 15:00:00'),
	(13, 'BG2026-013', 19, 25000000.00, 'Nhap', 5, NULL, '2026-05-26 09:00:00', '2026-05-26 09:00:00'),
	(14, 'BG2026-014', 20, 8000000.00, 'TuChoi', 6, NULL, '2026-05-20 09:00:00', '2026-06-01 10:00:00'),
	(15, 'BG2026-015', 23, 15000000.00, 'ChapNhan', 5, NULL, '2026-06-19 09:00:00', '2026-06-19 15:00:00'),
	(16, 'BG2026-016', 25, 70000000.00, 'Nhap', 3, NULL, '2026-06-28 09:00:00', '2026-06-28 09:00:00'),
	(17, 'BG2026-017', 22, 10500000.00, 'DaGui', 4, NULL, '2026-06-06 09:00:00', '2026-06-06 15:00:00'),
	(18, 'BG00018', 26, 1500000.00, 'ChapNhan', 2, NULL, '2026-07-08 11:54:31', '2026-07-08 11:54:57');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_BaoGia_ChiTiet
CREATE TABLE IF NOT EXISTS `HD_BaoGia_ChiTiet` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `BaoGia_Id` bigint unsigned NOT NULL,
  `SanPham_Id` int unsigned NOT NULL,
  `SoLuong` int NOT NULL DEFAULT (0),
  `DonGia` decimal(18,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `fk_bgct_bg` (`BaoGia_Id`),
  KEY `fk_bgct_sp` (`SanPham_Id`),
  CONSTRAINT `fk_bgct_bg` FOREIGN KEY (`BaoGia_Id`) REFERENCES `HD_BaoGia` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_bgct_sp` FOREIGN KEY (`SanPham_Id`) REFERENCES `BH_SanPham` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HD_BaoGia_ChiTiet: ~28 rows (xấp xỉ)
INSERT INTO `HD_BaoGia_ChiTiet` (`Id`, `BaoGia_Id`, `SanPham_Id`, `SoLuong`, `DonGia`) VALUES
	(1, 1, 2, 2, 15000000.00),
	(2, 1, 8, 1, 10000000.00),
	(3, 1, 1, 1, 5000000.00),
	(4, 2, 1, 1, 5000000.00),
	(5, 2, 8, 1, 10000000.00),
	(6, 3, 10, 1, 8000000.00),
	(7, 4, 4, 1, 30000000.00),
	(8, 4, 8, 3, 10000000.00),
	(9, 5, 3, 1, 45000000.00),
	(10, 5, 1, 1, 5000000.00),
	(11, 6, 1, 1, 5000000.00),
	(12, 6, 8, 1, 10000000.00),
	(13, 7, 6, 1, 12000000.00),
	(14, 8, 13, 6, 1500000.00),
	(15, 9, 5, 1, 60000000.00),
	(16, 9, 8, 1, 10000000.00),
	(17, 10, 7, 2, 8000000.00),
	(18, 11, 7, 2, 8000000.00),
	(19, 12, 3, 1, 45000000.00),
	(20, 12, 8, 1, 10000000.00),
	(21, 13, 12, 1, 15000000.00),
	(22, 13, 8, 1, 10000000.00),
	(23, 14, 7, 1, 8000000.00),
	(24, 15, 2, 1, 15000000.00),
	(25, 16, 5, 1, 60000000.00),
	(26, 16, 8, 1, 10000000.00),
	(27, 17, 9, 3, 3000000.00),
	(28, 17, 13, 1, 1500000.00),
	(29, 18, 13, 1, 1500000.00);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_HopDong
CREATE TABLE IF NOT EXISTS `HD_HopDong` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaHopDong` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `BaoGia_Id` bigint unsigned DEFAULT NULL,
  `NgayKy` date DEFAULT NULL,
  `ThoiHan` int DEFAULT NULL,
  `TrangThai` enum('DangThucHien','TamDung','ThanhLy') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'DangThucHien',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaHopDong` (`MaHopDong`),
  KEY `fk_hdong_kh` (`KhachHang_Id`),
  KEY `fk_hdong_baogia` (`BaoGia_Id`),
  CONSTRAINT `fk_hdong_baogia` FOREIGN KEY (`BaoGia_Id`) REFERENCES `HD_BaoGia` (`Id`),
  CONSTRAINT `fk_hdong_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HD_HopDong: ~10 rows (xấp xỉ)
INSERT INTO `HD_HopDong` (`Id`, `MaHopDong`, `KhachHang_Id`, `BaoGia_Id`, `NgayKy`, `ThoiHan`, `TrangThai`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'HD2026-001', 1, 1, '2026-05-10', 12, 'DangThucHien', '2026-05-10 10:00:00', '2026-05-10 10:00:00'),
	(2, 'HD2026-002', 10, 4, '2026-06-15', 12, 'DangThucHien', '2026-06-15 10:00:00', '2026-06-15 10:00:00'),
	(3, 'HD2026-006', 7, 5, '2026-05-20', 12, 'DangThucHien', '2026-05-20 10:00:00', '2026-05-20 10:00:00'),
	(4, 'HD2026-007', 13, 9, '2026-05-25', 24, 'DangThucHien', '2026-05-25 10:00:00', '2026-05-25 10:00:00'),
	(5, 'HD2026-009', 16, 11, '2026-06-02', 12, 'DangThucHien', '2026-06-02 10:00:00', '2026-06-02 10:00:00'),
	(6, 'HD2026-008', 23, 15, '2026-06-19', 12, 'DangThucHien', '2026-06-19 10:00:00', '2026-06-19 10:00:00'),
	(7, 'HD2025-018', 2, NULL, '2025-07-01', 6, 'ThanhLy', '2025-07-01 09:00:00', '2026-01-05 09:00:00'),
	(8, 'HD2026-003', 5, NULL, '2026-06-20', 12, 'TamDung', '2026-06-20 09:00:00', '2026-06-28 10:00:00'),
	(9, 'HD2025-022', 19, NULL, '2025-09-15', 12, 'ThanhLy', '2025-09-15 09:00:00', '2026-04-10 09:00:00'),
	(10, 'HD2025-030', 15, NULL, '2025-11-01', 6, 'TamDung', '2025-11-01 09:00:00', '2026-05-01 09:00:00'),
	(11, 'HD00011', 26, 18, '2026-07-08', 12, 'DangThucHien', '2026-07-08 11:55:11', '2026-07-08 11:55:11');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_ChucVu
CREATE TABLE IF NOT EXISTS `HT_ChucVu` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenChucVu` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `TenChucVu` (`TenChucVu`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HT_ChucVu: ~3 rows (xấp xỉ)
INSERT INTO `HT_ChucVu` (`Id`, `TenChucVu`, `IsActive`) VALUES
	(1, 'Giám Đốc', 1),
	(2, 'Trưởng Phòng', 1),
	(3, 'Nhân Viên', 1);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_PhongBan
CREATE TABLE IF NOT EXISTS `HT_PhongBan` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenPhongBan` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `TenPhongBan` (`TenPhongBan`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HT_PhongBan: ~4 rows (xấp xỉ)
INSERT INTO `HT_PhongBan` (`Id`, `TenPhongBan`, `IsActive`) VALUES
	(1, 'Ban Giám Đốc', 1),
	(2, 'Phòng Kinh Doanh', 1),
	(3, 'Phòng Kế Toán', 1),
	(4, 'Phòng Kỹ Thuật', 1);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_Role
CREATE TABLE IF NOT EXISTS `HT_Role` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `TenRole` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `MoTa` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HT_Role: ~4 rows (xấp xỉ)
INSERT INTO `HT_Role` (`Id`, `TenRole`, `MoTa`) VALUES
	(1, 'Admin', 'Quản trị viên hệ thống'),
	(2, 'Manager', 'Quản lý kinh doanh'),
	(3, 'Sale', 'Nhân viên kinh doanh'),
	(4, 'Accountant', 'Nhân viên kế toán');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_ThongTinNhanSu
CREATE TABLE IF NOT EXISTS `HT_ThongTinNhanSu` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `HoTen` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `SoDienThoai` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `PhongBan_Id` smallint unsigned DEFAULT NULL,
  `ChucVu_Id` smallint unsigned DEFAULT NULL,
  `TrangThai` tinyint(1) DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Email` (`Email`),
  KEY `fk_ns_phongban` (`PhongBan_Id`),
  KEY `fk_ns_chucvu` (`ChucVu_Id`),
  CONSTRAINT `fk_ns_chucvu` FOREIGN KEY (`ChucVu_Id`) REFERENCES `HT_ChucVu` (`Id`),
  CONSTRAINT `fk_ns_phongban` FOREIGN KEY (`PhongBan_Id`) REFERENCES `HT_PhongBan` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HT_ThongTinNhanSu: ~13 rows (xấp xỉ)
INSERT INTO `HT_ThongTinNhanSu` (`Id`, `HoTen`, `Email`, `SoDienThoai`, `PhongBan_Id`, `ChucVu_Id`, `TrangThai`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'Nguyễn Văn An', 'an.nguyen@techsol.vn', '0901234567', 1, 1, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(2, 'Trần Thị Bích Hà', 'ha.tran@techsol.vn', '0987654321', 2, 2, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(3, 'Lê Hoàng Nam', 'nam.le@techsol.vn', '0911222333', 2, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(4, 'Phạm Thị Ngọc Mai', 'mai.pham@techsol.vn', '0922333444', 2, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(5, 'Đỗ Minh Quân', 'quan.do@techsol.vn', '0933444555', 2, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(6, 'Ngô Thị Kim Ngân', 'ngan.ngo@techsol.vn', '0955112233', 2, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(7, 'Vũ Thị Thu Hương', 'huong.vu@techsol.vn', '0944555666', 3, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(8, 'Hoàng Văn Đức', 'duc.hoang@techsol.vn', '0955666777', 3, 2, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(9, 'Đặng Thị Mỹ Linh', 'linh.dang@techsol.vn', '0966887799', 3, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(10, 'Bùi Anh Tuấn', 'tuan.bui@techsol.vn', '0966777888', 4, 3, 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(11, 'Võ Văn Nhân', 'test01@gmail.com', '0901234567', 2, 2, 1, '2026-07-09 01:05:26', '2026-07-09 01:05:26'),
	(12, 'Nhân', 'test02@gmail.com', '0123333333', 3, 3, 1, '2026-07-09 01:06:37', '2026-07-09 01:06:37'),
	(13, 'Nhân Nhân Nhân', 'test03@gmail.com', '0933444555', 3, 3, 1, '2026-07-09 01:07:33', '2026-07-09 01:07:33');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_User
CREATE TABLE IF NOT EXISTS `HT_User` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `NhanSu_Id` int unsigned DEFAULT NULL,
  `Username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Role_Id` int unsigned DEFAULT NULL,
  `TrangThai` enum('Active','Locked','Inactive') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'Active',
  `TokenVersion` int NOT NULL DEFAULT '0',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Username` (`Username`),
  UNIQUE KEY `NhanSu_Id` (`NhanSu_Id`),
  KEY `fk_user_role` (`Role_Id`),
  CONSTRAINT `fk_user_nhansu` FOREIGN KEY (`NhanSu_Id`) REFERENCES `HT_ThongTinNhanSu` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_user_role` FOREIGN KEY (`Role_Id`) REFERENCES `HT_Role` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.HT_User: ~12 rows (xấp xỉ)
INSERT INTO `HT_User` (`Id`, `NhanSu_Id`, `Username`, `Password`, `Role_Id`, `TrangThai`, `TokenVersion`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 1, 'admin', '123456', 1, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:21:43'),
	(2, 2, 'manager.ha', '123456', 2, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:21:53'),
	(3, 3, 'sale.nam', '123456', 3, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:21:59'),
	(4, 4, 'sale.mai', '123456', 3, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:22:05'),
	(5, 5, 'sale.quan', '123456', 3, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:22:10'),
	(6, 6, 'sale.ngan', '123456', 3, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:22:15'),
	(7, 7, 'ketoan.huong', '123456', 4, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:22:19'),
	(8, 8, 'ketoan.duc', '123456', 4, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:22:23'),
	(9, 9, 'ketoan.linh', '123456', 4, 'Active', 0, '2026-05-01 08:00:00', '2026-07-05 07:22:27'),
	(10, 11, 'test01', '$2a$11$aoVjffgPuaVzZVw0Y4JL..phTXQce44QI67rA8h0I6.gMiQHlYOkG', 2, 'Active', 0, '2026-07-09 01:05:26', '2026-07-09 01:05:26'),
	(11, 12, 'test02', '$2a$11$SRfqY9r4XX.0gBYy.7z/puUtlD6Ilp5Aa7y7l0bH8erXBjUBLiT5.', 3, 'Active', 0, '2026-07-09 01:06:37', '2026-07-09 01:06:37'),
	(12, 13, 'test03', '$2a$11$Nf6FqL5UjZYQz8OTGA3LqezsYef5FkCA0lNRgq1LCONDo4rQ3Wqkq', 4, 'Locked', 2, '2026-07-09 01:07:33', '2026-07-09 01:25:38');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KT_HoaDon
CREATE TABLE IF NOT EXISTS `KT_HoaDon` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaHoaDon` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `HopDong_Id` bigint unsigned DEFAULT NULL,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `TongTien` decimal(18,2) NOT NULL,
  `SoTienDaThu` decimal(18,2) DEFAULT '0.00',
  `TrangThaiThanhToan` enum('ChuaThanhToan','ThanhToan1Phan','HoanTat') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'ChuaThanhToan',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaHoaDon` (`MaHoaDon`),
  KEY `fk_hdon_kh` (`KhachHang_Id`),
  KEY `fk_hdon_hopdong` (`HopDong_Id`),
  CONSTRAINT `fk_hdon_hopdong` FOREIGN KEY (`HopDong_Id`) REFERENCES `HD_HopDong` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_hdon_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KT_HoaDon: ~8 rows (xấp xỉ)
INSERT INTO `KT_HoaDon` (`Id`, `MaHoaDon`, `HopDong_Id`, `KhachHang_Id`, `TongTien`, `SoTienDaThu`, `TrangThaiThanhToan`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'INV2026-001', 1, 1, 45000000.00, 45000000.00, 'HoanTat', '2026-05-11 09:00:00', '2026-05-15 14:00:00'),
	(2, 'INV2026-002', 2, 10, 60000000.00, 30000000.00, 'ThanhToan1Phan', '2026-06-16 09:00:00', '2026-06-20 11:00:00'),
	(3, 'INV2026-003', 3, 7, 50000000.00, 50000000.00, 'HoanTat', '2026-05-21 09:00:00', '2026-05-28 14:00:00'),
	(4, 'INV2026-004', 4, 13, 70000000.00, 35000000.00, 'ThanhToan1Phan', '2026-05-26 09:00:00', '2026-06-10 11:00:00'),
	(5, 'INV2026-005', 5, 16, 16000000.00, 16000000.00, 'HoanTat', '2026-06-03 09:00:00', '2026-06-08 14:00:00'),
	(6, 'INV2026-006', 6, 23, 15000000.00, 0.00, 'ChuaThanhToan', '2026-06-20 09:00:00', '2026-06-20 09:00:00'),
	(7, 'INV2025-014', 7, 2, 18000000.00, 18000000.00, 'HoanTat', '2025-07-05 09:00:00', '2025-07-20 10:00:00'),
	(8, 'INV2025-019', 9, 19, 22000000.00, 22000000.00, 'HoanTat', '2025-09-20 09:00:00', '2025-10-05 10:00:00'),
	(9, 'INV-20260708-5E8384', 11, 26, 1.00, 0.00, 'ChuaThanhToan', '2026-07-08 11:56:45', '2026-07-08 11:56:45');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KT_PhieuThuChi
CREATE TABLE IF NOT EXISTS `KT_PhieuThuChi` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaPhieu` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `LoaiPhieu` enum('Thu','Chi') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `KhachHang_Id` bigint unsigned DEFAULT NULL,
  `HoaDon_Id` bigint unsigned DEFAULT NULL,
  `SoTien` decimal(18,2) NOT NULL,
  `NguoiLap_Id` int unsigned DEFAULT NULL,
  `NgayTao` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaPhieu` (`MaPhieu`),
  KEY `fk_ptc_kh` (`KhachHang_Id`),
  KEY `fk_ptc_hdon` (`HoaDon_Id`),
  KEY `fk_ptc_user` (`NguoiLap_Id`),
  CONSTRAINT `fk_ptc_hdon` FOREIGN KEY (`HoaDon_Id`) REFERENCES `KT_HoaDon` (`Id`),
  CONSTRAINT `fk_ptc_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`),
  CONSTRAINT `fk_ptc_user` FOREIGN KEY (`NguoiLap_Id`) REFERENCES `HT_User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KT_PhieuThuChi: ~10 rows (xấp xỉ)
INSERT INTO `KT_PhieuThuChi` (`Id`, `MaPhieu`, `LoaiPhieu`, `KhachHang_Id`, `HoaDon_Id`, `SoTien`, `NguoiLap_Id`, `NgayTao`, `UpdatedAt`) VALUES
	(1, 'PT2026-001', 'Thu', 1, 1, 45000000.00, 7, '2026-05-15 14:00:00', '2026-05-15 14:00:00'),
	(2, 'PT2026-002', 'Thu', 10, 2, 30000000.00, 7, '2026-06-20 11:00:00', '2026-06-20 11:00:00'),
	(3, 'PT2026-003', 'Thu', 7, 3, 50000000.00, 8, '2026-05-28 14:00:00', '2026-05-28 14:00:00'),
	(4, 'PT2026-004', 'Thu', 13, 4, 20000000.00, 7, '2026-06-01 11:00:00', '2026-06-01 11:00:00'),
	(5, 'PT2026-005', 'Thu', 13, 4, 15000000.00, 8, '2026-06-10 11:00:00', '2026-06-10 11:00:00'),
	(6, 'PT2026-006', 'Thu', 16, 5, 16000000.00, 9, '2026-06-08 14:00:00', '2026-06-08 14:00:00'),
	(7, 'PT2025-009', 'Thu', 2, 7, 18000000.00, 8, '2025-07-20 10:00:00', '2025-07-20 10:00:00'),
	(8, 'PT2025-013', 'Thu', 19, 8, 22000000.00, 7, '2025-10-05 10:00:00', '2025-10-05 10:00:00'),
	(9, 'PC2026-001', 'Chi', 8, NULL, 1500000.00, 7, '2026-06-15 10:00:00', '2026-06-15 10:00:00'),
	(10, 'PC2026-002', 'Chi', 20, NULL, 800000.00, 9, '2026-06-05 10:00:00', '2026-06-05 10:00:00');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_DiaChi
CREATE TABLE IF NOT EXISTS `KH_DiaChi` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `DiaChiChiTiet` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `TinhThanh` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `QuanHuyen` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `PhuongXa` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `LoaiDiaChi` enum('Billing','Shipping','Office') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsDefault` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `fk_dc_kh` (`KhachHang_Id`),
  CONSTRAINT `fk_dc_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_DiaChi: ~18 rows (xấp xỉ)
INSERT INTO `KH_DiaChi` (`Id`, `KhachHang_Id`, `DiaChiChiTiet`, `TinhThanh`, `QuanHuyen`, `PhuongXa`, `LoaiDiaChi`, `IsDefault`) VALUES
	(1, 1, 'Tòa nhà Bitexco Financial Tower', 'Hồ Chí Minh', 'Quận 1', 'Bến Nghé', 'Office', 1),
	(2, 1, 'Lầu 5, Tòa nhà Bitexco (Phòng Kế toán)', 'Hồ Chí Minh', 'Quận 1', 'Bến Nghé', 'Billing', 0),
	(3, 2, '25 Nguyễn Văn Trỗi', 'Hồ Chí Minh', 'Phú Nhuận', 'Phường 8', 'Office', 1),
	(4, 4, '100 Trần Duy Hưng', 'Hà Nội', 'Cầu Giấy', 'Trung Hòa', 'Office', 1),
	(5, 4, 'Kho hàng KCN Bắc Thăng Long', 'Hà Nội', 'Đông Anh', 'Kim Chung', 'Shipping', 1),
	(6, 5, '88 Lê Lợi', 'Hồ Chí Minh', 'Quận 1', 'Bến Thành', 'Office', 1),
	(7, 7, '45 Nguyễn Thị Minh Khai', 'Hồ Chí Minh', 'Quận 3', 'Võ Thị Sáu', 'Office', 1),
	(8, 7, 'Kho bãi Cát Lái', 'Hồ Chí Minh', 'Thành phố Thủ Đức', 'Cát Lái', 'Shipping', 0),
	(9, 9, '12 Nguyễn Trãi', 'Cần Thơ', 'Ninh Kiều', 'An Cư', 'Office', 1),
	(10, 10, '12 Lê Duẩn', 'Hồ Chí Minh', 'Quận 1', 'Bến Nghé', 'Office', 1),
	(11, 11, 'KCN Tân Bình, Lô A5', 'Hồ Chí Minh', 'Tân Bình', 'Tây Thạnh', 'Office', 1),
	(12, 13, '66 Điện Biên Phủ', 'Hồ Chí Minh', 'Bình Thạnh', 'Đa Kao', 'Office', 1),
	(13, 13, 'Kho GDP đạt chuẩn, KCN Vĩnh Lộc', 'Hồ Chí Minh', 'Bình Tân', 'Bình Hưng Hòa', 'Shipping', 1),
	(14, 15, '200 Quốc lộ 1A', 'Hồ Chí Minh', 'Bình Tân', 'Bình Trị Đông', 'Office', 1),
	(15, 17, '18 Lê Thánh Tôn', 'Hồ Chí Minh', 'Quận 1', 'Bến Nghé', 'Office', 1),
	(16, 19, 'KCN Biên Hòa 2', 'Đồng Nai', 'Biên Hòa', 'Long Bình', 'Office', 1),
	(17, 23, '77 Cách Mạng Tháng Tám', 'Hồ Chí Minh', 'Quận 10', 'Phường 4', 'Office', 1),
	(18, 25, '9 Nguyễn Đình Chiểu', 'Hồ Chí Minh', 'Quận 3', 'Võ Thị Sáu', 'Office', 1),
	(19, 26, '280 cao lỗ', 'HCM', '8', '4', 'Office', 1);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_DiemThuong
CREATE TABLE IF NOT EXISTS `KH_DiemThuong` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `SoDiem` int NOT NULL COMMENT 'Dương = cộng điểm (từ phiếu thu), Âm = trừ điểm (khi đổi voucher)',
  `LoaiGiaoDich` enum('MuaHang','DoiVoucher','ThuCong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `HoaDon_Id` bigint unsigned DEFAULT NULL,
  `PhieuThuChi_Id` bigint unsigned DEFAULT NULL COMMENT 'Phiếu thu là nguồn gốc cộng điểm, mỗi phiếu thu chỉ tạo điểm 1 lần',
  `NgayPhatSinh` date NOT NULL COMMENT 'Ngày phát sinh điểm = ngày phiếu thu, dùng để lọc cửa sổ 12 tháng khi tính hạng',
  `GhiChu` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NguoiTao_Id` int unsigned DEFAULT NULL COMMENT 'Nhân viên thực hiện điều chỉnh thủ công, NULL nếu hệ thống tự sinh',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_diemthuong_phieuthu` (`PhieuThuChi_Id`),
  KEY `idx_diemthuong_kh_ngay` (`KhachHang_Id`,`NgayPhatSinh`),
  KEY `fk_diemthuong_hoadon` (`HoaDon_Id`),
  KEY `fk_diemthuong_phieuthu` (`PhieuThuChi_Id`),
  KEY `fk_diemthuong_user` (`NguoiTao_Id`),
  CONSTRAINT `fk_diemthuong_hoadon` FOREIGN KEY (`HoaDon_Id`) REFERENCES `KT_HoaDon` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_diemthuong_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_diemthuong_phieuthu` FOREIGN KEY (`PhieuThuChi_Id`) REFERENCES `KT_PhieuThuChi` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_diemthuong_user` FOREIGN KEY (`NguoiTao_Id`) REFERENCES `HT_User` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Lịch sử tích điểm/trừ điểm, dùng chung cho B2C và B2B, tỷ lệ 100.000 VNĐ = 1 điểm';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_DiemThuong: ~0 rows (xấp xỉ)

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_EmailLog
CREATE TABLE IF NOT EXISTS `KH_EmailLog` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `LoaiEmail` enum('XacNhanThanhToan','ThangHang','XuongHang','CanhBaoXuongHang','SinhNhat','NgayThanhLap','NgayLe','CuoiNam') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Voucher_Id` bigint unsigned DEFAULT NULL,
  `EmailDen` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TieuDe` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TrangThaiGui` enum('ThanhCong','ThatBai') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'ThanhCong',
  `LoiChiTiet` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `idx_emaillog_kh` (`KhachHang_Id`),
  KEY `idx_emaillog_loai_ngay` (`LoaiEmail`,`CreatedAt`),
  KEY `fk_emaillog_voucher` (`Voucher_Id`),
  CONSTRAINT `fk_emaillog_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_emaillog_voucher` FOREIGN KEY (`Voucher_Id`) REFERENCES `KH_Voucher` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Log mọi email đã gửi cho khách, dùng để chống gửi trùng trong cùng tháng/năm và để demo/audit';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_EmailLog: ~16 rows (xấp xỉ)
INSERT INTO `KH_EmailLog` (`Id`, `KhachHang_Id`, `LoaiEmail`, `Voucher_Id`, `EmailDen`, `TieuDe`, `TrangThaiGui`, `LoiChiTiet`, `CreatedAt`) VALUES
	(1, 24, 'SinhNhat', NULL, 'duyen.le88@gmail.com', '[CRM] 🎂 Chúc mừng sinh nhật!', 'ThanhCong', NULL, '2026-07-06 01:13:49'),
	(2, 4, 'CanhBaoXuongHang', NULL, 'contact@dongaco.vn', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:13:54'),
	(3, 1, 'CanhBaoXuongHang', NULL, 'info@giaiphapso.vn', '[CRM] ⚠️ Hạng Vàng của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:13:59'),
	(4, 5, 'CanhBaoXuongHang', NULL, 'huongviet.rest@gmail.com', '[CRM] ⚠️ Hạng Vàng của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:03'),
	(5, 13, 'CanhBaoXuongHang', NULL, 'contact@minhkhangpharma.vn', '[CRM] ⚠️ Hạng Chiến Lược của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:07'),
	(6, 17, 'CanhBaoXuongHang', NULL, 'info@phugiareal.vn', '[CRM] ⚠️ Hạng Kim Cương của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:11'),
	(7, 21, 'CanhBaoXuongHang', NULL, 'info@vietxanhtravel.vn', '[CRM] ⚠️ Hạng Vàng của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:16'),
	(8, 25, 'CanhBaoXuongHang', NULL, 'info@hoabinhmedical.vn', '[CRM] ⚠️ Hạng Vàng của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:20'),
	(9, 8, 'CanhBaoXuongHang', NULL, 'dienmaythanhcong@gmail.com', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:24'),
	(10, 2, 'CanhBaoXuongHang', NULL, 'lienhe@hoangphat.vn', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:28'),
	(11, 9, 'CanhBaoXuongHang', NULL, 'antam.clinic@gmail.com', '[CRM] ⚠️ Hạng Vàng của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:33'),
	(12, 7, 'CanhBaoXuongHang', NULL, 'info@logisticsmiennam.vn', '[CRM] ⚠️ Hạng Kim Cương của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:37'),
	(13, 15, 'CanhBaoXuongHang', NULL, 'vantai.saigon@gmail.com', '[CRM] ⚠️ Hạng Vàng của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:42'),
	(14, 19, 'CanhBaoXuongHang', NULL, 'info@cokhichinhxac.vn', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:46'),
	(15, 23, 'CanhBaoXuongHang', NULL, 'ducanh.kimlong@gmail.com', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:50'),
	(16, 11, 'CanhBaoXuongHang', NULL, 'info@phucuongtextile.vn', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:54'),
	(17, 16, 'CanhBaoXuongHang', NULL, 'binhminh.mart@gmail.com', '[CRM] ⚠️ Hạng Bạc của bạn cần được duy trì', 'ThanhCong', NULL, '2026-07-06 01:14:58');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_HoatDong
CREATE TABLE IF NOT EXISTS `KH_HoatDong` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned DEFAULT NULL,
  `Lead_Id` bigint unsigned DEFAULT NULL,
  `LoaiHoatDong` enum('Call','Meeting','Email','Zalo') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NoiDung` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `ThoiGianThucHien` datetime DEFAULT NULL,
  `NhanVien_Id` int unsigned DEFAULT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `fk_hd_kh` (`KhachHang_Id`),
  KEY `fk_hd_lead` (`Lead_Id`),
  KEY `fk_hd_nv` (`NhanVien_Id`),
  CONSTRAINT `fk_hd_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_hd_lead` FOREIGN KEY (`Lead_Id`) REFERENCES `KH_Lead` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_hd_nv` FOREIGN KEY (`NhanVien_Id`) REFERENCES `HT_User` (`Id`),
  CONSTRAINT `chk_hd_target` CHECK (((`KhachHang_Id` is not null) or (`Lead_Id` is not null)))
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_HoatDong: ~30 rows (xấp xỉ)
INSERT INTO `KH_HoatDong` (`Id`, `KhachHang_Id`, `Lead_Id`, `LoaiHoatDong`, `NoiDung`, `ThoiGianThucHien`, `NhanVien_Id`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, NULL, 1, 'Call', 'Gọi điện giới thiệu giải pháp CRM cho xưởng cơ khí.', '2026-06-10 09:15:00', 3, '2026-06-10 09:15:00', '2026-06-10 09:15:00'),
	(2, NULL, 2, 'Zalo', 'Gửi bảng giá dịch vụ chăm sóc khách hàng qua Zalo.', '2026-06-12 14:30:00', 4, '2026-06-12 14:30:00', '2026-06-12 14:30:00'),
	(3, 1, NULL, 'Meeting', 'Họp trực tiếp ký kết hợp đồng triển khai CRM Pro.', '2026-05-10 10:00:00', 3, '2026-05-10 10:00:00', '2026-05-10 10:00:00'),
	(4, 4, NULL, 'Email', 'Gửi đề xuất gói bảo trì hệ thống hàng năm.', '2026-06-18 08:30:00', 2, '2026-06-18 08:30:00', '2026-06-18 08:30:00'),
	(5, 10, 3, 'Meeting', 'Gặp trực tiếp bàn giao yêu cầu triển khai ERP trước khi chốt hợp đồng.', '2026-06-14 09:30:00', 5, '2026-06-14 09:30:00', '2026-06-14 09:30:00'),
	(6, 7, NULL, 'Call', 'Gọi trao đổi thêm về module quản lý kho vận theo yêu cầu khách.', '2026-05-18 15:00:00', 5, '2026-05-18 15:00:00', '2026-05-18 15:00:00'),
	(7, NULL, 5, 'Call', 'Trao đổi lại, trường đã chọn phần mềm khác do ngân sách hạn chế.', '2026-06-05 11:00:00', 4, '2026-06-05 11:00:00', '2026-06-05 11:00:00'),
	(8, 9, NULL, 'Email', 'Gửi lại báo giá điều chỉnh sau khi khách phản hồi giá cao.', '2026-06-08 09:00:00', 4, '2026-06-08 09:00:00', '2026-06-08 09:00:00'),
	(9, NULL, 4, 'Call', 'Gọi khảo sát nhu cầu quản lý đơn hàng cho xưởng may.', '2026-06-20 10:00:00', 3, '2026-06-20 10:00:00', '2026-06-20 10:00:00'),
	(10, 23, 6, 'Zalo', 'Gửi catalogue dịch vụ qua Zalo theo yêu cầu khách.', '2026-06-18 15:00:00', 5, '2026-06-18 15:00:00', '2026-06-18 15:00:00'),
	(11, 11, NULL, 'Call', 'Gọi trao đổi lại đơn giá gói HRM sau khi khách phản hồi cao.', '2026-06-20 10:00:00', 6, '2026-06-20 10:00:00', '2026-06-20 10:00:00'),
	(12, 12, NULL, 'Email', 'Gửi báo giá sơ bộ phần mềm quản lý học sinh.', '2026-06-02 10:00:00', 4, '2026-06-02 10:00:00', '2026-06-02 10:00:00'),
	(13, 13, NULL, 'Meeting', 'Họp ký hợp đồng CRM chiến lược 24 tháng với khách VIP.', '2026-05-25 09:30:00', 3, '2026-05-25 09:30:00', '2026-05-25 09:30:00'),
	(14, 15, NULL, 'Call', 'Gọi trao đổi thêm về nhu cầu định vị xe cho vận tải.', '2026-06-22 10:00:00', 5, '2026-06-22 10:00:00', '2026-06-22 10:00:00'),
	(15, 16, NULL, 'Meeting', 'Bàn giao và hướng dẫn sử dụng phần mềm POS tại cửa hàng.', '2026-06-02 09:00:00', 6, '2026-06-02 09:00:00', '2026-06-02 09:00:00'),
	(16, 17, NULL, 'Email', 'Gửi lại điều khoản thanh toán theo yêu cầu khách.', '2026-06-27 09:00:00', 3, '2026-06-27 09:00:00', '2026-06-27 09:00:00'),
	(17, 19, NULL, 'Call', 'Trao đổi kỹ thuật về yêu cầu tích hợp máy CNC.', '2026-05-26 10:00:00', 5, '2026-05-26 10:00:00', '2026-05-26 10:00:00'),
	(18, 20, NULL, 'Call', 'Thông báo khách ngừng kinh doanh, hủy dự án.', '2026-06-01 09:30:00', 6, '2026-06-01 09:30:00', '2026-06-01 09:30:00'),
	(19, 21, NULL, 'Meeting', 'Khảo sát nhu cầu quản lý tour du lịch trực tiếp tại văn phòng.', '2026-06-25 09:30:00', 3, '2026-06-25 09:30:00', '2026-06-25 09:30:00'),
	(20, 22, NULL, 'Email', 'Gửi báo giá phần mềm quản lý hội viên gym.', '2026-06-06 09:30:00', 4, '2026-06-06 09:30:00', '2026-06-06 09:30:00'),
	(21, 25, NULL, 'Meeting', 'Khảo sát quy mô hệ thống ERP cho chuỗi cung ứng thiết bị y tế.', '2026-06-27 10:00:00', 3, '2026-06-27 10:00:00', '2026-06-27 10:00:00'),
	(22, 8, NULL, 'Call', 'Gọi lại đề xuất nâng cấp CRM, khách xác nhận ngừng giao dịch.', '2026-06-15 10:00:00', 3, '2026-06-15 10:00:00', '2026-06-15 10:00:00'),
	(23, NULL, 7, 'Call', 'Gọi giới thiệu phần mềm quản lý khách hàng cho spa/thẩm mỹ viện.', '2026-06-25 09:30:00', 6, '2026-06-25 09:30:00', '2026-06-25 09:30:00'),
	(24, NULL, 8, 'Zalo', 'Gửi thông tin dịch vụ triển khai phần mềm vận tải qua Zalo.', '2026-06-15 11:00:00', 3, '2026-06-15 11:00:00', '2026-06-15 11:00:00'),
	(25, NULL, 9, 'Call', 'Gọi khảo sát nhu cầu quản lý bán hàng cho cửa hàng thời trang.', '2026-06-27 14:30:00', 4, '2026-06-27 14:30:00', '2026-06-27 14:30:00'),
	(26, NULL, 10, 'Call', 'Trao đổi lại, xưởng gỗ quyết định tạm hoãn đầu tư phần mềm.', '2026-06-01 10:00:00', 5, '2026-06-01 10:00:00', '2026-06-01 10:00:00'),
	(27, NULL, 11, 'Email', 'Gửi bảng giá gói phần mềm quản lý trung tâm Anh ngữ.', '2026-06-24 11:00:00', 6, '2026-06-24 11:00:00', '2026-06-24 11:00:00'),
	(28, NULL, 12, 'Call', 'Gọi khảo sát nhu cầu quản lý chuỗi cung ứng thực phẩm sạch.', '2026-06-29 09:30:00', 3, '2026-06-29 09:30:00', '2026-06-29 09:30:00'),
	(29, NULL, 13, 'Zalo', 'Gửi catalogue phần mềm quản lý nhà hàng qua Zalo.', '2026-06-26 15:00:00', 4, '2026-06-26 15:00:00', '2026-06-26 15:00:00'),
	(30, NULL, 14, 'Call', 'Gọi khảo sát nhu cầu quản lý garage ô tô.', '2026-06-30 09:30:00', 5, '2026-06-30 09:30:00', '2026-06-30 09:30:00'),
	(31, NULL, 16, 'Call', 'Chào và đàm phán', '2026-07-08 11:43:00', 2, '2026-07-08 11:51:19', NULL),
	(32, NULL, 16, 'Meeting', 'Đi ăn ở STU', '2026-07-08 11:43:00', 2, '2026-07-08 11:51:36', NULL),
	(33, 26, NULL, 'Email', 'liên hệ', '2026-07-08 11:43:00', 2, '2026-07-08 11:52:39', NULL);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_KhachHang
CREATE TABLE IF NOT EXISTS `KH_KhachHang` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaKhachHang` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TenKhachHang` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `LoaiKhachHang_Id` smallint unsigned DEFAULT NULL,
  `HangKhachHang_Id` smallint unsigned DEFAULT '1' COMMENT 'Hạng hiện tại, FK tới KH_XepHang',
  `TinhTrang_Id` smallint unsigned DEFAULT NULL,
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `SoDienThoai` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `MaSoThue` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NgaySinh` date DEFAULT NULL COMMENT 'Ngày sinh, áp dụng cho khách B2C để gửi email sinh nhật',
  `NgayThanhLap` date DEFAULT NULL COMMENT 'Ngày thành lập công ty, áp dụng cho khách B2B để gửi email kỷ niệm',
  `NhanVienPhuTrach_Id` int unsigned DEFAULT NULL,
  `IsDeleted` tinyint(1) DEFAULT '0',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaKhachHang` (`MaKhachHang`),
  KEY `idx_kh_sdt` (`SoDienThoai`),
  KEY `idx_kh_filter` (`NhanVienPhuTrach_Id`,`IsDeleted`,`TinhTrang_Id`),
  KEY `fk_kh_loai` (`LoaiKhachHang_Id`),
  KEY `fk_kh_ttrang` (`TinhTrang_Id`),
  KEY `fk_kh_hang` (`HangKhachHang_Id`),
  FULLTEXT KEY `idx_fts_kh` (`TenKhachHang`,`Email`),
  CONSTRAINT `fk_kh_hang` FOREIGN KEY (`HangKhachHang_Id`) REFERENCES `KH_XepHang` (`Id`),
  CONSTRAINT `fk_kh_loai` FOREIGN KEY (`LoaiKhachHang_Id`) REFERENCES `KH_LoaiKhachHang` (`Id`),
  CONSTRAINT `fk_kh_nv` FOREIGN KEY (`NhanVienPhuTrach_Id`) REFERENCES `HT_User` (`Id`),
  CONSTRAINT `fk_kh_ttrang` FOREIGN KEY (`TinhTrang_Id`) REFERENCES `KH_TinhTrangKhachHang` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_KhachHang: ~25 rows (xấp xỉ)
INSERT INTO `KH_KhachHang` (`Id`, `MaKhachHang`, `TenKhachHang`, `LoaiKhachHang_Id`, `HangKhachHang_Id`, `TinhTrang_Id`, `Email`, `SoDienThoai`, `MaSoThue`, `NgaySinh`, `NgayThanhLap`, `NhanVienPhuTrach_Id`, `IsDeleted`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'KH0001', 'Công ty CP Giải pháp Số Việt', 2, 3, 1, 'info@giaiphapso.vn', '02838123456', '0312345678', NULL, '2018-03-10', 3, 0, '2026-05-02 09:00:00', '2026-05-02 09:00:00'),
	(2, 'KH0002', 'Công ty TNHH Thương mại Hoàng Phát', 2, 2, 1, 'lienhe@hoangphat.vn', '02839988776', '0309876543', NULL, '2012-07-22', 4, 0, '2026-05-02 09:15:00', '2026-05-02 09:15:00'),
	(3, 'KH0003', 'Nguyễn Thị Thanh Thảo', 3, 1, 2, 'thanhthao.nguyen@gmail.com', '0908123456', NULL, '1990-04-12', NULL, 5, 0, '2026-05-20 14:00:00', '2026-05-20 14:00:00'),
	(4, 'KH0004', 'Công ty CP Xây dựng Đông Á', 2, 2, 1, 'contact@dongaco.vn', '02473001122', '0106552211', NULL, '2015-03-20', 2, 0, '2026-05-03 10:00:00', '2026-05-03 10:00:00'),
	(5, 'KH0005', 'Chuỗi Nhà hàng Hương Việt', 2, 3, 1, 'huongviet.rest@gmail.com', '02866889900', '0311224455', NULL, '2019-09-05', 3, 0, '2026-05-06 11:00:00', '2026-05-06 11:00:00'),
	(6, 'KH0006', 'Trần Văn Hòa', 3, 1, 2, 'hoa.tran89@gmail.com', '0977345678', NULL, '1989-11-02', NULL, 4, 0, '2026-05-25 15:00:00', '2026-05-25 15:00:00'),
	(7, 'KH0007', 'Công ty TNHH Logistics Miền Nam', 1, 4, 1, 'info@logisticsmiennam.vn', '02838556677', '0304998877', NULL, '2010-01-15', 5, 0, '2026-05-08 08:45:00', '2026-05-08 08:45:00'),
	(8, 'KH0008', 'Cửa hàng Điện máy Thành Công', 2, 2, 3, 'dienmaythanhcong@gmail.com', '02513322110', '3600112233', NULL, '2016-06-01', 3, 0, '2026-05-04 09:30:00', '2026-05-04 09:30:00'),
	(9, 'KH0009', 'Phòng khám Đa khoa An Tâm', 2, 3, 1, 'antam.clinic@gmail.com', '02513776655', '3701122334', NULL, '2014-02-18', 4, 0, '2026-05-12 09:00:00', '2026-05-12 09:00:00'),
	(10, 'KH0010', 'Công ty CP Đầu tư Ánh Dương', 2, 1, 1, 'hung@anhduong.vn', '0988777666', '0313445566', NULL, '2020-05-01', 5, 0, '2026-06-14 09:30:00', '2026-06-14 09:30:00'),
	(11, 'KH0011', 'Công ty TNHH Dệt may Phú Cường', 2, 2, 1, 'info@phucuongtextile.vn', '02513667788', '3702233445', NULL, '2011-08-14', 6, 0, '2026-05-18 10:00:00', '2026-05-18 10:00:00'),
	(12, 'KH0012', 'Trường Tiểu học Việt Anh', 2, 1, 2, 'vietanh.school@gmail.com', '02438997766', '0107788990', NULL, '2017-09-01', 4, 0, '2026-06-01 09:00:00', '2026-06-01 09:00:00'),
	(13, 'KH0013', 'Công ty CP Dược phẩm Minh Khang', 1, 5, 1, 'contact@minhkhangpharma.vn', '02839112233', '0303344556', NULL, '2008-04-25', 3, 0, '2026-05-09 08:00:00', '2026-05-09 08:00:00'),
	(14, 'KH0014', 'Nguyễn Văn Bảo', 3, 1, 1, 'bao.nguyen92@gmail.com', '0913556677', NULL, '1992-02-14', NULL, 6, 0, '2026-05-22 13:00:00', '2026-05-22 13:00:00'),
	(15, 'KH0015', 'Công ty TNHH Vận tải Sài Gòn', 2, 3, 1, 'vantai.saigon@gmail.com', '02838991122', '0304556677', NULL, '2013-11-11', 5, 0, '2026-05-11 09:00:00', '2026-05-11 09:00:00'),
	(16, 'KH0016', 'Siêu thị Mini Bình Minh', 2, 2, 1, 'binhminh.mart@gmail.com', '02513445566', '3703344556', NULL, '2018-01-20', 6, 0, '2026-05-28 10:00:00', '2026-05-28 10:00:00'),
	(17, 'KH0017', 'Công ty CP Bất động sản Phú Gia', 2, 4, 1, 'info@phugiareal.vn', '02838223344', '0304667788', NULL, '2009-06-30', 3, 0, '2026-05-07 08:30:00', '2026-05-07 08:30:00'),
	(18, 'KH0018', 'Trần Thị Bích Ngọc', 3, 1, 2, 'ngoc.tran95@gmail.com', '0977889911', NULL, '1995-08-20', NULL, 4, 0, '2026-06-02 14:00:00', '2026-06-02 14:00:00'),
	(19, 'KH0019', 'Công ty TNHH Cơ khí Chính Xác', 2, 2, 1, 'info@cokhichinhxac.vn', '02513998877', '3704455667', NULL, '2014-10-05', 5, 0, '2026-05-14 09:00:00', '2026-05-14 09:00:00'),
	(20, 'KH0020', 'Nhà sách Tri Thức Việt', 2, 1, 3, 'trithucviet.books@gmail.com', '02838667799', '0305778899', NULL, '2015-05-15', 6, 0, '2026-05-19 10:00:00', '2026-05-19 10:00:00'),
	(21, 'KH0021', 'Công ty CP Du lịch Việt Xanh', 2, 3, 1, 'info@vietxanhtravel.vn', '02839334455', '0306889900', NULL, '2016-03-08', 3, 0, '2026-05-16 09:00:00', '2026-05-16 09:00:00'),
	(22, 'KH0022', 'Phòng tập Gym Sức Sống', 2, 1, 1, 'gym.sucsong@gmail.com', '02513556688', '3705890011', NULL, '2020-01-10', 4, 0, '2026-06-03 10:00:00', '2026-06-03 10:00:00'),
	(23, 'KH0023', 'Công ty TNHH In ấn Kim Long', 2, 2, 1, 'ducanhhh.kimlonggg@gmail.com', '0966112233', '0307990022', NULL, '2012-12-01', 5, 0, '2026-06-18 14:00:00', '2026-07-08 12:09:37'),
	(24, 'KH0024', 'Lê Thị Mỹ Duyên', 3, 1, 1, 'duyen.le88@gmail.com', '0933667788', NULL, '1988-07-07', NULL, 6, 0, '2026-05-30 11:00:00', '2026-05-30 11:00:00'),
	(25, 'KH0025', 'Công ty CP Thiết bị Y tế Hòa Bình', 1, 3, 1, 'info@hoabinhmedical.vn', '02838445566', '0308001133', NULL, '2011-02-22', 3, 0, '2026-05-13 09:00:00', '2026-05-13 09:00:00'),
	(26, 'KH0026', 'Công ty Nhân Nhân', 2, NULL, 1, 'vovannhan160204@gmail.com', '0123333333', '0311223344', NULL, '2004-08-07', 9, 0, '2026-07-08 11:51:44', '2026-07-08 12:02:39');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_Lead
CREATE TABLE IF NOT EXISTS `KH_Lead` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TenLead` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TenCongTy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `SoDienThoai` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `Email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `TinhTrang` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NhanVienPhuTrach_Id` int unsigned DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `fk_lead_nv` (`NhanVienPhuTrach_Id`),
  FULLTEXT KEY `idx_fts_lead` (`TenLead`,`TenCongTy`),
  CONSTRAINT `fk_lead_nv` FOREIGN KEY (`NhanVienPhuTrach_Id`) REFERENCES `HT_User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_Lead: ~15 rows (xấp xỉ)
INSERT INTO `KH_Lead` (`Id`, `TenLead`, `TenCongTy`, `SoDienThoai`, `Email`, `TinhTrang`, `NhanVienPhuTrach_Id`, `IsDeleted`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'Anh Tuấn', 'Công ty TNHH Cơ khí Phương Nam', '0901112222', 'tuan.pn@gmail.com', 'Moi', 3, 0, '2026-06-10 09:00:00', '2026-06-10 09:00:00'),
	(2, 'Chị Lan', 'Spa Hương Sen', '0933444555', 'lan.huongsen@gmail.com', 'DangChamSoc', 4, 0, '2026-06-08 13:00:00', '2026-06-12 14:30:00'),
	(3, 'Giám đốc Hùng', 'Công ty CP Đầu tư Ánh Dương', '0988777666', 'hung@anhduong.vn', 'DaChuyenDoi', 5, 0, '2026-06-01 08:00:00', '2026-06-14 09:30:00'),
	(4, 'Anh Khoa', 'Xưởng May Tiến Đạt', '0912334455', 'khoa.tiendat@gmail.com', 'Moi', 3, 0, '2026-06-20 10:00:00', '2026-06-20 10:00:00'),
	(5, 'Chị Hạnh', 'Trường Mầm non Ánh Sao', '0977889900', 'hanh.anhsao@gmail.com', 'ThatBai', 4, 0, '2026-05-28 10:00:00', '2026-06-05 11:00:00'),
	(6, 'Anh Đức Anh', 'Công ty TNHH In ấn Kim Long', '0966112233', 'ducanh.kimlong@gmail.com', 'DaChuyenDoi', 5, 0, '2026-06-18 14:00:00', '2026-06-18 14:20:00'),
	(7, 'Chị Thu', 'Thẩm mỹ viện Ngọc Trinh', '0911998877', 'thu.ngoctrinh@gmail.com', 'Moi', 6, 0, '2026-06-25 09:00:00', '2026-06-25 09:00:00'),
	(8, 'Anh Hải', 'Công ty Vận tải Đông Tây', '0922556677', 'hai.dongtay@gmail.com', 'DangChamSoc', 3, 0, '2026-06-05 10:00:00', '2026-06-15 11:00:00'),
	(9, 'Chị Nga', 'Cửa hàng Thời trang Nga Nguyễn', '0933778899', 'nga.thoitrang@gmail.com', 'Moi', 4, 0, '2026-06-27 14:00:00', '2026-06-27 14:00:00'),
	(10, 'Anh Tùng', 'Xưởng Gỗ Tùng Phát', '0944889900', 'tung.tungphat@gmail.com', 'ThatBai', 5, 0, '2026-05-20 09:00:00', '2026-06-01 10:00:00'),
	(11, 'Chị Yến', 'Trung tâm Anh ngữ Bright', '0955990011', 'yen.bright@gmail.com', 'DangChamSoc', 6, 0, '2026-06-12 10:00:00', '2026-06-24 11:00:00'),
	(12, 'Anh Phong', 'Công ty CP Thực phẩm Sạch Xanh', '0966001122', 'phong.sachxanh@gmail.com', 'Moi', 3, 0, '2026-06-29 09:00:00', '2026-06-29 09:00:00'),
	(13, 'Chị Thủy', 'Nhà hàng Hải Sản Biển Đông', '0977112233', 'thuy.biendong@gmail.com', 'DangChamSoc', 4, 0, '2026-06-14 10:00:00', '2026-06-26 15:00:00'),
	(14, 'Anh Sơn', 'Garage Ô tô Thành Đạt', '0988223344', 'son.thanhdat@gmail.com', 'Moi', 5, 0, '2026-06-30 09:00:00', '2026-06-30 09:00:00'),
	(15, 'Chị Hoa', 'Xưởng In Bao Bì Hoa Việt', '0999334455', 'hoa.hoaviet@gmail.com', 'ThatBai', 6, 0, '2026-05-25 09:00:00', '2026-06-10 10:00:00'),
	(16, 'Nhân', 'Công ty Nhân Nhân', '0123333333', 'vovannhan160204@gmail.com', 'DaChuyenDoi', 9, 1, '2026-07-08 11:50:59', '2026-07-09 01:30:00');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_LichSuHang
CREATE TABLE IF NOT EXISTS `KH_LichSuHang` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `HangCu_Id` smallint unsigned DEFAULT NULL,
  `HangMoi_Id` smallint unsigned NOT NULL,
  `LyDo` enum('TuDongDuDieuKien','TuDongXuongHang','AdminGanThuCong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `GhiChu` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NguoiThucHien_Id` int unsigned DEFAULT NULL COMMENT 'NULL nếu hệ thống tự động xử lý',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `idx_lichsuhang_kh` (`KhachHang_Id`),
  KEY `fk_lichsuhang_hangcu` (`HangCu_Id`),
  KEY `fk_lichsuhang_hangmoi` (`HangMoi_Id`),
  KEY `fk_lichsuhang_user` (`NguoiThucHien_Id`),
  CONSTRAINT `fk_lichsuhang_hangcu` FOREIGN KEY (`HangCu_Id`) REFERENCES `KH_XepHang` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_lichsuhang_hangmoi` FOREIGN KEY (`HangMoi_Id`) REFERENCES `KH_XepHang` (`Id`),
  CONSTRAINT `fk_lichsuhang_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_lichsuhang_user` FOREIGN KEY (`NguoiThucHien_Id`) REFERENCES `HT_User` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Lịch sử thay đổi hạng của khách hàng, dùng để truy vết và làm minh chứng khi cần';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_LichSuHang: ~0 rows (xấp xỉ)

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_LoaiKhachHang
CREATE TABLE IF NOT EXISTS `KH_LoaiKhachHang` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenLoai` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `MoTa` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_LoaiKhachHang: ~3 rows (xấp xỉ)
INSERT INTO `KH_LoaiKhachHang` (`Id`, `TenLoai`, `MoTa`, `IsActive`) VALUES
	(1, 'VIP', 'Khách hàng chiến lược', 1),
	(2, 'B2B', 'Doanh nghiệp', 1),
	(3, 'B2C', 'Cá nhân', 1);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_NgayLe
CREATE TABLE IF NOT EXISTS `KH_NgayLe` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenNgayLe` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `Thang` tinyint unsigned NOT NULL COMMENT 'Tháng diễn ra (1-12)',
  `Ngay` tinyint unsigned NOT NULL COMMENT 'Ngày diễn ra (1-31)',
  `SoNgayGuiTruoc` tinyint unsigned NOT NULL DEFAULT '5' COMMENT 'Gửi email trước bao nhiêu ngày',
  `ApDungChoLoaiKH` enum('B2C','B2B','TatCa') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'TatCa',
  `HangToiThieuApDung` smallint unsigned DEFAULT NULL COMMENT 'FK tới KH_XepHang.Id, NULL = áp dụng mọi hạng',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `fk_ngayle_hang` (`HangToiThieuApDung`),
  CONSTRAINT `fk_ngayle_hang` FOREIGN KEY (`HangToiThieuApDung`) REFERENCES `KH_XepHang` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Cấu hình ngày lễ/sự kiện để hệ thống tự gửi email ưu đãi, admin tự thêm/sửa/xóa được';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_NgayLe: ~4 rows (xấp xỉ)
INSERT INTO `KH_NgayLe` (`Id`, `TenNgayLe`, `Thang`, `Ngay`, `SoNgayGuiTruoc`, `ApDungChoLoaiKH`, `HangToiThieuApDung`, `IsActive`, `CreatedAt`) VALUES
	(1, 'Tết Dương Lịch', 7, 9, 5, 'TatCa', NULL, 1, '2026-05-01 08:00:00'),
	(2, 'Quốc tế Phụ nữ 8/3', 3, 8, 5, 'B2C', NULL, 1, '2026-05-01 08:00:00'),
	(3, 'Ngày Doanh nhân Việt Nam', 10, 13, 5, 'B2B', NULL, 1, '2026-05-01 08:00:00'),
	(4, 'Tổng kết cuối năm', 12, 15, 10, 'TatCa', 3, 1, '2026-05-01 08:00:00'),
	(5, 'BlackFriday', 1, 31, 5, 'B2C', 2, 1, '2026-07-05 07:23:28');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_TinhTrangKhachHang
CREATE TABLE IF NOT EXISTS `KH_TinhTrangKhachHang` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenTinhTrang` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_TinhTrangKhachHang: ~3 rows (xấp xỉ)
INSERT INTO `KH_TinhTrangKhachHang` (`Id`, `TenTinhTrang`, `IsActive`) VALUES
	(1, 'Đang giao dịch', 1),
	(2, 'Tiềm năng', 1),
	(3, 'Ngừng giao dịch', 1);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_Voucher
CREATE TABLE IF NOT EXISTS `KH_Voucher` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaVoucher` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `LoaiGiamGia` enum('PhanTram','SoTienCoDinh') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `GiaTriGiam` decimal(18,2) NOT NULL COMMENT 'PhanTram thì lưu % (vd 5), SoTienCoDinh thì lưu số tiền VNĐ',
  `GiaTriGiamToiDa` decimal(18,2) DEFAULT NULL COMMENT 'Giới hạn số tiền giảm tối đa khi LoaiGiamGia = PhanTram',
  `NgayBatDau` date NOT NULL,
  `NgayHetHan` date NOT NULL,
  `LyDoPhatHanh` enum('ThangHang','SinhNhat','NgayThanhLap','NgayLe','CuoiNam','ThuCong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `LichSuHang_Id` bigint unsigned DEFAULT NULL COMMENT 'Nếu phát do thăng hạng, liên kết bản ghi lịch sử hạng tương ứng',
  `TrangThaiYeuCau` enum('ChuaYeuCau','DaYeuCau') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'ChuaYeuCau' COMMENT 'Khách đã bấm link xác nhận sử dụng trong email chưa',
  `Ticket_Id` bigint unsigned DEFAULT NULL COMMENT 'Ticket hệ thống tự tạo khi khách bấm link sử dụng voucher',
  `IsUsed` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Đã được nhân viên áp dụng vào báo giá/hóa đơn thực tế chưa',
  `AppliedTo_BaoGia_Id` bigint unsigned DEFAULT NULL,
  `NgaySuDung` timestamp NULL DEFAULT NULL,
  `NguoiApDung_Id` int unsigned DEFAULT NULL COMMENT 'Nhân viên đã nhập mã voucher vào báo giá',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_voucher_ma` (`MaVoucher`),
  UNIQUE KEY `uq_voucher_ticket` (`Ticket_Id`),
  KEY `idx_voucher_kh` (`KhachHang_Id`),
  KEY `idx_voucher_trangthai` (`IsUsed`,`NgayHetHan`),
  KEY `fk_voucher_lichsuhang` (`LichSuHang_Id`),
  KEY `fk_voucher_baogia` (`AppliedTo_BaoGia_Id`),
  KEY `fk_voucher_nguoiapdung` (`NguoiApDung_Id`),
  CONSTRAINT `fk_voucher_baogia` FOREIGN KEY (`AppliedTo_BaoGia_Id`) REFERENCES `HD_BaoGia` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_voucher_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_voucher_lichsuhang` FOREIGN KEY (`LichSuHang_Id`) REFERENCES `KH_LichSuHang` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_voucher_nguoiapdung` FOREIGN KEY (`NguoiApDung_Id`) REFERENCES `HT_User` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_voucher_ticket` FOREIGN KEY (`Ticket_Id`) REFERENCES `TK_Ticket` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `chk_voucher_giatri` CHECK ((`GiaTriGiam` > 0)),
  CONSTRAINT `chk_voucher_ngay` CHECK ((`NgayHetHan` >= `NgayBatDau`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Voucher phát cho khách hàng qua email, khách bấm link tạo yêu cầu, nhân viên xử lý qua Ticket';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_Voucher: ~0 rows (xấp xỉ)

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_Voucher_Token
CREATE TABLE IF NOT EXISTS `KH_Voucher_Token` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `Voucher_Id` bigint unsigned NOT NULL,
  `Token` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT 'Token ngẫu nhiên duy nhất, nhúng vào link trong email',
  `NgayHetHanToken` datetime NOT NULL,
  `DaSuDung` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Chống bấm link xử lý 2 lần',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_voucher_token` (`Token`),
  KEY `fk_vouchertoken_voucher` (`Voucher_Id`),
  CONSTRAINT `fk_vouchertoken_voucher` FOREIGN KEY (`Voucher_Id`) REFERENCES `KH_Voucher` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Token bảo mật cho link sử dụng voucher trong email, tách riêng để không bị đoán mã qua Id tuần tự';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_Voucher_Token: ~0 rows (xấp xỉ)

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_XepHang
CREATE TABLE IF NOT EXISTS `KH_XepHang` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `MaHang` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT 'DONG, BAC, VANG, KIMCUONG, CHIENLUOC',
  `TenHang` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `ThuTu` tinyint unsigned NOT NULL COMMENT 'Thứ tự xếp hạng để so sánh lên/xuống (1 = thấp nhất)',
  `DiemToiThieu` int unsigned NOT NULL DEFAULT '0' COMMENT 'Mốc điểm tích lũy trong 12 tháng gần nhất để đạt hạng này',
  `SoLanThuToiThieu` int unsigned NOT NULL DEFAULT '0' COMMENT 'Số phiếu thu tối thiểu trong 12 tháng gần nhất',
  `PhanTramGiamVoucher` decimal(5,2) NOT NULL DEFAULT '0.00' COMMENT '% giảm giá voucher tự động phát khi khách thăng lên hạng này (0 = không phát)',
  `MoTaQuyenLoi` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin COMMENT 'Mô tả quyền lợi của hạng, dùng để chèn vào nội dung email',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_xephang_ma` (`MaHang`),
  UNIQUE KEY `uq_xephang_thutu` (`ThuTu`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Danh mục hạng khách hàng, 1 bộ tiêu chí dùng chung cho B2C và B2B';

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.KH_XepHang: ~5 rows (xấp xỉ)
INSERT INTO `KH_XepHang` (`Id`, `MaHang`, `TenHang`, `ThuTu`, `DiemToiThieu`, `SoLanThuToiThieu`, `PhanTramGiamVoucher`, `MoTaQuyenLoi`, `IsActive`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'DONG', 'Đồng', 1, 0, 0, 0.00, 'Chưa có ưu đãi đặc biệt.', 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(2, 'BAC', 'Bạc', 2, 500, 1, 3.00, 'Tặng voucher 50.000đ vào dịp lễ và sinh nhật/kỷ niệm thành lập.', 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(3, 'VANG', 'Vàng', 3, 2000, 2, 5.00, 'Voucher giảm 5% dịp sinh nhật/lễ. Ưu tiên xử lý yêu cầu hỗ trợ.', 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(4, 'KIMCUONG', 'Kim Cương', 4, 5000, 3, 10.00, 'Voucher giảm 10% + quà tặng dịp lễ/sinh nhật. Giảm thêm 3% khi gia hạn hợp đồng.', 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00'),
	(5, 'CHIENLUOC', 'Chiến Lược', 5, 10000, 5, 15.00, 'Voucher giảm 15% + hotline ưu tiên riêng. Có Account Manager riêng phụ trách.', 1, '2026-05-01 08:00:00', '2026-05-01 08:00:00');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.Kho_TheKho
CREATE TABLE IF NOT EXISTS `Kho_TheKho` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `SanPham_Id` int unsigned NOT NULL,
  `MaChungTu` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `LoaiGiaoDich` enum('NhapMua','XuatBan','NhapTraKhach','XuatTraNCC','XuatHuy','KiemKe') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `SoLuongThayDoi` int NOT NULL COMMENT 'Dấu cộng (+) là Nhập, Dấu trừ (-) là Xuất',
  `TonCuoi` int NOT NULL COMMENT 'Số lượng tồn lũy kế ngay sau khi giao dịch này xảy ra',
  `NgayGiaoDich` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `NguoiThucHien_Id` int unsigned DEFAULT NULL,
  `GhiChu` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `fk_thekho_user` (`NguoiThucHien_Id`),
  KEY `idx_thekho_truyvan` (`SanPham_Id`,`NgayGiaoDich`),
  CONSTRAINT `fk_thekho_sp` FOREIGN KEY (`SanPham_Id`) REFERENCES `BH_SanPham` (`Id`),
  CONSTRAINT `fk_thekho_user` FOREIGN KEY (`NguoiThucHien_Id`) REFERENCES `HT_User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.Kho_TheKho: ~19 rows (xấp xỉ)
INSERT INTO `Kho_TheKho` (`Id`, `SanPham_Id`, `MaChungTu`, `LoaiGiaoDich`, `SoLuongThayDoi`, `TonCuoi`, `NgayGiaoDich`, `NguoiThucHien_Id`, `GhiChu`) VALUES
	(1, 1, 'NK-2026-001', 'NhapMua', 60, 60, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm CRM Bản Basic'),
	(2, 2, 'NK-2026-002', 'NhapMua', 35, 35, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm CRM Bản Pro'),
	(3, 3, 'NK-2026-003', 'NhapMua', 15, 15, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm CRM Bản Enterprise'),
	(4, 4, 'NK-2026-004', 'NhapMua', 12, 12, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm ERP Bản Basic'),
	(5, 5, 'NK-2026-005', 'NhapMua', 6, 6, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm ERP Bản Pro'),
	(6, 6, 'NK-2026-006', 'NhapMua', 20, 20, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm quản lý nhân sự HRM'),
	(7, 7, 'NK-2026-007', 'NhapMua', 25, 25, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Phần mềm bán hàng POS cho bán lẻ'),
	(8, 8, 'NK-2026-008', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ triển khai hệ thống'),
	(9, 9, 'NK-2026-009', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ đào tạo sử dụng phần mềm'),
	(10, 10, 'NK-2026-010', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ bảo trì hệ thống hàng năm'),
	(11, 11, 'NK-2026-011', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ tùy biến phần mềm theo yêu cầu'),
	(12, 12, 'NK-2026-012', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ chuyển đổi dữ liệu từ hệ thống cũ'),
	(13, 13, 'NK-2026-013', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ tư vấn giải pháp CNTT'),
	(14, 14, 'NK-2026-014', 'NhapMua', 999, 999, '2026-05-01 08:30:00', 1, 'Nhập kho đầu kỳ: Dịch vụ hỗ trợ ưu tiên (gói VIP)'),
	(15, 2, 'PX-2026-001', 'XuatBan', -3, 32, '2026-05-10 10:00:00', 3, 'Xuất license CRM Pro cho hợp đồng HD2026-001'),
	(16, 4, 'PX-2026-002', 'XuatBan', -1, 11, '2026-06-15 10:00:00', 5, 'Xuất license ERP Basic cho hợp đồng HD2026-002'),
	(17, 3, 'PX-2026-003', 'XuatBan', -1, 14, '2026-06-20 10:00:00', 3, 'Xuất license CRM Enterprise cho hợp đồng HD2026-006'),
	(18, 7, 'PX-2026-004', 'XuatBan', -2, 23, '2026-06-22 10:00:00', 6, 'Xuất license POS-RETAIL cho hợp đồng HD2026-007'),
	(19, 6, 'PX-2026-005', 'XuatBan', -1, 19, '2026-06-24 10:00:00', 4, 'Xuất license HRM-BASIC cho hợp đồng HD2026-008');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.SYS_AuditLog
CREATE TABLE IF NOT EXISTS `SYS_AuditLog` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TableName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `RecordId` bigint unsigned NOT NULL,
  `Action` enum('INSERT','UPDATE','DELETE') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `OldData` json DEFAULT NULL,
  `NewData` json DEFAULT NULL,
  `UserId` int unsigned DEFAULT NULL,
  `ChangedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `idx_audit_main` (`TableName`,`RecordId`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.SYS_AuditLog: ~21 rows (xấp xỉ)
INSERT INTO `SYS_AuditLog` (`Id`, `TableName`, `RecordId`, `Action`, `OldData`, `NewData`, `UserId`, `ChangedAt`) VALUES
	(1, 'KH_Lead', 0, 'INSERT', NULL, '{"Id": 0, "Email": "vovannhan160204@gmail.com", "TenLead": "Nhân", "CreatedAt": "2026-07-08T11:50:58.507234Z", "IsDeleted": false, "TenCongTy": "Công ty Nhân Nhân", "TinhTrang": "Moi", "UpdatedAt": "2026-07-08T11:50:58.5072346Z", "SoDienThoai": "0123333333", "NhanVienPhuTrachId": 9}', 2, '2026-07-08 11:50:59'),
	(2, 'KH_Lead', 16, 'UPDATE', '{"Id": 16, "Email": "vovannhan160204@gmail.com", "TenLead": "Nhân", "CreatedAt": "2026-07-08T11:50:59", "IsDeleted": false, "TenCongTy": "Công ty Nhân Nhân", "TinhTrang": "Moi", "UpdatedAt": "2026-07-08T11:50:58", "SoDienThoai": "0123333333", "NhanVienPhuTrachId": 9}', '{"Id": 16, "Email": "vovannhan160204@gmail.com", "TenLead": "Nhân", "CreatedAt": "2026-07-08T11:50:59", "IsDeleted": false, "TenCongTy": "Công ty Nhân Nhân", "TinhTrang": "DangChamSoc", "UpdatedAt": "2026-07-08T11:51:04.868959Z", "SoDienThoai": "0123333333", "NhanVienPhuTrachId": 9}', 2, '2026-07-08 11:51:05'),
	(3, 'KH_KhachHang', 26, 'INSERT', NULL, '{"Id": 26, "Email": "vovannhan160204@gmail.com", "MaSoThue": null, "NgaySinh": null, "CreatedAt": "2026-07-08T11:51:44", "UpdatedAt": "2026-07-08T11:51:44", "MaKhachHang": "KH0026", "SoDienThoai": "0123333333", "TinhTrangId": null, "NgayThanhLap": null, "TenKhachHang": "Công ty Nhân Nhân", "TenTinhTrang": null, "HangKhachHangId": null, "LoaiKhachHangId": null, "TenHangKhachHang": null, "TenLoaiKhachHang": null, "NhanVienPhuTrachId": 9, "TenNhanVienPhuTrach": null}', 2, '2026-07-08 11:51:44'),
	(4, 'KH_Lead', 16, 'UPDATE', '{"TinhTrang": "Cũ"}', '{"TinhTrang": "DaChuyenDoi"}', 2, '2026-07-08 11:51:44'),
	(5, 'BH_CoHoiBanHang', 21, 'INSERT', NULL, '{"Id": 21, "GhiChu": "Lần đầu", "LeadId": null, "TenLead": null, "GiaiDoan": "KhaoSat", "CreatedAt": "2026-07-08T11:53:37", "UpdatedAt": "2026-07-08T11:53:37", "NgayDuKien": "2026-07-16", "KhachHangId": 26, "TenNhanVien": null, "TenThuongVu": "Triển khai 1 app", "TenKhachHang": "Công ty Nhân Nhân", "TyLeThanhCong": 80, "DoanhThuKyVong": 1000000.0, "NhanVienPhuTrachId": null}', 2, '2026-07-08 11:53:37'),
	(6, 'BH_CoHoiBanHang', 21, 'UPDATE', NULL, '{"Id": 21, "GhiChu": "Lần đầu", "LeadId": null, "TenLead": null, "GiaiDoan": "DeXuat", "CreatedAt": "2026-07-08T11:53:37", "UpdatedAt": "2026-07-08T11:53:52", "NgayDuKien": "2026-07-16", "KhachHangId": 26, "TenNhanVien": null, "TenThuongVu": "Triển khai 1 app", "TenKhachHang": "Công ty Nhân Nhân", "TyLeThanhCong": 80, "DoanhThuKyVong": 1000000.0, "NhanVienPhuTrachId": null}', 2, '2026-07-08 11:53:52'),
	(7, 'BH_CoHoiBanHang', 21, 'UPDATE', NULL, '{"Id": 21, "GhiChu": "Lần đầu", "LeadId": null, "TenLead": null, "GiaiDoan": "ThuongLuong", "CreatedAt": "2026-07-08T11:53:37", "UpdatedAt": "2026-07-08T11:53:57", "NgayDuKien": "2026-07-16", "KhachHangId": 26, "TenNhanVien": null, "TenThuongVu": "Triển khai 1 app", "TenKhachHang": "Công ty Nhân Nhân", "TyLeThanhCong": 80, "DoanhThuKyVong": 1000000.0, "NhanVienPhuTrachId": null}', 2, '2026-07-08 11:53:57'),
	(8, 'BH_CoHoiBanHang', 21, 'UPDATE', NULL, '{"Id": 21, "GhiChu": "Lần đầu", "LeadId": null, "TenLead": null, "GiaiDoan": "ThanhCong", "CreatedAt": "2026-07-08T11:53:37", "UpdatedAt": "2026-07-08T11:53:59", "NgayDuKien": "2026-07-16", "KhachHangId": 26, "TenNhanVien": null, "TenThuongVu": "Triển khai 1 app", "TenKhachHang": "Công ty Nhân Nhân", "TyLeThanhCong": 100, "DoanhThuKyVong": 1000000.0, "NhanVienPhuTrachId": null}', 2, '2026-07-08 11:53:59'),
	(9, 'HD_BaoGia', 18, 'INSERT', NULL, '{"Id": 18, "ChiTiet": [{"Id": 29, "MaSP": "SRV-CONSULT", "DonVi": "Giờ", "TenSP": "Dịch vụ tư vấn giải pháp CNTT", "DonGia": 1500000.0, "SoLuong": 1, "SanPhamId": 13, "ThanhTien": 1500000.0}], "MaBaoGia": "BG00018", "TongTien": 1500000, "CreatedAt": "2026-07-08T11:54:30.6257136Z", "TrangThai": "Nhap", "UpdatedAt": "2026-07-08T11:54:30.6257144Z", "LyDoTuChoi": null, "NhanVienId": 2, "KhachHangId": 26, "TenNhanVien": "manager.ha", "TenKhachHang": "Công ty Nhân Nhân"}', 2, '2026-07-08 11:54:31'),
	(10, 'HD_BaoGia', 18, 'UPDATE', '{"TrangThai": "Nhap"}', '{"TrangThai": "DaGui"}', 2, '2026-07-08 11:54:48'),
	(11, 'HD_BaoGia', 18, 'UPDATE', '{"TrangThai": "DaGui"}', '{"TrangThai": "ChapNhan"}', 2, '2026-07-08 11:54:57'),
	(12, 'HD_HopDong', 11, 'INSERT', NULL, '{"Id": 11, "GiaTri": 1500000.0, "NgayKy": "2026-07-08", "ThoiHan": 12, "BaoGiaId": 18, "MaBaoGia": "BG00018", "CreatedAt": "2026-07-08T11:55:11", "MaHopDong": "HD00011", "TrangThai": "DangThucHien", "UpdatedAt": "2026-07-08T11:55:11", "KhachHangId": 26, "TenKhachHang": "Công ty Nhân Nhân"}', 2, '2026-07-08 11:55:11'),
	(13, 'KT_HoaDon', 9, 'INSERT', NULL, '{"Id": 9, "MaHoaDon": "INV-20260708-5E8384", "TongTien": 1, "CreatedAt": "2026-07-08T11:56:45.4935941Z", "HopDongId": 11, "MaHopDong": "HD00011", "UpdatedAt": "2026-07-08T11:56:45.4935945Z", "KhachHangId": 26, "SoTienDaThu": 0, "SoTienConLai": 1, "TenKhachHang": "Công ty Nhân Nhân", "TrangThaiThanhToan": "ChuaThanhToan"}', 2, '2026-07-08 11:56:46'),
	(14, 'KH_KhachHang', 26, 'UPDATE', '{"Id": 26, "Email": "vovannhan160204@gmail.com", "MaSoThue": null, "NgaySinh": null, "CreatedAt": "2026-07-08T11:51:44", "UpdatedAt": "2026-07-08T11:51:44", "MaKhachHang": "KH0026", "SoDienThoai": "0123333333", "TinhTrangId": null, "NgayThanhLap": null, "TenKhachHang": "Công ty Nhân Nhân", "TenTinhTrang": null, "HangKhachHangId": null, "LoaiKhachHangId": null, "TenHangKhachHang": null, "TenLoaiKhachHang": null, "NhanVienPhuTrachId": 9, "TenNhanVienPhuTrach": null}', '{"Id": 26, "Email": "vovannhan160204@gmail.com", "MaSoThue": "0311223344", "NgaySinh": null, "CreatedAt": "2026-07-08T11:51:44", "UpdatedAt": "2026-07-08T12:02:39.1868841Z", "MaKhachHang": "KH0026", "SoDienThoai": "0123333333", "TinhTrangId": 1, "NgayThanhLap": "2004-08-07", "TenKhachHang": "Công ty Nhân Nhân", "TenTinhTrang": null, "HangKhachHangId": null, "LoaiKhachHangId": 2, "TenHangKhachHang": null, "TenLoaiKhachHang": null, "NhanVienPhuTrachId": 9, "TenNhanVienPhuTrach": null}', 2, '2026-07-08 12:02:39'),
	(15, 'HT_User', 10, 'INSERT', NULL, '{"Id": 10, "Email": "test01@gmail.com", "HoTen": "Võ Văn Nhân", "RoleId": 2, "ChucVuId": 2, "NhanSuId": 11, "RoleName": "Manager", "Username": "test01", "CreatedAt": "2026-07-09T01:05:26", "TenChucVu": "Trưởng Phòng", "TrangThai": "Active", "UpdatedAt": "2026-07-09T01:05:26", "PhongBanId": 2, "SoDienThoai": "0901234567", "TenPhongBan": "Phòng Kinh Doanh"}', 1, '2026-07-09 01:05:26'),
	(16, 'HT_User', 11, 'INSERT', NULL, '{"Id": 11, "Email": "test02@gmail.com", "HoTen": "Nhân", "RoleId": 3, "ChucVuId": 3, "NhanSuId": 12, "RoleName": "Sale", "Username": "test02", "CreatedAt": "2026-07-09T01:06:37", "TenChucVu": "Nhân Viên", "TrangThai": "Active", "UpdatedAt": "2026-07-09T01:06:37", "PhongBanId": 3, "SoDienThoai": "0123333333", "TenPhongBan": "Phòng Kế Toán"}', 1, '2026-07-09 01:06:37'),
	(17, 'HT_User', 12, 'INSERT', NULL, '{"Id": 12, "Email": "test03@gmail.com", "HoTen": "Nhân Nhân Nhân", "RoleId": 4, "ChucVuId": 3, "NhanSuId": 13, "RoleName": "Accountant", "Username": "test03", "CreatedAt": "2026-07-09T01:07:33", "TenChucVu": "Nhân Viên", "TrangThai": "Active", "UpdatedAt": "2026-07-09T01:07:33", "PhongBanId": 3, "SoDienThoai": "0933444555", "TenPhongBan": "Phòng Kế Toán"}', 1, '2026-07-09 01:07:33'),
	(18, 'HT_User', 12, 'UPDATE', '{"Action": "ChangePassword", "Username": "test03"}', '{"Action": "ChangePassword", "Username": "test03", "ChangedAt": "2026-07-09T01:24:09.0735307Z"}', 12, '2026-07-09 01:24:09'),
	(19, 'HT_User', 12, 'UPDATE', '{"Id": 12, "Email": "test03@gmail.com", "HoTen": "Nhân Nhân Nhân", "RoleId": 4, "ChucVuId": 3, "NhanSuId": 13, "RoleName": "Accountant", "Username": "test03", "CreatedAt": "2026-07-09T01:07:33", "TenChucVu": "Nhân Viên", "TrangThai": "Active", "UpdatedAt": "2026-07-09T01:24:09", "PhongBanId": 3, "SoDienThoai": "0933444555", "TenPhongBan": "Phòng Kế Toán"}', '{"Id": 12, "Email": "test03@gmail.com", "HoTen": "Nhân Nhân Nhân", "RoleId": 4, "ChucVuId": 3, "NhanSuId": 13, "RoleName": "Accountant", "Username": "test03", "CreatedAt": "2026-07-09T01:07:33", "TenChucVu": "Nhân Viên", "TrangThai": "Locked", "UpdatedAt": "2026-07-09T01:25:38", "PhongBanId": 3, "SoDienThoai": "0933444555", "TenPhongBan": "Phòng Kế Toán"}', 1, '2026-07-09 01:25:38'),
	(20, 'KH_Lead', 16, 'DELETE', '{"Id": 16, "Email": "vovannhan160204@gmail.com", "TenLead": "Nhân", "CreatedAt": "2026-07-08T11:50:59", "IsDeleted": false, "TenCongTy": "Công ty Nhân Nhân", "TinhTrang": "DaChuyenDoi", "UpdatedAt": "2026-07-08T11:51:43", "SoDienThoai": "0123333333", "NhanVienPhuTrachId": 9}', NULL, 2, '2026-07-09 01:30:01'),
	(21, 'BH_CoHoiBanHang', 21, 'DELETE', '{"Id": 21, "GhiChu": "Lần đầu", "LeadId": null, "GiaiDoan": "ThanhCong", "CreatedAt": "2026-07-08T11:53:37", "IsDeleted": false, "UpdatedAt": "2026-07-08T11:53:59", "NgayDuKien": "2026-07-16", "KhachHangId": 26, "TenThuongVu": "Triển khai 1 app", "TyLeThanhCong": 100, "DoanhThuKyVong": 1000000.0, "NhanVienPhuTrachId": null}', NULL, 2, '2026-07-09 01:32:17');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.TK_LoaiTicket
CREATE TABLE IF NOT EXISTS `TK_LoaiTicket` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenLoai` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT 'Tên loại ticket (Bảo hành, Khiếu nại, Hỗ trợ KT…)',
  `MoTa` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_loai_ticket_ten` (`TenLoai`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.TK_LoaiTicket: ~4 rows (xấp xỉ)
INSERT INTO `TK_LoaiTicket` (`Id`, `TenLoai`, `MoTa`, `IsActive`) VALUES
	(1, 'Bảo hành', 'Yêu cầu bảo hành sản phẩm / dịch vụ', 1),
	(2, 'Khiếu nại', 'Khiếu nại chất lượng hoặc dịch vụ', 1),
	(3, 'Hỗ trợ kỹ thuật', 'Hỗ trợ cài đặt, lỗi kỹ thuật, hướng dẫn sử dụng', 1),
	(4, 'Yêu cầu sử dụng Voucher', 'Khách bấm link xác nhận muốn sử dụng voucher nhận được qua email', 1);

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.TK_Ticket
CREATE TABLE IF NOT EXISTS `TK_Ticket` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaTicket` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT 'Mã Ticket tự sinh',
  `TieuDe` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `MoTa` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  `FileDinhKem` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `LoaiTicket_Id` smallint unsigned DEFAULT NULL,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `HopDong_Id` bigint unsigned DEFAULT NULL,
  `SanPham_Id` int unsigned DEFAULT NULL,
  `MucDoUuTien` enum('Thap','TrungBinh','Cao','KhanCap') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'TrungBinh',
  `NguonTiepNhan` enum('Email','Phone','Web','Zalo','TrucTiep') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'Phone',
  `TrangThai` enum('Moi','DangXuLy','ChoPhanHoi','Dong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'Moi',
  `NhanVienTiepNhan_Id` int unsigned DEFAULT NULL,
  `NhanVienXuLy_Id` int unsigned DEFAULT NULL,
  `NgayHenXuLy` datetime DEFAULT NULL,
  `NgayDong` datetime DEFAULT NULL,
  `LyDoDong` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsDeleted` tinyint(1) DEFAULT '0',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_ticket_ma` (`MaTicket`),
  KEY `idx_ticket_trangthai` (`TrangThai`,`IsDeleted`),
  KEY `idx_ticket_uutien` (`MucDoUuTien`),
  KEY `idx_ticket_kh` (`KhachHang_Id`),
  KEY `idx_ticket_xuly` (`NhanVienXuLy_Id`),
  KEY `fk_ticket_loai` (`LoaiTicket_Id`),
  KEY `fk_ticket_hd` (`HopDong_Id`),
  KEY `fk_ticket_sp` (`SanPham_Id`),
  KEY `fk_ticket_tiepnhan` (`NhanVienTiepNhan_Id`),
  CONSTRAINT `fk_ticket_hd` FOREIGN KEY (`HopDong_Id`) REFERENCES `HD_HopDong` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ticket_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`),
  CONSTRAINT `fk_ticket_loai` FOREIGN KEY (`LoaiTicket_Id`) REFERENCES `TK_LoaiTicket` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ticket_sp` FOREIGN KEY (`SanPham_Id`) REFERENCES `BH_SanPham` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ticket_tiepnhan` FOREIGN KEY (`NhanVienTiepNhan_Id`) REFERENCES `HT_User` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ticket_xuly` FOREIGN KEY (`NhanVienXuLy_Id`) REFERENCES `HT_User` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.TK_Ticket: ~15 rows (xấp xỉ)
INSERT INTO `TK_Ticket` (`Id`, `MaTicket`, `TieuDe`, `MoTa`, `FileDinhKem`, `LoaiTicket_Id`, `KhachHang_Id`, `HopDong_Id`, `SanPham_Id`, `MucDoUuTien`, `NguonTiepNhan`, `TrangThai`, `NhanVienTiepNhan_Id`, `NhanVienXuLy_Id`, `NgayHenXuLy`, `NgayDong`, `LyDoDong`, `IsDeleted`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'TK2026-0001', 'Lỗi đăng nhập CRM Pro sau khi đổi mật khẩu', 'Tài khoản admin phía khách không đăng nhập được sau khi đổi mật khẩu, báo lỗi tài khoản bị khóa.', NULL, 3, 1, 1, 2, 'Cao', 'Phone', 'Dong', 4, 3, '2026-05-20 17:00:00', '2026-05-20 10:30:00', 'Đã mở khóa tài khoản, khách xác nhận đăng nhập được.', 0, '2026-05-20 08:30:00', '2026-05-20 10:30:00'),
	(2, 'TK2026-0002', 'Yêu cầu bảo hành license CRM Basic bị báo hết hạn sớm', 'License mua tháng 5/2026 báo hết hạn trước thời hạn cam kết. Đề nghị kiểm tra lại.', NULL, 1, 2, NULL, 1, 'TrungBinh', 'Email', 'DangXuLy', 4, 4, '2026-06-05 17:00:00', NULL, NULL, 0, '2026-06-02 09:00:00', '2026-06-02 11:00:00'),
	(3, 'TK2026-0003', 'Khiếu nại hóa đơn tính thêm phí không có trong hợp đồng', 'Hóa đơn phát sinh thêm phí setup không có trong điều khoản hợp đồng đã ký. Đề nghị xem xét điều chỉnh.', NULL, 2, 4, NULL, NULL, 'KhanCap', 'Zalo', 'Dong', 2, 2, '2026-06-19 12:00:00', '2026-06-19 09:00:00', 'Xác nhận sai sót, đã xuất hóa đơn điều chỉnh giảm trừ phí setup.', 0, '2026-06-18 14:00:00', '2026-06-19 09:00:00'),
	(4, 'TK2026-0004', 'Hỏi cách xuất báo cáo doanh thu theo tháng', 'Khách muốn được hướng dẫn cách xuất báo cáo doanh thu theo từng tháng trên phần mềm.', NULL, 3, 7, NULL, 3, 'Thap', 'Web', 'Moi', NULL, NULL, NULL, NULL, NULL, 0, '2026-06-27 10:00:00', '2026-06-27 10:00:00'),
	(5, 'TK2026-0005', 'Khiếu nại thời gian phản hồi hỗ trợ chậm', 'Khách phản ánh đội hỗ trợ phản hồi chậm so với cam kết SLA trong hợp đồng cũ.', NULL, 2, 8, NULL, NULL, 'TrungBinh', 'Phone', 'ChoPhanHoi', 3, 3, '2026-05-05 17:00:00', NULL, NULL, 0, '2026-04-28 09:00:00', '2026-04-29 10:00:00'),
	(6, 'TK2026-0006', 'Yêu cầu hướng dẫn cấu hình phân quyền người dùng', 'Khách cần hướng dẫn cấu hình phân quyền cho 5 tài khoản nhân viên mới.', NULL, 3, 13, 4, 5, 'TrungBinh', 'Email', 'Dong', 3, 3, '2026-06-03 17:00:00', '2026-06-03 11:00:00', 'Đã hướng dẫn qua video call, khách xác nhận cấu hình thành công.', 0, '2026-06-03 08:00:00', '2026-06-03 11:00:00'),
	(7, 'TK2026-0007', 'Lỗi in hóa đơn từ phần mềm POS', 'Máy in hóa đơn không nhận lệnh in từ phần mềm POS sau khi cập nhật.', NULL, 3, 16, 5, 7, 'Cao', 'Phone', 'DangXuLy', 6, 6, '2026-06-25 17:00:00', NULL, NULL, 0, '2026-06-24 09:00:00', '2026-06-24 10:00:00'),
	(8, 'TK2026-0008', 'Khiếu nại tốc độ xử lý chậm khi vào cao điểm', 'Hệ thống CRM chạy chậm vào giờ cao điểm buổi sáng, ảnh hưởng công việc.', NULL, 2, 7, 3, 3, 'Cao', 'Zalo', 'ChoPhanHoi', 5, 5, '2026-06-01 17:00:00', NULL, NULL, 0, '2026-05-28 09:00:00', '2026-05-29 10:00:00'),
	(9, 'TK2026-0009', 'Yêu cầu bổ sung module báo cáo tồn kho', 'Khách muốn bổ sung thêm module báo cáo tồn kho chi tiết theo chi nhánh.', NULL, 3, 11, NULL, 6, 'Thap', 'Web', 'Moi', NULL, NULL, NULL, NULL, NULL, 0, '2026-06-26 10:00:00', '2026-06-26 10:00:00'),
	(10, 'TK2026-0010', 'Bảo hành lỗi đồng bộ dữ liệu ERP', 'Dữ liệu đơn hàng không đồng bộ đúng giữa 2 chi nhánh sau khi triển khai ERP.', NULL, 1, 10, 2, 5, 'KhanCap', 'Phone', 'Dong', 5, 5, '2026-06-18 17:00:00', '2026-06-18 15:00:00', 'Đã khắc phục lỗi đồng bộ do cấu hình sai múi giờ server, khách xác nhận ổn định.', 0, '2026-06-17 08:00:00', '2026-06-18 15:00:00'),
	(11, 'TK2026-0011', 'Khiếu nại nhân viên hỗ trợ thái độ chưa tốt', 'Khách phản ánh nhân viên hỗ trợ qua điện thoại thái độ chưa tốt khi giải quyết sự cố.', NULL, 2, 2, NULL, NULL, 'TrungBinh', 'Phone', 'Dong', 4, 2, '2026-05-10 17:00:00', '2026-05-10 11:00:00', 'Đã xin lỗi khách và nhắc nhở nhân viên liên quan, khách hài lòng với hướng xử lý.', 0, '2026-05-09 14:00:00', '2026-05-10 11:00:00'),
	(12, 'TK2026-0012', 'Yêu cầu hỗ trợ khôi phục dữ liệu bị xóa nhầm', 'Nhân viên khách hàng xóa nhầm một số bản ghi khách hàng trên CRM.', NULL, 3, 1, 1, 2, 'Cao', 'Email', 'Dong', 3, 3, '2026-06-25 17:00:00', '2026-06-25 14:00:00', 'Đã khôi phục dữ liệu từ bản sao lưu gần nhất, khách xác nhận đủ dữ liệu.', 0, '2026-06-25 09:00:00', '2026-06-25 14:00:00'),
	(13, 'TK2026-0013', 'Hỏi về gia hạn hợp đồng bảo trì', 'Khách hỏi thông tin và chi phí gia hạn hợp đồng bảo trì sắp hết hạn.', NULL, 1, 13, 4, 10, 'Thap', 'Email', 'DangXuLy', 7, 7, '2026-07-05 17:00:00', NULL, NULL, 0, '2026-06-29 09:00:00', '2026-06-29 10:00:00'),
	(14, 'TK2026-0014', 'Lỗi xuất file Excel báo cáo bị thiếu cột', 'File Excel xuất báo cáo doanh thu bị thiếu cột chiết khấu so với trước đây.', NULL, 3, 16, 5, 7, 'TrungBinh', 'Web', 'Moi', NULL, NULL, NULL, NULL, NULL, 0, '2026-06-28 10:00:00', '2026-06-28 10:00:00'),
	(15, 'TK2026-0015', 'Khiếu nại về việc chậm bàn giao license ERP', 'License ERP-PRO cam kết bàn giao trong 3 ngày nhưng chậm 1 tuần.', NULL, 2, 13, 4, 5, 'Cao', 'Phone', 'Dong', 3, 3, '2026-05-28 17:00:00', '2026-05-28 15:00:00', 'Xác nhận chậm do lỗi cấp phép từ nhà cung cấp, đã bàn giao và tặng thêm 1 tháng hỗ trợ ưu tiên.', 0, '2026-05-27 09:00:00', '2026-05-28 15:00:00');

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.TK_Ticket_PhanHoi
CREATE TABLE IF NOT EXISTS `TK_Ticket_PhanHoi` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `Ticket_Id` bigint unsigned NOT NULL,
  `NguoiPhanHoi_Id` int unsigned DEFAULT NULL,
  `LoaiPhanHoi` enum('NoiBoXuLy','PhanHoiKhachHang','YeuCauBoSung','DongTicket') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `NoiDung` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `FileDinhKem` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `TrangThaiTruoc` enum('Moi','DangXuLy','ChoPhanHoi','Dong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `TrangThaiSau` enum('Moi','DangXuLy','ChoPhanHoi','Dong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `idx_phanHoi_ticket` (`Ticket_Id`),
  KEY `idx_phanHoi_nguoi` (`NguoiPhanHoi_Id`),
  CONSTRAINT `fk_ph_ticket` FOREIGN KEY (`Ticket_Id`) REFERENCES `TK_Ticket` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_ph_user` FOREIGN KEY (`NguoiPhanHoi_Id`) REFERENCES `HT_User` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Đang kết xuất đổ dữ liệu cho bảng CRM-LVTN.TK_Ticket_PhanHoi: ~21 rows (xấp xỉ)
INSERT INTO `TK_Ticket_PhanHoi` (`Id`, `Ticket_Id`, `NguoiPhanHoi_Id`, `LoaiPhanHoi`, `NoiDung`, `FileDinhKem`, `TrangThaiTruoc`, `TrangThaiSau`, `CreatedAt`) VALUES
	(1, 1, 3, 'NoiBoXuLy', 'Đã kiểm tra, tài khoản bị khóa do đăng nhập sai quá 5 lần liên tiếp. Tiến hành mở khóa thủ công.', NULL, 'Moi', 'DangXuLy', '2026-05-20 09:15:00'),
	(2, 1, 3, 'DongTicket', 'Đã mở khóa tài khoản và hướng dẫn khách đặt lại mật khẩu. Khách xác nhận đăng nhập bình thường.', NULL, 'DangXuLy', 'Dong', '2026-05-20 10:30:00'),
	(3, 2, 4, 'NoiBoXuLy', 'Đã liên hệ đội kỹ thuật kiểm tra lại thời hạn kích hoạt license trên hệ thống license server.', NULL, 'Moi', 'DangXuLy', '2026-06-02 11:00:00'),
	(4, 3, 2, 'NoiBoXuLy', 'Đối chiếu lại hợp đồng, xác nhận phí setup phát sinh không đúng thỏa thuận ban đầu.', NULL, 'Moi', 'DangXuLy', '2026-06-18 15:00:00'),
	(5, 3, 2, 'DongTicket', 'Đã xuất hóa đơn điều chỉnh giảm trừ phí setup, khách xác nhận đồng ý. Đóng ticket.', NULL, 'DangXuLy', 'Dong', '2026-06-19 09:00:00'),
	(6, 5, 3, 'NoiBoXuLy', 'Ghi nhận phản ánh của khách, đang rà soát lại quy trình và thời gian phản hồi hỗ trợ.', NULL, 'Moi', 'DangXuLy', '2026-04-28 14:00:00'),
	(7, 5, 3, 'PhanHoiKhachHang', 'Đã phản hồi khách, xin thêm thời gian để xác minh nguyên nhân chậm trễ và đề xuất hướng khắc phục.', NULL, 'DangXuLy', 'ChoPhanHoi', '2026-04-29 10:00:00'),
	(8, 6, 3, 'NoiBoXuLy', 'Đã lên lịch video call hướng dẫn khách cấu hình phân quyền.', NULL, 'Moi', 'DangXuLy', '2026-06-03 09:00:00'),
	(9, 6, 3, 'DongTicket', 'Đã hướng dẫn xong qua video call, khách xác nhận cấu hình 5 tài khoản thành công.', NULL, 'DangXuLy', 'Dong', '2026-06-03 11:00:00'),
	(10, 7, 6, 'NoiBoXuLy', 'Đang phối hợp đội kỹ thuật kiểm tra driver máy in và bản cập nhật phần mềm POS.', NULL, 'Moi', 'DangXuLy', '2026-06-24 10:00:00'),
	(11, 8, 5, 'NoiBoXuLy', 'Đã ghi nhận, đang theo dõi tải hệ thống vào khung giờ cao điểm để xác định nguyên nhân.', NULL, 'Moi', 'DangXuLy', '2026-05-28 10:00:00'),
	(12, 8, 5, 'PhanHoiKhachHang', 'Đã phản hồi khách, đề xuất nâng cấp gói hạ tầng server để cải thiện tốc độ giờ cao điểm.', NULL, 'DangXuLy', 'ChoPhanHoi', '2026-05-29 10:00:00'),
	(13, 10, 5, 'NoiBoXuLy', 'Đã kiểm tra log hệ thống, xác định lỗi đồng bộ do lệch múi giờ giữa 2 server chi nhánh.', NULL, 'Moi', 'DangXuLy', '2026-06-18 10:00:00'),
	(14, 10, 5, 'DongTicket', 'Đã đồng bộ lại múi giờ server và kiểm tra dữ liệu, khách xác nhận đồng bộ ổn định.', NULL, 'DangXuLy', 'Dong', '2026-06-18 15:00:00'),
	(15, 11, 4, 'NoiBoXuLy', 'Đã trao đổi nội bộ với nhân viên hỗ trợ liên quan để xác minh sự việc.', NULL, 'Moi', 'DangXuLy', '2026-05-09 15:00:00'),
	(16, 11, 2, 'DongTicket', 'Đã xin lỗi khách và nhắc nhở nhân viên liên quan, khách hài lòng với hướng xử lý.', NULL, 'DangXuLy', 'Dong', '2026-05-10 11:00:00'),
	(17, 12, 3, 'NoiBoXuLy', 'Đang kiểm tra bản sao lưu gần nhất để khôi phục dữ liệu bị xóa nhầm.', NULL, 'Moi', 'DangXuLy', '2026-06-25 10:00:00'),
	(18, 12, 3, 'DongTicket', 'Đã khôi phục dữ liệu từ bản sao lưu, khách xác nhận đủ dữ liệu như trước.', NULL, 'DangXuLy', 'Dong', '2026-06-25 14:00:00'),
	(19, 13, 7, 'NoiBoXuLy', 'Đã gửi báo giá gia hạn hợp đồng bảo trì kèm ưu đãi cho khách VIP.', NULL, 'Moi', 'DangXuLy', '2026-06-29 10:00:00'),
	(20, 15, 3, 'NoiBoXuLy', 'Xác nhận với nhà cung cấp license, ghi nhận chậm trễ do lỗi cấp phép.', NULL, 'Moi', 'DangXuLy', '2026-05-27 10:00:00'),
	(21, 15, 3, 'DongTicket', 'Đã bàn giao license và tặng thêm 1 tháng hỗ trợ ưu tiên để bù đắp chậm trễ.', NULL, 'DangXuLy', 'Dong', '2026-05-28 15:00:00');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
