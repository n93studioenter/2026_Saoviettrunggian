//// ============================================================
//// Form1.Optimized.cs
//// Partial class của frmMain — chứa code tối ưu hiệu năng
//// Ghép Phần 1 + 2 + 3 vào cùng file này
//// ============================================================
//using DevExpress.XtraEditors;
//using FuzzySharp;
//using Newtonsoft.Json;
//using SaovietTax.Database;
//using SaovietTax.DTO;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.OleDb;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Xml;

//namespace SaovietTax
//{
//    public partial class frmMains
//    {
//        #region ================== BLOCK 1: CACHE TOÀN CỤC ==================

//        // ===== CACHE 3 TẦNG CHO Xulysohieuvattu =====
//        private readonly ConcurrentDictionary<string, (string SoHieu, double Percent)> _exactCache
//            = new ConcurrentDictionary<string, (string, double)>();

//        private readonly ConcurrentDictionary<string, List<MatchResult>> _fuzzyResultCache
//            = new ConcurrentDictionary<string, List<MatchResult>>();

//        private static readonly ConcurrentDictionary<string, double> _scoreCache
//            = new ConcurrentDictionary<string, double>();

//        // ===== CACHE CHO ĐỊNH DANH TÀI KHOẢN =====
//        private List<(string KeyValue, string TKNo, string TKCo, string Noidung, string IsChecked, string Loai)> _cachedRulesDefault;
//        private List<(string KeyValue, string TKNo, string TKCo)> _cachedRulesUuTienVao;
//        private List<(string KeyValue, string TKNo, string TKCo)> _cachedRulesUuTienRa;

//        // ===== CACHE CHO NHÀ CUNG CẤP =====
//        private readonly ConcurrentDictionary<string, string> _nccCache = new ConcurrentDictionary<string, string>();

//        // ===== CACHE CHO HỆ THỐNG TÀI KHOẢN =====
//        private readonly ConcurrentDictionary<string, int> _tkConCache = new ConcurrentDictionary<string, int>();

//        // ===== SEMAPHORE CHO DB WRITE =====
//        private readonly SemaphoreSlim _dbWriteSemaphore = new SemaphoreSlim(1, 1);

//        // ===== CACHE REGEX =====
//        private static readonly Regex _quyCachRegex = new Regex(
//            @"(\d+(g|ml|L|kg)|x\d+|(\d+\s*cái))",
//            RegexOptions.IgnoreCase | RegexOptions.Compiled);

//        // ===== MATCH RESULT CLASS =====
//        public class MatchResult
//        {
//            public string Key;
//            public double Percent;
//            public string TenChuan;
//            public string QuyCach;
//            public string DonVi;
//            public int MatchCount;
//            public double SoLuong;
//            public HashSet<string> AttrMatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//        }

//        #endregion

//        #region ================== BLOCK 2: LOGGING & HELPER ==================

//        private static readonly object _logLock = new object();

//        private static void LogToFile(string tag, Exception ex, string extra = "")
//        {
//            try
//            {
//                lock (_logLock)
//                {
//                    string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
//                    Directory.CreateDirectory(logDir);
//                    string file = Path.Combine(logDir, $"error_{DateTime.Now:yyyyMMdd}.log");
//                    File.AppendAllText(file,
//                        $"[{DateTime.Now:HH:mm:ss}] [{tag}] {ex.Message}\n" +
//                        (string.IsNullOrEmpty(extra) ? "" : $"  Extra: {extra}\n") +
//                        $"  Stack: {ex.StackTrace}\n\n");
//                }
//            }
//            catch { /* swallow */ }
//        }

//        private void ShowMessageSafe(string message, string caption = "",
//            System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.OK,
//            System.Windows.Forms.MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Information)
//        {
//            try
//            {
//                if (this.InvokeRequired)
//                    this.BeginInvoke(new Action(() => XtraMessageBox.Show(message, caption, buttons, icon)));
//                else
//                    XtraMessageBox.Show(message, caption, buttons, icon);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("ShowMessageSafe lỗi: " + ex.Message);
//            }
//        }

//        #endregion

//        #region ================== BLOCK 3: XULYSOHIEUVATTU - OPTIMIZED ==================

//        private string NormalizeNameForSearch(string input)
//        {
//            if (string.IsNullOrEmpty(input)) return input;

//            string result = Helpers.NormalizeVietnameseString(input);
//            result = result.ToLower().Trim();

//            if (_synonymDictionary != null)
//            {
//                foreach (var syn in _synonymDictionary)
//                {
//                    if (result.IndexOf(syn.Key, StringComparison.OrdinalIgnoreCase) >= 0)
//                        result = result.Replace(syn.Key, syn.Value);
//                }
//            }

//            result = Regex.Replace(result, @"[^\w\s]", " ");
//            result = Regex.Replace(result, @"\s+", " ").Trim();
//            return result;
//        }

//        private double GetCachedScore(string a, string b)
//        {
//            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

//            if (string.CompareOrdinal(a, b) > 0) { var t = a; a = b; b = t; }
//            string ck = a + "\u0001" + b;

//            return _scoreCache.GetOrAdd(ck, _ => CalculateMaterialSimilarity(a, b));
//        }

//        /// <summary>
//        /// Xử lý mã vật tư - 3 TẦNG CACHE
//        /// </summary>
//        private void Xulysohieuvattu(TbImportDetail tbImportDetail)
//        {
//            try
//            {
//                if (tbImportDetail == null || string.IsNullOrEmpty(tbImportDetail.Ten))
//                {
//                    if (tbImportDetail != null)
//                    {
//                        tbImportDetail.SoHieu = "";
//                        tbImportDetail.Percent = 0;
//                    }
//                    return;
//                }

//                if (!_isIndexBuilt) BuildIndexes();
//                if (_synonymDictionary == null) InitializeSynonymDictionary();
//                if (_attributeDictionary == null) InitializeAttributeDictionary();

//                string originalTen = tbImportDetail.Ten.Trim();
//                string key = NormalizeNameForSearch(originalTen);
//                string donViTinh = tbImportDetail.DVT?.Trim()?.ToLower() ?? "";

//                if (string.IsNullOrEmpty(key))
//                {
//                    tbImportDetail.SoHieu = GenerateResultString(NormalizeVietnameseString(originalTen));
//                    tbImportDetail.Percent = 0;
//                    return;
//                }

//                double minPercent;
//                if (!double.TryParse(txtTylechonHH.Text, out minPercent))
//                    minPercent = 50;

//                // ===== L1: EXACT CACHE (O(1)) =====
//                if (_exactCache.TryGetValue(key, out var exact))
//                {
//                    tbImportDetail.SoHieu = exact.SoHieu;
//                    tbImportDetail.Percent = exact.Percent == 0 ? 100 : exact.Percent;
//                    return;
//                }

//                // ===== L2: TÌM CHÍNH XÁC =====
//                if (_optimizedVatTu != null)
//                {
//                    foreach (var kvp in _optimizedVatTu)
//                    {
//                        string tenChuanNorm = NormalizeNameForSearch(kvp.Value.TenChuan);
//                        string tenPhuNorm = string.IsNullOrEmpty(kvp.Value.TenPhuChuan)
//                            ? null
//                            : NormalizeNameForSearch(kvp.Value.TenPhuChuan);

//                        if (tenChuanNorm != key && tenPhuNorm != key) continue;

//                        var invoiceAttrs = ExtractAttributes(key);
//                        var materialAttrs = ExtractAttributes(tenChuanNorm);

//                        bool hasConflict = invoiceAttrs.Any() && materialAttrs.Any()
//                            && !invoiceAttrs.Intersect(materialAttrs, StringComparer.OrdinalIgnoreCase).Any();

//                        if (hasConflict) continue;

//                        tbImportDetail.SoHieu = kvp.Key;
//                        tbImportDetail.Percent = 100;
//                        tbImportDetail.DVT = Helpers.ConvertVniToUnicode(kvp.Value.DonVi);
//                        _exactCache[key] = (kvp.Key, 100);
//                        return;
//                    }
//                }

//                // ===== L3: FUZZY RESULT CACHE =====
//                if (_fuzzyResultCache.TryGetValue(key, out var cachedResults))
//                {
//                    var best = cachedResults[0];
//                    tbImportDetail.SoHieu = best.Key;
//                    tbImportDetail.Percent = best.Percent;
//                    tbImportDetail.DVT = best.DonVi;
//                    _exactCache[key] = (best.Key, best.Percent);
//                    return;
//                }

//                // ===== L4: TÍNH TOÁN =====
//                var results = ComputeFuzzyMatches(key, donViTinh, minPercent);

//                if (results != null && results.Count > 0)
//                {
//                    var best = results[0];
//                    tbImportDetail.SoHieu = best.Key;
//                    tbImportDetail.Percent = best.Percent;
//                    tbImportDetail.DVT = best.DonVi;

//                    _fuzzyResultCache[key] = results;
//                    _exactCache[key] = (best.Key, best.Percent);
//                }
//                else
//                {
//                    string auto = GenerateResultString(NormalizeVietnameseString(originalTen));
//                    tbImportDetail.SoHieu = auto;
//                    tbImportDetail.Percent = 0;
//                    _exactCache[key] = (auto, 0);
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"❌ Lỗi Xulysohieuvattu: {ex.Message}");
//                if (tbImportDetail != null)
//                {
//                    tbImportDetail.Percent = 0;
//                    if (string.IsNullOrEmpty(tbImportDetail.SoHieu))
//                        tbImportDetail.SoHieu = GenerateResultString(tbImportDetail.Ten ?? "");
//                }
//            }
//        }

//        /// <summary>
//        /// Tính fuzzy matches — chỉ chạy khi cache miss
//        /// </summary>
//        private List<MatchResult> ComputeFuzzyMatches(string key, string donViTinh, double minPercent)
//        {
//            var invoiceTokens = GetMeaningfulTokens(key);
//            var invoiceAttrs = ExtractAttributes(key);
//            string quyCach = _quyCachRegex.Match(key).Value;
//            string tenHoaDonKhongNgoac = ExtractMainName(key);

//            var candidateKeys = new HashSet<string>();

//            if (_keywordIndex != null)
//            {
//                foreach (var word in invoiceTokens)
//                {
//                    if (_keywordIndex.TryGetValue(word, out var set))
//                        foreach (var k in set) candidateKeys.Add(k);
//                }
//            }

//            if (!string.IsNullOrEmpty(quyCach) && _quyCachIndex != null
//                && _quyCachIndex.TryGetValue(quyCach, out var qcSet))
//            {
//                foreach (var k in qcSet) candidateKeys.Add(k);
//            }

//            if (candidateKeys.Count == 0 && _optimizedVatTu != null)
//            {
//                int count = 0;
//                foreach (var kv in _optimizedVatTu)
//                {
//                    candidateKeys.Add(kv.Key);
//                    if (++count >= 200) break;
//                }
//            }

//            if (candidateKeys.Count == 0) return new List<MatchResult>();

//            var results = new List<MatchResult>(candidateKeys.Count);

//            foreach (var ck in candidateKeys)
//            {
//                if (!_optimizedVatTu.TryGetValue(ck, out var vatTu)) continue;

//                string tenChuanHoa = NormalizeNameForSearch(vatTu.TenChuan);
//                string tenKhongNgoac = ExtractMainName(tenChuanHoa);

//                double percentNoBracket = GetCachedScore(tenKhongNgoac, tenHoaDonKhongNgoac);
//                double percent = GetCachedScore(tenChuanHoa, key);
//                double finalPercent = Math.Max(percent, percentNoBracket);

//                var materialAttrs = ExtractAttributes(tenChuanHoa);
//                var matchedAttrs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

//                if (invoiceAttrs.Any() && materialAttrs.Any())
//                {
//                    var intersect = invoiceAttrs.Intersect(materialAttrs, StringComparer.OrdinalIgnoreCase).ToList();
//                    if (intersect.Any())
//                    {
//                        finalPercent += 15;
//                        foreach (var a in intersect) matchedAttrs.Add(a);
//                    }
//                    else
//                    {
//                        finalPercent -= 30;
//                    }
//                }
//                else if (invoiceAttrs.Any() || materialAttrs.Any())
//                {
//                    finalPercent -= 20;
//                }

//                string quyCachTrongKho = vatTu.QuyCach?.ToLower()?.Trim() ?? "";
//                if (!string.IsNullOrEmpty(quyCach) && !string.IsNullOrEmpty(quyCachTrongKho))
//                {
//                    if (quyCachTrongKho == quyCach || quyCachTrongKho.Contains(quyCach))
//                        finalPercent += 5;
//                }

//                if (finalPercent > 100) finalPercent = 100;
//                if (finalPercent < 0) finalPercent = 0;

//                string vatTuDvtVni = Helpers.ConvertUnicodeToVni(vatTu.DonVi).ToLower();
//                if (!string.IsNullOrEmpty(donViTinh) && !string.IsNullOrEmpty(vatTuDvtVni)
//                    && donViTinh != vatTuDvtVni)
//                {
//                    finalPercent = 0;
//                }

//                if (finalPercent >= minPercent && invoiceAttrs.Count > 0)
//                {
//                    bool anyAttrInName = false;
//                    foreach (var item in invoiceAttrs)
//                    {
//                        if (tenChuanHoa.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
//                        { anyAttrInName = true; break; }
//                    }
//                    if (!anyAttrInName) continue;
//                }

//                if (finalPercent >= minPercent)
//                {
//                    results.Add(new MatchResult
//                    {
//                        Key = ck,
//                        Percent = finalPercent,
//                        TenChuan = vatTu.TenChuan,
//                        QuyCach = vatTu.QuyCach,
//                        DonVi = vatTu.DonVi,
//                        MatchCount = invoiceTokens.Intersect(
//                            GetMeaningfulTokens(tenChuanHoa),
//                            StringComparer.OrdinalIgnoreCase).Count(),
//                        SoLuong = vatTu.soluong,
//                        AttrMatch = matchedAttrs
//                    });
//                }
//            }

//            results.Sort((a, b) =>
//            {
//                int c = b.Percent.CompareTo(a.Percent);
//                if (c != 0) return c;
//                c = b.MatchCount.CompareTo(a.MatchCount);
//                if (c != 0) return c;
//                c = b.AttrMatch.Count.CompareTo(a.AttrMatch.Count);
//                if (c != 0) return c;
//                return b.SoLuong.CompareTo(a.SoLuong);
//            });

//            if (results.Count > 20)
//                results = results.GetRange(0, 20);

//            return results;
//        }

//        private string ExtractMainName(string fullName)
//        {
//            if (string.IsNullOrEmpty(fullName)) return fullName;

//            string result = Regex.Replace(fullName, @"\(.*?\)", "").Trim();
//            result = Regex.Replace(result, @"\[.*?\]", "").Trim();
//            result = Regex.Replace(result, @"\{.*?\}", "").Trim();
//            result = Regex.Replace(result, @"\s+", " ").Trim();
//            return result;
//        }

//        private List<string> GetMeaningfulTokens(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

//            return text.ToLowerInvariant()
//                .Split(new[] { ' ', '-', ',', ';', '/', '(', ')', '[', ']' },
//                    StringSplitOptions.RemoveEmptyEntries)
//                .Where(x => x.Length >= 2)
//                .Distinct()
//                .ToList();
//        }

//        #endregion

//        #region ================== BLOCK 4: BUILD INDEXES ==================

//        private void BuildIndexes()
//        {
//            try
//            {
//                if (_isIndexBuilt) return;

//                _keywordIndex = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
//                _quyCachIndex = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

//                if (_optimizedVatTu == null)
//                {
//                    InitializeVatTuOptimization();
//                    if (_optimizedVatTu == null) return;
//                }

//                var keywordLocal = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
//                var quyCachLocal = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

//                foreach (var kvp in _optimizedVatTu)
//                {
//                    var words = kvp.Value.TenChuan.ToLower()
//                        .Split(new[] { ' ', '-', ',', ';', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
//                        .Where(w => w.Length >= 2);

//                    foreach (var word in words)
//                    {
//                        if (!keywordLocal.TryGetValue(word, out var set))
//                        {
//                            set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//                            keywordLocal[word] = set;
//                        }
//                        set.Add(kvp.Key);
//                    }

//                    if (!string.IsNullOrEmpty(kvp.Value.QuyCach))
//                    {
//                        if (!quyCachLocal.TryGetValue(kvp.Value.QuyCach, out var qset))
//                        {
//                            qset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//                            quyCachLocal[kvp.Value.QuyCach] = qset;
//                        }
//                        qset.Add(kvp.Key);
//                    }
//                }

//                _keywordIndex = keywordLocal;
//                _quyCachIndex = quyCachLocal;
//                _isIndexBuilt = true;

//                Debug.WriteLine($"✅ BuildIndexes: {_keywordIndex.Count} keywords, {_optimizedVatTu.Count} vattu");
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("❌ BuildIndexes: " + ex.Message);
//                LogToFile("BuildIndexes", ex);
//            }
//        }

//        private void InitializeVatTuOptimization()
//        {
//            if (vatTuLookup == null || vatTuLookup.Count == 0)
//            {
//                Debug.WriteLine("⚠ vatTuLookup rỗng, bỏ qua InitializeVatTuOptimization");
//                return;
//            }

//            _optimizedVatTu = new Dictionary<string, (string TenChuan, string TenPhuChuan, string QuyCach, string DonVi, double Dongia, double soluong)>(vatTuLookup.Count);

//            foreach (var item in vatTuLookup)
//            {
//                string ten1 = Helpers.NormalizeVietnameseString(item.Value.TenVattu);
//                string ten2 = string.IsNullOrEmpty(item.Value.TenVattu2)
//                    ? ""
//                    : Helpers.NormalizeVietnameseString(item.Value.TenVattu2);
//                string quyCach = _quyCachRegex.Match(ten1).Value;

//                _optimizedVatTu[item.Key] = (ten1, ten2, quyCach, item.Value.DonVi, item.Value.Dongia, item.Value.SoLuong);
//            }

//            Debug.WriteLine($"✅ InitializeVatTuOptimization: {_optimizedVatTu.Count} items");
//        }

//        private Dictionary<string, dynamic> GetVatTuFromList(string soHieu)
//        {
//            if (_lstvtBySoHieu == null)
//            {
//                lock (_lstvtLock)
//                {
//                    if (_lstvtBySoHieu == null)
//                    {
//                        _lstvtBySoHieu = new Dictionary<string, dynamic>();
//                        foreach (var v in lstvt)
//                        {
//                            if (!_lstvtBySoHieu.ContainsKey(v.SoHieu))
//                                _lstvtBySoHieu[v.SoHieu] = v;
//                        }
//                    }
//                }
//            }
//            return _lstvtBySoHieu.TryGetValue(soHieu, out var result) ? result : null;
//        }

//        private void InvalidateVatTuCaches()
//        {
//            _exactCache.Clear();
//            _fuzzyResultCache.Clear();
//            _scoreCache.Clear();
//            _lstvtBySoHieu = null;
//            _isIndexBuilt = false;
//            _optimizedVatTu = null;
//            _keywordIndex = null;
//            _quyCachIndex = null;
//        }

//        #endregion
//    }
//            #region ================== BLOCK 5: EXECUTE QUERY - OPTIMIZED ==================

//        public System.Data.DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
//        {
//            var dataTable = new System.Data.DataTable();

//            try
//            {
//                using (var connection = new OleDbConnection(connectionString))
//                using (var command = new OleDbCommand(query, connection))
//                using (var dataAdapter = new OleDbDataAdapter(command))
//                {
//                    if (parameters != null) command.Parameters.AddRange(parameters);
//                    connection.Open();
//                    dataAdapter.Fill(dataTable);
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"❌ ExecuteQuery: {ex.Message}\nSQL: {query}");
//                LogToFile("ExecuteQuery", ex, query);
//            }

//            return dataTable;
//        }

//        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
//        {
//            try
//            {
//                using (var connection = new OleDbConnection(connectionString))
//                {
//                    connection.Open();

//                    using (var command = new OleDbCommand(query, connection))
//                    {
//                        if (parameters != null) command.Parameters.AddRange(parameters);
//                        command.ExecuteNonQuery();
//                    }

//                    using (var idCommand = new OleDbCommand("SELECT @@IDENTITY", connection))
//                    {
//                        object result = idCommand.ExecuteScalar();
//                        if (result == null || result == DBNull.Value) return 0;
//                        return Convert.ToInt32(result);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                LogToFile("ExecuteQueryResult", ex, query);
//                throw;
//            }
//        }

//        public int ExecuteNonQuery2(string query, params OleDbParameter[] parameters)
//        {
//            using (var connection = new OleDbConnection(connectionString))
//            {
//                connection.Open();
//                using (var command = new OleDbCommand(query, connection))
//                {
//                    if (parameters != null) command.Parameters.AddRange(parameters);
//                    return command.ExecuteNonQuery();
//                }
//            }
//        }

//        #endregion

//        #region ================== BLOCK 6: SAVE XML - BATCH INSERT ==================

//        private async Task SaveDataXmlOne(TbImport item, int type)
//        {
//            if (item == null) return;

//            await _dbWriteSemaphore.WaitAsync();
//            try
//            {
//                using (var conn = new OleDbConnection(connectionString))
//                {
//                    await conn.OpenAsync();
//                    using (var trans = conn.BeginTransaction())
//                    {
//                        try
//                        {
//                            await InsertOneInvoiceInTx(conn, trans, item, type);
//                            trans.Commit();
//                        }
//                        catch (Exception ex)
//                        {
//                            trans.Rollback();
//                            LogToFile("SaveDataXmlOne", ex, $"HĐ {item.SHDon}");
//                            throw;
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                _dbWriteSemaphore.Release();
//            }
//        }

//        private async Task InsertOneInvoiceInTx(OleDbConnection conn, OleDbTransaction trans,
//            TbImport item, int type)
//        {
//            string sqlParent = @"INSERT INTO tbImport 
//                (SHDon,KHHDon,NLap,Ten,Noidung,TKNo,TKCo,TkThue,Mst,[Status],Ngaytao,
//                 TongTien,Vat,TPhi,TgTCThue,TgTThue,[Type],InvoiceType,IsHaschild,
//                 TVat,Vat2,TVat2,Vat3,TVat3,TgTCThue1,TgTCThue2,TgTCThue3,
//                 Khmshdon,hdon,[Path],NCC) 
//                VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

//            using (var cmd = new OleDbCommand(sqlParent, conn, trans))
//            {
//                cmd.Parameters.AddWithValue("?", item.SHDon ?? "");
//                cmd.Parameters.AddWithValue("?", item.KHHDon ?? "");
//                cmd.Parameters.AddWithValue("?", item.NLap);
//                cmd.Parameters.AddWithValue("?", item.Ten ?? "");
//                cmd.Parameters.AddWithValue("?", item.Noidung ?? "");
//                cmd.Parameters.AddWithValue("?", item.TKNo ?? "");
//                cmd.Parameters.AddWithValue("?", item.TKCo ?? "");
//                cmd.Parameters.AddWithValue("?", item.TkThue ?? "");
//                cmd.Parameters.AddWithValue("?", item.Mst ?? "");
//                cmd.Parameters.AddWithValue("?", "0");
//                cmd.Parameters.AddWithValue("?", DateTime.Now.ToShortDateString());
//                cmd.Parameters.AddWithValue("?", item.TongTien);
//                cmd.Parameters.AddWithValue("?", item.Vat);
//                cmd.Parameters.AddWithValue("?", item.TPhi ?? "0");
//                cmd.Parameters.AddWithValue("?", Math.Round(item.TgTCThue));
//                cmd.Parameters.AddWithValue("?", Math.Round(item.TgTThue));
//                cmd.Parameters.AddWithValue("?", type);
//                cmd.Parameters.AddWithValue("?", "0");
//                cmd.Parameters.AddWithValue("?", "1");
//                cmd.Parameters.AddWithValue("?", item.TVat);
//                cmd.Parameters.AddWithValue("?", item.Vat2 ?? "0");
//                cmd.Parameters.AddWithValue("?", item.TVat2);
//                cmd.Parameters.AddWithValue("?", item.Vat3 ?? "0");
//                cmd.Parameters.AddWithValue("?", item.TVat3);
//                cmd.Parameters.AddWithValue("?", item.TgTCThue1);
//                cmd.Parameters.AddWithValue("?", item.TgTCThue2);
//                cmd.Parameters.AddWithValue("?", item.TgTCThue3);
//                cmd.Parameters.AddWithValue("?", item.Khmshdon ?? "");
//                cmd.Parameters.AddWithValue("?", item.hdon ?? "");
//                cmd.Parameters.AddWithValue("?", item.Path ?? "");
//                cmd.Parameters.AddWithValue("?", item.NCC ?? "");

//                await cmd.ExecuteNonQueryAsync();
//            }

//            int parentID;
//            using (var cmd = new OleDbCommand("SELECT @@IDENTITY", conn, trans))
//                parentID = Convert.ToInt32(await cmd.ExecuteScalarAsync());

//            await BulkInsertDetailsAsync(conn, trans, parentID, item.tbImportDetails);
//        }

//        private async Task BulkInsertDetailsAsync(OleDbConnection conn, OleDbTransaction trans,
//            int parentID, List<TbImportDetail> details)
//        {
//            if (details == null || details.Count == 0) return;

//            const int BATCH = 200;
//            for (int i = 0; i < details.Count; i += BATCH)
//            {
//                var chunk = details.Skip(i).Take(BATCH).ToList();

//                var sb = new StringBuilder();
//                sb.Append("INSERT INTO tbimportdetail (ParentId,SoHieu,SoLuong,DonGia,DVT,Ten,MaCT,TKNo,TKCo,TTien,[Percent],Tchat,SoPSGoc,VAT,Hangam) VALUES ");

//                var parameters = new List<OleDbParameter>(chunk.Count * 15);

//                for (int j = 0; j < chunk.Count; j++)
//                {
//                    if (j > 0) sb.Append(",");
//                    sb.Append("(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");

//                    var d = chunk[j];
//                    int hangam = 0;
//                    var getvattu = lstvt?.FirstOrDefault(m => d.SoHieu != null
//                        && m.SoHieu.Equals(d.SoHieu, StringComparison.OrdinalIgnoreCase));
//                    if (getvattu != null && getvattu.SoLuong <= 0) hangam = 1;

//                    parameters.Add(new OleDbParameter("?", parentID));
//                    parameters.Add(new OleDbParameter("?", d.SoHieu ?? ""));
//                    parameters.Add(new OleDbParameter("?", d.Soluong));
//                    parameters.Add(new OleDbParameter("?", d.Dongia));
//                    parameters.Add(new OleDbParameter("?", d.DVT ?? ""));
//                    parameters.Add(new OleDbParameter("?", d.Ten ?? ""));
//                    parameters.Add(new OleDbParameter("?", ""));
//                    parameters.Add(new OleDbParameter("?", d.TKNo ?? ""));
//                    parameters.Add(new OleDbParameter("?", d.TKCo ?? ""));
//                    parameters.Add(new OleDbParameter("?", d.TTien));
//                    parameters.Add(new OleDbParameter("?", d.Percent));
//                    parameters.Add(new OleDbParameter("?", d.Tchat));
//                    parameters.Add(new OleDbParameter("?", d.TTien));
//                    parameters.Add(new OleDbParameter("?", d.Vat));
//                    parameters.Add(new OleDbParameter("?", hangam));
//                }

//                using (var cmd = new OleDbCommand(sb.ToString(), conn, trans))
//                {
//                    cmd.Parameters.AddRange(parameters.ToArray());
//                    await cmd.ExecuteNonQueryAsync();
//                }
//            }
//        }

//        private async Task SaveAllInvoicesBulk(List<TbImport> invoices, int type)
//        {
//            if (invoices == null || invoices.Count == 0) return;

//            const int BATCH = 50;

//            await _dbWriteSemaphore.WaitAsync();
//            try
//            {
//                using (var conn = new OleDbConnection(connectionString))
//                {
//                    await conn.OpenAsync();

//                    for (int i = 0; i < invoices.Count; i += BATCH)
//                    {
//                        var chunk = invoices.Skip(i).Take(BATCH).ToList();

//                        using (var trans = conn.BeginTransaction())
//                        {
//                            try
//                            {
//                                foreach (var item in chunk)
//                                    await InsertOneInvoiceInTx(conn, trans, item, type);

//                                trans.Commit();
//                            }
//                            catch (Exception ex)
//                            {
//                                trans.Rollback();
//                                LogToFile("SaveAllInvoicesBulk", ex, $"batch {i}");
//                                throw;
//                            }
//                        }

//                        SetCaption($"Đã lưu {Math.Min(i + BATCH, invoices.Count)}/{invoices.Count} hoá đơn...");
//                    }
//                }
//            }
//            finally
//            {
//                _dbWriteSemaphore.Release();
//            }
//        }

//        #endregion

//        #region ================== BLOCK 7: LOAD DATA VATTU - OPTIMIZED ==================

//        public async Task<List<DTO.VatTu>> LoadDataVattuAsync()
//        {
//            try
//            {
//                var queryVatTu = "SELECT * FROM Vattu";
//                var queryMaphanloai = "SELECT * FROM PhanLoaiVattu";

//                var result = await Task.Run(() =>
//                {
//                    var listVattu = ExecuteQuery(queryVatTu, null);
//                    var listPl = ExecuteQuery(queryMaphanloai, null);

//                    var plDict = new Dictionary<string, string>(listPl.Rows.Count);
//                    foreach (DataRow pl in listPl.Rows)
//                        plDict[pl["MaSo"].ToString()] = pl["TenPhanLoai"].ToString();

//                    foreach (DataRow row in listVattu.Rows)
//                    {
//                        row["TenVattu"] = Helpers.ConvertVniToUnicode(row["TenVattu"].ToString());
//                        row["TenVattu2"] = Helpers.ConvertVniToUnicode(row["TenVattu2"].ToString());
//                        row["DonVi"] = Helpers.ConvertVniToUnicode(row["DonVi"].ToString());
//                    }

//                    var maVatTuList = listVattu.Rows.Cast<DataRow>()
//                        .Select(r => int.Parse(r["MaSo"].ToString()))
//                        .Distinct()
//                        .ToList();

//                    if (maVatTuList.Count == 0) return new List<DTO.VatTu>();

//                    string queryTonKho = "SELECT * FROM TonKho WHERE MaVatTu IN ("
//                        + string.Join(",", maVatTuList) + ")";
//                    var allTonKho = ExecuteQuery(queryTonKho, null);

//                    var tonKhoDict = new Dictionary<int, DataRow>(allTonKho.Rows.Count);
//                    foreach (DataRow r in allTonKho.Rows)
//                    {
//                        int id = int.Parse(r["MaVatTu"].ToString());
//                        if (!tonKhoDict.ContainsKey(id))
//                            tonKhoDict[id] = r;
//                    }

//                    var lstVattu = new List<DTO.VatTu>(listVattu.Rows.Count);
//                    foreach (DataRow item in listVattu.Rows)
//                    {
//                        int maSo = int.Parse(item["MaSo"].ToString());
//                        var vt = new DTO.VatTu
//                        {
//                            MaSo = maSo,
//                            MaPhanLoai = int.Parse(item["MaPhanLoai"].ToString()),
//                            TenVattu = item["TenVattu"].ToString(),
//                            TenVattu2 = item["TenVattu2"].ToString(),
//                            SoHieu = item["SoHieu"].ToString(),
//                            DonVi = item["DonVi"].ToString(),
//                            GhiChu = item["GhiChu"].ToString(),
//                            PTGB = item.Table.Columns.Contains("PTGB") ? item["PTGB"].ToString() : "",
//                            TenMaPhanLoai = plDict.TryGetValue(item["MaPhanLoai"].ToString(), out var tpl) ? tpl : ""
//                        };

//                        if (tonKhoDict.TryGetValue(maSo, out var tk))
//                        {
//                            const int cnt = 12;
//                            var luongCol = "Luong_" + cnt;
//                            var tienCol = "Tien_" + cnt;

//                            if (tk.Table.Columns.Contains(luongCol) && tk[luongCol] != DBNull.Value)
//                                vt.SoLuong = double.TryParse(tk[luongCol].ToString(), out var sl) ? sl : 0;

//                            if (tk.Table.Columns.Contains(tienCol) && tk[tienCol] != DBNull.Value)
//                                vt.ThanhTien = double.TryParse(tk[tienCol].ToString(), out var tt) ? tt : 0;

//                            if (vt.SoLuong != 0 && vt.ThanhTien != 0)
//                                vt.Dongia = vt.ThanhTien / vt.SoLuong;
//                        }

//                        try
//                        {
//                            if (existingTbChungtu != null)
//                            {
//                                var lastCt = existingTbChungtu.AsEnumerable()
//                                    .Where(m => int.Parse(m["MaVattu"].ToString()) == maSo
//                                        && double.Parse(m["SoPS"].ToString()) != 0
//                                        && double.Parse(m["SoPS2Co"].ToString()) != 0
//                                        && !m["SoHieu"].ToString().Contains("V"))
//                                    .LastOrDefault();

//                                if (lastCt != null)
//                                {
//                                    double sp2co = double.Parse(lastCt["SoPS2Co"].ToString());
//                                    double sps = double.Parse(lastCt["SoPS"].ToString());
//                                    vt.Dongia2 = sp2co > 0 ? Math.Round(sps / sp2Co) : 0;
//                                }
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            Debug.WriteLine($"Lỗi tính đơn giá {vt.SoHieu}: {ex.Message}");
//                        }

//                        lstVattu.Add(vt);
//                    }

//                    return lstVattu;
//                });

//                lstvt = result;
//                BuildLookup();

//                return result;
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"❌ LoadDataVattuAsync: {ex.Message}");
//                LogToFile("LoadDataVattuAsync", ex);
//                return new List<DTO.VatTu>();
//            }
//        }

//        private void BuildLookup()
//        {
//            _lookupByTenChinh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
//            _lookupByTenPhu = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
//            _lookupByTenChinhs = new Dictionary<string, VatTuInfo>(StringComparer.OrdinalIgnoreCase);

//            vatTuLookup = lstvt
//                .Where(v => !string.IsNullOrEmpty(v.SoHieu))
//                .GroupBy(v => v.SoHieu)
//                .ToDictionary(
//                    g => g.Key,
//                    g =>
//                    {
//                        var v = g.First();
//                        return (v.TenVattu, v.TenVattu2, v.DonVi, v.Dongia, v.SoLuong);
//                    });

//            var matcher = new VietnameseProductMatcher();

//            foreach (var kvp in vatTuLookup)
//            {
//                string sohieu = kvp.Key;
//                string key1 = matcher.NormalizeVietnameseProduct(
//                    Helpers.NormalizeVietnameseString(kvp.Value.TenVattu?.Trim() ?? ""));

//                if (!string.IsNullOrEmpty(key1))
//                {
//                    _lookupByTenChinh[key1] = sohieu;
//                    _lookupByTenChinhs[key1] = new VatTuInfo
//                    {
//                        Ma = sohieu,
//                        DonViTinh = Helpers.NormalizeVietnameseString(kvp.Value.DonVi ?? "")
//                    };
//                }

//                if (!string.IsNullOrEmpty(kvp.Value.TenVattu2))
//                {
//                    string key2 = Helpers.NormalizeVietnameseString(kvp.Value.TenVattu2.Trim());
//                    if (!string.IsNullOrEmpty(key2))
//                        _lookupByTenPhu[key2] = sohieu;
//                }
//            }

//            InvalidateVatTuCaches();
//            InitializeVatTuOptimization();
//            BuildIndexes();
//        }

//        #endregion
//        #region ================== BLOCK 8: DOCFILEXMLONE - OPTIMIZED ==================

//        /// <summary>
//        /// Đọc 1 file XML — CHỈ PARSE, KHÔNG ghi DB, KHÔNG gọi SaveDataXmlOne
//        /// </summary>
//        private async Task<TbImport> DocfileXmlOne(string pathXml, int stt)
//        {
//            if (string.IsNullOrEmpty(pathXml)) return null;
//            if (!File.Exists(pathXml)) return null;

//            // Skip nếu đã xử lý (thread-safe)
//            if (!_processedXmlPaths.TryAdd(pathXml, 0))
//                return null;

//            // Skip nhanh nếu path đã có trong DB
//            if (checktbImport != null && checktbImport.AsEnumerable()
//                    .Any(m => m.Field<string>("Path") == pathXml))
//                return null;

//            if (pathXml.IndexOf("html", StringComparison.OrdinalIgnoreCase) >= 0)
//                return null;

//            if (tongfile == 0) tongfile = totalInvoices;

//            int localType = chkDauvao.Checked ? 1 : 2;

//            // Progress
//            if (modeClick != 2)
//            {
//                string loai = localType == 1 ? "Đầu vào" : "Đầu ra";
//                SetCaption($"Đang đọc file thứ {sothutu}/{tongfile}");
//                if (Isrunning && frmStatusAuto != null)
//                {
//                    frmStatusAuto.LoadMessage($"({loai}) - Đang đọc file thứ {sothutu}/{tongfile}");
//                }
//            }

//            TbImport tbImport = null;
//            try
//            {
//                tbImport = await Task.Run(() => ParseXmlFileInternal(pathXml, localType));
//            }
//            catch (Exception ex)
//            {
//                LogToFile("DocfileXmlOne", ex, pathXml);
//            }
//            finally
//            {
//                sothutu++;
//            }

//            if (tbImport == null) return null;

//            // Đánh dấu vào lookup
//            if (modeClick != 2)
//            {
//                var key = NormalizeTbImportKey(tbImport.Mst, tbImport.SHDon, tbImport.NLap, localType);
//                lookupTbImport.Add(key);
//            }

//            return tbImport;
//        }

//        /// <summary>
//        /// Parse XML — toàn bộ logic đọc file, KHÔNG chạm DB
//        /// </summary>
//        private TbImport ParseXmlFileInternal(string pathXml, int type)
//        {
//            XmlDocument xmlDoc = new XmlDocument();
//            var settings = new XmlReaderSettings
//            {
//                DtdProcessing = DtdProcessing.Parse,
//                XmlResolver = null,
//                Async = false,
//                CheckCharacters = false
//            };

//            using (var xmlReader = XmlReader.Create(pathXml, settings))
//            {
//                xmlDoc.Load(xmlReader);
//            }

//            XmlNode root = xmlDoc.DocumentElement;
//            if (root == null) return null;

//            XmlNode ttChung = root.SelectSingleNode("//TTChung");
//            XmlNode nBan = root.SelectSingleNode("//NBan");
//            XmlNode nMua = root.SelectSingleNode("//NMua");
//            XmlNode ttToan = root.SelectSingleNode("//TToan");
//            XmlNode tHDon = root.SelectSingleNode("//THDon");

//            if (ttChung == null || ttToan == null) return null;

//            var tb = new TbImport { Path = pathXml };

//            // === Loại hóa đơn ===
//            string thDonText = tHDon?.InnerText ?? "";
//            string thDonNorm = Helpers.NormalizeVietnameseString(thDonText.ToLower());
//            if (thDonNorm.Contains("giá trị gia tăng") || thDonNorm.Contains("gtgt"))
//                tb.hdon = "01";
//            else if (thDonNorm.Contains("bán hàng"))
//                tb.hdon = "02";
//            else
//                tb.hdon = "01";

//            // === Ngày lập ===
//            if (!DateTime.TryParse(ttChung.SelectSingleNode("NLap")?.InnerText, out DateTime nLap))
//                return null;
//            tb.NLap = nLap;

//            // Filter ngày
//            if (tb.NLap < dtTungay.DateTime || tb.NLap > dtDenngay.DateTime)
//                return null;

//            // === Nội dung điều chỉnh ===
//            var ttKhacNodes = ttChung.SelectNodes("TTKhac/TTin");
//            if (ttKhacNodes != null)
//            {
//                foreach (XmlNode node in ttKhacNodes)
//                {
//                    string dLieu = node.SelectSingleNode("DLieu")?.InnerText;
//                    if (!string.IsNullOrEmpty(dLieu)
//                        && dLieu.IndexOf("điều chỉnh", StringComparison.OrdinalIgnoreCase) >= 0)
//                    {
//                        tb.Noidung = Helpers.ConvertUnicodeToVni(dLieu);
//                        break;
//                    }
//                }
//            }

//            // === Số HĐ, ký hiệu ===
//            tb.SHDon = Helpers.RemoveLeadingZeros(ttChung.SelectSingleNode("SHDon")?.InnerText ?? "");
//            tb.KHHDon = ttChung.SelectSingleNode("KHHDon")?.InnerText ?? "";
//            tb.Khmshdon = root.SelectSingleNode("//KHMSHDon")?.InnerText ?? "";

//            // NCC
//            string nccCode = ttChung.SelectSingleNode("MSTTCGP")?.InnerText;
//            tb.NCC = GetNCCNameCached(nccCode);

//            // Phí
//            string tPhi = ttToan.SelectSingleNode("//TPhi")?.InnerText;
//            if (!string.IsNullOrEmpty(tPhi)) tb.TPhi = tPhi;

//            // === MST công ty & đối tác ===
//            if (type == 1)
//            {
//                if (mstcongty != nMua?.SelectSingleNode("MST")?.InnerText
//                    && CCCD != nMua?.SelectSingleNode("MST")?.InnerText)
//                    return null;

//                tb.Ten = Helpers.ConvertUnicodeToVni(nBan?.SelectSingleNode("Ten")?.InnerText ?? "");
//                tb.Mst = nBan?.SelectSingleNode("MST")?.InnerText ?? "";
//            }
//            else
//            {
//                if (mstcongty != nBan?.SelectSingleNode("MST")?.InnerText)
//                    return null;

//                string tenDoiTac =
//                    !string.IsNullOrWhiteSpace(nMua?.SelectSingleNode("Ten")?.InnerText)
//                        ? nMua.SelectSingleNode("Ten").InnerText
//                        : nMua?.SelectSingleNode("HVTNMHang")?.InnerText ?? "";

//                tb.Ten = Helpers.ConvertUnicodeToVni(tenDoiTac);
//                tb.Mst = nMua?.SelectSingleNode("MST")?.InnerText
//                    ?? nMua?.SelectSingleNode("CCCDan")?.InnerText ?? "";

//                if (string.IsNullOrEmpty(tb.Ten)
//                    || tb.Ten.Contains("khaùch khoâng laáy hoùa ñôn")
//                    || tb.Ten.Contains("Ngöôøi mua khoâng laáy hoùa ñôn")
//                    || tb.Ten.Contains("Khaùch leû"))
//                {
//                    var kl = tbKhachhang.AsEnumerable()
//                        .FirstOrDefault(m => m.Field<string>("SoHieu") == "KL");
//                    if (kl != null)
//                    {
//                        tb.Ten = kl.Field<string>("Ten");
//                        tb.Mst = "KL";
//                    }
//                }
//            }

//            // === Sinh số hiệu KH nếu thiếu MST ===
//            if (string.IsNullOrEmpty(tb.Mst) && !string.IsNullOrEmpty(tb.Ten))
//            {
//                var existing = tbKhachhang.AsEnumerable()
//                    .Where(m => m.Field<string>("Ten").ToLower() == tb.Ten.ToLower())
//                    .Select(r => r.Field<string>("SoHieu"))
//                    .ToList();

//                if (existing == null || existing.Count == 0)
//                {
//                    string baseCode = GenerateAbbreviation(
//                        Helpers.ConvertVniToUnicode(tb.Ten), existing).ToUpper();
//                    string finalSH = baseCode;
//                    int suffix = 1;
//                    while (tbKhachhang.AsEnumerable().Any(r => r.Field<string>("SoHieu") == finalSH))
//                        finalSH = $"{baseCode}_{suffix++}";
//                    tb.Mst = finalSH;
//                }
//                else
//                {
//                    tb.Mst = existing.FirstOrDefault();
//                }
//            }

//            // === Kiểm tra trùng bằng lookup (HashSet O(1)) ===
//            var key = NormalizeTbImportKey(tb.Mst, tb.SHDon, tb.NLap, type);
//            if (lookupTbImport.Contains(key)) return null;

//            // Danh sách tạm (in-memory) tránh trùng trong batch
//            var currentList = type == 1 ? lstdsVao : lstdsRa;
//            if (currentList.Any(m => m.SHDon == tb.SHDon
//                && m.NLap.Date == tb.NLap.Date
//                && m.Mst == tb.Mst))
//                return null;

//            var importList = type == 1 ? lstImportVao : lstImportRa;
//            if (importList != null && importList.Any(m => m.SHDon == tb.SHDon
//                && m.NLap.Date == tb.NLap.Date))
//                return null;

//            // Check CT
//            if (KiemtrahoadonCT(tb.SHDon, tb.KHHDon, tb.NLap, tb.Mst, type))
//                return null;

//            // === Tạo khách hàng nếu chưa có ===
//            if (tb.Mst != "KL" && !CheckExistKH(tb.Mst))
//            {
//                XmlNode doiTacNode = type == 1 ? nBan : nMua;
//                string dChi = doiTacNode?.SelectSingleNode("DChi")?.InnerText ?? "";
//                string sdt = doiTacNode?.SelectSingleNode("SDThoai")?.InnerText ?? "";

//                if (!string.IsNullOrEmpty(tb.Mst) && !string.IsNullOrEmpty(tb.Ten))
//                    InitCustomer(type == 1 ? 2 : 3, tb.Mst, tb.Ten, dChi, tb.Mst, "", sdt);
//                else
//                {
//                    var kl = tbKhachhang.AsEnumerable()
//                        .FirstOrDefault(m => m.Field<string>("SoHieu") == "KL");
//                    if (kl != null)
//                    {
//                        tb.Ten = kl.Field<string>("Ten");
//                        tb.Mst = "KL";
//                    }
//                }
//            }

//            // === Tài khoản ưu tiên ===
//            string kw = type == 1 ? "Ưu tiên vào" : "Ưu tiên ra";
//            var authRow = tbDinhDanhtaikhoan.AsEnumerable()
//                .FirstOrDefault(r => r.Field<string>("KeyValue")?.Contains(kw) == true);
//            if (authRow != null)
//            {
//                tb.TKNo = authRow["TKNo"]?.ToString();
//                tb.TKCo = authRow["TKCo"]?.ToString();
//                tb.TkThue = authRow["TKThue"]?.ToString();
//            }

//            tb.Status = 0;
//            tb.Ngaytao = DateTime.Now.ToShortDateString();

//            // === Tiền ===
//            tb.TongTien = SafeParse(ttToan.SelectSingleNode("TgTTTBSo")?.InnerText);
//            tb.TgTCThue = SafeParse(ttToan.SelectSingleNode("TgTCThue")?.InnerText);
//            tb.TgTThue = SafeParse(ttToan.SelectSingleNode("TgTThue")?.InnerText);

//            tb.Vat = 0;
//            tb.Vat2 = "0";
//            tb.Vat3 = "0";

//            var TTCKTMai = ttToan.SelectSingleNode("TTCKTMai");
//            string ttcktmaiText = TTCKTMai?.InnerText;

//            // === Thuế suất ===
//            var thueNodes = ttToan.SelectNodes("THTTLTSuat//LTSuat");
//            if (thueNodes != null)
//            {
//                for (int i = 0; i < thueNodes.Count; i++)
//                {
//                    XmlNode n = thueNodes[i];
//                    string tsStr = n.SelectSingleNode("TSuat")?.InnerText ?? "";
//                    double ttien = SafeParse(n.SelectSingleNode("ThTien")?.InnerText);
//                    double tthue = Math.Round(SafeParse(n.SelectSingleNode("TThue")?.InnerText));
//                    double vVal = (tsStr == "KCT" || tsStr == "KKKNT")
//                        ? 0
//                        : SafeParse(tsStr.Replace("%", ""));

//                    if ((tsStr == "KCT" && ttien == 0)
//                        || (tsStr == "KKKNT" && ttien == 0)
//                        || (tsStr == "0%" && ttien == 0)
//                        || ttien == 0)
//                        continue;

//                    if (tb.TgTCThue1 == 0)
//                    {
//                        tb.TgTCThue1 = ttien;
//                        tb.TVat = tthue;
//                        tb.Vat = vVal;
//                    }
//                    else if (tb.TgTCThue2 == 0)
//                    {
//                        tb.TgTCThue2 = ttien;
//                        tb.TVat2 = tthue;
//                        tb.Vat2 = vVal.ToString();
//                    }
//                    else if (tb.TgTCThue3 == 0)
//                    {
//                        tb.TgTCThue3 = ttien;
//                        tb.TVat3 = tthue;
//                        tb.Vat3 = vVal.ToString();
//                    }
//                }
//            }

//            // === Chi tiết hàng hóa ===
//            var hhdNodes = root.SelectNodes("//HHDVu");
//            double finalTotal = 0;

//            if (hhdNodes != null)
//            {
//                foreach (XmlNode node in hhdNodes)
//                {
//                    string tenGoc = node.SelectSingleNode("THHDVu")?.InnerText ?? "";
//                    try
//                    {
//                        if (Loaiborow(tenGoc)) continue;

//                        int tchat = int.Parse(node.SelectSingleNode("TChat")?.InnerText ?? "0");
//                        if (tenGoc.Contains("Chiết khấu") && tchat != 3) tchat = 3;
//                        bool daGiam = tenGoc.Contains("Đã giảm");
//                        if (tchat == 4 && !daGiam) continue;

//                        string dvtempty = (serverMode == "2" || Isrunning) ? "..." : "";
//                        string dvtRaw = node.SelectSingleNode("DVTinh")?.InnerText ?? dvtempty;
//                        dvtRaw = Helpers.NormalizeVietnameseString(dvtRaw);
//                        dvtRaw = CapitalizeFirstLetter(Helpers.ConvertUnicodeToVni(dvtRaw));

//                        var detail = new TbImportDetail
//                        {
//                            Tchat = tchat,
//                            Ten = tenGoc,
//                            TKNo = tb.TKNo,
//                            TKCo = tb.TKCo,
//                            DVT = dvtRaw,
//                            Soluong = SafeParse(node.SelectSingleNode("SLuong")?.InnerText),
//                            Dongia = SafeParse(node.SelectSingleNode("DGia")?.InnerText),
//                            TTien = SafeParse(node.SelectSingleNode("ThTien")?.InnerText),
//                            SoPSGoc = SafeParse(node.SelectSingleNode("ThTien")?.InnerText),
//                            Vat = SafeParse(node.SelectSingleNode("TSuat")?.InnerText?.Replace("%", ""))
//                        };

//                        // Chiết khấu
//                        double chietkhau = 0;
//                        double.TryParse(node.SelectSingleNode("STCKhau")?.InnerText, out chietkhau);
//                        if (chietkhau > 0 && detail.Soluong == 0)
//                            detail.TKCo = "711";

//                        if (string.IsNullOrEmpty(detail.DVT))
//                        {
//                            if (serverMode == "2" || Isrunning) detail.DVT = "...";
//                            var findvt = lstvt.FirstOrDefault(m =>
//                                m.TenVattu != null
//                                && m.TenVattu.ToLower() == detail.Ten.ToLower());
//                            if (findvt != null)
//                                detail.DVT = Helpers.ConvertUnicodeToVni(findvt.DonVi);
//                        }

//                        finalTotal += detail.TTien;

//                        if (daGiam)
//                        {
//                            var m = Regex.Match(tenGoc, @"\d{1,3}(?:\.\d{3})*(?:,\d+)?");
//                            if (m.Success)
//                                detail.TTien = double.Parse(m.Value.Replace(".", ""));
//                        }

//                        // === Xử lý mã vật tư (cache nội bộ per-hóa-đơn) ===
//                        cacheMatHangTrongHoaDon = cacheMatHangTrongHoaDon
//                            ?? new Dictionary<string, TbImportDetail>(StringComparer.OrdinalIgnoreCase);

//                        string keyCache = NormalizeVietnameseString(detail.Ten);
//                        if (cacheMatHangTrongHoaDon.TryGetValue(keyCache, out var cached))
//                        {
//                            detail.SoHieu = cached.SoHieu;
//                            detail.Percent = cached.Percent;
//                        }
//                        else
//                        {
//                            Xulysohieuvattu(detail); // dùng 3 tầng cache
//                            cacheMatHangTrongHoaDon[keyCache] = new TbImportDetail
//                            {
//                                Ten = detail.Ten,
//                                SoHieu = detail.SoHieu,
//                                Percent = detail.Percent,
//                                DVT = detail.DVT
//                            };
//                        }

//                        if (type == 1 && (tchat == 3 || daGiam))
//                            detail.TKCo = "711";

//                        detail.Ten = Helpers.ConvertUnicodeToVni(detail.Ten);
//                        detail.Percent = Math.Round(detail.Percent);

//                        if (detail.Dongia != 0 || detail.TTien != 0
//                            || !string.IsNullOrEmpty(detail.DVT) || detail.Soluong != 0)
//                        {
//                            tb.tbImportDetails.Add(detail);
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.WriteLine($"Lỗi parse dòng '{tenGoc}': {ex.Message}");
//                    }
//                }
//            }

//            // === TTCKTMai — chiết khấu toàn hóa đơn ===
//            if (!string.IsNullOrEmpty(ttcktmaiText)
//                && double.TryParse(ttcktmaiText, out double ttckValue)
//                && ttckValue > 0)
//            {
//                if (type == 1)
//                {
//                    if (!tb.tbImportDetails.Any(m => m.TKCo == "711"))
//                    {
//                        double sumNon711 = tb.tbImportDetails
//                            .Where(m => m.TKCo != "711")
//                            .Sum(m => m.TTien);

//                        if (tb.TgTCThue != sumNon711 && tb.TongTien != sumNon711)
//                        {
//                            tb.tbImportDetails.Add(new TbImportDetail
//                            {
//                                Tchat = 1,
//                                Ten = "Chiết khấu",
//                                TKNo = tb.TKNo,
//                                TKCo = "711",
//                                DVT = "xxx",
//                                Soluong = 0,
//                                Dongia = 0,
//                                TTien = ttckValue,
//                                SoPSGoc = 0,
//                                Vat = 0
//                            });
//                        }
//                    }
//                }
//            }

//            // === Làm tròn + phân bổ sai số ===
//            foreach (var lt in tb.tbImportDetails)
//                lt.TTien = Math.Round(lt.TTien);

//            if (tb.tbImportDetails.Count > 0)
//            {
//                double sodu = Math.Round(finalTotal) - tb.tbImportDetails.Sum(m => m.TTien);
//                if (sodu > 0 && sodu <= 1)
//                    tb.tbImportDetails.Last().TTien += sodu;

//                if (tb.TgTCThue != finalTotal)
//                {
//                    sodu = tb.TgTCThue - finalTotal;
//                    if (sodu > 0 && sodu <= 1)
//                    {
//                        tb.tbImportDetails.Last().TTien += sodu;
//                        if (tb.TgTCThue1 + sodu == tb.TgTCThue)
//                            tb.TgTCThue1 += sodu;
//                    }
//                }
//            }

//            // === Nội dung mặc định ===
//            if (string.IsNullOrEmpty(tb.Noidung) && tb.tbImportDetails.Count > 0)
//                tb.Noidung = tb.tbImportDetails[0].Ten;

//            // === Thông tin thay thế ===
//            string shdclQuan = ttChung.SelectSingleNode("//SHDCLQuan")?.InnerText;
//            if (!string.IsNullOrEmpty(shdclQuan))
//            {
//                try
//                {
//                    DateTime nlh = DateTime.Parse(ttChung.SelectSingleNode("//NLHDCLQuan")?.InnerText);
//                    string khh = ttChung.SelectSingleNode("//KHHDCLQuan")?.InnerText;
//                    tb.Noidung = Helpers.ConvertUnicodeToVni(
//                        $"Thay thế cho ký hiệu hóa đơn {khh}, số hóa đơn {shdclQuan}, ngày lập {nlh.ToShortDateString()}");
//                }
//                catch { }
//            }

//            // === Gộp 711/5211 (1 pass) ===
//            if (type == 1) Merge711Inplace(tb);
//            else Merge5211Inplace(tb);

//            return tb;
//        }

//        #endregion

//        #region ================== BLOCK 9: MERGE 711 / 5211 ==================

//        private void Merge711Inplace(TbImport inv)
//        {
//            if (inv?.tbImportDetails == null || inv.tbImportDetails.Count == 0) return;

//            double total711 = 0;
//            TbImportDetail first711 = null;
//            for (int i = inv.tbImportDetails.Count - 1; i >= 0; i--)
//            {
//                var d = inv.tbImportDetails[i];
//                if (d.TKCo == "711")
//                {
//                    total711 += d.TTien;
//                    if (first711 == null) first711 = d;
//                    else inv.tbImportDetails.RemoveAt(i);
//                }
//            }
//            if (first711 == null) return;

//            first711.TTien = total711;

//            // Phân bổ giảm giá vào các dòng còn lại
//            double totalBase = 0;
//            for (int i = 0; i < inv.tbImportDetails.Count; i++)
//            {
//                var d = inv.tbImportDetails[i];
//                if (d == first711) continue;
//                if (string.IsNullOrEmpty(d.DVT) || d.Dongia == 0) continue;
//                totalBase += d.TTien;
//            }
//            if (totalBase <= 0) return;

//            double absTotal711 = Math.Abs(total711);
//            double allocated = 0;
//            var remainList = new List<TbImportDetail>();
//            for (int i = 0; i < inv.tbImportDetails.Count; i++)
//            {
//                var d = inv.tbImportDetails[i];
//                if (d == first711 || string.IsNullOrEmpty(d.DVT) || d.Dongia == 0) continue;
//                remainList.Add(d);
//            }

//            for (int i = 0; i < remainList.Count; i++)
//            {
//                var d = remainList[i];
//                if (i < remainList.Count - 1)
//                {
//                    double cut = Math.Round(absTotal711 * d.TTien / totalBase);
//                    d.TTien = Math.Round(d.TTien - cut);
//                    allocated += cut;
//                }
//                else
//                {
//                    d.TTien = Math.Round(d.TTien - (absTotal711 * d.TTien / totalBase));
//                    double reTotal = Math.Round(inv.TgTCThue - remainList.Sum(x => x.TTien));
//                    if (reTotal > 0 || reTotal == -1) d.TTien += reTotal;
//                }
//            }
//        }

//        private static readonly List<string> _accountCodes = new List<string> { "5113", "5112", "5111" };

//        private void Merge5211Inplace(TbImport inv)
//        {
//            if (inv?.tbImportDetails == null || inv.tbImportDetails.Count == 0) return;

//            bool col711ra = tbLicense != null
//                && tbLicense.Rows.Count > 0
//                && tbLicense.Rows[0].Field<string>("col711ra") == "1";

//            if (col711ra)
//            {
//                var kiemtra5113 = inv.tbImportDetails.FirstOrDefault(m => m.TKCo != null && m.TKCo.Contains("5113"));
//                if (kiemtra5113 != null)
//                {
//                    double sum5211 = 0;
//                    for (int i = inv.tbImportDetails.Count - 1; i >= 0; i--)
//                    {
//                        var d = inv.tbImportDetails[i];
//                        if (d.Tchat == 3)
//                        {
//                            sum5211 += d.TTien;
//                            inv.tbImportDetails.RemoveAt(i);
//                        }
//                    }
//                    if (sum5211 != 0)
//                    {
//                        kiemtra5113.TTien -= sum5211;
//                        return;
//                    }
//                }
//            }

//            foreach (var code in _accountCodes)
//            {
//                if (inv.tbImportDetails.Any(m => m.TKCo != null && m.TKCo.Contains(code)))
//                {
//                    TbImportDetail first5211 = null;
//                    double sumRemain = 0;
//                    for (int i = inv.tbImportDetails.Count - 1; i >= 0; i--)
//                    {
//                        var d = inv.tbImportDetails[i];
//                        if (d.Tchat != 3) continue;
//                        if (first5211 == null) first5211 = d;
//                        else { sumRemain += d.TTien; inv.tbImportDetails.RemoveAt(i); }
//                    }
//                    if (first5211 != null)
//                    {
//                        first5211.TTien += sumRemain;
//                        var refCode = inv.tbImportDetails.FirstOrDefault(m =>
//                            m.TKCo != null && m.TKCo.Contains(code));
//                        first5211.TKNo = refCode?.TKCo;
//                        first5211.TKCo = "";
//                    }
//                    return;
//                }
//            }
//        }

//        #endregion

//        #region ================== BLOCK 10: PARSE XML BATCH PIPELINE ==================

//        private async Task ProcessXmlFilesBatch(List<string> files, int type)
//        {
//            if (files == null || files.Count == 0) return;

//            const int BATCH = 50;
//            var buffer = new List<TbImport>(BATCH);
//            int totalDone = 0;

//            foreach (var file in files)
//            {
//                var tb = await DocfileXmlOne(file, 1);
//                if (tb != null) buffer.Add(tb);
//                totalDone++;

//                if (buffer.Count >= BATCH)
//                {
//                    await SaveAllInvoicesBulk(buffer, type);
//                    if (type == 1) lstdsVao.AddRange(buffer);
//                    else lstdsRa.AddRange(buffer);
//                    buffer.Clear();

//                    SetCaption($"Đã xử lý {totalDone}/{files.Count} file...");
//                }
//            }

//            if (buffer.Count > 0)
//            {
//                await SaveAllInvoicesBulk(buffer, type);
//                if (type == 1) lstdsVao.AddRange(buffer);
//                else lstdsRa.AddRange(buffer);
//                buffer.Clear();
//            }

//            SetCaption($"Hoàn tất xử lý {totalDone} file.");
//        }

//        #endregion

//        #region ================== BLOCK 11: TẢI HÓA ĐƠN TỪ CQT ==================

//        public async Task sv_Taihoadon()
//        {
//            Create2026Structure(savedPath);
//            sv_Kiemtrangayhethan();

//            string tenLoaihd = "";
//            int typeHd = 0;
//            if (chkDauvao.Checked)
//            {
//                progressPanel1.Visible = true;
//                tenLoaihd = "đầu vào";
//                typeHd = 1;
//            }
//            else
//            {
//                progressPanel2.Visible = true;
//                tenLoaihd = "đầu ra";
//                typeHd = 2;
//            }

//            SetCaption($"Đang tiến hành tải hoá đơn {tenLoaihd} tháng {dtTungay.DateTime.Month} từ cơ quan thuế...");
//            Application.DoEvents();

//            // Đăng nhập lấy token
//            await sv_Gettoken();

//            if (string.IsNullOrEmpty(tokken))
//            {
//                ShowMessageSafe("Không lấy được token, vui lòng thử lại.", "Lỗi",
//                    System.Windows.Forms.MessageBoxButtons.OK,
//                    System.Windows.Forms.MessageBoxIcon.Warning);
//                return;
//            }

//            // Tải excel + đọc
//            if (chkDauvao.Checked)
//            {
//                SetCaption("Đang tải hoá đơn điện tử.xlsx");
//                await sv_Xulyexelvao(tokken, 1);
//                SetCaption("Đang tải hoá đơn không nhận mã.xlsx");
//                await sv_Xulyexelvao(tokken, 2);
//                SetCaption("Đang tải hoá đơn máy tính tiền.xlsx");
//                await sv_Xulyexelvao(tokken, 3);
//                SetCaption("Đang đọc dữ liệu Excel...");
//                await DocfileExcelVaoAsyncFast();
//            }
//            else
//            {
//                SetCaption("Đang tải hoá đơn điện tử.xlsx");
//                await sv_Xulyexelra(tokken, 1);
//                SetCaption("Đang tải hoá đơn máy tính tiền.xlsx");
//                await sv_Xulyexelra(tokken, 2);
//                await DocfileExcelRaAsyncFast();
//            }
//        }

//        #endregion

//        #region ================== BLOCK 12: XULYEXCELVAO / XULYEXELRA ==================

//        public async Task sv_Xulyexelvao(string token, int _type)
//        {
//            string tenfileExcel = _type == 1 ? "hoá đơn nhận mã"
//                : _type == 2 ? "hoá đơn không nhận mã"
//                : "hoá đơn máy tính tiền";

//            DateTime dtFrom = new DateTime(dtTungay.DateTime.Year, dtTungay.DateTime.Month, 1);
//            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

//            string fDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
//            string fDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

//            string url, filename, action;
//            switch (_type)
//            {
//                case 1:
//                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={fDate1};tdlap=le={fDate2};ttxly==5&type=purchase";
//                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
//                    action = "Xuất excel (hóa đơn mua vào)";
//                    break;
//                case 2:
//                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={fDate1};tdlap=le={fDate2};ttxly==6&type=purchase";
//                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
//                    action = "Xuất excel (hóa đơn mua vào)";
//                    break;
//                case 3:
//                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={fDate1};tdlap=le={fDate2};ttxly==8&type=purchase";
//                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
//                    action = "Xuất excel (hóa đơn máy tính tiền mua vào)";
//                    break;
//                default: return;
//            }

//            string pathYear = $"HD{dtTungay.DateTime.Year}";
//            string dir = Path.Combine(savedPath, pathYear, "HDVao", dtTungay.DateTime.Month.ToString());
//            string filePath = Path.Combine(dir, filename);

//            try { Directory.CreateDirectory(dir); }
//            catch (Exception ex) { Debug.WriteLine("Không tạo được thư mục: " + ex); return; }

//            if (File.Exists(filePath)
//                && (DateTime.Now - File.GetLastWriteTime(filePath)).TotalMinutes < 30)
//                return;

//            await DownloadExcelWithRetry(url, filePath, filename, action, token,
//                vao: true, tenfileExcel: tenfileExcel);
//        }

//        public async Task sv_Xulyexelra(string token, int _type)
//        {
//            string tenfileExcel = _type == 1 ? "Hoadondientu" : "HDDienTuMayTinhTien";

//            if (mstcongty == "8046549703") mstcongty = "048172000197";

//            DateTime dtFrom = new DateTime(dtTungay.DateTime.Year, dtTungay.DateTime.Month, 1);
//            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);
//            string fDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
//            string fDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

//            string url, filename, action;
//            switch (_type)
//            {
//                case 1:
//                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={fDate1};tdlap=le={fDate2}";
//                    filename = $"{mstcongty}_Hoadondientu.xlsx";
//                    action = "Xuất excel (hóa đơn bán ra)";
//                    break;
//                case 2:
//                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={fDate1};tdlap=le={fDate2}";
//                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
//                    action = "Xuất excel (hóa đơn máy tính tiền bán ra)";
//                    break;
//                default: return;
//            }

//            string pathYear = $"HD{dtTungay.DateTime.Year}";
//            string dir = Path.Combine(savedPath, pathYear, "HDRa", dtTungay.DateTime.Month.ToString());
//            string filePath = Path.Combine(dir, filename);

//            try { Directory.CreateDirectory(dir); }
//            catch (Exception ex) { Debug.WriteLine("Không tạo được thư mục: " + ex); return; }

//            if (File.Exists(filePath)
//                && (DateTime.Now - File.GetLastWriteTime(filePath)).TotalMinutes < 30)
//                return;

//            await DownloadExcelWithRetry(url, filePath, filename, action, token,
//                vao: false, tenfileExcel: tenfileExcel);
//        }

//        private async Task DownloadExcelWithRetry(string url, string filePath, string filename,
//            string action, string token, bool vao, string tenfileExcel)
//        {
//            const int maxRetries = 3;
//            const int retryDelayMs = 2500;

//            using (var client = new HttpClient())
//            {
//                client.Timeout = TimeSpan.FromSeconds(90);
//                client.DefaultRequestHeaders.Clear();
//                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
//                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
//                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
//                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
//                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
//                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");

//                for (int attempt = 1; attempt <= maxRetries; attempt++)
//                {
//                    try
//                    {
//                        SetCaption($"Đang tải {tenfileExcel} (lần {attempt}/{maxRetries})...");

//                        using (var req = new HttpRequestMessage(HttpMethod.Get, url))
//                        {
//                            req.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
//                            req.Headers.TryAddWithoutValidation("End-Point", "/tra-cuu/tra-cuu-hoa-don");
//                            if (!string.IsNullOrEmpty(action))
//                                req.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

//                            var response = await client.SendAsync(req);

//                            if (response.StatusCode == HttpStatusCode.Forbidden)
//                            {
//                                string errBody = await response.Content.ReadAsStringAsync();
//                                bool isHtml = errBody.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
//                                Debug.WriteLine($"403 {(isHtml ? "(WAF)" : "")} lần {attempt}");

//                                if (attempt < maxRetries) { await Task.Delay(retryDelayMs * attempt); continue; }
//                                else { ToastMeaasge("● Lỗi", $"Bị 403 khi tải {tenfileExcel}"); return; }
//                            }

//                            if (response.StatusCode == HttpStatusCode.Unauthorized)
//                            {
//                                ToastMeaasge("● Lỗi", "Token hết hạn, vui lòng đăng nhập lại.");
//                                return;
//                            }

//                            response.EnsureSuccessStatusCode();
//                            byte[] bytes = await response.Content.ReadAsByteArrayAsync();

//                            if (bytes.Length < 2048)
//                                throw new Exception("File quá nhỏ");

//                            if (!(bytes.Length > 4 && bytes[0] == 0x50 && bytes[1] == 0x4B))
//                                throw new Exception("Không phải file Excel");

//                            File.WriteAllBytes(filePath, bytes);
//                            SetCaption($"Đã tải xong {tenfileExcel}");
//                            return;
//                        }
//                    }
//                    catch (TaskCanceledException)
//                    {
//                        Debug.WriteLine($"Timeout {tenfileExcel} lần {attempt}");
//                        if (attempt == maxRetries)
//                        {
//                            ToastMeaasge("● Lỗi", $"Timeout tải {tenfileExcel}");
//                            return;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.WriteLine($"Lỗi {tenfileExcel} lần {attempt}: {ex.Message}");
//                        if (attempt == maxRetries)
//                        {
//                            ToastMeaasge("● Lỗi", $"Không tải được {tenfileExcel}");
//                            return;
//                        }
//                    }

//                    if (attempt < maxRetries) await Task.Delay(retryDelayMs * attempt);
//                }
//            }
//        }

//        #endregion

//        #region ================== BLOCK 13: DOC EXCEL VAO / RA - FAST ==================

//        public async Task DocfileExcelVaoAsyncFast()
//        {
//            taihoddon = true;
//            totalInvoices = 0;
//            currentProgress = 0;
//            GDTClient.UpdateToken(tokken);

//            string pathYear = $"HD{dtTungay.DateTime.Year}";
//            string dir = Path.Combine(savedPath, pathYear, "HDVao", dtTungay.DateTime.Month.ToString());

//            var excelFiles = Directory.EnumerateFiles(dir, "*.xlsx", SearchOption.AllDirectories)
//                .Where(m => m.Contains(mstcongty)).ToList();

//            DateTime tuNgay = dtTungay.DateTime;
//            DateTime denNgay = dtDenngay.DateTime;

//            // Đếm tổng hóa đơn cần xử lý
//            foreach (var excelFile in excelFiles)
//            {
//                using (var wb = new ClosedXML.Excel.XLWorkbook(excelFile))
//                {
//                    var ws = wb.Worksheet(1);
//                    foreach (var row in ws.RowsUsed().Skip(3))
//                    {
//                        string nLapStr = row.Cell(5).GetString();
//                        string khhd = row.Cell(3).GetString();
//                        string sohd = Helpers.RemoveLeadingZeros(row.Cell(4).GetString());
//                        string mstnb = row.Cell(6).GetString();

//                        if (!DateTime.TryParse(nLapStr, out DateTime ngay)) continue;
//                        DateTime d = ngay.Date;

//                        if (ngay < tuNgay || ngay > denNgay) continue;
//                        if (lookupHoaDonCT.Contains((mstnb, sohd, khhd, d, 1))) continue;
//                        if (lookupTbImportCQT.Contains((mstnb, sohd, d, 1))) continue;

//                        totalInvoices++;
//                    }
//                }
//            }

//            // Đếm file XML đã có
//            var allXml = Directory.GetFiles(dir, "*.xml", SearchOption.TopDirectoryOnly)
//                .Where(file =>
//                {
//                    string fn = Path.GetFileNameWithoutExtension(file);
//                    if (fn.Length < 8) return false;
//                    if (!DateTime.TryParseExact(fn.Substring(0, 8), "yyyyMMdd",
//                            System.Globalization.CultureInfo.InvariantCulture,
//                            System.Globalization.DateTimeStyles.None, out DateTime d)) return false;
//                    return d >= tuNgay.Date && d <= denNgay.Date;
//                }).ToList();

//            int hdtaithucsu = allXml.Count;
//            SetCaption($"Đang đọc file thứ {currentProgress} / {totalInvoices}");
//            tongsohodadon = excelFiles.Count;

//            if (totalInvoices == currentProgress || totalInvoices == 0)
//            {
//                modeClick = 2;
//                await Xulychonthang();
//                return;
//            }

//            // Xử lý file XML đã có trước
//            var parsedBatch = new List<TbImport>();
//            foreach (var xml in allXml)
//            {
//                var tb = await DocfileXmlOne(xml, 1);
//                if (tb != null) parsedBatch.Add(tb);

//                if (parsedBatch.Count >= 50)
//                {
//                    await SaveAllInvoicesBulk(parsedBatch, 1);
//                    lstdsVao.AddRange(parsedBatch);
//                    parsedBatch.Clear();
//                }
//            }
//            if (parsedBatch.Count > 0)
//            {
//                await SaveAllInvoicesBulk(parsedBatch, 1);
//                lstdsVao.AddRange(parsedBatch);
//                parsedBatch.Clear();
//            }

//            // Xử lý file Excel — tải XML còn thiếu
//            int invoiceType = 0;
//            foreach (var excelFile in excelFiles)
//            {
//                invoiceType++;
//                using (var wb = new ClosedXML.Excel.XLWorkbook(excelFile))
//                {
//                    var ws = wb.Worksheet(1);
//                    foreach (var row in ws.RowsUsed().Skip(3))
//                    {
//                        string khhd = row.Cell(2).GetString();
//                        string kihieu = row.Cell(3).GetString();
//                        string sohd = Helpers.RemoveLeadingZeros(row.Cell(4).GetString());
//                        string nLapStr = row.Cell(5).GetString();
//                        string mstnb = row.Cell(6).GetString();

//                        if (!DateTime.TryParse(nLapStr, out DateTime ngay)) continue;
//                        DateTime d = ngay.Date;

//                        if (ngay < tuNgay || ngay > denNgay) continue;
//                        if (lookupHoaDonCT.Contains((mstnb, sohd, kihieu, d, 1))) continue;
//                        if (lookupTbImportCQT.Contains((mstnb, sohd, d, 1))) continue;

//                        string fname = $"{d:yyyyMMdd}_{mstnb}_{sohd}_{kihieu}.zip";
//                        string fcheck = $"{d:yyyyMMdd}_{mstnb}_{sohd}_{kihieu}.xml";
//                        string path = Path.Combine(savedPath, pathYear, "HDVao", dtTungay.DateTime.Month.ToString(), fname);
//                        string pathcheck = Path.Combine(savedPath, pathYear, "HDVao", dtTungay.DateTime.Month.ToString(), fcheck);
//                        string folderPath = Path.Combine(savedPath, pathYear, "HDVao", dtTungay.DateTime.Month.ToString());
//                        string knmPath = Path.Combine(folderPath, $"{mstnb}_{sohd}_{kihieu}_KNM.xml");

//                        lookupTbImportCQT.Add(NormalizeTbImportKey(mstnb, sohd, d, 1));

//                        string url = invoiceType == 3
//                            ? $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={kihieu}&shdon={sohd}&khmshdon={khhd}"
//                            : $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={kihieu}&shdon={sohd}&khmshdon={khhd}";

//                        if (File.Exists(pathcheck) || File.Exists(knmPath))
//                        {
//                            currentProgress++;
//                            string existing = File.Exists(knmPath) ? knmPath : pathcheck;
//                            var tb = await DocfileXmlOne(existing, currentIndex);
//                            if (tb != null) parsedBatch.Add(tb);
//                            if (parsedBatch.Count >= 20)
//                            {
//                                await SaveAllInvoicesBulk(parsedBatch, 1);
//                                lstdsVao.AddRange(parsedBatch);
//                                parsedBatch.Clear();
//                            }
//                            continue;
//                        }

//                        try
//                        {
//                            await GDTClient.DownloadFileAsync(
//                                url: url,
//                                savePath: path,
//                                token: tokken,
//                                dt: d,
//                                completionCallback: (success, message, prog) =>
//                                {
//                                    if (success)
//                                    {
//                                        _ = Task.Run(async () =>
//                                        {
//                                            try
//                                            {
//                                                string ph = Path.Combine(folderPath, fcheck);
//                                                if (File.Exists(ph))
//                                                {
//                                                    var tb = await DocfileXmlOne(ph, currentIndex);
//                                                    if (tb != null)
//                                                    {
//                                                        parsedBatch.Add(tb);
//                                                        if (parsedBatch.Count >= 20)
//                                                        {
//                                                            await SaveAllInvoicesBulk(parsedBatch, 1);
//                                                            lock (lstdsVao) lstdsVao.AddRange(parsedBatch);
//                                                            parsedBatch.Clear();
//                                                        }
//                                                    }
//                                                }
//                                            }
//                                            catch (Exception exTask)
//                                            {
//                                                LogToFile("DocfileExcelVaoAsyncFast/callback", exTask);
//                                            }
//                                        });
//                                    }
//                                    else
//                                    {
//                                        if (!string.IsNullOrEmpty(message)
//                                            && message.IndexOf("Không tồn tại hồ sơ gốc", StringComparison.OrdinalIgnoreCase) >= 0)
//                                        {
//                                            _ = Task.Run(() => GetKNMXML(mstnb, kihieu, sohd, tokken, d, folderPath, fname));
//                                        }
//                                    }
//                                });
//                        }
//                        catch (Exception ex)
//                        {
//                            LogToFile("DownloadXML", ex, sohd);
//                            currentProgress++;
//                        }
//                    }
//                }
//            }

//            // Flush lần cuối
//            if (parsedBatch.Count > 0)
//            {
//                await SaveAllInvoicesBulk(parsedBatch, 1);
//                lstdsVao.AddRange(parsedBatch);
//            }

//            modeClick = 2;
//            await Xulychonthang();
//        }

//        public async Task DocfileExcelRaAsyncFast()
//        {
//            taihoddon = true;
//            totalInvoices = 0;
//            currentProgress = 0;
//            GDTClient.UpdateToken(tokken);

//            string pathYear = $"HD{dtTungay.DateTime.Year}";
//            string dir = Path.Combine(savedPath, pathYear, "HDRa", dtTungay.DateTime.Month.ToString());
//            string getmst = tbLicense.AsEnumerable().FirstOrDefault()?["MaSoThue"]?.ToString();

//            var excelFiles = Directory.EnumerateFiles(dir, "*.xlsx", SearchOption.AllDirectories)
//                .Where(m => m.Contains(getmst)).ToList();

//            DateTime tuNgay = dtTungay.DateTime;
//            DateTime denNgay = dtDenngay.DateTime;

//            var fallbackChungtu = new HashSet<(string, string, DateTime)>(
//                existingTbChungtu.AsEnumerable()
//                    .Where(m => m["MaLoai"].ToString() == "8")
//                    .Select(m => (
//                        m.Field<string>("SoHieu") ?? "",
//                        m.Field<string>("KyHieu") ?? "",
//                        m.Field<DateTime>("NgayCT").Date)));

//            var fallbackImport = new HashSet<(string, string, DateTime)>(
//                tbimport.AsEnumerable()
//                    .Where(m => m["Type"].ToString() == "2")
//                    .Select(m => (
//                        m.Field<string>("SHDon") ?? "",
//                        m.Field<string>("KHHDon") ?? "",
//                        m.Field<DateTime>("NLap").Date)));

//            foreach (var excelFile in excelFiles)
//            {
//                using (var wb = new ClosedXML.Excel.XLWorkbook(excelFile))
//                {
//                    var ws = wb.Worksheet(1);
//                    foreach (var row in ws.RowsUsed().Skip(3))
//                    {
//                        string nLapStr = row.Cell(5).GetString();
//                        string sohd = Helpers.RemoveLeadingZeros(row.Cell(4).GetString());
//                        string khhd = row.Cell(3).GetString();
//                        string mstnm = row.Cell(8).GetString();

//                        if (!DateTime.TryParse(nLapStr, out DateTime ngay)) continue;
//                        DateTime d = ngay.Date;

//                        bool daTonTai, daTonTaiimport;
//                        if (!string.IsNullOrEmpty(mstnm))
//                        {
//                            daTonTai = lookupHoaDonCT.Contains((mstnm, sohd, khhd, d, 2));
//                            daTonTaiimport = lookupTbImport.Contains((mstnm, sohd, d, 2));
//                        }
//                        else
//                        {
//                            daTonTai = fallbackChungtu.Contains((sohd, khhd, d));
//                            daTonTaiimport = fallbackImport.Contains((sohd, khhd, d));
//                        }

//                        if (ngay < tuNgay || ngay > denNgay || daTonTai || daTonTaiimport) continue;
//                        totalInvoices++;
//                    }
//                }
//            }

//            if (totalInvoices == 0)
//            {
//                modeClick = 2;
//                await Xulychonthang();
//                return;
//            }

//            SetCaption($"Đang đọc file thứ {currentProgress} / {totalInvoices}");
//            tongsohodadon = excelFiles.Count;

//            var parsedBatch = new List<TbImport>();
//            int i = 1;

//            foreach (var excelFile in excelFiles)
//            {
//                using (var wb = new ClosedXML.Excel.XLWorkbook(excelFile))
//                {
//                    var ws = wb.Worksheet(1);
//                    foreach (var row in ws.RowsUsed().Skip(3))
//                    {
//                        string khhd = row.Cell("B").GetString();
//                        string kihieu = row.Cell("C").GetString();
//                        string sohd = Helpers.RemoveLeadingZeros(row.Cell("D").GetString());
//                        string nLapStr = row.Cell("E").GetString();
//                        string mstnm = row.Cell("H").GetString();

//                        if (!DateTime.TryParse(nLapStr, out DateTime ngay)) continue;
//                        DateTime d = ngay.Date;

//                        bool daTonTai, daTonTaiimport;
//                        if (!string.IsNullOrEmpty(mstnm))
//                        {
//                            daTonTai = lookupHoaDonCT.Contains((mstnm, sohd, kihieu, d, 2));
//                            daTonTaiimport = lookupTbImportCQT.Contains((mstnm, sohd, d, 2));
//                        }
//                        else
//                        {
//                            daTonTai = fallbackChungtu.Contains((sohd, kihieu, d));
//                            daTonTaiimport = fallbackImport.Contains((sohd, kihieu, d));
//                        }

//                        if (ngay < tuNgay || ngay > denNgay || daTonTai || daTonTaiimport) continue;

//                        string url = i == 1
//                            ? $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstcongty}&khhdon={kihieu}&shdon={sohd}&khmshdon={khhd}"
//                            : $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstcongty}&khhdon={kihieu}&shdon={sohd}&khmshdon={khhd}";

//                        string fname = $"{d:yyyyMMdd}_{mstcongty}_{sohd}_{kihieu}.zip";
//                        string fcheck = $"{d:yyyyMMdd}{mstcongty}_{sohd}_{kihieu}.html";
//                        string path = Path.Combine(dir, fname);
//                        string pathcheck = Path.Combine(dir, fcheck);
//                        string folderPath = dir;

//                        lookupTbImportCQT.Add(NormalizeTbImportKey(mstcongty, sohd, d, 2));

//                        if (File.Exists(pathcheck))
//                        {
//                            currentProgress++;
//                            var tb = await DocfileXmlOne(pathcheck, currentIndex);
//                            if (tb != null) parsedBatch.Add(tb);
//                            if (parsedBatch.Count >= 20)
//                            {
//                                await SaveAllInvoicesBulk(parsedBatch, 2);
//                                lstdsRa.AddRange(parsedBatch);
//                                parsedBatch.Clear();
//                            }
//                            continue;
//                        }

//                        try
//                        {
//                            await GDTClient.DownloadFileAsync(
//                                url: url,
//                                savePath: path,
//                                token: tokken,
//                                dt: d,
//                                completionCallback: (success, message, prog) =>
//                                {
//                                    if (success)
//                                    {
//                                        _ = Task.Run(async () =>
//                                        {
//                                            try
//                                            {
//                                                string ph = Path.Combine(folderPath, fcheck);
//                                                if (File.Exists(ph))
//                                                {
//                                                    var tb = await DocfileXmlOne(ph, currentIndex);
//                                                    if (tb != null)
//                                                    {
//                                                        parsedBatch.Add(tb);
//                                                        if (parsedBatch.Count >= 20)
//                                                        {
//                                                            await SaveAllInvoicesBulk(parsedBatch, 2);
//                                                            lock (lstdsRa) lstdsRa.AddRange(parsedBatch);
//                                                            parsedBatch.Clear();
//                                                        }
//                                                    }
//                                                }
//                                            }
//                                            catch (Exception exTask)
//                                            {
//                                                LogToFile("DocfileExcelRaAsyncFast/callback", exTask);
//                                            }
//                                        });
//                                    }
//                                });
//                        }
//                        catch (Exception ex)
//                        {
//                            LogToFile("DownloadXMLRa", ex, sohd);
//                            currentProgress++;
//                        }
//                    }
//                }
//                i++;
//            }

//            if (parsedBatch.Count > 0)
//            {
//                await SaveAllInvoicesBulk(parsedBatch, 2);
//                lstdsRa.AddRange(parsedBatch);
//            }

//            modeClick = 2;
//            await Xulychonthang();
//        }

//        #endregion

//        #region ================== BLOCK 14: NCC CACHE HELPER ==================

//        private string GetNCCNameCached(string code)
//        {
//            if (string.IsNullOrEmpty(code)) return "";
//            return _nccCache.GetOrAdd(code, c => GetNCCName(c));
//        }

//        #endregion
//    }
//}
