using System;
using System.Collections.Generic;
using Core.Utils;
using NUnit.Framework;

namespace Tests.UnitTests.Utils
{
    [TestFixture]
    public class ListExtensionsTests
    {
        [Test]
        public void test_shuffling_list()
        {
            // Arrange
            var list = new List<int>() { 1, 2, 3, 4, 5 };

            // Act
            list.Shuffle();

            // Assert
            Assert.AreNotEqual(new List<int> { 1, 2, 3, 4, 5 }, list);
        }

        [Test]
        public void test_cloning_list()
        {
            // Arrange
            var list = new List<String>() { "1", "2", "3", "4", "5" };

            // Act
            var clone = list.Clone();

            // Assert
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], clone[i]);
            }
        }

        [Test]
        public void test_replacing_element_with_list_of_elements()
        {
            // Arrange
            var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var newValues = new List<int> { 100, 200 };
            
            // Act
            list.Replace(4, newValues);

            // Assert
            Assert.AreEqual(new List<int>() { 1, 2, 3, 100, 200, 5, 6, 7, 8, 9 }, list);
        }
    }
}