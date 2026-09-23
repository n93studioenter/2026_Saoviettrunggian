using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; 
namespace SaovietTax.DTO
{
    public class ScheduleHelper
    {
        // ========================================
        // HÀM TẠO LỊCH
        // ========================================
        public static void CreateSchedule(string taskName, int hour, int minute = 0)
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeWithArgs = $"\"{exePath}\" -autostart";

                // Xóa task cũ nếu tồn tại
                Process.Start("cmd", $"/c schtasks /delete /tn \"{taskName}\" /f").WaitForExit();

                // Tạo task mới
                string timeStr = $"{hour:D2}:{minute:D2}";
                string cmd = $"schtasks /create /tn \"{taskName}\" /tr \"{exeWithArgs}\" /sc daily /st {timeStr} /f";
                Process.Start("cmd", "/c " + cmd).WaitForExit();

                Console.WriteLine($"✅ Đã tạo lịch '{taskName}' lúc {timeStr} mỗi ngày!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
            }
        }

        // ========================================
        // HÀM XÓA LỊCH
        // ========================================
        public static void DeleteSchedule(string taskName)
        {
            try
            {
                string cmd = $"schtasks /delete /tn \"{taskName}\" /f";
                Process.Start("cmd", "/c " + cmd).WaitForExit();
                Console.WriteLine($"✅ Đã xóa lịch '{taskName}'!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi xóa: {ex.Message}");
            }
        }
    }
}