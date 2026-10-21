using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class PenghuniPage : UserControl
{
    private Penghuni? _terpilih;

    public PenghuniPage()
    {
        InitializeComponent();
        KosongkanForm();
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            DgPenghuni.ItemsSource = PenghuniRepository.GetAll(TxtCari.Text);
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Cari_Click(object sender, RoutedEventArgs e) => MuatData();

    private void TxtCari_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) MuatData();
    }

    private void DgPenghuni_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgPenghuni.SelectedItem is not Penghuni p) return;
        _terpilih = p;
        TxtNama.Text = p.NamaLengkap;
        TxtUsername.Text = p.Username;
        TxtPassword.Clear();
        TxtNik.Text = p.NIK;
        CmbJK.SelectedValue = p.JenisKelamin;
        TxtNoHp.Text = p.NoHp;
        TxtEmail.Text = p.Email;
        TxtPekerjaan.Text = p.Pekerjaan;
        TxtDarurat.Text = p.KontakDarurat;
        ChkAktif.IsChecked = p.IsAktif;

        TxtJudulForm.Text = $"Ubah Data {p.NamaLengkap}";
        LblPassword.Text = "Password Baru (isi hanya untuk Reset Password)";
        BtnHapus.IsEnabled = true;
        BtnReset.IsEnabled = true;
    }

    private void Baru_Click(object sender, RoutedEventArgs e) => KosongkanForm();

    private void KosongkanForm()
    {
        _terpilih = null;
        DgPenghuni.SelectedItem = null;
        foreach (var tb in new[] { TxtNama, TxtUsername, TxtNik, TxtNoHp, TxtEmail, TxtPekerjaan, TxtDarurat })
            tb.Clear();
        TxtPassword.Clear();
        CmbJK.SelectedIndex = 0;
        ChkAktif.IsChecked = true;
        TxtJudulForm.Text = "Tambah Penghuni";
        LblPassword.Text = "Password Awal (min. 6 karakter)";
        BtnHapus.IsEnabled = false;
        BtnReset.IsEnabled = false;
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var p = new Penghuni
            {
                PenghuniId = _terpilih?.PenghuniId ?? 0,
                UserId = _terpilih?.UserId ?? 0,
                UnitAktif = _terpilih?.UnitAktif,
                NamaLengkap = TxtNama.Text,
                Username = TxtUsername.Text,
                NIK = TxtNik.Text,
                JenisKelamin = CmbJK.SelectedValue as string ?? "",
                NoHp = TxtNoHp.Text,
                Email = TxtEmail.Text,
                Pekerjaan = TxtPekerjaan.Text,
                KontakDarurat = TxtDarurat.Text,
                IsAktif = ChkAktif.IsChecked == true
            };
            PenghuniService.Simpan(p, TxtPassword.Password);
            Pesan.Info(_terpilih == null
                ? $"Penghuni berhasil ditambahkan.\nAkun login: {p.Username}"
                : "Data penghuni berhasil diperbarui.");
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void ResetPassword_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        try
        {
            if (!Pesan.Konfirmasi($"Reset password akun {_terpilih.Username} dengan password baru yang diisi?")) return;
            PenghuniService.ResetPassword(_terpilih, TxtPassword.Password);
            TxtPassword.Clear();
            Pesan.Info("Password berhasil direset. Beritahukan password baru kepada penghuni.");
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Hapus_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        if (!Pesan.Konfirmasi($"Hapus penghuni {_terpilih.NamaLengkap} beserta akun loginnya?\n\n" +
                              "Penghuni yang sudah punya riwayat kontrak tidak dapat dihapus (nonaktifkan saja akunnya).")) return;
        try
        {
            PenghuniService.Hapus(_terpilih);
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}