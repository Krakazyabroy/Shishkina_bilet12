using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Shishkina_bilet12;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private const double ALUMINUM_PRICE = 15.50;
        private const double PLASTIC_PRICE = 9.90;

        // Тест 1: Ввод больших чисел
        [TestMethod]
        public void TestMethod1()
        {
            var win = new MainWindow();
            bool success = win.TryCalculateCost("1000000", "1000000", ALUMINUM_PRICE, out double total);
            Assert.IsTrue(success);
            Assert.IsTrue(total > 0);
        }


        // Тест 2: Ввод отрицательных чисел
        [TestMethod]
        public void TestMethod2()
        {
            var win = new MainWindow();
            bool success = win.TryCalculateCost("-1", "2", ALUMINUM_PRICE, out _);
            Assert.IsFalse(success);
        }

        // Тест 3: Пустые поля ввода
        [TestMethod]
        public void TestMethod3()
        {
            var win = new MainWindow();
            bool success = win.TryCalculateCost("", "2", ALUMINUM_PRICE, out _);
            Assert.IsFalse(success);
        }
    }
}

