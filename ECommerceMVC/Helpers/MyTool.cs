using System.Text;

namespace ECommerceMVC.Helpers
{
    public class MyTool
    {
        public static string UploadImage(IFormFile Hinh, string folder)
        {
            try
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", folder, Hinh.FileName); // chưa xử lý trùng tên fileName

                using (FileStream file = new FileStream(fullPath, FileMode.CreateNew))
                {
                    Hinh.CopyTo(file);
                }
                return Hinh.FileName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GenerateRandomKey(int length = 5)
        {
            string pattern = "MySupperKey122604@";
            StringBuilder sb = new StringBuilder();
            Random rd = new Random();

            for(int i = 0;i<length;i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }
            return sb.ToString();
        }    
    }
}
