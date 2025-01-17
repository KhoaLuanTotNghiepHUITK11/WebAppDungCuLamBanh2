﻿using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Data;
using System.Diagnostics;
using WebDungCuLamBanh.Data;
using WebDungCuLamBanh.Helpers;
using WebDungCuLamBanh.Models;
using WebDungCuLamBanh.Models.admin;
namespace WebDungCuLamBanh.AdminControllers
{
    public class AdministratorController : Controller
    {
        private readonly AppDbContext _context;

        public AdministratorController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(AdminModel adminModel)
        {
            try
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.TenNguoiDung == adminModel.TenNguoiDung && a.MatKhau == adminModel.MatKhau);
                if (admin != null && admin.Quyen == 1)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    HttpContext.Session.Set("admin", System.Text.Encoding.UTF8.GetBytes(admin.TenNguoiDung.ToString()));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.Error = "Đăng nhập không thành công!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }
        public IActionResult SignOut()
        {
            //clear admin session
            HttpContext.Session.Remove("admin");

            return RedirectToAction("Index");
        }


        public IActionResult Dashboard(DateTime fromDate, DateTime toDate)
        {
            @ViewBag.fromDate = fromDate;
            @ViewBag.toDate = toDate;
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            //Doanh thu tháng này
            var doanhThuThangNay = _context.DonHangs
                .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat.Value.Month == DateTime.Now.Month && d.NgayDat.Value.Year == DateTime.Now.Year)
                .Sum(d => d.TongTien);
            var tienVatThangNay = _context.DonHangs
                .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat.Value.Month == DateTime.Now.Month && d.NgayDat.Value.Year == DateTime.Now.Year)
                .Sum(d => d.VAT);

            var tienShipThangNay = _context.DonHangVanChuyens
                .Where(d => d.TrangThaiVanChuyen.Id_TrangThai != 1 && d.DonHang.NgayDat.Value.Month == DateTime.Now.Month && d.DonHang.NgayDat.Value.Year == DateTime.Now.Year)
                .Sum(d => d.PhiVanChuyen);
            var doanhThuSauThuevaChiPhiThangNay = Math.Round((decimal)(doanhThuThangNay * 92 / 100), 0) - tienShipThangNay - tienVatThangNay;
            var tienNhapHangThangNay = _context.HoaDonNhapHangs
                .Where(d => d.NgayNhapHang.Value.Month == DateTime.Now.Month && d.NgayNhapHang.Value.Year == DateTime.Now.Year)
                .Sum(d => d.TongTien);
            var LoiNhuanThangNay = doanhThuSauThuevaChiPhiThangNay - tienNhapHangThangNay;
            var vatThangNay = _context.DonHangs
                .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat.Value.Month == DateTime.Now.Month && d.NgayDat.Value.Year == DateTime.Now.Year)
                .Sum(d => d.VAT);
            ViewBag.tienVatThangNay = HtmlHelpers.FormatCurrency((decimal)tienVatThangNay);
            ViewBag.tienShipThangNay = HtmlHelpers.FormatCurrency((decimal)tienShipThangNay);
            ViewBag.doanhThuSauThuevaChiPhiThangNay = HtmlHelpers.FormatCurrency((decimal)doanhThuSauThuevaChiPhiThangNay);
            ViewBag.tienNhapHangThangNay = HtmlHelpers.FormatCurrency((decimal)tienNhapHangThangNay);
            ViewBag.loiNhuanThangNay = HtmlHelpers.FormatCurrency((decimal)LoiNhuanThangNay);
            ViewBag.vatThangNay = HtmlHelpers.FormatCurrency((decimal)vatThangNay);
            ViewBag.thangNay = DateTime.Now.Month;


            ViewBag.doanhThuThangNay = HtmlHelpers.FormatCurrency((decimal)doanhThuThangNay);
            //Đơn hàng chờ xử lý
            var donHangChoXuLy = _context.DonHangs
                .Where(d => d.TrangThai == "Chưa thanh toán")
                .Count();
            ViewBag.donHangChoXuLy = donHangChoXuLy;




            // Lọc doanh thu từ ngày đến ngày
            var doanhThuLoc = _context.DonHangs
            .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat >= fromDate && d.NgayDat <= toDate)
                .Sum(d => d.TongTien);
            var tienShipLoc = _context.DonHangVanChuyens
            .Where(d => d.TrangThaiVanChuyen.Id_TrangThai != 1 && d.DonHang.NgayDat >= fromDate && d.DonHang.NgayDat <= toDate)
                .Sum(d => d.PhiVanChuyen);
            var doanhThuSauThuevaChiPhiLoc = Math.Round((decimal)(doanhThuLoc * 92 / 100), 0) - tienShipLoc;
            var tienNhapHangLoc = _context.HoaDonNhapHangs
                .Where(d => d.NgayNhapHang >= fromDate && d.NgayNhapHang <= toDate)
                    .Sum(d => d.TongTien);
            var tienLoiLoc = doanhThuSauThuevaChiPhiLoc - tienNhapHangLoc;
            var vatLoc = _context.DonHangs
                .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat >= fromDate && d.NgayDat <= toDate)
                    .Sum(d => d.VAT);

            ViewBag.doanhThuLoc = doanhThuLoc;
            ViewBag.tienShipLoc = tienShipLoc;
            ViewBag.doanhThuSauThuevaChiPhiLoc = doanhThuSauThuevaChiPhiLoc;
            ViewBag.tienNhapHangLoc = tienNhapHangLoc;
            ViewBag.tienLoiLoc = tienLoiLoc;
            ViewBag.vatLoc = vatLoc;
            ViewBag.fromDate = fromDate;
            ViewBag.toDate = toDate;
            //Doanh thu hôm nay
            var doanhThuHomNay = _context.DonHangs
                .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat.Value.Date == DateTime.Now.Date)
                .Sum(d => d.TongTien);
            var tienShipHomNay = _context.DonHangVanChuyens
                    .Where(d => d.TrangThaiVanChuyen.Id_TrangThai != 1 && d.DonHang.NgayDat.Value.Date == DateTime.Now.Date)
                .Sum(d => d.PhiVanChuyen);
#pragma warning disable CS8629 // Nullable value type may be null.
            var doanhThuSauThuevaChiPhiHomNay = Math.Round((decimal)(doanhThuHomNay * 92 / 100), 0) - tienShipHomNay;
#pragma warning restore CS8629 // Nullable value type may be null.
            var tienNhapHangHomNay = _context.HoaDonNhapHangs
                .Where(d => d.NgayNhapHang.Value.Date == DateTime.Now.Date)
                .Sum(d => d.TongTien);
            var tienLoiHomNay = doanhThuSauThuevaChiPhiHomNay - tienNhapHangHomNay;
            var vatHomNay = _context.DonHangs
                .Where(d => d.TrangThai != "Chưa thanh toán" && d.NgayDat.Value.Date == DateTime.Now.Date)
                .Sum(d => d.VAT);

            ViewBag.doanhThuHomNay = HtmlHelpers.FormatCurrency((decimal)doanhThuHomNay);
            ViewBag.tienShipHomNay = HtmlHelpers.FormatCurrency((decimal)tienShipHomNay);
            ViewBag.doanhThuSauThuevaChiPhiHomNay = HtmlHelpers.FormatCurrency((decimal)doanhThuSauThuevaChiPhiHomNay);
            ViewBag.tienNhapHangHomNay = HtmlHelpers.FormatCurrency((decimal)tienNhapHangHomNay);
#pragma warning disable CS8629 // Nullable value type may be null.
            ViewBag.tienLoiHomNay = HtmlHelpers.FormatCurrency((decimal)tienLoiHomNay);
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            ViewBag.vatHomNay = HtmlHelpers.FormatCurrency((decimal)vatHomNay);
#pragma warning restore CS8629 // Nullable value type may be null.
            //Lấy sản phẩm trong Chi tiết đơn hàng
            var top10 = _context.ChiTietDonHangs
                .Include(ct => ct.DungCu)

                .Where(ct => ct.DonHang.TrangThai != "Chưa thanh toán")

                .ToList();
            var top10RepeatedProducts = top10
                .GroupBy(ct => ct.DungCu.Id_DungCu)  // Nhóm các sản phẩm theo ID
                .Select(group => new
                {
                    ProductId = group.Key,
                    Count = group.Count()
                })  // Chọn số lần xuất hiện của mỗi sản phẩm
                .OrderByDescending(item => item.Count)  // Sắp xếp theo số lần xuất hiện giảm dần
                .Take(10)  // Chọn 10 sản phẩm đầu tiên
                .ToList();

            ViewData["Top10"] = top10RepeatedProducts;
            return View();
        }
        public async Task<IActionResult> Product(string search = "")
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var appDbContext = _context.DungCus.Include(d => d.LoaiDungCu).Where(d => d.DaXoa == 0);

            //Lọc theo từ khoá
            if (!string.IsNullOrEmpty(search))
            {
                appDbContext = appDbContext.Where(d => d.TenDungCu.Contains(search));
            }
            return View(await appDbContext.ToListAsync());
        }
        public async Task<string?> UploadImage(IFormFile fileInput, string filename)
        {
            try
            {
                using (var inputStream = fileInput.OpenReadStream())
                using (var image = await Image.LoadAsync(inputStream))
                {
                    var encoder = new JpegEncoder
                    {
                        Quality = 30 // Đặt chất lượng nén, từ 0 đến 100
                    };
                    using (var outputStream = new MemoryStream())
                    {
                        await image.SaveAsync(outputStream, encoder);
                        outputStream.Seek(0, SeekOrigin.Begin);
                        long originalSize = fileInput.Length;
                        long compressedSize = outputStream.Length;
                        Console.WriteLine($"Original Size: {originalSize} bytes");
                        Console.WriteLine($"Compressed Size: {compressedSize} bytes");
                        // Tải ảnh đã nén lên kho lưu trữ
                        var storage = new FirebaseStorage("qldclb-770f5.appspot.com");
                        var imageUrl = await storage.Child("images")
                                                    .Child(fileInput.FileName)
                                                    .PutAsync(outputStream);
                        return imageUrl;
                    }

                }

                //var imageUrl = await storage.Child("images")
                //                            .Child(filename)
                //                            .PutAsync(fileInput.OpenReadStream());
                //return imageUrl;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public IActionResult CreateProduct()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            ViewData["Id_LoaiDungCu"] = new SelectList(_context.LoaiDungCus, "Id_LoaiDungCu", "TenLoaiDungCu");

            ViewData["Id_NhaCungCap"] = _context.NhaCungCaps
                .Select(ncc => new SelectListItem
                {
                    Value = ncc.Id_NhaCungCap.ToString(),
                    Text = $"{ncc.TenNhaCungCap}" + " - " + $"{ncc.DiaChi}"
                })
                .ToList();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(DungCuModel dungCuModel, IFormFile? imageInput = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra số lượng có hợp lệ không
                    if (dungCuModel.SoLuong < 0)
                    {
                        ModelState.AddModelError(string.Empty, "Số lượng phải là một số nguyên dương.");
                        return View(dungCuModel);
                    }

                    // Kiểm tra xem tên sản phẩm có bị trùng không
                    bool isDuplicateName = await _context.DungCus
                        .AnyAsync(dc => dc.TenDungCu == dungCuModel.TenDungCu);

                    if (isDuplicateName)
                    {
                        ModelState.AddModelError(string.Empty, "Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.");
                        return View(dungCuModel);
                    }

                    // Kiểm tra sản phẩm có tồn tại chưa
                    var sp = await _context.DungCus.FindAsync(dungCuModel.Id_DungCu);

                    // Khởi tạo các giá trị ban đầu cho sản phẩm
                    dungCuModel.DaXoa = 0;
                    dungCuModel.SoLuong = 0;
                    dungCuModel.GiaKhuyenMai = 0;

                    _context.Add(dungCuModel);
                    await _context.SaveChangesAsync();

                    // Lấy ID của sản phẩm mới được thêm vào
                    int id = _context.DungCus.Max(d => d.Id_DungCu);

                    // Nếu có ảnh, tải lên và cập nhật URL ảnh
                    if (imageInput != null)
                    {
                        string? newUrl = await UploadImage(imageInput, id.ToString());
                        dungCuModel.HinhAnh = newUrl;

                        _context.Update(dungCuModel);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Product));
                }
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = ex.Message });
            }

            return View(dungCuModel);
        }

        public async Task<string?> UpdateImage(string url, IFormFile fileInput)
        {

            Uri uri = new Uri(url);
            try
            {

                //Kiem tra hinh co chua

                var storage = new FirebaseStorage("qldclb-770f5.appspot.com");
                var existingImage = await storage.Child("images")
                                            .Child(Path.GetFileName(uri.LocalPath))
                                            .GetDownloadUrlAsync();
                if (existingImage != null)
                {
                    await storage.Child("images")
                                         .Child(Path.GetFileName(uri.LocalPath))
                                         .DeleteAsync();
                    var imageUrl = await storage.Child("images")
                                         .Child(fileInput.FileName)
                                         .PutAsync(fileInput.OpenReadStream());
                    return imageUrl;
                }
                else if (existingImage == null)
                {
                    using (var inputStream = fileInput.OpenReadStream())
                    using (var image = await Image.LoadAsync(inputStream))
                    {
                        var encoder = new JpegEncoder
                        {
                            Quality = 50 // Đặt chất lượng nén, từ 0 đến 100
                        };
                        using (var outputStream = new MemoryStream())
                        {
                            await image.SaveAsync(outputStream, encoder);
                            outputStream.Seek(0, SeekOrigin.Begin);
                            long originalSize = fileInput.Length;
                            long compressedSize = outputStream.Length;
                            Console.WriteLine($"Original Size: {originalSize} bytes");
                            Console.WriteLine($"Compressed Size: {compressedSize} bytes");
                            // Tải ảnh đã nén lên kho lưu trữ
                            var imageUrl = await storage.Child("images")
                                                        .Child(fileInput.FileName)
                                                        .PutAsync(outputStream);
                            return imageUrl;
                        }

                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return NotFound();
            }

            var dungCuModel = await _context.DungCus
                .Include(p => p.LoaiDungCu)
                .Include(p => p.NhaCungCap)
                .FirstOrDefaultAsync(m => m.Id_DungCu == id);

            if (dungCuModel == null)
            {
                return NotFound();
            }
            ViewData["LoaiDungCu"] = new SelectList(_context.LoaiDungCus, "Id_LoaiDungCu", "TenLoaiDungCu", dungCuModel.Id_LoaiDungCu);
            ViewData["NhaCungCap"] = _context.NhaCungCaps
                .Select(ncc => new SelectListItem
                {
                    Value = ncc.Id_NhaCungCap.ToString(),
                    Text = $"{ncc.TenNhaCungCap}" + " - " + $"{ncc.DiaChi}"
                })
                .ToList();
            return View(dungCuModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, DungCuModel dungCuModel, IFormFile? imageInput = null)
        {
            if (id != dungCuModel.Id_DungCu)
            {
                return NotFound();
            }
            //if (dungCuModel.SoLuong < 0||dungCuModel.Gia<=0||dungCuModel.GiaNhap<=0||dungCuModel.GiaKhuyenMai<=0)
            //{
            //    ModelState.AddModelError(string.Empty, "Không thể là số âm.");
            //}
            if (ModelState.IsValid)
            {
                try
                {


                    if (imageInput != null)
                    {
                        dungCuModel.DaXoa = 0;
                        string? newUrl = await UploadImage(imageInput, dungCuModel.Id_DungCu.ToString());
                        dungCuModel.HinhAnh = newUrl;
                        _context.Update(dungCuModel);
                        await _context.SaveChangesAsync();
                    }
                    else if (imageInput == null)
                    {
                        dungCuModel.HinhAnh = await _context.DungCus.Where(d => d.Id_DungCu == dungCuModel.Id_DungCu).Select(d => d.HinhAnh).FirstOrDefaultAsync();
                        _context.Update(dungCuModel);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = ex.Message });
                }
                return RedirectToAction(nameof(Product));
            }
            ViewData["Id_LoaiDungCu"] = new SelectList(_context.LoaiDungCus, "Id_LoaiDungCu", "Id_LoaiDungCu", dungCuModel.Id_LoaiDungCu);
            return View(dungCuModel);
        }
        public async Task<string?> DeleteImage(string filename)
        {
            //Kiem tra hinh co chua

            var storage = new FirebaseStorage("qldclb-770f5.appspot.com");
            var existingImage = await storage.Child("images")
                                        .Child(Path.GetFileName(filename))
                                        .GetDownloadUrlAsync();
            if (existingImage != null)
            {
                await storage.Child("images")
                                     .Child(Path.GetFileName(filename))
                                     .DeleteAsync();
            }
            return null;
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dungCuModel = await _context.DungCus
                .Include(d => d.LoaiDungCu)
                .FirstOrDefaultAsync(m => m.Id_DungCu == id);
            if (dungCuModel == null)
            {
                return NotFound();
            }

            return View(dungCuModel);
        }

        // POST: DungCuModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            try
            {
                var dungCuModel = await _context.DungCus.FindAsync(id);
                if (dungCuModel != null)
                {

                    dungCuModel.DaXoa = 1;
                    dungCuModel.SoLuong = 0;
                    _context.Update(dungCuModel);

                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Product));
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = ex.Message });
            }

        }
        private bool DungCuModelExists(int id)
        {
            return _context.DungCus.Any(e => e.Id_DungCu == id);
        }
        public IActionResult Category()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var viewModel = new CategoryViewModel()
            {
                Category = _context.LoaiDungCus.ToList(),
                NewCategory = new LoaiDungCuModel()
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(string TenLoaiDungCu)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var category = await _context.LoaiDungCus.FirstOrDefaultAsync(c => c.TenLoaiDungCu == TenLoaiDungCu);
                    if (category != null)
                    {
                        ModelState.AddModelError(string.Empty, "Loại dụng cụ này đã tồn tại.");
                    }
                    else
                    {
                        var loaiDungCuModel = new LoaiDungCuModel
                        {
                            TenLoaiDungCu = TenLoaiDungCu,
                        };
                        _context.Add(loaiDungCuModel);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Category));
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = ex.Message });
            }

            // Nếu có lỗi, trả về view cùng với model
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = "Đã có lỗi xảy ra." });
        }

        public async Task<IActionResult> SaleOff()
        {

            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            await ApplySaleOff();
            var appDbContext = _context.KhuyenMai2s;
            return View(await appDbContext.ToListAsync());
        }
        public async Task<IActionResult> CreateSaleOff()
        {
            ViewData["Id_SanPham"] = new SelectList(_context.DungCus, "Id_DungCu", "TenDungCu");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSaleOff(KhuyenMaiModel khuyenMai2)
        {

            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            if (khuyenMai2.NgayBatDau > khuyenMai2.NgayKetThuc)
            {
                ModelState.AddModelError(string.Empty, "Ngày không hợp lệ.");
            }
            // Thêm khuyenMai2 vào cơ sở dữ liệu
            try
            {
                _context.Add(khuyenMai2);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(AddProductToSaleOff), new { id = khuyenMai2.Id_KhuyenMai });
            }
            catch (Exception e)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = e.Message });
            }
        }
        //Xoá
        public async Task<IActionResult> DeleteSaleOff(int? id)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            //Xoá chi tiết khuyến mãi
            var ctkm = await _context.ChiTietKhuyenMais.Where(ct => ct.Id_KhuyenMai == id).ToListAsync();
            foreach (var ct in ctkm)
            {
                _context.ChiTietKhuyenMais.Remove(ct);
                await _context.SaveChangesAsync();
            }
            var khuyenMai2 = await _context.KhuyenMai2s.FindAsync(id);
            try
            {
                if (khuyenMai2 != null)
                {
                    _context.KhuyenMai2s.Remove(khuyenMai2);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(SaleOff));
            }
            catch (Exception e)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = e.Message });
            }

        }
        public async Task<IActionResult> AddProductToSaleOff(int id)
        {
            ViewData["Id_KhuyenMai"] = id;
            ViewData["KhuyenMai"] = await _context.KhuyenMai2s.FirstOrDefaultAsync(k => k.Id_KhuyenMai == id);
            ViewData["SanPham"] = new SelectList(_context.DungCus.Where(p => p.DaXoa == 0), "Id_DungCu", "TenDungCu");
            ViewData["ChiTietKhuyenMai"] = await _context.ChiTietKhuyenMais.Include(ct => ct.SanPham).Include(ct => ct.KhuyenMai).Where(ct => ct.Id_KhuyenMai == id).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToSaleOff(ChiTietKhuyenMaiModel chiTietKhuyenMai)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            if (chiTietKhuyenMai.GiaTriGiam > 99 || chiTietKhuyenMai.GiaTriGiam < 1)
            {
                return Json(new { success = false, message = "Giá trị giảm phải từ 1 đến 99." });
            }
            var existingChiTietKhuyenMai = await _context.ChiTietKhuyenMais.FirstOrDefaultAsync(k => k.Id_SanPham == chiTietKhuyenMai.Id_SanPham);
            if (existingChiTietKhuyenMai != null)
            {

                return Json(new { success = false, message = "Sản phẩm đã tồn tại." });
            }

            // Thêm khuyenMai2 vào cơ sở dữ liệu
            _context.Add(chiTietKhuyenMai);
            await _context.SaveChangesAsync();
            //Quay ve trang danh sach khuyen mai
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductSaleOff(int Id_KhuyenMai, int Id_CTKM)
        {

            // Lấy đơn hàng chưa thanh toán của khách hàng
            var hoadon = await _context.KhuyenMai2s
                .FirstOrDefaultAsync(hd => hd.Id_KhuyenMai == Id_KhuyenMai);

            // Lấy chi tiết đơn hàng cần xóa
            var ct = await _context.ChiTietKhuyenMais
                .FirstOrDefaultAsync(ct => ct.Id_CTKM == Id_CTKM);
            try
            {
                if (ct != null)
                {
                    // Xóa chi tiết đơn hàng
                    _context.ChiTietKhuyenMais.Remove(ct);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("AddProductToSaleOff", "Administrator", new { id = Id_KhuyenMai });
            }
            catch (Exception e)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = e.Message });
            }

        }
        [HttpPost]
        public async Task<ActionResult> ApplySaleOff()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            try
            {
                //Xoá giá khuyến mại cũ
                var kmcu = await _context.DungCus.ToListAsync();
                foreach (var dc in kmcu)
                {
                    dc.GiaKhuyenMai = 0;
                    _context.Update(dc);
                    await _context.SaveChangesAsync();
                }

                //Duyệt danh sách KhuyenMai
                var khuyenMai = await _context.KhuyenMai2s.ToListAsync();
                foreach (var km in khuyenMai)
                {
                    //Lấy danh sách ctkm
                    var ctkm = await _context.ChiTietKhuyenMais.Where(ct => ct.Id_KhuyenMai == km.Id_KhuyenMai).ToListAsync();
                    foreach (var ct in ctkm)
                    {
                        //Lấy sản phẩm
                        var dungCu = await _context.DungCus.FirstOrDefaultAsync(d => d.Id_DungCu == ct.Id_SanPham);
                        if (dungCu != null)
                        {
                            if (km.NgayBatDau <= DateTime.Now && km.NgayKetThuc >= DateTime.Now)
                            {
                                dungCu.GiaKhuyenMai = ((dungCu.Gia * (100 - ct.GiaTriGiam)) / 100);
                                _context.Update(dungCu);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                dungCu.GiaKhuyenMai = 0;
                                _context.Update(dungCu);
                                await _context.SaveChangesAsync();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            //Lấy danh sách ctkm

            return Json(new { success = true });
        }
        public async Task<IActionResult> Voucher()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var appDbContext = _context.MaGiamGias;
            return View(await appDbContext.ToListAsync());
        }
        public IActionResult CreateVoucher()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher(MaGiamGiaModel maGiamGia)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var existingMaGiamGia = await _context.MaGiamGias.FirstOrDefaultAsync(k => k.Id_MaGiamGia == maGiamGia.Id_MaGiamGia);
            if (existingMaGiamGia != null)
            {
                ModelState.AddModelError(string.Empty, "Mã giảm giá này đã tồn tại.");
                return View(maGiamGia);
            }
            try
            {
                _context.Add(maGiamGia);
                await _context.SaveChangesAsync();
                //Quay ve trang danh sach khuyen mai
                return RedirectToAction(nameof(Voucher));
            }
            catch (Exception e)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = e.Message });
            }
            // Thêm khuyenMai2 vào cơ sở dữ liệu

        }
        public async Task<IActionResult> DeleteVoucher(string? id)
        {
            var maGiamGia = await _context.MaGiamGias.FindAsync(id);
            if (maGiamGia != null)
            {
                _context.MaGiamGias.Remove(maGiamGia);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Voucher));
        }
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            var loaiDungCu = await _context.LoaiDungCus.FindAsync(id);
            if (loaiDungCu != null)
            {
                //Kiểm ra xem loại dụng cụ này đã có sản phẩm chưa
                var dungCu = await _context.DungCus.FirstOrDefaultAsync(d => d.Id_LoaiDungCu == id);
                if (dungCu != null)
                {
                    ModelState.AddModelError(string.Empty, "Loại dụng cụ này đã có sản phẩm.");
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = "Loại dụng cụ này đã có sản phẩm. Hãy xoá tất cả sản phẩm trước khi xoá loại dụng cụ." });
                }

                _context.LoaiDungCus.Remove(loaiDungCu);
                await _context.SaveChangesAsync();
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Category));
            }
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = "Đã có lỗi xảy ra." });
        }
        public async Task<IActionResult> AllOrders()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var donHangChuaGiao = _context.DonHangVanChuyens
                .Where(d => d.TinhTrang == 0)
                .Count();
            ViewBag.donHangChuaGiao = donHangChuaGiao;
            var donHangDaGiao = _context.DonHangVanChuyens
                .Where(d => d.TinhTrang == 2)
                .Count();
            ViewBag.donHangDaGiao = donHangDaGiao;
            var donHangDangGiao = _context.DonHangVanChuyens
                .Where(d => d.TinhTrang == 1)
                .Count();
            ViewBag.donHangDangGiao = donHangDangGiao;
            var donHangDaHuy = _context.DonHangVanChuyens
                .Where(d => d.TinhTrang == 3)
                .Count();
            ViewBag.donHangDaHuy = donHangDaHuy;
            var appDbContext = _context.DonHangVanChuyens
                .Include(d => d.DonHang)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.TrangThaiVanChuyen);
            ViewData["TrangThaiVanChuyen"] = _context.TrangThaiVanChuyens
                .Select(pttt => new SelectListItem
                {
                    Value = pttt.Id_TrangThai.ToString(),
                    Text = $"{pttt.TenTrangThai}"
                })
                .ToList();
            return View(await appDbContext.OrderByDescending(p => p.Id_DHVC).ToListAsync());
        }
        public async Task<IActionResult> OrdersNotDelivered()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var appDbContext = _context.DonHangVanChuyens
                .Include(d => d.DonHang)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.TrangThaiVanChuyen)
                .Where(d => d.TrangThaiVanChuyen.Id_TrangThai != 2)
                .OrderByDescending(d => d.DonHang.NgayDat);
            ViewData["TrangThaiVanChuyen"] = _context.TrangThaiVanChuyens
                .Select(pttt => new SelectListItem
                {
                    Value = pttt.Id_TrangThai.ToString(),
                    Text = $"{pttt.TenTrangThai}"
                })
                .ToList();
            return View(await appDbContext.OrderByDescending(p => p.Id_DHVC).ToListAsync());
        }
        public async Task<IActionResult> OrdersDelivered()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var appDbContext = _context.DonHangVanChuyens
                .Include(d => d.DonHang)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.TrangThaiVanChuyen)
                .Where(d => d.TrangThaiVanChuyen.Id_TrangThai == 2)
                .OrderByDescending(d => d.DonHang.NgayDat);
            ViewData["TrangThaiVanChuyen"] = _context.TrangThaiVanChuyens
                .Select(pttt => new SelectListItem
                {
                    Value = pttt.Id_TrangThai.ToString(),
                    Text = $"{pttt.TenTrangThai}"
                })
                .ToList();
            return View(await appDbContext.OrderByDescending(p => p.Id_DHVC).ToListAsync());
        }
        public async Task<IActionResult> OrderDetail(string id)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var session = HttpContext.Session.GetString("admin");
            ViewBag.email = session;


            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound("Không tìm thấy hóa đơn.");
            }

            var chiTietDonHang = await _context.ChiTietDonHangs
                .Where(ct => ct.Id_DonHang == id)
                .Include(p => p.DungCu)
                .ToListAsync();
            var donHangVanChuyen = await _context.DonHangVanChuyens
                .Where(ct => ct.Id_DonHang == id)
                .Include(p => p.TrangThaiVanChuyen)
                .Include(p => p.PhuongThucThanhToan)
                .FirstOrDefaultAsync();
            if (donHangVanChuyen == null)
            {
                return NotFound("Không tìm thấy hóa đơn vận chuyển.");
            }
            if (chiTietDonHang == null || chiTietDonHang.Count == 0)
            {
                return NotFound("Không tìm thấy chi tiết hóa đơn.");
            }

            var result = new OrderDetailViewModel
            {
                donHangModel = donHang,
                chiTietDonHangModel = chiTietDonHang,
                donHangVanChuyenModel = donHangVanChuyen
            };
            decimal? tongtien = donHang.TongTien;
            decimal? tamtinh = await _context.ChiTietDonHangs.Where(ct => ct.Id_DonHang == id).SumAsync(ct => ct.DonGia);
            decimal? vat = Math.Round((decimal)(tongtien * 8 / 108), 0);

            //Lấy giá trị giảm
            var magiamgia = donHang.Id_MaGiamGia;
            decimal giatrigiam = 0;
            if (magiamgia != null)
            {
                giatrigiam = (decimal)await _context.MaGiamGias.Where(gg => gg.Id_MaGiamGia == magiamgia).Select(gg => gg.GiaTriGiam).FirstOrDefaultAsync();
            }

            ViewBag.tamtinh = tamtinh;
            ViewBag.vat = vat;
            ViewBag.magiamgia = giatrigiam;
            ViewBag.tongtien = tongtien;
            ViewBag.phivanchuyen = donHangVanChuyen.PhiVanChuyen;
            return View(result);
        }
        public async Task<IActionResult> Banner()
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var appDbContext = _context.BannerModel;
            return View(await appDbContext.ToListAsync());
        }
        public IActionResult CreateBanner()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBanner(BannerModel bannerModel, IFormFile? imageInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //dungCuModel.HinhAnh= await UploadImage(imageInput, dungCuModel.Id_DungCu.ToString());

                    if (imageInput != null)
                    {
                        string? newUrl = await UploadImage(imageInput, bannerModel.TenBanner);
                        bannerModel.DuongDan = newUrl;
                        _context.Add(bannerModel);

                    }
                    else
                    {
                        bannerModel.DuongDan = "null";
                        _context.Add(bannerModel);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Banner));
                }
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = ex.Message });
            }
            return View(bannerModel);
        }
        public async Task<IActionResult> EditBanner(int? id)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return NotFound();
            }

            var bannerModel = await _context.BannerModel.FindAsync(id);
            if (bannerModel == null)
            {
                return NotFound();
            }
            return View(bannerModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBanner(int id, BannerModel bannerModel, IFormFile? imageInput = null)
        {
            if (id != bannerModel.Id_Banner)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageInput != null)
                    {
                        string? newUrl = await UpdateImage(bannerModel.DuongDan, imageInput);
                        bannerModel.DuongDan = newUrl;
                        _context.Update(bannerModel);
                        await _context.SaveChangesAsync();
                    }
                    else if (imageInput == null)
                    {
                        _context.Update(bannerModel);
                        await _context.SaveChangesAsync();
                    }
                    _context.Update(bannerModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bannerModel);
        }
        public async Task<IActionResult> DeleteBanner(int? id)
        {
            if (HttpContext.Session.GetString("admin") == null)
            {
                return RedirectToAction("Index");
            }
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                DeleteImage(banner.TenBanner);
                _context.Banners.Remove(banner);

                await _context.SaveChangesAsync();

            }
            return RedirectToAction(nameof(Banner));
        }
        [HttpPost]
        [Route("/Administrator/ChangeStatus")]
        public async Task<IActionResult> ChangeStatus(int dhvc, int value)
        {
            try
            {
                var donHangVanChuyen = await _context.DonHangVanChuyens
                                                     .Where(p => p.Id_DHVC == dhvc)
                                                     .FirstOrDefaultAsync();

                if (donHangVanChuyen != null)
                {
                    donHangVanChuyen.TinhTrang = value;
                    _context.Update(donHangVanChuyen);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                else
                {
                    // Log nếu không tìm thấy đơn hàng
                    Console.WriteLine($"Order with ID {dhvc} not found.");
                    return Json(new { success = false, error = $"Order with ID {dhvc} not found." });
                }
            }
            catch (Exception e)
            {
                // Log lỗi chi tiết
                Console.WriteLine("Error updating order status:", e.Message);
                return Json(new { success = false, error = e.Message });
            }
        }

        [HttpGet]
        [Route("/Administrator/GetEarningThisMonth")]
        public async Task<IActionResult> GetEarningThisMonth()
        {
            var earnings = new List<decimal>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < 6; i++)
            {
                var targetDate = currentDate.AddMonths(-i);
                var earning = _context.DonHangs
                    .Where(d => d.TrangThai != "Chưa thanh toán"
                                && d.NgayDat.Value.Month == targetDate.Month
                                && d.NgayDat.Value.Year == targetDate.Year)
                    .Sum(d => d.TongTien);
                earnings.Add((decimal)earning);
            }

            return Json(earnings);
        }



    }
}
