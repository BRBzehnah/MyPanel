using Data;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Agent
{
    public class PixelBot
    {
        #region Win32 API
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }
        #endregion

        private readonly Color _targetColor = Color.FromArgb(0, 164, 255); // Цвет кнопки "Войти"

        public async Task<Result> IsLoginWindowPresent()
        {
            IntPtr hWnd = FindWindow(null, "Войти в Steam");
            if (hWnd == IntPtr.Zero) return Result.Failure(new Error(ErrorType.AgentPixelBotError, "Окно 'Войти в Steam' не найдено"));

            if (!GetWindowRect(hWnd, out RECT rect)) return Result.Failure(new Error(ErrorType.AgentPixelBotError, "Не удалось получить размеры окна"));

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // Если окно свернуто или некорректно
            if (width <= 0 || height <= 0) return Result.Failure(new Error(ErrorType.AgentPixelBotError, "Окно некорректно"));
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // Вычисляем точку (50% ширины, 73% высоты)
                    int x = rect.Left + (int)(width * 0.5);
                    int y = rect.Top + (int)(height * 0.73);

                    // Захватываем только 1 пиксель для экономии ресурсов
                    g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
                }
                Color pixel = bmp.GetPixel(0, 0);
                if (IsColorMatch(pixel, _targetColor))
                    return Result.Success();
                return Result.Failure(new Error(ErrorType.AgentPixelBotError, "Не удалось верифицировать окно"));
            }
        }

        private bool IsColorMatch(Color c1, Color c2)
        {
            int tolerance = 25;
            return Math.Abs(c1.R - c2.R) < tolerance &&
                   Math.Abs(c1.G - c2.G) < tolerance &&
                   Math.Abs(c1.B - c2.B) < tolerance;
        }

        public async Task<Result> WaitForLoginWindow()
        {
            //Ждем появления окна минуту
            DateTime endTime = DateTime.Now.AddSeconds(60); 
            while (DateTime.Now < endTime)
            {
                if ((await IsLoginWindowPresent()).IsSuccess)
                    return Result.Success();
                await Task.Delay(500);
            }
            return Result.Failure(new Error(ErrorType.TimedOut, "Время ожидания окна вышло"));
        }
    }
}
