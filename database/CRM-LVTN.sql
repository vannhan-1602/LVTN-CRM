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

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.BH_LoaiSanPham
CREATE TABLE IF NOT EXISTS `BH_LoaiSanPham` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `TenLoai` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `HinhThuc` enum('VatLy','DichVu','License','Subscription') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'VatLy' COMMENT 'Chi loai VatLy moi ap dung SoLuongTon tren BH_SanPham',
  `MoTa` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.BH_SanPham_HinhAnh
CREATE TABLE IF NOT EXISTS `BH_SanPham_HinhAnh` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `SanPham_Id` int unsigned DEFAULT NULL,
  `UrlHinhAnh` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsMain` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `fk_sphinhanh_sp` (`SanPham_Id`),
  CONSTRAINT `fk_sphinhanh_sp` FOREIGN KEY (`SanPham_Id`) REFERENCES `BH_SanPham` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.DM_PhuongXa
CREATE TABLE IF NOT EXISTS `DM_PhuongXa` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `TinhThanh_Id` int unsigned NOT NULL,
  `TenPhuongXa` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tenphuong_tinh` (`TinhThanh_Id`,`TenPhuongXa`),
  CONSTRAINT `DM_PhuongXa_ibfk_1` FOREIGN KEY (`TinhThanh_Id`) REFERENCES `DM_TinhThanh` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3322 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.DM_TinhThanh
CREATE TABLE IF NOT EXISTS `DM_TinhThanh` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `TenTinhThanh` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tentinh` (`TenTinhThanh`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=59 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_HopDong
CREATE TABLE IF NOT EXISTS `HD_HopDong` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaHopDong` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `BaoGia_Id` bigint unsigned DEFAULT NULL,
  `NgayKy` date DEFAULT NULL,
  `ThoiHan` int DEFAULT NULL,
  `NgayKetThuc` date DEFAULT NULL,
  `HinhThucThanhToan` enum('ThanhToanMotLan','TraGop') COLLATE utf8mb4_bin DEFAULT 'ThanhToanMotLan',
  `TrangThai` enum('DangThucHien','TamDung','ThanhLy','HetHan') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'DangThucHien',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `LoaiHopDong` enum('ChinhThuc','GiaHan','BaoTri') COLLATE utf8mb4_bin DEFAULT 'ChinhThuc',
  `HopDongGoc_Id` bigint unsigned DEFAULT NULL,
  `NgayNhacGiaHanCuoi` date DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MaHopDong` (`MaHopDong`),
  KEY `fk_hdong_kh` (`KhachHang_Id`),
  KEY `fk_hdong_baogia` (`BaoGia_Id`),
  KEY `fk_hopdong_goc` (`HopDongGoc_Id`),
  CONSTRAINT `fk_hdong_baogia` FOREIGN KEY (`BaoGia_Id`) REFERENCES `HD_BaoGia` (`Id`),
  CONSTRAINT `fk_hdong_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`),
  CONSTRAINT `fk_hopdong_goc` FOREIGN KEY (`HopDongGoc_Id`) REFERENCES `HD_HopDong` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_License
CREATE TABLE IF NOT EXISTS `HD_License` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `HopDong_Id` bigint unsigned NOT NULL,
  `SanPham_Id` int unsigned NOT NULL,
  `SoLuongUser` int DEFAULT '1',
  `PhienBan` varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
  `MaLicenseKey` varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
  `MoiTruongTrienKhai` enum('Cloud','OnPremise') COLLATE utf8mb4_bin DEFAULT 'Cloud',
  `NgayKichHoat` date DEFAULT NULL,
  `NgayHetHan` date DEFAULT NULL,
  `TrangThai` enum('DangHoatDong','TamKhoa','HetHan') COLLATE utf8mb4_bin DEFAULT 'DangHoatDong',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_license_key` (`MaLicenseKey`),
  KEY `HopDong_Id` (`HopDong_Id`),
  KEY `SanPham_Id` (`SanPham_Id`),
  CONSTRAINT `HD_License_ibfk_1` FOREIGN KEY (`HopDong_Id`) REFERENCES `HD_HopDong` (`Id`),
  CONSTRAINT `HD_License_ibfk_2` FOREIGN KEY (`SanPham_Id`) REFERENCES `BH_SanPham` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_LichThanhToan
CREATE TABLE IF NOT EXISTS `HD_LichThanhToan` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `HopDong_Id` bigint unsigned NOT NULL,
  `SoDot` int NOT NULL,
  `SoTien` decimal(18,2) NOT NULL,
  `HanThanhToan` date NOT NULL,
  `TrangThai` enum('ChuaDenHan','ChoThanhToan','DaThanhToan','QuaHan') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT 'ChuaDenHan',
  PRIMARY KEY (`Id`),
  KEY `HopDong_Id` (`HopDong_Id`),
  CONSTRAINT `HD_LichThanhToan_ibfk_1` FOREIGN KEY (`HopDong_Id`) REFERENCES `HD_HopDong` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HD_MocTrienKhai
CREATE TABLE IF NOT EXISTS `HD_MocTrienKhai` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `HopDong_Id` bigint unsigned NOT NULL,
  `LoaiMoc` enum('DaoTao','BanGiao','NghiemThu') COLLATE utf8mb4_bin NOT NULL,
  `NoiDung` varchar(255) COLLATE utf8mb4_bin DEFAULT NULL,
  `NgayThucHien` datetime DEFAULT NULL,
  `NhanVienThucHien_Id` int unsigned DEFAULT NULL,
  `NguoiXacNhanKhach` varchar(255) COLLATE utf8mb4_bin DEFAULT NULL,
  `FileBienBan` varchar(500) COLLATE utf8mb4_bin DEFAULT NULL,
  `TrangThai` enum('ChuaThucHien','DaThucHien','DaXacNhan') COLLATE utf8mb4_bin DEFAULT 'ChuaThucHien',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `HopDong_Id` (`HopDong_Id`),
  KEY `NhanVienThucHien_Id` (`NhanVienThucHien_Id`),
  CONSTRAINT `HD_MocTrienKhai_ibfk_1` FOREIGN KEY (`HopDong_Id`) REFERENCES `HD_HopDong` (`Id`),
  CONSTRAINT `HD_MocTrienKhai_ibfk_2` FOREIGN KEY (`NhanVienThucHien_Id`) REFERENCES `HT_User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_ChucVu
CREATE TABLE IF NOT EXISTS `HT_ChucVu` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenChucVu` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `TenChucVu` (`TenChucVu`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_PhongBan
CREATE TABLE IF NOT EXISTS `HT_PhongBan` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenPhongBan` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `TenPhongBan` (`TenPhongBan`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.HT_Role
CREATE TABLE IF NOT EXISTS `HT_Role` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `TenRole` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `MoTa` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KT_HoaDon
CREATE TABLE IF NOT EXISTS `KT_HoaDon` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MaHoaDon` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `HopDong_Id` bigint unsigned DEFAULT NULL,
  `LichThanhToan_Id` bigint unsigned DEFAULT NULL,
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
  KEY `fk_hdon_lichthanhtoan` (`LichThanhToan_Id`),
  CONSTRAINT `fk_hdon_hopdong` FOREIGN KEY (`HopDong_Id`) REFERENCES `HD_HopDong` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_hdon_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`),
  CONSTRAINT `fk_hdon_lichthanhtoan` FOREIGN KEY (`LichThanhToan_Id`) REFERENCES `HD_LichThanhToan` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_DiaChi
CREATE TABLE IF NOT EXISTS `KH_DiaChi` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `DiaChiChiTiet` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `TinhThanh_Id` int unsigned DEFAULT NULL,
  `PhuongXa_Id` int unsigned DEFAULT NULL,
  `LoaiDiaChi` enum('Billing','Shipping','Office') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsDefault` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `fk_dc_kh` (`KhachHang_Id`),
  KEY `fk_dc_tinh` (`TinhThanh_Id`),
  KEY `fk_dc_phuong` (`PhuongXa_Id`),
  CONSTRAINT `fk_dc_kh` FOREIGN KEY (`KhachHang_Id`) REFERENCES `KH_KhachHang` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_dc_phuong` FOREIGN KEY (`PhuongXa_Id`) REFERENCES `DM_PhuongXa` (`Id`),
  CONSTRAINT `fk_dc_tinh` FOREIGN KEY (`TinhThanh_Id`) REFERENCES `DM_TinhThanh` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Lịch sử tích điểm/trừ điểm, dùng chung cho B2C và B2B, tỷ lệ 100.000 VNĐ = 1 điểm';

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_EmailLog
CREATE TABLE IF NOT EXISTS `KH_EmailLog` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KhachHang_Id` bigint unsigned NOT NULL,
  `LoaiEmail` enum('XacNhanThanhToan','ThangHang','XuongHang','CanhBaoXuongHang','SinhNhat','NgayThanhLap','NgayLe','CuoiNam','BaoGia','NhacThanhToan','QuaHanThanhToan','NhacGiaHanHopDong','KhaoSatHaiLong') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
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
) ENGINE=InnoDB AUTO_INCREMENT=63 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Log mọi email đã gửi cho khách, dùng để chống gửi trùng trong cùng tháng/năm và để demo/audit';

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_Lead
CREATE TABLE IF NOT EXISTS `KH_Lead` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TenLead` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `TenCongTy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `SoDienThoai` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `Email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NguonLead` varchar(50) COLLATE utf8mb4_bin DEFAULT 'Manual',
  `TinhTrang` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `NhanVienPhuTrach_Id` int unsigned DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `fk_lead_nv` (`NhanVienPhuTrach_Id`),
  FULLTEXT KEY `idx_fts_lead` (`TenLead`,`TenCongTy`),
  CONSTRAINT `fk_lead_nv` FOREIGN KEY (`NhanVienPhuTrach_Id`) REFERENCES `HT_User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Lịch sử thay đổi hạng của khách hàng, dùng để truy vết và làm minh chứng khi cần';

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_LoaiKhachHang
CREATE TABLE IF NOT EXISTS `KH_LoaiKhachHang` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenLoai` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `MoTa` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.KH_TinhTrangKhachHang
CREATE TABLE IF NOT EXISTS `KH_TinhTrangKhachHang` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenTinhTrang` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Voucher phát cho khách hàng qua email, khách bấm link tạo yêu cầu, nhân viên xử lý qua Ticket';

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Token bảo mật cho link sử dụng voucher trong email, tách riêng để không bị đoán mã qua Id tuần tự';

-- Việc xuất dữ liệu đã bị bỏ chọn.

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

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.TK_DanhGiaHaiLong
CREATE TABLE IF NOT EXISTS `TK_DanhGiaHaiLong` (
  `Ticket_Id` bigint unsigned NOT NULL,
  `Token` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT 'Token để khách đánh giá qua link public không cần đăng nhập, giống QuotePublicToken',
  `DiemDanhGia` tinyint unsigned DEFAULT NULL COMMENT 'Thang điểm 1-5, NULL nếu khách chưa đánh giá',
  `NhanXet` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  `DaGuiEmail` tinyint(1) NOT NULL DEFAULT '0',
  `NgayGuiEmail` datetime DEFAULT NULL,
  `NgayDanhGia` datetime DEFAULT NULL,
  PRIMARY KEY (`Ticket_Id`),
  UNIQUE KEY `uq_danhgia_token` (`Token`),
  CONSTRAINT `fk_danhgia_ticket` FOREIGN KEY (`Ticket_Id`) REFERENCES `TK_Ticket` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Khảo sát mức độ hài lòng của khách sau khi ticket được đóng';

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.TK_LoaiTicket
CREATE TABLE IF NOT EXISTS `TK_LoaiTicket` (
  `Id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `TenLoai` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT 'Tên loại ticket (Bảo hành, Khiếu nại, Hỗ trợ KT…)',
  `MoTa` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_loai_ticket_ten` (`TenLoai`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

-- Đang kết xuất đổ cấu trúc cho bảng CRM-LVTN.TK_SLA
CREATE TABLE IF NOT EXISTS `TK_SLA` (
  `MucDoUuTien` enum('Thap','TrungBinh','Cao','KhanCap') CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `SoGioPhanHoi` int unsigned NOT NULL COMMENT 'Số giờ tối đa phải phản hồi lần đầu',
  `SoGioXuLy` int unsigned NOT NULL COMMENT 'Số giờ tối đa phải xử lý xong (tính ThoiHanSLA)',
  PRIMARY KEY (`MucDoUuTien`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Cấu hình SLA theo mức độ ưu tiên ticket';

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
  `ThoiHanSLA` datetime DEFAULT NULL COMMENT 'CreatedAt + SoGioXuLy tuong ung MucDoUuTien, tinh khi tao ticket',
  `SoLanEscalate` int unsigned NOT NULL DEFAULT '0' COMMENT 'So lan da canh bao qua han SLA, dung chong gui canh bao trung',
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
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

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
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- Việc xuất dữ liệu đã bị bỏ chọn.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
