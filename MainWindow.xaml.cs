using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Shishkina_bilet12
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {  

            private double width = 0;
            private double height = 0;
            private string material = "";
            private double totalCost = 0;
            private double paidAmount = 0;
            private double change = 0;

            public MainWindow()
            {
                InitializeComponent();
                cmbMaterial.SelectedIndex = -1;
            }

        /// <summary>
        /// Выполняет расчёт стоимости жалюзи на основе входных данных.
        /// </summary>
        /// <param name="widthText">Текст ширины</param>
        /// <param name="heightText">Текст высоты</param>
        /// <param name="pricePerSqm">Цена за кв.м</param>
        /// <param name="totalCost">Результат: итоговая стоимость</param>
        /// <returns>true, если расчёт успешен; иначе false</returns>
        public bool TryCalculateCost(string widthText, string heightText, double pricePerSqm, out double totalCost)
        {
            totalCost = 0;

            if (!double.TryParse(widthText, out double width) || width <= 0)
                return false;

            if (!double.TryParse(heightText, out double height) || height <= 0)
                return false;

            totalCost = Math.Round(width * height * pricePerSqm, 2);
            return true;
        }

        /// <summary>
        /// Обработчик кнопки "Рассчитать"
        /// </summary>
        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
            {

            var selectedItem = cmbMaterial.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите материал");
                txtTotal.Text = "";
                return;
            }

            if (!double.TryParse(selectedItem.Tag.ToString(), out double pricePerSqm))
            {
                MessageBox.Show("Ошибка в данных материала");
                txtTotal.Text = "";
                return;
            }

            if (TryCalculateCost(txtWidth.Text, txtHeight.Text, pricePerSqm, out double total))
            {
                txtTotal.Text = $"{total:F2}";
                txtChange.Text = "";
            }
            else
            {
                MessageBox.Show("Введите корректные размеры (положительные числа)");
                txtTotal.Text = "";
            }
        }

            /// <summary>
            /// Расчет сдачи и вывод на форму
            /// </summary>
            private bool CalculateChange()
            {
                if (!double.TryParse(txtPaid.Text, out paidAmount) || paidAmount < 0)
                {
                    MessageBox.Show("Введите корректную сумму оплаты");
                    return false;
                }

                if (totalCost <= 0)
                {
                    MessageBox.Show("Сначала выполните расчет!");
                    return false;
                }

                if (paidAmount < totalCost)
                {
                    MessageBox.Show($"Сумма оплаты не может быть меньше итоговой стоимости");
                    txtChange.Text = "Недостаточно";
                    return false;
                }

                change = Math.Max(0, Math.Round(paidAmount - totalCost, 2));
                txtChange.Text = $"{change:F2} руб.";
                return true;
            }

            /// <summary>
            /// Обработчик кнопки "Оформить квитанцию"
            /// </summary>
            private void BtnReceipt_Click(object sender, RoutedEventArgs e)
            {
                if (totalCost <= 0)
                {
                    MessageBox.Show("Сначала выполните расчет!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtProductName.Text))
                {
                    MessageBox.Show("Введите наименование товара!");
                    return;
                }
                if (!CalculateChange())
                {
                    return; 
                }
                GenerateReceipt();
            }

            /// <summary>
            /// Генерирует PDF чек покупки
            /// </summary>
            private void GenerateReceipt()
            {
                try
                {
                    // Генерация уникального номера чека
                    string receiptNumber = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileName = $"Чек_{receiptNumber}_{DateTime.Now:yyyyMMdd}_{totalCost:F0}.pdf";

                    // Создание PDF документа
                    var document = new Document(new Rectangle(220, 450), 5, 5, 8, 5);

                    using (var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create)))
                    {
                        document.Open();

                        // Установка шрифтов
                        BaseFont baseFont = BaseFont.CreateFont(
                            @"C:\Windows\Fonts\arial.ttf",
                            BaseFont.IDENTITY_H,
                            BaseFont.NOT_EMBEDDED);
                        var titleFont = new Font(baseFont, 10, Font.BOLD);
                        var regularFont = new Font(baseFont, 9, Font.NORMAL);
                        var smallFont = new Font(baseFont, 8, Font.NORMAL);

                        // Шапка документа
                        var companyParagraph = new Paragraph("ООО \"Уютный Дом\"", titleFont)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 5f
                        };
                        document.Add(companyParagraph);

                        document.Add(new Paragraph("Добро пожаловать", regularFont)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 10f
                        });

                        // Реквизиты
                        document.Add(new Paragraph("ККМ 00075411     #3969", smallFont));
                        document.Add(new Paragraph("ИНН 1087746942040", smallFont));
                        document.Add(new Paragraph("ЭКЛЗ 3851495566", smallFont));
                        document.Add(new Paragraph($"Чек №{receiptNumber}", smallFont));
                        document.Add(new Paragraph($"{DateTime.Now:dd.MM.yy HH:mm} СИС.", smallFont));

                        document.Add(new Paragraph(" "));
                        document.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0)));
                        document.Add(new Paragraph(" "));

                        PdfPTable table = new PdfPTable(2);
                        table.WidthPercentage = 100;
                        table.SetWidths(new float[] { 1.2f, 0.8f });

                        table.AddCell(CreateCell("Наименование товара", regularFont));
                        table.AddCell(CreateCell(txtProductName.Text, regularFont));

                        table.AddCell(CreateCell("Размер", regularFont));
                        table.AddCell(CreateCell($"{width:F2} x {height:F2} м", regularFont));

                        table.AddCell(CreateCell("Материал", regularFont));
                        table.AddCell(CreateCell(material, regularFont));

                        table.AddCell(CreateCell("Итог", regularFont));
                        table.AddCell(CreateCell($"{totalCost:F2} руб.", regularFont));

                        table.AddCell(CreateCell("Оплачено", regularFont));
                        table.AddCell(CreateCell($"{paidAmount:F2} руб.", regularFont));

                        table.AddCell(CreateCell("Сдача", regularFont));
                        table.AddCell(CreateCell($"{change:F2} руб.", regularFont));

                        table.AddCell(CreateCell("Сумма итого:", titleFont));
                        table.AddCell(CreateCell($"{totalCost:F2} руб.", titleFont));

                        document.Add(table);
                        document.Add(new Paragraph(" "));

                        document.Add(new Paragraph("************************", regularFont)
                        {
                            Alignment = Element.ALIGN_CENTER
                        });

                        document.Add(new Paragraph($"     {receiptNumber.Substring(8, 6)}# {receiptNumber.Substring(0, 6)}", regularFont)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingBefore = 10f
                        });

                        document.Close();
                    }

                    // Открытие PDF
                    System.Diagnostics.Process.Start(fileName);
                    MessageBox.Show($"Чек сохранен:\n{fileName}", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка генерации чека: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            /// <summary>
            /// Создает ячейку таблицы
            /// </summary>
            private PdfPCell CreateCell(string text, Font font)
            {
                var cell = new PdfPCell(new Phrase(text, font));
                cell.Padding = 5;
                cell.BorderWidth = 0.5f;
                return cell;
            }
        }
    }

