using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;
public partial class BookingFasilitasPage : UserControl
{
    private readonly int _penghuniId = Session.PenghuniId!.Value;
    private BookingFasilitas? _bookingTerpilih;

    public BookingFasilitasPage()
    {
        InitializeComponent();

        var jam = Enumerable.Range(10, 35).Select(i => TimeSpan.FromMinutes(i * 30)).ToList();
        var teksJam = jam.Select(j => j.ToString(@"hh\:mm")).ToList();
        CmbJamMulai.ItemsSource = teksJam;
        CmbJamSelesai.ItemsSource = teksJam;
        CmbJamMulai.SelectedItem = "08:00";
        CmbJamSelesai.SelectedItem = "09:00";

        DpTanggal.DisplayDateStart = DateTime.Today;   
        DpTanggal.SelectedDate = DateTime.Today.AddDays(1);
        try
        {
            CmbFasilitas.ItemsSource = BookingRepository.GetFasilitasAktif();
            CmbFasilitas.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        MuatBookingSaya();
    }
    private void Jadwal_Changed(object? sender, SelectionChangedEventArgs e) => MuatJadwal();

    private void MuatJadwal()
    {
        if (CmbFasilitas.SelectedItem is not Fasilitas f || DpTanggal.SelectedDate is not DateTime tanggal) return;
        TxtInfoFasilitas.Text = f.Keterangan;
        try
        {
            var jadwal = BookingRepository.GetJadwal(f.FasilitasId, tanggal);
            DgJadwal.ItemsSource = jadwal;
            TxtJudulJadwal.Text = $"Jadwal Terisi · {f.NamaFasilitas} · {tanggal:dddd, dd MMMM yyyy}";
            TxtJadwalKosong.Visibility = jadwal.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void MuatBookingSaya()
    {
        try
        {
            DgBookingSaya.ItemsSource = BookingRepository.GetByPenghuni(_penghuniId);
            _bookingTerpilih = null;
            BtnBatal.IsEnabled = false;
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Booking_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var fasilitas = Input.Pilih<Fasilitas>(CmbFasilitas.SelectedItem, "fasilitas");
            if (CmbJamMulai.SelectedItem is not string mulai || CmbJamSelesai.SelectedItem is not string selesai)
                throw new ValidasiException("Pilih jam mulai dan jam selesai.");

            var booking = new BookingFasilitas
            {
                FasilitasId = fasilitas.FasilitasId,
                PenghuniId = _penghuniId,
                Tanggal = Input.Tanggal(DpTanggal.SelectedDate, "Tanggal"),
                JamMulai = TimeSpan.Parse(mulai),
                JamSelesai = TimeSpan.Parse(selesai),
                JumlahOrang = Input.Bulat(TxtJumlahOrang.Text, "Jumlah orang", 1, 999),
                Keperluan = TxtKeperluan.Text
            };
            LayananService.BuatBooking(booking);
            Pesan.Info($"Booking {fasilitas.NamaFasilitas} pada {booking.Tanggal:dddd, dd MMM yyyy} pukul {booking.JamTeks} berhasil.");
            TxtKeperluan.Clear();
            MuatJadwal();
            MuatBookingSaya();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);   
        }
    }

    private void DgBookingSaya_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _bookingTerpilih = DgBookingSaya.SelectedItem as BookingFasilitas;
        BtnBatal.IsEnabled = _bookingTerpilih?.BisaDibatalkan == true;
    }

    private void Batal_Click(object sender, RoutedEventArgs e)
    {
        if (_bookingTerpilih == null) return;
        if (!Pesan.Konfirmasi($"Batalkan booking {_bookingTerpilih.NamaFasilitas} pada {_bookingTerpilih.Tanggal:dd MMM yyyy} {_bookingTerpilih.JamTeks}?")) return;
        try
        {
            LayananService.BatalkanBooking(_bookingTerpilih);
            MuatJadwal();
            MuatBookingSaya();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
