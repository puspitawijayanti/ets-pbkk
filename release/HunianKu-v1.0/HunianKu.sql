/* =====================================================================
   HunianKu - Sistem Manajemen Apartemen
   Script lengkap: database, tabel, constraint, index, view, function,
   stored procedure, dan data dummy.

   Target  : SQL Server 2019+ (diuji di SQL Server 2025 Express)
   Cara run: buka di SSMS lalu Execute (F5), atau lewat command line:
             sqlcmd -S .\SQLEXPRESS -E -C -i HunianKu.sql

   PERINGATAN: script ini MENGHAPUS database HunianKu jika sudah ada,
   lalu membuatnya ulang dari nol (cocok untuk reset data demo).

   Akun demo (username / password):
     admin  / admin123      -> Admin/Pemilik
     rina   / staf123       -> Staf Pengelola
     dodi   / staf123       -> Staf Pengelola
     budi, siti, andi, ... / penghuni123  -> Penghuni
   ===================================================================== */

-- Wajib ON untuk computed column PERSISTED & filtered index
-- (SSMS sudah ON secara default, sqlcmd tidak).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE master;
GO

IF DB_ID('HunianKu') IS NOT NULL
BEGIN
    ALTER DATABASE HunianKu SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HunianKu;
END
GO

CREATE DATABASE HunianKu;
GO

USE HunianKu;
GO

/* =====================================================================
   1. TABEL MASTER
   ===================================================================== */

-- Akun login semua role. Nama lengkap disimpan di sini (tidak diulang
-- di tabel Penghuni) supaya tidak ada data ganda (3NF).
CREATE TABLE Users (
    UserId        INT IDENTITY(1,1) NOT NULL,
    Username      VARCHAR(50)    NOT NULL,
    PasswordHash  VARCHAR(200)   NOT NULL,   -- format: PBKDF2$iterasi$salt$hash
    NamaLengkap   NVARCHAR(100)  NOT NULL,
    Role          VARCHAR(20)    NOT NULL,
    IsAktif       BIT            NOT NULL CONSTRAINT DF_Users_IsAktif DEFAULT 1,
    CreatedAt     DATETIME2(0)   NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Staf', 'Penghuni')),
    CONSTRAINT CK_Users_Username CHECK (LEN(Username) >= 3)
);

-- Pengaturan tarif yang bisa diubah Admin tanpa mengubah kode program.
CREATE TABLE Tarif (
    KodeTarif   VARCHAR(30)    NOT NULL,
    NamaTarif   NVARCHAR(100)  NOT NULL,
    Nilai       DECIMAL(18,2)  NOT NULL,
    Satuan      NVARCHAR(30)   NOT NULL,
    DiubahPada  DATETIME2(0)   NOT NULL CONSTRAINT DF_Tarif_DiubahPada DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Tarif PRIMARY KEY (KodeTarif),
    CONSTRAINT CK_Tarif_Nilai CHECK (Nilai >= 0)
);

CREATE TABLE Tower (
    TowerId       INT IDENTITY(1,1) NOT NULL,
    KodeTower     VARCHAR(5)     NOT NULL,
    NamaTower     NVARCHAR(100)  NOT NULL,
    Alamat        NVARCHAR(200)  NULL,
    JumlahLantai  INT            NOT NULL,
    CONSTRAINT PK_Tower PRIMARY KEY (TowerId),
    CONSTRAINT UQ_Tower_Kode UNIQUE (KodeTower),
    CONSTRAINT CK_Tower_JumlahLantai CHECK (JumlahLantai BETWEEN 1 AND 100)
);

CREATE TABLE TipeUnit (
    TipeUnitId        INT IDENTITY(1,1) NOT NULL,
    NamaTipe          NVARCHAR(50)   NOT NULL,
    LuasM2            DECIMAL(6,2)   NOT NULL,
    JumlahKamar       INT            NOT NULL,
    HargaSewaBulanan  DECIMAL(18,2)  NOT NULL,
    IplPerBulan       DECIMAL(18,2)  NOT NULL,   -- IPL = service charge
    CONSTRAINT PK_TipeUnit PRIMARY KEY (TipeUnitId),
    CONSTRAINT UQ_TipeUnit_Nama UNIQUE (NamaTipe),
    CONSTRAINT CK_TipeUnit_Luas CHECK (LuasM2 > 0),
    CONSTRAINT CK_TipeUnit_Kamar CHECK (JumlahKamar >= 0),
    CONSTRAINT CK_TipeUnit_Harga CHECK (HargaSewaBulanan > 0 AND IplPerBulan >= 0)
);

CREATE TABLE Unit (
    UnitId      INT IDENTITY(1,1) NOT NULL,
    TowerId     INT          NOT NULL,
    TipeUnitId  INT          NOT NULL,
    NomorUnit   VARCHAR(10)  NOT NULL,
    Lantai      INT          NOT NULL,
    Status      VARCHAR(15)  NOT NULL CONSTRAINT DF_Unit_Status DEFAULT 'Kosong',
    CONSTRAINT PK_Unit PRIMARY KEY (UnitId),
    CONSTRAINT FK_Unit_Tower FOREIGN KEY (TowerId) REFERENCES Tower(TowerId),
    CONSTRAINT FK_Unit_TipeUnit FOREIGN KEY (TipeUnitId) REFERENCES TipeUnit(TipeUnitId),
    CONSTRAINT UQ_Unit_TowerNomor UNIQUE (TowerId, NomorUnit),   -- nomor unik per tower
    CONSTRAINT CK_Unit_Status CHECK (Status IN ('Kosong', 'Terisi', 'Maintenance')),
    CONSTRAINT CK_Unit_Lantai CHECK (Lantai >= 0)
);

-- Data pribadi penghuni. Setiap penghuni wajib punya akun login (1:1 dengan Users).
CREATE TABLE Penghuni (
    PenghuniId     INT IDENTITY(1,1) NOT NULL,
    UserId         INT            NOT NULL,
    NIK            CHAR(16)       NOT NULL,
    JenisKelamin   CHAR(1)        NOT NULL,
    NoHp           VARCHAR(20)    NOT NULL,
    Email          VARCHAR(100)   NULL,
    Pekerjaan      NVARCHAR(100)  NULL,
    KontakDarurat  NVARCHAR(150)  NULL,
    CONSTRAINT PK_Penghuni PRIMARY KEY (PenghuniId),
    CONSTRAINT FK_Penghuni_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Penghuni_UserId UNIQUE (UserId),
    CONSTRAINT UQ_Penghuni_NIK UNIQUE (NIK),
    CONSTRAINT CK_Penghuni_NIK CHECK (LEN(NIK) = 16 AND NIK NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_Penghuni_JK CHECK (JenisKelamin IN ('L', 'P')),
    CONSTRAINT CK_Penghuni_NoHp CHECK (NoHp NOT LIKE '%[^0-9+]%')
);

-- Teknisi yang bisa ditugaskan untuk komplain/perbaikan.
CREATE TABLE Teknisi (
    TeknisiId    INT IDENTITY(1,1) NOT NULL,
    NamaTeknisi  NVARCHAR(100) NOT NULL,
    Keahlian     NVARCHAR(50)  NOT NULL,
    NoHp         VARCHAR(20)   NOT NULL,
    IsAktif      BIT           NOT NULL CONSTRAINT DF_Teknisi_IsAktif DEFAULT 1,
    CONSTRAINT PK_Teknisi PRIMARY KEY (TeknisiId)
);

CREATE TABLE Fasilitas (
    FasilitasId     INT IDENTITY(1,1) NOT NULL,
    NamaFasilitas   NVARCHAR(100) NOT NULL,
    Lokasi          NVARCHAR(100) NOT NULL,
    Kapasitas       INT           NOT NULL,
    JamBuka         TIME(0)       NOT NULL,
    JamTutup        TIME(0)       NOT NULL,
    MaksDurasiJam   INT           NOT NULL,
    IsAktif         BIT           NOT NULL CONSTRAINT DF_Fasilitas_IsAktif DEFAULT 1,
    CONSTRAINT PK_Fasilitas PRIMARY KEY (FasilitasId),
    CONSTRAINT UQ_Fasilitas_Nama UNIQUE (NamaFasilitas),
    CONSTRAINT CK_Fasilitas_Jam CHECK (JamTutup > JamBuka),
    CONSTRAINT CK_Fasilitas_Kapasitas CHECK (Kapasitas > 0),
    CONSTRAINT CK_Fasilitas_Durasi CHECK (MaksDurasiJam BETWEEN 1 AND 12)
);
GO

/* =====================================================================
   2. TABEL TRANSAKSI
   ===================================================================== */

CREATE TABLE Kontrak (
    KontrakId         INT IDENTITY(1,1) NOT NULL,
    -- Nomor kontrak dibuat otomatis dari ID, contoh: KTR-00012
    NomorKontrak      AS ('KTR-' + RIGHT('00000' + CAST(KontrakId AS VARCHAR(10)), 5)) PERSISTED,
    UnitId            INT            NOT NULL,
    PenghuniId        INT            NOT NULL,
    TanggalMulai      DATE           NOT NULL,
    TanggalSelesai    DATE           NOT NULL,
    -- Harga disalin dari TipeUnit saat kontrak dibuat, karena harga yang
    -- disepakati di kontrak tidak boleh ikut berubah bila tarif tipe naik.
    HargaSewaBulanan  DECIMAL(18,2)  NOT NULL,
    Deposit           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Kontrak_Deposit DEFAULT 0,
    Status            VARCHAR(15)    NOT NULL CONSTRAINT DF_Kontrak_Status DEFAULT 'Aktif',
    CreatedAt         DATETIME2(0)   NOT NULL CONSTRAINT DF_Kontrak_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Kontrak PRIMARY KEY (KontrakId),
    CONSTRAINT FK_Kontrak_Unit FOREIGN KEY (UnitId) REFERENCES Unit(UnitId),
    CONSTRAINT FK_Kontrak_Penghuni FOREIGN KEY (PenghuniId) REFERENCES Penghuni(PenghuniId),
    CONSTRAINT CK_Kontrak_Periode CHECK (TanggalSelesai > TanggalMulai),
    CONSTRAINT CK_Kontrak_Nilai CHECK (HargaSewaBulanan > 0 AND Deposit >= 0),
    CONSTRAINT CK_Kontrak_Status CHECK (Status IN ('Aktif', 'Selesai', 'Dibatalkan'))
);

-- Catatan meteran listrik (kWh) dan air (m3) per unit per bulan.
CREATE TABLE Meteran (
    MeteranId     INT IDENTITY(1,1) NOT NULL,
    UnitId        INT            NOT NULL,
    Periode       DATE           NOT NULL,   -- selalu tanggal 1, contoh 2026-09-01 = September 2026
    ListrikAwal   DECIMAL(10,2)  NOT NULL,
    ListrikAkhir  DECIMAL(10,2)  NOT NULL,
    AirAwal       DECIMAL(10,2)  NOT NULL,
    AirAkhir      DECIMAL(10,2)  NOT NULL,
    DicatatOleh   INT            NOT NULL,
    TanggalCatat  DATETIME2(0)   NOT NULL CONSTRAINT DF_Meteran_TanggalCatat DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Meteran PRIMARY KEY (MeteranId),
    CONSTRAINT FK_Meteran_Unit FOREIGN KEY (UnitId) REFERENCES Unit(UnitId),
    CONSTRAINT FK_Meteran_Users FOREIGN KEY (DicatatOleh) REFERENCES Users(UserId),
    CONSTRAINT UQ_Meteran_UnitPeriode UNIQUE (UnitId, Periode),   -- 1 unit hanya 1 catatan per bulan
    CONSTRAINT CK_Meteran_Periode CHECK (DAY(Periode) = 1),
    CONSTRAINT CK_Meteran_Listrik CHECK (ListrikAwal >= 0 AND ListrikAkhir >= ListrikAwal),
    CONSTRAINT CK_Meteran_Air CHECK (AirAwal >= 0 AND AirAkhir >= AirAwal)
);

CREATE TABLE Tagihan (
    TagihanId          INT IDENTITY(1,1) NOT NULL,
    KontrakId          INT            NOT NULL,
    MeteranId          INT            NOT NULL,
    Periode            DATE           NOT NULL,
    -- Nomor tagihan otomatis, contoh: INV-202609-0003
    NomorTagihan       AS ('INV-' + CONVERT(CHAR(6), Periode, 112) + '-'
                           + RIGHT('0000' + CAST(KontrakId AS VARCHAR(10)), 4)) PERSISTED,
    TanggalJatuhTempo  DATE           NOT NULL,
    -- Biaya disimpan (snapshot) agar tagihan lama tidak berubah saat tarif diganti.
    BiayaSewa          DECIMAL(18,2)  NOT NULL,
    BiayaListrik       DECIMAL(18,2)  NOT NULL,
    BiayaAir           DECIMAL(18,2)  NOT NULL,
    BiayaIpl           DECIMAL(18,2)  NOT NULL,
    TotalTagihan       AS (BiayaSewa + BiayaListrik + BiayaAir + BiayaIpl) PERSISTED,
    Denda              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Tagihan_Denda DEFAULT 0,
    Status             VARCHAR(20)    NOT NULL CONSTRAINT DF_Tagihan_Status DEFAULT 'Belum Bayar',
    TanggalDibuat      DATETIME2(0)   NOT NULL CONSTRAINT DF_Tagihan_TanggalDibuat DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Tagihan PRIMARY KEY (TagihanId),
    CONSTRAINT FK_Tagihan_Kontrak FOREIGN KEY (KontrakId) REFERENCES Kontrak(KontrakId),
    CONSTRAINT FK_Tagihan_Meteran FOREIGN KEY (MeteranId) REFERENCES Meteran(MeteranId),
    CONSTRAINT UQ_Tagihan_KontrakPeriode UNIQUE (KontrakId, Periode),   -- tidak ada tagihan dobel
    CONSTRAINT UQ_Tagihan_Meteran UNIQUE (MeteranId),
    CONSTRAINT CK_Tagihan_Periode CHECK (DAY(Periode) = 1),
    CONSTRAINT CK_Tagihan_Biaya CHECK (BiayaSewa >= 0 AND BiayaListrik >= 0 AND BiayaAir >= 0
                                       AND BiayaIpl >= 0 AND Denda >= 0),
    CONSTRAINT CK_Tagihan_Status CHECK (Status IN ('Belum Bayar', 'Menunggu Verifikasi', 'Lunas'))
);

CREATE TABLE Pembayaran (
    PembayaranId       INT IDENTITY(1,1) NOT NULL,
    TagihanId          INT            NOT NULL,
    TanggalBayar       DATE           NOT NULL,
    JumlahBayar        DECIMAL(18,2)  NOT NULL,
    MetodeBayar        VARCHAR(20)    NOT NULL,
    NomorReferensi     VARCHAR(50)    NULL,
    Status             VARCHAR(15)    NOT NULL CONSTRAINT DF_Pembayaran_Status DEFAULT 'Menunggu',
    DiverifikasiOleh   INT            NULL,
    TanggalVerifikasi  DATETIME2(0)   NULL,
    Catatan            NVARCHAR(250)  NULL,
    CONSTRAINT PK_Pembayaran PRIMARY KEY (PembayaranId),
    CONSTRAINT FK_Pembayaran_Tagihan FOREIGN KEY (TagihanId) REFERENCES Tagihan(TagihanId),
    CONSTRAINT FK_Pembayaran_Users FOREIGN KEY (DiverifikasiOleh) REFERENCES Users(UserId),
    CONSTRAINT CK_Pembayaran_Jumlah CHECK (JumlahBayar > 0),
    CONSTRAINT CK_Pembayaran_Metode CHECK (MetodeBayar IN ('Transfer', 'E-Wallet', 'Tunai')),
    CONSTRAINT CK_Pembayaran_Status CHECK (Status IN ('Menunggu', 'Diterima', 'Ditolak')),
    -- Pembayaran yang sudah diverifikasi wajib mencatat siapa verifikatornya.
    CONSTRAINT CK_Pembayaran_Verifikasi CHECK (
        (Status = 'Menunggu' AND DiverifikasiOleh IS NULL)
        OR (Status IN ('Diterima', 'Ditolak') AND DiverifikasiOleh IS NOT NULL AND TanggalVerifikasi IS NOT NULL))
);

CREATE TABLE Komplain (
    KomplainId          INT IDENTITY(1,1) NOT NULL,
    NomorTiket          AS ('TKT-' + RIGHT('00000' + CAST(KomplainId AS VARCHAR(10)), 5)) PERSISTED,
    PenghuniId          INT            NOT NULL,
    UnitId              INT            NOT NULL,
    Kategori            VARCHAR(20)    NOT NULL,
    Judul               NVARCHAR(100)  NOT NULL,
    Deskripsi           NVARCHAR(1000) NOT NULL,
    Prioritas           VARCHAR(10)    NOT NULL CONSTRAINT DF_Komplain_Prioritas DEFAULT 'Sedang',
    Status              VARCHAR(15)    NOT NULL CONSTRAINT DF_Komplain_Status DEFAULT 'Diajukan',
    TeknisiId           INT            NULL,
    TanggalAjukan       DATETIME2(0)   NOT NULL CONSTRAINT DF_Komplain_TanggalAjukan DEFAULT SYSDATETIME(),
    TanggalSelesai      DATETIME2(0)   NULL,
    CatatanPenanganan   NVARCHAR(500)  NULL,
    CONSTRAINT PK_Komplain PRIMARY KEY (KomplainId),
    CONSTRAINT FK_Komplain_Penghuni FOREIGN KEY (PenghuniId) REFERENCES Penghuni(PenghuniId),
    CONSTRAINT FK_Komplain_Unit FOREIGN KEY (UnitId) REFERENCES Unit(UnitId),
    CONSTRAINT FK_Komplain_Teknisi FOREIGN KEY (TeknisiId) REFERENCES Teknisi(TeknisiId),
    CONSTRAINT CK_Komplain_Kategori CHECK (Kategori IN ('Listrik', 'Air', 'AC', 'Bangunan', 'Kebersihan', 'Lainnya')),
    CONSTRAINT CK_Komplain_Prioritas CHECK (Prioritas IN ('Rendah', 'Sedang', 'Tinggi')),
    CONSTRAINT CK_Komplain_Status CHECK (Status IN ('Diajukan', 'Diproses', 'Selesai')),
    -- Komplain hanya bisa diproses/selesai jika sudah ada teknisi.
    CONSTRAINT CK_Komplain_Teknisi CHECK (Status = 'Diajukan' OR TeknisiId IS NOT NULL),
    -- Tanggal selesai terisi jika dan hanya jika status Selesai.
    CONSTRAINT CK_Komplain_Selesai CHECK (
        (Status = 'Selesai' AND TanggalSelesai IS NOT NULL)
        OR (Status <> 'Selesai' AND TanggalSelesai IS NULL))
);

CREATE TABLE BookingFasilitas (
    BookingId    INT IDENTITY(1,1) NOT NULL,
    FasilitasId  INT            NOT NULL,
    PenghuniId   INT            NOT NULL,
    Tanggal      DATE           NOT NULL,
    JamMulai     TIME(0)        NOT NULL,
    JamSelesai   TIME(0)        NOT NULL,
    JumlahOrang  INT            NOT NULL,
    Keperluan    NVARCHAR(200)  NULL,
    Status       VARCHAR(15)    NOT NULL CONSTRAINT DF_Booking_Status DEFAULT 'Aktif',
    CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT DF_Booking_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_BookingFasilitas PRIMARY KEY (BookingId),
    CONSTRAINT FK_Booking_Fasilitas FOREIGN KEY (FasilitasId) REFERENCES Fasilitas(FasilitasId),
    CONSTRAINT FK_Booking_Penghuni FOREIGN KEY (PenghuniId) REFERENCES Penghuni(PenghuniId),
    CONSTRAINT CK_Booking_Jam CHECK (JamSelesai > JamMulai),
    CONSTRAINT CK_Booking_JumlahOrang CHECK (JumlahOrang > 0),
    CONSTRAINT CK_Booking_Status CHECK (Status IN ('Aktif', 'Dibatalkan'))
);
GO

/* =====================================================================
   3. INDEX
   Index mempercepat pencarian di kolom yang sering dipakai pada WHERE/JOIN.
   ===================================================================== */

CREATE INDEX IX_Unit_Status        ON Unit(Status);
CREATE INDEX IX_Unit_TipeUnit      ON Unit(TipeUnitId);
CREATE INDEX IX_Kontrak_Penghuni   ON Kontrak(PenghuniId);
CREATE INDEX IX_Kontrak_Selesai    ON Kontrak(Status, TanggalSelesai);
CREATE INDEX IX_Tagihan_Status     ON Tagihan(Status, TanggalJatuhTempo);
CREATE INDEX IX_Tagihan_Periode    ON Tagihan(Periode);
CREATE INDEX IX_Pembayaran_Tagihan ON Pembayaran(TagihanId);
CREATE INDEX IX_Pembayaran_Status  ON Pembayaran(Status, TanggalBayar);
CREATE INDEX IX_Komplain_Status    ON Komplain(Status);
CREATE INDEX IX_Komplain_Penghuni  ON Komplain(PenghuniId);
CREATE INDEX IX_Booking_Jadwal     ON BookingFasilitas(FasilitasId, Tanggal, Status);
CREATE INDEX IX_Booking_Penghuni   ON BookingFasilitas(PenghuniId);

-- Filtered unique index = aturan bisnis yang dijaga database:
-- 1 unit hanya boleh punya 1 kontrak Aktif, dan 1 penghuni hanya 1 kontrak Aktif.
CREATE UNIQUE INDEX UX_Kontrak_UnitAktif     ON Kontrak(UnitId)     WHERE Status = 'Aktif';
CREATE UNIQUE INDEX UX_Kontrak_PenghuniAktif ON Kontrak(PenghuniId) WHERE Status = 'Aktif';
-- 1 tagihan hanya boleh punya 1 pembayaran yang Diterima.
CREATE UNIQUE INDEX UX_Pembayaran_Diterima   ON Pembayaran(TagihanId) WHERE Status = 'Diterima';
GO

/* =====================================================================
   4. FUNCTION
   ===================================================================== */

-- Menghitung denda: (jumlah hari lewat jatuh tempo) x (tarif denda per hari).
-- Contoh: jatuh tempo 10 Okt, dihitung 15 Okt, tarif 10.000 -> 5 x 10.000 = 50.000
CREATE FUNCTION dbo.fn_HitungDenda (@TanggalJatuhTempo DATE, @TanggalAcuan DATE)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @HariTerlambat INT = DATEDIFF(DAY, @TanggalJatuhTempo, @TanggalAcuan);
    IF @HariTerlambat <= 0
        RETURN 0;

    DECLARE @DendaPerHari DECIMAL(18,2) =
        ISNULL((SELECT Nilai FROM Tarif WHERE KodeTarif = 'DENDA_PER_HARI'), 0);

    RETURN @HariTerlambat * @DendaPerHari;
END
GO

/* =====================================================================
   5. VIEW
   ===================================================================== */

-- Tagihan lengkap dengan nama penghuni, unit, pemakaian, dan total akhir.
CREATE VIEW dbo.vw_TagihanDetail
AS
SELECT
    t.TagihanId, t.NomorTagihan, t.KontrakId, k.NomorKontrak,
    p.PenghuniId, us.NamaLengkap AS NamaPenghuni,
    u.UnitId, tw.KodeTower + '-' + u.NomorUnit AS KodeUnit, tw.NamaTower,
    t.Periode, t.TanggalJatuhTempo,
    m.ListrikAkhir - m.ListrikAwal AS PemakaianListrik,
    m.AirAkhir - m.AirAwal         AS PemakaianAir,
    t.BiayaSewa, t.BiayaListrik, t.BiayaAir, t.BiayaIpl,
    t.TotalTagihan, t.Denda,
    t.TotalTagihan + t.Denda AS GrandTotal,
    t.Status,
    CASE WHEN t.Status = 'Belum Bayar' AND CAST(GETDATE() AS DATE) > t.TanggalJatuhTempo
         THEN DATEDIFF(DAY, t.TanggalJatuhTempo, CAST(GETDATE() AS DATE))
         ELSE 0 END AS HariTerlambat,
    t.TanggalDibuat
FROM Tagihan t
JOIN Kontrak  k  ON k.KontrakId  = t.KontrakId
JOIN Penghuni p  ON p.PenghuniId = k.PenghuniId
JOIN Users    us ON us.UserId    = p.UserId
JOIN Unit     u  ON u.UnitId     = k.UnitId
JOIN Tower    tw ON tw.TowerId   = u.TowerId
JOIN Meteran  m  ON m.MeteranId  = t.MeteranId;
GO

-- Okupansi per tower: berapa unit terisi dibanding total unit.
CREATE VIEW dbo.vw_OkupansiTower
AS
SELECT
    tw.TowerId, tw.KodeTower, tw.NamaTower,
    COUNT(u.UnitId) AS TotalUnit,
    SUM(CASE WHEN u.Status = 'Terisi'      THEN 1 ELSE 0 END) AS UnitTerisi,
    SUM(CASE WHEN u.Status = 'Kosong'      THEN 1 ELSE 0 END) AS UnitKosong,
    SUM(CASE WHEN u.Status = 'Maintenance' THEN 1 ELSE 0 END) AS UnitMaintenance,
    CAST(CASE WHEN COUNT(u.UnitId) = 0 THEN 0
              ELSE SUM(CASE WHEN u.Status = 'Terisi' THEN 1 ELSE 0 END) * 100.0 / COUNT(u.UnitId)
         END AS DECIMAL(5,2)) AS PersenOkupansi
FROM Tower tw
LEFT JOIN Unit u ON u.TowerId = tw.TowerId
GROUP BY tw.TowerId, tw.KodeTower, tw.NamaTower;
GO

-- Kontrak aktif yang akan berakhir dalam N hari (N diambil dari tabel Tarif).
CREATE VIEW dbo.vw_KontrakHampirHabis
AS
SELECT
    k.KontrakId, k.NomorKontrak, p.PenghuniId, us.NamaLengkap AS NamaPenghuni, p.NoHp,
    tw.KodeTower + '-' + u.NomorUnit AS KodeUnit,
    k.TanggalMulai, k.TanggalSelesai,
    DATEDIFF(DAY, CAST(GETDATE() AS DATE), k.TanggalSelesai) AS SisaHari
FROM Kontrak k
JOIN Penghuni p  ON p.PenghuniId = k.PenghuniId
JOIN Users    us ON us.UserId    = p.UserId
JOIN Unit     u  ON u.UnitId     = k.UnitId
JOIN Tower    tw ON tw.TowerId   = u.TowerId
WHERE k.Status = 'Aktif'
  AND k.TanggalSelesai >= CAST(GETDATE() AS DATE)
  AND k.TanggalSelesai <= DATEADD(DAY,
        ISNULL((SELECT CAST(Nilai AS INT) FROM Tarif WHERE KodeTarif = 'PENGINGAT_KONTRAK_HARI'), 30),
        CAST(GETDATE() AS DATE));
GO

/* =====================================================================
   6. STORED PROCEDURE
   ===================================================================== */

-- Membuat tagihan bulanan untuk semua kontrak yang berlaku pada @Periode.
-- Tagihan = sewa + (pemakaian listrik x tarif) + (pemakaian air x tarif) + IPL.
-- Kontrak yang meterannya belum diinput dilewati (dilaporkan di hasil).
-- Aman dijalankan berkali-kali: tagihan yang sudah ada tidak dibuat ulang.
CREATE PROCEDURE dbo.sp_GenerateTagihanBulanan
    @Periode DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Normalisasi ke tanggal 1 (mis. 2026-09-17 -> 2026-09-01)
    SET @Periode = DATEFROMPARTS(YEAR(@Periode), MONTH(@Periode), 1);

    DECLARE @TarifListrik DECIMAL(18,2) = (SELECT Nilai FROM Tarif WHERE KodeTarif = 'LISTRIK_PER_KWH');
    DECLARE @TarifAir     DECIMAL(18,2) = (SELECT Nilai FROM Tarif WHERE KodeTarif = 'AIR_PER_M3');
    DECLARE @TglTempo     INT           = (SELECT CAST(Nilai AS INT) FROM Tarif WHERE KodeTarif = 'JATUH_TEMPO_TANGGAL');

    IF @TarifListrik IS NULL OR @TarifAir IS NULL OR @TglTempo IS NULL
        THROW 50010, 'Tarif listrik/air/jatuh tempo belum diatur di tabel Tarif.', 1;

    -- Jatuh tempo = tanggal @TglTempo pada bulan berikutnya.
    DECLARE @BulanDepan DATE = DATEADD(MONTH, 1, @Periode);
    DECLARE @JatuhTempo DATE = DATEFROMPARTS(YEAR(@BulanDepan), MONTH(@BulanDepan), @TglTempo);

    -- Kontrak yang berlaku (minimal sebagian) pada bulan @Periode.
    CREATE TABLE #Hasil (
        KontrakId INT, NomorKontrak VARCHAR(20), KodeUnit VARCHAR(20),
        NamaPenghuni NVARCHAR(100), MeteranId INT NULL, Keterangan VARCHAR(30));

    INSERT INTO #Hasil (KontrakId, NomorKontrak, KodeUnit, NamaPenghuni, MeteranId, Keterangan)
    SELECT k.KontrakId, k.NomorKontrak, tw.KodeTower + '-' + u.NomorUnit, us.NamaLengkap, m.MeteranId,
           CASE WHEN EXISTS (SELECT 1 FROM Tagihan x WHERE x.KontrakId = k.KontrakId AND x.Periode = @Periode)
                     THEN 'Sudah ada'
                WHEN m.MeteranId IS NULL THEN 'Meteran belum diinput'
                ELSE 'Dibuat' END
    FROM Kontrak k
    JOIN Unit     u  ON u.UnitId = k.UnitId
    JOIN Tower    tw ON tw.TowerId = u.TowerId
    JOIN Penghuni p  ON p.PenghuniId = k.PenghuniId
    JOIN Users    us ON us.UserId = p.UserId
    LEFT JOIN Meteran m ON m.UnitId = k.UnitId AND m.Periode = @Periode
    WHERE k.Status <> 'Dibatalkan'
      AND k.TanggalMulai   <= EOMONTH(@Periode)
      AND k.TanggalSelesai >= @Periode;

    -- Satu perintah INSERT...SELECT sudah atomik: semua tagihan masuk, atau
    -- tidak sama sekali jika ada error (XACT_ABORT ON).
    INSERT INTO Tagihan (KontrakId, MeteranId, Periode, TanggalJatuhTempo,
                         BiayaSewa, BiayaListrik, BiayaAir, BiayaIpl)
    SELECT k.KontrakId, m.MeteranId, @Periode, @JatuhTempo,
           k.HargaSewaBulanan,
           (m.ListrikAkhir - m.ListrikAwal) * @TarifListrik,
           (m.AirAkhir - m.AirAwal) * @TarifAir,
           tu.IplPerBulan
    FROM #Hasil h
    JOIN Kontrak  k  ON k.KontrakId = h.KontrakId
    JOIN Meteran  m  ON m.MeteranId = h.MeteranId
    JOIN Unit     u  ON u.UnitId = k.UnitId
    JOIN TipeUnit tu ON tu.TipeUnitId = u.TipeUnitId
    WHERE h.Keterangan = 'Dibuat';

    SELECT NomorKontrak, KodeUnit, NamaPenghuni, Keterangan
    FROM #Hasil
    ORDER BY Keterangan, KodeUnit;
END
GO

-- Memperbarui denda semua tagihan yang belum dibayar sesuai tanggal hari ini.
-- Dipanggil aplikasi setiap kali daftar tagihan/laporan dibuka.
-- Tagihan "Menunggu Verifikasi"/"Lunas" tidak diubah (dendanya sudah dikunci saat bayar).
CREATE PROCEDURE dbo.sp_HitungDenda
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Tagihan
    SET Denda = dbo.fn_HitungDenda(TanggalJatuhTempo, CAST(GETDATE() AS DATE))
    WHERE Status = 'Belum Bayar';
END
GO

-- Laporan tunggakan: tagihan "Belum Bayar" yang sudah lewat jatuh tempo.
CREATE PROCEDURE dbo.sp_LaporanTunggakan
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_HitungDenda;   -- pastikan denda terbaru

    SELECT NomorTagihan, NamaPenghuni, KodeUnit, Periode, TanggalJatuhTempo,
           HariTerlambat, TotalTagihan, Denda, GrandTotal
    FROM dbo.vw_TagihanDetail
    WHERE Status = 'Belum Bayar'
      AND TanggalJatuhTempo < CAST(GETDATE() AS DATE)
    ORDER BY NamaPenghuni, Periode;
END
GO

-- Laporan pendapatan per bulan dalam 1 tahun (dari pembayaran yang Diterima).
CREATE PROCEDURE dbo.sp_LaporanPendapatan
    @Tahun INT
AS
BEGIN
    SET NOCOUNT ON;

    WITH Bulan AS (
        SELECT n FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS v(n)
    )
    SELECT
        b.n AS Bulan,
        COUNT(pb.PembayaranId)            AS JumlahTransaksi,
        ISNULL(SUM(t.BiayaSewa), 0)       AS Sewa,
        ISNULL(SUM(t.BiayaListrik), 0)    AS Listrik,
        ISNULL(SUM(t.BiayaAir), 0)        AS Air,
        ISNULL(SUM(t.BiayaIpl), 0)        AS Ipl,
        ISNULL(SUM(t.Denda), 0)           AS Denda,
        ISNULL(SUM(pb.JumlahBayar), 0)    AS TotalPendapatan
    FROM Bulan b
    LEFT JOIN Pembayaran pb
           ON pb.Status = 'Diterima'
          AND YEAR(pb.TanggalBayar) = @Tahun
          AND MONTH(pb.TanggalBayar) = b.n
    LEFT JOIN Tagihan t ON t.TagihanId = pb.TagihanId
    GROUP BY b.n
    ORDER BY b.n;
END
GO

-- Booking fasilitas dengan validasi lengkap, termasuk cek jadwal bentrok.
-- Dua jadwal bentrok jika: MulaiBaru < SelesaiLama DAN SelesaiBaru > MulaiLama.
-- UPDLOCK + HOLDLOCK mencegah dua orang mem-booking slot yang sama bersamaan.
CREATE PROCEDURE dbo.sp_BuatBooking
    @FasilitasId INT,
    @PenghuniId  INT,
    @Tanggal     DATE,
    @JamMulai    TIME(0),
    @JamSelesai  TIME(0),
    @JumlahOrang INT,
    @Keperluan   NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- jika THROW terjadi, transaksi otomatis di-rollback

    DECLARE @JamBuka TIME(0), @JamTutup TIME(0), @Kapasitas INT, @MaksDurasi INT, @Aktif BIT;
    SELECT @JamBuka = JamBuka, @JamTutup = JamTutup, @Kapasitas = Kapasitas,
           @MaksDurasi = MaksDurasiJam, @Aktif = IsAktif
    FROM Fasilitas WHERE FasilitasId = @FasilitasId;

    IF @JamBuka IS NULL OR @Aktif = 0
        THROW 50001, 'Fasilitas tidak ditemukan atau sedang tidak aktif.', 1;
    IF NOT EXISTS (SELECT 1 FROM Kontrak WHERE PenghuniId = @PenghuniId AND Status = 'Aktif')
        THROW 50002, 'Hanya penghuni dengan kontrak aktif yang dapat melakukan booking.', 1;
    IF @Tanggal < CAST(GETDATE() AS DATE)
       OR (@Tanggal = CAST(GETDATE() AS DATE) AND @JamMulai <= CAST(GETDATE() AS TIME(0)))
        THROW 50003, 'Waktu booking sudah lewat. Pilih tanggal/jam yang akan datang.', 1;
    IF @JamSelesai <= @JamMulai
        THROW 50004, 'Jam selesai harus lebih besar dari jam mulai.', 1;
    IF @JamMulai < @JamBuka OR @JamSelesai > @JamTutup
        THROW 50005, 'Jam booking di luar jam operasional fasilitas.', 1;
    IF DATEDIFF(MINUTE, @JamMulai, @JamSelesai) > @MaksDurasi * 60
        THROW 50006, 'Durasi booking melebihi batas maksimal fasilitas.', 1;
    IF @JumlahOrang < 1 OR @JumlahOrang > @Kapasitas
        THROW 50007, 'Jumlah orang melebihi kapasitas fasilitas.', 1;

    DECLARE @BookingId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1 FROM BookingFasilitas WITH (UPDLOCK, HOLDLOCK)
            WHERE FasilitasId = @FasilitasId
              AND Tanggal = @Tanggal
              AND Status = 'Aktif'
              AND @JamMulai < JamSelesai
              AND @JamSelesai > JamMulai)
            THROW 50008, 'Jadwal bentrok: fasilitas sudah dibooking pada jam tersebut.', 1;

        INSERT INTO BookingFasilitas (FasilitasId, PenghuniId, Tanggal, JamMulai, JamSelesai, JumlahOrang, Keperluan)
        VALUES (@FasilitasId, @PenghuniId, @Tanggal, @JamMulai, @JamSelesai, @JumlahOrang, @Keperluan);
        SET @BookingId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;   -- teruskan pesan error ke aplikasi
    END CATCH

    SELECT @BookingId AS BookingId;
END
GO

/* =====================================================================
   7. DATA DUMMY
   Tanggal dibuat relatif terhadap hari ini (GETDATE) supaya demo selalu
   realistis: ada tagihan 3 bulan terakhir, tunggakan, kontrak hampir
   habis, komplain berjalan, dan booking mendatang.
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO Tarif (KodeTarif, NamaTarif, Nilai, Satuan) VALUES
('LISTRIK_PER_KWH',        N'Tarif listrik',                     1700,  N'Rp/kWh'),
('AIR_PER_M3',             N'Tarif air',                         9500,  N'Rp/m3'),
('DENDA_PER_HARI',         N'Denda keterlambatan',               10000, N'Rp/hari'),
('JATUH_TEMPO_TANGGAL',    N'Tanggal jatuh tempo (bulan berikutnya)', 10, N'tanggal'),
('PENGINGAT_KONTRAK_HARI', N'Pengingat kontrak hampir habis',    30,    N'hari');

INSERT INTO Users (Username, PasswordHash, NamaLengkap, Role) VALUES
('admin',   'PBKDF2$100000$ogvKSko1JOQFXGtEPW1oqg==$7fP5zYnLlBNxz31Blef2lgzr76UmtvLFp2CqRKOcVXc=', N'Hendra Wijaya',      'Admin'),
('rina',    'PBKDF2$100000$DVfubcjuoIQ3CfHYIvZAdw==$PvthHk2I6ZG35yjOhU9mkMPK9LwPiloeGoAxAHDID5c=', N'Rina Marlina',       'Staf'),
('dodi',    'PBKDF2$100000$a+rp70Znes69EuLCAx99og==$+d57ZhamRjBZ3ZJaYMnxt5GiplQ3pJ6rs9XT6pC9s9s=', N'Dodi Prasetyo',      'Staf'),
('budi',    'PBKDF2$100000$S4SS9Lp9x7xfVs5ijeTYJA==$TcOU7ZsY8zdTAALw7ZxK+9daF2z0OL0OvWPrxCRChmo=', N'Budi Santoso',       'Penghuni'),
('siti',    'PBKDF2$100000$rsGUYzHccL74u+tx25WfwQ==$lfAhhTYtwoPXsYM3Cxs9UOxOu+WTqS7ZPaKpDM8cz5s=', N'Siti Rahmawati',     'Penghuni'),
('andi',    'PBKDF2$100000$RARvFY64H3hx3MHidAM54A==$UD86XGFOJT7/oVTM21n/ArgVbyh4+1NRQoOauKgOw7o=', N'Andi Saputra',       'Penghuni'),
('dewi',    'PBKDF2$100000$v1B6AJ7pSCJPEKZWVQ9x+g==$82geFkJafef2ZVJXApe+/KzJ6+oEASSVI1vt2w2Br8Y=', N'Dewi Anggraini',     'Penghuni'),
('rizky',   'PBKDF2$100000$o6RPECZkCCA/UOPabXa1Dw==$rNJH0k16e/eok9vvm9KthtYt0+BRYnT8LUlnNKCSjiw=', N'Rizky Pratama',      'Penghuni'),
('nadia',   'PBKDF2$100000$0uc+AJwm8i2PPAksox3atw==$EyoK6HtZmN2ZhuoLuhdx5/5rH4X+v2hPzj7UagvFghk=', N'Nadia Putri Hapsari','Penghuni'),
('agus',    'PBKDF2$100000$yudOH4dq6vFJf63wfzXsNA==$canqjKUH8N01vOhFwlYkDHJLz4z74LYCf7aG8AeiRGQ=', N'Agus Setiawan',      'Penghuni'),
('maya',    'PBKDF2$100000$UifZpTVhExqr/Blwcm+z+Q==$98Osc3FA6JUVOKTgNOiD5df/xYkWeR65BYwTHud4w50=', N'Maya Kusuma',        'Penghuni'),
('yoga',    'PBKDF2$100000$hCdG5ml3EcRVZYen0IokIg==$eQ/Yp3TjIz38GL7jC2B9n5B/zzAht3f+kqhqfCyIObM=', N'Yoga Aditya',        'Penghuni'),
('putri',   'PBKDF2$100000$HZOustLlRl4RSRroGhBAiA==$zpMklCFnINRyg2IlOuO6uN3XneDMKEkl/uTooq38Rsc=', N'Putri Maharani',     'Penghuni'),
('bayu',    'PBKDF2$100000$Dn+NK3eFd7143pGe78lYlA==$ciK40VnL6Q4mR4tkBpnpYXFqQxtJ0HkTDsMz+eawBGA=', N'Bayu Nugroho',       'Penghuni'),
('lestari', 'PBKDF2$100000$cxFVUV3apoabCkXh9+oRRg==$XMfrDlV9KuHp3quc9+6G//SWG+XmNeW6SFp0rmGlyPM=', N'Lestari Wulandari',  'Penghuni'),
('kevin',   'PBKDF2$100000$jE+SNXUs22F04u/HLRjkHQ==$PTmHz4XC3LADjy6xlu78i8jn6gIeGwDX2/kEg+CrS3U=', N'Kevin Halim',        'Penghuni'),
('sarah',   'PBKDF2$100000$hH/5E+7q1n88hU1kovQ3yQ==$ldxUi+sAUrDNCtAxB8MBXxTZBtIYSHRFu9e9+ts1bqM=', N'Sarah Amelia',       'Penghuni');

INSERT INTO Tower (KodeTower, NamaTower, Alamat, JumlahLantai) VALUES
('A', N'Tower Anggrek',     N'Jl. Raya Kertajaya Indah No. 12, Surabaya', 20),
('B', N'Tower Bougenville', N'Jl. Raya Kertajaya Indah No. 12, Surabaya', 25),
('C', N'Tower Cempaka',     N'Jl. Raya Kertajaya Indah No. 14, Surabaya', 18);

INSERT INTO TipeUnit (NamaTipe, LuasM2, JumlahKamar, HargaSewaBulanan, IplPerBulan) VALUES
(N'Studio',    24, 1,  3500000, 350000),
(N'1 Bedroom', 36, 1,  4800000, 450000),
(N'2 Bedroom', 54, 2,  7200000, 600000),
(N'3 Bedroom', 78, 3, 10500000, 850000);

-- 24 unit (8 per tower). TipeUnitId: 1=Studio, 2=1BR, 3=2BR, 4=3BR
INSERT INTO Unit (TowerId, TipeUnitId, NomorUnit, Lantai, Status) VALUES
(1, 1, '0501',  5, 'Terisi'), (1, 1, '0502',  5, 'Kosong'), (1, 2, '0503',  5, 'Terisi'),
(1, 2, '0805',  8, 'Kosong'), (1, 3, '0806',  8, 'Terisi'), (1, 3, '1201', 12, 'Terisi'),
(1, 4, '1502', 15, 'Kosong'), (1, 1, '1503', 15, 'Maintenance'),
(2, 1, '0301',  3, 'Terisi'), (2, 2, '0302',  3, 'Terisi'), (2, 2, '0303',  3, 'Kosong'),
(2, 3, '0701',  7, 'Terisi'), (2, 3, '0702',  7, 'Kosong'), (2, 4, '1101', 11, 'Terisi'),
(2, 1, '1102', 11, 'Kosong'), (2, 2, '1501', 15, 'Maintenance'),
(3, 1, '0201',  2, 'Terisi'), (3, 1, '0202',  2, 'Kosong'), (3, 2, '0601',  6, 'Terisi'),
(3, 3, '0602',  6, 'Terisi'), (3, 3, '1001', 10, 'Kosong'), (3, 4, '1002', 10, 'Terisi'),
(3, 2, '1401', 14, 'Terisi'), (3, 1, '1402', 14, 'Kosong');

-- 14 penghuni (UserId 4..17). Penghuni ke-14 (Sarah) belum punya kontrak.
INSERT INTO Penghuni (UserId, NIK, JenisKelamin, NoHp, Email, Pekerjaan, KontakDarurat) VALUES
( 4, '3578010101900001', 'L', '081234500001', 'budi.santoso@mail.com',   N'Karyawan Swasta', N'Sri (Istri) 081234599001'),
( 5, '3578014502920002', 'P', '081234500002', 'siti.rahma@mail.com',     N'Dosen',           N'Ahmad (Ayah) 081234599002'),
( 6, '3578011203880003', 'L', '081234500003', 'andi.saputra@mail.com',   N'Wiraswasta',      N'Rina (Istri) 081234599003'),
( 7, '3578015604950004', 'P', '081234500004', 'dewi.ang@mail.com',       N'Dokter',          N'Hadi (Suami) 081234599004'),
( 8, '3578012005970005', 'L', '081234500005', 'rizky.p@mail.com',        N'Programmer',      N'Nur (Ibu) 081234599005'),
( 9, '3578016106980006', 'P', '081234500006', 'nadia.hapsari@mail.com',  N'Desainer',        N'Bambang (Ayah) 081234599006'),
(10, '3578010707850007', 'L', '081234500007', 'agus.setiawan@mail.com',  N'Pedagang',        N'Wati (Istri) 081234599007'),
(11, '3578014808930008', 'P', '081234500008', 'maya.kusuma@mail.com',    N'Notaris',         N'Joko (Suami) 081234599008'),
(12, '3578011109960009', 'L', '081234500009', 'yoga.aditya@mail.com',    N'Mahasiswa S2',    N'Ani (Ibu) 081234599009'),
(13, '3578015210990010', 'P', '081234500010', 'putri.m@mail.com',        N'Perawat',         N'Sugeng (Ayah) 081234599010'),
(14, '3578010111870011', 'L', '081234500011', 'bayu.nugroho@mail.com',   N'PNS',             N'Lina (Istri) 081234599011'),
(15, '3578014412910012', 'P', '081234500012', 'lestari.w@mail.com',      N'Akuntan',         N'Dedi (Suami) 081234599012'),
(16, '3578011501940013', 'L', '081234500013', 'kevin.halim@mail.com',    N'Konsultan IT',    N'Linda (Ibu) 081234599013'),
(17, '3578015602000014', 'P', '081234500014', 'sarah.amelia@mail.com',   N'Fresh Graduate',  N'Robert (Ayah) 081234599014');

INSERT INTO Teknisi (NamaTeknisi, Keahlian, NoHp) VALUES
(N'Joko Susilo',   N'Listrik',          '081300000001'),
(N'Slamet Riyadi', N'Air/Plumbing',     '081300000002'),
(N'Wawan Hermawan',N'AC',               '081300000003'),
(N'Ujang Sopian',  N'Bangunan & Umum',  '081300000004');

INSERT INTO Fasilitas (NamaFasilitas, Lokasi, Kapasitas, JamBuka, JamTutup, MaksDurasiJam) VALUES
(N'Kolam Renang',    N'Tower A Lantai 3', 30, '06:00', '21:00', 2),
(N'Gym',             N'Tower B Lantai 2', 15, '05:00', '22:00', 2),
(N'Ruang Serbaguna', N'Tower C Lantai 1', 80, '08:00', '22:00', 6);
GO

-- ---------- Kontrak, meteran, tagihan, pembayaran (tanggal relatif) ----------
DECLARE @Hari    DATE = CAST(GETDATE() AS DATE);
DECLARE @BulanIni DATE = DATEFROMPARTS(YEAR(@Hari), MONTH(@Hari), 1);

-- Kontrak lama Kevin yang sudah selesai (riwayat), di unit C-1402
INSERT INTO Kontrak (UnitId, PenghuniId, TanggalMulai, TanggalSelesai, HargaSewaBulanan, Deposit, Status)
VALUES (24, 13, DATEADD(MONTH, -16, @BulanIni), DATEADD(DAY, -1, DATEADD(MONTH, -4, @BulanIni)), 3500000, 7000000, 'Selesai');

-- Kontrak aktif: (PenghuniId, UnitId, TanggalMulai, LamaBulan)
-- Harga sewa diambil dari tipe unit, deposit = 2x sewa.
DECLARE @K TABLE (PenghuniId INT, UnitId INT, Mulai DATE, Selesai DATE);
INSERT INTO @K VALUES
( 1,  1, DATEADD(MONTH, -10, @BulanIni), NULL),
( 2,  3, DATEADD(DAY, 13, DATEADD(YEAR, -1, @Hari)), DATEADD(DAY, 12, @Hari)),  -- habis 12 hari lagi
( 3,  5, DATEADD(DAY, 26, DATEADD(YEAR, -1, @Hari)), DATEADD(DAY, 25, @Hari)),  -- habis 25 hari lagi
( 4,  6, DATEADD(MONTH,  -6, @BulanIni), NULL),
( 5,  9, DATEADD(MONTH,  -9, @BulanIni), NULL),
( 6, 10, DATEADD(MONTH,  -5, @BulanIni), NULL),
( 7, 12, DATEADD(MONTH,  -4, @BulanIni), NULL),
( 8, 14, DATEADD(MONTH, -11, @BulanIni), NULL),
( 9, 17, DATEADD(MONTH,  -7, @BulanIni), NULL),
(10, 19, DATEADD(MONTH,  -3, @BulanIni), NULL),
(11, 20, DATEADD(MONTH,  -8, @BulanIni), NULL),
(12, 22, DATEADD(MONTH,  -6, @BulanIni), NULL),
(13, 23, DATEADD(MONTH,  -3, @BulanIni), NULL);

-- Kontrak tanpa tanggal selesai khusus = 1 tahun (kecuali penghuni 5, 8 = 2 tahun)
UPDATE @K SET Selesai = DATEADD(DAY, -1, DATEADD(MONTH, CASE WHEN PenghuniId IN (5, 8) THEN 24 ELSE 12 END, Mulai))
WHERE Selesai IS NULL;

INSERT INTO Kontrak (UnitId, PenghuniId, TanggalMulai, TanggalSelesai, HargaSewaBulanan, Deposit, Status)
SELECT k.UnitId, k.PenghuniId, k.Mulai, k.Selesai, tu.HargaSewaBulanan, tu.HargaSewaBulanan * 2, 'Aktif'
FROM @K k
JOIN Unit u      ON u.UnitId = k.UnitId
JOIN TipeUnit tu ON tu.TipeUnitId = u.TipeUnitId
ORDER BY k.PenghuniId;

-- Meteran 3 bulan terakhir untuk semua unit berkontrak aktif.
-- Ke = 1,2,3 -> 3 bulan lalu, 2 bulan lalu, bulan lalu.
-- Pemakaian dibuat bervariasi per unit dengan rumus sederhana agar realistis.
-- Unit 20, 22, 23 sengaja BELUM diinput untuk bulan lalu (bahan demo Staf).
WITH Bulan AS (SELECT Ke FROM (VALUES (1),(2),(3)) v(Ke))
INSERT INTO Meteran (UnitId, Periode, ListrikAwal, ListrikAkhir, AirAwal, AirAkhir, DicatatOleh, TanggalCatat)
SELECT k.UnitId,
       DATEADD(MONTH, b.Ke - 4, @BulanIni),
       1000 + k.UnitId * 100 + (b.Ke - 1) * (120 + (k.UnitId * 37) % 150) + 5 * (b.Ke - 1) * b.Ke / 2,
       1000 + k.UnitId * 100 + b.Ke * (120 + (k.UnitId * 37) % 150) + 5 * b.Ke * (b.Ke + 1) / 2,
       100 + k.UnitId * 10 + (b.Ke - 1) * (8 + (k.UnitId * 7) % 10) + (b.Ke - 1) * b.Ke / 2,
       100 + k.UnitId * 10 + b.Ke * (8 + (k.UnitId * 7) % 10) + b.Ke * (b.Ke + 1) / 2,
       2,
       DATEADD(DAY, 1, DATEADD(MONTH, b.Ke - 3, @BulanIni))
FROM Kontrak k
CROSS JOIN Bulan b
WHERE k.Status = 'Aktif'
  AND k.TanggalMulai <= EOMONTH(DATEADD(MONTH, b.Ke - 4, @BulanIni))
  AND NOT (b.Ke = 3 AND k.UnitId IN (20, 22, 23));

-- Generate tagihan memakai stored procedure yang sama dengan aplikasi.
DECLARE @P1 DATE = DATEADD(MONTH, -3, @BulanIni),
        @P2 DATE = DATEADD(MONTH, -2, @BulanIni),
        @P3 DATE = DATEADD(MONTH, -1, @BulanIni);
EXEC dbo.sp_GenerateTagihanBulanan @Periode = @P1;
EXEC dbo.sp_GenerateTagihanBulanan @Periode = @P2;
EXEC dbo.sp_GenerateTagihanBulanan @Periode = @P3;

-- Status pembayaran:
--  * 3 & 2 bulan lalu : lunas semua, KECUALI Agus (PenghuniId 7) dan Bayu (11, 2 bulan lalu) -> tunggakan
--  * bulan lalu       : penghuni 1-5 lunas, 6 & 8 menunggu verifikasi, sisanya belum bayar
UPDATE t SET Status = 'Lunas'
FROM Tagihan t JOIN Kontrak k ON k.KontrakId = t.KontrakId
WHERE (t.Periode IN (@P1, @P2) AND k.PenghuniId <> 7 AND NOT (k.PenghuniId = 11 AND t.Periode = @P2))
   OR (t.Periode = @P3 AND k.PenghuniId BETWEEN 1 AND 5);

INSERT INTO Pembayaran (TagihanId, TanggalBayar, JumlahBayar, MetodeBayar, NomorReferensi,
                        Status, DiverifikasiOleh, TanggalVerifikasi)
SELECT t.TagihanId,
       x.Tgl,
       t.TotalTagihan,
       CASE t.TagihanId % 3 WHEN 0 THEN 'Transfer' WHEN 1 THEN 'E-Wallet' ELSE 'Tunai' END,
       'REF' + CONVERT(CHAR(8), x.Tgl, 112) + RIGHT('000' + CAST(t.TagihanId AS VARCHAR(5)), 3),
       'Diterima',
       CASE t.TagihanId % 2 WHEN 0 THEN 2 ELSE 3 END,
       CAST(x.Tgl AS DATETIME2(0))
FROM Tagihan t
CROSS APPLY (SELECT CASE WHEN DATEADD(DAY, -3, t.TanggalJatuhTempo) > @Hari THEN @Hari
                         ELSE DATEADD(DAY, -3, t.TanggalJatuhTempo) END AS Tgl) x
WHERE t.Status = 'Lunas';

-- Menunggu verifikasi (Nadia & Maya membayar kemarin). Denda dikunci saat membayar.
UPDATE t SET Status = 'Menunggu Verifikasi',
             Denda  = dbo.fn_HitungDenda(t.TanggalJatuhTempo, DATEADD(DAY, -1, @Hari))
FROM Tagihan t JOIN Kontrak k ON k.KontrakId = t.KontrakId
WHERE t.Periode = @P3 AND k.PenghuniId IN (6, 8);

INSERT INTO Pembayaran (TagihanId, TanggalBayar, JumlahBayar, MetodeBayar, NomorReferensi, Status)
SELECT t.TagihanId, DATEADD(DAY, -1, @Hari), t.TotalTagihan + t.Denda, 'Transfer',
       'TRF' + CONVERT(CHAR(8), DATEADD(DAY, -1, @Hari), 112) + RIGHT('000' + CAST(t.TagihanId AS VARCHAR(5)), 3),
       'Menunggu'
FROM Tagihan t
WHERE t.Status = 'Menunggu Verifikasi';

EXEC dbo.sp_HitungDenda;

-- ---------- Komplain ----------
INSERT INTO Komplain (PenghuniId, UnitId, Kategori, Judul, Deskripsi, Prioritas, Status,
                      TeknisiId, TanggalAjukan, TanggalSelesai, CatatanPenanganan) VALUES
(1, 1, 'Air', N'Keran wastafel bocor', N'Keran wastafel dapur terus menetes walaupun sudah ditutup rapat.',
   'Sedang', 'Selesai', 2, DATEADD(DAY, -20, SYSDATETIME()), DATEADD(DAY, -18, SYSDATETIME()), N'Seal keran sudah diganti, tidak bocor lagi.'),
(8, 14, 'Kebersihan', N'Saluran sampah tersumbat', N'Saluran sampah di lantai 11 tersumbat dan berbau.',
   'Rendah', 'Selesai', 4, DATEADD(DAY, -10, SYSDATETIME()), DATEADD(DAY, -9, SYSDATETIME()), N'Saluran dibersihkan dan disemprot disinfektan.'),
(4, 6, 'AC', N'AC kamar tidak dingin', N'AC di kamar utama hanya mengeluarkan angin, tidak dingin sejak 2 hari lalu.',
   'Tinggi', 'Diproses', 3, DATEADD(DAY, -3, SYSDATETIME()), NULL, N'Dijadwalkan cuci AC dan cek freon.'),
(2, 3, 'Lainnya', N'Kartu akses lift tidak berfungsi', N'Kartu akses tidak terbaca di lift tower A sejak kemarin.',
   'Sedang', 'Diproses', 1, DATEADD(DAY, -2, SYSDATETIME()), NULL, NULL),
(7, 12, 'Listrik', N'Stop kontak dapur mati', N'Dua stop kontak di dapur tidak ada aliran listrik.',
   'Tinggi', 'Diajukan', NULL, DATEADD(DAY, -1, SYSDATETIME()), NULL, NULL),
(10, 19, 'Bangunan', N'Plafon kamar mandi rembes', N'Ada bercak air di plafon kamar mandi, kemungkinan dari unit atas.',
   'Sedang', 'Diajukan', NULL, SYSDATETIME(), NULL, NULL);

-- ---------- Booking fasilitas ----------
INSERT INTO BookingFasilitas (FasilitasId, PenghuniId, Tanggal, JamMulai, JamSelesai, JumlahOrang, Keperluan, Status) VALUES
(1, 1, DATEADD(DAY, 1, @Hari), '07:00', '09:00',  4, N'Berenang keluarga',    'Aktif'),
(1, 5, DATEADD(DAY, 1, @Hari), '16:00', '18:00',  2, N'Latihan renang',       'Aktif'),
(2, 3, DATEADD(DAY, 1, @Hari), '18:00', '20:00',  1, N'Fitness',              'Aktif'),
(2, 6, DATEADD(DAY, 2, @Hari), '06:00', '07:00',  2, NULL,                    'Aktif'),
(3, 8, DATEADD(DAY, 3, @Hari), '13:00', '17:00', 40, N'Arisan keluarga',      'Aktif'),
(3, 4, DATEADD(DAY, -5, @Hari), '10:00', '14:00', 25, N'Syukuran',            'Aktif'),
(1, 9, DATEADD(DAY, 2, @Hari), '08:00', '10:00',  3, N'Berenang',             'Dibatalkan');
GO

PRINT 'Database HunianKu berhasil dibuat beserta data dummy.';
GO
