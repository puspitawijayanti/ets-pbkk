using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Services;


public static class MasterDataService
{

    public static void SimpanTower(Tower t)
    {
        t.KodeTower = Input.Wajib(t.KodeTower, "Kode tower", 5).ToUpper();
        t.NamaTower = Input.Wajib(t.NamaTower, "Nama tower", 100);
        t.Alamat = Input.Opsional(t.Alamat, "Alamat", 200);

        if (t.JumlahLantai < 1 || t.JumlahLantai > 100)
            throw new ValidasiException("Jumlah lantai harus antara 1 dan 100.");
        if (TowerRepository.KodeSudahDipakai(t.KodeTower, t.TowerId))
            throw new ValidasiException($"Kode tower \"{t.KodeTower}\" sudah dipakai.");

        if (t.TowerId == 0)
        {
            TowerRepository.Insert(t);
        }
        else
        {
            int lantaiTerpakai = TowerRepository.LantaiTertinggiTerpakai(t.TowerId);
            if (t.JumlahLantai < lantaiTerpakai)
                throw new ValidasiException($"Sudah ada unit di lantai {lantaiTerpakai}, jumlah lantai tidak boleh lebih kecil.");
            TowerRepository.Update(t);
        }
    }

    public static void HapusTower(Tower t)
    {
        if (t.JumlahUnit > 0)
            throw new ValidasiException($"Tower {t.KodeTower} masih memiliki {t.JumlahUnit} unit. Hapus unitnya terlebih dahulu.");
        TowerRepository.Delete(t.TowerId);
    }


    public static void SimpanTipeUnit(TipeUnit t)
    {
        t.NamaTipe = Input.Wajib(t.NamaTipe, "Nama tipe", 50);
        if (t.LuasM2 <= 0) throw new ValidasiException("Luas unit harus lebih dari 0 m².");
        if (t.JumlahKamar < 0 || t.JumlahKamar > 10) throw new ValidasiException("Jumlah kamar harus antara 0 dan 10.");
        if (t.HargaSewaBulanan <= 0) throw new ValidasiException("Harga sewa harus lebih dari 0.");
        if (t.IplPerBulan < 0) throw new ValidasiException("IPL tidak boleh negatif.");
        if (TipeUnitRepository.NamaSudahDipakai(t.NamaTipe, t.TipeUnitId))
            throw new ValidasiException($"Tipe unit \"{t.NamaTipe}\" sudah ada.");

        if (t.TipeUnitId == 0) TipeUnitRepository.Insert(t);
        else TipeUnitRepository.Update(t);
    }

    public static void HapusTipeUnit(TipeUnit t) => TipeUnitRepository.Delete(t.TipeUnitId);


    public static void SimpanUnit(Unit u)
    {
        u.NomorUnit = Input.Wajib(u.NomorUnit, "Nomor unit", 10).ToUpper();
        if (u.TowerId == 0) throw new ValidasiException("Silakan pilih tower.");
        if (u.TipeUnitId == 0) throw new ValidasiException("Silakan pilih tipe unit.");

        var tower = TowerRepository.GetById(u.TowerId) ?? throw new ValidasiException("Tower tidak ditemukan.");
        if (u.Lantai < 0 || u.Lantai > tower.JumlahLantai)
            throw new ValidasiException($"Lantai harus antara 0 dan {tower.JumlahLantai} (jumlah lantai {tower.NamaTower}).");

        if (UnitRepository.NomorSudahDipakai(u.TowerId, u.NomorUnit, u.UnitId))
            throw new ValidasiException($"Nomor unit {u.NomorUnit} sudah ada di {tower.NamaTower}.");

        // Status "Terisi" hanya diatur otomatis lewat kontrak, bukan diubah manual.
        bool adaKontrakAktif = u.UnitId != 0 && UnitRepository.PunyaKontrakAktif(u.UnitId);
        if (adaKontrakAktif && u.Status != StatusUnit.Terisi)
            throw new ValidasiException("Unit ini masih punya kontrak aktif, statusnya harus tetap \"Terisi\". Akhiri kontraknya dulu.");
        if (!adaKontrakAktif && u.Status == StatusUnit.Terisi)
            throw new ValidasiException("Status \"Terisi\" diatur otomatis saat kontrak dibuat. Pilih Kosong atau Maintenance.");

        if (u.UnitId == 0) UnitRepository.Insert(u);
        else UnitRepository.Update(u);
    }

    public static void HapusUnit(Unit u)
    {
        if (UnitRepository.PunyaKontrakAktif(u.UnitId))
            throw new ValidasiException("Unit masih memiliki kontrak aktif dan tidak dapat dihapus.");
        UnitRepository.Delete(u.UnitId);
    }
}
