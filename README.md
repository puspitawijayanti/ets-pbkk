# HunianKu — Sistem Manajemen Hunian Apartemen

HunianKu merupakan aplikasi desktop untuk membantu pengelolaan operasional apartemen, mulai dari pendataan unit dan penghuni, kontrak sewa, tagihan bulanan, pembayaran, komplain, hingga pemesanan fasilitas. Aplikasi ini dikembangkan menggunakan C# WPF (.NET 10) dengan basis data SQL Server sebagai proyek ETS mata kuliah Pemrograman Berbasis Kerangka Kerja (PBKK).

## Anggota Kelompok

| Nama | NRP | Tanggung Jawab |
|---|---|---|
| Puspita Wijayanti Kusuma | 5025241059 | Database, data master, komplain, booking, dashboard, dan laporan |
| Nashwa Aulia Putri Diansyah | 5025241064 | Autentikasi, data penghuni, kontrak sewa, dan hak akses |
| Kineisha Rana Salsabila | 5025241149 | Setup project, tema, meteran, tagihan, pembayaran, dan publish |

## Fitur Utama

- **Manajemen Data:** Pengelolaan tower, unit, tipe unit, tarif, penghuni, dan kontrak sewa.
- **Tagihan dan Pembayaran:** Pencatatan meteran listrik dan air, pembuatan tagihan, denda keterlambatan, serta verifikasi pembayaran.
- **Komplain dan Booking:** Pengajuan komplain, pengelolaan perbaikan, dan pemesanan fasilitas apartemen.
- **Dashboard dan Laporan:** Menampilkan informasi operasional apartemen.

## Struktur Project

```text
ETS/
├── HunianKu.sln
├── database/HunianKu.sql
├── src/HunianKu/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   ├── Helpers/
│   ├── Views/
│   ├── Styles/
│   └── appsettings.json
├── publish/HunianKu/
├── release/HunianKu-v1.0.zip
└── docs/
    ├── PRD.md
    ├── diagram/
    └── screenshots/
```

## Persyaratan

- Windows 10/11 (64-bit)
- SQL Server (Express, Developer, atau LocalDB)
- Visual Studio 2026 dan .NET SDK 10 untuk pengembangan

## Cara Menjalankan

1. Buka `database/HunianKu.sql` melalui SQL Server Management Studio (SSMS), lalu jalankan skrip.
2. Sesuaikan koneksi database pada `appsettings.json`.
3. Buka `HunianKu.sln` di Visual Studio dan tekan **F5**.

Alternatif menggunakan terminal:

```bash
dotnet run --project src/HunianKu
```

**Catatan:** Skrip SQL menghapus dan membuat ulang database `HunianKu`, termasuk data demo. Pastikan tidak ada data penting sebelum menjalankannya.

## Akun Demo

| Role | Username | Password |
|---|---|---|
| Admin | `admin` | `admin123` |
| Staf | `rina`, `dodi` | `staf123` |
| Penghuni | `budi`, `agus`, `siti`, `sarah` | `penghuni123` |

## Skenario Demo

1. Melihat dashboard dan laporan sebagai admin.
2. Membuat kontrak sewa penghuni baru.
3. Mencatat meteran dan membuat tagihan.
4. Mengajukan serta memverifikasi pembayaran.
5. Mencoba booking fasilitas dengan jadwal bentrok dan tersedia.
6. Mengajukan komplain dan memperbarui status penanganannya.

## Build Aplikasi

Untuk membuat executable:

```bash
dotnet publish src/HunianKu/HunianKu.csproj -p:PublishProfile=SingleFileExe
```

Hasil build tersedia di `publish/HunianKu/`.

## Dokumentasi

### Link Repository Project
https://github.com/puspitawijayanti/ets-pbkk

### Link Download Aplikasi
https://github.com/puspitawijayanti/ets-pbkk/blob/main/release/HunianKu-v1.0.zip

### Link Video Demo Project
https://youtu.be/gIVeoPRkozk

### Link PRD
https://github.com/puspitawijayanti/ets-pbkk/blob/main/docs/PRD-5025241059_5025241064_5025241149.pdf

